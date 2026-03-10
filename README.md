# MediaBrowser

# Schema Updates

After making changes, run these commands from the root of the repo:
```sh
export NAME=InitialCreate
cd src/MediaBrowser

export db__type=sqlite
dotnet ef migrations add $NAME --project ../Migrations/MediaBrowser.Sqlite/MediaBrowser.Sqlite.csproj
dotnet ef migrations script --output ../Migrations/MediaBrowser.Sqlite/schema.sql

export db__type=mySql
dotnet ef migrations add $NAME --project ../Migrations/MediaBrowser.MySql/MediaBrowser.MySql.csproj
dotnet ef migrations script --output ../Migrations/MediaBrowser.MySql/schema.sql --idempotent

export db__type=postgres
dotnet ef migrations add $NAME --project ../Migrations/MediaBrowser.Postgres/MediaBrowser.Postgres.csproj
dotnet ef migrations script --output ../Migrations/MediaBrowser.Postgres/schema.sql --idempotent

export db__type=sqlServer
dotnet ef migrations add $NAME --project ../Migrations/MediaBrowser.SqlServer/MediaBrowser.SqlServer.csproj
dotnet ef migrations script --output ../Migrations/MediaBrowser.SqlServer/schema.sql --idempotent
```

# Docker

To build the docker image run the following from the root of the repository:
```sh
export FULL_SEM_VER=1.0.0
cd src
docker build -t mediabrowser:$FULL_SEM_VER \
  --build-arg FULL_SEM_VER_ARG=$FULL_SEM_VER \
  --build-arg ASSEMBLY_SEM_VER_ARG=$FULL_SEM_VER.0 \
  --build-arg ASSEMBLY_SEM_FILE_VER_ARG=$FULL_SEM_VER.0 \
  --build-arg INFORMATIONAL_VERSION_ARG=$FULL_SEM_VER.0 \
  .
```

To run the docker image run the following command:
```sh
docker run -d -p 5050:5050 --name mediabrowser-test mediabrowser
```

# Config

The best way to configure the service is through environment variables. Here are the variables:

* `cookies__secure` (boolean) - set to true when the cookie should only be set to "secure" (meaning https only cookie). The default is true.
* `db__migrateOnBoot` (boolean) - set to true to automatically update the DB schema on boot. The default is true.
* `db__type` (enum / string) - the type of DB: `mySql`, `postgres`, `sqlite`, and `sqlServer`. The default is `sqlite`.
* `db__mySqlConnectionString` (string) - required if `db__type` is `mySql`.
* `db__postgresConnectionString` (string) - required if `db__type` is `postgres`.
* `db__sqliteConnectionString` (string) - required if `db__type` is `sqlite`. The default is to use an in memory SQLite DB file which means if the process restarts then the DB contents are gone.
* `db__sqlServerConnectionString` (string) - required if `db__type` is `sqlServer`.
* `jwt__audience` (string) - this is the audience value for the authentication JWT. The default is `mediaBrowserAudience`.
* `jwt__expiryMinutes` (integer) - this is the number of minutes that the authentication JWT is set to expire and is also used to set the expiration for the cookie that contains the JWT. The default is `60`.
* `jwt__issuer` (string) - this is the issuer value for the authentication JWT. The default is `mediaBrowserIssuer`.
* `jwt__secretKey` (string) - it is important to set this value to a randomized long string (like a password). It is used for the JWT signing algorithm `HS256`. The default is `mediaBrowserSecretKey`.
* `initial__username` (string) - when the system boots it will create a user with this name (if the user doesn't already exists). The default is `admin`.
* `initial__password` (string) - the password to give for `initial__username`. The default is `admin`.
* `media__castDirectory` (string) - the directory path where cast member JPG files are stored. The file name should match name in the DB (i.e. `John Doe.jpg` should match `John Doe`). The default is `/cast`.
* `media__directorsDirectory` (string) - the directory path where director JPG files are stored. The file name should match name in the DB (i.e. `John Doe.jpg` should match `John Doe`). The default is `/directors`.
* `media__genresDirectory` (string) - the directory path where genre  JPG files are stored. The file name should match name in the DB (i.e. `Horror.jpg` should match `Horror`). The default is `/genres`.
* `media__producersDirectory` (string) - the directory path where producer JPG files are stored. The file name should match name in the DB (i.e. `John Doe.jpg` should match `John Doe`). The default is `/producers`.
* `media__writersDirectory` (string) - the directory path where writer JPG files are stored. The file name should match name in the DB (i.e. `John Doe.jpg` should match `John Doe`). The default is `/writers`.
* `media__mediaDirectory` (string) - the directory path where media files are stored with their thumbnails and NFO files. The file name should be the MD5 hash (lowercase) of the media file followed by the file extension (i.e. `1a79a4d60de6718e8e5b326e338ae533.mp4`). The thumbnail file should be `1a79a4d60de6718e8e5b326e338ae533.jpg` and optionally the fanart thumbnail should be `1a79a4d60de6718e8e5b326e338ae533-fanart.jpg`. The [NFO file](https://en.wikipedia.org/wiki/.nfo) should be `1a79a4d60de6718e8e5b326e338ae533.nfo`. The default is `/media`.
* `media__syncOnBoot` (boolean) - when true this will scan the media directory for NFO files and import the data from those files into the DB. After the import the service will terminate.
* `stopAfterSync` (boolean) - when true and `media__syncOnBoot` is true then the application will shutdown after importing the files.
* `media__importExtensions` (dictionary) - a dictionary of file extensions and `order`, `mime`, and `ext`. The `order` is used to sort the dictionary so that using the mine for a file can be used to find a corresponding file extension (since it is possible to find multiple file extensions). The `mime` is the corresponding mime type for the file extension. The `ext` is the normalized file extension (i.e. `wave` is normalized to `wav`).
* `media__importDirectory` (string) - a directory where files can be dumped so that they can be imported into the `media__mediaDirectory`.

# TODO - Features

* Cast, director, genre, producer, and writer management.
* M3u playlist support.