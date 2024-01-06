using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Pfim;

namespace XPK_Explorer.FileManagement.Loaders
{
    /// <summary>
    /// File loader for bitmaps. Can load images from bytes and interpret them as <see cref="Bitmap"/>.
    /// </summary>
    public class BitmapFileLoader : BaseFileLoader<Bitmap>
    {
        public override Bitmap Load(string extension, byte[] bytes)
        {
            Bitmap im = null;

            switch (extension.ToLower())
            {
                case ".dds":
                    im = LoadDdsImage(bytes);
                    break;

                case ".bmp":
                    im = LoadBitmapImage(bytes);
                    break;
            }

            return im;
        }

        private static Bitmap LoadBitmapImage(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return new Bitmap(ms);
            }
        }

        private static Bitmap LoadDdsImage(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                using (var image = Pfimage.FromStream(ms))
                {
                    PixelFormat format;

                    switch (image.Format)
                    {
                        default:
                            format = PixelFormat.Format32bppArgb;
                            break;
                    }

                    var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                    try
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                        return new Bitmap(image.Width, image.Height, image.Stride, format, data);
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
            }
        }
    }
}