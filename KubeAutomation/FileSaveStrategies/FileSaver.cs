using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.FileSaveStrategies
{
    // Сохранение файла
    public abstract class FileSaverBase
    {
        abstract public void Save(string script, string filename, string directory);
    }

    public class RecipeFileSaver : FileSaverBase
    {
        public override void Save(string script, string filename, string directory)
        {
            // создаем полный путь
            string filePath = Path.Combine(directory, $"{filename}.js");
            
            File.WriteAllText(filePath, script);
        }

    }
}
