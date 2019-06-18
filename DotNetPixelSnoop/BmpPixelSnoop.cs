using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace DotNetPixelSnoop
{
    unsafe class BmpPixelSnoop : IDisposable
    {
        private Bitmap bmp = null;
        private BitmapData data = null;
        private byte* scan0 = null;
        private int depth = 0;
        private int lineBytes = 0;

        private readonly int width;
        private readonly int height;


        public BmpPixelSnoop(Bitmap b)
        {
            bmp = b;
            width = bmp.Width;
            height = bmp.Height;

            // Works only for: PixelFormat.Format32bppArgb

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel
            scan0 = (byte*)data.Scan0.ToPointer();
            lineBytes = bmp.Width * depth;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bmp != null)
                    bmp.UnlockBits(data);
            }
            // free native resources if there are any.
        }

        //private List<int> lineStarts;

        private byte* PixelPointer(int x, int y)
        {
            /*
            if (lineStarts == null)
            {
                lineStarts = new List<int>();
                for (int j = 0; j != Height; j++)
                    lineStarts.Add((int)(scan0 + j * lineBytes));
            }*/

            return scan0 + y * lineBytes + x * depth;
            //return (byte*)lineStarts[y] + x * depth;

        }

        private byte* PixelPointer(int offset)
        {
            return scan0 + offset * depth;
        }

        public System.Drawing.Color GetPixel(int x, int y)
        {
            int a, r, g, b;

            byte* p = PixelPointer(x, y);

            b = *p++;
            g = *p++;
            r = *p++;
            a = *p;

            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        public struct NovaCol
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public NovaCol(int r, int g, int b, int a)
            {
                R = (byte)r;
                G = (byte)g;
                B = (byte)b;
                A = (byte)a;
            }
        }

        public NovaCol NovaGetPixel(int x, int y)
        {
            int a, r, g, b;

            byte* p = PixelPointer(x, y);

            b = *p++;
            g = *p++;
            r = *p++;
            a = *p;

            return new NovaCol(a, r, g, b);
        }


        public System.Drawing.Color GetPixel(int pixelOffset)
        {
            int a, r, g, b;

            byte* p = PixelPointer(pixelOffset);

            b = *p++;
            g = *p++;
            r = *p++;
            a = *p;

            var c = new System.Drawing.Color();


            return System.Drawing.Color.FromArgb(a, r, g, b);
        }



        public byte GetRed(int x, int y)
        {
            return *(PixelPointer(x, y) + 2);
        }

        public byte GetGreen(int x, int y)
        {
            return *(PixelPointer(x, y) + 1);
        }

        public byte GetBlue(int x, int y)
        {
            return *(PixelPointer(x, y));
        }

        public void SetPixel(int x, int y, System.Drawing.Color col)
        {
            byte* p = PixelPointer(x, y);

            *p++ = col.B;
            *p++ = col.G;
            *p++ = col.R;
            *p = col.A;
        }

        public int Width { get { return width; } }
        public int Height { get { return height; } }

       
    }
}
