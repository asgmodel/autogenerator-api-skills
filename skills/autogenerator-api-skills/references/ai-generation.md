# AI-native generation and integration

This workflow adapts the maintainer's LAHJA-AI validator-generation approach into agent-native instructions. The agent itself generates and reviews C#; it does not require Gradio, a Hugging Face model, network inference, or an external code-generation service. The WasmAI generator remains responsible for supported mechanical scaffolding. Read [the architecture ontology](architecture.jsonld) to locate dependencies and required checks, then inspect real code rather than treating ontology edges as proof that a component exists.

## Inspect and plan

- Read the target project, resolved package versions, model classes, existing generated layers and custom code, actual startup and registration, and one comparable working resource. Keep the baseline build and git diff in the target workspace.
- Identify requirements for each property, including nullability, requiredness, identity, foreign keys, collections and business constraints. For batch generation, map each requested entity to its own source declaration, namespace and rules. Never pass a shared unrelated model structure as the declaration of every model.
- Determine which parts should come from the upstream generator and which require agent-authored business logic. Preserve existing state IDs, feature keys, API contracts, generated roots, manual logic and authorization.
- Generate or adjust ITModel and DbContext registration only when needed. Run the verified generator command for the selected BPR mode in isolation; review the output. Do not invent APIs or silently change architecture.

## AI-generated condition validators

Follow [validator generation](validator-generation.md) and [ConditionChecker contract](condition-checker.md). Use actual model declarations and domain rules; batch generation must resolve each entity independently. The agent generates business handlers directly, with no external inference service. Do not duplicate the runtime rules in this guide.

## Dependency injection and lifetimes

Verified upstream snapshot: [4ef638f1](https://github.com/asgmodel/WasmAI.AutoGenerator.API/tree/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02). Read actual versions first.

| Marker / component | Observed registration |
| --- | --- |
| ITSingleton | AddAutoSingleton scans classes and registers the concrete type as Singleton. |
| ITTransient | AddAutoTransient scans classes and registers the concrete type as Transient. |
| ITScope | AddAutoScope handles share repositories, IUse service interfaces and ITBuilder concrete types; marker alone does not guarantee registration for arbitrary classes. |
| ITFactoryInjector | AddAutoValidator registers ITFactoryInjector/TFactoryInjector Scoped. |
| IConditionChecker | AddAutoValidator creates a Scoped ConditionChecker and calls BaseConfigValidator.Register(checker, assembly). |
| ValidatorContext | BaseConfigValidator discovers concrete ITValidator classes and instantiates them with the checker; do not register all validators separately as singletons. |

Source: [markers](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Config/Base/Interfaces.cs), [scanner](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Config/Config/Config.cs), [validator registration](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WebApplication1/v1/Validators/Conditions/Config.cs), [reflection registration](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator/Conditions/Config.cs).

- The observed template and generated ConditionChecker constructor fail to assign the readonly injector. If present in the target, repair the local constructor or owning template as appropriate to the requested scope: `_injector = injector ?? throw new ArgumentNullException(nameof(injector));`. Verify `checker.Injector` is non-null and references the current scope. Do not change upstream merely because the snapshot contains a bug.
- Verify AddAutoValidator is actually called once and scans the assembly containing the validators. Keep its one-checker factory; don't introduce circular constructor resolution by asking the container for IConditionChecker while constructing it.
- A Singleton must be thread-safe and must not capture DbContext, a scoped checker/injector, scoped repositories, request data or mutable per-user caches. Use a scoped service for request-bound work. For singleton background work needing a scope, use IServiceScopeFactory to create and dispose one per operation, without retaining its services. Use the host's actual DI pattern and do not add duplicate registrations.
- Enable scope/build validation in tests or development using existing host configuration. Test two independent scopes: same checker within a scope, distinct checkers across scopes, valid injector/context, and no cross-request cached model. Verify actual service descriptors, including concrete/interface resolution; do not call BuildServiceProvider during service registration.

## Procedures and integration

The supplied source covers entity and validator generation, not a concrete Procedure API. A procedure here means a target-host business operation; do not invent a Procedure base class, marker, registration or method. Inspect an existing procedure or application service and its contracts before generating.

- Establish operation input, output, authorization, dependencies and failure semantics; identify the actual layer owning orchestration. Reuse generated BPR repository/builder contracts and the scoped condition checker where business rules require it.
- Validate input and access, retrieve the necessary entities, evaluate prerequisite conditions in dependency order, stop on failure, then execute the requested business operation. Keep transaction ownership in the existing layer; avoid nested saves, duplicate mutations or unrequested external side effects. Return the actual result contract and preserve error details.
- Integrate through the existing service/controller and mapping flow. Do not embed unrelated rules in controllers or bypass authorization. Register through the verified scanner or explicit DI only as appropriate to the actual implementation.
- For missing procedure examples or unsupported library APIs, record the missing contract and implement only what the target proves. Do not claim a verified proprietary procedure API.

## Verification and completion

Follow the ontology's dependsOn graph as a planning guide; select applicable stages only. Checks are evidence obligations, not executed tests. Verify model and output contracts, condition discovery, unique state mapping, positive/negative/null and nested failures, injector assignment, scope isolation, procedure success/failure and no unintended writes, then build and run the relevant existing tests. Compare generated diffs and rerun safe generation where applicable. Mark verified only for actual successful checks, report blocked steps, and keep per-task logs/evidence in the consuming project (not this skills repository). No separate event log or duplicate ontology relations.
