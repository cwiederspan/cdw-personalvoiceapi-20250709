using System.ComponentModel;
using Microsoft.CognitiveServices.Speech;
using Azure.Identity;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var resourceId = builder.Configuration["Settings:SpeechResourceId"];
var region = builder.Configuration["Settings:SpeechRegion"];
var speakerId = builder.Configuration["Settings:SpeakerId"];

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.MapGet("/synthesize", async Task<IResult> (
    [Description("SSML input to be synthesized")] string ssml) =>
{
    IResult result = TypedResults.StatusCode(500);

    var credential = new DefaultAzureCredential(false);
    var accessToken = credential.GetToken(new TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" }));
    var speechToken = $"aad#{resourceId}#{accessToken.Token}";
    var speechConfig = SpeechConfig.FromAuthorizationToken(speechToken, region);

    //var audioConfig = AudioConfig.FromWavFileOutput("output.wav");

    using (var synthesizer = new SpeechSynthesizer(speechConfig, null /*, audioConfig*/))
    {
        var markup = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='http://www.w3.org/2001/mstts' xml:lang='en-US'>" +
        "<voice name='DragonLatestNeural'>" +
        $"<mstts:ttsembedding speakerProfileId='{speakerId}'>" +
        "<lang xml:lang='en-US'>" +
        // "<break time='1s'/>" +
        ssml +
        // "<break time='1s'/>" +
        "</lang>" +
        "</mstts:ttsembedding>" +
        "</voice>" +
        "</speak>";

        var synthResult = await synthesizer.SpeakSsmlAsync(markup);

        if (synthResult.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            result = TypedResults.File(synthResult.AudioData, "audio/wav", $"content_{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}.wav");
        }
    }

    return result;
})
.WithName("Synthesize").WithSummary("Synthesize speech from SSML and return a WAV file").Produces(200, contentType: "audio/wav");
//.Produces<StatusCodeResult>(200, "audio/wav");
// .Produces<StatusCodeResult>(500);

app.Run();