using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SirJoshua.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadsAndEnquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "bundle-band8");

            migrationBuilder.DeleteData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "pte-rapid");

            migrationBuilder.DeleteData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "toefl-integrated");

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    WhatsApp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    InterestType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ProductOrServiceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CurrentBand = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TargetBand = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TestDate = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PreferredDate = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PreferredTime = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FocusArea = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    SourcePage = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WritingFeedbackPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    WhatsApp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TotalCredits = table.Column<int>(type: "integer", nullable: false),
                    UsedCredits = table.Column<int>(type: "integer", nullable: false),
                    RemainingCredits = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingFeedbackPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WritingFeedbackSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    WhatsApp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IeltsModule = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TaskType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TargetBand = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TestDate = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    WritingText = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: true),
                    UploadedFileUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false),
                    EstimatedBandScore = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    FeedbackNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingFeedbackSubmissions", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "reading-speed",
                columns: new[] { "Author", "Description", "Meta", "SortOrder" },
                values: new object[] { "Sir Joshua Academy", "Skimming & scanning techniques to save time in the IELTS Reading test.", "132 pp · PDF", 3 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "speaking-fear",
                columns: new[] { "Author", "Description", "Meta" },
                values: new object[] { "Sir Joshua Academy", "Fluency drills and natural sample answers for Speaking Parts 1–3.", "180 pp · PDF" });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "vocab-builder",
                columns: new[] { "Author", "Badge", "Category", "Description", "Meta", "Series", "SortOrder" },
                values: new object[] { "Sir Joshua Academy", "Best seller", "General", "The 1,200 most common academic words, complete with example sentences.", "210 pp · PDF + EPUB", "General · Vocabulary", 4 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "writing-playbook",
                columns: new[] { "Author", "Badge", "Description", "Meta" },
                values: new object[] { "Sir Joshua Academy", "Best seller", "Answer frameworks for Task 1 & 2 to stay consistent at band 7+.", "248 pp · PDF + EPUB" });

            migrationBuilder.InsertData(
                table: "Ebooks",
                columns: new[] { "Id", "Author", "Badge", "Category", "CompareAtIdr", "Cover", "Description", "IsBundle", "Meta", "PriceIdr", "Rating", "ReviewCount", "Series", "SortOrder", "Title" },
                values: new object[,]
                {
                    { "band-7-formula", "Sir Joshua Academy", null, "IELTS", 599000, "teal", "A structured guide to high-scoring IELTS Writing — Task 1 & 2 structures, Band 7+ vocabulary, sample responses and exam-day strategy.", false, "Digital guide · PDF", 399000, 5.0m, 0, "IELTS · Writing", 0, "IELTS Writing Mastery: The Band 7+ Formula" },
                    { "bundle-band7", "Sir Joshua Academy", null, "Bundle", 674000, "deep", "All the supporting IELTS practice guides in one bundle. Save versus buying separately.", true, "6 guides · PDF + EPUB", 499000, 5.0m, 0, "Bundle · Practice", 7, "Complete IELTS Practice Bundle" },
                    { "grammar-essentials", "Sir Joshua Academy", null, "General", null, "deep", "The grammar range and accuracy examiners reward — explained simply.", false, "176 pp · PDF", 109000, 4.8m, 67, "General · Grammar", 6, "Grammar Essentials for Band 7+" },
                    { "listening-focus", "Sir Joshua Academy", "New", "IELTS", null, "ink", "Prediction and note-taking techniques to lift your IELTS Listening score.", false, "150 pp · PDF", 99000, 4.7m, 41, "IELTS · Listening", 5, "Listening: Predict & Note" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedAt",
                table: "Leads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ReferenceNumber",
                table: "Leads",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WritingFeedbackPackages_ReferenceNumber",
                table: "WritingFeedbackPackages",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WritingFeedbackSubmissions_ReferenceNumber",
                table: "WritingFeedbackSubmissions",
                column: "ReferenceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "WritingFeedbackPackages");

            migrationBuilder.DropTable(
                name: "WritingFeedbackSubmissions");

            migrationBuilder.DeleteData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "band-7-formula");

            migrationBuilder.DeleteData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "bundle-band7");

            migrationBuilder.DeleteData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "grammar-essentials");

            migrationBuilder.DeleteData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "listening-focus");

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "reading-speed",
                columns: new[] { "Author", "Description", "Meta", "SortOrder" },
                values: new object[] { "Aditya Pranata", "Teknik skimming & scanning untuk hemat waktu di ujian.", "132 hal · PDF", 6 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "speaking-fear",
                columns: new[] { "Author", "Description", "Meta" },
                values: new object[] { "Sarah Whitmore", "Latihan fluency dan contoh jawaban Part 1–3 yang natural.", "180 hal · PDF" });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "vocab-builder",
                columns: new[] { "Author", "Badge", "Category", "Description", "Meta", "Series", "SortOrder" },
                values: new object[] { "Tim Sir Joshua", "Terlaris", "Umum", "1.200 kata akademik tersering, lengkap dengan contoh kalimat.", "210 hal · PDF + EPUB", "Umum · Vocabulary", 5 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "writing-playbook",
                columns: new[] { "Author", "Badge", "Description", "Meta" },
                values: new object[] { "Aditya Pranata", "Terlaris", "Kerangka jawaban Task 1 & 2 untuk konsisten di band 7+.", "248 hal · PDF + EPUB" });

            migrationBuilder.InsertData(
                table: "Ebooks",
                columns: new[] { "Id", "Author", "Badge", "Category", "CompareAtIdr", "Cover", "Description", "IsBundle", "Meta", "PriceIdr", "Rating", "ReviewCount", "Series", "SortOrder", "Title" },
                values: new object[,]
                {
                    { "bundle-band8", "Tim Sir Joshua", null, "Bundle", 784000, "deep", "Semua panduan IELTS, TOEFL, PTE, dan vocabulary dalam satu paket. Hemat Rp 185.000 dibanding beli satuan.", true, "6 ebook · PDF + EPUB", 599000, 5.0m, 0, "Bundle · Band 8", 7, "Paket Lengkap Band 8" },
                    { "pte-rapid", "Maya Setiawan", null, "PTE", null, "deep", "Teknik menembus skor 79+ dengan format AI scoring PTE.", false, "156 hal · PDF", 139000, 4.8m, 61, "PTE · Academic", 4, "PTE Academic Rapid Score" },
                    { "toefl-integrated", "James O'Connor", "Baru", "TOEFL", null, "ink", "Strategi integrated tasks untuk reading, listening & speaking.", false, "312 hal · PDF + EPUB", 179000, 4.7m, 74, "TOEFL · iBT", 3, "TOEFL iBT: The Integrated Method" }
                });
        }
    }
}
