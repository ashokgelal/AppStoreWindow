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

using AppKit;
using CoreGraphics;
using System.Runtime.InteropServices;
using ObjCRuntime;
using System;

namespace AshokGelal.AppStoreWindow
{
    internal static class DrawingHelper
    {
        internal static CGPath CreateClippingPath(CGRect rect, float radius)
        {
            var path = new CGPath();
            path.MoveToPoint(rect.GetMinX(), rect.GetMinY());
            path.AddLineToPoint(rect.GetMinX(), rect.GetMaxY() - radius);
            path.AddArcToPoint(rect.GetMinX(), rect.GetMaxY(), rect.GetMinX() + radius, rect.GetMaxY(), radius);
            path.AddLineToPoint(rect.GetMaxX() - radius, rect.GetMaxY());
            path.AddArcToPoint(rect.GetMaxX(), rect.GetMaxY(), rect.GetMaxX(), rect.GetMaxY() - radius, radius);
            path.AddLineToPoint(rect.GetMaxX(), rect.GetMinY());
            path.CloseSubpath();

            return path;
        }

        internal static CGGradient CreateGraidentWithColors(NSColor startingColor, NSColor endingColor)
        {
            var locations = new nfloat[] { 0.0f, 1.0f };
            var cgStartingcolor = startingColor.CGColor;
            var cgEndingColor = endingColor.CGColor;
            var colors = new[] { cgStartingcolor, cgEndingColor };
            return new CGGradient(CGColorSpace.CreateDeviceRGB(), colors, locations);
        }
    }

    internal static class PathExtensions
    {
        internal static void ApplyClippingPath(this CGPath path, CGContext ctx)
        {
            ctx.AddPath(path);
            ctx.Clip();
        }

        internal static void ApplyClippingPathInCurrentContext(this CGPath path)
        {
            ApplyClippingPath(path, NSGraphicsContext.CurrentContext.GraphicsPort);
        }
    }

    internal static class WindowExtensions
    {
        internal static bool IsFullScreen(this NSWindow window)
        {
            return (window.StyleMask & NSWindowStyle.FullScreenWindow) == NSWindowStyle.FullScreenWindow;
        }

        internal static bool DrawsAsMainWindow(this NSWindow window)
        {
            return window.IsMainWindow && NSApplication.SharedApplication.Active;
        }

        internal static bool IsTextured(this NSWindow window)
        {
            return (window.StyleMask & NSWindowStyle.TexturedBackground) == NSWindowStyle.TexturedBackground;
        }
    }

    internal static class NSGraphicsExtensions
    {
        [DllImport(Constants.AppKitLibrary, EntryPoint = "NSDrawWindowBackground")]
        public extern static void DrawWindowBackground(CGRect aRect);

        [DllImport(Constants.AppKitLibrary, EntryPoint = "NSSetFocusRingStyle")]
        public extern static void SetFocusRingStyle(NSFocusRingPlacement placement);

        [DllImport(Constants.AppKitLibrary, EntryPoint = "NSDisableScreenUpdates")]
        public extern static void DisableScreenUpdates();

        [DllImport(Constants.AppKitLibrary, EntryPoint = "NSEnableScreenUpdates")]
        public extern static void EnableScreenUpdates();
    }

    public static class RectangleExtensions
    {
        public static nfloat GetMidHeight(this CGRect rect)
        {
            return rect.Height * 0.5f;
        }
    }
}