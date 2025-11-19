using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GainsLab.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitLocalSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "descriptors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "now()"),
                    updated_seq = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    authority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_descriptors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_changes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Entity = table.Column<string>(type: "TEXT", nullable: false),
                    EntityGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangeType = table.Column<int>(type: "INTEGER", nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    sent = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_changes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncStates",
                columns: table => new
                {
                    Partition = table.Column<string>(type: "TEXT", nullable: false),
                    SeedCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    SeedVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSeedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    SeedInProgress = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastDeltaAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    UpstreamSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    CursorsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncStates", x => x.Partition);
                });

            migrationBuilder.CreateTable(
                name: "equipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptorID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_seq = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    authority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_equipments_descriptors_DescriptorID",
                        column: x => x.DescriptorID,
                        principalTable: "descriptors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movement_category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptorID = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentCategoryDbId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "now()"),
                    updated_seq = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    authority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movement_category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movement_category_descriptors_DescriptorID",
                        column: x => x.DescriptorID,
                        principalTable: "descriptors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movement_category_movement_category_ParentCategoryDbId",
                        column: x => x.ParentCategoryDbId,
                        principalTable: "movement_category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "muscles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptorID = table.Column<int>(type: "INTEGER", nullable: false),
                    body_section = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_seq = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    authority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muscles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_muscles_descriptors_DescriptorID",
                        column: x => x.DescriptorID,
                        principalTable: "descriptors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movement_category_relations",
                columns: table => new
                {
                    ParentCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildCategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movement_category_relations", x => new { x.ParentCategoryId, x.ChildCategoryId });
                    table.ForeignKey(
                        name: "FK_movement_category_relations_movement_category_ChildCategoryId",
                        column: x => x.ChildCategoryId,
                        principalTable: "movement_category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movement_category_relations_movement_category_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "movement_category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "muscle_antagonists",
                columns: table => new
                {
                    MuscleId = table.Column<int>(type: "INTEGER", nullable: false),
                    AntagonistId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muscle_antagonists", x => new { x.MuscleId, x.AntagonistId });
                    table.ForeignKey(
                        name: "FK_muscle_antagonists_muscles_AntagonistId",
                        column: x => x.AntagonistId,
                        principalTable: "muscles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_muscle_antagonists_muscles_MuscleId",
                        column: x => x.MuscleId,
                        principalTable: "muscles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_descriptors_GUID",
                table: "descriptors",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_descriptors_updated_at_utc_updated_seq",
                table: "descriptors",
                columns: new[] { "updated_at_utc", "updated_seq" });

            migrationBuilder.CreateIndex(
                name: "IX_equipments_DescriptorID",
                table: "equipments",
                column: "DescriptorID");

            migrationBuilder.CreateIndex(
                name: "IX_equipments_GUID",
                table: "equipments",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipments_updated_at_utc_updated_seq",
                table: "equipments",
                columns: new[] { "updated_at_utc", "updated_seq" });

            migrationBuilder.CreateIndex(
                name: "IX_movement_category_DescriptorID",
                table: "movement_category",
                column: "DescriptorID");

            migrationBuilder.CreateIndex(
                name: "IX_movement_category_GUID",
                table: "movement_category",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movement_category_ParentCategoryDbId",
                table: "movement_category",
                column: "ParentCategoryDbId");

            migrationBuilder.CreateIndex(
                name: "IX_movement_category_updated_at_utc_updated_seq",
                table: "movement_category",
                columns: new[] { "updated_at_utc", "updated_seq" });

            migrationBuilder.CreateIndex(
                name: "IX_movement_category_relations_ChildCategoryId",
                table: "movement_category_relations",
                column: "ChildCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_muscle_antagonists_AntagonistId",
                table: "muscle_antagonists",
                column: "AntagonistId");

            migrationBuilder.CreateIndex(
                name: "IX_muscles_DescriptorID",
                table: "muscles",
                column: "DescriptorID");

            migrationBuilder.CreateIndex(
                name: "IX_muscles_GUID",
                table: "muscles",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_muscles_updated_at_utc_updated_seq",
                table: "muscles",
                columns: new[] { "updated_at_utc", "updated_seq" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_changes_sent_occurred_at",
                table: "outbox_changes",
                columns: new[] { "sent", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipments");

            migrationBuilder.DropTable(
                name: "movement_category_relations");

            migrationBuilder.DropTable(
                name: "muscle_antagonists");

            migrationBuilder.DropTable(
                name: "outbox_changes");

            migrationBuilder.DropTable(
                name: "SyncStates");

            migrationBuilder.DropTable(
                name: "movement_category");

            migrationBuilder.DropTable(
                name: "muscles");

            migrationBuilder.DropTable(
                name: "descriptors");
        }
    }
}
