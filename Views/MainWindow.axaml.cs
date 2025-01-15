using Avalonia;
using Avalonia.Controls;

namespace CodeChordCatcher.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            InitView();
        }
        private void InitView()
        {
            Width = MinWidth = MaxWidth = 800;
            Height = MinHeight = MaxHeight = 450;
        }
    }
}