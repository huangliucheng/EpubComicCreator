using ImageMagick;

namespace EpubComicCreator.Models.DataStructure
{
    // 图片的基本属性
    public class ComicImage
    {
        private string? _imageTitle;

        public bool HasChildImage { get; set; }             // 是否有子图片
        public MagickImage? Image { get; set; }              // 图片对象
        public string? ImageTitle                            // 图片标题
        {
            get => _imageTitle;
            set
            {
                _imageTitle = value;
                ChildImageTitle = [value + "-01", value + "-02"];
            }
        }
        public string? GetTopMargin { get; set; }            // 上边距
        public string? ImageRelativePath { get; set; }       // 图片相对路径
        public string? ImageExt { get; set; }                // 图片的扩展名
        public List<ComicImage>? ChildImage { get; set; }    // 子图片属性
        public string[]? ChildImageTitle { get; set; }       // 子图片标题列表
        public int[]? TargetWxH { get; set; }                // 目标宽高

        public ComicImage()
        {
            ImageTitle = "";
            GetTopMargin = "0";
            ChildImage = [];
            HasChildImage = false;
        }
    }
}
