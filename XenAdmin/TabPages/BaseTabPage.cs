﻿/* Copyright (c) Cloud Software Group, Inc. 
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

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using XenAdmin.Controls;
using XenAdmin.Controls.GradientPanel;

namespace XenAdmin.TabPages
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class BaseTabPage : UserControl
    {
        // Colours etc. that are used by more than one tab page
        internal const int ITEM_SPACING = 4;

        internal static readonly Color HeaderBorderColor = Color.FromArgb(128, 163, 189);
        internal static readonly Color HeaderBackColor = Color.Transparent;
        internal static readonly Color HeaderForeColor = SystemColors.ControlText;

        internal static readonly Color ItemBackColor = Color.Transparent;
        internal static readonly Color ItemLabelForeColor = SystemColors.HotTrack;
        internal static readonly Color ItemValueForeColor = SystemColors.ControlText;
        internal static readonly Font ItemLabelFont = Program.DefaultFontBold;
        internal static readonly Font ItemValueFont = Program.DefaultFont;
        internal static readonly Font ItemValueFontBold = Program.DefaultFontBold;

        private Font titleLabelFont = new Font(DefaultFont.FontFamily, DefaultFont.Size + 1f, FontStyle.Bold);

        public BaseTabPage()
        {
            InitializeComponent();
            titleLabel.Font = titleLabelFont;
            titleLabel.ForeColor = HorizontalGradientPanel.TextColor;
        }

        public override string Text
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        protected DeprecationBanner Banner => deprecationBanner1;

        public virtual void PageHidden()
        {
        }

        public virtual string HelpID => "";
    }
}
