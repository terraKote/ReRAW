using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace XPK_Explorer
{
    public abstract class ArchiveEntry
    {
        public ushort PathLength;
        public string Path;
    }

    public class ArchiveFolderEntry : ArchiveEntry
    {
        public uint StartFileOffset;
        public List<ArchiveEntry> ChildEntries;

        public List<ArchiveEntry> GetArchiveEntries()
        {
            var entries = new List<ArchiveEntry>();
            var pathStack = new Stack<ArchiveEntry>(ChildEntries);

            while (pathStack.Count > 0)
            {
                var entry = pathStack.Pop();
                entries.Add(entry);

                if (entry.GetType() == typeof(ArchiveFolderEntry))
                {
                    var folder = (ArchiveFolderEntry)entry;

                    foreach (var childEntry in folder.ChildEntries)
                    {
                        pathStack.Push(childEntry);
                    }
                }
            }

            return entries;
        }
    }

    public class ArchiveFileEntry : ArchiveEntry
    {
        public uint Index;
        public string Parent;
    }

    public struct FileSizeData
    {
        public uint Offset;
        public uint Size;
    }

    public class Archive
    {
        public List<ArchiveEntry> Entries;
        public List<FileSizeData> FileSizeData;
        public string XpkFilePath;
        public string Name;

        public Dictionary<string, ArchiveEntry> ArchiveEntries;

        public static Archive Open(string path, string name)
        {
            var xpkFolder = Path.Combine(path, "XPK");
            var xpktFolder = Path.Combine(path, "XPKT");

            var directoryDataFile = Path.Combine(xpktFolder, $"{name}_D.bin");
            var fileSizeDataFile = Path.Combine(xpktFolder, $"{name}_F.bin");

            var entries = new List<ArchiveEntry>();

            using (var binaryReader = new BinaryReader(File.OpenRead(directoryDataFile)))
            {
                // Skip the header
                binaryReader.BaseStream.Seek(12, SeekOrigin.Begin);
                var folderBeginOffset = binaryReader.ReadUInt32();

                var offset = folderBeginOffset;
                var parent = string.Empty;

                while (true)
                {
                    if (!TryReadArchiveEntry(binaryReader, ref offset, out var entry, parent))
                    {
                        break;
                    }

                    entries.Add(entry);
                }
            }

            var fileSizeData = new List<FileSizeData>();

            using (var binaryReader = new BinaryReader(File.OpenRead(fileSizeDataFile)))
            {
                // Skip the header
                binaryReader.BaseStream.Seek(12, SeekOrigin.Begin);

                while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                {
                    var offset = binaryReader.ReadUInt32();
                    var size = binaryReader.ReadUInt32();
                    binaryReader.BaseStream.Seek(4, SeekOrigin.Current);

                    var data = new FileSizeData()
                    {
                        Offset = offset,
                        Size = size
                    };

                    fileSizeData.Add(data);
                }
            }

            var archive = new Archive
            {
                Entries = entries,
                FileSizeData = fileSizeData,
                XpkFilePath = Path.Combine(xpkFolder, $"{name}.XPK"),
                Name = name
            };
            return archive;
        }

        private static bool TryReadArchiveEntry(BinaryReader binaryReader, ref uint offset, out ArchiveEntry entry, string parent)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var type = binaryReader.ReadUInt16();

            if (type == 0)
            {
                entry = null;
                return false;
            }

            var pathLength = binaryReader.ReadUInt16();
            var startFileOffset = binaryReader.ReadUInt32();
            var pathBytes = binaryReader.ReadBytes(pathLength);
            binaryReader.BaseStream.Seek(1, SeekOrigin.Current);

            offset = (uint)binaryReader.BaseStream.Position;
            var path = Encoding.UTF8.GetString(pathBytes);

            switch (type)
            {
                case 1:
                    entry = new ArchiveFileEntry
                    {
                        PathLength = pathLength,
                        Index = startFileOffset,
                        Path = path,
                        Parent = Path.Combine(parent, path)
                    };
                    return true;

                case 2:
                    var folderOffset = startFileOffset;
                    var children = new List<ArchiveEntry>();

                    while (TryReadArchiveEntry(binaryReader, ref folderOffset, out var child, Path.Combine(parent, path)))
                    {
                        children.Add(child);
                    }

                    entry = new ArchiveFolderEntry
                    {
                        PathLength = pathLength,
                        StartFileOffset = startFileOffset,
                        Path = path,
                        ChildEntries = children
                    };

                    return true;

                default:
                    throw new Exception("Unknown file type");
            }
        }

        public byte[] GetFileBytes(string path)
        {
            var p = path.Replace($"{Name}\\", string.Empty);
            var e = Entries.Where(x => x.GetType() == typeof(ArchiveFolderEntry)).Cast<ArchiveFolderEntry>()
                .SelectMany(x => x.GetArchiveEntries()).Where(x => x.GetType() == typeof(ArchiveFileEntry)).Cast<ArchiveFileEntry>().ToDictionary(x => x.Parent, y => y);

            if (e.TryGetValue(p, out var f))
            {
                var size = FileSizeData[(int)f.Index];

                using (var binaryReader = new BinaryReader(File.OpenRead(XpkFilePath)))
                {
                    // Skip the header
                    binaryReader.BaseStream.Seek(size.Offset, SeekOrigin.Begin);
                    return binaryReader.ReadBytes((int)size.Size);
                }
            }

            return null;
        }
    }
}
