using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TwentyOne.BLL.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            // Convert to lower case
            string slug = input.ToLowerInvariant();
            // Replace spaces with hyphens
            slug = slug.Replace(" ", "-");
            // Remove all non-alphanumeric characters except hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Remove duplicate hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            return slug;
        }
    }
}
