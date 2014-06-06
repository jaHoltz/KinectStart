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
  /// Interaction logic for MainWindow.xaml
  /// Main handling for entire project
  /// Displays data from the kinect
  /// </summary>
  public partial class MainWindow : Window
  {
    //Global variables
    KinectSensor sensor;
    WriteableBitmap depthBitmap;
    WriteableBitmap sliceBitmap;


    public MainWindow()
    {
      InitializeComponent();
      //Start the Event Handlers on initialize
      this.Loaded += MainWindow_Loaded;
      this.Closing += MainWindow_Closing;
      this.MouseDown += MainWindow_MouseDown;
    }

    //Event handler for main window loading
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      //Find a kinect and just pick one to be the sensor for our program
      //assumes we aren't using multiple kinects
      foreach (var curSensor in KinectSensor.KinectSensors)
      {
        if (curSensor.Status == KinectStatus.Connected)
        {
          sensor = curSensor;
          break;
        }
      }

      //Setup our sensor and get info if not null
      if (sensor != null)
      {
        //Enables the depth and color streams (we want these)
        sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
        sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
        //Enables the skeleton tracker (I think we want to play with this)
        //Also for testing player identification this is necessary
        sensor.SkeletonStream.Enable();
        sensor.AllFramesReady += this.sensor_AllFramesReady;

        //Start it if we still have a sensor
        try
        {
          this.sensor.Start();

          //Initialize a pair of writeable bitmaps after starting the sensor
          depthBitmap = new WriteableBitmap
                (this.sensor.DepthStream.FrameWidth,
                this.sensor.DepthStream.FrameHeight,
                96.0, 96.0, PixelFormats.Bgr32, null);

          sliceBitmap = new WriteableBitmap
              (this.sensor.DepthStream.FrameWidth,
              this.sensor.DepthStream.FrameHeight,
              96.0, 96.0, PixelFormats.Bgr32, null);

          //depthDisplay.Source = depthBitmap;
          depthSliceDisplay.Source = sliceBitmap;

        }
        catch (IOException)
        {
          this.sensor = null;
        }
      }

    }

    /*
     * Event which triggers when all frames are ready from the kinect
     * (will operate at the speed of the slowest camera)
     **/
    private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
    {
      using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
      {
        using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
        {

          if (depthFrame != null)
          {
            // Display an actual slice 
            // (smaller range should only display things within that range)
            //Range can be tweaked via sliders on application
            depthFrame.depthSlice(sliceBitmap, (int)minSlider.Value, (int)maxSlider.Value);
          }

          if (colorFrame != null)
          {
            //Initializes a byte array to the size 
            //of colorFrame's pixelDataLength
            byte[] colorPixels = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo(colorPixels);
            //Number of pixels in a row (based on bgr32 format)
            int stride = colorFrame.Width * 4;
            /*
            * Creates a bitmapsource based on the colorFrame coming 
            * from the kinect (in bgr32 using our calculated stride)
            * and displays it to the image frame named colorDisplay
            **/
            colorDisplay.Source =
                BitmapSource.Create
                (colorFrame.Width, colorFrame.Height,
                96, 96, PixelFormats.Bgr32, null, colorPixels, stride);
          }
        }
      }
    }

    void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
      this.DragMove();
    }

    private void CloseBtnClick(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    // Event handler for the closing of the main window (stops the kinect)
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (sensor != null)
      {
        sensor.Stop();
      }
    }


  }
}
