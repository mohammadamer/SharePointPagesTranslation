using SharePointPagesTranslation.Interfaces;
using SharePointPagesTranslation.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SharePointPagesTranslation.Providers
{
    public class TextTranslationProvider: ITextTranslationProvider
    {
        private readonly string _key = Environment.GetEnvironmentVariable(Constants.AITranslatorKey);
        private readonly string _endpoint = Environment.GetEnvironmentVariable(Constants.AIEndpoint);
        private readonly string _location = Environment.GetEnvironmentVariable(Constants.AIResourcelocation);
        private readonly string _apiVersion = Environment.GetEnvironmentVariable(Constants.AIAPIVersion);
        private readonly HttpClient _httpClient;

        public TextTranslationProvider()
        {
            _httpClient = new HttpClient();
        }

        public async Task<PageStructureModel> TranslateContent(PageStructureModel sourcePageContentStructure, string targetLanguage)
        {
            string route = $"{Constants.AITranslateService}?{_apiVersion}&" + string.Join("&", $"to={targetLanguage}");

            foreach (var section in sourcePageContentStructure.Sections)
            {
                if (section != null)
                {
                    foreach (var column in section.Columns)
                    {
                        if (column != null)
                        {
                            foreach (var control in column.Controls)
                            {
                                if (control != null)
                                {
                                    // Extract and preserve HTML tags
                                    var htmlText = control.Text;
                                    var tagMatches = Regex.Matches(htmlText, @"<[^>]+>|&nbsp;");
                                    var tags = new string[tagMatches.Count];
                                    for (int i = 0; i < tagMatches.Count; i++)
                                    {
                                        tags[i] = tagMatches[i].Value;
                                        htmlText = htmlText.Replace(tags[i], $"[{i}]");
                                    }

                                    object[] body = new object[] { new { Text = htmlText } };
                                    var requestBody = JsonSerializer.Serialize(body);

                                    using (var request = new HttpRequestMessage())
                                    {
                                        request.Method = HttpMethod.Post;
                                        request.RequestUri = new Uri(_endpoint + route);
                                        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                                        request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
                                        request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

                                        HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                                        string result = await response.Content.ReadAsStringAsync();

                                        List<LanguageTranslationResponse> translationResults = JsonSerializer.Deserialize<List<LanguageTranslationResponse>>(result);
                                        foreach (var item in translationResults)
                                        {
                                            foreach (var translation in item.Translations)
                                            {
                                                // Replace text placeholders with original HTML tags
                                                var translatedText = translation.Text;
                                                for (int i = 0; i < tags.Length; i++)
                                                {
                                                    translatedText = translatedText.Replace($"[{i}]", tags[i]);
                                                }

                                                control.Text = translatedText;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sourcePageContentStructure;
        }

        public async Task<string> DetectContent(PageStructureModel sourcePageContentStructure)
        {
            bool isPageContentEnglish = false;
            var pageLanguage = string.Empty;
            string route = $"{Constants.AIDetectService}?{_apiVersion}";

            foreach (var section in sourcePageContentStructure.Sections)
            {
                if (section != null)
                {
                    foreach (var column in section.Columns)
                    {
                        if (column != null)
                        {
                            foreach (var control in column.Controls)
                            {
                                if (control != null)
                                {
                                    object[] body = new object[] { new { Text = control.Text } };
                                    var requestBody = JsonSerializer.Serialize(body);

                                    using (var request = new HttpRequestMessage())
                                    {
                                        request.Method = HttpMethod.Post;
                                        request.RequestUri = new Uri(_endpoint + route);
                                        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                                        request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
                                        request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

                                        HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                                        string result = await response.Content.ReadAsStringAsync();

                                       List<LanguageDetectionResponse> languageDetectionResults = JsonSerializer.Deserialize<List<LanguageDetectionResponse>>(result);

                                        pageLanguage = languageDetectionResults[0].Language;

                                    }
                                }
                            }
                        }
                    }
                }
            }
            return pageLanguage;
        }
    }
}