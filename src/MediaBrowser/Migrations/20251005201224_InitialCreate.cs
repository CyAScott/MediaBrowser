using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaBrowser.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    path = table.Column<string>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    original_title = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    mime = table.Column<string>(type: "TEXT", nullable: false),
                    size = table.Column<long>(type: "INTEGER", nullable: true),
                    width = table.Column<int>(type: "INTEGER", nullable: true),
                    height = table.Column<int>(type: "INTEGER", nullable: true),
                    duration = table.Column<double>(type: "REAL", nullable: true),
                    md5 = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    rating = table.Column<double>(type: "REAL", nullable: true),
                    user_star_rating = table.Column<int>(type: "INTEGER", nullable: true),
                    published = table.Column<string>(type: "TEXT", nullable: false),
                    ctime_ms = table.Column<long>(type: "INTEGER", nullable: false),
                    mtime_ms = table.Column<long>(type: "INTEGER", nullable: false),
                    created_on = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_on = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ffprobe = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_cast",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", maxLength: 36, nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    media_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                    cast_member = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_cast", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_cast_media_media_id",
                        column: x => x.media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_directors",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", maxLength: 36, nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    media_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                    director = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_directors", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_directors_media_media_id",
                        column: x => x.media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_genres",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", maxLength: 36, nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    media_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                    genre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_genres", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_genres_media_media_id",
                        column: x => x.media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_producers",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", maxLength: 36, nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    media_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                    producer = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_producers", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_producers_media_media_id",
                        column: x => x.media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_writers",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", maxLength: 36, nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    media_id = table.Column<Guid>(type: "TEXT", maxLength: 36, nullable: false),
                    writer = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_writers", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_writers_media_media_id",
                        column: x => x.media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_media_cast_media_id_cast_member",
                table: "media_cast",
                columns: new[] { "media_id", "cast_member" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_directors_media_id_director",
                table: "media_directors",
                columns: new[] { "media_id", "director" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_genres_media_id_genre",
                table: "media_genres",
                columns: new[] { "media_id", "genre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_producers_media_id_producer",
                table: "media_producers",
                columns: new[] { "media_id", "producer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_writers_media_id_writer",
                table: "media_writers",
                columns: new[] { "media_id", "writer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_user_name",
                table: "users",
                column: "user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_cast");

            migrationBuilder.DropTable(
                name: "media_directors");

            migrationBuilder.DropTable(
                name: "media_genres");

            migrationBuilder.DropTable(
                name: "media_producers");

            migrationBuilder.DropTable(
                name: "media_writers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "media");
        }
    }
}
