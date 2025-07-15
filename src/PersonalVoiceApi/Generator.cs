using System.ComponentModel;
using Microsoft.CognitiveServices.Speech;
using Azure.Identity;
using Azure.Core;

public class Generator {

    private readonly SpeechConfig SpeechConfig;

    public Generator(string resourceId, string region) {

        var credential = new DefaultAzureCredential(false);
        var accessToken = credential.GetToken(new TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" }));
        var speechToken = $"aad#{resourceId}#{accessToken.Token}";

        this.SpeechConfig = SpeechConfig.FromAuthorizationToken(speechToken, region);
    }

    public async Task<byte[]?> GenerateContentAsync(string ssml, string speakerId) {

        byte[]? result = null;

        using (var synthesizer = new SpeechSynthesizer(this.SpeechConfig, null)) {

            var markup = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='http://www.w3.org/2001/mstts' xml:lang='en-US'>" +
            "<voice name='DragonLatestNeural'>" +
            $"<mstts:ttsembedding speakerProfileId='{speakerId}'>" +
            "<lang xml:lang='en-US'>" +
            ssml +
            "</lang>" +
            "</mstts:ttsembedding>" +
            "</voice>" +
            "</speak>";

            var synthResult = await synthesizer.SpeakSsmlAsync(markup);

            if (synthResult.Reason == ResultReason.SynthesizingAudioCompleted) {
                Console.WriteLine($"Processed audio data of size {(synthResult.AudioData?.Length ?? 0)} bytes.");
                result = synthResult.AudioData;
            }

            return result;
        }
    }
}