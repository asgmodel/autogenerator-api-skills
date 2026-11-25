# Generation and integration

## Registration

Use the host's real types. The example uses `using AutoGenerator;` and `using System.Reflection;` and wires the generator before building the app:

```csharp
builder.Services.AddWasmAutoGeneratorApi<PaymentDbContext, ApplicationUser>(new()
{
    Arags = args,
    NameRootApi = "v1",
    Assembly = Assembly.GetExecutingAssembly(),
    DbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"),
});
```

Use `Arags` with its upstream spelling and pass the startup argument array, including when it is empty. The wrapper registers EF Core with `UseSqlServer` when the connection string is non-null. If the host already registers its context, check for duplicate or conflicting provider settings. Register controllers and map them in the host. The sample calls `app.UseWasmAutoGeneratorApi()` before `app.MapControllers()`. The shown auth wrapper checks `IsAutoAuth` and the presence of `autoauthapisettings.json` for both service and middleware setup. For an API using its own auth setup, inspect its middleware and policy configuration directly.

## CLI commands

From the startup project's directory, after reviewing the resolved implementation:

| Intent | Command or parser behavior |
| --- | --- |
| Standard scaffolding | `dotnet run -- generate` uses `NameRootApi` when no positional root is passed. |
| BPR Builder DTO | `dotnet run -- generate /bpr c:build` |
| BPR Share DTO | `dotnet run -- generate /bpr c:share` |
| BPR DSO | `dotnet run -- generate /bpr c:dso` |
| Opt in to merge behavior | `dotnet run -- generate /m /bpr c:build`. In the reviewed parser `/m` and `/m/ai` are recognized only immediately after `generate`. |
| Auth config scaffolding | `dotnet run -- generate /bpr c:build e:auth` calls the config generator if the auth settings file is missing. `e:nauth` calls it unconditionally. Inspect its writes and use a disposable checkout for a trial. |

In the reviewed API parser, `c:all` is removed and assigns `GenType = Dso` directly. It does not call `SetGenType`. Verify the resulting files before describing the coverage of that mode. The parser contains no `e:payment` handling. Unconsumed positional tokens in the reviewed core generation branch can be treated as output root names, so use only flags supported by the installed build.

The reviewed Coder saves new files and can overwrite existing ones when the selected generator sets `IsEditFile`. For other existing files, merge mode can rewrite content and ordinary mode reads it back. AI merge applies only to certain generator types in the shown code. Inspect its data flow and keep it off unless expressly requested and appropriate for the source being processed.

## Check results

1. Check the SDK (`dotnet --info`), package references, and the host baseline build. Supply any needed development configuration outside tracked files. Confirm the CLI working directory since generation and auth settings involve filesystem paths.
2. Run a single selected generator command in the isolated workspace. The example host continues into `app.Run()`; if the code was generated and the process stays up, stop that local process normally before rebuilding.
3. Use `git status --short` and `git diff` to inspect output, including new files. Compare route attributes, inherited actions, DTO mappings, model registration, and DI against the closest existing resource.
4. Rebuild with `dotnet build`. Run existing relevant tests or smoke-test a concrete local endpoint using development data. Record which checks actually ran.
