using System.Text.Json;

namespace SirJoshua.Web.Content;

/// <summary>
/// Builds schema.org JSON-LD for the funnel. Generated server-side and serialised with
/// System.Text.Json so the output is always valid JSON and HTML-safe (the default encoder escapes
/// <c>&lt;</c>, <c>&gt;</c> and <c>&amp;</c>, so it cannot break out of the &lt;script&gt; tag).
/// No aggregate ratings or reviews are emitted — there are no verified reviews to cite.
/// </summary>
public static class StructuredData
{
    public static string HomeJsonLd(string baseUrl)
    {
        var graph = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["@type"] = "EducationalOrganization",
                ["@id"] = baseUrl + "/#org",
                ["name"] = SiteContent.ParentBrand,
                ["alternateName"] = SiteContent.Brand,
                ["url"] = baseUrl + "/",
                ["description"] = "IELTS Band 7 Plus™ helps students, professionals and future international applicants reach their target IELTS score through practical resources, proven strategies and personalized guidance.",
            },
            new Dictionary<string, object?>
            {
                ["@type"] = "Product",
                ["@id"] = baseUrl + SiteContent.Flagship.Route + "#product",
                ["name"] = SiteContent.Flagship.Name,
                ["description"] = SiteContent.Flagship.Description,
                ["category"] = "IELTS Writing preparation",
                ["brand"] = new Dictionary<string, object?> { ["@type"] = "Brand", ["name"] = SiteContent.Brand },
                ["offers"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Offer",
                    ["price"] = SiteContent.Flagship.LaunchPriceIdr.ToString(),
                    ["priceCurrency"] = "IDR",
                    ["availability"] = "https://schema.org/InStock",
                    ["url"] = baseUrl + SiteContent.Flagship.Route,
                },
            },
        };

        foreach (var s in SiteContent.Services)
        {
            graph.Add(new Dictionary<string, object?>
            {
                ["@type"] = "Service",
                ["name"] = s.Name,
                ["serviceType"] = "IELTS preparation service",
                ["description"] = s.Purpose,
                ["provider"] = new Dictionary<string, object?> { ["@id"] = baseUrl + "/#org" },
                ["offers"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Offer",
                    ["price"] = s.PriceIdr.ToString(),
                    ["priceCurrency"] = "IDR",
                },
            });
        }

        var doc = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = graph,
        };
        return JsonSerializer.Serialize(doc);
    }
}
