using Microsoft.UI.Xaml;

namespace KubeAutomation
{
    public partial class App : Application
    {
        private WinUIWindow? _mainWindow;
        public App()
        {
            this.InitializeComponent();
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Создаем и активируем главное окно WinUI
            _mainWindow = new WinUIWindow();
            _mainWindow.Activate();

            // Запускаем основную логику в фоне
            _ = MainLogic.StartMainLogicAsync(_mainWindow);
        }
    }
}