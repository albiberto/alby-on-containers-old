#dotnet ef --startup-project .. migrations add InitialMigration -c ApplicationDbContext -o Infrastructure/Migrations/Identity
#dotnet ef --startup-project .. migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Infrastructure/Migrations/IdentityServer/PersistedGrantDb
#dotnet ef --startup-project .. migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Infrastructure/Migrations/IdentityServer/ConfigurationDb

dotnet ef --startup-project .. database update --context ApplicationDbContext
dotnet ef --startup-project .. database update --context PersistedGrantDbContext
dotnet ef --startup-project .. database update --context ConfigurationDbContext