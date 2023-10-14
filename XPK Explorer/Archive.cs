using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XPK_Explorer
{
    public class FileEntry
    {
        public string Name { get; set; }
        public string PathWithoutName { get; set; }
        public string FullPath => Path.Combine(PathWithoutName, Name);
    }

    public class Archive
    {
        private const int HEADER_BYTES_SIZE = 12;
        private const int FILE_ENTRY_TYPE = 1;
        private const int FOLDER_ENTRY_TYPE = 2;

        private LinkedList<FileEntry> _entries;

        public IEnumerable<string> FilePathEntries => _entries.Select(x => x.FullPath);

        private Archive(LinkedList<FileEntry> entries)
        {
            _entries = entries;
        }

        public static Archive Open(string path, string name)
        {
            var xpkFolder = Path.Combine(path, "XPK");
            var xpktFolder = Path.Combine(path, "XPKT");

            var directoryDataFile = Path.Combine(xpktFolder, $"{name}_D.bin");
            var fileSizeDataFile = Path.Combine(xpktFolder, $"{name}_F.bin");

            var entries = new LinkedList<FileEntry>();

            using (var binaryReader = new BinaryReader(File.OpenRead(directoryDataFile)))
            {
                // Skip the header, it is currently unknown how to decode it
                var baseStream = binaryReader.BaseStream;
                baseStream.Seek(HEADER_BYTES_SIZE, SeekOrigin.Begin);

                // Get the offset where folder description starts and move to it
                var folderBeginOffset = binaryReader.ReadUInt32();
                baseStream.Seek(folderBeginOffset, SeekOrigin.Begin);

                var entryPath = new List<string>();
                var folderIndentation = new Stack<long>();

                // There's no explicit data of amount of folders
                // We need to read all the bytes and validate them manually
                while (true)
                {
                    var entryType = binaryReader.ReadUInt16();

                    // Most likely, this is the end of entry files chain
                    if (entryType == 0)
                    {
                        if (folderIndentation.Count > 0)
                        {
                            var position = folderIndentation.Pop();
                            baseStream.Seek(position, SeekOrigin.Begin);
                            entryPath.Clear();
                            continue;
                        }

                        break;
                    }

                    // Read the contents of the entry
                    var pathLength = binaryReader.ReadUInt16();
                    var startFileOffset = binaryReader.ReadUInt32();
                    var pathBytes = binaryReader.ReadBytes(pathLength);
                    var entryName = Encoding.UTF8.GetString(pathBytes);

                    // Skip the trailing byte
                    baseStream.Seek(1, SeekOrigin.Current);

                    // Check the type of entry to process it
                    switch (entryType)
                    {
                        case FILE_ENTRY_TYPE:
                            var fileEntry = new FileEntry()
                            {
                                Name = entryName,
                                PathWithoutName = Path.Combine(entryPath.ToArray())
                            };

                            entries.AddLast(fileEntry);
                            break;

                        case FOLDER_ENTRY_TYPE:
                            entryPath.Add(entryName);
                            folderIndentation.Push(baseStream.Position);
                            baseStream.Seek(startFileOffset, SeekOrigin.Begin);
                            break;

                        default:
                            throw new Exception("Unknown file type");
                    }
                }
            }

            return new Archive(entries);
        }
    }
}
