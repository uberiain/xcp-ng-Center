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
using System.Linq;
using XenAdmin.Actions.VMActions;
using XenAdmin.Controls;
using XenAPI;


namespace XenAdmin.Dialogs.VMDialogs
{
    public partial class MoveVMDialog : XenDialogBase
    {
        private VM vm;

        public MoveVMDialog(VM vm)
        {
            InitializeComponent();
            this.vm = vm;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            EnableMoveButton();

            var vdis = (from VBD vbd in vm.Connection.ResolveAll(vm.VBDs)
                where vbd.GetIsOwner() && vbd.type != vbd_type.CD
                let vdi = vm.Connection.Resolve(vbd.VDI)
                where vdi != null
                select vdi).ToArray();

            srPicker1.Populate(SrPicker.SRPickerType.Move, vm.Connection, vm.Home(), null, vdis);
        }

        private void EnableMoveButton()
        {
            buttonMove.Enabled = srPicker1.SR != null;
        }

        #region Control event handlers

        private void srPicker1_DoubleClickOnRow(object sender, EventArgs e)
        {
            if (buttonMove.Enabled)
                buttonMove.PerformClick();
        }

        private void buttonRescan_Click(object sender, EventArgs e)
        {
            srPicker1.ScanSRs();
        }

        private void buttonMove_Click(object sender, EventArgs e)
        {
            var action = new VMMoveAction(vm, srPicker1.SR, vm.GetStorageHost(false));
            action.RunAsync();
            Close();
        }

        private void srPicker1_CanBeScannedChanged()
        {
            buttonRescan.Enabled = srPicker1.CanBeScanned;
            EnableMoveButton();
        }

        private void srPicker1_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableMoveButton();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
