using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections;
using Microsoft.CognitiveServices.Speech;

using Azure.Core;
using Azure.Identity;

// Possible null reference argument.
#pragma warning disable CS8604 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var resourceId = builder.Configuration["Settings:SpeechResourceId"];
var region = builder.Configuration["Settings:SpeechRegion"];
var speakerId = builder.Configuration["Settings:SpeakerId"];
var storageEndpoint = builder.Configuration["AZURE_STORAGE_BLOB_ENDPOINT"];

builder.Services.AddTransient((sp) => new Generator(resourceId, region));
builder.Services.AddTransient((sp) => new FileStore(storageEndpoint, "downloads"));

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

app.MapGet("/synthesize/file", async Task<IResult> (string ssml, Generator generator, FileStore fileStore) => {

    Console.WriteLine($"Received SSML: {ssml}");

    IResult result = TypedResults.StatusCode(500);

    var audioData = await generator.GenerateContentAsync(ssml, speakerId);

    if (audioData != null) {

        string url = await fileStore.SaveFileAsync(audioData);

        var json = new {
            downloadUrl = url,
            fileType = "audio/wav"
        };

        result = TypedResults.Json(json);
    }

    return result;
});

app.Run();

 // Possible null reference argument.
#pragma warning restore CS8604