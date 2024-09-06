using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using EpubComicCreator.ViewModel;
namespace EpubComicCreator.Views.Pages
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).homePageViewMode;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 检查按下的键是否为 Enter 键
            if (e.Key == Key.Enter)
            {
                // 清除焦点，取消文本框的选中状态
                Keyboard.ClearFocus();
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);
                e.Handled = true;
            }
        }

        private void TaskTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TaskTextBox.ScrollToEnd();
        }
    }
}
