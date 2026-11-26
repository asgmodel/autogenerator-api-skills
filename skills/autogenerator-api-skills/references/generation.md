# Core backend generation procedure

This is the default end-to-end workflow for a new or existing ASP.NET Core backend using the actual WasmAI.AutoGenerator.API library. For a narrow repair or validator-only task, inspect the existing host and reuse completed stages; never regenerate the entire backend unnecessarily. Use the target project's conventions and the applicable [architecture dependencies](architecture.jsonld). Do not create duplicate generators, domain models or service layers.

## 1. Inspect the target project

Start in the user's actual workspace, not in the skill repository. Identify the solution, startup .csproj, target framework and SDK, resolved packages/project references, existing entity definitions, DbContext, identity user/role/key types, Program.cs (or equivalent startup), configured database provider and connection-string key, generated API roots and custom layers. Check git status and protect uncommitted changes. If no project exists, determine its required domain and create an appropriate ASP.NET Core host only within the requested scope.

Record the selected startup project directory, candidate entity list, existing architecture and blockers. Do not assume a PaymentDbContext, ApplicationUser, v1 or SQL Server if the target already uses different conventions.

## 2. Select or create ITModel entities

Use the requirements and the EF model to identify domain entities that should participate in generation. Prefer existing classes; add the real `AutoGenerator.ITModel` marker only to selected entities that do not already implement it (directly or through inheritance). Keep any base class and existing interfaces, IDs, nullability, attributes, navigation properties, foreign keys, table mappings and business rules. Do not mark DTOs, view models, arbitrary helpers, identity/system entities, abstract classes or unrelated tables by default. Do not remove pre-existing markers to narrow scope without agreement. Avoid generating duplicate classes; generate only missing entities using the AI agent from actual project requirements.

Maintainer-provided convention for a new translatable category entity:

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using AutoGenerator;

public class CategoryModel : ITModel
{
    [Key]
    public string? Id { get; set; } = $"catm_{Guid.NewGuid():N}";

    [Required]
    [ToTranslation]
    public string? Name { get; set; }

    [ToTranslation]
    public string? Description { get; set; }
}
```

The ID is produced by the property initializer, not ITModel; the interface is a marker. Use this prefix/GUID convention only for new string-key models when appropriate; preserve existing keys. The actual ToTranslation attribute is in AutoGenerator and the coder inspects it when generating DTO/VM property types. It enables the library's multilingual representation/mapping conventions; it does not itself call a translation service. Use it on intentionally translatable fields only and verify generated types, persistence format and language mapping.

Check that the selected entities are discoverable by both EF and the generator. In the reviewed source, AddAutoBuilderCore<TModel> sets AssemblyModels to typeof(TModel).Assembly, where TModel is the supplied TUser from the API wrapper. Setting option.AssemblyModels alone does not override that in this snapshot. The generator may scan every concrete or otherwise eligible ITModel class in the scanned assembly, not just the requested subset. Inventory all discovered types (including existing markers); if unrelated models would be generated, or selected models are in another assembly, stop and resolve actual supported discovery options or isolated compatible host. Do not move entities or change identity types silently.

Model selection precedes dependency setup. Plan/edit marker annotations and new entities, then install/restore required references before compiling. If the entity project differs from the startup project, it must also resolve the real ITModel/ToTranslation assembly via a compatible project/package reference; do not fabricate local marker interfaces.

## 3. Install or reuse the library

Return the installation link and the exact command tailored to the actual .csproj. Reuse a compatible existing reference; do not add duplicate package/project references or blindly upgrade. Check the target framework and package version. The source snapshot targets .NET 8 and declares API version 1.0.0-c. Its dependency references include ConditionChecker 1.0.1; use the resolved dependency and install it explicitly only if the host needs a separate reference.

- [AutoGenerator NuGet](https://www.nuget.org/packages/WasmAI.AutoGenerator.API)
- [AutoGenerator source](https://github.com/asgmodel/WasmAI.AutoGenerator.API)
- [ConditionChecker NuGet](https://www.nuget.org/packages/WasmAI.ConditionChecker)

For a compatible .NET 8 startup project without the reference, the source-documented installation is:

```bash
dotnet add package WasmAI.AutoGenerator.API --version 1.0.0-c
dotnet restore
```

Run from the actual startup project directory, or specify the actual .csproj with dotnet add <project> package. Verify the version is available and compatible before using it; preserve package version management (e.g. Directory.Packages.props). If restore fails, report the diagnostic and do not fake a successful install.

## 4. Configure DbContext and Program.cs

Register the selected models in the existing EF context (DbSet and/or actual model configuration), preserving mappings, provider and identity types. Add missing context and identity setup only for a new host that needs it. The upstream example uses AutoIdentityDataContext<TUser,TRole,TKey> and ITAutoDbContext; use this convention when applicable, not as an automatic replacement for an existing DbContext. The inspected wrapper only requires TContext : DbContext and TUser : class; actual downstream requirements still need verification. Do not create or apply destructive database migrations as part of scaffolding.

The following is the maintainer's SQL Server sample, adapted to actual host types and namespace imports when generating. It assumes a compatible PaymentDbContext and ApplicationUser, an actual configured DefaultConnection, and no duplicate DbContext registration.

```csharp
using System.Reflection;
using AutoGenerator;
using AutoGenerator.Config;
using WasmAI.PaymentProvider.Data;
using WasmAI.PaymentProvider.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddWasmAutoGeneratorApi<PaymentDbContext, ApplicationUser>(new()
    {
        Arags = args,
        NameRootApi = "v1",
        Assembly = Assembly.GetExecutingAssembly(),
        DbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"),
    });

var app = builder.Build();

app.UseWasmAutoGeneratorApi();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

Merge the required registrations into the existing Program.cs; do not replace it wholesale. Keep the upstream spelling Arags and forward args. Use the existing root, or v1 for a new host by default. Verify assembly discovery. With a non-null DbConnectionString the wrapper calls AddDbContext with UseSqlServer; for an already configured context omit this option if the resolved wrapper supports it, retain its provider and avoid duplicate/conflicting registration. Do not embed credentials. Preserve the existing middleware order, authorization, authentication, routing, Swagger, DI and mapper configuration. If the host owns auth, inspect IsAutoAuth and disable the wrapper's automatic auth where needed (both service and middleware options) rather than overriding the host's policy.

Run dotnet build on the configured host before invoking generation. A generation-only mode may continue into app.Run in the sample; use an isolated local workspace and safe development settings, and stop that process normally once generation completes. Do not start production hosts or background jobs, connect to a production database or publish changes simply to generate files.

## 5. Recommend one BPR mode and hand off the command

Recommend a single mode by requirements, existing layer shape and complexity, with one sentence explaining the choice. Do not run all modes successively.

| Mode | Command | Selection guideline |
| --- | --- | --- |
| Builder | `dotnet run -- generate /bpr c:build` | Default for straightforward CRUD and a smaller stack (Builder DTO/repository). |
| Share | `dotnet run -- generate /bpr c:share` | Shared DTO and repository boundaries are needed across services/resources. Includes Builder structures in reviewed folder template. |
| DSO | `dotnet run -- generate /bpr c:dso` | Rich domain/service orchestration or explicit DSO contracts. Reviewed folder template includes Builder, Share and DSO. |
| All | `dotnet run -- generate /bpr c:all` | Advertised complete enterprise stack; only recommend after verifying installed version's actual output. In the reviewed API parser it sets GenType=Dso directly rather than calling SetGenType; do not claim equivalence or full coverage. Prefer c:dso for known DSO structure when appropriate. |

For a non-BPR request the standard command is `dotnet run -- generate`. The double dash explicitly separates application args from dotnet run options; the maintainer's equivalent commands omit it. Default handoff: after preparing the project, give the actual startup directory (or exact --project path), installation status, selected models, chosen mode, rationale and ready-to-run command. Ask the user to run it and supply output if the agent cannot execute, or execute directly when the user requests generation and a safe authorized workspace is available. Do not claim files were generated merely because a command was proposed. When executing on an existing project, preserve the baseline using an isolated checkout/scratch copy, inspect differences and bring only approved-scope changes into the target.

## 6. Review generated files and organize layers

Inspect the actual generated root (e.g. v1) and git diff/status, including new files. Verify selected models, DTO/DSO/VM contracts, base generics, repositories, controllers, service/Builder/Share layers, mappings and validator infrastructure. Do not invent folder names or move generated files arbitrarily; preserve the selected pattern and existing root. Mechanical generation can produce incomplete validator stubs: never treat scaffolding as implemented business rules. Rebuild and identify the first diagnostic if the generator produces invalid code.

## 7. Complete validators and integrate layers

Use [ConditionChecker contract](condition-checker.md) and [validator generation](validator-generation.md) for each applicable model. Keep the real marker interfaces and typed condition handlers, existing Enum values, scoped IConditionChecker and actual DataFilter semantics. Complete meaningful business conditions and related model checks, ensure injector assignment, verify AddAutoValidator exists and is called once in normal runtime when needed. It is not guaranteed by AddWasmAutoGeneratorApi alone. Integrate through the existing BPR layer/controller/service/procedure, not by duplicating validation logic. Use [AI integration](ai-generation.md) for DI lifetimes, business operations and scope safety. For a narrow change reuse the existing generated layers.

## 8. Verify and report

Run dotnet build and available tests, validate DI registration, mapper contracts, routes, condition registration and actual behavior including negative cases. Test database-dependent operations only with an authorized development/test database. Preserve custom logic and public contracts. Return the installation links and actual commands, selected model list, selected BPR pattern, modified/generated paths and verification evidence. If the environment lacks SDK, packages or database access, describe the blocker and give exact next commands; do not mark generation, build or tests as passed without evidence.

## Advanced flags and compatibility

| Intent | Command or parser behavior |
| --- | --- |
| Opt in to merge behavior | `dotnet run -- generate /m /bpr c:build`. In the reviewed parser `/m` and `/m/ai` are recognized only immediately after `generate`. |
| Auth config scaffolding | `dotnet run -- generate /bpr c:build e:auth` calls the config generator if the auth settings file is missing. `e:nauth` calls it unconditionally. Inspect its writes and use a disposable checkout for a trial. |

In the reviewed API parser, `c:all` is removed and assigns `GenType = Dso` directly. It does not call `SetGenType`. Verify the resulting files before describing the coverage of that mode. The parser contains no `e:payment` handling. Unconsumed positional tokens in the reviewed core generation branch can be treated as output root names, so use only flags supported by the installed build.

The reviewed Coder saves new files and can overwrite existing ones when the selected generator sets `IsEditFile`. For other existing files, merge mode can rewrite content and ordinary mode reads it back. AI merge applies only to certain generator types in the shown code. Inspect its data flow and keep it off unless expressly requested and appropriate for the source being processed.

Source references: [API configuration](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator/Config/Config.cs), [core discovery and registration](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Config/Base/InstallServiceCollection.cs), [BPR folder templates](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Coder/Config/Config.cs), [marker interfaces](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Config/Base/Interfaces.cs), [translation attribute](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Config/Base/Attributes.cs). Verify against the installed package: the source snapshot is not proof of runtime binary compatibility.
