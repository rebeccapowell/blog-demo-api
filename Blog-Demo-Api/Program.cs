using Microsoft.AspNetCore.Mvc;
using Powell.UtrTaxNumberTools;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
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

app.UseHttpsRedirection();

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

app.Run();

record UtrCheckResult(bool IsValid, string UtrNumber);
record UtrGenerateResult(string UtrNumber);