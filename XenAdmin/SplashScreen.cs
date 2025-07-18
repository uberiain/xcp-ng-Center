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
using System.Drawing;
using System.Windows.Forms;
using XenAdmin.Core;

namespace XenAdmin
{
    public partial class SplashScreen : Form
    {
        public event Action ShowMainWindowRequested;

        public volatile bool AllowToClose;

        public SplashScreen()
        {
            InitializeComponent();
            pictureBox1.Image = Images.StaticImages.splash;
            label1.Text = "Version " + Program.VersionText;
            //labelCopyright.Text = BrandManager.Copyright;
            //labelCopyright.ForeColor = Color.FromArgb(39, 52, 64);

            //setting the parent is needed so the transparency can show
            //the picturebox content instead of the control behind it
            //labelCopyright.Parent = pictureBox1;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!AllowToClose)
                return;

            timer1.Stop();
            ShowMainWindowRequested?.Invoke();
        }
    }
}
