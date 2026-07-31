using System;
using System.Collections.Generic;
using System.Linq;
using KubeAutomation.GenerationStrategies.Generators;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    public static class RecipeTypeDescriptor
    {
        private static readonly Dictionary<string, Func<BaseCreateRecipeConfig>> _factories =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["compacting"]           = () => new CreateCompactingConfig(),
                ["crushing"]             = () => new CreateCrushingConfig(),
                ["cutting"]              = () => new CreateCuttingConfig(),
                ["deploying"]            = () => new CreateDeployingConfig(),
                ["emptying"]             = () => new CreateEmptyingConfig(),
                ["filling"]              = () => new CreateFillingConfig(),
                ["haunting"]             = () => new CreateHauntingConfig(),
                ["milling"]              = () => new CreateMillingConfig(),
                ["mixing"]               = () => new CreateMixingConfig(),
                ["pressing"]             = () => new CreatePressingConfig(),
                ["sandpaper_polishing"]  = () => new CreateSandpaperPolishingConfig(),
                ["splashing"]            = () => new CreateSplashingConfig(),
            };

        private static readonly Dictionary<string, CreateRecipeConstraints> _minecraftConstraints =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["workbench"] = new CreateRecipeConstraints
                {
                    FixedInputCount = 0, FixedOutputCount = 1,
                    ShowOutputAmount = true, AllowChanceOutputs = false
                },
                ["smelting"] = new CreateRecipeConstraints
                {
                    FixedInputCount = 1, FixedOutputCount = 1,
                    ShowOutputAmount = true, AllowChanceOutputs = false
                },
                ["blasting"] = new CreateRecipeConstraints
                {
                    FixedInputCount = 1, FixedOutputCount = 1,
                    ShowOutputAmount = true, AllowChanceOutputs = false
                },
                ["smoking"] = new CreateRecipeConstraints
                {
                    FixedInputCount = 1, FixedOutputCount = 1,
                    ShowOutputAmount = true, AllowChanceOutputs = false,
                    SupportedModifiers = [CreateModifierType.Experience]
                },
                ["stonecutting"] = new CreateRecipeConstraints
                {
                    FixedInputCount = 1, FixedOutputCount = 1,
                    ShowOutputAmount = true, AllowChanceOutputs = false
                },
            };

        public static IReadOnlyList<string> CreateTypeNames { get; } =
            _factories.Keys.OrderBy(k => k).ToList();

        public static IReadOnlyList<string> AllTypeNames { get; } =
            new[] { "workbench" }
                .Concat(MinecraftStandartRecipesGenerator.Blocks)
                .Concat(CreateTypeNames)
                .ToList();

        public static bool IsCreateType(string typeName) =>
            _factories.ContainsKey(typeName);

        public static bool IsMinecraftType(string typeName) =>
            _minecraftConstraints.ContainsKey(typeName);

        public static CreateRecipeConstraints? GetConstraints(string typeName)
        {
            if (_minecraftConstraints.TryGetValue(typeName, out var mc))
                return mc;

            if (!_factories.TryGetValue(typeName, out var factory)) return null;
            return factory().GetConstraints();
        }

        public static BaseCreateRecipeConfig CreateConfig(string typeName)
        {
            if (!_factories.TryGetValue(typeName, out var factory))
                throw new ArgumentException($"Unknown Create recipe type: {typeName}");
            return factory();
        }
    }
}