# DotNetPixelSnoop
Faster GetPixel() and SetPixel() functionality for .NET Bitmaps

To Use:

Include the file BmpPixelSnoop.cs into your project.

And then use BmpPixelSnoop like this:

using (var snoop = new BmpPixelSnoop(theBitmap))
{
  // Now use the faster GetPixel() and SetPixel(), e.g.
  var col = snoop.GetPixel(0, 0);
}
