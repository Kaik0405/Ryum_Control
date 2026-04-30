# GestionApp — Distribution Management System

Desktop application built in **C# + WPF (.NET 8)** focused on the day‑to‑day operation of a distribution business: inventory, combos, deliveries, costs, and movements. It prioritizes **clean architecture**, **explicit business rules**, and a **consistent UX**.

## 🚧 Project status

**Actively in development.** The system is continuously evolving and improvements are added progressively. This repository reflects ongoing work.

## ✨ Key features

- 📦 **Inventory** with stock control and movement traceability.
- 🧩 **Combos/groupings** and product relationships.
- 🚚 **Deliveries** with priorities and date‑based alerts.
- 💾 **Local persistence** with SQLite + EF Core.
- 🧭 **Modular navigation** and reactive UI through MVVM.

## 🧱 Architecture & design

- 🧩 **MVVM** with strict separation of View ↔ ViewModel ↔ Model.
- 🧰 **Dependency Injection** for services, navigation, and ViewModels.
- 🧠 **Domain services** encapsulating business rules.
- 🗂️ **Soft delete** to preserve traceability.

## 🛠️ Environment requirements

- 🪟 **Windows 10/11** (WPF is Windows‑only).
- 🧬 **.NET SDK 8.0** or later.

## ▶️ Installation & run

```bash
dotnet restore
dotnet build
dotnet run
```

If the process is blocking compilation:

```bash
taskkill /F /IM GestionApp.exe
dotnet build
```

## 🗄️ Database

Local SQLite database created automatically at startup using `EnsureCreated()`:

```
%APPDATA%\GestionApp\gestion.db
```

> **Technical note:** if you change models or relationships, delete `gestion.db` and run again. Migrations are not used in this workflow.

## 🗂️ Repository structure

```
GestionApp/
├── Data/              # DbContext and EF Core configuration
├── Helpers/           # Commands, converters, and MVVM utilities
├── Models/            # Domain entities
├── Services/          # Business and navigation services
├── ViewModels/        # Presentation logic
├── Views/             # WPF views (XAML)
├── App.xaml           # Resources and DataTemplates
├── MainWindow.xaml    # Main shell
└── GestionApp.csproj
```

## 📦 Core packages

| Package | Purpose |
|---|---|
| Microsoft.EntityFrameworkCore.Sqlite | SQLite persistence |
| Microsoft.EntityFrameworkCore.Tools | EF Core tooling |
| Microsoft.Extensions.DependencyInjection | DI container |

## 🧪 Developer quick guide

- Keep logic in **services** and ViewModels; views should be “thin”.
- Forms use `string` for numeric fields to avoid decimal format issues.
- Critical operations register movements for auditability.

## 📚 Documentation

- Extended technical documentation in `docs/DOCUMENTACION.md`.

## 🤝 Contributing

If you want to collaborate, open an issue describing the proposed change. PRs should include a clear technical description and minimal manual verification steps.

---

*Built with C# and WPF (.NET 8).* 
