using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Pfim;
using ImageFormat = Pfim.ImageFormat;

namespace XPK_Explorer.FileManagement.Loaders
{
    public class BitmapFileLoader : BaseFileLoader<Bitmap>
    {
        public override Bitmap Load(byte[] bytes)
        {
            Bitmap im;

            using (var ms = new MemoryStream(bytes))
            {
                using (var image = Pfimage.FromStream(ms))
                {
                    PixelFormat format;

                    // Convert from Pfim's backend agnostic image format into GDI+'s image format
                    switch (image.Format)
                    {
                        case ImageFormat.Rgba32:
                            format = PixelFormat.Format32bppArgb;
                            break;
                        default:
                            // see the sample for more details
                            throw new NotImplementedException();
                    }

                    // Pin pfim's data array so that it doesn't get reaped by GC, unnecessary
                    // in this snippet but useful technique if the data was going to be used in
                    // control like a picture box
                    var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                    try
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                        return new Bitmap(image.Width, image.Height, image.Stride, format, data);
                        //bitmap.Save(Path.ChangeExtension(path, ".png"), System.Drawing.Imaging.ImageFormat.Png);
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