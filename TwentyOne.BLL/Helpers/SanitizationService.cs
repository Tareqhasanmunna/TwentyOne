using Ganss.Xss;

namespace TwentyOne.BLL.Helpers
{
    public class SanitizationService
    {
        private readonly HtmlSanitizer _sanitizer;

        public SanitizationService()
        {
            _sanitizer = new HtmlSanitizer();

            // Only allow safe tags
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("strong");
            _sanitizer.AllowedTags.Add("em");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("p");

            // Remove all attributes
            _sanitizer.AllowedAttributes.Clear();
        }

        public string Sanitize(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return _sanitizer.Sanitize(input);
        }

        public string SanitizePlainText(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Strip all HTML for plain text fields
            return System.Text.RegularExpressions.Regex
                .Replace(input, "<.*?>", string.Empty)
                .Trim();
        }
    }
}