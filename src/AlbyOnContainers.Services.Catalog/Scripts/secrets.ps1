dotnet user-secrets set "ConnectionStrings:DefaultDatabase" "Host=postgres;Database=sherlock-development;Username=postgres;Password=postgres;Port=5432"

dotnet user-secrets remove "ConnectionStrings:DefaultDatabase"