using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApplication.Migrations
{
    public partial class ChangedEmailfromalternatekeytounique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Students_Email",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Students_Email",
                table: "Students",
                column: "Email");
        }
    }
}
