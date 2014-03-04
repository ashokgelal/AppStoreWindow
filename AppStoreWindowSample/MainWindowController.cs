// MonoMac port of Indragie Karunaratne's awesome INAppStoreWindow Objective-C library.
//
// Copyright Ashok Gelal 2014. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AshokGelal.AppStoreWindow;

namespace AppStoreWindowSample
{
    public partial class MainWindowController : MonoMac.AppKit.NSWindowController
    {
        #region Properties

        public new AppStoreWindow Window
        {
            get
            {
                return (AppStoreWindow)base.Window;
            }
        }

        #endregion

        #region Constructors

        // Called when created from unmanaged code
        public MainWindowController(IntPtr handle) : base(handle)
        {
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
        }
        // Call to load from the XIB/NIB file
        public MainWindowController() : base("MainWindow")
        {
        }

        public override void WindowDidLoad()
        {
            base.WindowDidLoad();
            Window.TitleBarHeight = (float)titleBarHeightSlider.DoubleValue;
            Window.ShowBaselineSeparator = showBaselineSeparatorButton.State == NSCellStateValue.On;
            Window.ShowsTitle = true;
            Window.VerticallyCenterTitle = verticallyCenterTitleButton.State == NSCellStateValue.On;
            Window.CenterFullScreenButton = centerFullScreenButton.State == NSCellStateValue.On;
            Window.HideTitleBarInFullScreen = true;
        }

        #endregion

        partial void checkboxAction(MonoMac.Foundation.NSObject sender)
        {
            var button = (NSButton)sender;
            var isChecked = button.State == NSCellStateValue.On;
            if (button.Equals(centerFullScreenButton))
            {
                Window.CenterFullScreenButton = isChecked;
                return;
            }
            if (button.Equals(showBaselineSeparatorButton))
            {
                Window.ShowBaselineSeparator = isChecked;
                return;
            }
            if (button.Equals(centerTrafficLightButton))
            {
                Window.CenterTrafficLightButtons = isChecked;
                return;
            }
            if (button.Equals(verticalTrafficLightButton))
            {
                Window.VerticalTrafficLightButtons = isChecked;
                return;
            }
            if (button.Equals(verticallyCenterTitleButton))
            {
                Window.VerticallyCenterTitle = isChecked;
                return;
            }
            if (button.Equals(texturedWindowButton))
            {
                if (isChecked)
                {
                    Window.StyleMask |= NSWindowStyle.TexturedBackground;
                }
                else
                {
                    Window.StyleMask &= ~NSWindowStyle.TexturedBackground;
                }
                return;
            }

        }

        partial void sliderAction(MonoMac.Foundation.NSObject sender)
        {
            var slider = (NSSlider)sender;
            var value = (float)slider.DoubleValue;
            if (slider.Equals(titleBarHeightSlider))
            {
                Window.TitleBarHeight = value;
                return;
            }
            if (slider.Equals(trafficLightLeftMarginSlider))
            {
                Window.TrafficLightButtonsLeftMargin = value;
                return;
            }
            if (slider.Equals(trafficLightSeparationSlider))
            {
                Window.TrafficLightSeparation = value;
                return;
            }
            if (slider.Equals(fullScreenRightMarginSlider))
            {
                Window.FullScreenButtonRightMargin = value;
                return;
            }

        }
    }
}

