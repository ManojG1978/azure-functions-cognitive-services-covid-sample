using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace SpeechRecognition
{
    public static class ProcessSoundFile
    {
        [FunctionName("ProcessSoundFile")]
        public static async Task Run(
            [BlobTrigger("soundfiles/{name}", Connection = "storageConnectionString")]
            Stream inputBlob,
            [Blob("sroutput/{name}.json", FileAccess.Write, Connection = "storageConnectionString")]
            Stream outputBlob,
            string name,
            ILogger log)
        {
            log.LogInformation(
                $"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");

            (string text, bool success) = await RecognizeSpeechAsync(inputBlob, log);

            dynamic jsonResponse = new JObject();
            jsonResponse.Text = success ? text : string.Empty;
            jsonResponse.ErrorMessage = success ? string.Empty : text;

            using (var sw = new StreamWriter(outputBlob))
            {
                sw.Write(jsonResponse.ToString());
            }

            log.LogInformation($"Completed processing of blob\n Name:{name} ");
        }

        private static async Task<Tuple<string, bool>> RecognizeSpeechAsync(Stream input, ILogger log)
        {
            var config = SpeechConfig.FromSubscription(
                Environment.GetEnvironmentVariable("speechApiKey"),
                Environment.GetEnvironmentVariable("speechApiRegion"));

            var response = new Tuple<string, bool>(string.Empty, false);

            using (var audioInput = SpeechUtils.OpenWavFile(new BinaryReader(input)))
            {
                using var recognizer = new SpeechRecognizer(config, audioInput);
                var result = await recognizer.RecognizeOnceAsync();

                switch (result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        return new Tuple<string, bool>(result.Text, true);
                    case ResultReason.NoMatch:
                        return new Tuple<string, bool>("Speech could not be recognized.", false);
                    case ResultReason.Canceled:
                        var cancellation = CancellationDetails.FromResult(result);

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            log.LogError($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            log.LogError($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            log.LogError("CANCELED: Did you update the subscription info?");
                        }

                        return new Tuple<string, bool>($"CANCELED: Reason={cancellation.Reason}", true);
                }
            }
            return response;
        }
    }
}