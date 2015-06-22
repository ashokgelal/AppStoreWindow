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
//

using AppKit;
using CoreGraphics;
using Foundation;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace AshokGelal.AppStoreWindow
{
    /// <summary>
    ///  Creates a window similar to the Mac App Store window, with centered traffic lights and an enlarged title bar. 
    /// This does not handle creating the toolbar.
    /// </summary>
    public partial class AppStoreWindow : NSWindow
    {
        #region Fields

        /** Values chosen to match the defaults in OS X 10.9, which may change in future versions **/
        private const float WINDOW_DOCUMENT_ICON_BUTTON_ORIGIN_Y = 3.0f;
        private const float WINDOW_DOCUMENT_VERSIONS_BUTTON_ORIGIN_Y = 2.0f;
        private const float WINDOW_DOCUMENT_VERSIONS_DIVIDER_ORIGIN_Y = 2.0f;
        private const float EPSILON = 0.0000001f;
        private nfloat _cachedTitleBarHeight;
        private bool _centerFullScreenButton;
        private bool _centerTrafficLightButtons;
        private WindowButton _closeButton;
        private WindowButton _fullScreenButton;
        private nfloat _fullScreenButtonRightMargin;
        private WindowButton _minimizeButton;
        private nfloat _minimumTitleBarHeight;
        private bool _preventWindowFrameChange;
        private NSUrl _representedUrl;
        private bool _setFullScreenButtonRightMargin;
        private bool _showBaselineSeparator;
        private bool _showsDocumentProxyIcon;
        private bool _showsTitle;
        private TitleBarContainer _titleBarContainer;
        private nfloat _titleBarHeight;
        private TitleBarView _titleBarView;
        private nfloat _trafficLightButtonsLeftMargin;
        private nfloat _trafficLightLeftMargin;
        private nfloat _trafficLightSeparation;
        private bool _verticalTrafficLightButtons;
        private bool _verticallyCenterTitle;
        private WindowButton _zoomButton;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the title bar drawing callback action to implement custon drawing code for a windows' title bar.
        /// </summary>
        /// <value>The title bar drawing callback action.</value>
        public Action<bool, CGRect, CGPath> TitleBarDrawingCallbackAction { get; set; }

        /// <summary>
        /// Gets or sets the height of the title bar. By default, this is set to the standard title bar height.
        /// </summary>
        /// <value>The height of the title bar.</value>
        public nfloat TitleBarHeight
        {
            get { return _titleBarHeight; }
            set { SetTitleBarHeight(value, true); }
        }

        /// <summary>
        /// Gets or sets the container view for custom views added to the title bar.
        /// Add subviews to this view that you want to show in the title bar (e.g. buttons, a toolbar, etc.).
        /// This view can also be set if you want to use a different style title bar from the default one (textured, etc.).
        /// </summary>
        /// <value>The title bar view.</value>
        public TitleBarView TitleBarView
        {
            get { return _titleBarView; }
            set
            {
                if (value == null || _titleBarView == value)
                    return;
                if (_titleBarView != null)
                    _titleBarView.RemoveFromSuperview();
                _titleBarView = value;
                _titleBarView.Frame = _titleBarContainer.Bounds;
                _titleBarView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
                _titleBarContainer.AddSubview(_titleBarView);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the fullscreen button is vertically centered.
        /// </summary>
        /// <value><c>true</c> if center full screen button; otherwise, <c>false</c>.</value>
        public bool CenterFullScreenButton
        {
            get { return _centerFullScreenButton; }
            set
            {
                if (_centerFullScreenButton == value)
                    return;
                _centerFullScreenButton = value;
                LayoutTrafficLightsAndContent();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the traffic light buttons are vertically centered.
        /// </summary>
        /// <value><c>true</c> if center traffic light buttons; otherwise, <c>false</c>.</value>
        public bool CenterTrafficLightButtons
        {
            get { return _centerTrafficLightButtons; }
            set
            {
                if (_centerTrafficLightButtons == value)
                    return;
                _centerTrafficLightButtons = value;
                LayoutTrafficLightsAndContent();
                SetupTrafficLightsTrackingArea();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the traffic light buttons are displayed in vertical orientation.
        /// </summary>
        /// <value><c>true</c> if vertical traffic light buttons; otherwise, <c>false</c>.</value>
        public bool VerticalTrafficLightButtons
        {
            get { return _verticalTrafficLightButtons; }
            set
            {
                if (_verticalTrafficLightButtons == value)
                    return;
                _verticalTrafficLightButtons = value;
                LayoutTrafficLightsAndContent();
                SetupTrafficLightsTrackingArea();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the title is centered vertically.
        /// </summary>
        /// <value><c>true</c> if vertically center title; otherwise, <c>false</c>.</value>
        public bool VerticallyCenterTitle
        {
            get { return _verticallyCenterTitle; }
            set
            {
                if (_verticallyCenterTitle == value)
                    return;

                _verticallyCenterTitle = value;
                DisplayWindowAndTitleBar();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the title bar in fullscreen mode.
        /// </summary>
        /// <value><c>true</c> if hide title bar in full screen; otherwise, <c>false</c>.</value>
        public bool HideTitleBarInFullScreen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the 
        /// baseline separator between the window's title bar and content area.
        /// </summary>
        /// <value><c>true</c> if show baseline separator; otherwise, <c>false</c>.</value>
        public bool ShowBaselineSeparator
        {
            get { return _showBaselineSeparator; }
            set
            {
                if (_showBaselineSeparator == value)
                    return;
                _showBaselineSeparator = value;
                TitleBarView.NeedsDisplay = true;
            }
        }

        /// <summary>
        /// Gets or sets the distance between the traffic light buttons and the left edge of the window.
        /// </summary>
        /// <value>The traffic light buttons left margin.</value>
        public nfloat TrafficLightButtonsLeftMargin
        {
            get { return _trafficLightButtonsLeftMargin; }
            set
            {
                if (Math.Abs(_trafficLightButtonsLeftMargin - value) < EPSILON)
                    return;
                _trafficLightButtonsLeftMargin = value;
                LayoutTrafficLightsAndContent();
                DisplayWindowAndTitleBar();
                SetupTrafficLightsTrackingArea();
            }
        }

        /// <summary>
        /// Gets or sets the distance between the traffic light buttons and the top edge of the window.
        /// </summary>
        /// <value>The traffic light buttons top margin.</value>
        public nfloat TrafficLightButtonsTopMargin { get; set; }

        /// <summary>
        /// Gets or sets the distance between the fullscreen button and the right edge of the window.
        /// </summary>
        /// <value>The full screen button right margin.</value>
        public nfloat FullScreenButtonRightMargin
        {
            get { return _fullScreenButtonRightMargin; }
            set
            {
                if (Math.Abs(_fullScreenButtonRightMargin - value) < EPSILON)
                    return;

                _setFullScreenButtonRightMargin = true;
                _fullScreenButtonRightMargin = value;
                LayoutTrafficLightsAndContent();
                DisplayWindowAndTitleBar();
            }
        }

        /// <summary>
        /// Gets or sets the distance between the fullscreen button and the top edge of the window.
        /// </summary>
        /// <value>The full screen button top margin.</value>
        public float FullScreenButtonTopMargin { get; set; }

        /// <summary>
        /// Gets or sets the Number of points in any direction above which the window will be allowed to reposition.
        /// A Higher value indicates coarser movements but much reduced CPU overhead. Defaults to 1.
        /// </summary>
        /// <value>The mouse drag detection threshold.</value>
        public float MouseDragDetectionThreshold
        {
            get { return _titleBarContainer.MouseDragDetectionThreshold; }
            set { _titleBarContainer.MouseDragDetectionThreshold = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the window's title text. 
        /// If <c>true</c>, the title will be shown even if <c>TitleBarDrawingCallbackAction</c> is set. 
        /// To draw the title manually, set this property to <c>false</c> and draw the title using <c>TitleBarDrawingCallbackAction</c>.
        /// </summary>
        /// <value><c>true</c> to show title; otherwise, <c>false</c>.</value>
        public bool ShowsTitle
        {
            get { return _showsTitle; }
            set
            {
                if (_showsTitle == value)
                    return;
                _showsTitle = value;
                DisplayWindowAndTitleBar();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the window's title text in fullscreen mode.

        /// </summary>
        /// <value><c>true</c> if shows title in fullscreen; otherwise, <c>false</c>.</value>
        public bool ShowsTitleInFullscreen { get; set; }

        public bool ShowsDocumentProxyIcon
        {
            get { return _showsDocumentProxyIcon; }
            set
            {
                if (_showsDocumentProxyIcon == value)
                    return;
                _showsDocumentProxyIcon = value;
                DisplayWindowAndTitleBar();
            }
        }

        /// <summary>
        /// Spacing between the traffic light buttons.
        /// </summary>
        /// <value>The traffic light separation.</value>
        public nfloat TrafficLightSeparation
        {
            get { return _trafficLightSeparation; }
            set
            {
                if (Math.Abs(_trafficLightSeparation - value) < EPSILON)
                    return;
                _trafficLightSeparation = value;
                LayoutTrafficLightsAndContent();
                SetupTrafficLightsTrackingArea();
            }
        }

        /// <summary>
        /// Gets or sets the close button. If this property is nil, the default button will be used. 
        /// </summary>
        /// <value>The close button.</value>
        public WindowButton CloseButton
        {
            get { return _closeButton; }
            set
            {
                if (_closeButton == value)
                {
                    return;
                }
                _closeButton.Activated -= CloseButton_Activated;
                _closeButton.RemoveFromSuperview();

                _closeButton = value;
                _closeButton.Activated += CloseButton_Activated;
                _closeButton.SetFrameOrigin(StandardWindowButton(NSWindowButton.CloseButton).Frame.Location);
                ThemeFrameView.AddSubview(_closeButton);
            }
        }

        /// <summary>
        /// Gets or sets the minimize button. If this property is nil, the default button will be used.
        /// </summary>
        /// <value>The minimize button.</value>
        public WindowButton MinimizeButton
        {
            get { return _minimizeButton; }
            set
            {
                if (_minimizeButton == value)
                {
                    return;
                }
                _minimizeButton.Activated -= MinimizeButton_Activated;
                _minimizeButton.RemoveFromSuperview();

                _minimizeButton = value;
                _minimizeButton.Activated += MinimizeButton_Activated;
                _minimizeButton.SetFrameOrigin(StandardWindowButton(NSWindowButton.MiniaturizeButton).Frame.Location);
                ThemeFrameView.AddSubview(_minimizeButton);
            }
        }

        /// <summary>
        /// Gets or sets the zoom button. If this property is nil, the default button will be used.
        /// </summary>
        /// <value>The zoom button.</value>
        public WindowButton ZoomButton
        {
            get { return _zoomButton; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null);
                if (_zoomButton == value)
                {
                    return;
                }
                _zoomButton.Activated -= ZoomButton_Activated;
                _zoomButton.RemoveFromSuperview();

                _zoomButton = value;
                _zoomButton.Activated += ZoomButton_Activated;
                _zoomButton.SetFrameOrigin(StandardWindowButton(NSWindowButton.ZoomButton).Frame.Location);
                ThemeFrameView.AddSubview(_zoomButton);
            }
        }

        /// <summary>
        /// Gets or sets the full screen button. If this property is nil, the default button will be used. 
        /// </summary>
        /// <value>The full screen button.</value>
        public WindowButton FullScreenButton
        {
            get { return _fullScreenButton; }
            set
            {
                if (value == null || _fullScreenButton == value)
                {
                    return;
                }

                if (_fullScreenButton != null)
                {
                    _fullScreenButton.Activated -= FullScreenButton_Activated;
                    _fullScreenButton.RemoveFromSuperview();
                }

                _fullScreenButton = value;
                _fullScreenButton.Activated += FullScreenButton_Activated;
                _fullScreenButton.SetFrameOrigin(StandardWindowButton(NSWindowButton.FullScreenButton).Frame.Location);
                ThemeFrameView.AddSubview(_fullScreenButton);
            }
        }

        /// <summary>
        /// Gets or sets the title font.
        /// </summary>
        /// <value>The title font.</value>
        public NSFont TitleFont { get; set; }

        /// <summary>
        /// Gets or sets the start color of the title bar.
        /// </summary>
        /// <value>The color of the title bar start.</value>
        public NSColor TitleBarStartColor { get; set; }

        /// <summary>
        /// Gets or sets the end color of the title bar.
        /// </summary>
        /// <value>The color of the title bar end.</value>
        public NSColor TitleBarEndColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the baseline separator.
        /// </summary>
        /// <value>The color of the baseline separator.</value>
        public NSColor BaselineSeparatorColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the inactive baseline separator.
        /// </summary>
        /// <value>The color of the inactive baseline separator.</value>
        public NSColor InactiveBaselineSeparatorColor { get; set; }

        private NSView ThemeFrameView
        {
            get { return ContentView.Superview; }
        }

        /// <summary>
        /// Gets or sets the style mask.
        /// </summary>
        /// <value>The style mask.</value>
        public override NSWindowStyle StyleMask
        {
            get { return base.StyleMask; }
            set
            {
                // Prevent drawing artifacts when turning off NSTexturedBackgroundWindowMask before
                // exiting from full screen and then resizing the title bar; the problem is that internally
                // the content border is still set to the previous value, which confuses the system

                if (((StyleMask & NSWindowStyle.TexturedBackground) == NSWindowStyle.TexturedBackground) &&
                    ((value & NSWindowStyle.TexturedBackground) != NSWindowStyle.TexturedBackground))
                {
                    SetContentBorderThickness(0, NSRectEdge.MaxYEdge);
                    SetAutorecalculatesContentBorderThickness(true, NSRectEdge.MaxYEdge);
                }

                base.StyleMask = value;
                DisplayWindowAndTitleBar();
                ContentView.Display();
                _preventWindowFrameChange = false;
            }
        }

        /// <summary>
        /// Gets or sets the content view.
        /// </summary>
        /// <value>The content view.</value>
        public override NSView ContentView
        {
            get { return base.ContentView; }
            set
            {
                base.ContentView = value;
                RepositionContentView();
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return base.Title; }
            set
            {
                base.Title = value;
                LayoutTrafficLightsAndContent();
                DisplayWindowAndTitleBar();
            }
        }

        /// <summary>
        /// Gets or sets the max size of the window.
        /// </summary>
        /// <value>The size of the max.</value>
        public override CGSize MaxSize
        {
            get { return base.MaxSize; }
            set
            {
                base.MaxSize = value;
                LayoutTrafficLightsAndContent();
            }
        }

        /// <summary>
        /// Gets or sets the minimum size of the window.
        /// </summary>
        /// <value>The minimum size.</value>
        public override CGSize MinSize
        {
            get { return base.MinSize; }
            set
            {
                base.MinSize = value;
                LayoutTrafficLightsAndContent();
            }
        }

        /// <summary>
        /// Gets or sets the represented URL of the window.
        /// </summary>
        /// <value>The represented URL.</value>
        public override NSUrl RepresentedUrl
        {
            get { return _representedUrl; }
            set
            {
                _representedUrl = value;
                if (!ShowsDocumentProxyIcon)
                {
                    StandardWindowButton(NSWindowButton.DocumentIconButton).Image = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the start color of the inactive title bar.
        /// </summary>
        /// <value>The color of the inactive title bar start.</value>
        public NSColor InactiveTitleBarStartColor { get; set; }

        /// <summary>
        /// Gets or sets the end color of the inactive title bar.
        /// </summary>
        /// <value>The color of the inactive title bar end.</value>
        public NSColor InactiveTitleBarEndColor { get; set; }

        /// <summary>
        /// Gets or sets the title text shadow.
        /// </summary>
        /// <value>The title text shadow.</value>
        public NSShadow TitleTextShadow { get; set; }

        /// <summary>
        /// Gets or sets the inactive title text shadow.
        /// </summary>
        /// <value>The inactive title text shadow.</value>
        public NSShadow InactiveTitleTextShadow { get; set; }

        /// <summary>
        /// Gets or sets the color of the title text.
        /// </summary>
        /// <value>The color of the title text.</value>
        public NSColor TitleTextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the inactive title text.
        /// </summary>
        /// <value>The color of the inactive title text.</value>
        public NSColor InactiveTitleTextColor { get; set; }

        #endregion Properties

        #region Constructors

        public AppStoreWindow(IntPtr handle)
            : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public AppStoreWindow(NSCoder coder)
            : base(coder)
        {
        }

        public AppStoreWindow()
            : base()
        {
         
        }

        public AppStoreWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation)
            : base(contentRect, aStyle, bufferingType, deferCreation)
        {

        }

        public AppStoreWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, NSScreen screen)
            : base(contentRect, aStyle, bufferingType, deferCreation, screen)
        {
        }


        public override void AwakeFromNib()
        {
            DoInitialWindowSetup();
        }

        #endregion Constructors

        #region Methods

        private void DoInitialWindowSetup()
        {
            _showBaselineSeparator = true;
            _centerTrafficLightButtons = true;
            _titleBarHeight = _cachedTitleBarHeight = MinimumTitleBarHeight();
            _trafficLightButtonsLeftMargin = DefaultTrafficLightLeftMargin();
            TrafficLightButtonsTopMargin = 3f;
            FullScreenButtonTopMargin = 3f;
            _trafficLightSeparation = DefaultTrafficLightSeparation();

            HookInitialEvents();
            CreateTitlebarView();
            LayoutTrafficLightsAndContent();
            SetupTrafficLightsTrackingArea();
        }

        private void CreateTitlebarView()
        {
            var container = new TitleBarContainer(CGRect.Empty);
            var firstSubView = ThemeFrameView.Subviews[0];
            ThemeFrameView.AddSubview(container, NSWindowOrderingMode.Below, firstSubView);
            _titleBarContainer = container;
            TitleBarView = new TitleBarView(CGRect.Empty);
        }

        private void RecalculateFrameForTitleBarContainer()
        {
            if (_titleBarContainer == null)
                return;
            var themeFrameRect = ThemeFrameView.Frame;
            var titleFrame = new CGRect(0f, themeFrameRect.GetMaxY() - TitleBarHeight, themeFrameRect.Width, TitleBarHeight);
            _titleBarContainer.Frame = titleFrame;
        }

        private void HookInitialEvents()
        {
            DidResize += AppStoreWindow_DidResize;
            DidMove += AppStoreWindow_DidMoved;
            DidEndSheet += AppStoreWindow_DidEndSheet;
            DidExitFullScreen += AppStoreWindow_DidExitFullScreen;
            WillEnterFullScreen += AppStoreWindow_WillEnterFullScreen;
            WillExitFullScreen += AppStoreWindow_WillExitFullScreen;
        }

        private void UnhookInitialEvents()
        {
            DidResize -= AppStoreWindow_DidResize;
            DidMove -= AppStoreWindow_DidMoved;
            DidEndSheet -= AppStoreWindow_DidEndSheet;
            DidExitFullScreen -= AppStoreWindow_DidExitFullScreen;
            WillEnterFullScreen -= AppStoreWindow_WillEnterFullScreen;
            WillExitFullScreen -= AppStoreWindow_WillExitFullScreen;
        }

        protected void AppStoreWindow_WillExitFullScreen(object sender, EventArgs e)
        {
            if (!HideTitleBarInFullScreen)
                return;
            _titleBarHeight = _cachedTitleBarHeight;
            LayoutTrafficLightsAndContent();
            DisplayWindowAndTitleBar();
            TitleBarView.Hidden = false;
        }

        protected void AppStoreWindow_WillEnterFullScreen(object sender, EventArgs e)
        {
            if (!HideTitleBarInFullScreen)
                return;
            _titleBarHeight = 0f;
            LayoutTrafficLightsAndContent();
            DisplayWindowAndTitleBar();
            TitleBarView.Hidden = true;
        }

        protected void AppStoreWindow_DidExitFullScreen(object sender, EventArgs e)
        {
            LayoutTrafficLightsAndContent();
            SetupTrafficLightsTrackingArea();
        }

        protected void AppStoreWindow_DidEndSheet(object sender, EventArgs e)
        {
            LayoutTrafficLightsAndContent();
        }

        protected void AppStoreWindow_DidMoved(object sender, EventArgs e)
        {
            LayoutTrafficLightsAndContent();
        }

        protected void AppStoreWindow_DidResize(object sender, EventArgs e)
        {
            LayoutTrafficLightsAndContent();
        }

        private nfloat DefaultTrafficLightLeftMargin()
        {
            if (Math.Abs(_trafficLightLeftMargin - default(float)) < EPSILON)
            {
                var close = ButtonToLayout(NSWindowButton.CloseButton);
                _trafficLightLeftMargin = close.Frame.GetMinX();
            }
            return _trafficLightLeftMargin;
        }

        internal nfloat MinimumTitleBarHeight()
        {
            if (Math.Abs(_minimumTitleBarHeight - default(float)) < EPSILON)
            {
                var frameRect = Frame;
                var contentRect = ContentRectFor(frameRect);
                _minimumTitleBarHeight = frameRect.Height - contentRect.Height;
            }
            return _minimumTitleBarHeight;
        }

        protected void CloseButton_Activated(object sender, EventArgs e)
        {
            PerformClose(this);
        }

        private void MinimizeButton_Activated(object sender, EventArgs e)
        {
            PerformMiniaturize(this);
        }

        private void ZoomButton_Activated(object sender, EventArgs e)
        {
            PerformZoom(this);
        }

        private void FullScreenButton_Activated(object sender, EventArgs e)
        {
            ToggleFullScreen(this);
        }

        public override void SetFrame(CGRect frameRect, bool display)
        {
            if (!_preventWindowFrameChange)
            {
                base.SetFrame(frameRect, display);
            }
        }

        public override void SetFrame(CGRect frameRect, bool display, bool animate)
        {
            if (!_preventWindowFrameChange)
            {
                base.SetFrame(frameRect, display, animate);
            }
        }

        public override void BecomeKeyWindow()
        {
            base.BecomeKeyWindow();
            UpdateTitlebarView();
            LayoutTrafficLightsAndContent();
            SetupTrafficLightsTrackingArea();
        }

        public override void ResignKeyWindow()
        {
            base.ResignKeyWindow();
            UpdateTitlebarView();
            LayoutTrafficLightsAndContent();
        }

        public override void BecomeMainWindow()
        {
            base.BecomeMainWindow();
            UpdateTitlebarView();
        }

        public override void ResignMainWindow()
        {
            base.ResignMainWindow();
            UpdateTitlebarView();
        }

        private void LayoutTrafficLightsAndContent()
        {
            if (_titleBarContainer == null)
                return;
            // Reposition/resize the title bar view as needed
            RecalculateFrameForTitleBarContainer();
            var close = ButtonToLayout(NSWindowButton.CloseButton);
            var minimize = ButtonToLayout(NSWindowButton.MiniaturizeButton);
            var zoom = ButtonToLayout(NSWindowButton.ZoomButton);

            // Set the frame of the window buttons
            var closeFrame = close.Frame;
            var minimizeFrame = minimize.Frame;
            var zoomFrame = zoom.Frame;
            var titleBarFrame = _titleBarContainer.Frame;
            nfloat buttonOrigin;

            if (!VerticalTrafficLightButtons)
            {
                if (CenterTrafficLightButtons)
                {
                    buttonOrigin = (float)Math.Round(titleBarFrame.GetMidY() - TrafficLightButtonsTopMargin);
                }
                else
                {
                    buttonOrigin = titleBarFrame.GetMaxY() - closeFrame.Height - TrafficLightButtonsTopMargin;
                }

                closeFrame.Location = new CGPoint(TrafficLightButtonsLeftMargin, buttonOrigin);
                minimizeFrame.Location = new CGPoint(closeFrame.GetMaxX() + TrafficLightSeparation, buttonOrigin);
                zoomFrame.Location = new CGPoint(minimizeFrame.GetMaxX() + TrafficLightSeparation, buttonOrigin);
            }
            else
            {
                var groupHeight = closeFrame.Height + minimizeFrame.Height + zoomFrame.Height +
                                  2f * (TrafficLightSeparation - 2f);
                if (CenterTrafficLightButtons)
                {
                    buttonOrigin = (float)Math.Round(titleBarFrame.GetMidY() - groupHeight / 2f);
                }
                else
                {
                    buttonOrigin = titleBarFrame.GetMaxY() - groupHeight - TrafficLightButtonsTopMargin;
                }
                zoomFrame.Location = new CGPoint(TrafficLightButtonsLeftMargin, buttonOrigin);
                minimizeFrame.Location = new CGPoint(TrafficLightButtonsLeftMargin,
                    zoomFrame.GetMaxY() + TrafficLightSeparation - 2f);
                closeFrame.Location = new CGPoint(TrafficLightButtonsLeftMargin,
                    minimizeFrame.GetMaxY() + TrafficLightSeparation - 2f);
            }

            close.Frame = closeFrame;
            minimize.Frame = minimizeFrame;
            zoom.Frame = zoomFrame;

            var docIconButton = StandardWindowButton(NSWindowButton.DocumentIconButton);
            if (docIconButton != null)
            {
                var docButtonIconFrame = docIconButton.Frame;

                if (VerticallyCenterTitle)
                {
                    docButtonIconFrame.Y = (nfloat)Math.Floor(titleBarFrame.GetMidY() - docButtonIconFrame.GetMidHeight());
                }
                else
                {
                    docButtonIconFrame.Y = titleBarFrame.GetMaxY() - docButtonIconFrame.Height - WINDOW_DOCUMENT_ICON_BUTTON_ORIGIN_Y;
                }

                docIconButton.Frame = docButtonIconFrame;
            }

            var fullScreen = ButtonToLayout(NSWindowButton.FullScreenButton);
            if (fullScreen != null)
            {
                var fullScreenFrame = fullScreen.Frame;

                if (!_setFullScreenButtonRightMargin)
                {
                    FullScreenButtonRightMargin = _titleBarContainer.Frame.Width - fullScreen.Frame.GetMaxX();
                }

                fullScreenFrame.X = titleBarFrame.Width - fullScreenFrame.Width - FullScreenButtonRightMargin;
                if (CenterFullScreenButton)
                {
                    fullScreenFrame.Y = titleBarFrame.GetMidY() - fullScreenFrame.GetMidHeight();
                }
                else
                {
                    fullScreenFrame.Y = titleBarFrame.GetMaxY() - fullScreenFrame.Height - FullScreenButtonTopMargin;
                }

                fullScreen.Frame = fullScreenFrame;
            }

            var versionsButton = StandardWindowButton(NSWindowButton.DocumentVersionsButton);
            if (versionsButton != null)
            {
                var versionsButtonFrame = versionsButton.Frame;
                if (VerticallyCenterTitle)
                {
                    versionsButtonFrame.Y = (float)Math.Floor(titleBarFrame.GetMidY() - versionsButtonFrame.GetMidHeight());
                }
                else
                {
                    versionsButtonFrame.Y = titleBarFrame.GetMaxY() - versionsButtonFrame.Height - WINDOW_DOCUMENT_VERSIONS_BUTTON_ORIGIN_Y;
                }

                versionsButton.Frame = versionsButtonFrame;

                if (TitleFont != null)
                {
                    versionsButton.Font = TitleFont;
                }
            }

            foreach (var textField in ThemeFrameView.Subviews.OfType<NSTextField>())
            {
                var textFieldFrame = textField.Frame;
                if (VerticallyCenterTitle)
                {
                    textFieldFrame.Y = (float)Math.Round(titleBarFrame.GetMidY() - textFieldFrame.GetMidHeight());
                }
                else
                {
                    textFieldFrame.Y = titleBarFrame.GetMaxY() - textFieldFrame.Height - WINDOW_DOCUMENT_VERSIONS_DIVIDER_ORIGIN_Y;
                }

                textField.Frame = textFieldFrame;

                if (TitleFont != null)
                {
                    textField.Font = TitleFont;
                }
            }

            RepositionContentView();
        }

        private void UpdateTitlebarView()
        {
            DisplayWindowAndTitleBar();
            var isMainWindowAndActive = IsMainWindow && NSApplication.SharedApplication.Active;
            foreach (var control in TitleBarView.Subviews.OfType<NSControl>())
            {
                control.Enabled = isMainWindowAndActive;
            }
        }

        private nfloat DefaultTrafficLightSeparation()
        {
            if (Math.Abs(_trafficLightSeparation - default(float)) < EPSILON)
            {
                var close = ButtonToLayout(NSWindowButton.CloseButton);
                var minimize = ButtonToLayout(NSWindowButton.MiniaturizeButton);
                _trafficLightSeparation = minimize.Frame.GetMinX() - close.Frame.GetMaxX();
            }
            return _trafficLightSeparation;
        }

        internal NSButton ButtonToLayout(NSWindowButton buttonType)
        {
            NSButton button;
            switch (buttonType)
            {
                case NSWindowButton.CloseButton:
                    button = CloseButton;
                    break;

                case NSWindowButton.MiniaturizeButton:
                    button = MinimizeButton;
                    break;

                case NSWindowButton.ZoomButton:
                    button = ZoomButton;
                    break;

                case NSWindowButton.FullScreenButton:
                    button = FullScreenButton;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("buttonType");
            }

            return WindowButtonToLayout(buttonType, button);
        }

        private NSButton WindowButtonToLayout(NSWindowButton defaultButtonType, NSButton userButton)
        {
            var defaultButton = StandardWindowButton(defaultButtonType);
            if (defaultButton == null)
                return null;

            if (userButton != null)
            {
                defaultButton.Hidden = true;
                defaultButton = userButton;
            }
            else if (defaultButton.Superview != ThemeFrameView)
            {
                defaultButton.Hidden = false;
            }
            return defaultButton;
        }

        private void RepositionContentView()
        {
            var contentView = ContentView;
            var newFrame = ContentFrameView();
            if (!contentView.Frame.Equals(newFrame))
            {
                contentView.Frame = newFrame;
                contentView.NeedsDisplay = true;
            }
        }

        private CGRect ContentFrameView()
        {
            var windowFrame = Frame;
            var contentRect = ContentRectFor(windowFrame);
            contentRect.Height = windowFrame.Height - _titleBarHeight;
            contentRect.Location = CGPoint.Empty;
            return contentRect;
        }

        private void DisplayWindowAndTitleBar()
        {
            if (TitleBarView != null)
                TitleBarView.NeedsDisplay = true;
        }

        private void SetupTrafficLightsTrackingArea()
        {
            ThemeFrameView.ViewWillStartLiveResize();
            ThemeFrameView.ViewDidEndLiveResize();
        }

        /// <summary>
        /// Sets the height of the title bar.
        /// </summary>
        /// <param name="newHeight">New height.</param>
        /// <param name="adjustWindowFrame">If set to <c>true</c> adjust window frame.</param>
        public void SetTitleBarHeight(nfloat newHeight, bool adjustWindowFrame)
        {
            if (Math.Abs(_titleBarHeight - newHeight) < EPSILON)
                return;
            var windowFrame = Frame;
            if (adjustWindowFrame)
            {
                windowFrame.Location = new CGPoint(windowFrame.X, windowFrame.Y - newHeight - _titleBarHeight);
                windowFrame.Size = new CGSize(windowFrame.Width, windowFrame.Height + newHeight - _titleBarHeight);
            }

            _titleBarHeight = _cachedTitleBarHeight = newHeight;

            SetFrame(windowFrame, true);
            LayoutTrafficLightsAndContent();
            DisplayWindowAndTitleBar();
            if ((StyleMask & NSWindowStyle.TexturedBackground) == NSWindowStyle.TexturedBackground)
            {
                ContentView.DisplayIfNeeded();
            }
            if (MinSize.Height < _titleBarHeight)
                MinSize = new CGSize(MinSize.Width, _titleBarHeight);
        }

        public static NSColor DefaultTitleBarStartColor(bool drawsAsMainWindow)
        {
            return drawsAsMainWindow ? NSColor.FromDeviceWhite(0.66f, 1.0f) : (NSColor.FromDeviceWhite(0.878f, 1.0f));
        }

        public static NSColor DefaultTitleBarEndColor(bool drawsAsMainWindow)
        {
            return drawsAsMainWindow ? NSColor.FromDeviceWhite(0.9f, 1.0f) : (NSColor.FromDeviceWhite(0.976f, 1.0f));
        }

        public static NSColor DefaultBaselineSeparatorColor(bool drawsAsMainWindow)
        {
            return drawsAsMainWindow ? NSColor.FromDeviceWhite(0.408f, 1.0f) : (NSColor.FromDeviceWhite(0.655f, 1.0f));
        }

        public static NSColor DefaultTitleTextColor(bool drawsAsMainWindow)
        {
            return drawsAsMainWindow
                ? NSColor.FromDeviceWhite(56.0f / 255.0f, 1.0f)
                : (NSColor.FromDeviceWhite(56.0f / 255.0f, 0.5f));
        }

        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            UnhookInitialEvents();
            base.Dispose(disposing);
        }

        #endregion Methods
    }
}