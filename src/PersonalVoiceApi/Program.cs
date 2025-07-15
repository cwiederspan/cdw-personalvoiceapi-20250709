using System.ComponentModel;
using Microsoft.CognitiveServices.Speech;
using Azure.Identity;
using Azure.Core;
using Microsoft.AspNetCore.Components;

// Possible null reference argument.
#pragma warning disable CS8604 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var resourceId = builder.Configuration["Settings:SpeechResourceId"];
var region = builder.Configuration["Settings:SpeechRegion"];
var speakerId = builder.Configuration["Settings:SpeakerId"];

builder.Services.AddTransient<Generator>((sp) => new Generator(resourceId, region));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.MapGet("/synthesize", async Task<IResult> (string ssml, Generator generator) => {

    Console.WriteLine($"Received SSML: {ssml}");

    IResult result = TypedResults.StatusCode(500);

    var audioData = await generator.GenerateContentAsync(ssml, speakerId);

    if (audioData != null) {
        result = TypedResults.File(audioData, "audio/wav", $"content_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}.wav");
    }

    return result;
});

app.Run();

 // Possible null reference argument.
#pragma warning restore CS8604