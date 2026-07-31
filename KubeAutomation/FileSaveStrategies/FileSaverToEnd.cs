using KubeAutomation.Instruments;

namespace KubeAutomation.FileSaveStrategies
{
    using System;
    using System.IO;
    using System.Text;


    public class FileSaverToEnd : FileSaverBase
    {
        /// <summary>
        /// Сохраняет фрагмент рецепта в конец файла
        /// Если файл не существует — создаёт с wrapper
        /// </summary>
        public override void Save(string fragment, string filename, string directory)
        {
            // 1. Создаём полный путь
            string filePath = Path.Combine(directory, $"{filename}.js");

            // 2. Проверяем существование файла
            if (!File.Exists(filePath))
            {
                // Файл не существует, создаём новый с wrapper
                CreateNewFileWithWrapper(filePath, fragment);
            }
            else
            {
                // Файл существует, добавляем фрагмент в конец
                AppendFragmentToFile(filePath, fragment);
            }
        }

        /// <summary>
        /// Создаёт новый файл с wrapper и первым рецептом
        /// </summary>
        private void CreateNewFileWithWrapper(string filePath, string fragment)
        {
            // Создаём wrapper через EmptyFunctionGenerator
            var wrapper = EmptyFunctionGenerator.GenerateTitle(new ServerRecipeType());

            // Находим позицию вставки (перед закрывающей "})")
            int insertPosition = wrapper.LastIndexOf("})");
            if (insertPosition == -1)
                throw new InvalidOperationException("Wrapper не содержит закрывающую скобку");

            // Формируем итоговое содержимое
            var before = wrapper.Substring(0, insertPosition);
            var after = wrapper.Substring(insertPosition);

            // Добавляем фрагмент с отступом и пустой строкой для разделения
            string content = $"{before}    {fragment}\n{after}";

            // Атомарная запись
            AtomicWrite(filePath, content);
        }

        /// <summary>
        /// Добавляет фрагмент в существующий файл
        /// </summary>
        private void AppendFragmentToFile(string filePath, string fragment)
        {
            // 1. Читаем существующее содержимое
            string content = File.ReadAllText(filePath, Encoding.UTF8);

            // 2. Находим позицию закрывающей "});" (последняя в файле)
            int insertPosition = content.LastIndexOf("});");
            if (insertPosition == -1)
                throw new InvalidOperationException("Файл не содержит закрывающую скобку рецепта");

            // 3. Проверяем, есть ли уже рецепты в файле
            // Если между opening и closing есть контент, добавляем пустую строку перед фрагментом
            string before = content.Substring(0, insertPosition);
            string after = content.Substring(insertPosition);

            // Проверяем, нужно ли добавить пустую строку для разделения рецептов
            string separator = before.TrimEnd().EndsWith("{") ? "" : "\n    ";

            // 4. Формируем итоговое содержимое
            string newContent = $"{before}{separator}{fragment}\n{after}";

            // 5. Атомарная запись
            AtomicWrite(filePath, newContent);
        }

        /// <summary>
        /// Атомарная запись файла (temp + rename)
        /// </summary>
        private void AtomicWrite(string filePath, string content)
        {
            // 1. Создаём временный файл в той же директории
            string tempPath = filePath + ".tmp";

            try
            {
                // 2. Пишем во временный файл
                File.WriteAllText(tempPath, content, Encoding.UTF8);

                // 3. Удаляем оригинал (если существует)
                if (File.Exists(filePath))
                    File.Delete(filePath);

                // 4. Переименовываем временный файл в оригинал
                File.Move(tempPath, filePath);
            }
            catch
            {
                // 5. При ошибке удаляем временный файл
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                throw;
            }
        }
    }
    
}
