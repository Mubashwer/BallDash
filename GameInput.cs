using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.UI.Xaml;
using Windows.UI.Input;
using Windows.UI.Core;


namespace Project
{
    public class GameInput
    {
        public Accelerometer accelerometer;
        public CoreWindow window;
        public GestureRecognizer gestureRecognizer;
        public GameInput()
        {
            // Get the accelerometer object
            accelerometer = Accelerometer.GetDefault();
            window = Window.Current.CoreWindow;

            // Set up the gesture recognizer.  In this example, it only responds to TranslateX, Scale and Tap events
            gestureRecognizer = new Windows.UI.Input.GestureRecognizer();
            gestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationScale | GestureSettings.Tap;

            // Register event handlers for pointer events
            window.PointerPressed += OnPointerPressed;
            window.PointerMoved += OnPointerMoved;
            window.PointerReleased += OnPointerReleased;
        }

        // Call the gesture recognizer when a pointer event occurs
        void OnPointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            try
            {
                gestureRecognizer.ProcessDownEvent(args.CurrentPoint);
            }
            catch { }
        }

        void OnPointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            try
            {
                gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints());
            }
            catch { }
        }

        void OnPointerReleased(CoreWindow sender, PointerEventArgs args)
        {
            try
            {
                gestureRecognizer.ProcessUpEvent(args.CurrentPoint);
            }
            catch { }
        }
    }
}