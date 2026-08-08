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
- `EmailSettings__UseSsl` (`true` to require encryption; port `465` uses
  implicit TLS and ports such as `587`/`2525` use STARTTLS)
- `EmailSettings__Username` (the SMTP login email; it is also used as the
  sender and care-team notification recipient)
- `EmailSettings__Password`
- `EmailSettings__FromName`

Do not commit SMTP passwords to `appsettings.json` or source control. The sender
and care-team recipient are automatically taken from the authenticated SMTP
username.

When a request is submitted, the application authenticates with the SMTP server,
sends the care-team notification first, and then sends a separate confirmation
to the visitor's submitted email address. A successful form response means the
SMTP server accepted both messages; inbox placement still depends on the email
provider and spam filtering.