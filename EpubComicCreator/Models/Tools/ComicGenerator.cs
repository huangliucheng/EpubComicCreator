using EpubComicCreator.Models.DataStructure;
using EpubComicCreator.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace EpubComicCreator.Models.Tools
{
    // 用于生成漫画书的工具类
    public class ComicGenerator
    {
        private static List<string> manifestItem = [];
        private static List<string> spineItemref = [];
        private static List<XmlElement> navListItem = [];
        private static List<XmlElement> navPointItem = [];


        // 创建container.xml文件
        public static void ContainerGenerator(ZipArchive archive)
        {
            var entry = archive.CreateEntry("META-INF/container.xml", CompressionLevel.Optimal);
            using (StreamWriter writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.WriteLine("<?xml version=\"1.0\"?>");
                writer.WriteLine("<container verison=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">");
                writer.WriteLine("\t<rootfiles>");
                writer.WriteLine("\t\t<rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/>");
                writer.WriteLine("\t</rootfiles>");
                writer.WriteLine("</container>");
            }
        }

        //创建mimetype文件
        public static void MimeTypeGenerator(ZipArchive archive)
        {
            var entry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (StreamWriter writer = new(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write("application/epub+zip");
            }
        }

        // 创建OEBPS文件夹及其子文件
        public static void OEBPSGenerator(ZipArchive archive, BookBasicProperties book, EBookSetting setting)
        {
            // 克隆文件结构
            TreeNode epubNode = book.BookStructure.Clone();
            // 构造Epub文件中的路径结构
            FileTools.NodeValueReplace(epubNode);

            // 获取图片的源路径
            List<string> imagePathList = FileTools.GetFilePathList(book.ParentPath, book.BookStructure);
            // 获取图片的保存路径
            List<string> savePathList = FileTools.GetFilePathList(string.Empty, epubNode);

            SubFileGenerator(archive, imagePathList, savePathList, setting);
            ContentGenertor(archive, book, setting);
            NavGenerator(archive, book.Title);
            TocGenerator(archive, book.Title, book.BookID);
        }

        // 创建OEBPS文件夹的子文件
        private static void SubFileGenerator(ZipArchive archive, List<string> imagePathList, List<string> savePathList, EBookSetting setting)
        {


            StyleCSS(archive);
            string chapterName = string.Empty;
            for (int i = 0; i < imagePathList.Count; i++)
            {
                WeakReferenceMessenger.Default.Send(new ImageProgressBarValue(i + 1, imagePathList.Count));
                string currentChapterName = Path.GetFileName(Path.GetDirectoryName(imagePathList[i]));
                bool newChapter = (currentChapterName != chapterName);
                if (newChapter) chapterName = currentChapterName;
                ComicImage Image = new()
                {
                    Image = new(imagePathList[i]),
                    ImageTitle = Path.GetFileNameWithoutExtension(savePathList[i]),
                    ImageRelativePath = savePathList[i],
                    ImageExt = setting.UsePNG ? "png" : "jpg",
                    TargetWxH = setting.TargetWxH
                };

                ImageProcess.Start(Image, setting);
                if (Image.HasChildImage)
                {
                    ComicImage firstChildImage = Image.ChildImage[0];
                    ComicImage secondChildImage = Image.ChildImage[1];

                    // 是否有首页
                    if (i == 0)
                    {
                        var cover = archive.CreateEntry("OEBPS/Images/cover." + Image.ImageExt, CompressionLevel.Optimal);
                        using var coverStream = cover.Open();
                        {
                            firstChildImage.Image.Write(coverStream);
                        }
                    }
                    ImageXhtml(archive, firstChildImage);
                    ImageXhtml(archive, secondChildImage);
                    // 是否是新章节
                    if (newChapter)
                    {
                        string chapterPath = "Text/" + firstChildImage.ImageRelativePath + ".xhtml";
                        navListItem.Add(NavListItem(chapterPath, currentChapterName));
                        navPointItem.Add(NavPointItem(firstChildImage.ImageRelativePath.Replace("/", "_"), currentChapterName, chapterPath));
                    }

                    var firstEntry = archive.CreateEntry("OEBPS/Images/" + firstChildImage.ImageRelativePath + "." + Image.ImageExt);
                    using (var entryStream = firstEntry.Open())
                    {
                        firstChildImage.Image.Write(entryStream);
                    }

                    var secondEntry = archive.CreateEntry("OEBPS/Images/" + secondChildImage.ImageRelativePath + "." + Image.ImageExt);
                    using (var entryStream = secondEntry.Open())
                    {
                        secondChildImage.Image.Write(entryStream);
                    }

                    manifestItem.Add(
                        ManifestItem("page_" + firstChildImage.ImageRelativePath.Replace("/", "_"), 
                        "Text/" + firstChildImage.ImageRelativePath + ".xhtml", 
                        "application/xhtml+xml"));
                    manifestItem.Add(
                        ManifestItem("image_" + firstChildImage.ImageRelativePath.Replace("/", "_"),
                        "Images/" + firstChildImage.ImageRelativePath + "." + Image.ImageExt,
                        setting.UsePNG ? "image/png" : "image/jpeg"));
                    manifestItem.Add(
                        ManifestItem("page_" + secondChildImage.ImageRelativePath.Replace("/", "_"),
                        "Text/" + secondChildImage.ImageRelativePath + ".xhtml",
                        "application/xhtml+xml"));
                    manifestItem.Add(
                        ManifestItem("image_" + secondChildImage.ImageRelativePath.Replace("/", "_"),
                        "Images/" + secondChildImage.ImageRelativePath + "." + Image.ImageExt,
                        setting.UsePNG ? "image/png" : "image/jpeg"));
                    spineItemref.Add(SpineItemref("page_" + firstChildImage.ImageRelativePath.Replace("/", "_")));
                    spineItemref.Add(SpineItemref("page_" + secondChildImage.ImageRelativePath.Replace("/", "_")));
                }
                else
                {
                    if (i == 0)
                    {
                        var cover = archive.CreateEntry("OEBPS/Images/cover." + Image.ImageExt, CompressionLevel.Optimal);
                        using var coverStream = cover.Open();
                        Image.Image.Write(coverStream);
                    }

                    ImageXhtml(archive, Image);
                    if (newChapter)
                    {
                        string chapterPath = "Text/" + Image.ImageRelativePath + ".xhtml";
                        navListItem.Add(NavListItem(chapterPath, currentChapterName));
                        navPointItem.Add(NavPointItem(Image.ImageRelativePath.Replace("/", "_"), currentChapterName, chapterPath));
                    }

                    var entry = archive.CreateEntry("OEBPS/Images/" + Image.ImageRelativePath + "." + Image.ImageExt);
                    using (var entryStream = entry.Open())
                    {
                        Image.Image.Write(entryStream);
                    }

                    manifestItem.Add(
                        ManifestItem("page_" + Image.ImageRelativePath.Replace("/", "_"),
                        "Text/" + Image.ImageRelativePath + ".xhtml",
                        "application/xhtml+xml"));
                    manifestItem.Add(
                        ManifestItem("image_" + Image.ImageRelativePath.Replace("/", "_"),
                        "Images/" + Image.ImageRelativePath + "." + Image.ImageExt,
                        setting.UsePNG ? "image/png" : "image/jpeg"));
                    spineItemref.Add(SpineItemref("page_" + Image.ImageRelativePath.Replace("/", "_")));
                }

            }

        }

        // 创建content.opf文件
        private static void ContentGenertor(ZipArchive archive, BookBasicProperties book, EBookSetting setting)
        {
            string title = book.Title;
            string bookID = book.BookID;
            string modifiedTime = book.ModifiedTime;
            string creator = book.Creator;
            string resolution = string.Format("{0}x{1}", setting.TargetWidth, setting.TargetHeight);
            string pageDirection = setting.MangeMode ? "rtl" : "ltr";
            string imageExt = setting.UsePNG ? "png" : "jpg";
            string imageType = setting.UsePNG ? "image/png" : "image/jpeg";

            var entry = archive.CreateEntry("OEBPS/content.opf", CompressionLevel.Optimal);
            using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            writer.WriteLine("<package version=\"3.0\" unique-identifier=\"BookID\" xmlns=\"http://www.idpf.org/2007/opf\">");
            writer.WriteLine("\t<metadata xmlns:opf=\"http://www.idpf.org/2007/opf\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">");
            writer.WriteLine($"\t\t<dc:title>{title}</dc:title>");
            writer.WriteLine("\t\t<dc:language>zh-CN</dc:language>");
            writer.WriteLine($"\t\t<dc:identifier id=\"BookID\">{bookID}</dc:identifier>");
            writer.WriteLine("\t\t<dc:contributor id=\"contributor\">EpubComicConverter</dc:contributor>");
            writer.WriteLine($"\t\t<dc:creator>{creator}</dc:creator>");
            writer.WriteLine($"\t\t<meta property=\"dcterms:modified\">{modifiedTime}</meta>");
            writer.WriteLine("\t\t<meta name=\"cover\" content=\"cover\"/>");
            writer.WriteLine("\t\t<meta name=\"fixed-layout\" content=\"true\"/>");
            writer.WriteLine($"\t\t<meta name=\"original-resolution\" content=\"{resolution}\"/>");
            writer.WriteLine("\t\t<meta name=\"book-type\" content=\"comic\"/>");
            writer.WriteLine("\t\t<meta name=\"primary-writing-mode\" content=\"horizontal-rl\"/>");
            writer.WriteLine("\t\t<meta name=\"zero-gutter\" content=\"true\"/>");
            writer.WriteLine("\t\t<meta name=\"zero-margin\" content=\"true\"/>");
            writer.WriteLine("\t\t<meta name=\"ke-border-color\" content=\"#FFFFFF\"/>");
            writer.WriteLine("\t\t<meta name=\"ke-border-width\" content=\"0\"/>");
            writer.WriteLine("\t\t<meta property=\"rendition:spread\">landscape</meta>");
            writer.WriteLine("\t\t<meta property=\"rendition:layout\">pre-paginated</meta>");
            writer.WriteLine("\t\t<meta name=\"orientation-lock\" content=\"none\"/>");
            writer.WriteLine("\t\t<meta name=\"region-mag\" content=\"true\"/>");
            writer.WriteLine("\t</metadata>");
            writer.WriteLine("\t<manifest>");
            writer.WriteLine("\t\t<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>");
            writer.WriteLine("\t\t<item id=\"nav\" href=\"nav.xhtml\" properties=\"nav\" media-type=\"application/xhtml+xml\"/>");
            writer.WriteLine($"\t\t<item id=\"cover\" href=\"Images/cover.{imageExt}\" media-type=\"{imageType}\" properties=\"cover-image\"/>");

            foreach (string item in manifestItem)
            {
                writer.WriteLine(item);
            }
            writer.WriteLine("\t\t<item id=\"css\" href=\"Text/style.css\" media-type=\"text/css\"/>");
            writer.WriteLine("\t</manifest>");
            writer.WriteLine($"\t<spine page-progression-direction=\"{pageDirection}\" toc=\"ncx\">");

            foreach (string item in spineItemref)
            {
                writer.WriteLine(item);
            }
            writer.WriteLine("\t</spine>");
            writer.WriteLine("</package>");
        }

        // 创建Nav.xhtml文件
        private static void NavGenerator(ZipArchive archive, string bookTitle)
        {
            var entry = archive.CreateEntry("OEBPS/nav.xhtml", CompressionLevel.Optimal);
            XmlDocument xhtml = new();
            XmlDeclaration declaration = xhtml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xhtml.AppendChild(declaration);

            XmlDocumentType docType = xhtml.CreateDocumentType("html", null, null, null);
            xhtml.AppendChild(docType);

            XmlElement html = xhtml.CreateElement("html");
            html.SetAttribute("xmlns", "http://www.w3.org/1999/xhtml");
            html.SetAttribute("xmlns:epub", "http://www.idpf.org/2007/ops");
            xhtml.AppendChild(html);

            XmlElement head = xhtml.CreateElement("head");
            html.AppendChild(head);

            XmlElement title = xhtml.CreateElement("title");
            title.InnerText = bookTitle;
            head.AppendChild(title);

            XmlElement meta = xhtml.CreateElement("meta");
            meta.SetAttribute("charset", "UTF-8");
            head.AppendChild(meta);

            XmlElement link = xhtml.CreateElement("link");
            link.SetAttribute("href", "style.css");
            link.SetAttribute("type", "text/css");
            link.SetAttribute("rel", "stylesheet");
            head.AppendChild(link);

            XmlElement body = xhtml.CreateElement("body");
            html.AppendChild(body);

            XmlElement nav_toc = xhtml.CreateElement("nav");
            nav_toc.SetAttribute("epub:type", "toc");
            nav_toc.SetAttribute("id", "toc");
            body.AppendChild(nav_toc);

            XmlElement nav_page_list = xhtml.CreateElement("nav");
            nav_page_list.SetAttribute("epub:type", "page-list");
            body.AppendChild(nav_page_list);

            XmlElement ol_toc = xhtml.CreateElement("ol");
            XmlElement ol_page_list = xhtml.CreateElement("ol");
            nav_toc.AppendChild(ol_toc);
            nav_page_list.AppendChild(ol_page_list);

            foreach (XmlElement item in navListItem)
            {
                XmlElement importedItem = (XmlElement)xhtml.ImportNode(item, true);
                ol_toc.AppendChild(importedItem);
                ol_page_list.AppendChild(importedItem.CloneNode(true));
            }

            using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
            xhtml.Save(writer);
        }

        // 创建toc.ncx文件
        private static void TocGenerator(ZipArchive archive, string bookTitle, string bookID)
        {
            var entry = archive.CreateEntry("OEBPS/toc.ncx", CompressionLevel.Optimal);
            XmlDocument toc = new();
            XmlDeclaration declaration = toc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            toc.AppendChild(declaration);

            XmlDocumentType docType = toc.CreateDocumentType("ncx", null, null, null);
            toc.AppendChild(docType);

            XmlElement ncx = toc.CreateElement("ncx");
            ncx.SetAttribute("xmlns", "http://www.daisy.org/z3986/2005/ncx/");
            ncx.SetAttribute("version", "2005-1");
            toc.AppendChild(ncx);

            XmlElement head = toc.CreateElement("head");
            ncx.AppendChild(head);

            XmlElement meta = toc.CreateElement("meta");
            meta.SetAttribute("name", "dtb:uid");
            meta.SetAttribute("content", bookID);
            head.AppendChild(meta);

            XmlElement meta1 = toc.CreateElement("meta");
            meta1.SetAttribute("name", "dtb:depth");
            meta1.SetAttribute("content", "1");
            head.AppendChild(meta1);

            XmlElement meta2 = toc.CreateElement("meta");
            meta2.SetAttribute("name", "dtb:totalPageCount");
            meta2.SetAttribute("content", "0");
            head.AppendChild(meta2);

            XmlElement meta3 = toc.CreateElement("meta");
            meta3.SetAttribute("name", "dtb:maxPageNumber");
            meta3.SetAttribute("content", "0");
            head.AppendChild(meta3);

            XmlElement docTitle = toc.CreateElement("docTitle");
            ncx.AppendChild(docTitle);

            XmlElement text = toc.CreateElement("text");
            text.InnerText = bookTitle;
            docTitle.AppendChild(text);

            XmlElement navMap = toc.CreateElement("navMap");
            ncx.AppendChild(navMap);

            foreach (XmlElement item in navPointItem)
            {
                XmlElement importedItem = (XmlElement)toc.ImportNode(item, true);
                navMap.AppendChild(importedItem);
            }

            using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
            toc.Save(writer);
        }

        //创建CSS文件
        private static void StyleCSS(ZipArchive archive)
        {
            var entry = archive.CreateEntry("OEBPS/Text/style.css", CompressionLevel.Optimal);
            string content = "@page {\r\nmargin: 0;\r\n}\r\nbody {\r\ndisplay: block;\r\nmargin: 0;\r\npadding: 0;\r\n}";
            using (StreamWriter writer = new(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static void ImageXhtml(ZipArchive archive, ComicImage comicImage)
        {
            // 构造["../" * n]的字符串
            int number = comicImage.ImageRelativePath.Split('/').Length;
            string str = new StringBuilder("../".Length * number).Insert(0, "../", number).ToString();

            XmlDocument xhtml = new();
            //创建声明
            XmlDeclaration declaration = xhtml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xhtml.AppendChild(declaration);

            XmlDocumentType docType = xhtml.CreateDocumentType("html", null, null, null);
            xhtml.AppendChild(docType);

            //创建<html>元素
            XmlElement html = xhtml.CreateElement("html");
            html.SetAttribute("xmlns", "http://www.w3.org/1999/xhtml");
            html.SetAttribute("xmlns:epub", "http://www.idpf.org/2007/ops");
            xhtml.AppendChild(html);

            //创建<head>元素
            XmlElement head = xhtml.CreateElement("head");
            html.AppendChild(head);

            //创建<title>元素
            XmlElement title = xhtml.CreateElement("title");
            title.InnerText = comicImage.ImageTitle;
            head.AppendChild(title);

            //创建<link>元素
            XmlElement link = xhtml.CreateElement("link");
            link.SetAttribute("href", str + "Text/style.css");
            link.SetAttribute("type", "text/css");
            link.SetAttribute("rel", "stylesheet");
            head.AppendChild(link);

            //创建<meta>元素
            XmlElement meta = xhtml.CreateElement("meta");
            meta.SetAttribute("name", "viewport");
            meta.SetAttribute("content", string.Format("width={0}, height={1}", comicImage.Image.Width, comicImage.Image.Height));
            head.AppendChild(meta);

            //创建<body>元素
            XmlElement body = xhtml.CreateElement("body");
            body.SetAttribute("style", "");
            html.AppendChild(body);

            //创建<div>元素
            XmlElement div = xhtml.CreateElement("div");
            div.SetAttribute("style", string.Format("text-align:center;top:{0}%;", comicImage.GetTopMargin));
            body.AppendChild(div);

            //创建<img>
            XmlElement img = xhtml.CreateElement("img");
            img.SetAttribute("width", string.Format("{0}", comicImage.Image.Width));
            img.SetAttribute("height", string.Format("{0}", comicImage.Image.Height));
            img.SetAttribute("src", str + "Images/" + comicImage.ImageRelativePath + "." + comicImage.ImageExt);
            div.AppendChild(img);

            var entry = archive.CreateEntry("OEBPS/Text/" + comicImage.ImageRelativePath + ".xhtml", CompressionLevel.Optimal);
            using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
            xhtml.Save(writer);
        }

        // 用于生成content.opf文件中的manifest元素
        private static string ManifestItem(string id, string href, string mediaType)
        {
            return string.Format("\t\t<item id=\"{0}\" href=\"{1}\" media-type=\"{2}\"/>", id, href, mediaType);
        }

        // 用于生成content.opf文件中的spine元素
        private static string SpineItemref(string idref)
        {
            return string.Format("\t\t<itemref idref=\"{0}\"/>", idref);
        }

        // 用于生成Nav.xhtml文件中的列表
        private static XmlElement NavListItem(string href, string title)
        {
            XmlDocument doc = new();
            XmlElement li = doc.CreateElement("li");
            XmlElement a = doc.CreateElement("a");
            a.SetAttribute("href", href);
            a.InnerText = title;
            li.AppendChild(a);
            return li;
        }

        // 用于生成toc.ncx文件中的navPoint元素
        private static XmlElement NavPointItem(string id, string title, string src)
        {
            XmlDocument doc = new();
            XmlElement navPoint = doc.CreateElement("navPoint");
            navPoint.SetAttribute("id", id);

            XmlElement navLabel = doc.CreateElement("navLabel");
            XmlElement text = doc.CreateElement("text");
            text.InnerText = title;
            navLabel.AppendChild(text);
            navPoint.AppendChild(navLabel);

            XmlElement content = doc.CreateElement("content");
            content.SetAttribute("src", src);
            navPoint.AppendChild(content);
            
            return navPoint;
        }
    }
}
