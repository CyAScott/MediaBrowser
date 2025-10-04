# MediaBrowser

# Schema Updates

After making changes, run these commands from the root of the repo:
```
cd src/MediaBrowser
dotnet ef migrations add NameOfSchemaChange
dotnet ef migrations script -o schema-sqlite.sql
```
