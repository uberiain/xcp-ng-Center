﻿/* Copyright (c) XCP-ng Project. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Security.Permissions;
using System.Windows.Forms;

namespace XenAdmin.Controls.TabControl
{
    [ToolboxBitmap(typeof(System.Windows.Forms.TabControl))]
    public class CustomTabControl : System.Windows.Forms.TabControl
    {
        #region String formatting

        private StringFormat GetStringFormat()
        {
            StringFormat format = null;

            //	Rotate Text by 90 degrees for left and right tabs
            switch (Alignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    format = new StringFormat();
                    break;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    format = new StringFormat(StringFormatFlags.DirectionVertical);
                    break;
            }

            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            if (FindForm() != null && FindForm().KeyPreview)
                format.HotkeyPrefix = HotkeyPrefix.Show;
            else
                format.HotkeyPrefix = HotkeyPrefix.Hide;
            if (RightToLeft == RightToLeft.Yes)
                format.FormatFlags = format.FormatFlags | StringFormatFlags.DirectionRightToLeft;
            return format;
        }

        #endregion

        #region Construction

        public CustomTabControl()
        {
            SetStyle(
                ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque |
                ControlStyles.ResizeRedraw, true);

            _BackBuffer = new Bitmap(Width, Height);
            _BackBufferGraphics = Graphics.FromImage(_BackBuffer);
            _TabBuffer = new Bitmap(Width, Height);
            _TabBufferGraphics = Graphics.FromImage(_TabBuffer);

            DisplayStyle = TabStyle.Default;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            OnFontChanged(EventArgs.Empty);
        }


        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                var cp = base.CreateParams;
                if (RightToLeftLayout)
                    cp.ExStyle = cp.ExStyle | NativeMethods.WS_EX_LAYOUTRTL | NativeMethods.WS_EX_NOINHERITLAYOUT;
                return cp;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_BackImage != null) _BackImage.Dispose();
                if (_BackBufferGraphics != null) _BackBufferGraphics.Dispose();
                if (_BackBuffer != null) _BackBuffer.Dispose();
                if (_TabBufferGraphics != null) _TabBufferGraphics.Dispose();
                if (_TabBuffer != null) _TabBuffer.Dispose();

                if (_StyleProvider != null) _StyleProvider.Dispose();
            }
        }

        #endregion

        #region Private variables

        private Bitmap _BackImage;
        private Bitmap _BackBuffer;
        private Graphics _BackBufferGraphics;
        private Bitmap _TabBuffer;
        private Graphics _TabBufferGraphics;

        private int _oldValue;
        private Point _dragStartPosition = Point.Empty;

        private TabStyle _Style;
        private TabStyleProvider _StyleProvider;

        private List<TabPage> _TabPages;

        #endregion

        #region Public properties

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TabStyleProvider DisplayStyleProvider
        {
            get
            {
                if (_StyleProvider == null) DisplayStyle = TabStyle.Default;

                return _StyleProvider;
            }
            set => _StyleProvider = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(TabStyle), "Default")]
        [RefreshProperties(RefreshProperties.All)]
        public TabStyle DisplayStyle
        {
            get => _Style;
            set
            {
                if (_Style != value)
                {
                    _Style = value;
                    _StyleProvider = TabStyleProvider.CreateProvider(this);
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [RefreshProperties(RefreshProperties.All)]
        public new bool Multiline
        {
            get => base.Multiline;
            set
            {
                base.Multiline = value;
                Invalidate();
            }
        }


        //	Hide the Padding attribute so it can not be changed
        //	We are handling this on the Style Provider
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Point Padding
        {
            get => DisplayStyleProvider.Padding;
            set => DisplayStyleProvider.Padding = value;
        }

        public override bool RightToLeftLayout
        {
            get => base.RightToLeftLayout;
            set
            {
                base.RightToLeftLayout = value;
                UpdateStyles();
            }
        }


        //	Hide the HotTrack attribute so it can not be changed
        //	We are handling this on the Style Provider
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool HotTrack
        {
            get => DisplayStyleProvider.HotTrack;
            set => DisplayStyleProvider.HotTrack = value;
        }

        [Category("Appearance")]
        public new TabAlignment Alignment
        {
            get => base.Alignment;
            set
            {
                base.Alignment = value;
                switch (value)
                {
                    case TabAlignment.Top:
                    case TabAlignment.Bottom:
                        Multiline = false;
                        break;
                    case TabAlignment.Left:
                    case TabAlignment.Right:
                        Multiline = true;
                        break;
                }
            }
        }

        //	Hide the Appearance attribute so it can not be changed
        //	We don't want it as we are doing all the painting.
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public new TabAppearance Appearance
        {
            get => base.Appearance;
            set =>
                //	Don't permit setting to other appearances as we are doing all the painting
                base.Appearance = TabAppearance.Normal;
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                var tabStripHeight = 0;
                var itemHeight = 0;

                if (Alignment <= TabAlignment.Bottom)
                    itemHeight = ItemSize.Height;
                else
                    itemHeight = ItemSize.Width;

                tabStripHeight = 5 + itemHeight * RowCount;

                var rect = new Rectangle(4, tabStripHeight, Width - 8, Height - tabStripHeight - 4);
                switch (Alignment)
                {
                    case TabAlignment.Top:
                        rect = new Rectangle(4, tabStripHeight, Width - 8, Height - tabStripHeight - 4);
                        break;
                    case TabAlignment.Bottom:
                        rect = new Rectangle(4, 4, Width - 8, Height - tabStripHeight - 4);
                        break;
                    case TabAlignment.Left:
                        rect = new Rectangle(tabStripHeight, 4, Width - tabStripHeight - 4, Height - 8);
                        break;
                    case TabAlignment.Right:
                        rect = new Rectangle(4, 4, Width - tabStripHeight - 4, Height - 8);
                        break;
                }

                return rect;
            }
        }

        [Browsable(false)]
        public int ActiveIndex
        {
            get
            {
                var hitTestInfo = new NativeMethods.TCHITTESTINFO(PointToClient(Control.MousePosition));
                var index = NativeMethods.SendMessage(Handle, NativeMethods.TCM_HITTEST, IntPtr.Zero,
                    NativeMethods.ToIntPtr(hitTestInfo)).ToInt32();
                if (index == -1) return -1;

                if (TabPages[index].Enabled)
                    return index;
                return -1;
            }
        }

        [Browsable(false)]
        public TabPage ActiveTab
        {
            get
            {
                var activeIndex = ActiveIndex;
                if (activeIndex > -1)
                    return TabPages[activeIndex];
                return null;
            }
        }

        #endregion

        #region Extension methods

        public void HideTab(TabPage page)
        {
            if (page != null && TabPages.Contains(page))
            {
                BackupTabPages();
                TabPages.Remove(page);
            }
        }

        public void HideTab(int index)
        {
            if (IsValidTabIndex(index)) HideTab(_TabPages[index]);
        }

        public void HideTab(string key)
        {
            if (TabPages.ContainsKey(key)) HideTab(TabPages[key]);
        }

        public void ShowTab(TabPage page)
        {
            if (page != null)
            {
                if (_TabPages != null)
                {
                    if (!TabPages.Contains(page)
                        && _TabPages.Contains(page))
                    {
                        //	Get insert point from backup of pages
                        var pageIndex = _TabPages.IndexOf(page);
                        if (pageIndex > 0)
                        {
                            var start = pageIndex - 1;

                            //	Check for presence of earlier pages in the visible tabs
                            for (var index = start; index >= 0; index--)
                                if (TabPages.Contains(_TabPages[index]))
                                {
                                    //	Set insert point to the right of the last present tab
                                    pageIndex = TabPages.IndexOf(_TabPages[index]) + 1;
                                    break;
                                }
                        }

                        //	Insert the page, or add to the end
                        if (pageIndex >= 0 && pageIndex < TabPages.Count)
                            TabPages.Insert(pageIndex, page);
                        else
                            TabPages.Add(page);
                    }
                }
                else
                {
                    //	If the page is not found at all then just add it
                    if (!TabPages.Contains(page)) TabPages.Add(page);
                }
            }
        }

        public void ShowTab(int index)
        {
            if (IsValidTabIndex(index)) ShowTab(_TabPages[index]);
        }

        public void ShowTab(string key)
        {
            if (_TabPages != null)
            {
                var tab = _TabPages.Find(delegate(TabPage page)
                {
                    return page.Name.Equals(key, StringComparison.OrdinalIgnoreCase);
                });
                ShowTab(tab);
            }
        }

        private bool IsValidTabIndex(int index)
        {
            BackupTabPages();
            return index >= 0 && index < _TabPages.Count;
        }

        private void BackupTabPages()
        {
            if (_TabPages == null)
            {
                _TabPages = new List<TabPage>();
                foreach (TabPage page in TabPages) _TabPages.Add(page);
            }
        }

        #endregion

        #region Drag 'n' Drop

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (AllowDrop) _dragStartPosition = new Point(e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (AllowDrop) _dragStartPosition = Point.Empty;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            if (drgevent.Data.GetDataPresent(typeof(TabPage)))
                drgevent.Effect = DragDropEffects.Move;
            else
                drgevent.Effect = DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            if (drgevent.Data.GetDataPresent(typeof(TabPage)))
            {
                drgevent.Effect = DragDropEffects.Move;

                var dragTab = (TabPage)drgevent.Data.GetData(typeof(TabPage));

                if (ActiveTab == dragTab) return;

                //	Capture insert point and adjust for removal of tab
                //	We cannot assess this after removal as differeing tab sizes will cause
                //	inaccuracies in the activeTab at insert point.
                var insertPoint = ActiveIndex;
                if (dragTab.Parent.Equals(this) && TabPages.IndexOf(dragTab) < insertPoint) insertPoint--;
                if (insertPoint < 0) insertPoint = 0;

                //	Remove from current position (could be another tabcontrol)
                ((System.Windows.Forms.TabControl)dragTab.Parent).TabPages.Remove(dragTab);

                //	Add to current position
                TabPages.Insert(insertPoint, dragTab);
                SelectedTab = dragTab;

                //	deal with hidden tab handling?
            }
        }

        private void StartDragDrop()
        {
            if (!_dragStartPosition.IsEmpty)
            {
                var dragTab = SelectedTab;
                if (dragTab != null)
                {
                    //	Test for movement greater than the drag activation trigger area
                    var dragTestRect = new Rectangle(_dragStartPosition, Size.Empty);
                    dragTestRect.Inflate(SystemInformation.DragSize);
                    var pt = PointToClient(Control.MousePosition);
                    if (!dragTestRect.Contains(pt))
                    {
                        DoDragDrop(dragTab, DragDropEffects.All);
                        _dragStartPosition = Point.Empty;
                    }
                }
            }
        }

        #endregion

        #region Events

        [Category("Action")] public event ScrollEventHandler HScroll;
        [Category("Action")] public event EventHandler<TabControlEventArgs> TabImageClick;
        [Category("Action")] public event EventHandler<TabControlCancelEventArgs> TabClosing;

        #endregion

        #region Base class event processing

        protected override void OnFontChanged(EventArgs e)
        {
            var hFont = Font.ToHfont();
            NativeMethods.SendMessage(Handle, NativeMethods.WM_SETFONT, hFont, (IntPtr)(-1));
            NativeMethods.SendMessage(Handle, NativeMethods.WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
            UpdateStyles();
            if (Visible) Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            //	Recreate the buffer for manual double buffering
            if (Width > 0 && Height > 0)
            {
                if (_BackImage != null)
                {
                    _BackImage.Dispose();
                    _BackImage = null;
                }

                if (_BackBufferGraphics != null) _BackBufferGraphics.Dispose();
                if (_BackBuffer != null) _BackBuffer.Dispose();

                _BackBuffer = new Bitmap(Width, Height);
                _BackBufferGraphics = Graphics.FromImage(_BackBuffer);

                if (_TabBufferGraphics != null) _TabBufferGraphics.Dispose();
                if (_TabBuffer != null) _TabBuffer.Dispose();

                _TabBuffer = new Bitmap(Width, Height);
                _TabBufferGraphics = Graphics.FromImage(_TabBuffer);

                if (_BackImage != null)
                {
                    _BackImage.Dispose();
                    _BackImage = null;
                }
            }

            base.OnResize(e);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            if (_BackImage != null)
            {
                _BackImage.Dispose();
                _BackImage = null;
            }

            base.OnParentBackColorChanged(e);
        }

        protected override void OnParentBackgroundImageChanged(EventArgs e)
        {
            if (_BackImage != null)
            {
                _BackImage.Dispose();
                _BackImage = null;
            }

            base.OnParentBackgroundImageChanged(e);
        }

        private void OnParentResize(object sender, EventArgs e)
        {
            if (Visible) Invalidate();
        }


        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null) Parent.Resize += OnParentResize;
        }

        protected override void OnSelecting(TabControlCancelEventArgs e)
        {
            base.OnSelecting(e);

            //	Do not allow selecting of disabled tabs
            if (e.Action == TabControlAction.Selecting && e.TabPage != null && !e.TabPage.Enabled) e.Cancel = true;
        }

        protected override void OnMove(EventArgs e)
        {
            if (Width > 0 && Height > 0)
                if (_BackImage != null)
                {
                    _BackImage.Dispose();
                    _BackImage = null;
                }

            base.OnMove(e);
            Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (Visible) Invalidate();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            if (Visible) Invalidate();
        }


        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessMnemonic(char charCode)
        {
            foreach (TabPage page in TabPages)
                if (IsMnemonic(charCode, page.Text))
                {
                    SelectedTab = page;
                    return true;
                }

            return base.ProcessMnemonic(charCode);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_HSCROLL:

                    //	Raise the scroll event when the scroller is scrolled
                    base.WndProc(ref m);
                    OnHScroll(new ScrollEventArgs((ScrollEventType)NativeMethods.LoWord(m.WParam), _oldValue,
                        NativeMethods.HiWord(m.WParam), ScrollOrientation.HorizontalScroll));
                    break;
//				case NativeMethods.WM_PAINT:
//					
//					//	Handle painting ourselves rather than call the base OnPaint.
//					CustomPaint(ref m);
//					break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            var index = ActiveIndex;

            //	If we are clicking on an image then raise the ImageClicked event before raising the standard mouse click event
            //	if there if a handler.
            if (index > -1 && TabImageClick != null
                           && (TabPages[index].ImageIndex > -1 || !string.IsNullOrEmpty(TabPages[index].ImageKey))
                           && GetTabImageRect(index).Contains(MousePosition))
            {
                OnTabImageClick(new TabControlEventArgs(TabPages[index], index, TabControlAction.Selected));

                //	Fire the base event
                base.OnMouseClick(e);
            }
            else
            {
                //	Fire the base event
                base.OnMouseClick(e);
            }
        }

        protected virtual void OnTabImageClick(TabControlEventArgs e)
        {
            if (TabImageClick != null) TabImageClick(this, e);
        }

        protected virtual void OnTabClosing(TabControlCancelEventArgs e)
        {
            if (TabClosing != null) TabClosing(this, e);
        }

        protected virtual void OnHScroll(ScrollEventArgs e)
        {
            //	repaint the moved tabs
            Invalidate();

            //	Raise the event
            if (HScroll != null) HScroll(this, e);

            if (e.Type == ScrollEventType.EndScroll) _oldValue = e.NewValue;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //	Initialise Drag Drop
            if (AllowDrop && e.Button == MouseButtons.Left) StartDragDrop();
        }

        #endregion

        #region Basic drawing methods

//		private void CustomPaint(ref Message m){
//			NativeMethods.PAINTSTRUCT paintStruct = new NativeMethods.PAINTSTRUCT();
//			NativeMethods.BeginPaint(m.HWnd, ref paintStruct);
//			using (Graphics screenGraphics = this.CreateGraphics()) {
//				this.CustomPaint(screenGraphics);
//			}
//			NativeMethods.EndPaint(m.HWnd, ref paintStruct);
//		}

        protected override void OnPaint(PaintEventArgs e)
        {
            if (layoutSuspended) return;

            //	We must always paint the entire area of the tab control
            if (e.ClipRectangle.Equals(ClientRectangle))
                CustomPaint(e.Graphics);
            else
                //	it is less intensive to just reinvoke the paint with the whole surface available to draw on.
                Invalidate();
        }

        private bool layoutSuspended;

        public new void SuspendLayout()
        {
            layoutSuspended = true;
            base.SuspendLayout();
        }

        public new void ResumeLayout()
        {
            layoutSuspended = false;
            base.ResumeLayout();
        }

        private void CustomPaint(Graphics screenGraphics)
        {
            //	We render into a bitmap that is then drawn in one shot rather than using
            //	double buffering built into the control as the built in buffering
            // 	messes up the background painting.
            //	Equally the .Net 2.0 BufferedGraphics object causes the background painting
            //	to mess up, which is why we use this .Net 1.1 buffering technique.

            //	Buffer code from Gil. Schmidt http://www.codeproject.com/KB/graphics/DoubleBuffering.aspx

            if (Width > 0 && Height > 0)
            {
                if (_BackImage == null)
                {
                    //	Cached Background Image
                    _BackImage = new Bitmap(Width, Height);
                    var backGraphics = Graphics.FromImage(_BackImage);
                    backGraphics.Clear(Color.Transparent);
                    PaintTransparentBackground(backGraphics, ClientRectangle);
                }

                _BackBufferGraphics.Clear(Color.Transparent);
                _BackBufferGraphics.DrawImageUnscaled(_BackImage, 0, 0);

                _TabBufferGraphics.Clear(Color.Transparent);

                if (TabCount > 0)
                {
                    //	When top or bottom and scrollable we need to clip the sides from painting the tabs.
                    //	Left and right are always multiline.
                    if (Alignment <= TabAlignment.Bottom && !Multiline)
                        _TabBufferGraphics.Clip = new Region(new RectangleF(ClientRectangle.X + 3, ClientRectangle.Y,
                            ClientRectangle.Width - 6, ClientRectangle.Height));

                    //	Draw each tabpage from right to left.  We do it this way to handle
                    //	the overlap correctly.
                    if (Multiline)
                    {
                        for (var row = 0; row < RowCount; row++)
                        for (var index = TabCount - 1; index >= 0; index--)
                            if (index != SelectedIndex && (RowCount == 1 || GetTabRow(index) == row))
                                DrawTabPage(index, _TabBufferGraphics);
                    }
                    else
                    {
                        for (var index = TabCount - 1; index >= 0; index--)
                            if (index != SelectedIndex)
                                DrawTabPage(index, _TabBufferGraphics);
                    }

                    //	The selected tab must be drawn last so it appears on top.
                    if (SelectedIndex > -1) DrawTabPage(SelectedIndex, _TabBufferGraphics);
                }

                _TabBufferGraphics.Flush();

                //	Paint the tabs on top of the background

                // Create a new color matrix and set the alpha value to 0.5
                var alphaMatrix = new ColorMatrix();
                alphaMatrix.Matrix00 = alphaMatrix.Matrix11 = alphaMatrix.Matrix22 = alphaMatrix.Matrix44 = 1;
                alphaMatrix.Matrix33 = _StyleProvider.Opacity;

                // Create a new image attribute object and set the color matrix to
                // the one just created
                using (var alphaAttributes = new ImageAttributes())
                {
                    alphaAttributes.SetColorMatrix(alphaMatrix);

                    // Draw the original image with the image attributes specified
                    _BackBufferGraphics.DrawImage(_TabBuffer,
                        new Rectangle(0, 0, _TabBuffer.Width, _TabBuffer.Height),
                        0, 0, _TabBuffer.Width, _TabBuffer.Height, GraphicsUnit.Pixel,
                        alphaAttributes);
                }

                _BackBufferGraphics.Flush();

                //	Now paint this to the screen


                //	We want to paint the whole tabstrip and border every time
                //	so that the hot areas update correctly, along with any overlaps

                //	paint the tabs etc.
                if (RightToLeftLayout)
                    screenGraphics.DrawImageUnscaled(_BackBuffer, -1, 0);
                else
                    screenGraphics.DrawImageUnscaled(_BackBuffer, 0, 0);
            }
        }

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect)
        {
            if (Parent != null)
            {
                //	Set the cliprect to be relative to the parent
                clipRect.Offset(Location);

                //	Save the current state before we do anything.
                var state = graphics.Save();

                //	Set the graphicsobject to be relative to the parent
                graphics.TranslateTransform(-Location.X, -Location.Y);
                graphics.SmoothingMode = SmoothingMode.HighSpeed;

                //	Paint the parent
                var e = new PaintEventArgs(graphics, clipRect);
                try
                {
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                }
                finally
                {
                    //	Restore the graphics state and the clipRect to their original locations
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            }
        }

        private void DrawTabPage(int index, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.HighSpeed;

            //	Get TabPageBorder
            using (var tabPageBorderPath = GetTabPageBorder(index))
            {
                //	Paint the background
                using (var fillBrush = _StyleProvider.GetPageBackgroundBrush(index))
                {
                    graphics.FillPath(fillBrush, tabPageBorderPath);
                }

                //	Paint the tab
                _StyleProvider.PaintTab(index, graphics);

                //	Draw any image
                DrawTabImage(index, graphics);

                //	Draw the text
                DrawTabText(index, graphics);

                //	Paint the border
                DrawTabBorder(tabPageBorderPath, index, graphics);
            }
        }

        private void DrawTabBorder(GraphicsPath path, int index, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            Color borderColor;
            if (index == SelectedIndex)
                borderColor = _StyleProvider.BorderColorSelected;
            else if (_StyleProvider.HotTrack && index == ActiveIndex)
                borderColor = _StyleProvider.BorderColorHot;
            else
                borderColor = _StyleProvider.BorderColor;

            using (var borderPen = new Pen(borderColor))
            {
                graphics.DrawPath(borderPen, path);
            }
        }

        private void DrawTabText(int index, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var tabBounds = GetTabTextRect(index);

            if (SelectedIndex == index)
            {
                using (Brush textBrush = new SolidBrush(_StyleProvider.TextColorSelected))
                {
                    graphics.DrawString(TabPages[index].Text, Font, textBrush, tabBounds, GetStringFormat());
                }
            }
            else
            {
                if (TabPages[index].Enabled)
                    using (Brush textBrush = new SolidBrush(_StyleProvider.TextColor))
                    {
                        graphics.DrawString(TabPages[index].Text, Font, textBrush, tabBounds, GetStringFormat());
                    }
                else
                    using (Brush textBrush = new SolidBrush(_StyleProvider.TextColorDisabled))
                    {
                        graphics.DrawString(TabPages[index].Text, Font, textBrush, tabBounds, GetStringFormat());
                    }
            }
        }

        private void DrawTabImage(int index, Graphics graphics)
        {
            Image tabImage = null;
            if (TabPages[index].ImageIndex > -1 && ImageList != null &&
                ImageList.Images.Count > TabPages[index].ImageIndex)
                tabImage = ImageList.Images[TabPages[index].ImageIndex];
            else if (!string.IsNullOrEmpty(TabPages[index].ImageKey) && !TabPages[index].ImageKey
                                                                         .Equals("(none)",
                                                                             StringComparison.OrdinalIgnoreCase)
                                                                     && ImageList != null &&
                                                                     ImageList.Images.ContainsKey(TabPages[index]
                                                                         .ImageKey))
                tabImage = ImageList.Images[TabPages[index].ImageKey];

            if (tabImage != null)
            {
                if (RightToLeftLayout) tabImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                var imageRect = GetTabImageRect(index);
                if (TabPages[index].Enabled)
                    graphics.DrawImage(tabImage, imageRect);
                else
                    ControlPaint.DrawImageDisabled(graphics, tabImage, imageRect.X, imageRect.Y, Color.Transparent);
            }
        }

        #endregion

        #region Tab borders and bounds properties

        private GraphicsPath GetTabPageBorder(int index)
        {
            var path = new GraphicsPath();
            var pageBounds = GetPageBounds(index);
            var tabBounds = _StyleProvider.GetTabRect(index);
            _StyleProvider.AddTabBorder(path, tabBounds);
            AddPageBorder(path, pageBounds, tabBounds);

            path.CloseFigure();
            return path;
        }

        public Rectangle GetPageBounds(int index)
        {
            var pageBounds = TabPages[index].Bounds;
            pageBounds.Width += 1;
            pageBounds.Height += 1;
            pageBounds.X -= 1;
            pageBounds.Y -= 1;

            if (pageBounds.Bottom > Height - 4) pageBounds.Height -= pageBounds.Bottom - Height + 4;
            return pageBounds;
        }

        private Rectangle GetTabTextRect(int index)
        {
            var textRect = new Rectangle();
            using (var path = _StyleProvider.GetTabBorder(index))
            {
                var tabBounds = path.GetBounds();

                textRect = new Rectangle((int)tabBounds.X, (int)tabBounds.Y, (int)tabBounds.Width,
                    (int)tabBounds.Height);

                //	Make it shorter or thinner to fit the height or width because of the padding added to the tab for painting
                switch (Alignment)
                {
                    case TabAlignment.Top:
                        textRect.Y += 4;
                        textRect.Height -= 6;
                        break;
                    case TabAlignment.Bottom:
                        textRect.Y += 2;
                        textRect.Height -= 6;
                        break;
                    case TabAlignment.Left:
                        textRect.X += 4;
                        textRect.Width -= 6;
                        break;
                    case TabAlignment.Right:
                        textRect.X += 2;
                        textRect.Width -= 6;
                        break;
                }

                //	If there is an image allow for it
                if (ImageList != null && (TabPages[index].ImageIndex > -1
                                          || (!string.IsNullOrEmpty(TabPages[index].ImageKey)
                                              && !TabPages[index].ImageKey.Equals("(none)",
                                                  StringComparison.OrdinalIgnoreCase))))
                {
                    var imageRect = GetTabImageRect(index);
                    if ((_StyleProvider.ImageAlign & NativeMethods.AnyLeftAlign) != 0)
                    {
                        if (Alignment <= TabAlignment.Bottom)
                        {
                            textRect.X = imageRect.Right + 4;
                            textRect.Width -= textRect.Right - (int)tabBounds.Right;
                        }
                        else
                        {
                            textRect.Y = imageRect.Y + 4;
                            textRect.Height -= textRect.Bottom - (int)tabBounds.Bottom;
                        }
                    }
                    else
                    {
                        if (Alignment <= TabAlignment.Bottom)
                            textRect.Width -= (int)tabBounds.Right - imageRect.X + 4;
                        else
                            textRect.Height -= (int)tabBounds.Bottom - imageRect.Y + 4;
                    }
                }

                //	Ensure it fits inside the path at the centre line
                if (Alignment <= TabAlignment.Bottom)
                {
                    while (!path.IsVisible(textRect.Right, textRect.Y) && textRect.Width > 0) textRect.Width -= 1;
                    while (!path.IsVisible(textRect.X, textRect.Y) && textRect.Width > 0)
                    {
                        textRect.X += 1;
                        textRect.Width -= 1;
                    }
                }
                else
                {
                    while (!path.IsVisible(textRect.X, textRect.Bottom) && textRect.Height > 0) textRect.Height -= 1;
                    while (!path.IsVisible(textRect.X, textRect.Y) && textRect.Height > 0)
                    {
                        textRect.Y += 1;
                        textRect.Height -= 1;
                    }
                }
            }

            return textRect;
        }

        public int GetTabRow(int index)
        {
            //	All calculations will use this rect as the base point
            //	because the itemsize does not return the correct width.
            var rect = GetTabRect(index);

            var row = -1;

            switch (Alignment)
            {
                case TabAlignment.Top:
                    row = (rect.Y - 2) / rect.Height;
                    break;
                case TabAlignment.Bottom:
                    row = (Height - rect.Y - 2) / rect.Height - 1;
                    break;
                case TabAlignment.Left:
                    row = (rect.X - 2) / rect.Width;
                    break;
                case TabAlignment.Right:
                    row = (Width - rect.X - 2) / rect.Width - 1;
                    break;
            }

            return row;
        }

        public Point GetTabPosition(int index)
        {
            //	If we are not multiline then the column is the index and the row is 0.
            if (!Multiline) return new Point(0, index);

            //	If there is only one row then the column is the index
            if (RowCount == 1) return new Point(0, index);

            //	We are in a true multi-row scenario
            var row = GetTabRow(index);
            var rect = GetTabRect(index);
            var column = -1;

            //	Scan from left to right along rows, skipping to next row if it is not the one we want.
            for (var testIndex = 0; testIndex < TabCount; testIndex++)
            {
                var testRect = GetTabRect(testIndex);
                if (Alignment <= TabAlignment.Bottom)
                {
                    if (testRect.Y == rect.Y) column += 1;
                }
                else
                {
                    if (testRect.X == rect.X) column += 1;
                }

                if (testRect.Location.Equals(rect.Location)) return new Point(row, column);
            }

            return new Point(0, 0);
        }

        public bool IsFirstTabInRow(int index)
        {
            if (index < 0) return false;
            var firstTabinRow = index == 0;
            if (!firstTabinRow)
            {
                if (Alignment <= TabAlignment.Bottom)
                {
                    if (GetTabRect(index).X == 2) firstTabinRow = true;
                }
                else
                {
                    if (GetTabRect(index).Y == 2) firstTabinRow = true;
                }
            }

            return firstTabinRow;
        }

        private void AddPageBorder(GraphicsPath path, Rectangle pageBounds, Rectangle tabBounds)
        {
            switch (Alignment)
            {
                case TabAlignment.Top:
                    path.AddLine(tabBounds.Right, pageBounds.Y, pageBounds.Right, pageBounds.Y);
                    path.AddLine(pageBounds.Right, pageBounds.Y, pageBounds.Right, pageBounds.Bottom);
                    path.AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.X, pageBounds.Bottom);
                    path.AddLine(pageBounds.X, pageBounds.Bottom, pageBounds.X, pageBounds.Y);
                    path.AddLine(pageBounds.X, pageBounds.Y, tabBounds.X, pageBounds.Y);
                    break;
                case TabAlignment.Bottom:
                    path.AddLine(tabBounds.X, pageBounds.Bottom, pageBounds.X, pageBounds.Bottom);
                    path.AddLine(pageBounds.X, pageBounds.Bottom, pageBounds.X, pageBounds.Y);
                    path.AddLine(pageBounds.X, pageBounds.Y, pageBounds.Right, pageBounds.Y);
                    path.AddLine(pageBounds.Right, pageBounds.Y, pageBounds.Right, pageBounds.Bottom);
                    path.AddLine(pageBounds.Right, pageBounds.Bottom, tabBounds.Right, pageBounds.Bottom);
                    break;
                case TabAlignment.Left:
                    path.AddLine(pageBounds.X, tabBounds.Y, pageBounds.X, pageBounds.Y);
                    path.AddLine(pageBounds.X, pageBounds.Y, pageBounds.Right, pageBounds.Y);
                    path.AddLine(pageBounds.Right, pageBounds.Y, pageBounds.Right, pageBounds.Bottom);
                    path.AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.X, pageBounds.Bottom);
                    path.AddLine(pageBounds.X, pageBounds.Bottom, pageBounds.X, tabBounds.Bottom);
                    break;
                case TabAlignment.Right:
                    path.AddLine(pageBounds.Right, tabBounds.Bottom, pageBounds.Right, pageBounds.Bottom);
                    path.AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.X, pageBounds.Bottom);
                    path.AddLine(pageBounds.X, pageBounds.Bottom, pageBounds.X, pageBounds.Y);
                    path.AddLine(pageBounds.X, pageBounds.Y, pageBounds.Right, pageBounds.Y);
                    path.AddLine(pageBounds.Right, pageBounds.Y, pageBounds.Right, tabBounds.Y);
                    break;
            }
        }

        private Rectangle GetTabImageRect(int index)
        {
            using (var tabBorderPath = _StyleProvider.GetTabBorder(index))
            {
                return GetTabImageRect(tabBorderPath);
            }
        }

        private Rectangle GetTabImageRect(GraphicsPath tabBorderPath)
        {
            var imageRect = new Rectangle();
            var rect = tabBorderPath.GetBounds();

            //	Make it shorter or thinner to fit the height or width because of the padding added to the tab for painting
            switch (Alignment)
            {
                case TabAlignment.Top:
                    rect.Y += 4;
                    rect.Height -= 6;
                    break;
                case TabAlignment.Bottom:
                    rect.Y += 2;
                    rect.Height -= 6;
                    break;
                case TabAlignment.Left:
                    rect.X += 4;
                    rect.Width -= 6;
                    break;
                case TabAlignment.Right:
                    rect.X += 2;
                    rect.Width -= 6;
                    break;
            }

            //	Ensure image is fully visible
            if (Alignment <= TabAlignment.Bottom)
            {
                if ((_StyleProvider.ImageAlign & NativeMethods.AnyLeftAlign) != 0)
                {
                    imageRect = new Rectangle((int)rect.X,
                        (int)rect.Y + (int)Math.Floor((double)((int)rect.Height - 16) / 2), 16, 16);
                    while (!tabBorderPath.IsVisible(imageRect.X, imageRect.Y)) imageRect.X += 1;
                    imageRect.X += 4;
                }
                else if ((_StyleProvider.ImageAlign & NativeMethods.AnyCenterAlign) != 0)
                {
                    imageRect = new Rectangle(
                        (int)rect.X +
                        (int)Math.Floor((double)(((int)rect.Right - (int)rect.X - (int)rect.Height + 2) / 2)),
                        (int)rect.Y + (int)Math.Floor((double)((int)rect.Height - 16) / 2), 16, 16);
                }
                else
                {
                    imageRect = new Rectangle((int)rect.Right,
                        (int)rect.Y + (int)Math.Floor((double)((int)rect.Height - 16) / 2), 16, 16);
                    while (!tabBorderPath.IsVisible(imageRect.Right, imageRect.Y)) imageRect.X -= 1;
                    imageRect.X -= 4;
                }
            }
            else
            {
                if ((_StyleProvider.ImageAlign & NativeMethods.AnyLeftAlign) != 0)
                {
                    imageRect = new Rectangle((int)rect.X + (int)Math.Floor((double)((int)rect.Width - 16) / 2),
                        (int)rect.Y, 16, 16);
                    while (!tabBorderPath.IsVisible(imageRect.X, imageRect.Y)) imageRect.Y += 1;
                    imageRect.Y += 4;
                }
                else if ((_StyleProvider.ImageAlign & NativeMethods.AnyCenterAlign) != 0)
                {
                    imageRect = new Rectangle((int)rect.X + (int)Math.Floor((double)((int)rect.Width - 16) / 2),
                        (int)rect.Y +
                        (int)Math.Floor((double)(((int)rect.Bottom - (int)rect.Y - (int)rect.Width + 2) / 2)), 16, 16);
                }
                else
                {
                    imageRect = new Rectangle((int)rect.X + (int)Math.Floor((double)((int)rect.Width - 16) / 2),
                        (int)rect.Bottom, 16, 16);
                    while (!tabBorderPath.IsVisible(imageRect.X, imageRect.Bottom)) imageRect.Y -= 1;
                    imageRect.Y -= 4;
                }
            }

            return imageRect;
        }

        public Rectangle GetTabCloserRect(int index)
        {
            var closerRect = new Rectangle();
            using (var path = _StyleProvider.GetTabBorder(index))
            {
                var rect = path.GetBounds();

                //	Make it shorter or thinner to fit the height or width because of the padding added to the tab for painting
                switch (Alignment)
                {
                    case TabAlignment.Top:
                        rect.Y += 4;
                        rect.Height -= 6;
                        break;
                    case TabAlignment.Bottom:
                        rect.Y += 2;
                        rect.Height -= 6;
                        break;
                    case TabAlignment.Left:
                        rect.X += 4;
                        rect.Width -= 6;
                        break;
                    case TabAlignment.Right:
                        rect.X += 2;
                        rect.Width -= 6;
                        break;
                }

                if (Alignment <= TabAlignment.Bottom)
                {
                    if (RightToLeftLayout)
                    {
                        closerRect = new Rectangle((int)rect.Left,
                            (int)rect.Y + (int)Math.Floor((double)((int)rect.Height - 6) / 2), 6, 6);
                        while (!path.IsVisible(closerRect.Left, closerRect.Y) && closerRect.Right < Width)
                            closerRect.X += 1;
                        closerRect.X += 4;
                    }
                    else
                    {
                        closerRect = new Rectangle((int)rect.Right,
                            (int)rect.Y + (int)Math.Floor((double)((int)rect.Height - 6) / 2), 6, 6);
                        while (!path.IsVisible(closerRect.Right, closerRect.Y) && closerRect.Right > -6)
                            closerRect.X -= 1;
                        closerRect.X -= 4;
                    }
                }
                else
                {
                    if (RightToLeftLayout)
                    {
                        closerRect = new Rectangle((int)rect.X + (int)Math.Floor((double)((int)rect.Width - 6) / 2),
                            (int)rect.Top, 6, 6);
                        while (!path.IsVisible(closerRect.X, closerRect.Top) && closerRect.Bottom < Height)
                            closerRect.Y += 1;
                        closerRect.Y += 4;
                    }
                    else
                    {
                        closerRect = new Rectangle((int)rect.X + (int)Math.Floor((double)((int)rect.Width - 6) / 2),
                            (int)rect.Bottom, 6, 6);
                        while (!path.IsVisible(closerRect.X, closerRect.Bottom) && closerRect.Top > -6)
                            closerRect.Y -= 1;
                        closerRect.Y -= 4;
                    }
                }
            }

            return closerRect;
        }

        public new Point MousePosition
        {
            get
            {
                var loc = PointToClient(Control.MousePosition);
                if (RightToLeftLayout) loc.X = Width - loc.X;
                return loc;
            }
        }

        #endregion
    }
}