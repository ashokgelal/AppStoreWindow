// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace AppStoreWindowSample
{
    [Register("MainWindowController")]
    partial class MainWindowController
    {
        [Outlet]
        AppKit.NSButton centerFullScreenButton { get; set; }

        [Outlet]
        AppKit.NSButton centerTrafficLightButton { get; set; }

        [Outlet]
        AppKit.NSSlider fullScreenRightMarginSlider { get; set; }

        [Outlet]
        AppKit.NSButton showBaselineSeparatorButton { get; set; }

        [Outlet]
        AppKit.NSButton texturedWindowButton { get; set; }

        [Outlet]
        AppKit.NSSlider titleBarHeightSlider { get; set; }

        [Outlet]
        AppKit.NSSlider trafficLightLeftMarginSlider { get; set; }

        [Outlet]
        AppKit.NSSlider trafficLightSeparationSlider { get; set; }

        [Outlet]
        AppKit.NSButton verticallyCenterTitleButton { get; set; }

        [Outlet]
        AppKit.NSButton verticalTrafficLightButton { get; set; }

        [Action("checkboxAction:")]
        partial void checkboxAction(Foundation.NSObject sender);

        [Action("createWindowController:")]
        partial void createWindowController(Foundation.NSObject sender);

        [Action("showSheetAction:")]
        partial void showSheetAction(Foundation.NSObject sender);

        [Action("sliderAction:")]
        partial void sliderAction(Foundation.NSObject sender);

        void ReleaseDesignerOutlets()
        {
            if (centerFullScreenButton != null)
            {
                centerFullScreenButton.Dispose();
                centerFullScreenButton = null;
            }

            if (centerTrafficLightButton != null)
            {
                centerTrafficLightButton.Dispose();
                centerTrafficLightButton = null;
            }

            if (fullScreenRightMarginSlider != null)
            {
                fullScreenRightMarginSlider.Dispose();
                fullScreenRightMarginSlider = null;
            }

            if (showBaselineSeparatorButton != null)
            {
                showBaselineSeparatorButton.Dispose();
                showBaselineSeparatorButton = null;
            }

            if (texturedWindowButton != null)
            {
                texturedWindowButton.Dispose();
                texturedWindowButton = null;
            }

            if (titleBarHeightSlider != null)
            {
                titleBarHeightSlider.Dispose();
                titleBarHeightSlider = null;
            }

            if (trafficLightLeftMarginSlider != null)
            {
                trafficLightLeftMarginSlider.Dispose();
                trafficLightLeftMarginSlider = null;
            }

            if (trafficLightSeparationSlider != null)
            {
                trafficLightSeparationSlider.Dispose();
                trafficLightSeparationSlider = null;
            }

            if (verticallyCenterTitleButton != null)
            {
                verticallyCenterTitleButton.Dispose();
                verticallyCenterTitleButton = null;
            }

            if (verticalTrafficLightButton != null)
            {
                verticalTrafficLightButton.Dispose();
                verticalTrafficLightButton = null;
            }
        }
    }

    [Register("MainWindow")]
    partial class MainWindow
    {
		
        void ReleaseDesignerOutlets()
        {
        }
    }
}
