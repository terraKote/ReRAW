using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XPK_Explorer.FileManagement
{
    public class Archive
    {
        private const int HEADER_BYTES_SIZE = 12;
        private const int FILE_ENTRY_TYPE = 1;
        private const int FOLDER_ENTRY_TYPE = 2;

        private readonly LinkedList<FileEntry> _entries;
        private readonly string _pathToXpkFile;

        public string Name { get; }

        public IEnumerable<string> FilePathEntries => _entries.Select(x => x.FullPath);

        private Archive(string name, LinkedList<FileEntry> entries, string pathToXpkFile)
        {
            Name = name;
            _entries = entries;
            _pathToXpkFile = pathToXpkFile;
        }

        public static Archive Open(string path, string name)
        {
            var xpkFolder = Path.Combine(path, "XPK");
            var xpktFolder = Path.Combine(path, "XPKT");

            var directoryDataFile = Path.Combine(xpktFolder, $"{name}_D.bin");
            var fileSizeDataFile = Path.Combine(xpktFolder, $"{name}_F.bin");

            var entries = new LinkedList<FileEntry>();

            // Read directory data file, to get the list of contents
            using (var binaryReader = new BinaryReader(File.OpenRead(directoryDataFile)))
            {
                // Skip the header, it is currently unknown how to decode it
                var baseStream = binaryReader.BaseStream;
                baseStream.Seek(HEADER_BYTES_SIZE, SeekOrigin.Begin);

                // Get the offset where folder description starts and move to it
                var folderBeginOffset = binaryReader.ReadUInt32();
                baseStream.Seek(folderBeginOffset, SeekOrigin.Begin);

                var entryPath = new Stack<string>();
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
                            entryPath.Pop();
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
                            var fileEntry = new FileEntry(entryName, Path.Combine(entryPath.Reverse().ToArray()));
                            entries.AddLast(fileEntry);
                            break;

                        case FOLDER_ENTRY_TYPE:
                            entryPath.Push(entryName);
                            folderIndentation.Push(baseStream.Position);
                            baseStream.Seek(startFileOffset, SeekOrigin.Begin);
                            break;

                        default:
                            throw new Exception("Unknown file type");
                    }
                }
            }

            // Read file size data file, to get a size and an offset of a file
            using (var binaryReader = new BinaryReader(File.OpenRead(fileSizeDataFile)))
            {
                // Skip the header, it is currently unknown how to decode it
                var baseStream = binaryReader.BaseStream;
                baseStream.Seek(HEADER_BYTES_SIZE, SeekOrigin.Begin);

                foreach (var fileEntry in entries)
                {
                    var offset = binaryReader.ReadUInt32();
                    var size = binaryReader.ReadUInt32();

                    fileEntry.Offset = offset;
                    fileEntry.Size = size;

                    // Skip end of entry empty bytes
                    baseStream.Seek(4, SeekOrigin.Current);
                }
            }

            return new Archive(name, entries, Path.Combine(xpkFolder, $"{name}.XPK"));
        }

        public FileEntry GetFileEntry(string path)
        {
            return _entries.FirstOrDefault(x => string.Equals(x.FullPath, path));
        }

        public byte[] GetFileEntryBytes(FileEntry fileEntry)
        {
            using (var binaryReader = new BinaryReader(File.OpenRead(_pathToXpkFile)))
            {
                binaryReader.BaseStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
                return binaryReader.ReadBytes((int)fileEntry.Size);
            }
        }
    }
}
