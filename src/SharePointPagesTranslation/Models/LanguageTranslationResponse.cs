using System.Text.Json.Serialization;

namespace SharePointPagesTranslation.Models
{
    public class LanguageTranslationResponse
    {
        [JsonPropertyName("detectedLanguage")]
        public DetectedLanguage DetectedLanguage { get; set; }

        [JsonPropertyName("translations")]
        public Translation[] Translations { get; set; }
    }

    public partial class DetectedLanguage
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }

    public partial class Translation
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }
    }
}