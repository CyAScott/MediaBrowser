CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE TABLE `media` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `path` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `original_title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `description` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
        `mime` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `size` bigint NULL,
        `width` int NULL,
        `height` int NULL,
        `duration` double NULL,
        `md5` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
        `rating` double NULL,
        `user_star_rating` int NULL,
        `published` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `ctime_ms` bigint NOT NULL,
        `mtime_ms` bigint NOT NULL,
        `created_on` datetime(6) NULL,
        `updated_on` datetime(6) NULL,
        `ffprobe` JSON NOT NULL,
        CONSTRAINT `PK_media` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE TABLE `users` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `user_name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `password_hash` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_users` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE TABLE `media_cast` (
        `id` int NOT NULL AUTO_INCREMENT,
        `media_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `cast_member` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_media_cast` PRIMARY KEY (`id`),
        CONSTRAINT `FK_media_cast_media_media_id` FOREIGN KEY (`media_id`) REFERENCES `media` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE TABLE `media_directors` (
        `id` int NOT NULL AUTO_INCREMENT,
        `media_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `director` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_media_directors` PRIMARY KEY (`id`),
        CONSTRAINT `FK_media_directors_media_media_id` FOREIGN KEY (`media_id`) REFERENCES `media` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE TABLE `media_genres` (
        `id` int NOT NULL AUTO_INCREMENT,
        `media_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `genre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_media_genres` PRIMARY KEY (`id`),
        CONSTRAINT `FK_media_genres_media_media_id` FOREIGN KEY (`media_id`) REFERENCES `media` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE TABLE `media_producers` (
        `id` int NOT NULL AUTO_INCREMENT,
        `media_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `producer` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_media_producers` PRIMARY KEY (`id`),
        CONSTRAINT `FK_media_producers_media_media_id` FOREIGN KEY (`media_id`) REFERENCES `media` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE TABLE `media_writers` (
        `id` int NOT NULL AUTO_INCREMENT,
        `media_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `writer` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_media_writers` PRIMARY KEY (`id`),
        CONSTRAINT `FK_media_writers_media_media_id` FOREIGN KEY (`media_id`) REFERENCES `media` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_media_cast_media_id_cast_member` ON `media_cast` (`media_id`, `cast_member`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_media_directors_media_id_director` ON `media_directors` (`media_id`, `director`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_media_genres_media_id_genre` ON `media_genres` (`media_id`, `genre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_media_producers_media_id_producer` ON `media_producers` (`media_id`, `producer`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_media_writers_media_id_writer` ON `media_writers` (`media_id`, `writer`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_users_user_name` ON `users` (`user_name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251013215024_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251013215024_InitialCreate', '9.0.9');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251017193532_Thumbnail') THEN

    ALTER TABLE `media` ADD `thumbnail` double NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251017193532_Thumbnail') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251017193532_Thumbnail', '9.0.9');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

