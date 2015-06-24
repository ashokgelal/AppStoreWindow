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
using AppKit;
using Foundation;
using CoreGraphics;

namespace AshokGelal.AppStoreWindow
{
    internal class TitleBarContainer : NSView
    {
        internal float MouseDragDetectionThreshold { get; set; }

        internal TitleBarContainer(CGRect frame)
            : base(frame)
        {
            MouseDragDetectionThreshold = 1;
        }

        public override void MouseDragged(NSEvent theEvent)
        {
            if (Window.MovableByWindowBackground ||
                (Window.StyleMask & NSWindowStyle.FullScreenWindow) == NSWindowStyle.FullScreenWindow)
            {
                base.MouseDragged(theEvent);
                return;
            }

            var @where = Window.ConvertBaseToScreen(theEvent.LocationInWindow);
            var origin = Window.Frame.Location;
            nfloat deltaX = 0.0f;
            nfloat deltaY = 0.0f;

            while ((theEvent != null) && (theEvent.Type != NSEventType.LeftMouseUp))
            {
                using (new NSAutoreleasePool())
                {
                    var now = Window.ConvertBaseToScreen(theEvent.LocationInWindow);
                    deltaX += now.X - @where.X;
                    deltaY += now.Y - @where.Y;

                    if (Math.Abs(deltaX) >= MouseDragDetectionThreshold ||
                        Math.Abs(deltaY) >= MouseDragDetectionThreshold)
                    {
                        origin.X += deltaX;
                        origin.Y += deltaY;

                        Window.SetFrameOrigin(origin);
                        deltaX = 0f;
                        deltaY = 0f;
                    }

                    @where = now;
                    theEvent = NSApplication.SharedApplication.NextEvent(NSEventMask.LeftMouseDown | NSEventMask.LeftMouseDragged | NSEventMask.LeftMouseUp, NSDate.DistantFuture, NSRunLoopMode.EventTracking.ToString(), true);
                }
            }
        }
    }
}