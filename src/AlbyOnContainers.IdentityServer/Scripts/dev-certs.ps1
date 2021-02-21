# The certificate name must match the project assembly name. password is used as a stand-in for a password of your own choosing. 
# If console returns "A valid HTTPS certificate is already present.", a trusted certificate already exists in your store.
dotnet dev-certs https --trust -ep C:\Users\viezzi\.aspnet\https\cert.pfx -p password
dotnet dev-certs https --clean
