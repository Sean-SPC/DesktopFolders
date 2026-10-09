
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace DesktopFolders
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Remove the standard border and title bar.
            var presenter = OverlappedPresenter.Create();
            presenter.SetBorderAndTitleBar(
                hasBorder: false,
                hasTitleBar: false);

            // Apply the borderless window style.
            AppWindow.SetPresenter(presenter);

            // Maximize within the normal desktop work area.
            presenter.Maximize();

            // Load the desktop folder interface.
            RootFrame.Navigate(typeof(MainPage));
        }
    }
}