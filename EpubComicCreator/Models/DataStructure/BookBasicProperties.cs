namespace EpubComicCreator.Models.DataStructure
{
    // 漫画书的基本属性
    public class BookBasicProperties
    {
        private string _title;
        private string _bookID;
        private string _creator;
        private string _modifiedTime;
        private string _parentPath;
        private string _savePath;

        // 书名
        public string Title
        {
            get => _title; 
            set => _title = value ?? throw new ArgumentException("书名不能为空");
        }
        // 书籍ID
        public string BookID { get => _bookID;  set => _bookID = value;  }
        // 书籍创建者
        public string Creator { get => _creator;  set => _creator = value; }
        // 书籍创建时间
        public string ModifiedTime { get => _modifiedTime;  set => _modifiedTime = value;}
        // 书籍的父路径
        public string ParentPath { get => _parentPath;  set => _parentPath = value;}
        // 书籍的保存路径
        public string SavePath { get => _savePath;  set => _savePath = value; }

        // 漫画书的文件结构
        public TreeNode BookStructure { get; set; }

        // 构造函数
        public BookBasicProperties(string Title)
        {
            _title = Title;
            _bookID = "urn:uuid:" + Guid.NewGuid().ToString();
            _modifiedTime = DateTime.Now.ToString("yyyy-MM-dd");
            _creator = "ComicEBookCreator";
        }
    }
}
