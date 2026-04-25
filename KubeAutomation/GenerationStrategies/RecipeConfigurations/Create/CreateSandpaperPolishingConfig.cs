using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Рецепт sandpaper_polishing (использует sandpaper)
    /// </summary>
    public class CreateSandpaperPolishingConfig : BaseCreateRecipeConfig
    {
        public override string RecipeTypeId => "create_sandpaper_polishing";

        public override CreateRecipeConstraints GetConstraints()
        {
            return new CreateRecipeConstraints
            {
                FixedInputCount = 1,
                FixedOutputCount = 1,
                SupportedModifiers = new List<CreateModifierType>(),
                AllowChanceOutputs = true
            };
        }

        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            var outputString = Outputs[0].ToJsString();
            var inputString = Inputs[0].ToJsString();

            var baseCall = $"{indent}event.recipes.create.sandpaper_polishing({outputString}, {inputString})";

            return baseCall;
        }
    }
}