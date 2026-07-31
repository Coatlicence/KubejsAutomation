using KubeAutomation.Instruments;


namespace KubeAutomation.Instruments
{
    public static class EmptyFunctionGenerator
    {
        /// <summary>
        /// Создаёт пустую функцию
        /// </summary>
        /// <param name="type">ServerEvents.recipes или что-то другое</param>
        /// <returns>JS-файл без вызова функций</returns>
        public static string GenerateTitle(IRecipeType type)
        {
            List<string> lines = [];

            lines.Add($"{type.EventPrefix}((event) => {{");
            lines.Add("    ");
            lines.Add("});");

            return string.Join(Environment.NewLine, lines);
        }
    }
}