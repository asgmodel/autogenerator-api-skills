---
name: autogenerator-api-skills
description: Generate, integrate, and repair ASP.NET Core backends using WasmAI.AutoGenerator.API and WasmAI.ConditionChecker. Use for entity/ITModel scaffolding, BPR Builder/Share/DSO, condition validators, typed DataFilter, RegisterConditionValidator, IConditionChecker, nested business conditions, repositories, controllers, DI, EF Core, or safe regeneration.
---

# AutoGenerator API Skills

Use the actual WasmAI libraries and host architecture. The coding agent authors business rules; AutoGenerator handles supported scaffolding. Do not replace either library or require external AI inference.

## Route the task

- Model, CRUD, BPR, DTO, DSO, repository or controller: [generation](references/generation.md) and [repository landmarks](references/repository.md).
- Validator, condition, checker, feature value or nested rule: [ConditionChecker contract](references/condition-checker.md), then [validator generation](references/validator-generation.md). Use its reusable template and examples.
- Host registration, DI, procedures or integration: [AI integration](references/ai-generation.md).
- For multi-stage tasks use [architecture ontology](references/architecture.jsonld) to order only applicable dependencies. [Examples](references/examples.md) indexes scenarios.

## Workflow

1. Inspect the actual target .csproj, resolved packages, Program.cs, entity and DbContext, generated layer and comparable resource. Check source versus package versions. Preserve namespaces, custom behavior, enum IDs, feature contracts and user changes.
2. Plan outputs and existing dependencies; define domain rules from requirements, not blanket property checks. Use the generator's verified CLI mode in an isolated workspace for scaffolding. Use the agent for custom validators and integration; only modify requested files and inspect diff.
3. For conditions use exactly one enum owner, typed attributed handlers, explicit dynamic/fixed values, correct failure Message, strict success and scoped checker/lookup. Read the runtime contract before producing code. Do not use fail-open aggregate APIs for mandatory gates; use ordered single-state or composite checks.
4. Integrate only through verified host contracts. Respect authorization, DTOs, mapper, routes and scoped services; never capture DbContext or checker in a singleton. Verify injector assignment and registration. Do not invent proprietary APIs.
5. Run target build and applicable tests (dispatch, missing state/value, nested failures, scope, persistence/authorization); report actual commands, changes, results and blockers. Keep execution evidence in the target project, not the skill repository. No unverified completion claims.

For regeneration or failure, inspect actual generated files, first compiler diagnostic, version/assembly scan and DI before changing the library. Preserve custom code and use explicit user-approved scope for upstream changes.
