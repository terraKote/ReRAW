using System.ComponentModel;

namespace XPK_Explorer
{
    public class TextureFileDescription : BaseFileDescription
    {
        private uint _width;
        private uint _height;

        [Category("Texture File Data")]
        public uint Width => _width;
        [Category("Texture File Data")]
        public uint Height => _height;

        public TextureFileDescription(string internalPath, long fileSize, uint width, uint height) : base(internalPath, fileSize)
        {
            _width = width;
            _height = height;
        }
    }
}