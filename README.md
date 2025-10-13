# MediaBrowser

# Schema Updates

After making changes, run these commands from the root of the repo:
```
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
