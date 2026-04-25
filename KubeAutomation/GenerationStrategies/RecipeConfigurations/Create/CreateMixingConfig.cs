using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Рецепт mixing (смешивание в Mechanical Mixer)
    /// </summary>
    public class CreateMixingConfig : BaseCreateRecipeConfig
    {
        public override string RecipeTypeId => "create_mixing";

        public override CreateRecipeConstraints GetConstraints()
        {
            return new CreateRecipeConstraints
            {
                MinInputs = 1,
                MaxInputs = null,
                MinOutputs = 1,
                MaxOutputs = null,
                SupportedModifiers = new List<CreateModifierType> { CreateModifierType.Heated, CreateModifierType.Superheated },
                AllowChanceOutputs = true,
                AllowFluidInputs = true,
                AllowFluidOutputs = true
            };
        }

        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            var outputsString = $"[{string.Join(", ", Outputs.Select(o => o.ToJsString()))}]";
            var inputsString = $"[{string.Join(", ", Inputs.Select(i => i.ToJsString()))}]";

            var baseCall = $"{indent}event.recipes.create.mixing({outputsString}, {inputsString})";
            var modifierChain = GenerateModifierChain();

            return baseCall + modifierChain;
        }
    }
}