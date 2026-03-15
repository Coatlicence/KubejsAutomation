using KubeScriptAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    public class GregTechRecipeConfiguration : BaseRecipeConfiguration
    {
        public override string RecipeTypeId => "gtceu";

        public MachineType MachineType { get; set; }
        public List<FluidComponent> InputFluids { get; set; } = new List<FluidComponent>();
        public List<FluidComponent> OutputFluids { get; set; } = new List<FluidComponent>();
        public List<ItemComponent> InputItems { get; set; } = new List<ItemComponent>();
        public List<ItemComponent> OutputItems { get; set; } = new List<ItemComponent>();
        public int? CircuitSetting { get; set; }
        public string NotConsumableItemId { get; set; }
        public int DurationSeconds { get; set; }
        public int EUt { get; set; }
        public IRecipeType RecipeType { get; set; }

        public override void Validate()
        {
            bool hasEnter = InputItems.Count != 0 || InputFluids.Count != 0;
            bool hasExit = OutputFluids.Count != 0 || OutputFluids.Count != 0;

            if (!hasEnter || !hasExit) { throw new ArgumentException("noEnter || noExit"); }


        }
    }
}
