using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using EpubComicCreator.Models;
using EpubComicCreator.Models.DataStructure;
using EpubComicCreator.Models.Tools;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;

namespace EpubComicCreator.ViewModel
{
    public partial class HomePageViewModel : ObservableRecipient
    {
        // 构造函数
        public HomePageViewModel()
        {
            // 初始化
            _comicTitle = "";
            _inputPath = "";
            _savePath = "";
            _booksBasicProperties = [];
            bookStatus = [];
            StartButtonEnable = true;
            StopButtonEnable = false;
            WeakReferenceMessenger.Default.Register<string, string>(this, MessageToken.ProgramMessage, HandleReceivedMessage);
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(
                this, 
                MessageToken.HomePage,
                (r, message) => OutSplit = message.Value);
        }

        private void HandleReceivedMessage(object recipient, string message)
        {
            // 将新消息添加到文本框的最后一行
            _receivedMessage.AppendLine(message);
            OnPropertyChanged(nameof(Message));
        }

        // HomePage 基本属性
        private string _inputPath;
        private string _lastInputPath;
        private string _savePath;
        private string _comicTitle;
        private List<BookBasicProperties> _booksBasicProperties;
        private ObservableCollection<BookStatus> _books;
        private readonly StringBuilder _receivedMessage = new();
        private bool _outSplit;
        private bool OutSplit
        {
            get => _outSplit;
            set
            {
                if (_outSplit != value)
                {
                    _outSplit = value;
                    if (!string.IsNullOrEmpty(_inputPath))
                    {
                        BookList(_inputPath, _comicTitle, _outSplit);
                    }
                }
            }
        }
        private TreeNode ComicFileStructure;
        private CancellationTokenSource _cancellationTokenSource;

        // 生成电子书的状态列表
        public ObservableCollection<BookStatus> bookStatus
        {
            get { return _books; }
            set
            {
                if (_books != value)
                {
                    _books = value;
                    OnPropertyChanged(nameof(bookStatus));
                }
            }
        }
        // 漫画标题
        public string ComicTitle
        {
            get { return _comicTitle; }
            set
            {
                // 如果漫画标题发生改变, 则修改所有待生成电子书的标题
                if (_comicTitle != value)
                {
                    _comicTitle = value;
                    OnPropertyChanged(nameof(ComicTitle));
                    if (bookStatus.Count > 0)
                    {
                        for (int i = 0; i < bookStatus.Count; i++)
                        {
                            bookStatus[i].Name = _comicTitle + $"_{i+1:D2}";
                            _booksBasicProperties[i].Title = bookStatus[i].Name;
                        }
                    }
                }
            }
        }
        // 用户输入路径
        public string InputPath
        {
            get { return _inputPath; }
            set
            {
                if (_inputPath != value)
                {
                    _inputPath = value;
                    _lastInputPath = _inputPath;
                    OnPropertyChanged(nameof(InputPath));
                    BookList(_inputPath);

                    if (!string.IsNullOrEmpty(Path.GetDirectoryName(_inputPath)))
                    {
                        SavePath = Path.GetDirectoryName(_inputPath);
                    }
                    else
                    {
                        SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }

                    OnPropertyChanged(nameof(SavePath));
                    ComicTitle = Path.GetFileName(_inputPath);
                    OnPropertyChanged(nameof(ComicTitle));

                    ComicFileStructure = FileTools.BuildTree(_inputPath);
                }
            }
        }
        // 保存路径
        public string SavePath
        {
            get { return _savePath; }
            set
            {
                if (_savePath != value)
                {
                    _savePath = value;
                    OnPropertyChanged(nameof(SavePath));
                    WeakReferenceMessenger.Default.Send("保存路径已改变");
                    for (int i = 0; i < _booksBasicProperties.Count; i++)
                    {
                        _booksBasicProperties[i].SavePath = _savePath;
                    }
                }
            }
        }
        // 开始按钮的可见性
        public bool StartButtonEnable { get; set; }
        // 终止按钮的可见性
        public bool StopButtonEnable { get; set; }
        // 文本框内的消息
        public string Message
        {
            get { return _receivedMessage.ToString(); }
        }

        /// <summary>
        /// HomePageViewModel 的命令
        /// </summary>
        [RelayCommand]
        public void OpenInputFolder()
        {
            if (string.IsNullOrEmpty(_lastInputPath))
            {
                _lastInputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            OpenFolderDialog openFolderDialog =
                new()
                {
                    Multiselect = false,
                    InitialDirectory = _lastInputPath
                };

            if (openFolderDialog.ShowDialog() == false) return;
            if (openFolderDialog.FolderNames.Length == 0) return;

            InputPath = openFolderDialog.FolderNames[0];
            _lastInputPath = InputPath;
        }

        [RelayCommand]
        public void OpenSaveFolder()
        {
            if (string.IsNullOrEmpty(_savePath))
            {
                _savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            OpenFolderDialog openFolderDialog =
                new()
                {
                    Multiselect = false,
                    InitialDirectory = _savePath
                };

            if (openFolderDialog.ShowDialog() == false) return;
            if (openFolderDialog.FolderNames.Length == 0) return;

            SavePath = openFolderDialog.FolderNames[0];
        }

        private Task _currentTask;

        [RelayCommand]
        public async Task StartProgram()
        {
            StartButtonEnable = false;
            StopButtonEnable = true;
            OnPropertyChanged(nameof(StartButtonEnable));
            OnPropertyChanged(nameof(StopButtonEnable));

            if (string.IsNullOrEmpty(InputPath))
            {
                WeakReferenceMessenger.Default.Send("输入路径为空!", MessageToken.ProgramMessage);
            }
            else
            {
                WeakReferenceMessenger.Default.Send("Visible", MessageToken.ImageBarVisibility);

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;
                try
                {
                    WeakReferenceMessenger.Default.Send("开始生成电子书", MessageToken.ProgramMessage);
                    var setting = ((App)Application.Current).settingViewModel;
                    SplitTreeNode(_booksBasicProperties, ComicFileStructure);
                    _currentTask = Task.Run(() => BookProgram.Start(_booksBasicProperties, setting, bookStatus, token), token);
                    await _currentTask;
                    WeakReferenceMessenger.Default.Send("任务已完成!", MessageToken.ProgramMessage);
                }
                catch
                {
                    _cancellationTokenSource.Cancel();
                    WeakReferenceMessenger.Default.Send("任务出错, 请重试!", MessageToken.ProgramMessage);
                    WeakReferenceMessenger.Default.Send(new BookProgressBarValue(0, 1));
                }
            }

            StartButtonEnable = true;
            StopButtonEnable = false;
            OnPropertyChanged(nameof(StartButtonEnable));
            OnPropertyChanged(nameof(StopButtonEnable));
            WeakReferenceMessenger.Default.Send("Hidden", MessageToken.ImageBarVisibility);
        }

        [RelayCommand]
        public async Task StopProgramAsync()
        {
            if (_cancellationTokenSource != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                _cancellationTokenSource.Token.Register(() => tcs.SetResult(true));
                _cancellationTokenSource.Cancel();
                WeakReferenceMessenger.Default.Send("正在取消任务!", MessageToken.ProgramMessage);

                // 等待任务完成
                await tcs.Task;
            }

            StopButtonEnable = false;
            StartButtonEnable = true;
            OnPropertyChanged(nameof(StopButtonEnable));
            OnPropertyChanged(nameof(StartButtonEnable));

            WeakReferenceMessenger.Default.Send("任务已取消!", MessageToken.ProgramMessage);
        }


        // 输入路径发生改变时, 待生成的电子书列表的生成方法
        private void BookList(string inputPath)
        {
            bookStatus = [];
            _booksBasicProperties = [];
            // 获取输入路径下的所有文件夹
            var booksPath = Directory.GetDirectories(inputPath);

            // 如果没有子文件夹, 则输出模式自动调整为整合模式
            if (booksPath.Length == 0)
            {
                OutSplit = false;
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(OutSplit), MessageToken.SettingPage);
                string bookname = Path.GetFileName(inputPath);
                bookStatus.Add(new BookStatus { Name=bookname, Status=""});
                _booksBasicProperties.Add(new BookBasicProperties(bookname));
                _booksBasicProperties[0].SavePath = SavePath;
            }
            // 如果有子文件夹, 根据输出模式生成电子书列表
            else
            {
                if (OutSplit)
                {
                    foreach (var childPath in booksPath)
                    {
                        string bookname = Path.GetFileName(childPath);
                        bookStatus.Add(new BookStatus { Name = bookname, Status = "" });
                        _booksBasicProperties.Add(new BookBasicProperties(bookname));
                        _booksBasicProperties[^1].SavePath = SavePath;
                    }
                }
                else
                {
                    string bookname = Path.GetFileName(inputPath);
                    bookStatus.Add(new BookStatus { Name = bookname, Status = "" });
                    _booksBasicProperties.Add(new BookBasicProperties(bookname));
                    _booksBasicProperties[0].SavePath = SavePath;
                }
            }
            WeakReferenceMessenger.Default.Send(new BookProgressBarValue(0, bookStatus.Count));
        }
        // 输出模式改变时, 待生成的电子书列表的生成方法
        private void BookList(string inputPath, string comicTitle, bool outSplit)
        {
            var booksPath = Directory.GetDirectories(inputPath);

            if (booksPath.Length == 0) return;
            else
            {
                bookStatus = [];
                _booksBasicProperties = [];
                if (outSplit)
                {
                    for (int i = 0; i < booksPath.Length; i++)
                    {
                        bookStatus.Add(new BookStatus { Name = comicTitle + $"_{(i+1):D2}", Status = "" });
                        _booksBasicProperties.Add(new BookBasicProperties(bookStatus[i].Name));
                        _booksBasicProperties[^1].SavePath = SavePath;
                    }
                }
                else
                {
                    bookStatus.Add(new BookStatus { Name = comicTitle + $"_{1:D2}", Status = "" });
                    _booksBasicProperties.Add(new BookBasicProperties(bookStatus[0].Name));
                    _booksBasicProperties[0].SavePath = SavePath;
                }
                OnPropertyChanged(nameof(bookStatus));
            }
        }

        // 根据输出模式分割文件结构节点
        private void SplitTreeNode(List<BookBasicProperties> bookProperties, TreeNode fileNode)
        {
            if (OutSplit)
            {
                for (int i = 0; i < bookProperties.Count; i++)
                {
                    bookProperties[i].BookStructure = fileNode.Children[i];
                    FileTools.ReduceLevel(bookProperties[i].BookStructure);
                    bookProperties[i].ParentPath = InputPath;
                }
            }
            else
            {
                bookProperties[0].BookStructure = fileNode;
                bookProperties[0].ParentPath = Path.GetDirectoryName(InputPath);
            }
        }

        /// <summary>
        /// HomePageViewModel 的相关参数的保存和读取
        /// </summary>
        // ViewModel保存设置
        public void SaveSetting()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["LastInputPath"].Value = _lastInputPath;
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void LoadSetting()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _lastInputPath = config.AppSettings.Settings["LastInputPath"].Value;
        }
    }
}