/* Copyright (c) Cloud Software Group, Inc. 
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
using XenAdmin.Controls;
using XenAdmin.Core;


namespace XenAdmin.Wizards.ConversionWizard
{
    public partial class SummaryPage :  XenTabPage
    {
        public SummaryPage()
        {
            InitializeComponent();
        }

        public Func<IEnumerable<KeyValuePair<string, string>>> SummaryRetreiver { private get; set; }

        #region XenTabPage implementation

        public override string Text => Messages.CONVERSION_SUMMARY_PAGE_TEXT;

        public override string PageTitle => Messages.CONVERSION_SUMMARY_PAGE_TITLE;

        public override string HelpID => "ConversionSummary";

        protected override void PageLoadedCore(PageLoadedDirection direction)
        {
            if (direction == PageLoadedDirection.Forward)
            {
                if (SummaryRetreiver == null)
                    return;

                var entries = SummaryRetreiver.Invoke();
                m_dataGridView.Rows.Clear();

                foreach (var pair in entries)
                {
                    var row = new DataGridViewRow();
                    row.Cells.AddRange(new DataGridViewTextBoxCell { Value = pair.Key },
                        new DataGridViewTextBoxCell { Value = pair.Value });
                    m_dataGridView.Rows.Add(row);
                }

                HelpersGUI.ResizeGridViewColumnToAllCells(Column2);//set properly the width of the last column
            }
        }

        #endregion
    }
}
