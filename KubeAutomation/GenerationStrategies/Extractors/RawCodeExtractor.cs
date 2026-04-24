using Esprima.Ast;
using KubeAutomation.GenerationStrategies.Parsing;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

public class RawCodeExtractor : IRecipeExtractor
{
    public bool CanHandle(Node? node)
    {
        // Ловим ЛЮБОЙ узел, который не обработали другие экстракторы.
        // Это финальная «страховочная сетка».
        // Возвращаем true для любого узла, чтобы гарантировать сохранение кода.
        return node != null;
    }

    public BaseRecipeConfiguration? Extract(Node? node)
    {
        if (node == null)
            return null;

        return new RawCodeBlock
        {
            RawNode = node,
            Reason = RawCodeReason.UnknownCall
        };
    }
}