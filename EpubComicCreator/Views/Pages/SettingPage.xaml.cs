using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace EpubComicCreator.Views.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        public SettingPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).settingViewModel;
        }

        private void DeviceOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StackPanel stackPanel = FindVisualChild<StackPanel>(this, "CustomProfile");
            if (stackPanel != null)
            {
                if (DeviceOption.SelectedIndex == DeviceOption.Items.Count - 1)
                {
                    stackPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    stackPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null)
                {
                    if (child is T typedChild && (string)child.GetValue(FrameworkElement.NameProperty) == name)
                    {
                        return typedChild;
                    }
                    else
                    {
                        var result = FindVisualChild<T>(child, name);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DockPanel dockPanel = FindVisualChild<DockPanel>(this, "OutSplitNumBox");
            if (dockPanel != null)
            {
                if (OutSplitOption.SelectedIndex == OutSplitOption.Items.Count - 1)
                {
                    dockPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    dockPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void NumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            NumberBox numberBox = (NumberBox)sender;

            if (string.IsNullOrEmpty(numberBox.Text))
            {
                numberBox.Text = numberBox.Minimum.ToString();
            }
        }
    }
}
