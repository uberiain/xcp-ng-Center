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
using XenAdmin.Actions;
using XenAdmin.Core;
using XenAPI;
using XenAdmin.Dialogs;


namespace XenAdmin.Commands
{
    /// <summary>
    /// Forgets the selected SRs. Shows a confirmation dialog.
    /// </summary>
    internal class ForgetSRCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of this Command. The parameter-less constructor is required if 
        /// this Command is to be attached to a ToolStrip menu item or button. It should not be used in any other scenario.
        /// </summary>
        public ForgetSRCommand()
        {
        }

        public ForgetSRCommand(IMainWindow mainWindow, IEnumerable<SelectedItem> selection)
            : base(mainWindow, selection)
        {
        }

        public ForgetSRCommand(IMainWindow mainWindow, SR sr)
            : base(mainWindow, sr)
        {
        }

        protected override void RunCore(SelectedItemCollection selection)
        {
            List<AsyncAction> actions = new List<AsyncAction>();
            foreach (SR sr in selection.AsXenObjects<SR>(CanRun))
            {
                actions.Add(new ForgetSrAction(sr));
            }
            RunMultipleActions(actions, Messages.ACTION_SRS_FORGETTING, Messages.FORGETTING_SRS, Messages.SRS_FORGOTTEN, true);
        }

        protected override bool CanRunCore(SelectedItemCollection selection)
        {
            return selection.AllItemsAre<SR>() && selection.AtLeastOneXenObjectCan<SR>(CanRun);
        }

        private static bool CanRun(SR sr)
        {
            return sr != null
                && !sr.HasRunningVMs()
                && sr.CanCreateWithXenCenter()
                && sr.allowed_operations.Contains(storage_operations.forget)
                && !HelpersGUI.GetActionInProgress(sr);
        }

        public override string MenuText => Messages.MAINWINDOW_FORGET_SR;

        protected override bool ConfirmationRequired => true;

        protected override string ConfirmationDialogText
        {
            get
            {
                List<SR> srs = GetSelection().AsXenObjects<SR>();
                if (srs.Count == 1)
                {
                    return string.Format(Messages.MESSAGEBOX_FORGET_SR_CONTINUE, srs[0].Name());
                }
                return Messages.MESSAGEBOX_FORGET_SRS_CONTINUE;
            }
        }

        protected override string ConfirmationDialogTitle
        {
            get
            {
                if (GetSelection().Count == 1)
                {
                    return Messages.MESSAGEBOX_FORGET_SR_CONTINUE_TITLE;
                }
                return Messages.MESSAGEBOX_FORGET_SRS_CONTINUE_TITLE;
            }
        }

        protected override CommandErrorDialog GetErrorDialogCore(IDictionary<IXenObject, string> cantRunReasons)
        {
            return new CommandErrorDialog(Messages.ERROR_DIALOG_FORGET_SR_TITLE, Messages.ERROR_DIALOG_FORGET_SR_TEXT, cantRunReasons);
        }

        protected override string GetCantRunReasonCore(IXenObject item)
        {
            SR sr = item as SR;
            if (sr == null)
            {
                return base.GetCantRunReasonCore(item);
            }
            if (sr.HasRunningVMs())
            {
                return Messages.SR_HAS_RUNNING_VMS;
            }
            else if (!sr.CanCreateWithXenCenter())
            {
                return string.Format(Messages.SR_CANNOT_BE_FORGOTTEN_WITH_XC, BrandManager.BrandConsole);
            }
            else if (HelpersGUI.GetActionInProgress(sr))
            {
                return Messages.SR_ACTION_IN_PROGRESS;
            }
            return base.GetCantRunReasonCore(item);
        }

        protected override string ConfirmationDialogYesButtonLabel => Messages.MESSAGEBOX_FORGET_SR_CONTINUE_YES_BUTTON_LABEL;

        protected override bool ConfirmationDialogNoButtonSelected => true;
    }
}
