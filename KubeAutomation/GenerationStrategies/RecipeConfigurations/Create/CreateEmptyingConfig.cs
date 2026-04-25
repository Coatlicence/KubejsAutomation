using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Рецепт emptying (опустошение предмета в Item Drain)
    /// </summary>
    public class CreateEmptyingConfig : BaseCreateRecipeConfig
    {
        public override string RecipeTypeId => "create_emptying";

        public override CreateRecipeConstraints GetConstraints()
        {
            return new CreateRecipeConstraints
            {
                FixedInputCount = 1,
                FixedOutputCount = 2,
                SupportedModifiers = new List<CreateModifierType>(),
                AllowChanceOutputs = false,
                AllowFluidOutputs = true
            };
        }

        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            var outputsString = $"[{string.Join(", ", Outputs.Select(o => o.ToJsString()))}]";
            var inputsString = $"[{string.Join(", ", Inputs.Select(i => i.ToJsString()))}]";

            var baseCall = $"{indent}event.recipes.create.emptying({outputsString}, {inputsString})";

            return baseCall;
        }
    }
}