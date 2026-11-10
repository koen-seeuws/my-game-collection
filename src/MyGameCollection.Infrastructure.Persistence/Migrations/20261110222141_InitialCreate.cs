using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGameCollection.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platforms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    release_year = table.Column<int>(type: "integer", nullable: true),
                    aliases = table.Column<List<string>>(type: "text[]", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platforms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    platform_id = table.Column<Guid>(type: "uuid", nullable: false),
                    region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    condition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    has_box = table.Column<bool>(type: "boolean", nullable: false),
                    has_manual = table.Column<bool>(type: "boolean", nullable: false),
                    is_sealed = table.Column<bool>(type: "boolean", nullable: false),
                    barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    purchase_price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: true),
                    estimated_value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    storage_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    publisher = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    developer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    genre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    release_year = table.Column<int>(type: "integer", nullable: true),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    edition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_items_platforms_platform_id",
                        column: x => x.platform_id,
                        principalTable: "platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    thumbnail_file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_photos_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_photos_item_id",
                table: "item_photos",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_items_barcode",
                table: "items",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "ix_items_platform_id",
                table: "items",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "ix_items_title",
                table: "items",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_platforms_name",
                table: "platforms",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_photos");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "platforms");
        }
    }
}
