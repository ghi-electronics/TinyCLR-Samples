using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    /// <summary>
    /// to manage file listing result
    /// </summary>
    public class FileExplorer
    {
        public enum ExploreMode { Idle, Listing, Search };

        public ExploreMode Mode { get; set; } = ExploreMode.Idle;
        public string CurrentDirectory { get; set; } = string.Empty;
        ArrayList Content { get; set; }
        public int CurrentIndex { get; set; } = 0;
        public int Count => this.Content.Count;

        public ItemResult GetByIndex(int index)
        {
            if(index>=0 && index < this.Count)
            {
                return this.Content[index] as ItemResult;
            }
            return null;
        }

        public FileExplorer() => this.Content = new ArrayList();

        public void AddFile(FileInfo file)
        {
            var res = new ItemResult() { FullName = file.FullName, Name = file.Name, Attribute = ItemResult.GetAttribute( file.Attributes), LastModified = file.LastWriteTime, ResultType = ItemResult.ResultTypes.File, Size = file.Length };
            this.Content.Add(res);
        }
        public void AddDirectory(DirectoryInfo dir)
        {
            var res = new ItemResult() { FullName = dir.FullName, Name = dir.Name, Attribute = ItemResult.GetAttribute( dir.Attributes), LastModified = dir.LastWriteTime, ResultType = ItemResult.ResultTypes.Folder, Size = 0 };
            this.Content.Add(res);
        }
        public void Clear() => this.Content.Clear();
    }
}
