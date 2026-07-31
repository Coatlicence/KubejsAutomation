namespace KubeAutomation.Instruments
{
    /// <summary>
    /// Мета-информация о типе рецепта: где и как его регистрировать.
    /// </summary>
    public interface IRecipeType
    {
        /// <summary>
        /// Префикс события KubeJS для регистрации рецепта.
        /// Пример: "ServerEvents.recipes"
        /// </summary>
        string EventPrefix { get; }

        /// <summary>
        /// Имя директории для сохранения файла с рецептом.
        /// Пример: "server_scripts"
        /// </summary>
        string DirectoryName { get; }
    }

    /// <summary>
    /// Тип рецепта для серверных скриптов (стандартный для большинства рецептов).
    /// </summary>
    public class ServerRecipeType : IRecipeType
    {
        public string EventPrefix => "ServerEvents.recipes";
        public string DirectoryName => "server_scripts";
    }
}