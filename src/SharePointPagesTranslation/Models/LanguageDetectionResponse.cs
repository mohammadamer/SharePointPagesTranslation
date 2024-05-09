using System.Text.Json.Serialization;

namespace SharePointPagesTranslation.Models
{
    public class LanguageDetectionResponse
    {
        [JsonPropertyName("isTranslationSupported")]
        public bool IsTranslationSupported { get; set; }

        [JsonPropertyName("isTransliterationSupported")]
        public bool IsTransliterationSupported { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }
}
