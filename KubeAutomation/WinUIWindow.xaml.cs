using KubeAutomation.FileSaveStrategies;
using KubeAutomation.GenerationStrategies.Extractors;
using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.Parsing;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeAutomation.GenerationStrategies.RecipeConfigurations.Create;
using KubeAutomation.Tests;
using LogExtractorLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace KubeAutomation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WinUIWindow : Window
    {
        public MainViewModel ViewModel { get; set; }

        private ObservableCollection<MinecraftItemViewModel> _filteredItems = new();
        public ObservableCollection<MinecraftItemViewModel> FilteredItems => _filteredItems;

        private string _lastSearchText = "";
        private DispatcherTimer _searchDebounceTimer;

        private string _gamePath = "C:\\Users\\KOMP_2024\\AppData\\Roaming\\Create_Cog_And_Circut";
        private string _saveDir = "C:\\Users\\KOMP_2024\\AppData\\Roaming\\Create_Cog_And_Circut\\kubejs\\server_scripts\\tests";

        // null = не ждём выбора, "input"/"output" = Minecraft слот, RecipeSlotViewModel = Create слот
        private RecipeSlotViewModel? _pendingSlotSelection = null;
        private Border? _highlightedSlotBorder = null;

        // --- Recipe form handlers ---

        private void BlockComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BlockComboBox.SelectedItem is not string block) return;
            ViewModel.SelectedBlock = block;
            ViewModel.ApplyRecipeType(block);
            if (HeatNoneRadio != null) HeatNoneRadio.IsChecked = true;
        }

        private void Slot_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is RecipeSlotViewModel slot)
            {
                ClearSlotHighlight();
                _pendingSlotSelection = slot;
                _highlightedSlotBorder = border;
                border.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gold);
            }
        }

        private void Slot_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is RecipeSlotViewModel slot)
            {
                slot.Item = null;
                if (_pendingSlotSelection == slot)
                {
                    ClearSlotHighlight();
                    _pendingSlotSelection = null;
                }
            }
        }

        private void ItemBorder_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_pendingSlotSelection == null) return;
            if (sender is FrameworkElement fe && fe.Tag is MinecraftItemViewModel item)
            {
                AssignItemToSlot(item);
                e.Handled = true;
            }
        }

        private void AssignItemToSlot(MinecraftItemViewModel item)
        {
            if (_pendingSlotSelection is RecipeSlotViewModel slot)
            {
                slot.Item = item;
            }
            ClearSlotHighlight();
            _pendingSlotSelection = null;
        }

        private void ClearSlotHighlight()
        {
            if (_highlightedSlotBorder != null)
            {
                _highlightedSlotBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                _highlightedSlotBorder = null;
            }
        }

        private void AddInput_Click(object sender, RoutedEventArgs e) => ViewModel.AddInputSlot();
        private void RemoveInput_Click(object sender, RoutedEventArgs e) => ViewModel.RemoveInputSlot();
        private void AddOutput_Click(object sender, RoutedEventArgs e) => ViewModel.AddOutputSlot();
        private void RemoveOutput_Click(object sender, RoutedEventArgs e) => ViewModel.RemoveOutputSlot();

        private void HeatRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
                ViewModel.SelectedHeat = tag;
        }

        private void GenerateRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            string code;
            try
            {
                code = ViewModel.SelectedBlock.Equals("workbench", StringComparison.OrdinalIgnoreCase)
                    ? BuildWorkbenchRecipeCode()
                    : RecipeTypeDescriptor.IsCreateType(ViewModel.SelectedBlock)
                        ? BuildCreateRecipeCode()
                        : BuildMinecraftRecipeCode();
            }
            catch (Exception ex)
            {
                GeneratedCodeBlock.Text = $"Ошибка: {ex.Message}";
                GeneratedCodeBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            if (code == null) return; // validation already showed error

            GeneratedCodeBlock.Text = code;
            GeneratedCodeBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);

            try
            {
                if (ViewModel.IsEditMode)
                {
                    var entry = ViewModel.EditingEntry!;
                    var config = entry.Config;
                    string path = ViewModel.RecipeFile.FilePath;

                    string content = File.ReadAllText(path, System.Text.Encoding.UTF8);
                    string newContent = content.Substring(0, config.SourceStart) + code + content.Substring(config.SourceEnd);

                    string tempPath = path + ".tmp";
                    File.WriteAllText(tempPath, newContent, System.Text.Encoding.UTF8);
                    File.Delete(path);
                    File.Move(tempPath, path);

                    SaveStatusBlock.Text = $"Обновлено в {Path.GetFileName(path)}";
                    SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
                    ViewModel.EditingEntry = null;
                    LoadRecipeFile(path);
                    return;
                }

                bool insertByPosition = ViewModel.RecipeFile.PendingInsertPosition >= 0
                    && !string.IsNullOrWhiteSpace(ViewModel.RecipeFile.FilePath)
                    && File.Exists(ViewModel.RecipeFile.FilePath);

                if (insertByPosition)
                {
                    var saver = new FileSaverAtPosition();
                    saver.Insert(ViewModel.RecipeFile.FilePath, code, ViewModel.RecipeFile.PendingInsertPosition);
                    SaveStatusBlock.Text = $"Вставлено в {Path.GetFileName(ViewModel.RecipeFile.FilePath)}";
                    SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
                    ViewModel.RecipeFile.PendingInsertPosition = -1;
                    ViewModel.RecipeFile.StatusText = "";
                    LoadRecipeFile(ViewModel.RecipeFile.FilePath);
                    return;
                }

                string fileName = CreateFileNameBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    SaveStatusBlock.Text = "Введите имя файла.";
                    SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    return;
                }

                var fileSaver = new FileSaverToEnd();
                fileSaver.Save(code, fileName, _saveDir);
                SaveStatusBlock.Text = $"Сохранено в {fileName}.js";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
            }
            catch (Exception ex)
            {
                SaveStatusBlock.Text = $"Ошибка сохранения: {ex.Message}";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private string BuildWorkbenchRecipeCode()
        {
            var outputSlot = ViewModel.OutputSlots.Count > 0 ? ViewModel.OutputSlots[0] : null;
            if (outputSlot?.Item == null)
                throw new InvalidOperationException("Выберите выходной предмет.");

            var slots = ViewModel.WorkbenchSlots;
            bool anyFilled = slots.Any(s => s.Item != null);
            if (!anyFilled)
                throw new InvalidOperationException("Заполните хотя бы одну ячейку сетки.");

            // Assign a letter to each unique item ID
            var letterMap = new Dictionary<string, char>();
            char next = 'A';
            foreach (var slot in slots)
            {
                if (slot.Item != null && !letterMap.ContainsKey(slot.Item.Id))
                    letterMap[slot.Item.Id] = next++;
            }

            // Build 3 rows of 3 chars each
            var rowStrings = new string[3];
            for (int row = 0; row < 3; row++)
            {
                var chars = new char[3];
                for (int col = 0; col < 3; col++)
                {
                    var item = slots[row * 3 + col].Item;
                    chars[col] = item != null ? letterMap[item.Id] : ' ';
                }
                rowStrings[row] = new string(chars);
            }

            // Trim trailing all-space rows
            int lastNonEmpty = 2;
            while (lastNonEmpty > 0 && rowStrings[lastNonEmpty].Trim().Length == 0)
                lastNonEmpty--;
            var trimmedRows = rowStrings.Take(lastNonEmpty + 1).ToArray();

            // Also trim trailing spaces from each row
            trimmedRows = trimmedRows.Select(r => r.TrimEnd()).ToArray();

            var keyMap = letterMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            var grid = new WorkbenchCraftGrid(trimmedRows);
            var pattern = new RecipePattern(grid, keyMap);

            var config = new WorkbenchRecipeConfiguration
            {
                itemOutput = new ItemComponent(outputSlot.Item.Id, (int)outputSlot.Amount),
                pattern    = pattern,
            };

            config.Validate();
            return config.GenerateJsCode();
        }

        private string? BuildMinecraftRecipeCode()
        {
            var inputSlot = ViewModel.InputSlots.Count > 0 ? ViewModel.InputSlots[0] : null;
            var outputSlot = ViewModel.OutputSlots.Count > 0 ? ViewModel.OutputSlots[0] : null;

            if (inputSlot?.Item == null || outputSlot?.Item == null)
            {
                GeneratedCodeBlock.Text = "Ошибка: выберите входной и выходной предметы.";
                GeneratedCodeBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return null;
            }

            var inputComponent = new ItemComponent(inputSlot.Item.Id, 1);
            var outputComponent = new ItemComponent(outputSlot.Item.Id, (int)outputSlot.Amount);
            float? xp = ViewModel.SelectedBlock == "smoking" ? (float)ViewModel.Experience : null;

            var errors = MinecraftStandartRecipesGenerator.Validate(
                ViewModel.SelectedBlock, outputComponent, inputComponent, xp);

            if (errors.Count > 0)
            {
                GeneratedCodeBlock.Text = "Ошибки валидации:\n" + string.Join("\n", errors);
                GeneratedCodeBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return null;
            }

            return MinecraftStandartRecipesGenerator.Generate(
                ViewModel.SelectedBlock, outputComponent, inputComponent, xp);
        }

        private string BuildCreateRecipeCode()
        {
            // Validate all slots are filled
            var emptyInputs = ViewModel.InputSlots.Where(s => s.Item == null).ToList();
            var emptyOutputs = ViewModel.OutputSlots.Where(s => s.Item == null).ToList();
            if (emptyInputs.Count > 0 || emptyOutputs.Count > 0)
                throw new InvalidOperationException("Заполните все слоты входов и выходов.");

            var config = RecipeTypeDescriptor.CreateConfig(ViewModel.SelectedBlock);

            config.Inputs = ViewModel.InputSlots
                .Select(s => (CreateIngredientComponent)new CreateItemIngredientComponent { ItemId = s.Item!.Id })
                .ToList();

            config.Outputs = ViewModel.OutputSlots
                .Select(s => CreateItemComponent.Parse(s.Item!.Id, s.ShowChance ? (float)s.Chance : null))
                .ToList();

            // Modifiers
            config.Modifiers.Heat = ViewModel.SelectedHeat switch
            {
                "heated"      => CreateHeatType.Heated,
                "superheated" => CreateHeatType.Superheated,
                _             => null
            };
            config.Modifiers.ProcessingTime = ViewModel.ShowModifierProcessingTime == Visibility.Visible
                ? (int)ViewModel.ProcessingTime : null;
            config.Modifiers.KeepHeldItem = ViewModel.ShowModifierKeepHeldItem == Visibility.Visible && ViewModel.KeepHeldItem
                ? true : null;

            config.Validate();
            return config.GenerateJsCode();
        }

        private void RemoveRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            string? typeSelection = RemoveTypeCombo.SelectedItem as string;
            string? modSelection = RemoveModCombo.SelectedItem as string;

            var filter = new RecipeFilter
            {
                Output = ViewModel.RemovalOutputSlot.Item?.Id,
                Input  = ViewModel.RemovalInputSlot.Item?.Id,
                Type   = typeSelection,
                Mod    = modSelection,
                Id     = string.IsNullOrEmpty(RemoveIdBox.Text.Trim()) ? null : RemoveIdBox.Text.Trim(),
            };

            if (filter.IsEmpty())
            {
                RemoveStatusBlock.Text = "Заполните хотя бы одно поле фильтра.";
                RemoveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            string fileName = RemoveFileNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                RemoveStatusBlock.Text = "Введите имя файла.";
                RemoveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            try
            {
                var config = new RecipeRemovalConfig { Filters = [filter] };
                config.Validate();
                string code = config.GenerateJsCode();

                RemoveCodeBlock.Text = code;
                RemoveCodeBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);

                var saver = new FileSaverToEnd();
                saver.Save(code, fileName, _saveDir);

                RemoveStatusBlock.Text = $"Сохранено в {fileName}.js";
                RemoveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
            }
            catch (RecipeValidationException ex)
            {
                RemoveStatusBlock.Text = "Ошибка: " + string.Join(", ", ex.Errors);
                RemoveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
            catch (Exception ex)
            {
                RemoveStatusBlock.Text = $"Ошибка: {ex.Message}";
                RemoveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        public WinUIWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();

            GamePathProvider.SetGameRootPath(_gamePath);

            BlockComboBox.SelectedIndex = 0;
            _searchDebounceTimer = new DispatcherTimer();
            _searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(100);
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                PerformSearch();
            };

        }




        // Метод для обновления предметов (вызывается из логики)
        public void UpdateItems(List<MinecraftItem> items)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                ViewModel.Items.Clear();
                var viewModels = new List<MinecraftItemViewModel>(items.Count);
                foreach (var item in items)
                {
                    var vm = new MinecraftItemViewModel(item);
                    viewModels.Add(vm);
                    ViewModel.Items.Add(vm);
                }

                ViewModel.RefreshRemoveModOptions();

                if (!string.IsNullOrEmpty(SearchBox.Text))
                {
                    _searchDebounceTimer.Stop();
                    PerformSearch();
                }
                else
                {
                    UpdateFilteredItems(ViewModel.Items);
                }

                // Загружаем изображения в фоне, не блокируя UI-поток
                var semaphore = new System.Threading.SemaphoreSlim(8);
                _ = Task.WhenAll(viewModels.Select(async vm =>
                {
                    await semaphore.WaitAsync();
                    try { await vm.LoadImageAsync(64); }
                    finally { semaphore.Release(); }
                }));
            });
        }

        // Метод для обновления рецептов (вызывается из логики)
        public void UpdateRecipes(List<string> recipes)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                ViewModel.Recipes.Clear();
                foreach (var recipe in recipes)
                {
                    ViewModel.Recipes.Add(recipe);
                }
            });
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            _lastSearchText = "";
            UpdateFilteredItems(ViewModel.Items);
        }

        private void PerformSearch()
        {
            string input = SearchBox.Text;

            if (string.IsNullOrEmpty(input))
            {
                UpdateFilteredItems(ViewModel.Items);
                _lastSearchText = "";
                return;
            }

            if (input == _lastSearchText)
            {
                UpdateFilteredItems(ViewModel.Items.Where(i => MatchesSearch(i, input)).ToList());
                return;
            }

            var filteredItems = ViewModel.Items.Where(i => MatchesSearch(i, input)).ToList();
            _lastSearchText = input;
            UpdateFilteredItems(filteredItems);
        }

        private static bool MatchesSearch(MinecraftItemViewModel item, string input)
        {
            return item.LocalizedName.Contains(input, StringComparison.OrdinalIgnoreCase)
                || item.Id.Contains(input, StringComparison.OrdinalIgnoreCase)
                || item.ModName.Contains(input, StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateFilteredItems(IEnumerable<MinecraftItemViewModel> items)
        {
            _filteredItems.Clear();
            foreach (var item in items)
            {
                _filteredItems.Add(item);
            }
        }

        public async Task RunItemDemo()
        {
            var iconExtractor = new ItemFluidWithIconExtractor();
            if (!iconExtractor.IsValid())
            {
                return;
            }

            var itemData = await iconExtractor.ExtractAsync();
            var _itemDatabase = new Dictionary<string, MinecraftItem>();

            // Объединяем Items и Fluids в один словарь
            foreach (var item in itemData.Items)
            {
                _itemDatabase[item.Id] = item;
            }
            foreach (var fluid in itemData.Fluids)
            {
                _itemDatabase[fluid.Id] = fluid;
            }

            var recipeExtractor = new RecipesExtractor();
            if (!recipeExtractor.IsValid())
            {
                return;
            }
            var _recipeDatabase = (await recipeExtractor.ExtractAsync()).ToList();

            UpdateItems(_itemDatabase.Values.ToList());
            UpdateRecipes(_recipeDatabase);
        }

        // --- Вкладка "Файл" ---

        private async void FileTab_Browse_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".js");
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // WinUI 3 требует привязки пикера к HWND окна
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                ViewModel.RecipeFile.FilePath = file.Path;
                LoadRecipeFile(file.Path);
            }
        }

        private void FileTab_InsertBefore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not RecipeEntryViewModel entry) return;
            ViewModel.RecipeFile.PendingInsertPosition = entry.Config.SourceStart;
            ViewModel.RecipeFile.StatusText = $"Вставка до: {entry.DisplayName}";
            MainTabView.SelectedIndex = 0; // переключаемся на вкладку "Создать"
        }

        private void FileTab_InsertAfter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not RecipeEntryViewModel entry) return;
            ViewModel.RecipeFile.PendingInsertPosition = entry.Config.SourceEnd;
            ViewModel.RecipeFile.StatusText = $"Вставка после: {entry.DisplayName}";
            MainTabView.SelectedIndex = 0;
        }

        private void FileTab_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Берём путь из поля, либо открываем диалог
            string path = FileTabPathBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                ViewModel.RecipeFile.StatusText = "Введите путь к файлу.";
                return;
            }
            LoadRecipeFile(path);
        }

        private void FileTab_Reload_Click(object sender, RoutedEventArgs e)
        {
            string path = ViewModel.RecipeFile.FilePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                ViewModel.RecipeFile.StatusText = "Файл не выбран.";
                return;
            }
            LoadRecipeFile(path);
        }

        private void LoadRecipeFile(string path)
        {
            try
            {
                var parser = new JsRecipeParser();
                var configs = parser.ParseFile(path);

                ViewModel.RecipeFile.FilePath = path;
                ViewModel.RecipeFile.Entries.Clear();
                foreach (var config in configs)
                    ViewModel.RecipeFile.Entries.Add(new RecipeEntryViewModel(config));

                ViewModel.RecipeFile.StatusText = $"Загружено {configs.Count} рецептов.";
            }
            catch (Exception ex)
            {
                ViewModel.RecipeFile.StatusText = $"Ошибка: {ex.Message}";
            }
        }

        private void FileTab_EditEntry_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not RecipeEntryViewModel entry) return;

            if (entry.Config is WorkbenchRecipeConfiguration wb)
            {
                ViewModel.EditingEntry = entry;

                int idx = ViewModel.RecipeBlocks.IndexOf("workbench");
                if (idx >= 0) BlockComboBox.SelectedIndex = idx;

                // Populate output
                var outputVm = ViewModel.Items.FirstOrDefault(i => i.Id == wb.itemOutput?.ItemId);
                if (ViewModel.OutputSlots.Count > 0)
                {
                    ViewModel.OutputSlots[0].Item   = outputVm;
                    ViewModel.OutputSlots[0].Amount = wb.itemOutput?.Amount ?? 1;
                }

                // Populate grid from pattern
                if (wb.pattern.HasValue)
                {
                    var rows   = wb.pattern.Value.Grid.ToArray();
                    var keyMap = wb.pattern.Value.ToKeyMapDict();

                    ViewModel.ClearWorkbenchSlots();
                    for (int r = 0; r < rows.Length && r < 3; r++)
                    {
                        for (int c = 0; c < rows[r].Length && c < 3; c++)
                        {
                            char ch = rows[r][c];
                            if (ch == ' ') continue;
                            if (keyMap.TryGetValue(ch, out var itemId))
                            {
                                var itemVm = ViewModel.Items.FirstOrDefault(i => i.Id == itemId);
                                ViewModel.WorkbenchSlots[r * 3 + c].Item = itemVm;
                            }
                        }
                    }
                }

                SaveStatusBlock.Text = $"Редактирование: {entry.DisplayName}";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue);
                MainTabView.SelectedIndex = 0;
            }
            else if (entry.Config is MinecraftStandardRecipeConfig mc)
            {
                ViewModel.EditingEntry = entry;

                // Выбираем тип рецепта
                string typeName = mc.Type.ToString().ToLower();
                int idx = ViewModel.RecipeBlocks.IndexOf(typeName);
                if (idx >= 0) BlockComboBox.SelectedIndex = idx;

                // Заполняем входной предмет
                var inputVm = ViewModel.Items.FirstOrDefault(i => i.Id == mc.Input.ItemId);
                ViewModel.InputSlots[0].Item = inputVm;

                // Заполняем выходной предмет
                var outputVm = ViewModel.Items.FirstOrDefault(i => i.Id == mc.Output.ItemId);
                ViewModel.OutputSlots[0].Item = outputVm;

                ViewModel.OutputSlots[0].Amount = mc.Output.Amount;

                if (mc.Type == MinecraftRecipeType.Smoking && mc.Experience.HasValue)
                    ViewModel.Experience = mc.Experience.Value;

                SaveStatusBlock.Text = $"Редактирование: {entry.DisplayName}";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue);

                MainTabView.SelectedIndex = 0;
            }
            else
            {
                ViewModel.RecipeFile.StatusText = $"Редактирование типа «{entry.Config.RecipeTypeId}» не поддерживается.";
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditingEntry = null;
            SaveStatusBlock.Text = "";
        }

        private void FileTab_DeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not RecipeEntryViewModel entry)
                return;

            var config = entry.Config;
            if (!config.HasSourcePosition)
            {
                ViewModel.RecipeFile.StatusText = "Нет позиции в файле — удаление невозможно.";
                return;
            }

            string path = ViewModel.RecipeFile.FilePath;
            try
            {
                string content = File.ReadAllText(path, System.Text.Encoding.UTF8);

                // Вырезаем фрагмент по позиции, убираем лишнюю пустую строку
                string before = content.Substring(0, config.SourceStart);
                string after = content.Substring(config.SourceEnd);

                // Убираем trailing newline перед фрагментом или leading newline после
                if (after.StartsWith("\r\n")) after = after.Substring(2);
                else if (after.StartsWith("\n")) after = after.Substring(1);

                string newContent = before.TrimEnd() + "\n" + after.TrimStart('\r', '\n');

                // Если после удаления осталось только обёртка без рецептов — оставляем как есть
                string tempPath = path + ".tmp";
                File.WriteAllText(tempPath, newContent, System.Text.Encoding.UTF8);
                File.Delete(path);
                File.Move(tempPath, path);

                ViewModel.RecipeFile.StatusText = $"Удалено: {entry.DisplayName}";
                LoadRecipeFile(path);
            }
            catch (Exception ex)
            {
                ViewModel.RecipeFile.StatusText = $"Ошибка удаления: {ex.Message}";
            }
        }
    }
}
