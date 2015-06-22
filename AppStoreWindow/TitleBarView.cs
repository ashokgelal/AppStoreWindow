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
using CoreGraphics;
using Foundation;

namespace AshokGelal.AppStoreWindow
{
    /// <summary>
    /// Draws a default OSX title bar.
    /// </summary>
    public class TitleBarView : NSView
    {
        private const float CORNER_CLIP_RADIUS = 4.0f;
        private CGSize TITLE_DOCUMENT_BUTTON_OFFSET = new CGSize(4.0f, 1.0f);
        private const float TITLE_VERSIONS_BUTTON_OFFSET = -1.0f;
        private const float TITLE_DOCUMENT_STATUS_X_OFFSET = -20.0f;
        private CGSize TITLE_MARGINS = new CGSize(8.0f, 2.0f);
        private CGImage _noiseImageRef;

        private CGRect BaselineSeparatorFrame
        {
            get
            {
                return new CGRect(0, Bounds.GetMinY(), Bounds.Width, 1);
            }
        }

        public TitleBarView(CGRect empty)
            : base(empty)
        {
        }

        public override void MouseUp(NSEvent theEvent)
        {
            if (theEvent.ClickCount != 2)
                return;
            const string mdAppleMiniaturizeOnDoubleClickKey = "AppleMiniaturizeOnDoubleClick";
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            var shouldMiniaturize = userDefaults.BoolForKey(mdAppleMiniaturizeOnDoubleClickKey);
            if (shouldMiniaturize)
            {
                Window.PerformMiniaturize(this);
            }
        }

        private void DrawWindowBackgroundGradient(CGRect drawingRect, CGPath clippingPath)
        {
            var window = (AppStoreWindow)Window;
            clippingPath.ApplyClippingPathInCurrentContext();
            if (window.IsTextured())
            {
                // If this is a textured window, we can draw the real background gradient and noise pattern
                var contentBorderThickness = window.TitleBarHeight;
                if (window.IsFullScreen())
                {
                    contentBorderThickness -= window.MinimumTitleBarHeight();
                }
                window.SetAutorecalculatesContentBorderThickness(false, NSRectEdge.MaxYEdge);
                window.SetContentBorderThickness(contentBorderThickness, NSRectEdge.MaxYEdge);
                NSGraphicsExtensions.DrawWindowBackground(drawingRect);
            }
            else
            {
                // Not textured, we have to fake the background gradient and noise pattern
                var drawsAsMainWindow = window.DrawsAsMainWindow();
                var startColor = drawsAsMainWindow ? window.TitleBarStartColor : window.InactiveTitleBarStartColor;
                var endColor = drawsAsMainWindow ? window.TitleBarEndColor : window.InactiveTitleBarEndColor;

                if (startColor == default(NSColor))
                    startColor = AppStoreWindow.DefaultTitleBarStartColor(drawsAsMainWindow);
                if (endColor == default(NSColor))
                    endColor = AppStoreWindow.DefaultTitleBarEndColor(drawsAsMainWindow);

                var context = NSGraphicsContext.CurrentContext.GraphicsPort;
                var gradient = DrawingHelper.CreateGraidentWithColors(startColor, endColor);
                context.DrawLinearGradient(gradient, new CGPoint(drawingRect.GetMidX(), drawingRect.GetMinY()), new CGPoint(drawingRect.GetMidX(), drawingRect.GetMaxY()), 0);

                if (drawsAsMainWindow)
                {
                    var noiseRect = new CGRect(1.0f, 1.0f, drawingRect.Width, drawingRect.Height);

                    if (window.ShowBaselineSeparator)
                    {
                        var separatorHeight = BaselineSeparatorFrame.Height;
                        noiseRect.Y -= separatorHeight;
                        noiseRect.Height += separatorHeight;
                    }

                    NSGraphicsContext.CurrentContext.SaveGraphicsState();
                    var noiseClippingPath = DrawingHelper.CreateClippingPath(noiseRect, CORNER_CLIP_RADIUS);
                    context.AddPath(noiseClippingPath);
                    context.Clip();
                    DrawNoise(0.1f);
                    NSGraphicsContext.CurrentContext.RestoreGraphicsState();
                }
            }
            if (window.ShowBaselineSeparator)
            {
                DrawBaselineSeparator(BaselineSeparatorFrame);
            }
        }

        private void DrawNoise(float opacity)
        {
            if (_noiseImageRef == null)
            {
                const int width = 124;
                const int height = width;
                const int size = width * height;

                var rgba = new byte[size];
                var random = new Random(120);
                for (var i = 0; i < size; i++)
                {
                    rgba[i] = (byte)(random.Next() % 256);
                }
                var colorSpace = CGColorSpace.CreateDeviceGray();
                var bitmapContext = new CGBitmapContext(rgba, width, height, 8, width, colorSpace, CGImageAlphaInfo.None);
                _noiseImageRef = bitmapContext.ToImage();
            }

            var context = NSGraphicsContext.CurrentContext.GraphicsPort;
            NSGraphicsContext.CurrentContext.SaveGraphicsState();
            context.SetAlpha(opacity);
            context.SetBlendMode(CGBlendMode.Screen);
            var scaleFactor = Window.BackingScaleFactor;
            context.ScaleCTM(1f / scaleFactor, 1f / scaleFactor);
            var imageRect = new CGRect(CGPoint.Empty, new CGSize(_noiseImageRef.Width, _noiseImageRef.Height));
            context.DrawTiledImage(imageRect, _noiseImageRef);
            NSGraphicsContext.CurrentContext.RestoreGraphicsState();
        }

        private void DrawBaselineSeparator(CGRect separatorFrame)
        {
            var window = (AppStoreWindow)Window;
            var drawsAsMainWindow = window.DrawsAsMainWindow();
            var bottomColor = drawsAsMainWindow ? window.BaselineSeparatorColor : window.InactiveBaselineSeparatorColor;
            if (bottomColor == default(NSColor))
                bottomColor = AppStoreWindow.DefaultBaselineSeparatorColor(drawsAsMainWindow);

            bottomColor.Set();

            NSGraphics.RectFill(separatorFrame);
            separatorFrame.Y += separatorFrame.Height;
            separatorFrame.Height = 1.0f;
            NSColor.FromDeviceWhite(1.0f, 0.12f).SetFill();
            NSBezierPath.FromRect(separatorFrame).Fill();
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            var window = ((AppStoreWindow)Window);
            var drawsAsMainWindow = window.DrawsAsMainWindow();
            // Start by filling the title bar area with black in fullscreen mode to match native apps
            // Custom title bar drawing blocks can simply override this by not applying the clipping path
            if (window.IsFullScreen())
            {
                NSColor.Black.SetFill();
                NSBezierPath.FromRect(Bounds).Fill();
            }

            CGPath clippingPath;
            var drawingRect = Bounds;

            if (window.TitleBarDrawingCallbackAction != null)
            {
                clippingPath = DrawingHelper.CreateClippingPath(drawingRect, CORNER_CLIP_RADIUS);
                window.TitleBarDrawingCallbackAction(drawsAsMainWindow, drawingRect, clippingPath);
            }
            else
            {
                // There's a thin whitish line between the darker gray window border line
                // and the gray noise textured gradient; preserve that when drawing a native title bar
                var clippingRect = drawingRect;
                clippingRect.Height -= 1;
                clippingPath = DrawingHelper.CreateClippingPath(clippingRect, CORNER_CLIP_RADIUS);

                DrawWindowBackgroundGradient(drawingRect, clippingPath);
            }

            if (window.ShowsTitle && ((window.StyleMask & NSWindowStyle.FullScreenWindow) == 0 || window.ShowsTitleInFullscreen))
            {
                NSDictionary dictionary;
                var titleTextRect = GetTitleFrame(out dictionary, window);

                if (window.VerticallyCenterTitle)
                {
                    titleTextRect.Y = (float)Math.Floor(drawingRect.GetMidY() - titleTextRect.Height / 2f + 1);
                }

                var title = new NSAttributedString(window.Title, dictionary);
                title.DrawString(titleTextRect);
            }
        }

        private CGRect GetTitleFrame(out NSDictionary titleTextStyles, AppStoreWindow window)
        {
            var drawsAsMainWindow = window.DrawsAsMainWindow();
            var titleTextShadow = drawsAsMainWindow ? window.TitleTextShadow : window.InactiveTitleTextShadow;
            if (titleTextShadow == null)
            {
                titleTextShadow = new NSShadow
                {
                    ShadowBlurRadius = 0f,
                    ShadowOffset = new CGSize(0f, -1.0f),
                    ShadowColor = NSColor.FromDeviceWhite(1.0f, 0.5f),
                };
            }

            var titleTextColor = drawsAsMainWindow ? window.TitleTextColor : window.InactiveTitleTextColor;
            if (titleTextColor == default(NSColor))
            {
                titleTextColor = AppStoreWindow.DefaultTitleTextColor(drawsAsMainWindow);
            }
            var titleFont = window.TitleFont ??
                            NSFont.TitleBarFontOfSize(NSFont.SystemFontSizeForControlSize(NSControlSize.Regular));

            var titleParagraphStyle = (NSParagraphStyle)NSParagraphStyle.DefaultParagraphStyle.MutableCopy();//new NSParagraphStyle { LineBreakMode = NSLineBreakMode.TruncatingTail };
            titleParagraphStyle.LineBreakMode = NSLineBreakMode.TruncatingTail;

            titleTextStyles = NSDictionary.FromObjectsAndKeys(
                new object[]
                {
                    titleFont, titleTextColor,
                    titleTextShadow, titleParagraphStyle
                },
                new object[]
                {
                    NSStringAttributeKey.Font, NSStringAttributeKey.ForegroundColor,
                    NSStringAttributeKey.Shadow, NSStringAttributeKey.ParagraphStyle
                });

            var titleSize = new NSAttributedString(window.Title, titleTextStyles).Size;
            var titleTextRect = new CGRect(0, 0, titleSize.Width, titleSize.Height);

            var docIconButton = window.StandardWindowButton(NSWindowButton.DocumentIconButton);
            var versionButton = window.StandardWindowButton(NSWindowButton.DocumentVersionsButton);
            var closeButton = window.ButtonToLayout(NSWindowButton.CloseButton);
            var minimizeButton = window.ButtonToLayout(NSWindowButton.MiniaturizeButton);
            var zoomButton = window.ButtonToLayout(NSWindowButton.ZoomButton);

            if (docIconButton != null)
            {
                var docIconButtonFrame = ConvertRectFromView(docIconButton.Frame, docIconButton.Superview);
                titleTextRect.X = docIconButtonFrame.GetMaxX() + TITLE_DOCUMENT_BUTTON_OFFSET.Width;
                titleTextRect.Y = docIconButtonFrame.GetMidY() - titleSize.Height / 2f +
                TITLE_DOCUMENT_BUTTON_OFFSET.Height;
            }
            else if (versionButton != null)
            {
                var versionsButtonFrame = ConvertRectFromView(versionButton.Frame, versionButton.Superview);
                titleTextRect.X = versionsButtonFrame.GetMinX() - titleSize.Width + TITLE_VERSIONS_BUTTON_OFFSET;

                var document = ((NSWindowController)window.WindowController).Document;
                if (document.HasUnautosavedChanges || document.IsDocumentEdited)
                {
                    titleTextRect.X += TITLE_DOCUMENT_STATUS_X_OFFSET;
                }
            }
            else if (closeButton != null || minimizeButton != null || zoomButton != null)
            {
                var closeMaxX = closeButton == null ? 0f : closeButton.Frame.GetMaxX();
                var minimizeMaxX = minimizeButton == null ? 0f : minimizeButton.Frame.GetMaxX();
                var zoomMaxX = zoomButton == null ? 0f : zoomButton.Frame.GetMaxX();

                var adjustedX = NMath.Max(NMath.Max(closeMaxX, minimizeMaxX), zoomMaxX) + TITLE_MARGINS.Width;
                var proposedX = Bounds.GetMidX() - titleSize.Width / 2f;
                titleTextRect.X = NMath.Max(proposedX, adjustedX);
            }
            else
            {
                titleTextRect.X = Bounds.GetMidX() - titleSize.Width / 2f;
            }

            var fullScreenButton = window.ButtonToLayout(NSWindowButton.FullScreenButton);
            if (fullScreenButton != null)
            {
                var fullScreenX = fullScreenButton.Frame.X;
                var maxTitleX = titleTextRect.GetMaxX();
                if ((fullScreenX - TITLE_MARGINS.Width) < titleTextRect.GetMaxX())
                {
                    titleTextRect.Width = titleTextRect.Size.Width - (maxTitleX - fullScreenX) - TITLE_MARGINS.Width;
                }
            }

            titleTextRect.Y = Bounds.GetMaxY() - titleSize.Height - TITLE_MARGINS.Height;

            return titleTextRect;
        }
    }
}