# Validator generation

Read [ConditionChecker contract](condition-checker.md) first; it is the source of truth for pinned runtime behavior. Use this document for the generation procedure, not a second list of runtime semantics.

## Abstract input and output

Input: target entity declaration, key/relationships, existing validators, operation constraints, resolved checker version, namespaces, DI. Output: one enum owner, typed attributed handlers, consumer integration and tests. Generate from actual model/domain contracts; Product and Category are examples, not required architecture entities. For each proposed state plan its predicate, input source (ID, trusted model, caller comparison, fixed constant), TValue, failure message, payload type, dependency states and test cases. Account for each relevant property without inventing mandatory rules. Preserve existing names, keys, enum IDs and custom behavior.

## Generate

1. Inspect actual host wrappers and registration. Select the existing ValidatorContext base, not FluentValidation or a replacement checker. Discover concrete types and constructor contract. Generate infrastructure only if absent; use the AutoGenerator for supported scaffolding.
2. Apply [the substitution template](../assets/templates/ValidatorContext.cs.txt); replace tokens with real types, namespace and predicate. Use private Task<ConditionResult> handlers with exact DataFilter<TValue,TModel> signature. Keep InitializeConditions empty for attribute mode. No duplicated enum owners/states or hardcoded success. Distinguish fixed attribute values from caller values per contract.
3. Reuse pure helpers and checker calls to related registered states; compose ordered conditions and propagate failures. Prefer actual model lookup via existing host, no per-request state in a singleton. Do not rely on CheckAllAsync or the fail-open helpers listed in the contract.
4. Integrate through existing controller/service/procedure only as requested. Authorization and business rules are separate; use trusted identity, authorized lookups and established response contract. Stop before side effects on failure.
5. Build and test each state via the actual checker. Verify binding, messages vs payload, nullable inputs, fixed-value override, missing registration, nested results, scope isolation, and failure paths. Capture evidence in target project only.

## Examples

| File | Illustrates |
| --- | --- |
| [Models](../assets/examples/validators/Models.cs.txt) | Example ITModel entities |
| [Category](../assets/examples/validators/CategoryValidator.cs.txt) | Existence and active state |
| [Product](../assets/examples/validators/ProductValidator.cs.txt) | Property, dynamic nullable bool, list, multiple attributes on one handler, fixed constants, related validator, composite publication |
| [Original ModelAI](condition-validators.md) | Historical maintainer example; read compatibility warning before reuse |

Examples are domain-specific illustrations, not production code. Nonnegative price, optional HTTP(S) URL and required tags are assumptions for this example. Keep actual host model and authorization rules.

```csharp
// Persisted entity lookup (use authorized server-side ID).
var found = await _checker.CheckAndResultAsync(ProductValidatorStates.IsFound, productId);

// Runtime comparison: explicitly supply typed value, no attribute default.
var match = await _checker.CheckAndResultAsync(
    ProductValidatorStates.MatchesActive, new DataFilter { Id = productId, Value = false });

// Fixed comparison: attribute supplies "digital".
var digital = await _checker.CheckAndResultAsync(ProductValidatorStates.IsDigital, productId);

// Composite gate with strict success and preserved message.
var canPublish = await _checker.CheckAndResultAsync(ProductValidatorStates.CanPublish, productId);
if (canPublish.Success != true)
{
    // Return the host's safe failure response; do not execute the operation.
}
```

## Verification

The repository smoke harness exercises actual source and, optionally, published 1.0.1. It compiles the very example files above, with real API wrapper code and a minimal in-memory host. It is not proof of the full ASP.NET/EF production pipeline. Run `python3 scripts/check_skill.py` for static structure and `bash scripts/test-validators.sh` for both runtime targets. See the root README. In consuming projects run their own build, EF/DI and integration tests. Report blocked checks rather than asserting compatibility.

Expected: missing entity/state fails; required blank values fail; optional null URL passes, invalid scheme fails; explicit false matches false but absent/wrong type fails; fixed attribute overrides caller; runtime string is preserved; list matching works; nested missing/inactive fails, active passes; composite stops on failure; scoped checker does not leak model state; failure Message contains text and Result is not a message string.
