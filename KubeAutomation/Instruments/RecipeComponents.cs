namespace KubeAutomation.Instruments
{
    /// <summary>
    /// Компонент предмета для использования в рецептах.
    /// </summary>
    public class ItemComponent
    {
        public ItemComponent() { }

        public ItemComponent(string itemId, int amount = 1)
        {
            ItemId = itemId;
            Amount = amount;
        }

        public string ItemId { get; set; } = "";
        public int Amount { get; set; } = 1;

        /// <summary>
        /// Форматирует предмет в строку для KubeJS.
        /// Пример: "minecraft:iron_ingot" или "2x minecraft:stick"
        /// </summary>
        public string ToJsString()
        {
            var escaped = ItemId?.Replace("\\", "\\\\").Replace("'", "\\'") ?? "";
            // Убираем кавычки — возвращаем только экранированное значение
            return Amount > 1 ? $"{Amount}x {escaped}" : escaped;
        }

        public override string ToString() => ToJsString();
    }

    /// <summary>
    /// Компонент жидкости для использования в рецептах.
    /// </summary>
    public class FluidComponent
    {
        public FluidComponent() { }

        public FluidComponent(string fluidId, int amount)
        {
            FluidId = fluidId;
            Amount = amount;
        }

        public string FluidId { get; set; } = "";
        public int Amount { get; set; }
    }
}