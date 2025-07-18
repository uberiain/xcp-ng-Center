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
using XenAdmin.Controls;


namespace XenAdmin.Wizards.NewVMWizard
{
    public partial class Page_Finish : XenTabPage
    {
        public Page_Finish()
        {
            InitializeComponent();
        }

        public override string Text => Messages.NEWVMWIZARD_FINISHPAGE_NAME;

        public override string PageTitle => Messages.NEWVMWIZARD_FINISHPAGE_TITLE;

        public override string HelpID => "Finish";

        public override string NextText(bool isLastPage)
        {
            return Messages.NEWVMWIZARD_FINISHPAGE_CREATE;
        }

        public bool StartImmediately => AutoStartCheckBox.Checked;

        private bool _canStartImmediately = true;

        public bool CanStartImmediately
        {
            get => _canStartImmediately;
            set => _canStartImmediately = AutoStartCheckBox.Checked = AutoStartCheckBox.Enabled = value;
        }

        protected override void PageLoadedCore(PageLoadedDirection direction)
        {
            SummaryGridView.Rows.Clear();
            
            if (SummaryRetriever == null)
                return;

            var entries = SummaryRetriever.Invoke();
            foreach (var pair in entries)
                SummaryGridView.Rows.Add(pair.Key, pair.Value);
        }

        public override void SelectDefaultControl()
        {
            if(CanStartImmediately)
                AutoStartCheckBox.Select();
        }

        public Func<IEnumerable<KeyValuePair<string, string>>> SummaryRetriever { private get; set; }
    }
}
