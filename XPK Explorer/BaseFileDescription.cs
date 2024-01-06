using System.ComponentModel;

namespace XPK_Explorer
{
    public class BaseFileDescription
    {
        private string _internalPath;
        private long _fileSize;

        [Category("Base File Data")]
        [DisplayName("Internal Path")]
        [Description("Path to file in .XPK archive.")]
        public string InternalPath => _internalPath;

        [Category("Base File Data")]
        public long FileSize => _fileSize;

        protected BaseFileDescription(string internalPath, long fileSize)
        {
            _internalPath = internalPath;
            _fileSize = fileSize;
        }
    }
}
