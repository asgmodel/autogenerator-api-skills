# Usage scenarios and example roadmap

These are agent prompts, not executed examples. Use the consuming host's actual types, installed package and source before selecting any command. The original reference snapshot is linked in [repository.md](repository.md).

| Section | Starter request | Evidence needed for a complete example |
| --- | --- | --- |
| Host setup | Inspect my ASP.NET Core host and integrate the generator without replacing its auth or DbContext. | `.csproj`, `Program.cs`, context and identity type |
| Model discovery | Inspect my `Product` entity and determine whether the generator discovers it via `ITModel`. | Entity, key, relationships and scanned assembly |
| Builder DTO | Generate Builder CRUD for `Product` in an isolated checkout. | Host, generated DTOs, repository, controller, validator and build |
| Share DTO | Generate Share DTO output for the selected resource and inspect the diff. | Resolved generator version and generated files |
| DSO | Generate DSO output for the selected resource and verify its contracts. | Resolved generator version and generated files |
| Controllers and validation | Trace the create/read/update/delete routes and validate request mappings. | Actual generated routes, inherited actions and validators |
| Condition Validators | Implement a typed, attribute-registered business condition and a nested checker call. | [Maintainer example](condition-validators.md); host types, resolved package, registration and tests |
| DI and mapping | Diagnose a missing repository registration or AutoMapper map. | First diagnostic, constructor and registration code |
| SQL Server and auth | Verify development database registration and Swagger/JWT without exposing secrets. | Redacted configuration, middleware and test environment |
| Regeneration and QA | Regenerate without losing custom code; show changes and run applicable checks. | Baseline, diff, build and available tests |

## Adding a verified example

For each section, record the source version, required host files, exact command or API, expected output paths, actual diff and test results. Keep secrets and real customer data outside the repository. Add only verified commands; do not infer unsupported generator methods or flags. A maintainer-supplied Condition Validator example is now available; its runtime checks and detailed examples of other generator modes and procedures remain pending.
