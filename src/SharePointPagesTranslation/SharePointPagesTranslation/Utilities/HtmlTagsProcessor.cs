using System.Text.RegularExpressions;

namespace SharePointPagesTranslation.Utilities
{
    public class HtmlTagsProcessor
    {

        public static string[] ExtractHtmlTags(string htmlText)
        {
            // Extract and preserve HTML tags
            var tagMatches = Regex.Matches(htmlText, @"<[^>]+>|&nbsp;");
            var tags = new string[tagMatches.Count];
            for (int i = 0; i < tagMatches.Count; i++)
            {
                tags[i] = tagMatches[i].Value;
                htmlText = htmlText.Replace(tags[i], $"[T{i}]");
            }
            return tags;
        }
        static string AddHtmlTags(string plainText, string[] tags)
        {
            // Replace text placeholders with original HTML tags
            for (int i = 0; i < tags.Length; i++)
            {
                plainText = plainText.Replace($"[T{i}]", tags[i]);
            }
            return plainText;
        }
    }
}
