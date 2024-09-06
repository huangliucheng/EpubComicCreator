namespace EpubComicCreator.ViewModel
{
    public class MessageToken
    {
        // 用于传递程序消息的Token
        public static readonly string ProgramMessage = "ProgramMessage";
        // 用于传递制作状态消息的Token
        public static readonly string StatusMessage = "StatusMessage";
        // 用于HomePage接受输出模式消息的Token
        public static readonly string HomePage = "HomePage";
        // 用于SettingPage接受输出模式消息的Token
        public static readonly string SettingPage = "SettingPage";
        // 用于ImageProgressBar的可视性消息的Token
        public static readonly string ImageBarVisibility = "ImageBarVisibility";
    }
}
