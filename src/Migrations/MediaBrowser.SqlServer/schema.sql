IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE TABLE [media] (
        [id] uniqueidentifier NOT NULL,
        [path] nvarchar(500) NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [original_title] nvarchar(200) NOT NULL,
        [description] nvarchar(2000) NOT NULL,
        [mime] nvarchar(100) NOT NULL,
        [size] bigint NULL,
        [width] int NULL,
        [height] int NULL,
        [duration] float NULL,
        [md5] nvarchar(32) NOT NULL,
        [rating] float NULL,
        [user_star_rating] int NULL,
        [published] nvarchar(100) NOT NULL,
        [ctime_ms] bigint NOT NULL,
        [mtime_ms] bigint NOT NULL,
        [created_on] datetime2 NULL,
        [updated_on] datetime2 NULL,
        [ffprobe] NVARCHAR(MAX) NOT NULL,
        CONSTRAINT [PK_media] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE TABLE [users] (
        [id] uniqueidentifier NOT NULL,
        [user_name] nvarchar(50) NOT NULL,
        [password_hash] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_users] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE TABLE [media_cast] (
        [id] int NOT NULL IDENTITY,
        [media_id] uniqueidentifier NOT NULL,
        [cast_member] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_media_cast] PRIMARY KEY ([id]),
        CONSTRAINT [FK_media_cast_media_media_id] FOREIGN KEY ([media_id]) REFERENCES [media] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE TABLE [media_directors] (
        [id] int NOT NULL IDENTITY,
        [media_id] uniqueidentifier NOT NULL,
        [director] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_media_directors] PRIMARY KEY ([id]),
        CONSTRAINT [FK_media_directors_media_media_id] FOREIGN KEY ([media_id]) REFERENCES [media] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE TABLE [media_genres] (
        [id] int NOT NULL IDENTITY,
        [media_id] uniqueidentifier NOT NULL,
        [genre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_media_genres] PRIMARY KEY ([id]),
        CONSTRAINT [FK_media_genres_media_media_id] FOREIGN KEY ([media_id]) REFERENCES [media] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE TABLE [media_producers] (
        [id] int NOT NULL IDENTITY,
        [media_id] uniqueidentifier NOT NULL,
        [producer] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_media_producers] PRIMARY KEY ([id]),
        CONSTRAINT [FK_media_producers_media_media_id] FOREIGN KEY ([media_id]) REFERENCES [media] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE TABLE [media_writers] (
        [id] int NOT NULL IDENTITY,
        [media_id] uniqueidentifier NOT NULL,
        [writer] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_media_writers] PRIMARY KEY ([id]),
        CONSTRAINT [FK_media_writers_media_media_id] FOREIGN KEY ([media_id]) REFERENCES [media] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_media_cast_media_id_cast_member] ON [media_cast] ([media_id], [cast_member]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_media_directors_media_id_director] ON [media_directors] ([media_id], [director]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_media_genres_media_id_genre] ON [media_genres] ([media_id], [genre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_media_producers_media_id_producer] ON [media_producers] ([media_id], [producer]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_media_writers_media_id_writer] ON [media_writers] ([media_id], [writer]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_users_user_name] ON [users] ([user_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251013215024_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251013215024_InitialCreate', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017193532_Thumbnail'
)
BEGIN
    ALTER TABLE [media] ADD [thumbnail] float NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017193532_Thumbnail'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251017193532_Thumbnail', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306204659_ExpandPasswordLength'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[users]') AND [c].[name] = N'password_hash');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [users] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [users] ALTER COLUMN [password_hash] nvarchar(125) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306204659_ExpandPasswordLength'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260306204659_ExpandPasswordLength', N'9.0.9');
END;

COMMIT;
GO

