// Minimal in-memory host adapter for actual checker integration.
// The linked upstream wrappers and registration are real. EF and notifier are
// intentionally not included; a real application needs its own integration tests.
using AutoGenerator.Conditions;
using Example.Models;
using Microsoft.Extensions.DependencyInjection;

namespace v1.Validators.Conditions
{
    public interface ITFactoryInjector
    {
        Task<T?> Find<T>(string? id) where T : class;
    }

    public sealed class MemoryInjector : ITFactoryInjector
    {
        public Product Product { get; } = new()
        {
            Id = "p1", Name = "Example", IsActive = false, Kind = "digital",
            Price = 10m, CategoryId = "c1", Tags = new List<string> { "tag" }
        };
        public Category Category { get; } = new() { Id = "c1", Name = "Example", IsActive = true };
        public int Lookups { get; private set; }

        public Task<T?> Find<T>(string? id) where T : class
        {
            Lookups++;
            object? entity = typeof(T) == typeof(Product) && id == Product.Id ? Product
                : typeof(T) == typeof(Category) && id == Category.Id ? Category : null;
            return Task.FromResult(entity as T);
        }
    }

    public static class HostServices
    {
        public static IServiceCollection AddExampleValidators(this IServiceCollection services)
        {
            services.AddScoped<ITFactoryInjector, MemoryInjector>();
            services.AddScoped<v1.Validators.IConditionChecker>(sp =>
            {
                var checker = new v1.Validators.ConditionChecker(sp.GetRequiredService<ITFactoryInjector>());
                // Use the actual AutoGenerator assembly scanner.
                BaseConfigValidator.Register(checker, typeof(HostServices).Assembly);
                return checker;
            });
            return services;
        }
    }
}

namespace v1.Validators
{
    using v1.Validators.Conditions;

    public interface IConditionChecker : IBaseConditionChecker
    {
        ITFactoryInjector Injector { get; }
    }

    public sealed class ConditionChecker : BaseConditionChecker, IConditionChecker
    {
        public ITFactoryInjector Injector { get; }
        public ConditionChecker(ITFactoryInjector injector)
        {
            Injector = injector ?? throw new ArgumentNullException(nameof(injector));
        }
    }

    public abstract class ValidatorContext<TModel, TStates>
        : BaseValidatorContext<TModel, TStates>, ITValidator
        where TModel : class where TStates : Enum
    {
        protected readonly ITFactoryInjector _injector;
        protected ValidatorContext(IConditionChecker checker) : base(checker)
        {
            _injector = checker.Injector;
        }
        protected override Task<TModel?> GetModel(string? id) => _injector.Find<TModel>(id);
    }
}
