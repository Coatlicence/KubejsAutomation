using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Рецепт filling (наполнение предмета в Spout)
    /// </summary>
    public class CreateFillingConfig : BaseCreateRecipeConfig
    {
        public override string RecipeTypeId => "create_filling";

        public override CreateRecipeConstraints GetConstraints()
        {
            return new CreateRecipeConstraints
            {
                FixedInputCount = 2,
                FixedOutputCount = 1,
                SupportedModifiers = new List<CreateModifierType>(),
                AllowChanceOutputs = false,
                AllowFluidInputs = true
            };
        }

        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            var outputString = Outputs[0].ToJsString();
            var inputsString = $"[{string.Join(", ", Inputs.Select(i => i.ToJsString()))}]";

            var baseCall = $"{indent}event.recipes.create.filling({outputString}, {inputsString})";

            return baseCall;
        }
    }
}