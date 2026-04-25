using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Рецепт cutting (нарезка в Mechanical Saw)
    /// </summary>
    public class CreateCuttingConfig : BaseCreateRecipeConfig
    {
        public override string RecipeTypeId => "create_cutting";

        public override CreateRecipeConstraints GetConstraints()
        {
            return new CreateRecipeConstraints
            {
                FixedInputCount = 1,
                MinOutputs = 1,
                MaxOutputs = null,
                SupportedModifiers = new List<CreateModifierType> { CreateModifierType.ProcessingTime },
                AllowChanceOutputs = true
            };
        }

        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            var outputsString = $"[{string.Join(", ", Outputs.Select(o => o.ToJsString()))}]";
            var inputString = Inputs[0].ToJsString();

            var baseCall = $"{indent}event.recipes.create.cutting({outputsString}, {inputString})";
            var modifierChain = GenerateModifierChain();

            return baseCall + modifierChain;
        }
    }
}