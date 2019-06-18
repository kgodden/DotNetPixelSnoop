using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPixelSnoop
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSnoop();
        }

        static void TestSnoop()
        {
            int testWidth = 1920;
            int testHeight = 1080;

            // Create a bitmap
            Bitmap bmp = new Bitmap(testWidth, testHeight);

            // Now fill up the Bitmap with some random data
            // using the bitmap's normal SetPixel() function.
            Random rand = new Random();

            for (int j = 0; j != bmp.Height; j++)
            {
                for (int i = 0; i != bmp.Width; i++)
                {
                    bmp.SetPixel(i, j, Color.FromArgb(rand.Next()));
                }
            }

            TestGetPixel(bmp);
            TestSetPixel(bmp);
            TestGetPixelSpeed(bmp);


            int sum = 0;

            /////

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int height = bmp.Height;
            int width = bmp.Width;

            for (int j = 0; j != height; j++)
            {
                for (int i = 0; i != width; i++)
                {
                    sum += bmp.GetPixel(i, j).R;
                }
            }

            Trace.WriteLine(string.Format("M Sum: {0}, took: {1}", sum, stopwatch.ElapsedMilliseconds));


            var bmpClone = bmp.Clone() as Bitmap;
            var snoop = new BmpPixelSnoop(bmpClone);
            /*
            // Set using normal
            stopwatch.Reset();
            stopwatch.Start();

            for (int j = 0; j != height; j++)
            {
                for (int i = 0; i != width; i++)
                {
                    bmp.SetPixel(i, j, Color.Red);
                }
            }

            Trace.WriteLine(string.Format("Set 0, took: {0}", stopwatch.ElapsedMilliseconds));
            */

            // blaster

            stopwatch.Reset();
            stopwatch.Start();

 
            sum = 0;

            for (int j = 0; j != height; j++)
            {
                for (int i = 0; i != width; i++)
                {
                    sum += snoop.GetPixel(i, j).R;
                }
            }

            Trace.WriteLine(string.Format("B0 Sum: {0}, took: {1}", sum, stopwatch.ElapsedMilliseconds));

            // Using pixel offset
            // Red channel only
            stopwatch.Reset();
            stopwatch.Start();

            sum = 0;

            int size = snoop.Width * snoop.Height;
            for (int i = 0; i != snoop.Width * snoop.Height; i++)
            {
                var col = snoop.GetPixel(i);
                sum += col.R + col.G + col.B;
            }


            Trace.WriteLine(string.Format("B1 Sum: {0}, took: {1}", sum, stopwatch.ElapsedMilliseconds));



            // Red channel only
            stopwatch.Reset();
            stopwatch.Start();


            sum = 0;


            for (int j = 0; j != snoop.Height; j++)
            {
                for (int i = 0; i != snoop.Width; i++)
                {
                    sum += snoop.GetRed(i, j);
                    sum += snoop.GetGreen(i, j);
                    sum += snoop.GetBlue(i, j);
                }
            }


            Trace.WriteLine(string.Format("BR Sum: {0}, took: {1}", sum, stopwatch.ElapsedMilliseconds));


            // Red channel only
            stopwatch.Reset();
            stopwatch.Start();


            sum = 0;


            for (int j = 0; j != snoop.Height; j++)
            {
                for (int i = 0; i != snoop.Width; i++)
                {
                    var col = snoop.NovaGetPixel(i, j);
                    sum += col.R + col.G + col.B;
                }
            }


            Trace.WriteLine(string.Format("BN Sum: {0}, took: {1}", sum, stopwatch.ElapsedMilliseconds));

            // Set using blaster
            stopwatch.Reset();
            stopwatch.Start();

            sum = 0;


            for (int j = 0; j != snoop.Height; j++)
            {
                for (int i = 0; i != snoop.Width; i++)
                {
                    snoop.SetPixel(i, j, Color.Red);
                }
            }


            var a = string.Format("Set 1, took: {0}", stopwatch.ElapsedMilliseconds);
            Trace.WriteLine(a);
        }

        static void TestGetPixel(Bitmap bmp)
        {
            // Deep copy the bitmap
            var bmpClone = bmp.Clone() as Bitmap;

            // Now iterate over the bitmap clone and compare each pixel
            // 
            using (var snoop = new BmpPixelSnoop(bmpClone))
            { 
                Console.WriteLine("Testing GetPixel()");

                for (int j = 0; j != bmp.Height; j++)
                {
                    for (int i = 0; i != bmp.Width; i++)
                    {
                        var p1 = bmp.GetPixel(i, j);
                        var p2 = snoop.GetPixel(i, j);
                        if (p1 != p2)
                        {
                            Console.WriteLine(string.Format("Pixel at ({0}, {1}) does not match!", i, j));
                            return;
                        }
                    }
                }
            }

            Console.WriteLine("GetPixel() OK");
        }



        static void TestSetPixel(Bitmap bmp)
        {
            // Deep copy the bitmap
            var bmpClone = bmp.Clone() as Bitmap;

            // Now iterate over the bitmap clone and compare each pixel
            // 
            using (var snoop = new BmpPixelSnoop(bmpClone))
            {
                Console.WriteLine("Testing SetPixel()");

                for (int j = 0; j != bmp.Height; j++)
                {
                    for (int i = 0; i != bmp.Width; i++)
                    {
                        var p1 = bmp.GetPixel(i, j);
                        snoop.SetPixel(i, j, p1);
                        var p2 = snoop.GetPixel(i, j);
                        if (p1 != p2)
                        {
                            Console.WriteLine(string.Format("Pixel at ({0}, {1}) does not match!", i, j));
                            return;
                        }
                    }
                }


            }

            Console.WriteLine("SetPixel() OK");
        }

        static void TestGetPixelSpeed(Bitmap bmp)
        {
            Console.WriteLine("Testing GetPixel() Speed");
            // Deep copy the bitmap
            var bmpClone = bmp.Clone() as Bitmap;

            int height = bmpClone.Height;
            int width = bmpClone.Width;

            long sum = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int j = 0; j != height; j++)
            {
                for (int i = 0; i != width; i++)
                {
                    sum += bmpClone.GetPixel(i, j).R;
                }
            }

            var time = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();

            long snoopSum = 0;

            using (var snoop = new BmpPixelSnoop(bmpClone))
            {
                for (int j = 0; j != snoop.Height; j++)
                {
                    for (int i = 0; i != snoop.Width; i++)
                    {
                        snoopSum += snoop.GetPixel(i, j).R;
                    }
                }
            }

            var snoopTime = stopwatch.ElapsedMilliseconds;

            if (sum != snoopSum)
            {
                Console.WriteLine("Pixel sums don't match!");
                return;
            }

            Console.WriteLine(string.Format("Bitmap.GetPixel() took {0}ms, BmpPixelSnoop.GetPixel() took {1}ms", time, snoopTime));
        }

    }
}
