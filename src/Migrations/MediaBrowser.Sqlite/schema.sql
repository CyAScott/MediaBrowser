CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "media" (
    "id" TEXT NOT NULL CONSTRAINT "PK_media" PRIMARY KEY,
    "path" TEXT NOT NULL,
    "title" TEXT NOT NULL,
    "original_title" TEXT NOT NULL,
    "description" TEXT NOT NULL,
    "mime" TEXT NOT NULL,
    "size" INTEGER NULL,
    "width" INTEGER NULL,
    "height" INTEGER NULL,
    "duration" REAL NULL,
    "md5" TEXT NOT NULL,
    "rating" REAL NULL,
    "user_star_rating" INTEGER NULL,
    "published" TEXT NOT NULL,
    "ctime_ms" INTEGER NOT NULL,
    "mtime_ms" INTEGER NOT NULL,
    "created_on" TEXT NULL,
    "updated_on" TEXT NULL,
    "ffprobe" TEXT NOT NULL
);

CREATE TABLE "users" (
    "id" TEXT NOT NULL CONSTRAINT "PK_users" PRIMARY KEY,
    "user_name" TEXT NOT NULL,
    "password_hash" TEXT NOT NULL
);

CREATE TABLE "media_cast" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_media_cast" PRIMARY KEY AUTOINCREMENT,
    "media_id" TEXT NOT NULL,
    "cast_member" TEXT NOT NULL,
    CONSTRAINT "FK_media_cast_media_media_id" FOREIGN KEY ("media_id") REFERENCES "media" ("id") ON DELETE CASCADE
);

CREATE TABLE "media_directors" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_media_directors" PRIMARY KEY AUTOINCREMENT,
    "media_id" TEXT NOT NULL,
    "director" TEXT NOT NULL,
    CONSTRAINT "FK_media_directors_media_media_id" FOREIGN KEY ("media_id") REFERENCES "media" ("id") ON DELETE CASCADE
);

CREATE TABLE "media_genres" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_media_genres" PRIMARY KEY AUTOINCREMENT,
    "media_id" TEXT NOT NULL,
    "genre" TEXT NOT NULL,
    CONSTRAINT "FK_media_genres_media_media_id" FOREIGN KEY ("media_id") REFERENCES "media" ("id") ON DELETE CASCADE
);

CREATE TABLE "media_producers" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_media_producers" PRIMARY KEY AUTOINCREMENT,
    "media_id" TEXT NOT NULL,
    "producer" TEXT NOT NULL,
    CONSTRAINT "FK_media_producers_media_media_id" FOREIGN KEY ("media_id") REFERENCES "media" ("id") ON DELETE CASCADE
);

CREATE TABLE "media_writers" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_media_writers" PRIMARY KEY AUTOINCREMENT,
    "media_id" TEXT NOT NULL,
    "writer" TEXT NOT NULL,
    CONSTRAINT "FK_media_writers_media_media_id" FOREIGN KEY ("media_id") REFERENCES "media" ("id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_media_cast_media_id_cast_member" ON "media_cast" ("media_id", "cast_member");

CREATE UNIQUE INDEX "IX_media_directors_media_id_director" ON "media_directors" ("media_id", "director");

CREATE UNIQUE INDEX "IX_media_genres_media_id_genre" ON "media_genres" ("media_id", "genre");

CREATE UNIQUE INDEX "IX_media_producers_media_id_producer" ON "media_producers" ("media_id", "producer");

CREATE UNIQUE INDEX "IX_media_writers_media_id_writer" ON "media_writers" ("media_id", "writer");

CREATE UNIQUE INDEX "IX_users_user_name" ON "users" ("user_name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251013215024_InitialCreate', '9.0.9');

ALTER TABLE "media" ADD "thumbnail" REAL NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251017193532_Thumbnail', '9.0.9');

COMMIT;

