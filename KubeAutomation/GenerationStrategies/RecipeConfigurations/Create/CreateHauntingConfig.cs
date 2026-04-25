using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Рецепт haunting (использует Encased Fan и Soul Fire)
    /// </summary>
    public class CreateHauntingConfig : BaseCreateRecipeConfig
    {
        public override string RecipeTypeId => "create_haunting";

        public override CreateRecipeConstraints GetConstraints()
        {
            return new CreateRecipeConstraints
            {
                FixedInputCount = 1,
                MinOutputs = 1,
                MaxOutputs = null,
                SupportedModifiers = new List<CreateModifierType>(),
                AllowChanceOutputs = true
            };
        }

        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            var outputsString = $"[{string.Join(", ", Outputs.Select(o => o.ToJsString()))}]";
            var inputString = Inputs[0].ToJsString();

            var baseCall = $"{indent}event.recipes.create.haunting({outputsString}, {inputString})";

            return baseCall;
        }
    }
}