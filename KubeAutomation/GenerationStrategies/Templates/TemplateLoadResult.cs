using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.Templates;

/// <summary>
/// Результат загрузки шаблона
/// </summary>
public class TemplateLoadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public CustomRecipeConfig? Config { get; set; }
    public TemplateMetadata? Metadata { get; set; }
    public List<PathInfo>? Paths { get; set; }

    public static TemplateLoadResult Fail(string error)
    {
        return new TemplateLoadResult
        {
            Success = false,
            ErrorMessage = error
        };
    }

    public static TemplateLoadResult Ok(
        CustomRecipeConfig config,
        TemplateMetadata metadata,
        List<PathInfo> paths)
    {
        return new TemplateLoadResult
        {
            Success = true,
            Config = config,
            Metadata = metadata,
            Paths = paths
        };
    }
}