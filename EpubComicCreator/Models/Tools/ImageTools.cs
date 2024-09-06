using EpubComicCreator.Models.DataStructure;
using ImageMagick;

namespace EpubComicCreator.Models.Tools
{
    // 用于处理图片的工具类
    public class ImageTools
    {
        // 转换图片为灰度图
        public static void GrayImage(ComicImage image)
        {
            image.Image.ColorSpace = ColorSpace.Gray;
        }

        // 旋转图片
        public static void RotateImage(ComicImage image)
        {
            image.Image.Rotate(90);
        }

        // 分割图片
        public static void PageSplit(ComicImage image)
        {
            int halfWidth = image.Image.Width / 2;
            MagickImage leftImage = (MagickImage)image.Image.Clone();
            MagickImage rightImage = (MagickImage)image.Image.Clone();

            leftImage.Crop(new MagickGeometry(0, 0, halfWidth, image.Image.Height));
            leftImage.RePage();
            rightImage.Crop(new MagickGeometry(halfWidth, 0, halfWidth, image.Image.Height));
            rightImage.RePage();

            ComicImage LeftImage = new()
            {
                Image = leftImage,
                TargetWxH = image.TargetWxH,
                HasChildImage = false,
                GetTopMargin = image.GetTopMargin,
                ChildImageTitle = [],
                ImageExt = image.ImageExt,
            };
            ComicImage RightImage = new()
            {
                Image = rightImage,
                TargetWxH = image.TargetWxH,
                HasChildImage = false,
                GetTopMargin = image.GetTopMargin,
                ChildImageTitle = [],
                ImageExt = image.ImageExt,
            };

            image.ChildImage.Add(LeftImage);
            image.ChildImage.Add(RightImage);
        }

        // 调整图片大小
        public static void ResizeImage(ComicImage image)
        {
            // 目标设备中宽和高
            int targetWidth = image.TargetWxH[0];
            int targetHeight = image.TargetWxH[1];

            // 图像的原始宽和高
            int originalWidth = image.Image.Width;
            int originalHeight = image.Image.Height;

            // 计算缩放比例
            double scaleFactor = Math.Min((double)targetWidth / originalWidth, (double)targetHeight / originalHeight);

            // 计算缩放后的宽度和高度
            int newWidth = (int)Math.Round(originalWidth * scaleFactor);
            int newHeight = (int)Math.Round(originalHeight * scaleFactor);

            // 缩放图片
            image.Image.Resize(newWidth, newHeight);
        }

        // 边缘裁剪
        public static void MarginCrop(ComicImage image)
        {
            int width = image.Image.Width;
            int height = image.Image.Height;

            using MagickImage tmpImage = (MagickImage)image.Image.Clone();
            tmpImage.ColorSpace = ColorSpace.Gray;
            tmpImage.AutoThreshold(AutoThresholdMethod.OTSU);

            if (tmpImage.BoundingBox == null) return;
            IMagickGeometry boundingBox = tmpImage.BoundingBox;

            // 如果boundingBox的面积小于图像面积的一半，则不进行边缘裁剪
            if (boundingBox.Width * boundingBox.Height < width * height / 2)
            {
                return;
            }

            boundingBox.Width = (boundingBox.Width + 20 < width ? boundingBox.Width + 20 : boundingBox.Width);
            boundingBox.Height = (boundingBox.Height + 40 < height ? boundingBox.Height + 40 : boundingBox.Height);
            boundingBox.X = (boundingBox.X - 10 < 0 ? boundingBox.X : boundingBox.X - 10);
            boundingBox.Y = (boundingBox.Y - 20 < 0 ? boundingBox.Y : boundingBox.Y - 20);
            image.Image.Crop(boundingBox);
        }

        // 获得图片在设备中的垂直居中的百分比
        public static string GetTopMargin(ComicImage image)
        {
            int targetHeight = image.TargetWxH[1];
            int imageHeight = image.Image.Height;

            int topMargin = (targetHeight - imageHeight) / 2 * 100 / targetHeight;
            topMargin = topMargin < 0 ? 0 : topMargin;
            return topMargin.ToString("F1");
        }
    }
}
