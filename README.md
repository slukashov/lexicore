# LexiCore.

[![NuGet Version](https://img.shields.io/nuget/v/lexicore.svg)](https://www.nuget.org/packages/lexicore)
[![Build Status](https://img.shields.io/github/actions/workflow/status/slukashov/lexicore/nuget-publish.yml?branch=main)](https://github.com/slukashov/lexicore/actions)

**LexiCore** is a lightweight, database-driven localization engine for modern .NET applications. It replaces static `.resx` files with a dynamic Entity Framework Core backend, features an embedded React UI for managing translations on the fly, and uses **DotLiquid** templating for powerful, variable-driven localization strings.

## ✨ Features

* **Drop-in Replacement:** Fully implements `IStringLocalizer` and `IStringLocalizerFactory`.
* **Database Driven:** Backed by Entity Framework Core (SQL Server, Postgres, SQLite, etc.).
* **Embedded UI:** Includes a beautiful, embedded React dashboard (Dark/Light mode, Monaco Editor) to manage keys—no separate deployment required.
* **High Performance:** Aggressively caches translations using `IMemoryCache` to prevent database hits on every request.
* **DotLiquid Templating:** Go beyond `string.Format`. Use Liquid syntax (e.g., `Hello {{ user.name }}!`) for complex, logical string templates.

---

## 📦 Installation

Install the package via the NuGet Package Manager or the .NET CLI:

```bash
dotnet add package LexiCore
```

---

## 🚀 Getting Started

### 1. Register Services
In your `Program.cs`, add LexiCore to your DI container and configure your DbContext.

```csharp
using LexiCore;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add LexiCore
builder.Services.AddLexiCore(options =>
{
    // Define the cultures your app supports
    options.SupportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
        new CultureInfo("fr-FR")
    };

    // Configure the EF Core database provider
    options.ConfigureDbContext = db =>
        db.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
```

### 2. Configure the Pipeline
Set up the database, the API endpoints, and the embedded UI middleware.

```csharp
var app = builder.Build();

// Ensure the LexiCore database tables are created
await app.InitializeLexiCoreDatabaseAsync();

// Map the API endpoints used by the React UI
app.MapLexiCoreApi();

// Serve the embedded React UI
app.UseLexiCoreUI("/lexi-core-ui");

app.Run();
```

---

## 💻 Usage

### Accessing the Dashboard
Run your application and navigate to **`https://localhost:<port>/lexi-core-ui`**.
From here, you can:
* Create and edit translation keys.
* Write DotLiquid templates with a live preview.
* Deprecate/Archive old keys.
* Export and import translations.

### Using Translations in Code
Because LexiCore implements Microsoft's standard localization interfaces, you use it exactly like you normally would.

**In an API Controller or Minimal API:**
```csharp
app.MapGet("/greeting", (IStringLocalizer localizer) =>
{
    // Simple key lookup
    return localizer["welcome_message"].Value;
});
```

**Using DotLiquid Variables:**
If your translation value in the database is `Welcome to the app, {{ name }}! You have {{ points }} points.`, you can pass arguments to it:

```csharp
app.MapGet("/user-greeting", (IStringLocalizer localizer) =>
{
    // LexiCore will automatically parse the anonymous object into the DotLiquid template
    return localizer["user_greeting", new { name = "Alex", points = 150 }].Value;
});
```

---

## 🛠️ Local Development

If you want to clone this repository and build LexiCore from source:

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or newer)
* [Node.js 20+](https://nodejs.org/)

### Building the UI
The React frontend is embedded into the .NET assembly. You **must** build the UI before running the .NET project, or the UI middleware will fail.

```bash
cd src/ClientApp
npm install
npm run build
```

### Running the Tests
LexiCore has a robust test suite covering both the frontend (Vitest) and the backend (xUnit).

**Run Frontend Tests:**
```bash
cd src/ClientApp
npx vitest run
```

**Run Backend Tests:**
```bash
dotnet test
```