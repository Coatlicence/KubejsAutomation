using KubeScriptAutomation.CodeGeneratorStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeScriptAutomation
{
    // Сохранение файла
    public interface IRecipeFileSaver
    {
        void Save(string script, RecipeConfiguration config);
    }

    public class RecipeFileSaver : IRecipeFileSaver
    {
        public void Save(string script, RecipeConfiguration config)
        {
            string directory = Path.Combine(Directory.GetCurrentDirectory(), config.RecipeType.DirectoryName);
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, $"{config.RecipeId}.js");
            File.WriteAllText(filePath, script);
            Console.WriteLine($"Рецепт сохранен в {filePath}");
        }
    }
}
