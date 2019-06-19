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

*NB* When you are snooping a bitmap, you cannot access the snooped bitmap using the normal functions until the BmpPixelSnoop object goes out of scope, for example when execution leaves the using() block in the code example below.  This is because the bitmap is locked (using LockBits()) when it's being snooped, it is unlocked when the snoop objev=ct is disposed.


Tests indcate that snoop's GetPixel() and SetPixel() methods can be 10 times faster than the originals.
