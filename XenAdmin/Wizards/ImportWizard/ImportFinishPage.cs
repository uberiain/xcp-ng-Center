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
using XenAdmin.Controls;
using XenAdmin.Core;


namespace XenAdmin.Wizards.ImportWizard
{
	/// <summary>
	/// Class representing the last page of the Import/Export wizards;
	/// here the user reviews the settings specified on the previous wizard pages
	/// </summary>
	public partial class ImportFinishPage : XenTabPage
	{
		public ImportFinishPage()
		{
			InitializeComponent();
		}

		#region Base class (XenTabPage) overrides

		/// <summary>
		/// The page's label in the (left hand side) wizard progress panel
		/// </summary>
		public override string Text => Messages.FINISH_PAGE_TEXT;

        /// <summary>
		/// Gets the page's title (headline)
		/// </summary>
		public override string PageTitle => Messages.FINISH_PAGE_TITLE_IMPORT;

        protected override bool ImplementsIsDirty()
        {
            return true;
        }

		public override void PopulatePage()
		{
			if (SummaryRetriever == null)
				return;

			var entries = SummaryRetriever.Invoke();
			m_dataGridView.Rows.Clear();

			foreach (var pair in entries)
			{
				var row = new DataGridViewRow();
				row.Cells.AddRange(new DataGridViewTextBoxCell { Value = pair.Key }, new DataGridViewTextBoxCell { Value = pair.Value });
				m_dataGridView.Rows.Add(row);
			}

            HelpersGUI.ResizeGridViewColumnToAllCells(Column2);//set properly the width of the last column
		}

		#endregion

        public Func<IEnumerable<KeyValuePair<string, string>>> SummaryRetriever { private get; set; }

        private bool _canStartVmsAutomatically = true;
        public bool CanStartVmsAutomatically
        {
            get => _canStartVmsAutomatically;
            set => _canStartVmsAutomatically = m_checkBoxStartVms.Enabled = m_checkBoxStartVms.Checked = value;
        }

        /// <summary>
		/// Do the action described after the import/export has finished?
		/// </summary>
		public bool StartVmsAutomatically => CanStartVmsAutomatically && m_checkBoxStartVms.Visible && m_checkBoxStartVms.Checked;

        public bool ShowStartVmsGroupBox
        {
            set => m_checkBoxStartVms.Visible = value;
        }
    }
}
