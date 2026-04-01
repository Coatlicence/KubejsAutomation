using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    public abstract class BaseRecipeConfiguration 
    {
        public string? RecipeId { get; set; }

        public abstract void Validate();

        /// <summary>
        /// Позволяет легко различать типы конфигураций 
        /// при извлечении рецепта из файла.
        /// 
        /// Например GregTechRecipeConfiguration = "gtceu"
        /// </summary>
        public abstract string RecipeTypeId { get; }
    }

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
        public FluidComponent()
        {
            // empty
        }

        public FluidComponent(string fluidId, int amount)
        {
            FluidId = fluidId;
            Amount = amount;
        }

        public string FluidId { get; set; }
        public int Amount { get; set; }
    }

    public class ItemComponent
    {
        public ItemComponent(int amount, string itemId)
        {
            Amount = amount;
            ItemId = itemId;
        }

        public ItemComponent()
        {

        }

        public int Amount { get; set; }
        public string ItemId { get; set; }

        public string ToJsString()
        {
            var escapedId = ItemId.Replace("\\", "\\\\").Replace("'", "\\'");
            return Amount > 1 ? $"{Amount}x {escapedId}" : escapedId;
        }

        public override string ToString() => ToJsString();  // Для обратной совместимости
    }
}
