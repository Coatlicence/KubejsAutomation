using System.IO;
using System.Text;

namespace KubeAutomation.FileSaveStrategies
{
    /// <summary>
    /// Вставляет фрагмент рецепта в файл по заданной символьной позиции.
    /// Передавай SourceStart для вставки до рецепта, SourceEnd — после.
    /// </summary>
    public class FileSaverAtPosition
    {
        public void Insert(string filePath, string fragment, int position)
        {
            string content = File.ReadAllText(filePath, Encoding.UTF8);

            string before = content.Substring(0, position);
            string after  = content.Substring(position);

            string newContent = before.TrimEnd() + "\n    " + fragment + "\n" + after.TrimStart('\r', '\n');

            AtomicWrite(filePath, newContent);
        }

        private static void AtomicWrite(string filePath, string content)
        {
            string tempPath = filePath + ".tmp";
            try
            {
                File.WriteAllText(tempPath, content, Encoding.UTF8);
                if (File.Exists(filePath)) File.Delete(filePath);
                File.Move(tempPath, filePath);
            }
            catch
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }
        }
    }
}
