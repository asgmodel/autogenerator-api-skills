# Repository landmarks

Source reviewed at commit [4ef638f1](https://github.com/asgmodel/WasmAI.AutoGenerator.API/tree/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02). Treat these details as a snapshot and confirm behavior against the consuming project's resolved version.

| Area | Inspect |
| --- | --- |
| Package and framework | `WasmAI.AutoGenerator/WasmAI.AutoGenerator.csproj` declares `WasmAI.AutoGenerator.API` version `1.0.0-c` targeting `net8.0`. The local API project consumes the Coder and Patterns as NuGet dependencies. |
| Startup, flags, SQL provider | `WasmAI.AutoGenerator/Config/Config.cs` defines `AddWasmAutoGeneratorApi`, `AddAutoBuilderApiCore`, `UseWasmAutoGeneratorApi`, `cmdargs`, and `AddAutoDbContext`. |
| Core registration | `WasmAI.AutoGenerator.Config/Base/InstallServiceCollection.cs` selects generation or DI registration. `Base/Interfaces.cs` defines marker interfaces. `Config/Config.cs` implements assembly scanning. |
| Folder layout | `WasmAI.AutoGenerator.Config/ApiFolder/ApiFolderGenerator.cs` and `FolderStructureReader.cs` determine paths and load the structure. |
| Generated code and BPR | `WasmAI.AutoGenerator.Coder/Code/` contains generators. `Config/Config.cs` handles BPR configuration. `TM/` contains templates. `WasmAI.AutoGenerator.Patterns/` contains base classes. |
| Auth and Swagger | `WasmAI.AutoGenerator/Auth/` contains auth config, service registration, and middleware. |
| Working example in source | `WebApplication1/Program.cs`, `Data/PaymentDbContext.cs`, `Models/Models.cs`, and checked-in output under `WebApplication1/v1/`. |

## Trace a concrete resource

Start with `Product : ITModel` in `WebApplication1/Models/Models.cs`, then follow `v1/Controllers/Api/ProductController.cs` to its concrete `ProductBuilderRepository`. Check the corresponding files under `v1/Repositories/Builder/Product/`, `v1/DyModels/Dto/Build/`, `v1/DyModels/VM/Product/`, and `v1/Validators/Validators/`. The controller route attribute is `api/v1/[controller]`. Read its base layer to determine the action templates and HTTP verbs.

## Compatibility details

- In the reviewed Config source, core registration sets `ApiFolderInfo.AssemblyModels` from the generic user type's assembly and sets `AssemblyShare` from `option.Assembly`. The API wrapper forwards `AssemblyModels`, but the shown core method uses `typeof(TModel).Assembly`. If a model from another assembly is absent, trace the generator's actual type selection before changing options.
- The programmatic signature is `ApiFolderGenerator.Build(ApiFolderBuildOptions)` in the reviewed source. `ProjectPath` is expected to be a directory. This implementation constructs the JSON path with `Path.Combine(projectPath, projectPath)`, and the reader uses a built-in structure when the resulting file is missing. Inspect the resolved source and effective path before relying on a custom `folderStructure.json`.
- The example's generated folders reflect Builder mode. Their presence in source establishes the shape of the example, with runtime behavior dependent on configuration and resolved packages.
