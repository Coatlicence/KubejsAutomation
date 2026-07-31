using Esprima.Ast;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.RawCode
{
    /// <summary>
    /// Конфигурация для неизвестных вызовов, которые не распознаны другими экстракторами.
    /// Хранит AST-узел и восстанавливает исходный код через Node.ToString().
    /// </summary>
    public class RawCodeBlock : BaseRecipeConfiguration
    {
        /// <summary>
        /// AST-узел, представляющий исходный вызов
        /// </summary>
        public Node? RawNode { get; set; }

        /// <summary>
        /// Причина попадания в сырой код (для отладки/фильтрации)
        /// </summary>
        public RawCodeReason Reason { get; set; }

        public override string RecipeTypeId => "raw";

        public override void Validate()
        {
            // Валидация не требуется: сырой код принимается как есть
        }

        public override string GenerateJsCode()
        {
            // Восстанавливаем код через встроенный сериализатор Esprima
            return RawNode?.ToString() ?? "";
        }
    }

    /// <summary>
    /// Причины попадания кода в RawCodeBlock
    /// </summary>
    public enum RawCodeReason
    {
        /// <summary>
        /// Вызов функции, для которого нет экстрактора
        /// </summary>
        UnknownCall,

        /// <summary>
        /// Сложное выражение, которое не удалось распарсить
        /// </summary>
        ComplexExpression,

        /// <summary>
        /// Другая причина
        /// </summary>
        Other
    }
}