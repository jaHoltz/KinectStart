using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Kinect;

namespace KinectStart
{
  /// <summary>
  /// Contains helper functions used to display the depth 
  /// </summary>
  public static class depthHelper
  {
    // Globals used to determine the depths we are interested in
    private const int MaxDepthDistance = 4000;
    private const int MinDepth = 850;
    private const int MaxDepthOffset = 3150;

    /*
     * "Slices" the depth image, returns a bitmap source with the pixels 
     * colored so that no objects
     * outside of a certain depth range are colored.
     * Coloration taken predominantly from the microsoft depth tutorial, 
     * slicing idea updated from Erik Climczak,
     * at http://blogs.claritycon.com/blog/2012/11/blob-tracking-kinect-opencv-wpf/
     **/
    public static void depthSlice(this DepthImageFrame depthImage, WriteableBitmap bitmap, int min, int max)
    {
      int width = depthImage.Width;
      int height = depthImage.Height;

      //Raw depth data from kinect
      short[] rawDepthData = new short[depthImage.PixelDataLength];
      depthImage.CopyPixelDataTo(rawDepthData);

      var pixels = new byte[height * width * 4];

      //Constants for the positions of the relevant colors (Bgr32)
      const int BlueIndex = 0;
      const int GreenIndex = 1;
      const int RedIndex = 2;

      // Loop over two indicies, 
      // one into the depth pixels, the other into the color
      // pixel array that will be output 
      for (int depthIndex = 0, colorIndex = 0;
          depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
          depthIndex++, colorIndex += 4)
      {

        // Calculate the distance in mm 
        // represented by the two depth bytes (bitshifting)
        int depth = rawDepthData[depthIndex]
            >> DepthImageFrame.PlayerIndexBitmaskWidth;
        //Calculate the player value for the given index
        int player = rawDepthData[depthIndex] &
            DepthImageFrame.PlayerIndexBitmask;

        // Map the distance to an intesity that can be represented in RGB
        var intensity = calculateIntensity(depth);

        if (depth > min && depth < max)
        {
          // Apply the intensity to the color channels
          pixels[colorIndex + BlueIndex] = intensity; //blue
          pixels[colorIndex + GreenIndex] = intensity; //green
          pixels[colorIndex + RedIndex] = intensity; //red     

          /* Colors a player (if it decides something is a player
         * the relevant pixels will be gold)
         * */
          if (player > 0)
          {
            pixels[colorIndex + BlueIndex] = Colors.Gold.B;
            pixels[colorIndex + GreenIndex] = Colors.Gold.G;
            pixels[colorIndex + RedIndex] = Colors.Gold.R;
          }
        }
      }

      //Update the given writeableBitmap with the new pixel information
      bitmap.WritePixels(
        new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
        pixels, bitmap.PixelWidth * sizeof(int), 0);
    }

    /*
     * Calculates the intentsity based on the given depth value of a pixel.
     * This is a byte with a value between 0 and 255, 
     * which will be mapped to RGB pixels and used to color the depthImage
     * */
    public static byte calculateIntensity(int depth)
    {
      int newMax = depth - MinDepth;
      // Uses an offset to calculate values within the range
      return (byte)(255 - (255 * Math.Max(depth - MinDepth, 0)
          / (MaxDepthOffset)));
    }
  }
}
