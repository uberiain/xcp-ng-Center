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

using System.Collections.Generic;
using XenAdmin.Core;
using XenAdmin.Dialogs;
using XenAdmin.Wizards.DRWizards;
using XenAPI;


namespace XenAdmin.Commands
{
    /// <summary>
    /// Launches the Dry-run Wizard.
    /// </summary>
    internal class DRFailbackCommand : Command
    {
        private DRFailoverWizard _wizard;

        /// <summary>
        /// Initializes a new instance of this Command. The parameter-less constructor is required in the derived
        /// class if it is to be attached to a ToolStrip menu item or button. It should not be used in any other scenario.
        /// </summary>
        public DRFailbackCommand()
        {
        }

        public DRFailbackCommand(IMainWindow mainWindow, IEnumerable<SelectedItem> selection)
            : base(mainWindow, selection)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DRFailoverCommand"/> class. 
        /// </summary>
        /// <param name="mainWindow">The main window interface. It can be found at MainWindow.CommandInterface.</param>
        public DRFailbackCommand(IMainWindow mainWindow)
            : base(mainWindow)
        {
        }

        public DRFailbackCommand(IMainWindow mainWindow, IXenObject selection)
            : base(mainWindow, selection)
        {
        }


        protected override void RunCore(SelectedItemCollection selection)
        {

            var pool = Helpers.GetPoolOfOne(selection.FirstAsXenObject.Connection);
            if (pool != null)
            {
                _wizard = new DRFailoverWizard(pool, DRWizardType.Failback);
                this.MainWindowCommandInterface.ShowPerConnectionWizard(pool.Connection, _wizard);
            }
        }

        protected override bool CanRunCore(SelectedItemCollection selection)
        {
            return selection.Count == 1 && selection.FirstAsXenObject != null && selection.FirstAsXenObject.Connection != null;
        }

        public override string ContextMenuText
        {
            get
            {
                return Messages.DR_FAILBACK_AMP;
            }
        }
    }
}
