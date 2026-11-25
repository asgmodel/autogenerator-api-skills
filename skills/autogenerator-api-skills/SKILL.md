---
name: autogenerator-api-skills
description: Develop, integrate, and troubleshoot ASP.NET Core backends that use asgmodel/WasmAI.AutoGenerator.API. Use for .NET 8 code generation with this package, BPR or CRUD scaffolding from EF Core ITModel entities, Builder/Share DTO or DSO layers, generated controllers, attribute-based Condition Validators, IConditionChecker, DI, AutoMapper, SQL Server, or Swagger/JWT setup in a WasmAI AutoGenerator host. استخدمها لتوليد أو تعديل واجهات وطبقات BPR وتشخيص مشاكل تشغيل مكتبة WasmAI AutoGenerator.
---

# AutoGenerator API Skills

Powered by WasmAI.AutoGenerator.API. This skill guides an agent through the actual generator; it is not a replacement generator or a standalone backend.

## Section map

- [Generation and integration](references/generation.md): host registration, Builder/Share/DSO modes, CLI flags and verification.
- [Repository landmarks](references/repository.md): source paths, compatibility and resource tracing.
- [Condition Validators](references/condition-validators.md): ValidatorContext, state enums, RegisterConditionValidator, DataFilter, ConditionResult and nested conditions.
- [Usage scenarios](references/examples.md): starter prompts and the evidence needed for future section-specific examples.

## Locate the active implementation

1. Open the consuming project's `.csproj`, `Program.cs`, model and DbContext, and any existing generated root. Identify the startup project, package versions or project references, DI and API route conventions, and whether the work concerns the host, generated output, or the generator itself.
2. Read [repository landmarks and compatibility notes](references/repository.md) for upstream paths and known differences between the README and source. Verify the matching package or source version before using a flag or method. The host may consume released dependencies even when their source projects exist in the checkout.
3. Decide the scope from the request. For a new resource, follow a comparable local resource across its model, DbContext, request and response types, mappings, repository or service, validator, and controller. For an existing resource, trace the affected behavior and preserve public routes, DTO contracts, and custom logic except where the request changes them. Resolve evidence from the host before changing a shared generator template.

## Generate or extend an API

1. Confirm entity discovery, including the `ITModel` marker, key, relationships, DbContext model, and the assembly the active generator actually scans. Preserve the host's identity types and DI lifetimes. Bind SQL Server settings through the host's existing configuration.
2. Use [generation and integration](references/generation.md) to choose the CLI mode, registration points, and output root. Generate in a disposable checkout or a dedicated scratch host when the active output is already customized. Check changed and new files after generation. Bring in only the requested output, preserving manual code and keeping EF migrations separate from scaffolding.
3. Verify the complete request path. Compare generated request/response types and AutoMapper profiles with the actual entity and base generics. Trace DI registration for the concrete builder or service injected into the controller. Check routing, validation, and authorization against the host's conventions. Treat provider integrations such as Stripe and PayPal as separate application logic unless the installed implementation explicitly supplies them.
4. Build the startup project after generation. Run existing relevant tests where available. If a development host and test database are available, verify a representative generated route and the requested behavior. For a write path, check persistence and the expected status using disposable data. Report the command, generated or modified files, and verification limits. If restore, build, or execution is blocked, give the exact failing step.

## Implement condition-based business validation

For tasks involving `AutoGenerator.Conditions`, read [Condition Validators](references/condition-validators.md) first. Inspect the host's existing validator, model, attribute discovery and checker contracts. Define typed state handlers and feature keys; use nested checker calls only where the requested business rule depends on another condition. Preserve failure propagation, verify result types and test missing data. Do not assume all validators are generated or that the example's application-specific model types exist in another project.

## Diagnose failures

- **No generated files or unexpected directories** — Verify the working directory, `Arags = args`, parsed argument order, BPR mode, inspected assembly, effective root, and the actual folder-loading code. Confirm filesystem changes even if the generator reports success.
- **Compilation, DI, or mapping failure** — Start from the first diagnostic. Compare generated base generics and namespaces with the resolved packages. Follow the constructor dependency through its service marker, assembly scan or explicit registration, and scoped DbContext. Check the specific map used by the action.
- **404, 401/403, or missing Swagger** — Read the generated route and inherited actions. Check `MapControllers()`, environment, Swagger setup, and the actual authentication and authorization wiring. Distinguish migration and SQL connectivity problems from routing and DI issues.
- **Regeneration changes custom code** — Inspect the write and merge paths for the affected generator. Compare the diff against the original and retain custom endpoints, validation, security behavior, and mappings.
