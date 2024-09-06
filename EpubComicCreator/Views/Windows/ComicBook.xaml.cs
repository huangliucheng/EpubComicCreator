using EpubComicCreator.Views.Pages;
using Wpf.Ui.Controls;


namespace EpubComicCreator.Views.Windows
{
    /// <summary>
    /// ComicBook.xaml 的交互逻辑
    /// </summary>
    public partial class ComicBook : FluentWindow
    {
        public ComicBook()
        {
            InitializeComponent();
            Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));
        }
    }
}
