using KubeAutomation.GenerationStrategies.Generators;
using LogExtractorLibrary;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _recipes = new();
        public ObservableCollection<string> Recipes
        {
            get => _recipes;
            set
            {
                _recipes = value;
                OnPropertyChanged();
            }
        }

        // --- MinecraftStandardRecipe panel ---

        public ObservableCollection<string> RecipeBlocks { get; } =
            new(MinecraftStandartRecipesGenerator.Blocks);

        private string _selectedBlock = MinecraftStandartRecipesGenerator.Blocks[0];
        public string SelectedBlock
        {
            get => _selectedBlock;
            set
            {
                _selectedBlock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSmokingSelected));
            }
        }

        public bool IsSmokingSelected => _selectedBlock == "smoking";

        private MinecraftItemViewModel? _inputItem;
        public MinecraftItemViewModel? InputItem
        {
            get => _inputItem;
            set
            {
                _inputItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InputItemId));
            }
        }

        public string InputItemId => _inputItem?.Id ?? "не выбран";

        private MinecraftItemViewModel? _outputItem;
        public MinecraftItemViewModel? OutputItem
        {
            get => _outputItem;
            set
            {
                _outputItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OutputItemId));
            }
        }

        public string OutputItemId => _outputItem?.Id ?? "не выбран";

        private double _outputAmount = 1;
        public double OutputAmount
        {
            get => _outputAmount;
            set { _outputAmount = value; OnPropertyChanged(); }
        }

        private double _experience = 0.1;
        public double Experience
        {
            get => _experience;
            set { _experience = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
            private set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        private readonly byte[] _imageData;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
                // Декодируем и масштабируем — всё в UI-потоке (BitmapDecoder требует STA)
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

                // Масштабирование nearest-neighbor в фоне
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
}