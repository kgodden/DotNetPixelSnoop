# DotNetPixelSnoop
Faster *GetPixel()* and *SetPixel()* functionality for .NET Bitmaps

The GetPixel() and SetPixel() functions of the .Net System.Drawing.Bitmap calss are incredibbly slow, to the point of being unuseable.  This project introduces a class called *BmpPixelSnoop* which wraps a .Net bitmap and provides much faster GetPixel() and SetPixel() access to the original bitmap.

A description can be found here:

http://www.ridgesolutions.ie/index.php/2019/06/19/a-faster-alternative-to-the-slow-getpixel-and-setpixel-for-net-bitmaps/

Currently BmpPixelSnoop works for bitmaps with a pixel format of PixelFormat.Format32bppArgb (a very common format!)

To Use:

Include the file *BmpPixelSnoop.cs* into your project.

And then use *BmpPixelSnoop* like this:

```csharp
using (var snoop = new BmpPixelSnoop(theBitmap))
{
  // Now use the faster GetPixel() and SetPixel(), e.g.
  var col = snoop.GetPixel(0, 0);
}
```

*NB* When you are snooping a bitmap, you cannot access the snooped bitmap using the normal functions until the BmpPixelSnoop object goes out of scope, (e.g. when execution leaves the using() block in the code example above).  This is because the bitmap is locked (using LockBits()) when it's being snooped, it is unlocked when the snoop object is disposed.


Tests indcate that snoop's GetPixel() and SetPixel() methods can be 10 times faster than the originals.

This repo contains a visual studio project with the class defined in BmpPixelSnoop.cs and some test code in Program.cs.  To use the class just include BmpPixelSnoop.cs into your project.

The only possible gotcha is that your project must be marked to 'Allow unsafe code' (in its project settings) to use this class! 

Sample output from the test code (in Program.cs):

```
Testing GetPixel()
GetPixel() OK
Testing SetPixel()
SetPixel() OK
Testing GetPixel() Speed
Bitmap.GetPixel() took 759ms, BmpPixelSnoop.GetPixel() took 67ms
Testing SetPixel() Speed
Bitmap.SetPixel() took 907ms, BmpPixelSnoop.SetPixel() took 72ms
```
