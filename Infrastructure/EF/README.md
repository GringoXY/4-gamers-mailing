# Updating DB

## Add migration
First of all make sure you have installed dotnet-ef tool:
```
dotnet tool install -g dotnet-ef
```

Then go to directory where the infrastructure is:
```
cd /Infrastructure
```

Now it's time to add migration:
```
dotnet ef migrations add <PREFIX>_<NAME> -c Infrastructure.EF.PostgreSQL.ApplicationDbContext -o EF\PostgreSQL\Migrations -s Infrastructure.csproj
```
where:
- ```PREFIX``` - Added || Updated || Removed etc.,
- ```NAME``` - Unique name for instance UsersTable/NewColumn,
- ```--context (-c)``` - namespace to DbContext,
- ```--project (-p)``` - where EF Core packages are,
- ```--output-dir (-o)``` - where put migration files,
- ```--startup-project (-s)``` - API startup project.

Last thing we have to do it is update the DB by:
```
dotnet ef database update -p Infrastructure.csproj -s Infrastructure.csproj
```

## Remove migration
If you have made migration by mistake or migration is incorrect then run:
```
dotnet ef migrations remove -f -c Infrastructure.EF.PostgreSQL.ApplicationDbContext -p Infrastructure.csproj -s Infrastructure.csproj
```

remove manually files related to your migration and again run DB update:
```
dotnet ef database update [NAME_OF_LAST_GOOD_MIGRATION] -p Infrastructure.csproj -s Infrastructure.csproj
```

> :warning: As default when previous migration was removed then calling DB update command will update DB base on **PREVIOUS** migration

where:
- **[NAME_OF_LAST_GOOD_MIGRATION]** - is optional name of last/previous migration
