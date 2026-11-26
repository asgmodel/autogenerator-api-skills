---
name: autogenerator-api-skills
description: Generate, integrate, and repair ASP.NET Core backends using WasmAI.AutoGenerator.API and WasmAI.ConditionChecker. Use for entity/ITModel scaffolding, BPR Builder/Share/DSO, condition validators, typed DataFilter, RegisterConditionValidator, IConditionChecker, nested business conditions, repositories, controllers, DI, EF Core, or safe regeneration.
---

# AutoGenerator API Skills

Use the actual WasmAI libraries and host architecture. The coding agent authors business rules; AutoGenerator handles supported scaffolding. Do not replace either library or require external AI inference.

## Route the task

Start by reading [architecture.jsonld](references/architecture.jsonld). Its `UsageExample` nodes map ready-to-use requests to relevant architecture targets. Match the actual intent by meaning, not exact wording. Always inspect the target first; reuse completed stages and execute only applicable dependencies. The ontology is a reference, not a separately executable tool.

- Model, CRUD, BPR, DTO, DSO, repository or controller: [generation](references/generation.md) and [repository landmarks](references/repository.md).
- Validator, condition, checker, feature value or nested rule: [ConditionChecker contract](references/condition-checker.md), then [validator generation](references/validator-generation.md). Use its reusable template and examples.
- Host registration, DI, procedures or integration: [AI integration](references/ai-generation.md).
- For multi-stage tasks use [architecture ontology](references/architecture.jsonld) to order only applicable dependencies. [Examples](references/examples.md) indexes scenarios.

## Mandatory core procedure

For new backend generation or extending the full generated stack, follow [Core backend generation procedure](references/generation.md) in order: inspect target → select existing or create missing ITModel entities → install/reuse compatible library → configure DbContext and Program.cs → recommend one BPR mode and hand off its exact command (or execute when requested and safe) → review generated files → complete validators → integrate layers → build/test. Do not skip the model selection and host preparation gates. For narrow fixes, inspect and reuse completed stages without unnecessary regeneration.

## Workflow

1. Inspect the actual target .csproj, resolved packages, Program.cs, entity and DbContext, generated layer and comparable resource. Check source versus package versions. Preserve namespaces, custom behavior, enum IDs, feature contracts and user changes.
2. Follow the core procedure for scaffolding. Select or create only relevant entities implementing the real ITModel marker; verify all assembly-discovered models. Reuse or install the compatible package, prepare actual host configuration, then recommend a single BPR command. Prefer user handoff unless execution is requested and feasible. Use an isolated workspace for execution and review the diff. Use the agent for custom validators and integration.
3. For conditions use exactly one enum owner, typed attributed handlers, explicit dynamic/fixed values, correct failure Message, strict success and scoped checker/lookup. Read the runtime contract before producing code. Do not use fail-open aggregate APIs for mandatory gates; use ordered single-state or composite checks.
4. Integrate only through verified host contracts. Respect authorization, DTOs, mapper, routes and scoped services; never capture DbContext or checker in a singleton. Verify injector assignment and registration. Do not invent proprietary APIs.
5. Run target build and applicable tests (dispatch, missing state/value, nested failures, scope, persistence/authorization); report actual commands, changes, results and blockers. Keep execution evidence in the target project, not the skill repository. No unverified completion claims.

For regeneration or failure, inspect actual generated files, first compiler diagnostic, version/assembly scan and DI before changing the library. Preserve custom code and use explicit user-approved scope for upstream changes.
