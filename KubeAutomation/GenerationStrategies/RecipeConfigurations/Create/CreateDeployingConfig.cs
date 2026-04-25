using System.Collections.Generic;
using System.Linq;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Рецепт deploying (использует Deployer)
    /// </summary>
    public class CreateDeployingConfig : BaseCreateRecipeConfig
    {
        public override string RecipeTypeId => "create_deploying";

        public override CreateRecipeConstraints GetConstraints()
        {
            return new CreateRecipeConstraints
            {
                FixedInputCount = 2,
                MinOutputs = 1,
                MaxOutputs = null,
                SupportedModifiers = new List<CreateModifierType> { CreateModifierType.KeepHeldItem },
                AllowChanceOutputs = true
            };
        }

        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            var outputsString = $"[{string.Join(", ", Outputs.Select(o => o.ToJsString()))}]";
            var inputsString = $"[{string.Join(", ", Inputs.Select(i => i.ToJsString()))}]";

            var baseCall = $"{indent}event.recipes.create.deploying({outputsString}, {inputsString})";
            var modifierChain = GenerateModifierChain();

            return baseCall + modifierChain;
        }
    }
}