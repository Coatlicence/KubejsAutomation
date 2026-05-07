using KubeAutomation.FileSaveStrategies;
using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
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

        private ObservableCollection<string> _autocompleteSuggestions = new();
        private string _lastSearchText = "";
        private DispatcherTimer _searchDebounceTimer;

        private readonly object _consoleLock = new object();
        private bool _isProcessingCommand = false;
        private string _gamePath = "C:\\Users\\KOMP_2024\\AppData\\Roaming\\Create_Cog_And_Circut";
        private string _saveDir = "C:\\Users\\KOMP_2024\\AppData\\Roaming\\Create_Cog_And_Circut\\kubejs\\server_scripts\\tests";

        // null = не ждём выбора, "input" / "output" = ждём клика по предмету
        private string? _pendingSlotSelection = null;

        // --- Recipe form handlers ---

        private void BlockComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BlockComboBox.SelectedItem is string block)
            {
                ViewModel.SelectedBlock = block;
                ExperiencePanel.Visibility = block == "smoking" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void InputSlot_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _pendingSlotSelection = "input";
            SlotSelectionStatus.Text = "Кликните предмет в сетке для входного слота...";
        }

        private void OutputSlot_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _pendingSlotSelection = "output";
            SlotSelectionStatus.Text = "Кликните предмет в сетке для выходного слота...";
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
            if (_pendingSlotSelection == "input")
            {
                ViewModel.InputItem = item;
                InputItemImage.Source = item.Image;
                InputItemLabel.Text = "";
                SlotSelectionStatus.Text = $"Вход: {item.Id}";
            }
            else if (_pendingSlotSelection == "output")
            {
                ViewModel.OutputItem = item;
                OutputItemImage.Source = item.Image;
                OutputItemLabel.Text = "";
                SlotSelectionStatus.Text = $"Выход: {item.Id}";
            }
            _pendingSlotSelection = null;
        }

        private void SaveRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            string code = GeneratedCodeBlock.Text;
            if (string.IsNullOrWhiteSpace(code) || GeneratedCodeBlock.Foreground is SolidColorBrush b && b.Color == Microsoft.UI.Colors.Red)
            {
                SaveStatusBlock.Text = "Сначала сгенерируйте рецепт.";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            string fileName = SaveFileNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                SaveStatusBlock.Text = "Введите имя файла.";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            try
            {
                var saver = new FileSaverToEnd();
                saver.Save(code, fileName, _saveDir);
                SaveStatusBlock.Text = $"Сохранено в {_saveDir}\\{fileName}.js";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
            }
            catch (Exception ex)
            {
                SaveStatusBlock.Text = $"Ошибка: {ex.Message}";
                SaveStatusBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private void GenerateRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.InputItem == null || ViewModel.OutputItem == null)
            {
                GeneratedCodeBlock.Text = "Ошибка: выберите входной и выходной предметы.";
                GeneratedCodeBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            var inputComponent = new ItemComponent(ViewModel.InputItem.Id, 1);
            var outputComponent = new ItemComponent(ViewModel.OutputItem.Id, (int)ViewModel.OutputAmount);
            float? xp = ViewModel.IsSmokingSelected ? (float)ViewModel.Experience : null;

            var errors = MinecraftStandartRecipesGenerator.Validate(
                ViewModel.SelectedBlock, outputComponent, inputComponent, xp);

            if (errors.Count > 0)
            {
                GeneratedCodeBlock.Text = "Ошибки валидации:\n" + string.Join("\n", errors);
                GeneratedCodeBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            string code = MinecraftStandartRecipesGenerator.Generate(
                ViewModel.SelectedBlock, outputComponent, inputComponent, xp);

            GeneratedCodeBlock.Text = code;
            GeneratedCodeBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
        }

        public WinUIWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();

            GamePathProvider.SetGameRootPath(_gamePath);
            
            // Инициализируем ItemsRepeater с пустой коллекцией
            // _filteredItems.Clear(); // ObservableCollection и так пуст
            // Инициализируем ListView автодополнения
            AutocompleteListView.ItemsSource = _autocompleteSuggestions;
            BlockComboBox.SelectedIndex = 0;
            // Инициализируем таймер для debounce
            _searchDebounceTimer = new DispatcherTimer();
            _searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(100);
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                PerformSearch();
            };

            // Инициализация консоли
            AppendToConsole("Система автоматизации KubeJS скриптов");
            AppendToConsole("===============================================");
            AppendToConsole("Введите команду. Доступные команды: да, нет, получить, test, gr, exit");
        }

        private void AppendToConsole(string message)
        {
            lock (_consoleLock)
            {
                // Используем DispatcherQueue для потокобезопасного обновления UI
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    if (!string.IsNullOrEmpty(ConsoleOutputBox.Text))
                    {
                        ConsoleOutputBox.Text += "\n";
                    }
                    ConsoleOutputBox.Text += message;
                    // Прокручиваем вниз
                    ConsoleOutputScrollViewer.ChangeView(
                        null, // HorizontalOffset
                        ConsoleOutputScrollViewer.ScrollableHeight, // VerticalOffset
                        null, // ZoomFactor
                        true // DisableAnimation
                    );
                });
            }
        }

        private void ProcessConsoleCommand(string command)
        {
            if (_isProcessingCommand) return; // Защита от повторных вызовов
            _isProcessingCommand = true;

            // Используем Task.Run, чтобы не блокировать UI-поток длительными операциями
            _ = Task.Run(async () =>
            {
                try
                {
                    AppendToConsole($"> {command}");
                    string res = command.Trim().ToLower();

                    switch (res)
                    {
                        case "exit":
                        case "quit":
                        case "выйти":
                            AppendToConsole("Программа завершена.");
                            this.Close();
                            break;

                        case "получить":
                        case "п":
                        case "g":
                        case "get":
                            AppendToConsole("Запуск получения данных...");
                            // await StartExternalAppAsync(); // Вызовите ваш метод
                            AppendToConsole("Запуск задачи получения данных (фоновый процесс).(заглушка)");
                            break;

                        case "t":
                        case "т":
                        case "test":
                        case "тест":
                            AppendToConsole("Запуск тестов...");
                            TestRunner.RunAll();
                            AppendToConsole("Тесты завершены. Введите любой символ для продолжения...");
                            break;

                        case "пр":
                        case "gr":
                            AppendToConsole("Попытка извлечения рецептов...");
                            try
                            {
                                var extractor = new RecipesExtractor();
                                AppendToConsole($"Экстрактор готов к работе: {extractor.IsValid()}.");

                                var recipes = await extractor.ExtractAsync();
                                AppendToConsole($"Получено {recipes.Count} рецептов.");
                            }
                            catch (Exception ex)
                            {
                                AppendToConsole($"Ошибка при извлечении рецептов: {ex.Message}");
                            }
                            break;

                        case "да":
                        case "yes":
                            AppendToConsole("Введите ID рецепта или нажмите Enter для создания нового:");
                            // Можно добавить логику для создания рецепта
                            // Пока просто спрашиваем путь
                            AppendToConsole("Введите путь сохранения (или 'н', чтобы использовать стандартный):");
                            // Для простоты, сразу используем стандартный путь
                            string savePath = _saveDir;
                            AppendToConsole($"Путь сохранения: {savePath}");

                            // Здесь должна быть логика сбора данных для рецепта
                            // var config = inputCollector.Collect(); // Не интерактивно
                            // string script = GregTechRecipeCodeGenerator.Generate(config);
                            // fileSaver.Save(script, config.RecipeId, savePath);

                            AppendToConsole("Рецепт успешно создан! (заглушка)");
                            break;

                        case "нет":
                        case "n":
                        case "no":
                            AppendToConsole("Программа завершена.");
                            this.Close();
                            break;

                        default:
                            AppendToConsole($"Неизвестная команда: '{command}'. Попробуйте снова.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AppendToConsole($"Ошибка при выполнении команды: {ex.Message}");
                }
                finally
                {
                    _isProcessingCommand = false;
                }
            });
        }

        private void ConsoleSendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = ConsoleInputBox.Text;
            if (!string.IsNullOrWhiteSpace(input))
            {
                ProcessConsoleCommand(input);
                ConsoleInputBox.Text = ""; // Очищаем поле ввода
            }
        }

        private void ConsoleInputBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ConsoleSendButton_Click(sender, new RoutedEventArgs());
            }
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
            _autocompleteSuggestions.Clear();
            AutocompleteListView.Visibility = Visibility.Collapsed;
            UpdateFilteredItems(ViewModel.Items);
        }

        private void PerformSearch()
        {
            string input = SearchBox.Text;

            if (string.IsNullOrEmpty(input))
            {
                _autocompleteSuggestions.Clear();
                AutocompleteListView.Visibility = Visibility.Collapsed;
                UpdateFilteredItems(ViewModel.Items);
                _lastSearchText = "";
                return;
            }

            if (input == _lastSearchText)
            {
                if (AutocompleteListView.Visibility == Visibility.Collapsed && _autocompleteSuggestions.Count > 0)
                {
                    AutocompleteListView.Visibility = Visibility.Visible;
                }
                UpdateFilteredItems(ViewModel.Items.Where(i => MatchesSearch(i, input)).ToList());
                return;
            }

            var filteredItems = ViewModel.Items.Where(i => MatchesSearch(i, input)).ToList();

            var suggestions = filteredItems
                .Select(i => i.LocalizedName)
                .Distinct()
                .Take(10)
                .ToList();

            _lastSearchText = input;
            _autocompleteSuggestions.Clear();
            foreach (var suggestion in suggestions)
            {
                _autocompleteSuggestions.Add(suggestion);
            }

            AutocompleteListView.Visibility = suggestions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

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

        private void AutocompleteListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AutocompleteListView.SelectedItem != null)
            {
                string selected = AutocompleteListView.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selected))
                {
                    SearchBox.Text = selected;
                    SearchBox.SelectionStart = SearchBox.Text.Length; // Перемещаем курсор в конец
                    AutocompleteListView.Visibility = Visibility.Collapsed;
                    _autocompleteSuggestions.Clear();
                    // Обновляем фильтр для отображения только выбранного элемента (или всех совпадений)
                    var filteredItems = ViewModel.Items.Where(i => i.LocalizedName.Contains(selected, StringComparison.OrdinalIgnoreCase)).ToList();
                    UpdateFilteredItems(filteredItems);
                }
            }
        }

        private void SearchBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Down && _autocompleteSuggestions.Count > 0)
            {
                if (AutocompleteListView.Visibility != Visibility.Visible)
                {
                    AutocompleteListView.Visibility = Visibility.Visible;
                }
                AutocompleteListView.Focus(FocusState.Programmatic);
                if (AutocompleteListView.Items.Count > 0)
                {
                    AutocompleteListView.SelectedIndex = 0;
                }
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                AutocompleteListView.Visibility = Visibility.Collapsed;
                _autocompleteSuggestions.Clear();
                e.Handled = true;
            }
        }

        private void AutocompleteListView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                AutocompleteListView_SelectionChanged(sender, null);
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                AutocompleteListView.Visibility = Visibility.Collapsed;
                _autocompleteSuggestions.Clear();
                SearchBox.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Up && AutocompleteListView.SelectedIndex == 0)
            {
                AutocompleteListView.Visibility = Visibility.Collapsed;
                SearchBox.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private void AutocompleteListView_LostFocus(object sender, RoutedEventArgs e)
        {
            // Используем Dispatcher для отложенного вызова, чтобы проверить, куда перешел фокус
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(10); // Небольшая задержка, чтобы фокус успел перейти
                var focusedElement = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement() as FrameworkElement;
                if (focusedElement != SearchBox && focusedElement != AutocompleteListView)
                {
                    AutocompleteListView.Visibility = Visibility.Collapsed;
                    _autocompleteSuggestions.Clear();
                }
            });
        }

        public async Task RunItemDemo()
        {
            AppendToConsole("Запуск демонстрации предметов...");
            var iconExtractor = new ItemFluidWithIconExtractor();
            if (!iconExtractor.IsValid())
            {
                AppendToConsole("Путь к иконкам недействителен.");
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

            AppendToConsole($"Загружено {_itemDatabase.Count} элементов.\n");

            AppendToConsole($"=== Первые 10 элементов из базы данных ===");
            int shown = 0;
            foreach (var kvp in _itemDatabase)
            {
                if (shown >= 10) break;

                AppendToConsole($"ID: {kvp.Key}");
                AppendToConsole($"  Название: {kvp.Value.LocalizedName}");
                AppendToConsole($"  Мод: {kvp.Value.ModName}");
                AppendToConsole($"  Тип: {kvp.Value.Type}");
                AppendToConsole($"  Теги: {string.Join(", ", kvp.Value.Tags)}");
                AppendToConsole($"  Размер изображения: {kvp.Value.ImageData.Length} байт");
                AppendToConsole("");

                shown++;
            }

            var recipeExtractor = new RecipesExtractor();
            if (!recipeExtractor.IsValid())
            {
                AppendToConsole("Путь к рецептам недействителен.");
                return;
            }
            var _recipeDatabase = (await recipeExtractor.ExtractAsync()).ToList();
            
            UpdateItems(_itemDatabase.Values.ToList());
            UpdateRecipes(_recipeDatabase);
        }
    }
}
