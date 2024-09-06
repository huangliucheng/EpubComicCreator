using EpubComicCreator.Models.DataStructure;
using EpubComicCreator.Models.Tools;
using EpubComicCreator.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpubComicCreator.Models
{
    public class ImageProcess
    {
        public static void Start(ComicImage Image, EBookSetting Setting)
        {
            // 是否处理图片
            if (Setting.DisProcess) return;

            // 是否转换为灰度图像
            if (Setting.GrayMode) ImageTools.GrayImage(Image);

            // 长页处理
            if (Image.Image.Width > Image.Image.Height)
            {
                // 是否旋转图片
                if (Setting.Rotate)
                {
                    ImageTools.RotateImage(Image);
                    Image.HasChildImage = false;
                }
                else if (Setting.Split)
                {
                    ImageTools.PageSplit(Image);
                    Image.HasChildImage = true;
                    // 是否是日漫格式
                    if (Setting.MangeMode)
                    {
                        // 日漫格式下，左右图片的顺序是相反的
                        Image.ChildImage.Reverse();
                    }
                    string parentDirectory = Path.GetDirectoryName(Image.ImageRelativePath);
                    Image.ChildImage[0].ImageRelativePath = parentDirectory + Image.ChildImageTitle[0];
                    Image.ChildImage[1].ImageRelativePath = parentDirectory + Image.ChildImageTitle[1];
                }
            }

            // 是否裁剪边距
            if (Setting.MarginCrop)
            {
                if (Image.HasChildImage)
                {
                    foreach (var child in Image.ChildImage)
                    {
                        ImageTools.MarginCrop(child);
                    }
                }
                else
                {
                    ImageTools.MarginCrop(Image);
                }
            }



            // 调整图片大小并获得图像在目标设备的垂直百分比
            if (Image.HasChildImage)
            {
                foreach (var child in Image.ChildImage)
                {
                    ImageTools.ResizeImage(child);
                    ImageTools.GetTopMargin(child);
                }
            }
            else
            {
                ImageTools.ResizeImage(Image);
                ImageTools.GetTopMargin(Image);
            }
        }
    }
}
