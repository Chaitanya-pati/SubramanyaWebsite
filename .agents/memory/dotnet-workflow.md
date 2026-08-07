---
name: Imported .NET workflows
description: Environment-specific SDK selection for imported ASP.NET Core projects.
---

Imported ASP.NET Core projects targeting .NET 8 can encounter a default shell/workflow
that resolves .NET 7 even when .NET 8 is installed. The managed workflow must select an
available .NET 8 SDK explicitly while preserving the project's target framework.

**Why:** The imported project's default module configuration exposed a .NET 7 `dotnet`
first, which prevented restore and startup despite a working .NET 8 SDK being available.

**How to apply:** When a .NET 8 workflow reports `NETSDK1045`, locate the available
.NET 8 SDK and configure the managed workflow command's `PATH` before changing project
targets or restructuring the application.