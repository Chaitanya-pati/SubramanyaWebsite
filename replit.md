# Sri Subramanya Home Healthcare

## Project overview

This is an ASP.NET Core MVC application targeting .NET 8 for Sri Subramanya Home
Healthcare in Belagavi. The home-care request form sends the request to the configured
care-team email address and sends a confirmation email to the visitor.

## Running on Replit

The `Start application` workflow runs the site on port 5000 with the .NET 8 SDK.

## Email configuration

Set these values in the app's environment-specific configuration or Replit
Secrets/environment variables before using the form in production:

- `EmailSettings__SmtpHost`
- `EmailSettings__SmtpPort` (`465` for implicit SSL, or `587` for STARTTLS)
- `EmailSettings__UseSsl` (`true` for SSL/TLS)
- `EmailSettings__Username`
- `EmailSettings__Password`
- `EmailSettings__FromEmail`
- `EmailSettings__FromName`
- `EmailSettings__Emails` (one or more recipient addresses separated by commas)

Do not commit SMTP passwords to `appsettings.json` or source control. The sender
address should normally be the authenticated SMTP account or an address authorized
by that account.