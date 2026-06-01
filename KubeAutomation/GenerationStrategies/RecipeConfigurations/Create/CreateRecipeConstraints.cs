using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    public class CreateRecipeConstraints
    {
        public int MinInputs { get; set; } = 1;
        public int? MaxInputs { get; set; } = null;
        public int MinOutputs { get; set; } = 1;
        public int? MaxOutputs { get; set; } = null;
        public List<CreateModifierType> SupportedModifiers { get; set; } = new();
        public bool ShowOutputAmount { get; set; } = false;
        public bool AllowChanceOutputs { get; set; } = true;
        public bool AllowFluidInputs { get; set; } = false;
        public bool AllowFluidOutputs { get; set; } = false;
        public int? FixedInputCount { get; set; } = null;
        public int? FixedOutputCount { get; set; } = null;
    }

    public enum CreateModifierType
    {
        Heated,
        Superheated,
        ProcessingTime,
        KeepHeldItem,
        Experience
    }
}
