using System;
using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    /// <summary>
    /// Тип комментария в JavaScript
    /// </summary>
    public enum JsCommentType
    {
        /// <summary>
        /// Однострочный: // текст
        /// </summary>
        SingleLine,

        /// <summary>
        /// Многострочный: /* текст */
        /// </summary>
        MultiLine
    }

    /// <summary>
    /// Представляет комментарий из JS-файла как BaseRecipeConfiguration.
    /// Нужен для вписывания в List<BaseRecipeConfiguration>.
    /// </summary>
    public class CommentRecipeConfiguration : BaseRecipeConfiguration
    {
        /// <summary>
        /// Идентификатор типа для парсинга
        /// </summary>
        public override string RecipeTypeId => "comment";

        /// <summary>
        /// Тип комментария (однострочный или многострочный)
        /// </summary>
        public JsCommentType Type { get; set; }

        /// <summary>
        /// Текст комментария без // или /* */
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Исходное содержимое как есть (с // или /* */)
        /// </summary>
        public string RawContent { get; set; } = "";

        /// <summary>
        /// Валидация всегда успешна (комментарии не валидируем)
        /// </summary>
        public override void Validate()
        {
            // Комментарии не требуют валидации
        }

        /// <summary>
        /// Генерирует JS-код (просто возвращает исходный комментарий)
        /// </summary>
        public override string GenerateJsCode()
        {
            return RawContent;
        }
    }
}