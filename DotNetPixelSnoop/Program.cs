using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace DotNetPixelSnoop
{
    class Program
    {
        /// <summary>
        /// Runs some correctness and performance tests
        /// for BmpPixelSnoop, see file BmpPixelSnoop.cs for
        /// the actual bitmap snoop code.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int testWidth = 1920;
            int testHeight = 1080;

            // First create a bitmap to test with
            var bmp = GenerateTestBitmap(testWidth, testHeight);

            try
            {
                // Test the BmpPixelSnoop.GetPixel()
                // works as expected
                TestGetPixel(bmp);

                // Test the BmpPixelSnoop.SetPixel()
                // works as expected
                TestSetPixel(bmp);

                // See how BmpPixelSnoop.GetPixel()'s performance
                // compares to that of the regular GetPixel()
                TestGetPixelSpeed(bmp);

                // See how BmpPixelSnoop.SetPixel()'s performance
                // compares to that of the regular SetPixel()
                TestSetPixelSpeed(bmp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed - " + ex.Message);
            }
        }

        /// <summary>
        /// Generates a bitmap that's filled in withe some random
        /// data that we can use for testing.
        /// </summary>
        /// <param name="width">The required width of the new bitmap</param>
        /// <param name="height">The required height of the new bitmap.</param>
        /// <returns>The newly created bitmap</returns>
        static Bitmap GenerateTestBitmap(int width, int height)
        {
            // Create a bitmap
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Now fill up the Bitmap with some random data
            // using the bitmap's normal SetPixel() function 
            //as we know that it works! ;-)
            var rand = new Random();

            for (int j = 0; j != bmp.Height; j++)
            {
                for (int i = 0; i != bmp.Width; i++)
                {
                    bmp.SetPixel(i, j, Color.FromArgb(rand.Next()));
                }
            }

            return bmp;
        }

        /// <summary>
        /// Test that GetPixel() works the same way as Bitmap.GetPixel()
        /// </summary>
        /// <param name="bmp">The bitmap to test with</param>
        static void TestGetPixel(Bitmap bmp)
        {
            Console.WriteLine("Testing GetPixel()");

            // Deep copy the bitmap
            var bmpClone = bmp.Clone() as Bitmap;

            // Now create a snoop over the clone and
            // iterate over both and compare each pixel
            // 
            using (var snoop = new BmpPixelSnoop(bmpClone))
            { 
 
                for (int j = 0; j != bmp.Height; j++)
                {
                    for (int i = 0; i != bmp.Width; i++)
                    {
                        // Normal Butmap.GetPixel()
                        var p1 = bmp.GetPixel(i, j);

                        // BmpPixelSnoop's GetPixel()
                        var p2 = snoop.GetPixel(i, j);

                        // Are they the same (they should be..)
                        if (p1 != p2)
                        {
                            throw new Exception(string.Format("Pixel at ({0}, {1}) does not match!", i, j));
                        }
                    }
                }
            }

            Console.WriteLine("GetPixel() OK");
        }

        /// <summary>
        /// Test that SetPixel() works the same way as Bitmap.SetPixel()
        /// </summary>
        /// <param name="bmp">The bitmap to test with</param>
        static void TestSetPixel(Bitmap bmp)
        {
            Console.WriteLine("Testing SetPixel()");

            // Create an empty target bitmap
            var bmpTarget = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);

            // We get each pixel from the input bitmap and
            // set to the corresponding pixel in the snooped bitmap.
            // Then we read back this pixel and check that it equals the 
            // original pixel value.
            // 
            using (var snoop = new BmpPixelSnoop(bmpTarget))
            {
                for (int j = 0; j != bmp.Height; j++)
                {
                    for (int i = 0; i != bmp.Width; i++)
                    {
                        // Get a pixel from the input bitmap
                        var p1 = bmp.GetPixel(i, j);

                        // Set it into the snooped bitmap
                        snoop.SetPixel(i, j, p1);

                        // Read it back from the snooped bitmap
                        var p2 = snoop.GetPixel(i, j);

                        // And compare with original.
                        if (p1 != p2)
                        {
                            throw new Exception(string.Format("Pixel at ({0}, {1}) does not match!", i, j));
                        }
                    }
                }

            }

            // Now test that the snooped bitmap has been released and
            // that we can call SetPixel() on it without an excpetion
            // being thrown...
            //
            try
            {
                bmpTarget.SetPixel(0, 0, System.Drawing.Color.Aqua);
            }
            catch
            {
                throw new Exception(string.Format("Could not write to bitmap, BitmapSnoop did not release it!"));
            }

            Console.WriteLine("SetPixel() OK");
        }

        /// <summary>
        /// Test how fast GetPixel() works compared to Bitmap.GetPixel()
        /// </summary>
        /// <param name="bmp">The bitmap to test with</param>
        static void TestGetPixelSpeed(Bitmap bmp)
        {
            Console.WriteLine("Testing GetPixel() Speed");

            // Deep copy the bitmap
            var bmpClone = bmp.Clone() as Bitmap;

            // Calculate a simple sum over all of the pixels
            // in the original bitmap.
            long sum = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int j = 0; j != bmpClone.Height; j++)
            {
                for (int i = 0; i != bmpClone.Width; i++)
                {
                    var col = bmpClone.GetPixel(i, j);

                    sum += col.R +
                            col.G +
                            col.B;
                }
            }

            var time = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();

            long snoopSum = 0;

            // Calculate a simple sum over all of the pixels
            // in the snooped bitmap.
            using (var snoop = new BmpPixelSnoop(bmpClone))
            {
                for (int j = 0; j != snoop.Height; j++)
                {
                    for (int i = 0; i != snoop.Width; i++)
                    {
                        var col = snoop.GetPixel(i, j);

                        snoopSum += col.R +
                                    col.G +
                                    col.B;
                    }
                }
            }

            var snoopTime = stopwatch.ElapsedMilliseconds;

            // Just make sure our sums match
            if (sum != snoopSum)
            {
                throw new Exception("Pixel sums don't match!");
            }

            Console.WriteLine(string.Format("Bitmap.GetPixel() took {0}ms, BmpPixelSnoop.GetPixel() took {1}ms", time, snoopTime));
        }

        /// <summary>
        /// Test how fast SetPixel() works compared to Bitmap.SetPixel()
        /// </summary>
        /// <param name="bmp">The bitmap to test with</param>
        static void TestSetPixelSpeed(Bitmap bmp)
        {
            Console.WriteLine("Testing SetPixel() Speed");

            // Deep copy the bitmap
            var bmpClone = bmp.Clone() as Bitmap;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Loop through the bitmap and set each pixel's value
            // to something interesting..
            for (int j = 0; j != bmpClone.Height; j++)
            {
                for (int i = 0; i != bmpClone.Width; i++)
                {
                    var col = Color.FromArgb(i % 255, j % 255, (i + j) % 255, (i * j) % 255);
                    bmpClone.SetPixel(i, j, col);
                }
            }

            var time = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();

            // Now do the same on the snooped bitmap..
            using (var snoop = new BmpPixelSnoop(bmpClone))
            {
                for (int j = 1; j != snoop.Height; j++)
                {
                    for (int i = 0; i != snoop.Width; i++)
                    {
                        var col = Color.FromArgb(i % 255, j % 255, (i * j) % 255, (i + j) % 255);
                        snoop.SetPixel(i, j, col);
                    }
                }
            }

            var snoopTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine(string.Format("Bitmap.SetPixel() took {0}ms, BmpPixelSnoop.SetPixel() took {1}ms", time, snoopTime));
        }
    }
}
