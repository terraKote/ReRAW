using System.ComponentModel;

namespace XPK_Explorer.FileManagement.Descriptions
{
    public class TextureFileDescription : BaseFileDescription
    {
        [Category("Texture File Data")]
        public uint Width { get; }

        [Category("Texture File Data")]
        public uint Height { get; }

        public TextureFileDescription(string internalPath, long fileSize, uint width, uint height) : base(internalPath, fileSize)
        {
            Width = width;
            Height = height;
        }
    }
}