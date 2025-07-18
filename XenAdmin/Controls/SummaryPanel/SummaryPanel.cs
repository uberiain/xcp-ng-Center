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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace XenAdmin.Controls.SummaryPanel
{
    public partial class SummaryPanel : UserControl, ISummaryPanelView
    {
        public SummaryPanelController Controller { private get; set; }
        public SummaryPanel()
        {
            InitializeComponent();
            licenseHelperLinkLabel.LinkClicked += licenseHelperLink_LinkClicked;
            supportHelperLinkLabel.LinkClicked += supportHelperLink_LinkClicked;
            information.LinkClicked += information_LinkClicked;
        }

        public string Title
        {
            set{ Controller.Title = value; }
        }

        public string LicenseHelperUrlText
        {
            set { Controller.LicenseHelperUrlText = value; }
        }

        public string SupportHelperUrlText
        {
            set { Controller.SupportHelperUrlText = value; }
        }

        public bool LicenseHelperUrlVisible
        {
            set { Controller.LicenseHelperUrlVisible = value; }
        }

        public bool SupportHelperUrlVisible
        {
            set { Controller.SupportHelperUrlVisible = value; }
        }

        public bool LicenseWarningVisible
        {
            set { Controller.DisplayLicenseWarning = value; }
        }

        public bool SupportWarningVisible
        {
            set { Controller.DisplaySupportWarning = value; }
        }

        public bool InformationVisible
        {
            set { Controller.InformationVisible = value; }
        }

        public string LicenseWarningText
        {
            set { Controller.LicenseWarningMessage = value; }
        }

        public string SupportWarningText
        {
            set { Controller.SupportWarningMessage = value; }
        }

        public Action RunOnLicenseUrlClick
        {
            set { Controller.RunOnLicenseUrlClick = value; }
        }

        public Action RunOnSupportUrlClick
        {
            set { Controller.RunOnSupportUrlClick = value; }
        }

        public SummaryTextComponent SummaryText
        {
            set { Controller.TextSummary = value; }
        }

        public string InformationText
        {
            set { Controller.InformationText = value; }
        }

        public Bitmap LicenseWarningIcon
        {
            set { Controller.LicenseWarningIcon = value; }

        }

        public Bitmap SupportWarningIcon
            {
                set { Controller.SupportWarningIcon = value; }
            }

        private void licenseHelperLink_LinkClicked(object sender, EventArgs e)
        {
            Controller.LicenseUrlClicked();
        }

        private void supportHelperLink_LinkClicked(object sender, EventArgs e)
        {
            Controller.SupportUrlClicked();
        }

        private string summaryLink;

        private void information_LinkClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(summaryLink))
                Program.OpenURL(summaryLink);
        }
        
        #region ISummaryPanelView Members
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawTitle
        {
            set { titleLabel.Text = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawLicenseWarningMessage
        {
            set { licenseWarningLabel.Text = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawSupportWarningMessage
        {
            set { supportWarningLabel.Text = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Bitmap DrawLicenseWarningIcon
        {
            set { licenseWarningImage.Image = value;  }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Bitmap DrawSupportWarningIcon
        {
            set { supportWarningImage.Image = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Bitmap DrawInformationIcon
        {
            set { informationImage.Image = value;  }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawLicenseHelperUrlText
        {
            set { licenseHelperLinkLabel.Text = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawSupportHelperUrlText
        {
            set { supportHelperLinkLabel.Text = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool LicenseWarningTextVisibility
        {
            set { licenseWarningLabel.Visible = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool SupportWarningTextVisibility
        {
            set { supportWarningLabel.Visible = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool LicenseWarningIconVisibility
        {
            set { licenseWarningImage.Visible = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool SupportWarningIconVisibility
        {
            set { supportWarningImage.Visible = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawSummaryText
        {
            set
            {
                information.Text = value;
                information.LinkArea = new LinkArea(0, 0);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawSummaryLink
        {
            set { summaryLink = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public LinkArea DrawSummaryLinkArea
        {
            set { information.LinkArea = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DrawLicenseUrlVisible
        {
            set { licenseHelperLinkLabel.Visible = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DrawSupportUrlVisible
        {
            set { supportHelperLinkLabel.Visible = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DrawInformationVisible
        {
            set { informationLayoutPanel.Visible = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DrawInformationText
        {
            set { informationLabel.Text = value; }
        }

        #endregion
    }
}
