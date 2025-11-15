using Hotel_Booking_API.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Hotel_Booking_API.Infrastructure.Templates
{
    public static class EmailTemplateLoader
    {
        private static readonly MemoryCache LocalCache = new(new MemoryCacheOptions());
        private static readonly string TemplatesFolder = Path.Combine(
            AppContext.BaseDirectory,
            "Infrastructure",
            "Templates"
        );

        public static string LoadTemplate(string templateName)
        {
            try
            {
                var key = CacheKeys.Templates.Email(templateName);
                if (!LocalCache.TryGetValue(key, out string? template) || template is null)
                {
                    var templatePath = Path.Combine(TemplatesFolder, templateName);
                    if (!File.Exists(templatePath))
                    {
                        Log.Error("Email template not found: {TemplatePath}", templatePath);
                        throw new FileNotFoundException($"Email template not found: {templateName}");
                    }

                    template = File.ReadAllText(templatePath);
                    var options = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                        .SetPriority(CacheItemPriority.Low);
                    LocalCache.Set(key, template, options);
                    Log.Debug("Loaded and cached email template: {TemplateName}", templateName);
                }
                else
                {
                    Log.Debug("Loaded email template from cache: {TemplateName}", templateName);
                }

                return template;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading email template: {TemplateName}", templateName);
                throw;
            }
        }

        public static string FillTemplate(string template, Dictionary<string, string> placeholders)
        {
            try
            {
                var filledTemplate = template;

                foreach (var placeholder in placeholders)
                {
                    filledTemplate = filledTemplate.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                }

                Log.Debug("Filled email template with {Count} placeholders", placeholders.Count);

                return filledTemplate;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error filling email template");
                throw;
            }
        }
    }
}

