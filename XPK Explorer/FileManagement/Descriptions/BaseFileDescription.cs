using System.ComponentModel;

namespace XPK_Explorer.FileManagement.Descriptions
{
    public class BaseFileDescription
    {
        [Category("Base File Data")]
        [DisplayName("Internal Path")]
        [Description("Path to file in .XPK archive.")]
        public string InternalPath { get; }

        [Category("Base File Data")]
        public long FileSize { get; }

        protected BaseFileDescription(string internalPath, long fileSize)
        {
            InternalPath = internalPath;
            FileSize = fileSize;
        }
    }
}
