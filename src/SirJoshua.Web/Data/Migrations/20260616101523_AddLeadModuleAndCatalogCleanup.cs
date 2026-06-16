using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SirJoshua.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadModuleAndCatalogCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IeltsModule",
                table: "Leads",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "band-7-formula",
                column: "Rating",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "bundle-band7",
                column: "Rating",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "grammar-essentials",
                columns: new[] { "Rating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "listening-focus",
                columns: new[] { "Badge", "Rating", "ReviewCount" },
                values: new object[] { null, 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "reading-speed",
                columns: new[] { "Rating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "speaking-fear",
                columns: new[] { "Rating", "ReviewCount" },
                values: new object[] { 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "vocab-builder",
                columns: new[] { "Badge", "Rating", "ReviewCount" },
                values: new object[] { null, 0m, 0 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "writing-playbook",
                columns: new[] { "Badge", "Rating", "ReviewCount" },
                values: new object[] { null, 0m, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IeltsModule",
                table: "Leads");

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "band-7-formula",
                column: "Rating",
                value: 5.0m);

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "bundle-band7",
                column: "Rating",
                value: 5.0m);

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "grammar-essentials",
                columns: new[] { "Rating", "ReviewCount" },
                values: new object[] { 4.8m, 67 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "listening-focus",
                columns: new[] { "Badge", "Rating", "ReviewCount" },
                values: new object[] { "New", 4.7m, 41 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "reading-speed",
                columns: new[] { "Rating", "ReviewCount" },
                values: new object[] { 4.6m, 54 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "speaking-fear",
                columns: new[] { "Rating", "ReviewCount" },
                values: new object[] { 4.8m, 96 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "vocab-builder",
                columns: new[] { "Badge", "Rating", "ReviewCount" },
                values: new object[] { "Best seller", 4.9m, 203 });

            migrationBuilder.UpdateData(
                table: "Ebooks",
                keyColumn: "Id",
                keyValue: "writing-playbook",
                columns: new[] { "Badge", "Rating", "ReviewCount" },
                values: new object[] { "Best seller", 4.9m, 128 });
        }
    }
}
