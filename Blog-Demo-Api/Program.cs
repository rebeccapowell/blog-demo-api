using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Powell.UtrTaxNumberTools;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Listen on two ports: 8080 (public via proxy) and 8081 (internal/management)
builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(8080); // public traffic via ingress
    o.ListenAnyIP(8081); // management/health, not exposed by ingress
});

// Services
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Build
var app = builder.Build();

// Behind a proxy (Cloudflare/Traefik/Caddy): respect X-Forwarded-*
// This helps avoid HTTPS redirect loops and ensures correct scheme/origin.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Dev-only API docs
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("UTR Validation & Generation (UK Tax Number Tools) API");
        options.WithTheme(ScalarTheme.Moon);
        options.WithSidebar(true);
    });
}

// If your TLS terminates at the proxy, you can keep this off to avoid loops
// app.UseHttpsRedirection();

// --- Public API (8080)
app.MapGet("/utr-check/{utrNumber}", ([FromRoute] string utrNumber) =>
{
    var validator = new Validator();
    var isValid = validator.Validate(utrNumber);
    return Results.Ok(new UtrCheckResult(isValid, utrNumber));
})
.WithName("CheckUtrNumber");

app.MapGet("/utr-generate", () =>
{
    var generator = new Generator();
    var utrNumber = generator.Generate();
    return Results.Ok(new UtrGenerateResult(utrNumber));
})
.WithName("GenerateUtrNumber");

// --- Health endpoint (8081 only)
// RequireHost("*:8081") ensures /healthz does NOT respond on the public port.
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = async (ctx, rpt) =>
    {
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync($$"""
        {"status":"{{rpt.Status}}"}
        """);
    }
})
.RequireHost($"*:{8081}");

app.Run();

// Records
record UtrCheckResult(bool IsValid, string UtrNumber);
record UtrGenerateResult(string UtrNumber);