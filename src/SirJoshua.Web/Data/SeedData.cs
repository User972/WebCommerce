using SirJoshua.Web.Models;

namespace SirJoshua.Web.Data;

/// <summary>
/// Catalog seed. The first entry is the flagship <b>IELTS Writing Mastery: The Band 7+ Formula</b>
/// product — it lives in the catalog so the existing server-authoritative PayPal checkout can price
/// it; it is rendered by its own premium product section (and excluded from the secondary resource
/// grid) in the homepage. The remaining entries are the supporting practice-resource library,
/// reframed in English. Prices are in whole IDR. Author bylines use the academy name rather than
/// named individuals (no instructor profiles are claimed).
///
/// Claims-safety: <see cref="Ebook.Rating"/>/<see cref="Ebook.ReviewCount"/> are intentionally zero
/// and no superlative <see cref="Ebook.Badge"/> is set — there are no verified reviews, so no
/// social-proof numbers or "best seller"-style claims can ever be rendered. Add real, approved
/// values later if/when they exist.
/// </summary>
public static class SeedData
{
    public const string FlagshipId = "band-7-formula";

    public static readonly Ebook[] Ebooks =
    {
        new()
        {
            Id = FlagshipId,
            Title = "IELTS Writing Mastery: The Band 7+ Formula",
            Author = "Sir Joshua Academy",
            Description = "A structured guide to high-scoring IELTS Writing — Task 1 & 2 structures, Band 7+ vocabulary, sample responses and exam-day strategy.",
            PriceIdr = 399_000,
            CompareAtIdr = 599_000,
            Category = "IELTS",
            Cover = "teal",
            Series = "IELTS · Writing",
            Meta = "Digital guide · PDF",
            SortOrder = 0
        },
        new()
        {
            Id = "writing-playbook",
            Title = "The IELTS Writing Playbook",
            Author = "Sir Joshua Academy",
            Description = "Answer frameworks for Task 1 & 2 to stay consistent at band 7+.",
            PriceIdr = 149_000,
            Category = "IELTS",
            Cover = "teal",
            Series = "IELTS · Writing",
            Meta = "248 pp · PDF + EPUB",
            SortOrder = 1
        },
        new()
        {
            Id = "speaking-fear",
            Title = "Speaking Without Fear",
            Author = "Sir Joshua Academy",
            Description = "Fluency drills and natural sample answers for Speaking Parts 1–3.",
            PriceIdr = 129_000,
            Category = "IELTS",
            Cover = "terra",
            Series = "IELTS · Speaking",
            Meta = "180 pp · PDF",
            SortOrder = 2
        },
        new()
        {
            Id = "reading-speed",
            Title = "Reading Speed & Skimming",
            Author = "Sir Joshua Academy",
            Description = "Skimming & scanning techniques to save time in the IELTS Reading test.",
            PriceIdr = 89_000,
            Category = "IELTS",
            Cover = "sand",
            Series = "IELTS · Reading",
            Meta = "132 pp · PDF",
            SortOrder = 3
        },
        new()
        {
            Id = "vocab-builder",
            Title = "Academic Vocabulary Builder",
            Author = "Sir Joshua Academy",
            Description = "The 1,200 most common academic words, complete with example sentences.",
            PriceIdr = 99_000,
            Category = "General",
            Cover = "gold",
            Series = "General · Vocabulary",
            Meta = "210 pp · PDF + EPUB",
            SortOrder = 4
        },
        new()
        {
            Id = "listening-focus",
            Title = "Listening: Predict & Note",
            Author = "Sir Joshua Academy",
            Description = "Prediction and note-taking techniques to lift your IELTS Listening score.",
            PriceIdr = 99_000,
            Category = "IELTS",
            Cover = "ink",
            Series = "IELTS · Listening",
            Meta = "150 pp · PDF",
            SortOrder = 5
        },
        new()
        {
            Id = "grammar-essentials",
            Title = "Grammar Essentials for Band 7+",
            Author = "Sir Joshua Academy",
            Description = "The grammar range and accuracy examiners reward — explained simply.",
            PriceIdr = 109_000,
            Category = "General",
            Cover = "deep",
            Series = "General · Grammar",
            Meta = "176 pp · PDF",
            SortOrder = 6
        },
        new()
        {
            Id = "bundle-band7",
            Title = "Complete IELTS Practice Bundle",
            Author = "Sir Joshua Academy",
            Description = "All the supporting IELTS practice guides in one bundle. Save versus buying separately.",
            PriceIdr = 499_000,
            CompareAtIdr = 674_000,
            Category = "Bundle",
            Cover = "deep",
            Series = "Bundle · Practice",
            Meta = "6 guides · PDF + EPUB",
            IsBundle = true,
            SortOrder = 7
        }
    };
}
