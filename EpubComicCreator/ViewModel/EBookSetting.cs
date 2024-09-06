using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using EpubComicCreator.Config;
using System.Configuration;
using System.Text.Json;

namespace EpubComicCreator.ViewModel
{
    // 处理漫画书的相关设置
    public class EBookSetting : ObservableObject
    {
        public EBookSetting()
        {
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(
                this,
                MessageToken.SettingPage,
                (r, message) => OutSplit = message.Value);
        }

        /// <summary>
        /// 私有成员变量
        /// </summary>
        private bool _usePNG;
        private bool _grayMode;
        private bool _marginCrop;
        private bool _split;
        private bool _rotate;
        private bool _outSplit;
        private bool _mangeMode;
        private bool _disProcess;
        private int _targetHeight;
        private int _targetWidth;
        private int _deviceSelectedIndex;
        private string[] _devicesList;

        /// <summary>
        /// 公共成员变量
        /// </summary>
        public bool UsePNG
        {
            get => _usePNG;
            set => SetProperty(ref _usePNG, value);
        }
        public bool GrayMode
        {
            get => _grayMode;
            set => SetProperty(ref _grayMode, value);
        }
        public bool MarginCrop
        {
            get => _marginCrop;
            set => SetProperty(ref _marginCrop, value);
        }
        public bool Split
        {
            get => _split;
            set
            {
                SetProperty(ref _split, value);
                SetProperty(ref _rotate, !value);
            }
        }
        public bool OutSplit
        {
            get => _outSplit;
            set
            {
                SetProperty(ref _outSplit, value);
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(_outSplit), MessageToken.HomePage);
            }
        }
        public bool Rotate
        {
            get => _rotate;
            set
            {
                SetProperty(ref _rotate, value);
                SetProperty(ref _split, !value);
            }
        }
        public bool MangeMode
        {
            get => _mangeMode;
            set => SetProperty(ref _mangeMode, value);
        }
        public bool DisProcess
        {
            get => _disProcess;
            set => SetProperty(ref _disProcess, value);
        }
        public Dictionary<string, int[]> Profiles { get; set; }
        public int[] TargetWxH { get; set; }
        public int DeviceSelectedIndex
        {
            get => _deviceSelectedIndex;
            set
            {
                SetProperty(ref _deviceSelectedIndex, value);
                TargetWxH = Profiles[DevicesList[_deviceSelectedIndex]];
            }
        }
        public int TargetHeight
        {
            get => _targetHeight;
            set
            {
                SetProperty(ref _targetHeight, value);
                TargetWxH[1] = value;
            }
        }
        public int TargetWidth
        {
            get => _targetWidth;
            set
            {
                SetProperty(ref _targetWidth, value);
                TargetWxH[0] = value;
            }
        }
        public string[] DevicesList
        {
            get => _devicesList;
            set => SetProperty(ref _devicesList, value);
        }

        /// <summary>
        /// EBookSetting的保存和加载
        /// </summary>
        // 保存设置
        public void SaveSetting()
        {
            // 打开配置文件
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 获取自定义配置节
            var configSection = (EBookSettingConfigSection)config.GetSection("EBookSettingConfigSection");
            var setting = configSection.EBookSetting;

            // 修改配置节中的值
            setting.UsePNG = this.UsePNG;
            setting.GrayMode = this.GrayMode;
            setting.MarginCrop = this.MarginCrop;
            setting.Split = this.Split;
            setting.OutSplit = this.OutSplit;
            setting.Rotate = this.Rotate;
            setting.MangeMode = this.MangeMode;
            setting.DisProcess = this.DisProcess;
            setting.DeviceSelectedIndex = this.DeviceSelectedIndex;
            setting.TargetHeight = this.TargetHeight;
            setting.TargetWidth = this.TargetWidth;
            setting.DevicesList = string.Join(",", this.DevicesList);
            setting.Profiles = JsonSerializer.Serialize(this.Profiles);

            // 保存配置文件
            config.Save(ConfigurationSaveMode.Modified);

            // 强制重新加载配置文件
            ConfigurationManager.RefreshSection("EBookSettingConfigSection");
        }

        // 加载设置
        public void LoadSetting()
        {
            var configSection = (EBookSettingConfigSection)ConfigurationManager.GetSection("EBookSettingConfigSection");
            var setting = configSection.EBookSetting;

            // 修改配置节中的值
            this.UsePNG = setting.UsePNG;
            this.GrayMode = setting.GrayMode;
            this.MarginCrop = setting.MarginCrop;
            this.Split = setting.Split;
            this.OutSplit = setting.OutSplit;
            this.Rotate = setting.Rotate;
            this.MangeMode = setting.MangeMode;
            this.DisProcess = setting.DisProcess;
            this.DevicesList = setting.DevicesList.Split(",");
            this.Profiles = JsonSerializer.Deserialize<Dictionary<string, int[]>>(setting.Profiles);
            this.DeviceSelectedIndex = setting.DeviceSelectedIndex;
            this.TargetHeight = setting.TargetHeight;
            this.TargetWidth = setting.TargetWidth;
        }
    }
}
