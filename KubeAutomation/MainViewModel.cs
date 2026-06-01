using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeAutomation.GenerationStrategies.RecipeConfigurations.Create;
using LogExtractorLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace KubeAutomation
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<MinecraftItemViewModel> _items = new();
        public ObservableCollection<MinecraftItemViewModel> Items
        {
            get => _items;
            set { _items = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _recipes = new();
        public ObservableCollection<string> Recipes
        {
            get => _recipes;
            set { _recipes = value; OnPropertyChanged(); }
        }

        // --- Recipe type selector (Minecraft + Create combined) ---

        public ObservableCollection<string> RecipeBlocks { get; } =
            new(RecipeTypeDescriptor.AllTypeNames);

        private string _selectedBlock = RecipeTypeDescriptor.AllTypeNames[0];
        public string SelectedBlock
        {
            get => _selectedBlock;
            set
            {
                _selectedBlock = value;
                OnPropertyChanged();
            }
        }

        // --- Unified dynamic slots ---

        public ObservableCollection<RecipeSlotViewModel> InputSlots { get; } = new();
        public ObservableCollection<RecipeSlotViewModel> OutputSlots { get; } = new();

        // 9 fixed slots for the workbench 3×3 grid (row-major: 0-2 = row1, 3-5 = row2, 6-8 = row3)
        public RecipeSlotViewModel[] WorkbenchSlots { get; } =
            System.Linq.Enumerable.Range(0, 9)
                .Select(i => new RecipeSlotViewModel(i, showChance: false, showAmount: false))
                .ToArray();

        private Visibility _showWorkbenchGrid = Visibility.Collapsed;
        public Visibility ShowWorkbenchGrid
        {
            get => _showWorkbenchGrid;
            private set { _showWorkbenchGrid = value; OnPropertyChanged(); }
        }

        private Visibility _showInputSlots = Visibility.Visible;
        public Visibility ShowInputSlots
        {
            get => _showInputSlots;
            private set { _showInputSlots = value; OnPropertyChanged(); }
        }

        private CreateRecipeConstraints? _currentConstraints;

        private bool _canAddInput;
        public bool CanAddInput
        {
            get => _canAddInput;
            private set { _canAddInput = value; OnPropertyChanged(); }
        }

        private bool _canRemoveInput;
        public bool CanRemoveInput
        {
            get => _canRemoveInput;
            private set { _canRemoveInput = value; OnPropertyChanged(); }
        }

        private bool _canAddOutput;
        public bool CanAddOutput
        {
            get => _canAddOutput;
            private set { _canAddOutput = value; OnPropertyChanged(); }
        }

        private bool _canRemoveOutput;
        public bool CanRemoveOutput
        {
            get => _canRemoveOutput;
            private set { _canRemoveOutput = value; OnPropertyChanged(); }
        }

        // --- Visibility of +/- buttons ---

        private Visibility _showInputButtons = Visibility.Collapsed;
        public Visibility ShowInputButtons
        {
            get => _showInputButtons;
            private set { _showInputButtons = value; OnPropertyChanged(); }
        }

        private Visibility _showOutputButtons = Visibility.Collapsed;
        public Visibility ShowOutputButtons
        {
            get => _showOutputButtons;
            private set { _showOutputButtons = value; OnPropertyChanged(); }
        }

        // --- Modifiers ---

        private Visibility _showModifierHeated = Visibility.Collapsed;
        public Visibility ShowModifierHeated
        {
            get => _showModifierHeated;
            private set { _showModifierHeated = value; OnPropertyChanged(); }
        }

        private Visibility _showModifierProcessingTime = Visibility.Collapsed;
        public Visibility ShowModifierProcessingTime
        {
            get => _showModifierProcessingTime;
            private set { _showModifierProcessingTime = value; OnPropertyChanged(); }
        }

        private Visibility _showModifierKeepHeldItem = Visibility.Collapsed;
        public Visibility ShowModifierKeepHeldItem
        {
            get => _showModifierKeepHeldItem;
            private set { _showModifierKeepHeldItem = value; OnPropertyChanged(); }
        }

        private Visibility _showModifierExperience = Visibility.Collapsed;
        public Visibility ShowModifierExperience
        {
            get => _showModifierExperience;
            private set { _showModifierExperience = value; OnPropertyChanged(); }
        }

        private string _selectedHeat = "none";
        public string SelectedHeat
        {
            get => _selectedHeat;
            set { _selectedHeat = value; OnPropertyChanged(); }
        }

        private double _processingTime = 100;
        public double ProcessingTime
        {
            get => _processingTime;
            set { _processingTime = value; OnPropertyChanged(); }
        }

        private bool _keepHeldItem;
        public bool KeepHeldItem
        {
            get => _keepHeldItem;
            set { _keepHeldItem = value; OnPropertyChanged(); }
        }

        private double _experience = 0.1;
        public double Experience
        {
            get => _experience;
            set { _experience = value; OnPropertyChanged(); }
        }

        public void ApplyRecipeType(string typeName)
        {
            var constraints = RecipeTypeDescriptor.GetConstraints(typeName);
            _currentConstraints = constraints;

            if (constraints == null) return;

            // Workbench uses the 3×3 grid — hide flat input list
            if (typeName.Equals("workbench", StringComparison.OrdinalIgnoreCase))
            {
                ShowWorkbenchGrid = Visibility.Visible;
                ShowInputSlots    = Visibility.Collapsed;
                ShowInputButtons  = Visibility.Collapsed;
                InputSlots.Clear();
                ClearWorkbenchSlots();
            }
            else
            {
                ShowWorkbenchGrid = Visibility.Collapsed;
                ShowInputSlots    = Visibility.Visible;

                int minIn = constraints.FixedInputCount ?? constraints.MinInputs;
                InputSlots.Clear();
                for (int i = 0; i < minIn; i++)
                    InputSlots.Add(new RecipeSlotViewModel(i, showChance: false, showAmount: false));

                ShowInputButtons = constraints.FixedInputCount.HasValue ? Visibility.Collapsed : Visibility.Visible;
            }

            int minOut = constraints.FixedOutputCount ?? constraints.MinOutputs;
            OutputSlots.Clear();
            for (int i = 0; i < minOut; i++)
                OutputSlots.Add(new RecipeSlotViewModel(i,
                    showChance: constraints.AllowChanceOutputs,
                    showAmount: constraints.ShowOutputAmount));

            RefreshSlotButtons(constraints);

            ShowOutputButtons = constraints.FixedOutputCount.HasValue ? Visibility.Collapsed : Visibility.Visible;

            // Modifiers
            bool hasHeated = constraints.SupportedModifiers.Contains(CreateModifierType.Heated)
                          || constraints.SupportedModifiers.Contains(CreateModifierType.Superheated);
            ShowModifierHeated = hasHeated ? Visibility.Visible : Visibility.Collapsed;
            ShowModifierProcessingTime = constraints.SupportedModifiers.Contains(CreateModifierType.ProcessingTime)
                ? Visibility.Visible : Visibility.Collapsed;
            ShowModifierKeepHeldItem = constraints.SupportedModifiers.Contains(CreateModifierType.KeepHeldItem)
                ? Visibility.Visible : Visibility.Collapsed;
            ShowModifierExperience = constraints.SupportedModifiers.Contains(CreateModifierType.Experience)
                ? Visibility.Visible : Visibility.Collapsed;

            SelectedHeat = "none";
            KeepHeldItem = false;
        }

        public void ClearWorkbenchSlots()
        {
            foreach (var slot in WorkbenchSlots)
                slot.Item = null;
        }

        public void AddInputSlot()
        {
            if (_currentConstraints == null) return;
            InputSlots.Add(new RecipeSlotViewModel(InputSlots.Count, showChance: false, showAmount: false));
            RefreshSlotButtons(_currentConstraints);
        }

        public void RemoveInputSlot()
        {
            if (_currentConstraints == null || InputSlots.Count == 0) return;
            InputSlots.RemoveAt(InputSlots.Count - 1);
            RefreshSlotButtons(_currentConstraints);
        }

        public void AddOutputSlot()
        {
            if (_currentConstraints == null) return;
            OutputSlots.Add(new RecipeSlotViewModel(OutputSlots.Count,
                showChance: _currentConstraints.AllowChanceOutputs,
                showAmount: _currentConstraints.ShowOutputAmount));
            RefreshSlotButtons(_currentConstraints);
        }

        public void RemoveOutputSlot()
        {
            if (_currentConstraints == null || OutputSlots.Count == 0) return;
            OutputSlots.RemoveAt(OutputSlots.Count - 1);
            RefreshSlotButtons(_currentConstraints);
        }

        private void RefreshSlotButtons(CreateRecipeConstraints c)
        {
            int inCount = InputSlots.Count;
            int outCount = OutputSlots.Count;

            bool inputFixed = c.FixedInputCount.HasValue;
            bool outputFixed = c.FixedOutputCount.HasValue;

            CanAddInput    = !inputFixed  && (!c.MaxInputs.HasValue  || inCount  < c.MaxInputs.Value);
            CanRemoveInput = !inputFixed  && inCount  > c.MinInputs;
            CanAddOutput   = !outputFixed && (!c.MaxOutputs.HasValue || outCount < c.MaxOutputs.Value);
            CanRemoveOutput= !outputFixed && outCount > c.MinOutputs;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // --- Removal tab ---

        public RecipeSlotViewModel RemovalOutputSlot { get; } = new(0, showChance: false, showAmount: false);
        public RecipeSlotViewModel RemovalInputSlot { get; } = new(1, showChance: false, showAmount: false);

        public ObservableCollection<string> RemoveTypeOptions { get; } =
            new(RecipeTypeDescriptor.AllTypeNames);

        public ObservableCollection<string> RemoveModOptions { get; } = new();

        public void RefreshRemoveModOptions()
        {
            var mods = Items.Select(i => i.ModName).Distinct().OrderBy(m => m).ToList();
            RemoveModOptions.Clear();
            foreach (var mod in mods)
                RemoveModOptions.Add(mod);
        }

        public RecipeFileViewModel RecipeFile { get; } = new();

        // --- Edit mode ---

        private RecipeEntryViewModel? _editingEntry;
        public RecipeEntryViewModel? EditingEntry
        {
            get => _editingEntry;
            set
            {
                _editingEntry = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(CancelEditVisibility));
                OnPropertyChanged(nameof(FileNameVisibility));
                OnPropertyChanged(nameof(CreateButtonLabel));
            }
        }

        public bool IsEditMode => _editingEntry != null;
        public Visibility CancelEditVisibility => _editingEntry != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility FileNameVisibility => _editingEntry == null ? Visibility.Visible : Visibility.Collapsed;
        public string CreateButtonLabel => IsEditMode ? "Сохранить изменения" : "Создать рецепт";
    }

    public class RecipeSlotViewModel : INotifyPropertyChanged
    {
        public int SlotIndex { get; }
        public bool ShowChance { get; }
        public bool ShowAmount { get; }

        private MinecraftItemViewModel? _item;
        public MinecraftItemViewModel? Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemId));
                OnPropertyChanged(nameof(PlaceholderVisibility));
                OnPropertyChanged(nameof(ImageVisibility));
            }
        }

        public string ItemId => _item?.Id ?? "не выбран";
        public Visibility PlaceholderVisibility => _item == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ImageVisibility => _item != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ChanceVisibility => ShowChance ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AmountVisibility => ShowAmount ? Visibility.Visible : Visibility.Collapsed;

        private double _chance = 1.0;
        public double Chance
        {
            get => _chance;
            set { _chance = value; OnPropertyChanged(); }
        }

        private double _amount = 1;
        public double Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        public RecipeSlotViewModel(int index, bool showChance, bool showAmount = false)
        {
            SlotIndex = index;
            ShowChance = showChance;
            ShowAmount = showAmount;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MinecraftItemViewModel : INotifyPropertyChanged
    {
        public string Id { get; }
        public string LocalizedName { get; }
        public string ModName { get; }
        public string Type { get; }
        public string TagsDisplay { get; }

        private ImageSource? _image;
        public ImageSource? Image
        {
            get => _image;
            private set { _image = value; OnPropertyChanged(); }
        }

        private readonly byte[] _imageData;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MinecraftItemViewModel(MinecraftItem item)
        {
            Id = item.Id;
            LocalizedName = item.LocalizedName;
            ModName = item.ModName;
            Type = item.Type;
            TagsDisplay = string.Join(", ", item.Tags);
            _imageData = item.ImageData;
        }

        public async Task LoadImageAsync(int targetSize = 64)
        {
            if (_imageData.Length == 0) return;
            try
            {
                using var ms = new System.IO.MemoryStream(_imageData);
                var ras = ms.AsRandomAccessStream();
                var decoder = await BitmapDecoder.CreateAsync(ras);
                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    new BitmapTransform(),
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage);

                var srcPixels = pixelData.DetachPixelData();
                int srcW = (int)decoder.PixelWidth;
                int srcH = (int)decoder.PixelHeight;

                var dstPixels = await Task.Run(() => ScaleNearestNeighbor(srcPixels, srcW, srcH, targetSize));

                var result = new WriteableBitmap(targetSize, targetSize);
                using var stream = result.PixelBuffer.AsStream();
                await stream.WriteAsync(dstPixels, 0, dstPixels.Length);
                result.Invalidate();
                Image = result;
            }
            catch
            {
                Image = null;
            }
        }

        private static byte[] ScaleNearestNeighbor(byte[] srcPixels, int srcW, int srcH, int targetSize)
        {
            var dst = new byte[targetSize * targetSize * 4];
            for (int y = 0; y < targetSize; y++)
            {
                int srcY = y * srcH / targetSize;
                for (int x = 0; x < targetSize; x++)
                {
                    int srcX = x * srcW / targetSize;
                    int srcIdx = (srcY * srcW + srcX) * 4;
                    int dstIdx = (y * targetSize + x) * 4;
                    dst[dstIdx]     = srcPixels[srcIdx];
                    dst[dstIdx + 1] = srcPixels[srcIdx + 1];
                    dst[dstIdx + 2] = srcPixels[srcIdx + 2];
                    dst[dstIdx + 3] = srcPixels[srcIdx + 3];
                }
            }
            return dst;
        }
    }

    public class RecipeEntryViewModel : INotifyPropertyChanged
    {
        public BaseRecipeConfiguration Config { get; }

        public string DisplayName => Config switch
        {
            MinecraftStandardRecipeConfig mc  => $"{mc.Type.ToString().ToLower()}: {mc.Input.ItemId} → {mc.Output.ToJsString()}",
            WorkbenchRecipeConfiguration wb   => $"shaped: {wb.itemOutput?.ItemId ?? "?"} ×{wb.itemOutput?.Amount ?? 1}",
            RecipeRemovalConfig rem           => $"remove: {DescribeFilters(rem)}",
            RecipeModificationConfig mod      => $"{mod.Type.ToString().ToLower()}: {mod.IngredientToReplace} → {mod.ReplacementIngredient}",
            BaseCreateRecipeConfig cr         => $"create.{cr.RecipeTypeId.Replace("create_", "")}: {DescribeCreateRecipe(cr)}",
            RawCodeBlock raw                  => $"raw ({raw.Reason})",
            _                                => Config.RecipeTypeId,
        };

        public bool CanDelete => Config.HasSourcePosition;
        public bool CanInsert => Config.HasSourcePosition;

        public RecipeEntryViewModel(BaseRecipeConfiguration config)
        {
            Config = config;
        }

        private static string DescribeFilters(RecipeRemovalConfig rem)
        {
            if (rem.Filters.Count == 0) return "—";
            var f = rem.Filters[0];
            return f.Output ?? f.Input ?? f.Type ?? f.Mod ?? f.Id ?? "{}";
        }

        private static string DescribeCreateRecipe(BaseCreateRecipeConfig cr)
        {
            var inputs  = string.Join(", ", cr.Inputs.Select(i => i.ToJsString()));
            var outputs = string.Join(", ", cr.Outputs.Select(o => o.ToJsString()));
            return $"{inputs} → {outputs}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RecipeFileViewModel : INotifyPropertyChanged
    {
        private string _filePath = "";
        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public int PendingInsertPosition { get; set; } = -1;

        public ObservableCollection<RecipeEntryViewModel> Entries { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
