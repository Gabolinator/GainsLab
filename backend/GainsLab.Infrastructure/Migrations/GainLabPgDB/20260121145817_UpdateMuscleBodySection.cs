using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GainsLab.Infrastructure.Migrations.GainLabPgDB
{
    /// <inheritdoc />
    public partial class UpdateMuscleBodySection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "body_section",
                schema: "public",
                table: "muscles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "body_section",
                schema: "public",
                table: "muscles",
                type: "integer",
                nullable: false,
                defaultValue: 3,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
