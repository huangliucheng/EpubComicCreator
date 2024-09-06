using EpubComicCreator.Models.DataStructure;
using EpubComicCreator.Models.Tools;
using EpubComicCreator.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;

namespace EpubComicCreator.Models
{
    public class BookProgram
    {

        public static void Start(
            List<BookBasicProperties> properties, 
            EBookSetting setting, 
            ObservableCollection<BookStatus> bookStatus,
            CancellationToken token)
        {
            List<BookBasicProperties> finishedBook = [];
            for(int i = 0; i < properties.Count; i++)
            {
                // 检查取消标记
                if (token.IsCancellationRequested)
                {
                    foreach (var epub in finishedBook)
                    {
                        File.Delete(Path.Combine(epub.SavePath, epub.Title + ".tmpepub"));
                    }
                    WeakReferenceMessenger.Default.Send(new BookProgressBarValue(0, 1));
                    return;
                }

                var book = properties[i];
                bookStatus[i].Status = "正在处理";
                WeakReferenceMessenger.Default.Send(new BookProgressBarValue((i + 1) / 2.0, properties.Count));
                WeakReferenceMessenger.Default.Send($"正在制作第{i + 1}本漫画书!", MessageToken.ProgramMessage);

                using (FileStream zipFileStream = new((Path.Combine(book.SavePath, book.Title + ".tmpepub")), FileMode.Create))
                {
                    using ZipArchive zipArchive = new(zipFileStream, ZipArchiveMode.Create);
                    ComicGenerator.ContainerGenerator(zipArchive);
                    ComicGenerator.MimeTypeGenerator(zipArchive);
                    ComicGenerator.OEBPSGenerator(zipArchive, book, setting);

                }
                WeakReferenceMessenger.Default.Send(new BookProgressBarValue((double)(i + 1), properties.Count));
                finishedBook.Add(book);
                bookStatus[i].Status = "已完成";
                WeakReferenceMessenger.Default.Send($"第{i + 1}本漫画书制作完成!", MessageToken.ProgramMessage);
            }

            foreach (var epub in finishedBook)
            {
                string oldPath = Path.Combine(epub.SavePath, epub.Title + ".tmpepub");
                string newPath = Path.Combine(epub.SavePath, epub.Title + ".epub");
                if (File.Exists(newPath)) File.Delete(newPath);
                File.Move(oldPath, newPath);
            }

            WeakReferenceMessenger.Default.Send(new BookProgressBarValue(0, 1));

            return;
        }

        
    }
}