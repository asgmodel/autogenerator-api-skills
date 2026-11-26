using AutoGenerator.Conditions;
using Example.Models;
using v1.Validators;
using WasmAI.ConditionChecker.Base;

namespace ValidatorSmoke;

public enum ProbeStates { WithoutLookup }
public enum UnregisteredStates { Missing }

public sealed class ProbeValidator : ValidatorContext<Product, ProbeStates>, ITValidator
{
    public ProbeValidator(IConditionChecker checker) : base(checker) { }
    protected override void InitializeConditions() { }

    [RegisterConditionValidator(typeof(ProbeStates), ProbeStates.WithoutLookup,
        "Supply the model.", IsCachability = false)]
    private Task<ConditionResult> Direct(DataFilter<string, Product> f)
        => f.Share is not null
            ? ConditionResult.ToSuccessAsync(f.Share)
            : ConditionResult.ToFailureAsync(null, "Supply the model.");
}
