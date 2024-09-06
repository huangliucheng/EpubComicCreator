using EpubComicCreator.Models.DataStructure;
using System.IO;

namespace EpubComicCreator.Models.Tools
{
    // 用于处理文件结构的工具类
    public class FileTools
    {
        // 建立文件路径树结构
        public static TreeNode BuildTree(string rootPath)
        {
            // 使用递归方法建立文件路径树结构
            static void BuildTreeRecursive(TreeNode parentNode, string directoryPath, int level)
            {

                string[] directories = Directory.GetDirectories(directoryPath);
                foreach (string directory in directories)
                {
                    TreeNode childNode = new TreeNode(Path.GetFileName(directory), level + 1);
                    parentNode.AddChild(childNode);
                    BuildTreeRecursive(childNode, directory, level + 1);
                }

                string[] files = Directory.GetFiles(directoryPath);
                files = files.Where(file => file.EndsWith(".jpg") || file.EndsWith(".png") || file.EndsWith(".jpeg")).ToArray();
                foreach (string file in files)
                {
                    parentNode.AddChild(new TreeNode(Path.GetFileName(file), level + 1));
                }

            }

            TreeNode rootNode = new(Path.GetFileName(rootPath), 1);
            BuildTreeRecursive(rootNode, rootPath, 1);
            return rootNode;
        }

        //获得树形结构的深度
        private static int GetDepth(TreeNode node)
        {
            var children = node.Children;
            if (children.Count == 0) return 1;

            int maxChildDepth = 0;
            foreach (var child in children)
            {
                int childDepth = GetDepth(child);
                if (childDepth > maxChildDepth) maxChildDepth = childDepth;
            }

            return maxChildDepth + 1;
        }

        //替换树中节点的值
        private static readonly string[] ComicValueList = ["Image", "Chapter", "Volume", "Comic"];
        public static void NodeValueReplace(TreeNode node)
        {
            static void ValueReplace(TreeNode node, int maxDepth)
            {
                var children = node.Children;
                if (children.Count == 0) return;

                for (int i = 0; i < children.Count; i++)
                {
                    string levelName = ComicValueList[maxDepth - node.Level - 1];
                    children[i].Name = $"{levelName}-{i + 1:D4}";
                    ValueReplace(children[i], maxDepth);
                }
            }
            int maxDepth = GetDepth(node);
            TreeNode rootNode = new("head", 0);
            rootNode.AddChild(node);
            ValueReplace(rootNode, maxDepth);
        }

        // 减少树节点的Level
        public static void ReduceLevel(TreeNode node)
        {
            static void ReduceLevelRecursive(TreeNode node, int level)
            {
                node.Level = level;
                foreach (var child in node.Children)
                {
                    ReduceLevelRecursive(child, level + 1);
                }
            }

            ReduceLevelRecursive(node, 1);
        }


        // 根据节点获得文件路径列表
        public static List<string> GetFilePathList(string rootPath, TreeNode fileNode)
        {
            List<string> pathsList = [];
            Stack<(string currentPath, TreeNode node)> stack = new();

            stack.Push((rootPath, fileNode));

            while (stack.Count > 0)
            {
                var (currentPath, node) = stack.Pop();
                string newPath;
                if (string.IsNullOrEmpty(currentPath))
                {
                    newPath = $"{node.Name}".Replace("\\", "/");
                }
                else
                {
                   newPath = $"{currentPath}/{node.Name}".Replace("\\", "/");
                }

                if (node.Children.Count == 0)
                {
                    pathsList.Add(newPath);
                }
                else
                {
                    foreach (var child in node.Children)
                    {
                        stack.Push((newPath, child));
                    }
                }
            }

            pathsList.Reverse();
            return pathsList;
        }
    }
}