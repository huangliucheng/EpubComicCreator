using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace EpubComicCreator.ViewModel
{
    public class BookProgressBarValue
    {
        public double CurrentBookNumber { get; set; }
        public int MaxBookNumber { get; set; }

        public BookProgressBarValue(double currentBookNumber, int maxBookNumber)
        {
            CurrentBookNumber = currentBookNumber;
            MaxBookNumber = maxBookNumber;
        }
    }

    public class ImageProgressBarValue
    {
        public int CurrentImageNumber { get; set; }
        public int MaxImageNumber { get; set; }

        public ImageProgressBarValue(int currentImageNumber, int maxImageNumber)
        {
            CurrentImageNumber = currentImageNumber;
            MaxImageNumber = maxImageNumber;
        }
    }

    public class ProgressBarViewModel : ObservableRecipient
    {

        public ProgressBarViewModel()
        {
            CurrentBookNumber = 0;
            MaxBookNumber = 1;
            CurrentImageNumber = 0;
            MaxImageNumber = 1;
            ImageProgressBarVisibility = "Hidden";
            WeakReferenceMessenger.Default.Register<BookProgressBarValue>(this, HandleReceivedBookNumer);
            WeakReferenceMessenger.Default.Register<ImageProgressBarValue>(this, HandleReceivedImageNumer);
            WeakReferenceMessenger.Default.Register<string, string>(this, MessageToken.ImageBarVisibility, HandleReceivedImageBarVisibility);
        }

        private double _currentBookNumber;
        private int _maxBookNumber;
        private int _currentImageNumber;
        private int _maxImageNumber;
        private string _imageProgressBarVisibility;

        // 总任务进度条当前进度值
        public double CurrentBookNumber
        {
            get { return _currentBookNumber; }
            set
            {
                if (_currentBookNumber != value)
                {
                    _currentBookNumber = value;
                    OnPropertyChanged(nameof(CurrentBookNumber));
                }
            }
        }
        // 总任务进度条最大值
        public int MaxBookNumber
        {
            get { return _maxBookNumber; }
            set
            {
                if (_maxBookNumber != value)
                {
                    _maxBookNumber = value;
                    OnPropertyChanged(nameof(MaxBookNumber));
                }
            }
        }
        // 每本漫画书中处理图片的进度值
        public int CurrentImageNumber
        {
            get { return _currentImageNumber; }
            set
            {
                if (_currentImageNumber != value)
                {
                    _currentImageNumber = value;
                    OnPropertyChanged(nameof(CurrentImageNumber));
                }
            }
        }
        // 每本漫画书中的图片总数量
        public int MaxImageNumber
        {
            get { return _maxImageNumber; }
            set
            {
                if (_maxImageNumber != value)
                {
                    _maxImageNumber = value;
                    OnPropertyChanged(nameof(MaxImageNumber));
                }
            }
        }
        // 图片进度条的可见性
        public string ImageProgressBarVisibility
        {
            get { return _imageProgressBarVisibility; }
            set
            {
                if (_imageProgressBarVisibility != value)
                {
                    _imageProgressBarVisibility = value;
                    OnPropertyChanged(nameof(ImageProgressBarVisibility));
                }
            }
        }

        private void HandleReceivedBookNumer(object recipient, BookProgressBarValue message)
        {
            CurrentBookNumber = message.CurrentBookNumber;
            MaxBookNumber = message.MaxBookNumber;
        }

        private void HandleReceivedImageNumer(object recipient, ImageProgressBarValue message)
        {
            CurrentImageNumber = message.CurrentImageNumber;
            MaxImageNumber = message.MaxImageNumber;
        }

        private void HandleReceivedImageBarVisibility(object recipient, string message)
        {
            if (message == "Visible") _imageProgressBarVisibility = "Visible";
            else _imageProgressBarVisibility = "Hidden";
            OnPropertyChanged(nameof(ImageProgressBarVisibility));
        }
    }
}
