using System.IO;

namespace XPK_Explorer
{
    public class FileEntry
    {
        public string Name { get; }
        public string PathWithoutName { get; }
        public string FullPath => Path.Combine(PathWithoutName, Name);
        public uint Offset { get; set; }
        public uint Size { get; set; }

        public FileEntry(string name, string pathWithoutName)
        {
            Name = name;
            PathWithoutName = pathWithoutName;
        }
    }
}