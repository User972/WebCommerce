using SirJoshua.Web.Models;

namespace SirJoshua.Web.Data;

/// <summary>
/// Catalog seeded to match the "Toko ebook" section of the Check Homepage design,
/// rebranded as Sir Joshua Press. Prices are in whole IDR.
/// </summary>
public static class SeedData
{
    public static readonly Ebook[] Ebooks =
    {
        new()
        {
            Id = "writing-playbook",
            Title = "The IELTS Writing Playbook",
            Author = "Aditya Pranata",
            Description = "Kerangka jawaban Task 1 & 2 untuk konsisten di band 7+.",
            PriceIdr = 149_000,
            Category = "IELTS",
            Cover = "teal",
            Series = "IELTS · Writing",
            Meta = "248 hal · PDF + EPUB",
            Rating = 4.9m,
            ReviewCount = 128,
            Badge = "Terlaris",
            SortOrder = 1
        },
        new()
        {
            Id = "speaking-fear",
            Title = "Speaking Without Fear",
            Author = "Sarah Whitmore",
            Description = "Latihan fluency dan contoh jawaban Part 1–3 yang natural.",
            PriceIdr = 129_000,
            Category = "IELTS",
            Cover = "terra",
            Series = "IELTS · Speaking",
            Meta = "180 hal · PDF",
            Rating = 4.8m,
            ReviewCount = 96,
            SortOrder = 2
        },
        new()
        {
            Id = "toefl-integrated",
            Title = "TOEFL iBT: The Integrated Method",
            Author = "James O'Connor",
            Description = "Strategi integrated tasks untuk reading, listening & speaking.",
            PriceIdr = 179_000,
            Category = "TOEFL",
            Cover = "ink",
            Series = "TOEFL · iBT",
            Meta = "312 hal · PDF + EPUB",
            Rating = 4.7m,
            ReviewCount = 74,
            Badge = "Baru",
            SortOrder = 3
        },
        new()
        {
            Id = "pte-rapid",
            Title = "PTE Academic Rapid Score",
            Author = "Maya Setiawan",
            Description = "Teknik menembus skor 79+ dengan format AI scoring PTE.",
            PriceIdr = 139_000,
            Category = "PTE",
            Cover = "deep",
            Series = "PTE · Academic",
            Meta = "156 hal · PDF",
            Rating = 4.8m,
            ReviewCount = 61,
            SortOrder = 4
        },
        new()
        {
            Id = "vocab-builder",
            Title = "Academic Vocabulary Builder",
            Author = "Tim Sir Joshua",
            Description = "1.200 kata akademik tersering, lengkap dengan contoh kalimat.",
            PriceIdr = 99_000,
            Category = "Umum",
            Cover = "gold",
            Series = "Umum · Vocabulary",
            Meta = "210 hal · PDF + EPUB",
            Rating = 4.9m,
            ReviewCount = 203,
            Badge = "Terlaris",
            SortOrder = 5
        },
        new()
        {
            Id = "reading-speed",
            Title = "Reading Speed & Skimming",
            Author = "Aditya Pranata",
            Description = "Teknik skimming & scanning untuk hemat waktu di ujian.",
            PriceIdr = 89_000,
            Category = "IELTS",
            Cover = "sand",
            Series = "IELTS · Reading",
            Meta = "132 hal · PDF",
            Rating = 4.6m,
            ReviewCount = 54,
            SortOrder = 6
        },
        new()
        {
            Id = "bundle-band8",
            Title = "Paket Lengkap Band 8",
            Author = "Tim Sir Joshua",
            Description = "Semua panduan IELTS, TOEFL, PTE, dan vocabulary dalam satu paket. Hemat Rp 185.000 dibanding beli satuan.",
            PriceIdr = 599_000,
            CompareAtIdr = 784_000,
            Category = "Bundle",
            Cover = "deep",
            Series = "Bundle · Band 8",
            Meta = "6 ebook · PDF + EPUB",
            Rating = 5.0m,
            ReviewCount = 0,
            IsBundle = true,
            SortOrder = 7
        }
    };
}
