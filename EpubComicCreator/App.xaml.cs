using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using EpubComicCreator.Views.Windows;
using EpubComicCreator.ViewModel;
using EpubComicCreator.Views.Pages;

namespace EpubComicCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public HomePageViewModel homePageViewMode;
        public EBookSetting settingViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            homePageViewMode = new HomePageViewModel();
            settingViewModel = new EBookSetting();
            settingViewModel.LoadSetting();
            homePageViewMode.LoadSetting();

            var mainWindow = new ComicBook();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            homePageViewMode.SaveSetting();
            settingViewModel.SaveSetting();
        }
    }
}
