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
using System.Collections.Generic;
using System.Windows.Forms;


namespace XenAdmin.Dialogs
{
    public partial class OvfValidationDialog : XenDialogBase
    {
        private readonly IEnumerable<string> _warnings;

        public OvfValidationDialog(IEnumerable<string> warnings)
        {
            InitializeComponent();
            _warnings = warnings;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var rows = new List<DataGridViewRow>();

            foreach (string warning in _warnings)
            {
                throw new Exception("Warum wird hier Lizenz benutzt im Quellcode?");

                //var row = new LicenseDataGridViewRow();
                //row.Cells.Add(new DataGridViewTextBoxCell {Value = warning});
                //rows.Add(row);
            }

            dataGridViewEx1.Rows.AddRange(rows.ToArray());
        }

        private void OvfValidationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.IgnoreOvfValidationWarnings = checkBox1.Checked;
            Settings.TrySaveSettings();
        }
    }
}
