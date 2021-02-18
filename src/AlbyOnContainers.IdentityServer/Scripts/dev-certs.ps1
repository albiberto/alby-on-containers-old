cd ..

# The certificate name must match the project assembly name. password is used as a stand-in for a password of your own choosing. 
# If console returns "A valid HTTPS certificate is already present.", a trusted certificate already exists in your store.
dotnet dev-certs https --trust -ep C:\Users\viezzi\.aspnet\https\cert.pfx -p password
dotnet dev-certs https --clean
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "password"
dotnet user-secrets set "ConnectionStrings:DefaultDatabase" "Host=postgres;Database=hulk;Username=postgres;Password=postgres;Port=5432"
dotnet user-secrets remove "Kestrel:Certificates:Default:Password" -p ../AlbyOnContainers.IdentityServer.csproj
dotnet user-secrets remove "ConnectionStrings:DefaultDatabase" -p ../AlbyOnContainers.IdentityServer.csproj