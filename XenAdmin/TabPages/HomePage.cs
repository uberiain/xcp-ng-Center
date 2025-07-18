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
using System.Windows.Forms;
using XenAdmin.Controls;
using XenAdmin.Commands;


namespace XenAdmin.TabPages
{
    public partial class HomePage : DoubleBufferedPanel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _initializing;
        private readonly string _html;

        public HomePage()
        {
            InitializeComponent();

            switch (Program.CurrentLanguage)
            {
                default:
                    _html = Properties.Resources.HomePage;
                    break;
            }

            Load();
        }

        private void Load()
        {
            _initializing = true;

            try
            {
                webBrowser.DocumentText = _html;
            }
            catch (Exception ex)
            {
                log.Error($"Failed to load the HomePage for {Program.CurrentLanguage}", ex);
            }

            _initializing = false;
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (_initializing)
                return;

            e.Cancel = true;

            string url = e.Url.OriginalString;

            if (url.StartsWith("XenCenter://", StringComparison.InvariantCultureIgnoreCase))
            {
                if (url.Contains("AddServer"))
                {
                    new AddHostCommand(Program.MainWindow, this).Run();
                }
            }
            else
            {
                Program.OpenURL(url);
            }
        }

        public string HelpID => "TabPageHome";
    }
}
