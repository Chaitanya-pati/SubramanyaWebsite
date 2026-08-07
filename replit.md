# SubramanyaWebsite

## Project overview

This is an ASP.NET Core MVC application targeting .NET 8. The main site is served by
`SubramanyaWebsite/Program.cs` and uses the default Home controller and Razor views.

## Running on Replit

The Replit workflow runs:

```bash
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://0.0.0.0:5000 dotnet run --project SubramanyaWebsite/SubramanyaWebsite.csproj --no-launch-profile
```

The application is available through the Replit preview on port 5000.