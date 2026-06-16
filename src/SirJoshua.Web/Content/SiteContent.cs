namespace SirJoshua.Web.Content;

/// <summary>
/// Centralised, strongly-typed source of truth for every IELTS Band 7 Plus™ offer shown on the
/// site — the flagship product, paid services, the free lead magnet, the "coming soon" series,
/// accepted payment methods, and supporting copy. Editing a price, an inclusion or a CTA here
/// updates the homepage, the product page and the enquiry forms in one place. No marketing copy
/// should be hard-coded inside the Razor views.
///
/// Brand architecture: <b>Sir Joshua Academy</b> is the parent authority brand; <b>IELTS Band 7
/// Plus™</b> is the flagship product funnel that sits under it.
/// </summary>
public static class SiteContent
{
    public const string Brand = "IELTS Band 7 Plus";
    public const string BrandTm = "IELTS Band 7 Plus™";
    public const string ParentBrand = "Sir Joshua Academy";
    public const string FooterBrand = "IELTS Band 7 Plus™ by Sir Joshua Academy";

    /// <summary>WhatsApp number used by enquiry CTAs (digits only, international format, no '+').</summary>
    public const string WhatsAppNumber = "6281100000000";
    public const string ContactEmail = "hello@sirjoshua.id";

    // ── Flagship product ──────────────────────────────────────────────────────────────────────
    public static readonly FlagshipProduct Flagship = new()
    {
        Id = "band-7-formula", // matches the Ebook catalog row so PayPal checkout can price it
        Name = "IELTS Writing Mastery: The Band 7+ Formula",
        ShortName = "The Band 7+ Formula",
        Subtitle = "The flagship IELTS Writing guide",
        Description =
            "A comprehensive guide designed to help IELTS candidates understand the key elements of " +
            "high-scoring writing responses and develop a structured approach to the IELTS Writing test.",
        RegularPriceIdr = 599_000,
        LaunchPriceIdr = 399_000,
        Route = "/band-7-formula",
        CtaLabel = "Get the Band 7+ Formula",
        LearningOutcomes = new[]
        {
            "Essay structures for Task 1 and Task 2",
            "Band 7+ vocabulary and expressions",
            "High-scoring sample responses",
            "Common mistakes and how to avoid them",
            "Writing improvement techniques",
            "Exam-day strategies",
        },
        WhoFor = new[]
        {
            "University applicants who need a Writing band of 7 or higher",
            "Professionals preparing for international roles or registration",
            "Candidates applying for migration who keep missing their Writing target",
            "Anyone who studies regularly but cannot move their Writing score",
        },
        ProblemsSolved = new[]
        {
            "Not knowing what examiners actually reward in Task 1 and Task 2",
            "Ideas that are hard to organise into a clear, coherent response",
            "Repeating the same vocabulary and grammar mistakes under time pressure",
            "Running out of time or ideas on exam day",
        },
        Inclusions = new[]
        {
            "Step-by-step structures for every Task 1 and Task 2 question type",
            "A Band 7+ vocabulary and linking-phrase bank with usage examples",
            "Annotated high-scoring sample responses",
            "A common-mistakes checklist with fixes",
            "A practical writing-improvement routine",
            "An exam-day timing and planning strategy",
        },
        Faq = new[]
        {
            new FaqItem("Is this for Academic or General Training?",
                "Both. The structures, vocabulary and strategies apply to Academic and General Training Writing; Task 1 differences are covered separately."),
            new FaqItem("What format is the guide delivered in?",
                "A digital guide (PDF) delivered to your email after purchase, readable on any device."),
            new FaqItem("Will this guarantee a Band 7?",
                "No responsible course can guarantee a score. The guide is designed to help you understand what high-scoring responses look like and how to build them. Results vary based on your starting level and study consistency."),
            new FaqItem("How is this different from free advice online?",
                "It replaces scattered tips with one structured, examiner-focused method so you always know what to do next."),
        },
    };

    // ── Paid services ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<ServiceOffer> Services = new[]
    {
        new ServiceOffer
        {
            Id = "writing-feedback",
            Name = "IELTS Writing Feedback",
            PriceIdr = 249_000,
            Purpose = "Helps candidates understand exactly where they need to improve.",
            CtaLabel = "Submit Writing for Feedback",
            FormType = "writing-feedback",
            Includes = new[]
            {
                "Detailed review of one writing task",
                "Estimated band score",
                "Feedback on strengths and weaknesses",
                "Suggestions for improvement",
            },
        },
        new ServiceOffer
        {
            Id = "writing-feedback-package",
            Name = "IELTS Writing Feedback Package",
            PriceIdr = 599_000,
            Purpose = "Supports consistent improvement over time.",
            CtaLabel = "Get 3 Writing Reviews",
            FormType = "writing-package",
            Badge = "Best value",
            Includes = new[]
            {
                "Review of three writing tasks",
                "Estimated band scores",
                "Personalized feedback",
                "Progress recommendations",
            },
        },
        new ServiceOffer
        {
            Id = "speaking-assessment",
            Name = "IELTS Speaking Assessment",
            PriceIdr = 399_000,
            Purpose = "Helps candidates identify strengths and areas for development before the actual exam.",
            CtaLabel = "Book Speaking Assessment",
            FormType = "speaking-assessment",
            Includes = new[]
            {
                "Simulated IELTS Speaking test",
                "Estimated band score",
                "Feedback on fluency, vocabulary, grammar and pronunciation",
                "Improvement recommendations",
            },
        },
        new ServiceOffer
        {
            Id = "strategy-consultation",
            Name = "IELTS Strategy Consultation",
            PriceIdr = 599_000,
            Purpose = "Provides candidates with a clear preparation roadmap.",
            CtaLabel = "Book Strategy Consultation",
            FormType = "strategy-consultation",
            Includes = new[]
            {
                "60-minute consultation session",
                "Assessment of current level",
                "Personalized study plan",
                "Recommendations based on target score and timeline",
            },
        },
    };

    public static ServiceOffer? FindService(string? id) =>
        id is null ? null : Services.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));

    // ── Free lead magnet ──────────────────────────────────────────────────────────────────────
    public static readonly LeadMagnet Checklist = new()
    {
        Id = "free-checklist",
        Title = "Free IELTS Study Checklist",
        Description =
            "Not sure where to start? Download the Free IELTS Study Checklist and get a clear preparation " +
            "path, practical study recommendations, and writing improvement tips.",
        CtaLabel = "Get Free IELTS Checklist",
        Benefits = new[]
        {
            "A printable IELTS preparation checklist",
            "Practical study recommendations",
            "Writing improvement tips",
            "Access to an email learning series",
        },
    };

    // ── Coming soon series ────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<ComingSoonProduct> ComingSoon = new[]
    {
        new ComingSoonProduct("IELTS Speaking Mastery", "Fluency, pronunciation and Part 1–3 confidence."),
        new ComingSoonProduct("IELTS Reading Mastery", "Skimming, scanning and time-saving techniques."),
        new ComingSoonProduct("IELTS Listening Mastery", "Prediction, note-taking and accent training."),
        new ComingSoonProduct("IELTS Vocabulary Mastery", "Topic vocabulary that lifts every band."),
    };

    // ── Payment methods (config-driven) ───────────────────────────────────────────────────────
    // SupportsCheckout = an automated in-app checkout exists today (only PayPal, when configured).
    // All listed methods are "accepted"; non-checkout methods are settled via a payment link.
    public static readonly IReadOnlyList<PaymentMethod> PaymentMethods = new[]
    {
        new PaymentMethod("paypal", "PayPal", supportsCheckout: true,
            "Pay securely with PayPal, or a credit/debit card via PayPal."),
        new PaymentMethod("stripe", "Stripe", supportsCheckout: false,
            "Card payments via Stripe — we send a secure payment link."),
        new PaymentMethod("wise", "Wise", supportsCheckout: false,
            "International transfer via Wise — details sent on request."),
        new PaymentMethod("visa", "Visa", supportsCheckout: false,
            "Visa cards accepted via our payment link."),
        new PaymentMethod("mastercard", "Mastercard", supportsCheckout: false,
            "Mastercard accepted via our payment link."),
        new PaymentMethod("qris", "QRIS", supportsCheckout: false,
            "Scan & pay from any Indonesian e-wallet via QRIS."),
    };

    // ── Success outcomes (safe, non-fabricated framing) ───────────────────────────────────────
    // NOTE: These describe what learners work toward — they are NOT testimonials and contain no
    // real names, photos or verified score claims. Replace with approved testimonials when available.
    public static readonly IReadOnlyList<SuccessOutcome> SuccessOutcomes = new[]
    {
        new SuccessOutcome("Clarity on what examiners reward",
            "Learners use the Band 7+ structures to plan responses with confidence instead of guessing."),
        new SuccessOutcome("A repeatable writing routine",
            "Candidates report knowing exactly what to improve next after structured feedback."),
        new SuccessOutcome("Calmer exam-day preparation",
            "A clear timing and planning strategy replaces last-minute uncertainty."),
    };

    // ── Interest options for the contact form / enquiry routing ───────────────────────────────
    public static readonly IReadOnlyList<string> ContactInterests = new[]
    {
        "Band 7+ Formula",
        "Writing Feedback",
        "Writing Feedback Package",
        "Speaking Assessment",
        "Strategy Consultation",
        "Free IELTS Checklist",
        "General enquiry",
    };

    // ── General FAQ (homepage) ────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<FaqItem> GeneralFaq = new[]
    {
        new FaqItem("Who is IELTS Band 7 Plus™ for?",
            "University applicants, professionals seeking international opportunities, people preparing for migration, and any IELTS candidate aiming for Band 7 or higher."),
        new FaqItem("Do you guarantee a Band 7?",
            "No. We provide structured resources, proven strategies and personalized feedback designed to help you improve. Results vary based on your starting level, study consistency and test readiness."),
        new FaqItem("How do the paid services work?",
            "Choose a service, submit the short form, and we’ll confirm the next steps and send secure payment instructions. Writing reviews and assessments come back with an estimated band score and clear, actionable feedback."),
        new FaqItem("How do I pay?",
            "We accept PayPal, Stripe, Wise, Visa, Mastercard and QRIS. For most options we send a secure payment link after you submit your request — we never collect card details on this site."),
        new FaqItem("Is the IELTS Study Checklist really free?",
            "Yes. Share your name and email and we’ll send the Free IELTS Study Checklist plus practical study recommendations and writing tips."),
        new FaqItem("Will I get my materials by email?",
            "Yes — digital products and resources are delivered to the email address you provide. If anything doesn’t arrive, contact us and we’ll resend it."),
    };

    // ── Primary navigation ────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<NavLink> Nav = new[]
    {
        new NavLink("Home", "#top"),
        new NavLink("The Band 7+ Formula", "#formula"),
        new NavLink("Services", "#services"),
        new NavLink("Success Stories", "#success"),
        new NavLink("Free Resources", "#free-checklist"),
        new NavLink("About Sir Joshua", "#about"),
        new NavLink("Contact", "#contact"),
    };
}

/// <summary>IDR money formatting that does not depend on a specific server culture being installed
/// (works even under invariant-globalization), e.g. 599000 → "Rp 599.000".</summary>
public static class Money
{
    public static string Rp(int idr) =>
        "Rp " + idr.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
}

public sealed record FaqItem(string Question, string Answer);
public sealed record NavLink(string Label, string Href);
public sealed record ComingSoonProduct(string Name, string Benefit);
public sealed record SuccessOutcome(string Title, string Body);

public sealed class PaymentMethod
{
    public PaymentMethod(string id, string name, bool supportsCheckout, string instructions)
    {
        Id = id;
        Name = name;
        SupportsCheckout = supportsCheckout;
        Instructions = instructions;
    }

    public string Id { get; }
    public string Name { get; }
    /// <summary>True only when an automated in-app checkout exists for this method today.</summary>
    public bool SupportsCheckout { get; }
    public string Instructions { get; }
}

public sealed class FlagshipProduct
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required string Subtitle { get; init; }
    public required string Description { get; init; }
    public required int RegularPriceIdr { get; init; }
    public required int LaunchPriceIdr { get; init; }
    public required string Route { get; init; }
    public required string CtaLabel { get; init; }
    public required IReadOnlyList<string> LearningOutcomes { get; init; }
    public required IReadOnlyList<string> WhoFor { get; init; }
    public required IReadOnlyList<string> ProblemsSolved { get; init; }
    public required IReadOnlyList<string> Inclusions { get; init; }
    public required IReadOnlyList<FaqItem> Faq { get; init; }

    public int SavingsIdr => RegularPriceIdr - LaunchPriceIdr;
}

public sealed class ServiceOffer
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required int PriceIdr { get; init; }
    public required string Purpose { get; init; }
    public required string CtaLabel { get; init; }
    /// <summary>Drives which fields the enquiry modal renders (see forms.js).</summary>
    public required string FormType { get; init; }
    public required IReadOnlyList<string> Includes { get; init; }
    public string? Badge { get; init; }
}

public sealed class LeadMagnet
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string CtaLabel { get; init; }
    public required IReadOnlyList<string> Benefits { get; init; }
}
