using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeScriptAutomation.CodeGeneratorStrategies
{
    // Базовые классы для расширяемости
    public interface IRecipeType
    {
        string EventPrefix { get; }
        string DirectoryName { get; }
    }

    public class ServerRecipeType : IRecipeType
    {
        public string EventPrefix => "ServerEvents.recipes";
        public string DirectoryName => "server_scripts";
    }

    // Модель данных для рецепта
    public class FluidComponent
    {
        public string FluidId { get; set; }
        public int Amount { get; set; }
    }

    public class ItemComponent
    {
        public int Amount { get; set; } // от 1 до 64
        public string ItemId { get; set; }
    }
    // Обновляем конфигурацию рецепта
    public class RecipeConfiguration
    {
        public string RecipeId { get; set; }
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
    }

}
