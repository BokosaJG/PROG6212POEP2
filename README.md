# CMCSGUI

## Overview

CMCSGUI is a **.NET 9.0 ASP.NET Core MVC application** that provides a graphical user interface (GUI) for managing claims data. It follows a standard MVC pattern with controllers, models, and views, making it easy to extend and maintain.

---

## Features

* ASP.NET Core MVC architecture  
* Claims management module (`ClaimsController`, `Claim` model)  
* Configurable settings via `appsettings.json`  
* Error handling through `ErrorViewModel`  
* Ready-to-run .NET 9.0 executable build  

---

## Project Structure

```
CMCSGUI/
├── Controllers/          # MVC controllers (e.g., ClaimsController, HomeController)
├── Models/               # Data models (e.g., Claim, ErrorViewModel)
├── Views/                # Razor views (if implemented)
├── Tests/                # Unit tests for controllers, models, and services
│   ├── ClaimsControllerTests.cs
│   └── CMCSGUI.Tests.csproj
├── Program.cs            # Application entry point
├── appsettings.json      # Configuration file
├── CMCSGUI.csproj        # Project file
├── CMCSGUI.sln           # Solution file
└── bin/                  # Build outputs (executables, DLLs)
```

---

## Requirements

* **.NET 9.0 SDK** (or higher)  
* Visual Studio 2022+ or VS Code with C# extension  
* Windows/Linux/macOS supported  

---

## Getting Started

1. **Clone or extract** the project files.

   ```bash
   git clone <repo-url>
   cd CMCSGUI
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Build the project**

   ```bash
   dotnet build
   ```

4. **Run the application**

   ```bash
   dotnet run
   ```

   The application will start and be available at `http://localhost:5000` (or as configured).

---

## Configuration

* Update `appsettings.json` and `appsettings.Development.json` with your environment-specific settings.  
* Supports standard ASP.NET Core configuration for logging, database connections, etc.  

---

## Unit Tests

The project includes a dedicated **unit test suite** to ensure stability and reliability of key components such as controllers, models, and services.

### Test Frameworks Used
* **xUnit** – Core testing framework  
* **Moq** – For mocking dependencies and services  
* **Microsoft.AspNetCore.Mvc.Testing** – For controller and endpoint testing  

### Running Tests

1. Navigate to the test project directory:

   ```bash
   cd Tests
   ```

2. Run all unit tests:

   ```bash
   dotnet test
   ```

3. To see detailed test results (with code coverage, if enabled):

   ```bash
   dotnet test --logger "console;verbosity=detailed"
   ```

### Example Test Classes
* `ClaimsControllerTests.cs` – Verifies claim creation, retrieval, update, and deletion operations.   
* `HomeControllerTests.cs` – Tests default routes and view rendering.  

### Test Output
After successful test execution, results are displayed in the console. You can also integrate with CI/CD pipelines (e.g., GitHub Actions, Azure DevOps) for automated testing.

---

## Development Notes

* Controllers are located in `Controllers/`  
* Models are located in `Models/`  
* Razor views (if present) should be in `Views/`  
* Unit tests are stored in `Tests/`  
* Debug builds output to `bin/Debug/net9.0/`  

---

## License

This project currently does not specify a license. Please add one if you intend to distribute it.

https://youtu.be/kYKF-LuY5hQ?si=8QJhYIpzGucmqlR_

