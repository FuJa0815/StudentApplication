using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApplication.Migrations
{
    public partial class ChangedPKs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseStudent_Courses_CoursesId",
                table: "CourseStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseStudent_Students_StudentsId",
                table: "CourseStudent");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Students",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "StudentsId",
                table: "CourseStudent",
                newName: "StudentsStudentId");

            migrationBuilder.RenameColumn(
                name: "CoursesId",
                table: "CourseStudent",
                newName: "CoursesCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseStudent_StudentsId",
                table: "CourseStudent",
                newName: "IX_CourseStudent_StudentsStudentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Courses",
                newName: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseStudent_Courses_CoursesCourseId",
                table: "CourseStudent",
                column: "CoursesCourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseStudent_Students_StudentsStudentId",
                table: "CourseStudent",
                column: "StudentsStudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseStudent_Courses_CoursesCourseId",
                table: "CourseStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseStudent_Students_StudentsStudentId",
                table: "CourseStudent");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Students",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StudentsStudentId",
                table: "CourseStudent",
                newName: "StudentsId");

            migrationBuilder.RenameColumn(
                name: "CoursesCourseId",
                table: "CourseStudent",
                newName: "CoursesId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseStudent_StudentsStudentId",
                table: "CourseStudent",
                newName: "IX_CourseStudent_StudentsId");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Courses",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseStudent_Courses_CoursesId",
                table: "CourseStudent",
                column: "CoursesId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseStudent_Students_StudentsId",
                table: "CourseStudent",
                column: "StudentsId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
