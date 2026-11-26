using Example.Models;
using Example.Validators;
using Microsoft.Extensions.DependencyInjection;
using v1.Validators;
using v1.Validators.Conditions;
using ValidatorSmoke;
using WasmAI.ConditionChecker.Base;

var services = new ServiceCollection().AddExampleValidators();
using var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true, ValidateOnBuild = true
});
using var scope = provider.CreateScope();
var checker = scope.ServiceProvider.GetRequiredService<IConditionChecker>();
var injector = (MemoryInjector)checker.Injector;
var product = injector.Product;
var category = injector.Category;
int checks = 0;
void Assert(bool ok, string label)
{
    if (!ok) throw new InvalidOperationException("FAIL: " + label);
    checks++;
    Console.WriteLine("PASS: " + label);
}
async Task<ConditionResult> Check(ProductValidatorStates state, object? context = null)
    => await checker.CheckAndResultAsync(state, context ?? product.Id);
async Task Expect(ProductValidatorStates state, bool expected, string label, object? context = null)
{
    var r = await Check(state, context);
    Assert(r.Success == expected, label);
    if (!expected)
        Assert(!string.IsNullOrWhiteSpace(r.Message), label + " message");
}

Assert(ReferenceEquals(checker, scope.ServiceProvider.GetRequiredService<IConditionChecker>()), "one checker per scope");
Assert(checker.GetProvider<ProductValidatorStates>()?.GetConditionTypes().Count() == 12, "all product states registered");
Assert(checker.GetProvider<CategoryValidatorStates>()?.GetConditionTypes().Count() == 2, "all category states registered");
var found = await Check(ProductValidatorStates.IsFound);
Assert(found.Success == true && ReferenceEquals(found.Result, product), "string ID maps to loaded model");
await Expect(ProductValidatorStates.IsFound, false, "missing ID", "missing");
var absent = await checker.CheckAndResultAsync(UnregisteredStates.Missing, product.Id);
Assert(absent.Success == false, "unregistered state fails via single-state API");
Assert(!await checker.CheckAsync((ProductValidatorStates)9999, product.Id), "undefined enum state fails");
product.Name = " ";
var blank = await Check(ProductValidatorStates.HasName);
Assert(blank.Success == false && blank.Result is null && blank.Message == "Product name is required.", "failure message not payload");
product.Name = "Example";
await Expect(ProductValidatorStates.HasName, true, "valid name");
await Expect(ProductValidatorStates.HasValidOptionalUrl, true, "optional URL absent");
product.Website = "ftp://example.com";
await Expect(ProductValidatorStates.HasValidOptionalUrl, false, "invalid scheme");
product.Website = "https://example.com";
await Expect(ProductValidatorStates.HasValidOptionalUrl, true, "https");
await Expect(ProductValidatorStates.MatchesActive, true, "explicit false matches false", new DataFilter { Id = product.Id, Value = false });
await Expect(ProductValidatorStates.MatchesActive, false, "true differs", new DataFilter { Id = product.Id, Value = true });
await Expect(ProductValidatorStates.MatchesActive, false, "missing bool rejected");
await Expect(ProductValidatorStates.MatchesActive, false, "string false rejected", new DataFilter { Id = product.Id, Value = "false" });
await Expect(ProductValidatorStates.HasKind, true, "dynamic string value preserved", new DataFilter { Id = product.Id, Value = "DIGITAL" });
await Expect(ProductValidatorStates.HasKind, false, "dynamic string mismatch", new DataFilter { Id = product.Id, Value = "physical" });
await Expect(ProductValidatorStates.IsDigital, true, "fixed digital overrides caller", new DataFilter { Id = product.Id, Value = "physical" });
await Expect(ProductValidatorStates.IsPhysical, false, "fixed physical overrides caller", new DataFilter { Id = product.Id, Value = "digital" });
await Expect(ProductValidatorStates.IsInKinds, true, "list membership", new DataFilter { Id = product.Id, Value = new List<string> { "digital" } });
await Expect(ProductValidatorStates.IsInKinds, false, "wrong list type", new DataFilter { Id = product.Id, Value = "digital" });
product.Price = -1m;
await Expect(ProductValidatorStates.HasValidPrice, false, "negative price");
await Expect(ProductValidatorStates.CanPublish, false, "composite rejects negative price");
product.Price = 0m;
await Expect(ProductValidatorStates.HasValidPrice, true, "zero price allowed");
product.Tags.Clear();
await Expect(ProductValidatorStates.HasItems, false, "empty collection");
await Expect(ProductValidatorStates.CanPublish, false, "composite rejects empty tags");
product.Tags.Add("tag");
category.IsActive = false;
await Expect(ProductValidatorStates.HasActiveCategory, false, "nested inactive category");
await Expect(ProductValidatorStates.CanPublish, false, "composite rejects nested failure");
category.IsActive = true;
product.CategoryId = "missing";
await Expect(ProductValidatorStates.HasActiveCategory, false, "nested missing category");
product.CategoryId = category.Id;
await Expect(ProductValidatorStates.HasActiveCategory, true, "nested active category");
await Expect(ProductValidatorStates.CanPublish, true, "composite valid");
var lookupCount = injector.Lookups;
var direct = await checker.CheckAndResultAsync(ProbeStates.WithoutLookup, product.Id);
Assert(direct.Success == false && injector.Lookups == lookupCount, "IsCachability false disables lookup");
direct = await checker.CheckAndResultAsync(ProbeStates.WithoutLookup, new DataFilter { Share = product });
Assert(direct.Success == true && injector.Lookups == lookupCount, "supplied model without lookup");
var trusted = await Check(ProductValidatorStates.IsFound, new DataFilter { Id = "other", Share = product });
Assert(trusted.Success == true && ReferenceEquals(trusted.Result, product), "supplied Share takes precedence over ID");
using var secondScope = provider.CreateScope();
var second = secondScope.ServiceProvider.GetRequiredService<IConditionChecker>();
Assert(!ReferenceEquals(checker, second) && !ReferenceEquals(checker.Injector, second.Injector), "separate checker and injector per scope");
var other = await second.CheckAndResultAsync(ProductValidatorStates.IsFound, product.Id);
Assert(other.Success == true && !ReferenceEquals(other.Result, product), "no shared entity object across scopes");
Console.WriteLine($"Completed {checks} assertions through actual checker.");
