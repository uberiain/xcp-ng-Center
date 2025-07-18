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
using System.Linq;
using XenAdmin.Controls;
using XenAdmin.Core;
using XenAdmin.Network;
using XenAPI;
using XenAdmin.Actions;


namespace XenAdmin.Dialogs
{
    public partial class NewDiskDialog : XenDialogBase
    {
        #region Private fields

        private readonly VM _vm;
        private readonly VDI _diskTemplate;
        private readonly IEnumerable<VDI> _vdiNamesInUse = new List<VDI>();

        #endregion

        #region Constructors

        public NewDiskDialog(IXenConnection connection, SR sr)
            : base(connection ?? throw new ArgumentNullException(nameof(connection)))
        {
            InitializeComponent();

            NameTextBox.Text = GetDefaultVDIName();
            diskSpinner1.Populate();
            srPicker.Populate(SrPicker.SRPickerType.InstallFromTemplate, connection, null, sr, new[] { NewDisk() });
            buttonRescan.Enabled = srPicker.CanBeScanned;
            UpdateErrorsAndButtons();
        }

        public NewDiskDialog(IXenConnection connection, VM vm, Host affinity,
            SrPicker.SRPickerType pickerUsage = SrPicker.SRPickerType.VM, VDI diskTemplate = null,
            bool canResize = true, long minSize = 0, IEnumerable<VDI> vdiNamesInUse = null)
            : base(connection ?? throw new ArgumentNullException(nameof(connection)))
        {
            InitializeComponent();

            _vm = vm;
            _vdiNamesInUse = vdiNamesInUse ?? new List<VDI>();
            diskSpinner1.CanResize = canResize;

            if (diskTemplate == null)
            {
                NameTextBox.Text = GetDefaultVDIName();
                diskSpinner1.Populate(minSize: minSize);
                srPicker.Populate(pickerUsage, connection, affinity, null, new[] { NewDisk() });
                buttonRescan.Enabled = srPicker.CanBeScanned;
                UpdateErrorsAndButtons();
            }
            else
            {
                _diskTemplate = diskTemplate;
                NameTextBox.Text = _diskTemplate.Name();
                DescriptionTextBox.Text = _diskTemplate.Description();
                Text = Messages.EDIT_DISK;
                OkButton.Text = Messages.OK;
                diskSpinner1.Populate(_diskTemplate.virtual_size, minSize);
                srPicker.Populate(pickerUsage, connection, affinity, connection.Resolve(_diskTemplate.SR), new[] { NewDisk() });
                buttonRescan.Enabled = srPicker.CanBeScanned;
                UpdateErrorsAndButtons();
            }
        }

        #endregion

        public VDI Disk { get; private set; }

        public VBD Device { get; private set; }

        public bool DontCreateVDI { get; set; }

        internal override string HelpName => _diskTemplate == null ? "NewDiskDialog" : "EditNewDiskDialog";

        private string GetDefaultVDIName()
        {
            List<string> usedNames = new List<string>();
            foreach (VDI v in connection.Cache.VDIs.Concat(_vdiNamesInUse))
            {
                usedNames.Add(v.Name());
            }
            return Helpers.MakeUniqueName(Messages.DEFAULT_VDI_NAME, usedNames);
        }

        private void srListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateErrorsAndButtons();
        }

        private void srPicker_CanBeScannedChanged()
        {
            buttonRescan.Enabled = srPicker.CanBeScanned;
            UpdateErrorsAndButtons();
        }

        private void buttonRescan_Click(object sender, EventArgs e)
        {
            srPicker.ScanSRs();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (srPicker.SR == null || NameTextBox.Text == "" || !connection.IsConnected)
                return;

            Disk = NewDisk();
            Device = NewDevice();

            if (DontCreateVDI)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            SR sr = srPicker.SR;
            var actions = new List<AsyncAction>();

            if (!sr.shared && _vm != null && _vm.HaPriorityIsRestart())
            {
                using (var dlg = new WarningDialog(Messages.NEW_SR_DIALOG_ATTACH_NON_SHARED_DISK_HA,
                                ThreeButtonDialog.ButtonYes, ThreeButtonDialog.ButtonNo))
                {
                    if (dlg.ShowDialog(this) != DialogResult.Yes)
                        return;
                }

                actions.Add(new HAUnprotectVMAction(_vm));
            }

            if (_vm != null)
            {
                //note that this action alters the Device
                actions.Add(new CreateDiskAction(Disk, Device, _vm));

                // Now try to plug the VBD.
                var plugAction = new VbdCreateAndPlugAction(_vm, Device, Disk.Name(), false);
                plugAction.ShowUserInstruction += PlugAction_ShowUserInstruction;
                actions.Add(plugAction);
            }
            else
            {
                actions.Add(new CreateDiskAction(Disk));
            }

            new MultipleAction(connection, "", "", "", actions, true, true, true).RunAsync();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void PlugAction_ShowUserInstruction(string message)
        {
            Program.Invoke(Program.MainWindow, () =>
            {
                if (!Program.RunInAutomatedTestMode)
                {
                    using (var dlg = new InformationDialog(message))
                        dlg.ShowDialog(Program.MainWindow);
                }
            });
        }

        private VDI NewDisk()
        {
            VDI vdi = new VDI
            {
                Connection = connection,
                read_only = _diskTemplate?.read_only ?? false,
                SR = srPicker.SR == null ? new XenRef<SR>(Helper.NullOpaqueRef) : new XenRef<SR>(srPicker.SR),
                virtual_size = diskSpinner1.SelectedSize,
                name_label = NameTextBox.Text,
                name_description = DescriptionTextBox.Text,
                sharable = _diskTemplate?.sharable ?? false,
                type = _diskTemplate?.type ?? vdi_type.user
            };
            vdi.SetVmHint(_vm != null ? _vm.uuid : "");
            return vdi;
        }

        private VBD NewDevice()
        {
            VBD vbd = new VBD();
            vbd.Connection = connection;
            vbd.device = "";
            vbd.empty = false;
            vbd.type = vbd_type.Disk;
            vbd.mode = vbd_mode.RW;
            vbd.SetIsOwner(true);
            vbd.unpluggable = true;
            return vbd;

        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateErrorsAndButtons();
        }

        private void diskSpinner1_SelectedSizeChanged()
        {
            srPicker.UpdateDisks(NewDisk());
            UpdateErrorsAndButtons();
        }

        private void UpdateErrorsAndButtons()
        {
            // Ordering is important here, we want to show the most relevant message
            // The error should be shown only for size errors

            if (!diskSpinner1.IsSizeValid)
            {
                OkButton.Enabled = false;
                return;
            }

            bool allDisabled = true;
            bool anyScanning = false;

            foreach (SrPickerItem item in srPicker.Items)
            {
                if (item == null)
                    continue;
                
                if (item.Enabled)
                {
                    allDisabled = false;
                    break;
                }

                if (item.Scanning)
                    anyScanning = true;
            }

            if (allDisabled)
            {
                OkButton.Enabled = false;
                diskSpinner1.SetError(anyScanning ? null : Messages.NO_VALID_DISK_LOCATION);
                return;
            }

            if (srPicker.SR == null) //enabled SR exists but the user selects a disabled one
            {
                OkButton.Enabled = false;
                diskSpinner1.SetError(null);
                return;
            }

            if (string.IsNullOrEmpty(NameTextBox.Text.Trim()))
            {
                OkButton.Enabled = false;
                diskSpinner1.SetError(null);
                return;
            }

            OkButton.Enabled = true;
            diskSpinner1.SetError(null);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
