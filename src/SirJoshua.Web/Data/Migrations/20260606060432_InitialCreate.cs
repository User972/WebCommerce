using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SirJoshua.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ebooks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Author = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    PriceIdr = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Cover = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Series = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Meta = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    Rating = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    Badge = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    IsBundle = table.Column<bool>(type: "boolean", nullable: false),
                    CompareAtIdr = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ebooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubtotalIdr = table.Column<int>(type: "integer", nullable: false),
                    DiscountIdr = table.Column<int>(type: "integer", nullable: false),
                    TotalIdr = table.Column<int>(type: "integer", nullable: false),
                    PromoCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AmountUsd = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PayPalOrderId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PayPalCaptureId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EbookId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    UnitPriceIdr = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Ebooks",
                columns: new[] { "Id", "Author", "Badge", "Category", "CompareAtIdr", "Cover", "Description", "IsBundle", "Meta", "PriceIdr", "Rating", "ReviewCount", "Series", "SortOrder", "Title" },
                values: new object[,]
                {
                    { "bundle-band8", "Tim Sir Joshua", null, "Bundle", 784000, "deep", "Semua panduan IELTS, TOEFL, PTE, dan vocabulary dalam satu paket. Hemat Rp 185.000 dibanding beli satuan.", true, "6 ebook · PDF + EPUB", 599000, 5.0m, 0, "Bundle · Band 8", 7, "Paket Lengkap Band 8" },
                    { "pte-rapid", "Maya Setiawan", null, "PTE", null, "deep", "Teknik menembus skor 79+ dengan format AI scoring PTE.", false, "156 hal · PDF", 139000, 4.8m, 61, "PTE · Academic", 4, "PTE Academic Rapid Score" },
                    { "reading-speed", "Aditya Pranata", null, "IELTS", null, "sand", "Teknik skimming & scanning untuk hemat waktu di ujian.", false, "132 hal · PDF", 89000, 4.6m, 54, "IELTS · Reading", 6, "Reading Speed & Skimming" },
                    { "speaking-fear", "Sarah Whitmore", null, "IELTS", null, "terra", "Latihan fluency dan contoh jawaban Part 1–3 yang natural.", false, "180 hal · PDF", 129000, 4.8m, 96, "IELTS · Speaking", 2, "Speaking Without Fear" },
                    { "toefl-integrated", "James O'Connor", "Baru", "TOEFL", null, "ink", "Strategi integrated tasks untuk reading, listening & speaking.", false, "312 hal · PDF + EPUB", 179000, 4.7m, 74, "TOEFL · iBT", 3, "TOEFL iBT: The Integrated Method" },
                    { "vocab-builder", "Tim Sir Joshua", "Terlaris", "Umum", null, "gold", "1.200 kata akademik tersering, lengkap dengan contoh kalimat.", false, "210 hal · PDF + EPUB", 99000, 4.9m, 203, "Umum · Vocabulary", 5, "Academic Vocabulary Builder" },
                    { "writing-playbook", "Aditya Pranata", "Terlaris", "IELTS", null, "teal", "Kerangka jawaban Task 1 & 2 untuk konsisten di band 7+.", false, "248 hal · PDF + EPUB", 149000, 4.9m, 128, "IELTS · Writing", 1, "The IELTS Writing Playbook" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PayPalOrderId",
                table: "Orders",
                column: "PayPalOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ebooks");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
