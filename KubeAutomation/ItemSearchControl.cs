using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Threading;
//using System.Windows.Forms;
using System.Windows.Forms;

namespace KubeAutomation
{
    public partial class ItemSearchControl : UserControl
    {
        private TextBox searchBox;
        private ListBox autocompleteListBox;
        private bool listBoxVisible = false;

        // Свойства для хранения данных
        private List<string> items = new List<string>();
        // Кэшируем изображения как Image объекты, а не byte[]
        private Dictionary<string, Image> itemImages = new Dictionary<string, Image>();
        private Dictionary<string, Image> fluidImages = new Dictionary<string, Image>();

        // Событие, которое будет вызываться при выборе элемента
        public event Action<string> ItemSelected;

        // --- Добавляем поля для оптимизации ---
        private System.Windows.Forms.Timer searchTimer; // Для Debouncing
        private string lastSearchText = ""; // Для отслеживания последнего поискового запроса
        private List<string> lastMatches = new List<string>(); // Для отслеживания последних совпадений

        public ItemSearchControl()
        {
            InitializeComponent();
            SetupControls();
        }

        private void SetupControls()
        {
            searchBox = new TextBox
            {
                Name = "searchBox",
                Dock = DockStyle.Fill,
                TabStop = true
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            searchBox.KeyDown += SearchBox_KeyDown;
            searchBox.Leave += SearchBox_Leave;

            autocompleteListBox = new ListBox
            {
                Name = "autocompleteListBox",
                DrawMode = DrawMode.OwnerDrawFixed,
                Height = 200,
                ItemHeight = 20,
                ScrollAlwaysVisible = true,
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle,
                TabStop = false,
                Width = this.Width
            };
            autocompleteListBox.DrawItem += AutocompleteListBox_DrawItem;
            autocompleteListBox.Click += AutocompleteListBox_Click;
            autocompleteListBox.KeyDown += AutocompleteListBox_KeyDown;
            autocompleteListBox.Leave += AutocompleteListBox_Leave;

            this.Controls.Add(searchBox);

            this.Height = searchBox.Height + 3;

            // --- Инициализируем Timer для Debouncing ---
            searchTimer = new System.Windows.Forms.Timer
            {
                Interval = 100 // 200 миллисекунд
            };
            searchTimer.Tick += (s, e) =>
            {
                searchTimer.Stop(); // Останавливаем таймер
                PerformSearch(); // Выполняем поиск
            };
            // --- Конец инициализации Timer ---
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (this.TopLevelControl is Form parentForm)
            {
                if (!parentForm.Controls.Contains(autocompleteListBox))
                {
                    parentForm.Controls.Add(autocompleteListBox);
                    autocompleteListBox.BringToFront();
                    autocompleteListBox.Visible = false;
                }
            }
        }

        // --- Обновлённый Dispose ---
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Останавливаем таймер
                searchTimer?.Stop();
                searchTimer?.Dispose();

                // Убираем ListBox из формы
                if (this.TopLevelControl is Form parentForm && parentForm.Controls.Contains(autocompleteListBox))
                {
                    parentForm.Controls.Remove(autocompleteListBox);
                    // Освобождаем кэшированные изображения
                    foreach (var img in itemImages.Values) { img?.Dispose(); }
                    foreach (var img in fluidImages.Values) { img?.Dispose(); }
                    itemImages.Clear();
                    fluidImages.Clear();
                    autocompleteListBox?.Dispose();
                }
                // Освобождаем ресурсы, созданные через Designer
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        // --- Новый метод для выполнения поиска ---
        private void PerformSearch()
        {
            string input = searchBox.Text;

            if (string.IsNullOrEmpty(input))
            {
                HideListBox();
                lastSearchText = "";
                lastMatches.Clear();
                return;
            }

            // Проверяем, отличается ли текст от последнего поиска
            if (input == lastSearchText)
            {
                // Если текст не изменился, не пересчитываем
                // Но возможно, нужно обновить видимость/позицию
                if (!listBoxVisible && lastMatches.Any())
                {
                    ShowListBox(lastMatches);
                }
                else if (listBoxVisible && !lastMatches.Any())
                {
                    HideListBox();
                }
                else if (listBoxVisible)
                {
                    // ListBox уже виден, но возможно, элементы изменились снаружи
                    // Проверим, совпадают ли текущие Items с lastMatches
                    if (autocompleteListBox.Items.Count != lastMatches.Count ||
                        !lastMatches.SequenceEqual(autocompleteListBox.Items.Cast<string>()))
                    {
                        UpdateListBoxItems(lastMatches);
                    }
                    // Обновляем позицию, если UserControl переместился
                    UpdateListBoxPosition();
                }
                return;
            }

            // Выполняем поиск
            var matches = items.Where(x => x.Contains(input, StringComparison.OrdinalIgnoreCase)).ToList();

            lastSearchText = input;
            lastMatches = matches; // Сохраняем результаты

            if (matches.Any())
            {
                ShowListBox(matches);
            }
            else
            {
                HideListBox();
            }
        }

        // --- Новый метод для отображения ListBox ---
        private void ShowListBox(List<string> matches)
        {
            if (!listBoxVisible)
            {
                UpdateListBoxPosition(); // Обновляем позицию при показе
                autocompleteListBox.BringToFront(); // Убедимся, что поверх
            }
            UpdateListBoxItems(matches);
            autocompleteListBox.Visible = true;
            listBoxVisible = true;
        }

        // --- Новый метод для скрытия ListBox ---
        private void HideListBox()
        {
            if (listBoxVisible)
            {
                autocompleteListBox.Visible = false;
                listBoxVisible = false;
            }
            // Сбрасываем последние совпадения при скрытии
            lastMatches.Clear();
            lastSearchText = "";
        }

        // --- Новый метод для обновления Items в ListBox ---
        private void UpdateListBoxItems(List<string> matches)
        {
            autocompleteListBox.BeginUpdate(); // Приостанавливаем отрисовку
            try
            {
                autocompleteListBox.Items.Clear();
                foreach (var match in matches)
                {
                    autocompleteListBox.Items.Add(match);
                }
            }
            finally
            {
                autocompleteListBox.EndUpdate(); // Возобновляем отрисовку и перерисовываем
            }
        }


        private void SearchBox_TextChanged(object? sender, EventArgs e)
        {
            // --- Вместо выполнения поиска сразу ---
            // string input = searchBox.Text;
            // ... (старая логика поиска)

            // --- Запускаем таймер для Debouncing ---
            searchTimer.Stop(); // Сбрасываем таймер при каждом изменении
            searchTimer.Start(); // Запускаем его заново
            // --- Конец замены ---
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (listBoxVisible)
            {
                UpdateListBoxPosition();
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (listBoxVisible)
            {
                UpdateListBoxPosition();
            }
        }

        private void UpdateListBoxPosition()
        {
            if (this.TopLevelControl is Form parentForm)
            {
                Point controlLocationOnForm = this.PointToScreen(Point.Empty);
                controlLocationOnForm = parentForm.PointToClient(controlLocationOnForm);

                autocompleteListBox.Location = new Point(controlLocationOnForm.X, controlLocationOnForm.Y + this.Height);
                autocompleteListBox.Width = this.Width;
            }
        }

        private void AutocompleteListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            string itemText = autocompleteListBox.Items[e.Index].ToString() ?? "";

            // --- Используем кэшированные Image объекты ---
            Image? img = null;
            if (itemImages.TryGetValue(itemText, out var cachedItemImage))
            {
                img = cachedItemImage;
            }
            else if (fluidImages.TryGetValue(itemText, out var cachedFluidImage))
            {
                img = cachedFluidImage;
            }
            // --- Конец изменения ---

            if (img != null)
            {
                int imgWidth = 16;
                int imgHeight = 16;
                int margin = 2;

                e.Graphics.DrawImage(img, e.Bounds.Left + margin, e.Bounds.Top + (e.Bounds.Height - imgHeight) / 2, imgWidth, imgHeight);
                e.Graphics.DrawString(itemText, e.Font, SystemBrushes.WindowText, e.Bounds.Left + imgWidth + margin * 2, e.Bounds.Top + (e.Bounds.Height - e.Font.Height) / 2);
            }
            else
            {
                e.Graphics.DrawString(itemText, e.Font, SystemBrushes.WindowText, e.Bounds.Left, e.Bounds.Top + (e.Bounds.Height - e.Font.Height) / 2);
            }

            e.DrawFocusRectangle();
        }

        private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && autocompleteListBox.Visible && autocompleteListBox.Items.Count > 0)
            {
                autocompleteListBox.Focus();
                autocompleteListBox.SelectedIndex = 0;
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Escape && listBoxVisible)
            {
                HideListBox();
                // Сбрасываем таймер, если Escape нажат во время ожидания
                searchTimer.Stop();
                lastSearchText = searchBox.Text; // Обновляем lastSearchText, чтобы избежать повторного поиска при отпускании клавиши
                e.Handled = true;
            }
        }

        private void AutocompleteListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && autocompleteListBox.SelectedIndex >= 0)
            {
                SelectItem();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideListBox();
                searchTimer.Stop(); // Сбрасываем таймер
                lastSearchText = searchBox.Text; // Обновляем lastSearchText
                searchBox.Focus();
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Up && autocompleteListBox.SelectedIndex == 0)
            {
                searchBox.Focus();
                e.Handled = true;
            }
        }

        private void AutocompleteListBox_Click(object? sender, EventArgs e)
        {
            SelectItem();
        }

        private void SearchBox_Leave(object? sender, EventArgs e)
        {
            if (!autocompleteListBox.Focused && !autocompleteListBox.ContainsFocus)
            {
                HideListBox();
                searchTimer.Stop(); // Сбрасываем таймер при потере фокуса
            }
        }

        private void AutocompleteListBox_Leave(object? sender, EventArgs e)
        {
            if (Form.ActiveForm != null)
            {
                Control focusedControl = Form.ActiveForm.ActiveControl;
                Control current = focusedControl;
                bool focusIsInsideOrOnControls = false;
                while (current != null)
                {
                    if (current == this || current == autocompleteListBox || current == searchBox)
                    {
                        focusIsInsideOrOnControls = true;
                        break;
                    }
                    current = current.Parent;
                }

                if (!focusIsInsideOrOnControls)
                {
                    HideListBox();
                    searchTimer.Stop(); // Сбрасываем таймер
                }
            }
        }

        private void SelectItem()
        {
            if (autocompleteListBox.SelectedItem != null)
            {
                string selected = autocompleteListBox.SelectedItem.ToString();
                if (selected != null)
                {
                    searchBox.Text = selected;
                }
                HideListBox();
                searchTimer.Stop(); // Сбрасываем таймер
                searchBox.Focus();

                ItemSelected?.Invoke(selected);
            }
        }

        // --- ОБНОВЛЁННЫЙ метод для обновления данных ---
        public void UpdateItemsAndImages(List<string> newItems, Dictionary<string, byte[]> newItemImages, Dictionary<string, byte[]> newFluidImages)
        {
            this.items = newItems ?? new List<string>();

            // --- Преобразуем byte[] в Image и кэшируем ---
            this.itemImages.Clear();
            if (newItemImages != null)
            {
                foreach (var kvp in newItemImages)
                {
                    try
                    {
                        using (var ms = new MemoryStream(kvp.Value))
                        {
                            // Создаем новый Image объект
                            var img = Image.FromStream(ms);
                            // Кэшируем его
                            this.itemImages[kvp.Key] = img;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибки
                        // Debug.WriteLine($"Error caching image for item '{kvp.Key}': {ex.Message}");
                    }
                }
            }

            this.fluidImages.Clear();
            if (newFluidImages != null)
            {
                foreach (var kvp in newFluidImages)
                {
                    try
                    {
                        using (var ms = new MemoryStream(kvp.Value))
                        {
                            var img = Image.FromStream(ms);
                            this.fluidImages[kvp.Key] = img;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибки
                        // Debug.WriteLine($"Error caching fluid image for item '{kvp.Key}': {ex.Message}");
                    }
                }
            }
            // --- Конец изменения ---

            // После обновления данных, возможно, нужно обновить отображение, если список открыт
            if (listBoxVisible && !string.IsNullOrEmpty(searchBox.Text))
            {
                // Перезапускаем поиск для текущего текста с новыми данными
                searchTimer.Stop();
                PerformSearch(); // Выполняем поиск сразу, так как данные обновились
            }
        }
    }
}