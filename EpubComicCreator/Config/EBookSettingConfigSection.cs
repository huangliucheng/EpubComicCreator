using System.Configuration;
namespace EpubComicCreator.Config
{
    public class EBookSettingConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("EBookSetting")]
        public EBookSettingElement EBookSetting
        {
            get { return (EBookSettingElement)this[nameof(EBookSetting)]; }
            set { this[nameof(EBookSetting)] = value; }
        }
    }

    public class EBookSettingElement : ConfigurationElement
    {
        [ConfigurationProperty("UsePNG", IsRequired = true)]
        public bool UsePNG
        {
            get { return (bool)this[nameof(UsePNG)]; }
            set { this[nameof(UsePNG)] = value; }
        }

        [ConfigurationProperty("GrayMode", IsRequired = true)]
        public bool GrayMode
        {
            get { return (bool)this[nameof(GrayMode)]; }
            set { this[nameof(GrayMode)] = value; }
        }

        [ConfigurationProperty("MarginCrop", IsRequired = true)]
        public bool MarginCrop
        {
            get { return (bool)this[nameof(MarginCrop)]; }
            set { this[nameof(MarginCrop)] = value; }
        }

        [ConfigurationProperty("Split", IsRequired = true)]
        public bool Split
        {
            get { return (bool)this[nameof(Split)]; }
            set { this[nameof(Split)] = value; }
        }

        [ConfigurationProperty("OutSplit", IsRequired = true)]
        public bool OutSplit
        {
            get { return (bool)this[nameof(OutSplit)]; }
            set { this[nameof(OutSplit)] = value; }
        }

        [ConfigurationProperty("Rotate", IsRequired = true)]
        public bool Rotate
        {
            get { return (bool)this[nameof(Rotate)]; }
            set { this[nameof(Rotate)] = value; }
        }

        [ConfigurationProperty("MangeMode", IsRequired = true)]
        public bool MangeMode
        {
            get { return (bool)this[nameof(MangeMode)]; }
            set { this[nameof(MangeMode)] = value; }
        }

        [ConfigurationProperty("DisProcess", IsRequired = true)]
        public bool DisProcess
        {
            get { return (bool)this[nameof(DisProcess)]; }
            set { this[nameof(DisProcess)] = value; }
        }

        [ConfigurationProperty("DeviceSelectedIndex", IsRequired = true)]
        public int DeviceSelectedIndex
        {
            get { return (int)this[nameof(DeviceSelectedIndex)]; }
            set { this[nameof(DeviceSelectedIndex)] = value; }
        }

        [ConfigurationProperty("TargetHeight", IsRequired = true)]
        public int TargetHeight
        {
            get { return (int)this[nameof(TargetHeight)]; }
            set { this[nameof(TargetHeight)] = value; }
        }

        [ConfigurationProperty("TargetWidth", IsRequired = true)]
        public int TargetWidth
        {
            get { return (int)this[nameof(TargetWidth)]; }
            set { this[nameof(TargetWidth)] = value; }
        }

        [ConfigurationProperty("DevicesList", IsRequired = true)]
        public string DevicesList
        {
            get { return (string)this[nameof(DevicesList)]; }
            set { this[nameof(DevicesList)] = value; }
        }

        [ConfigurationProperty("Profiles", IsRequired = true)]
        public string Profiles
        {
            get { return (string)this[nameof(Profiles)]; }
            set { this[nameof(Profiles)] = value; }
        }
    }
}

