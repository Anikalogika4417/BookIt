using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Businesses_CreatedAt",
                table: "Businesses",
                column: "CreatedAt",
                descending: []);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Businesses_CreatedAt",
                table: "Businesses");
        }
    }
}
