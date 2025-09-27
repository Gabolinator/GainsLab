using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GainsLab.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Equipments_DescriptorID;");
            migrationBuilder.CreateIndex(
                name: "IX_Equipments_DescriptorID",
                table: "Equipments",
                column: "DescriptorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Descriptors_DescriptorID",
                table: "Equipments",
                column: "DescriptorID",
                principalTable: "Descriptors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Descriptors_DescriptorID",
                table: "Equipments");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_DescriptorID",
                table: "Equipments");
        }
    }
}
