CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "users" (
    "id" TEXT NOT NULL CONSTRAINT "PK_users" PRIMARY KEY,
    "user_name" TEXT NOT NULL,
    "password_hash" TEXT NOT NULL
);

CREATE UNIQUE INDEX "IX_users_user_name" ON "users" ("user_name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251004185545_InitialCreate', '9.0.9');

COMMIT;

