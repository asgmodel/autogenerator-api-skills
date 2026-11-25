# Condition Validators

Maintainer-supplied application example for `AutoGenerator.Conditions`. This is a host-specific usage pattern, not a generated file or an independently compiled example. Resolve the actual library version and inspect the condition registration and dispatch implementation before adapting it.

For agent-native generation, async return correctness, registration, caching and scoped DI, follow [AI-native generation and integration](ai-generation.md). The original example below remains unchanged.

## Design

| Component | Role in the example |
| --- | --- |
| `ModelValidatorStates` | Enum identifies validation conditions. |
| `ModelFeatureValidatorKeys` | String constants associate selected conditions with feature keys. |
| `ValidatorContext<ModelAi, ModelValidatorStates>` | Base context for the model and its condition states. |
| `ITValidator` | Validator marker. |
| `IConditionChecker` | Injected checker; used by the base context and nested validation. |
| `RegisterConditionValidator` | Attribute declares the enum type, state, message, and optional Value key. |
| `DataFilter<TValue, TModel>` | Provides the expected Value and the Share model. |
| `ConditionResult` | Carries success, failure, and result data. |

## Implementation procedure

1. Inspect the host's existing validator namespaces, resolved package and model properties; do not copy LAHJAAPI-specific imports into unrelated hosts.
2. Define an enum with the required states and constants for conditions that use named feature keys. Keep the actual key strings stable where clients depend on them.
3. Inherit from `ValidatorContext<TModel, TStates>` and implement `ITValidator`. Inject `IConditionChecker` and forward it to the base constructor.
4. Override `InitializeConditions()`. In this example it remains empty because registration is attribute-based. Verify that the installed framework discovers private attributed methods and the validator itself.
5. Annotate each method with `RegisterConditionValidator` and use the matching `DataFilter<TValue, TModel>`. Return the shared model on success or a failure result on mismatch.
6. For cross-model checks, await the checker using the other enum state and identifier. Propagate the nested failure. On success, verify the result type before assigning related data.
7. Verify discovery, DI, dispatch, matching, missing data and nested results in the target host. Do not invent checker overloads or manual registration APIs.

## Condition mapping

| State | Value key | Input | Behavior |
| --- | --- | --- | --- |
| HasCategory | category | string | Compares Category with Value |
| HasLanguage | language | string | Compares Language with Value |
| IsStandardModel | is_standard | bool | Compares IsStandard with Value |
| HasDialect | dialect | string | Compares Dialect with Value |
| HasService | Not specified | string | Checks ModelGatewayValidatorStates.HasModel using ModelGatewayId; assigns ModelGateway on success |

The last method does not use the incoming string Value. The relationship between HasService and HasModel is application-specific.

## Original example: ModelAIValidator

The following C# is supplied by the maintainer and retained as an illustrative example. It depends on the original host's model and other validator definitions.

```csharp
using ApiCore.Validators.Conditions;
using AutoGenerator.Conditions;
using LAHJAAPI.Models;
using LAHJAAPI.V1.Validators.Conditions;
using LAHJAAPI.Validators;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using V1.DyModels.Dso.ResponseFilters;

namespace ApiCore.Validators
{
    public enum ModelValidatorStates
    {
        HasCategory,
        HasLanguage,
        IsStandardModel,
        HasDialect,
        HasService,
    }

    public class ModelFeatureValidatorKeys
    {
        public const string Category = "category";
        public const  string Language = "language";
        public const  string IsStandard = "is_standard";
        public const  string Dialect = "dialect";
    }

    public class ModelAIValidator : ValidatorContext<ModelAi, ModelValidatorStates>, ITValidator
    {
        public ModelAIValidator(IConditionChecker checker) : base(checker)
        {
        }

        protected override void InitializeConditions()
        {
            // All conditions now registered via attributes
            

        }

        [RegisterConditionValidator(typeof(ModelValidatorStates), ModelValidatorStates.HasCategory, "Model category does not match the required value.", Value = ModelFeatureValidatorKeys.Category)]
        
        private Task<ConditionResult> CheckHasCategory(DataFilter<string, ModelAi> f)
        {
            return f.Share?.Category == f.Value
                ? ConditionResult.ToSuccessAsync(f.Share)
                : ConditionResult.ToFailureAsync("Category mismatch.");
        }

        [RegisterConditionValidator(typeof(ModelValidatorStates), ModelValidatorStates.HasLanguage, "Model language does not match the required value.", Value = ModelFeatureValidatorKeys.Language)]
        private Task<ConditionResult> CheckHasLanguage(DataFilter<string, ModelAi> f)
        {
            return f.Share?.Language == f.Value
                ? ConditionResult.ToSuccessAsync(f.Share)
                : ConditionResult.ToFailureAsync("Language mismatch.");
        }

        [RegisterConditionValidator(typeof(ModelValidatorStates), ModelValidatorStates.IsStandardModel, "Model is not marked as standard.", Value = ModelFeatureValidatorKeys.IsStandard)]
        private Task<ConditionResult> CheckIsStandard(DataFilter<bool, ModelAi> f)
        {
            return f.Share?.IsStandard == f.Value
                ? ConditionResult.ToSuccessAsync(f.Share)
                : ConditionResult.ToFailureAsync("Model is not marked as standard.");
        }

        [RegisterConditionValidator(typeof(ModelValidatorStates), ModelValidatorStates.HasDialect, "Model dialect does not match the required value.", Value = ModelFeatureValidatorKeys.Dialect)]
        private Task<ConditionResult> CheckHasDialect(DataFilter<string, ModelAi> f)
        {
            return f.Share?.Dialect == f.Value
                ? ConditionResult.ToSuccessAsync(f.Share)
                : ConditionResult.ToFailureAsync("Dialect mismatch.");
        }

        [RegisterConditionValidator(typeof(ModelValidatorStates), ModelValidatorStates.HasService, "Model service is missing")]
        private async Task<ConditionResult> CheckHasModel(DataFilter<string, ModelAi> f)
        {
            if (f.Share != null)
            {
                var res = await _checker.CheckAndResultAsync(ModelGatewayValidatorStates.HasModel, f.Share.ModelGatewayId);

                if (res.Success==true)
                {
                    f.Share.ModelGateway = (ModelGateway?)res.Result;
                    return ConditionResult.ToSuccess(f.Share);
                }

                return res;
            }

            return ConditionResult.ToFailure(null,"Model AI is not found");
        }

    }
}
```

## Review and testing checklist

The example is unchanged; the following are recommendations, not claims that the original code has been repaired or tested.

- Test equal and unequal category, language and dialect, including null inputs if allowed. If Value is null, a null Share can compare equal through the null-conditional expression; explicitly reject missing Share when required.
- Test IsStandardModel with both true and false. Its predicate compares to the requested value, so matching false is a success; the failure message currently describes only one direction.
- Test successful nested lookup, nested failure propagation, null Share, null result and unexpected result type. A cast to ModelGateway can fail for an incompatible object; verify the success payload contract and type before assignment in new implementations.
- Confirm whether comparison should be case-sensitive and whether mutating the navigation property is desired. Avoid circular condition dependencies.
- Test actual attribute registration, discovery of private handlers, feature-key dispatch and dependency injection using the resolved version.
- Do not claim the example compiles or runs outside the host: ModelAi, ModelGateway, ModelGatewayValidatorStates and the dispatch implementation were not supplied.

## Agent usage

> Use autogenerator-api-skills to create an attribute-registered Condition Validator for my existing entity. Define the state enum and feature-key constants, inherit ValidatorContext, use typed DataFilter handlers, and return ConditionResult. For a requested cross-entity condition, inspect the target validator and reuse IConditionChecker. Follow the host's actual package API, preserve contracts, and test success, mismatch, null and nested failures.
