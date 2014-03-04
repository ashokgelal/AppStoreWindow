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
using System.Collections.Generic;
using MonoMac.Foundation;

namespace AshokGelal.AppStoreWindow
{
    internal class WindowButtonGroup : NSObject
    {
        #region Fields

        private const string WindowButtonGroupDefaultIdentifier = "AshokGelal.AppStoreWindow.ButtonGroupDefaultIdentifier";
        private static IDictionary<string, WindowButtonGroup> _groups;
        private int _numberOfCaptures;

        public event EventHandler RolloverStateUpdatedEvent;

        #endregion

        internal int NumberOfCaptures
        {
            get
            {
                return _numberOfCaptures;
            }
            set
            {
                if (_numberOfCaptures != value && value >= 0)
                {
                    _numberOfCaptures = value;
                    OnRolloverStateUpdatedEvent(EventArgs.Empty);
                }
            }
        }

        internal bool ShouldDisplayRollOver
        {
            get { return _numberOfCaptures > 0; }
        }

        internal static WindowButtonGroup GroupInstance(string identifier = null)
        {
            if (_groups == null)
            {
                _groups = new Dictionary<string, WindowButtonGroup>();
            }
            if (string.IsNullOrWhiteSpace(identifier))
            {
                identifier = WindowButtonGroupDefaultIdentifier;
            }

            WindowButtonGroup group;
            if (!_groups.TryGetValue(identifier, out group))
            {
                group = new WindowButtonGroup();
                _groups.Add(identifier, group);
            }
            return group;
        }

        private WindowButtonGroup()
        {

        }

        protected virtual void OnRolloverStateUpdatedEvent(EventArgs e)
        {
            var handler = RolloverStateUpdatedEvent;
            if (handler != null)
                handler(this, e);
        }

        internal void DidCaptureMousePointer()
        {
            _numberOfCaptures++;
        }

        internal void DidReleaseMousePointer()
        {
            _numberOfCaptures--;
        }

        internal void ResetMouseCaptures()
        {
            _numberOfCaptures = 0;
        }
    }
}

