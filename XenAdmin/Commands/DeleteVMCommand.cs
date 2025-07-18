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
using System.Windows.Forms;
using XenAdmin.Actions;
using XenAdmin.Core;
using XenAdmin.Dialogs;
using XenAPI;
using XenAdmin.Actions.VMActions;


namespace XenAdmin.Commands
{
    /// <summary>
    /// Deletes the selected VMs. Shows a confirmation dialog.
    /// </summary>
    internal class DeleteVMCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of this Command. The parameter-less constructor is required in the derived
        /// class if it is to be attached to a ToolStrip menu item or button. It should not be used in any other scenario.
        /// </summary>
        public DeleteVMCommand()
        {
        }

        public DeleteVMCommand(IMainWindow mainWindow, IEnumerable<SelectedItem> selection)
            : base(mainWindow, selection)
        {
        }

        public DeleteVMCommand(IMainWindow mainWindow, SelectedItem selection)
            : base(mainWindow, selection)
        {
        }

        public DeleteVMCommand(IMainWindow mainWindow, VM vm)
            : base(mainWindow, vm)
        {
        }

        protected sealed override void RunCore(SelectedItemCollection selection)
        {
            ConfirmVMDeleteDialog dialog = new ConfirmVMDeleteDialog(selection.AsXenObjects<VM>());

            if (MainWindowCommandInterface.RunInAutomatedTestMode || dialog.ShowDialog(Parent) == DialogResult.Yes)
            {
                CommandErrorDialog errorDialog = null;
                var cantRunReasons = GetCantRunReasons();

                if (cantRunReasons.Count > 0)
                {
                    errorDialog = new CommandErrorDialog(ErrorDialogTitle, ErrorDialogText, cantRunReasons);
                }

                List<AsyncAction> actions = new List<AsyncAction>();
                foreach (VM vm in selection.AsXenObjects<VM>(CanRun))
                {
                    var snapshotsToDelete = dialog.DeleteSnapshots.FindAll(x => x.Connection.Resolve(x.snapshot_of) == vm);
                    actions.Add(new VMDestroyAction(vm, dialog.DeleteDisks, snapshotsToDelete));
                }
                RunMultipleActions(actions, Messages.ACTION_VMS_DESTROYING_TITLE, Messages.ACTION_VM_DESTROYING, Messages.ACTION_VM_DESTROYED, true);

                if (errorDialog != null)
                {
                    errorDialog.ShowDialog(Parent);
                }
            }
        }

        protected virtual bool CanRun(VM vm)
        {
            return vm != null && !vm.is_a_template && !vm.Locked && vm.allowed_operations != null && vm.allowed_operations.Contains(vm_operations.destroy);
        }

        protected sealed override bool CanRunCore(SelectedItemCollection selection)
        {
            return selection.AllItemsAre<VM>() && selection.AtLeastOneXenObjectCan<VM>(CanRun);
        }

        public override string MenuText => Messages.MAINWINDOW_DELETE_OBJECTS;

        protected virtual string ErrorDialogText
        {
            get
            {
                return Messages.ERROR_DIALOG_DELETE_VM_TEXT;
            }
        }

        protected virtual string ErrorDialogTitle
        {
            get
            {
                return Messages.ERROR_DIALOG_DELETE_VM_TITLE;
            }
        }

        protected override string GetCantRunReasonCore(IXenObject item)
        {
            VM vm = item as VM;
            if (vm == null)
            {
                return base.GetCantRunReasonCore(item);
            }
            if (!vm.is_a_template && vm.power_state != vm_power_state.Halted)
            {
                return Messages.VM_NOT_SHUT_DOWN;
            }
            else if (vm.is_a_template && vm.DefaultTemplate())
            {
                return string.Format(Messages.CANNOT_DELETE_DEFAULT_TEMPLATE, BrandManager.ProductBrand);
            }
            return base.GetCantRunReasonCore(item);
        }
    }
}
