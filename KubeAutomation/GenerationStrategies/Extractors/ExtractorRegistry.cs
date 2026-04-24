using System;
using System.Collections.Generic;
using System.Linq;
using Esprima.Ast;

namespace KubeAutomation.GenerationStrategies.Parsing
{
    /// <summary>
    /// Реестр экстракторов. Порядок регистрации определяет приоритет.
    /// </summary>
    public class ExtractorRegistry
    {
        private readonly List<IRecipeExtractor> _extractors = new();

        public void Register(IRecipeExtractor extractor)
        {
            _extractors.Add(extractor);
            // Приоритеты удалены: порядок регистрации = порядок проверки
        }

        public IRecipeExtractor? GetExtractor(Node? node)
        {
            // Первый совпавший экстрактор обрабатывает узел
            return _extractors.FirstOrDefault(e => e.CanHandle(node));
        }

        public IReadOnlyList<IRecipeExtractor> GetAll()
        {
            return _extractors.AsReadOnly();
        }

        public static ExtractorRegistry CreateDefault()
        {
            var registry = new ExtractorRegistry();

            // Порядок регистрации = порядок проверки (первый совпавший выигрывает)
            // Специфичные экстракторы — первыми, общий (Raw) — последним
            registry.Register(new RemovalExtractor());    // event.remove
            registry.Register(new CustomExtractor());     // event.custom
            registry.Register(new RawCodeExtractor());    // любой другой event.что-то

            return registry;
        }
    }
}