dotnet user-secrets set "ConnectionStrings:DefaultDatabase" "Host=postgres;Database=pollon-development;Username=postgres;Password=postgres;Port=5432"

dotnet user-secrets remove "ConnectionStrings:DefaultDatabase"