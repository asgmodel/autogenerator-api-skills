# Validator generation — canonical workflow

Use this reference whenever asked to generate, repair or integrate a condition validator. It supplements the [original ModelAI example](condition-validators.md) and is the canonical generation workflow. No external AI inference service is required.

## 1. Read the actual host contract

The upstream snapshot [4ef638f1](https://github.com/asgmodel/WasmAI.AutoGenerator.API/tree/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02) shows:

- [BaseValidatorContext](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator/Conditions/BaseValidatorContext.cs) delegates to WasmAI.ConditionChecker. The dependency's internals must be checked from the resolved version; the wrapper alone does not prove binding behavior.
- [ValidatorContext](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WebApplication1/v1/Validators/Conditions/ValidatorContext.cs) retrieves models through `_injector.Context.Set<TContext>().FindAsync(id)`.
- [Registration](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WebApplication1/v1/Validators/Conditions/Config.cs) constructs a scoped checker and registers validators from the executing assembly. [BaseConfigValidator](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator/Conditions/Config.cs) instantiates concrete ITValidator types using the checker. Preserve that constructor contract.
- [TmValidator](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Coder/TM/TmValidator.cs) documents attribute discovery, typed DataFilter and nested checks; [TmController](https://github.com/asgmodel/WasmAI.AutoGenerator.API/blob/4ef638f1d1c6209d633f76e1d5bc6b8e309dff02/WasmAI.AutoGenerator.Coder/TM/TmController.cs) documents checker call forms.

Inspect actual namespaces, package versions, enum ID policy, attribute usage, DataFilter constructors and generic bindings, model lookup and success/failure result factories. Do not infer undocumented overloads or attribute Value semantics.

## 2. Generate a plan per entity

Input: actual C# entity declaration, EF key/relationships, existing validators, domain rules and target namespace. For batch generation use a separate declaration and rule set for every entity.

For each meaningful state record: state, property or operation, filter value type, predicate, failure message, successful payload, and any nested state dependency. Account for every property but do not create meaningless validations or reject valid defaults. Preserve existing enum numeric values and stable external feature keys. New state IDs must follow the host's actual collision policy. Determine whether existence means a non-null supplied model or a persisted database lookup: these are different contracts.

## 3. Apply the C# template

Use [ValidatorContext.cs.txt](../assets/templates/ValidatorContext.cs.txt) as a non-compilable substitution template. Replace every token, resolve imports from the host and use its existing names. Remove feature-key machinery if not required. Do not add a second validator for a state that already exists.

- Model states use an enum; methods are private and return Task<ConditionResult>. Register a state exactly once, normally through RegisterConditionValidator attributes. Keep InitializeConditions empty when attribute discovery is supported. Do not mix manual registration for the same state.
- A non-async handler returns a Task from ToSuccessAsync/ToFailureAsync. An async handler uses await and returns a ConditionResult (or awaits the Task factory). Nullable success must be compared with true.
- Share is the model, Value is the expected value, Id is lookup/request ID, Name is optional context. Do not treat Id or Name as authenticated identity. Reject missing Share explicitly for model-dependent conditions; do not accidentally accept null == null.
- Attribute Value can denote a feature key or fixed value in the original example. Its dispatch semantics depend on the resolved framework. If using several attributes on one handler, test each state and expected value individually; otherwise use separate handlers that share a helper.
- Prefer base GetModel for entity lookup. If overriding, use the actual key type and preserve base behavior, tenant restrictions and scoped context; no static entity cache or unnecessary GetModel override.
- Nested checks use _checker.CheckAndResultAsync with a proven state/context. Fail closed on missing ID, unsuccessful result or incompatible payload. Do not create cyclic dependencies. Compose pure rules through helpers; invoke cross-entity business rules through the checker.
- Do not query unrelated data or mutate persistent state in simple validators. Treat validation as distinct from authorization and writes. Enforce real authenticated user/tenant ownership via existing policy or trusted context; equality between two client-supplied IDs is not authorization. Handle secrets without returning tokens or credentials as result data.
- Correct missing ConditionChecker injector assignment in the target if present, verify AddAutoValidator scans the correct assembly, and preserve scoped checker/injector/DbContext. See [AI integration](ai-generation.md).

## 4. Concrete example: Product and Category

The example files are educational C# snippets, not an independent project or generated production artifacts. Adapt namespaces/imports and keys against the installed framework before compilation.

| File | Purpose |
| --- | --- |
| [Models.cs.txt](../assets/examples/validators/Models.cs.txt) | Two example ITModel entities |
| [CategoryValidator.cs.txt](../assets/examples/validators/CategoryValidator.cs.txt) | Existence and active state |
| [ProductValidator.cs.txt](../assets/examples/validators/ProductValidator.cs.txt) | Required/optional fields, bool and decimal checks, collections, value matching, fixed kinds, nested and composite conditions |

The example intentionally uses a nonnegative price and optional HTTP(S) URL as assumed business rules, not universal constraints. For collection validation the host filter type must be checked; the Tags rule does not consume Value. The examples return the model payload for model conditions; adapt if the host requires another contract. If externally exposing a result, map to a safe DTO.

### Consumer invocation

These forms are documented by the upstream TmController prompt; verify the installed implementation, especially string-ID lookup and typed conversion from the non-generic filter. Resolve DataFilter from the host, not a newly declared duplicate.

```csharp
// Check a boolean rule using the model ID.
bool active = await _checker.CheckAsync(CategoryValidatorStates.IsActive, categoryId);

// Retrieve the result and its reason.
var result = await _checker.CheckAndResultAsync(
    ProductValidatorStates.IsFound, new DataFilter { Id = productId });
if (result.Success == true && result.Result is Product product)
{
    // Use the validated payload internally.
}

// Compare an explicit caller-supplied value; false is valid when it matches.
var match = await _checker.CheckAndResultAsync(
    ProductValidatorStates.MatchesActive,
    new DataFilter { Id = productId, Value = false });

// Invoke a composite business condition before a write.
var allowed = await _checker.CheckAndResultAsync(
    ProductValidatorStates.CanPublish, productId);
if (allowed.Success != true)
{
    // Map the failure using the host's established response contract.
    // Stop: do not write or execute the protected operation.
}
```

For an authenticated operation, authorization must also run using trusted identity and tenant scope; CanPublish is only a business rule. Apply the operation after both checks succeed, and protect against concurrent state changes where required.

### Multi-attribute variant (conditional)

The maintainer's ServiceValidator template shows several RegisterConditionValidator attributes on one handler with Value constants. Use the following only if the installed attribute allows multiple instances AND the dispatcher supplies the configured constant as the effective f.Value. Otherwise retain the two explicit fixed-kind handlers in the example. Do not register the same states twice.

```csharp
[RegisterConditionValidator(typeof(ProductValidatorStates),
    ProductValidatorStates.IsDigital, "Product is not digital.", Value = ProductKinds.Digital)]
[RegisterConditionValidator(typeof(ProductValidatorStates),
    ProductValidatorStates.IsPhysical, "Product is not physical.", Value = ProductKinds.Physical)]
private Task<ConditionResult> ValidateFixedKind(DataFilter<string, Product> f)
{
    return !string.IsNullOrWhiteSpace(f.Value)
        ? MatchFixedKind(f.Share, f.Value)
        : ConditionResult.ToFailureAsync("Missing kind.");
}
```

## 5. Verification matrix

Run through the real checker in the target host, not just private handler calls. Build the target and use its test framework. The following are expected outcomes, not executed tests.

| Case | Expected |
| --- | --- |
| Missing model ID / lookup returns null | IsFound and model-dependent checks fail |
| Name empty, whitespace, valid | fail, fail, pass |
| URL null, empty, malformed, HTTPS, non-HTTP | pass, fail, fail, pass, fail |
| Active false with expected false / true | pass / fail (verify missing-value behavior separately) |
| Price -1 / 0 / 10 | fail / pass / pass |
| Tags null/empty / whitespace only / valid tag | fail / fail / pass |
| Kind mismatch / matches ignoring case / empty Value | fail / pass / fail |
| Allowed kinds null / empty / containing kind | fail / fail / pass |
| Category missing / inactive / active | fail / fail / pass |
| Nested success with incompatible payload | fail |
| Publication: invalid name or tags, negative price, missing category | fail before write |
| Publication: all business prerequisites satisfied | pass; authorization remains separate |
| Attribute registration and enum state dispatch | one handler per intended state; actual filter and payload match |
| Two DI scopes | distinct checker/context, no model leakage |
| Controller/service failure path | no mutation when rule or authorization fails |

Check each state individually and ensure all attribute references correspond to enum values. Check no unresolved template tokens remain in generated .cs, no placeholder successes, duplicate states or cycles. Run dotnet build and relevant tests; record actual evidence in the consuming project. Do not claim runtime verification merely from reading or static checks.

## Agent request example

"Use autogenerator-api-skills to generate validators for Product and Category from my existing entity definitions. Keep existing state values. Implement required fields, optional URL, boolean comparison, category lookup and a composite publish condition using the actual checker and scoped registration. Adapt the canonical C# template, verify each enum/state binding and run the build and integration tests."
