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
using System.Linq;
using System.Windows.Forms;
using XenAdmin.Actions;
using XenAdmin.Controls;
using XenAdmin.Core;
using XenAdmin.Dialogs;
using XenAdmin.Dialogs.WarningDialogs;
using XenAdmin.Network;
using XenAPI;
using System.Drawing;


namespace XenAdmin.Wizards.NewSRWizard_Pages.Frontends
{
    public partial class LVMoHBA : XenTabPage
    {
        private List<FibreChannelDevice> _selectedDevices = new List<FibreChannelDevice>();

        public LVMoHBA()
        {
            InitializeComponent();
            SrType = SR.SRTypes.lvmohba;
        }

        public SR.SRTypes SrType { get; set; }

        protected virtual bool ShowNicColumn => false;

        private FibreChannelDescriptor CreateSrDescriptor(FibreChannelDevice device)
        {
            return SrType == SR.SRTypes.gfs2 ? CreateGfs2Descriptor(device) : CreateLvmSrDescriptor(device);
        }

        protected virtual FibreChannelDescriptor CreateLvmSrDescriptor(FibreChannelDevice device)
        {
            return new LvmOhbaSrDescriptor(device);
        }

        protected virtual FibreChannelDescriptor CreateGfs2Descriptor(FibreChannelDevice device)
        {
            return new Gfs2HbaSrDescriptor(device);
        }
        
        #region XenTabPage overrides

        public override string PageTitle => Messages.NEWSR_SELECT_LUN;

        public override string Text => Messages.NEWSR_LOCATION;

        public override string HelpID => "Location_HBA";

        protected override void PageLeaveCore(PageLoadedDirection direction, ref bool cancel)
        {
            if (direction == PageLoadedDirection.Back)
                return;

            Host coordinator = Helpers.GetCoordinator(Connection);
            if (coordinator == null)
            {
                cancel = true;
                return;
            }

            SrDescriptors = new List<FibreChannelDescriptor>();
            var formatDiskDescriptors = new Dictionary<FibreChannelDescriptor, FibreChannelDescriptor>(); // key = requested SR, value = existing SR

            var performSecondProbe = Helpers.KolkataOrGreater(Connection) && !Helpers.FeatureForbidden(Connection, Host.RestrictCorosync)
                && SrType != SR.SRTypes.lvmofcoe; // gfs2 over fcoe is not supported yet

            foreach (var device in _selectedDevices)
            {
                // Start probe
                var formatDiskDescriptor = CreateSrDescriptor(device);
                var currentSrDescriptor = formatDiskDescriptor;

                if (!RunProbe(coordinator, currentSrDescriptor, out List<SR.SRInfo> srs))
                {
                    cancel = true;
                    return;
                }

                if (performSecondProbe && srs.Count == 0)
                {
                    // Start second probe
                    currentSrDescriptor = SrType == SR.SRTypes.gfs2 ? CreateLvmSrDescriptor(device) : CreateGfs2Descriptor(device);

                    if (!RunProbe(coordinator, currentSrDescriptor, out srs))
                    {
                        cancel = true;
                        return;
                    }
                }

                currentSrDescriptor.UUID = srs.Select(sr => sr.UUID).FirstOrDefault();
                if (srs.Count > 0)
                    currentSrDescriptor.UpdateDeviceConfig(srs[0].Configuration);

                if (!string.IsNullOrEmpty(SrWizardType.UUID))
                {
                    // Check LUN contains correct SR
                    if (currentSrDescriptor.UUID == SrWizardType.UUID)
                    {
                        SrDescriptors.Add(currentSrDescriptor);
                        continue;
                    }

                    using (var dlog = new ErrorDialog(String.Format(Messages.INCORRECT_LUN_FOR_SR, SrWizardType.SrName)))
                        dlog.ShowDialog(this);

                    cancel = true;
                    return;
                }

                if (string.IsNullOrEmpty(currentSrDescriptor.UUID))
                {
                    // No existing SRs were found on this LUN. If allowed to create
                    // a new SR, ask the user if they want to proceed and format.
                    if (!SrWizardType.AllowToCreateNewSr)
                    {
                        using (var dlog = new ErrorDialog(Messages.NEWSR_LUN_HAS_NO_SRS))
                            dlog.ShowDialog(this);

                        cancel = true;
                        return;
                    }

                    if (!Program.RunInAutomatedTestMode)
                        formatDiskDescriptors.Add(formatDiskDescriptor, null);
                }
                else
                {
                    // Check this isn't an existing SR on the current pool
                    var existingSr = Connection.Cache.SRs.FirstOrDefault(sr => sr.uuid == currentSrDescriptor.UUID);
                    if (existingSr != null)
                    {
                        var pool = Helpers.GetPool(existingSr.Connection);
                        var errorText = pool != null
                            ? string.Format(Messages.NEWSR_LUN_IN_USE_ON_SELECTED_POOL, device.SCSIid, existingSr.Name())
                            : string.Format(Messages.NEWSR_LUN_IN_USE_ON_SELECTED_SERVER, device.SCSIid, existingSr.Name());

                        using (var dlog = new ErrorDialog(errorText))
                            dlog.ShowDialog(this);

                        cancel = true;
                        return;
                    }
                    
                    // CA-17230: Check this isn't an existing SR on any of the known pools. 
                    // If it is then just continue (i.e. do not ask the user if they want to format or reattach it, we will just reattach it)
                    existingSr = SrWizardHelpers.SrInUse(currentSrDescriptor.UUID);
                    if (existingSr != null)
                    {
                        SrDescriptors.Add(currentSrDescriptor);
                        continue;
                    }

                    // We found a SR on this LUN. Will ask user for choice later.
                    formatDiskDescriptors.Add(formatDiskDescriptor, currentSrDescriptor);
                }
            }

            if (!cancel && formatDiskDescriptors.Count > 0)
            {
                var launcher = new LVMoHBAWarningDialogLauncher(Connection, this, formatDiskDescriptors, SrType);
                launcher.ShowWarnings();
                cancel = launcher.Cancelled;
                if (!cancel && launcher.SrDescriptors.Count > 0)
                    SrDescriptors.AddRange(launcher.SrDescriptors);
            }
        }

        private bool RunProbe(Host coordinator, FibreChannelDescriptor srDescriptor, out List<SR.SRInfo> srs)
        {
            var action = new SrProbeAction(Connection, coordinator, srDescriptor.SrType, srDescriptor.DeviceConfig);

            using (var dlg = new ActionProgressDialog(action, ProgressBarStyle.Marquee))
                dlg.ShowDialog(this);

            srs = action.SRs ?? new List<SR.SRInfo>();
            return action.Succeeded;
        }

        public override bool EnableNext()
        {
            UpdateSelectionButtons();
            return _selectedDevices.Count > 0;
        }

        public override bool EnablePrevious()
        {
            if (SrWizardType.DisasterRecoveryTask && SrWizardType.SrToReattach == null)
                return false;

            return true;
        }

        protected override void PageLoadedCore(PageLoadedDirection direction)
        {
            if (direction == PageLoadedDirection.Back)
                return;

            colNic.Visible = ShowNicColumn; 
            dataGridView.Rows.Clear();

            var vendorGroups = from device in FCDevices
                         group device by device.Vendor into g
                         orderby g.Key
                         select new { VendorName = g.Key, Devices = g.OrderBy(x => x.Serial) };

            foreach (var vGroup in vendorGroups)
            {
                var vendorRow = new VendorRow(vGroup.VendorName);
                dataGridView.Rows.Add(vendorRow);

                using (var font = new Font(dataGridView.DefaultCellStyle.Font, FontStyle.Bold))
                    vendorRow.DefaultCellStyle = new DataGridViewCellStyle(dataGridView.DefaultCellStyle)
                        {
                            Font = font,
                            SelectionBackColor = dataGridView.DefaultCellStyle.BackColor,
                            SelectionForeColor = dataGridView.DefaultCellStyle.ForeColor
                        };

                var deviceRows = from device in vGroup.Devices select new FCDeviceRow(device, ShowNicColumn);
                dataGridView.Rows.AddRange(deviceRows.ToArray());
            }
        }

        public override void SelectDefaultControl()
        {
            dataGridView.Select();
        }

        #endregion

        #region Event handlers

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_srWizardType.SrToReattach != null)
                return;

            if (e.ColumnIndex != colCheck.Index || e.RowIndex < 0 || e.RowIndex > dataGridView.RowCount - 1)
                return;

            var deviceRow = dataGridView.Rows[e.RowIndex] as FCDeviceRow;
            if (deviceRow == null)
                return;

            deviceRow.Cells[colCheck.Index].Value = !(bool)deviceRow.Cells[colCheck.Index].Value;

            UpdateSelectedDevices();
            OnPageUpdated();
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_srWizardType.SrToReattach == null)
                return;

            UpdateSelectedDevices();
            OnPageUpdated();
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (var row in dataGridView.Rows)
            {
                var deviceRow = row as FCDeviceRow;
                if (deviceRow != null && deviceRow.Cells.Count > 0)
                    deviceRow.Cells[colCheck.Index].Value = true;
            }

            UpdateSelectedDevices();
            OnPageUpdated();
        }

        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            foreach (var row in dataGridView.Rows)
            {
                var deviceRow = row as FCDeviceRow;
                if (deviceRow != null && deviceRow.Cells.Count > 0)
                    deviceRow.Cells[colCheck.Index].Value = false;
            }

            UpdateSelectedDevices();
            OnPageUpdated();
        }

        #endregion

        private void UpdateSelectedDevices()
        {
            if (SrWizardType.SrToReattach == null)
            {
                //when creating a new SR the checkbox column is visible
                _selectedDevices = (from DataGridViewRow row in dataGridView.Rows
                                    let deviceRow = row as FCDeviceRow
                                    where deviceRow != null
                                          && deviceRow.Cells.Count > 0
                                          && (bool)(deviceRow.Cells[colCheck.Index].Value)
                                    select deviceRow.Device).ToList();
            }
            else
            {
                //when reattaching SR the checkbox column is hidden
                _selectedDevices = (from DataGridViewRow row in dataGridView.Rows
                                    let deviceRow = row as FCDeviceRow
                                    where deviceRow != null && deviceRow.Selected
                                    select deviceRow.Device).ToList();
            }
        }

        private void UpdateSelectionButtons()
        {
            if (buttonSelectAll.Visible)
                buttonSelectAll.Enabled = _selectedDevices.Count < FCDevices.Count;

            if (buttonClearAll.Visible)
                buttonClearAll.Enabled = _selectedDevices.Count > 0;
        }

        public bool FiberChannelScan(IWin32Window owner, IXenConnection connection, out List<FibreChannelDevice> devices)
        {
            devices = new List<FibreChannelDevice>();

            Host coordinator = Helpers.GetCoordinator(connection);
            if (coordinator == null)
                return false;

            var action = new FibreChannelProbeAction(coordinator, SrType);
            using (var  dialog = new ActionProgressDialog(action, ProgressBarStyle.Marquee))
                dialog.ShowDialog(owner); //Will block until dialog closes, action completed

            if (!action.Succeeded)
                return false;

            devices = action.FibreChannelDevices;
            if (devices != null && devices.Count > 0)
                return true;

            using (var dlg = new WarningDialog(Messages.FIBRECHANNEL_NO_RESULTS))
                dlg.ShowDialog();

            return false;
        }

        #region Accessors

        public List<FibreChannelDevice> FCDevices { private get; set; }

        private SrWizardType _srWizardType;
        public SrWizardType SrWizardType
        {
            private get
            {
                return _srWizardType;
            }
            set
            {
                _srWizardType = value;

                bool creatingNew = _srWizardType.SrToReattach == null;
                
                colCheck.Visible = creatingNew;
                dataGridView.MultiSelect = creatingNew;
                buttonSelectAll.Visible = creatingNew;
                buttonClearAll.Visible = creatingNew;
                labelCreate.Visible = creatingNew;
                labelReattach.Visible = !creatingNew;
            }
        }

        public List<FibreChannelDescriptor> SrDescriptors { get; private set; }

        /// <summary>
        /// min size
        /// </summary>
        public long SRSize
        {
            get
            {
                long size = long.MaxValue;
                foreach (var srDescriptor in SrDescriptors)
                {
                    if (srDescriptor.Device.Size < size)
                        size = srDescriptor.Device.Size;
                }
                return size;
            }
        }

        #endregion
        
        #region Nested classes

        private class FCDeviceRow : DataGridViewRow
        {
            public FibreChannelDevice Device { get; private set; }

            public FCDeviceRow(FibreChannelDevice device, bool showNicColumn)
            {
                Device = device;

                string id = string.IsNullOrEmpty(device.SCSIid) ? device.Path : device.SCSIid;
                string details = String.Format("{0}:{1}:{2}:{3}", device.adapter, device.channel, device.id, device.lun);

                Cells.AddRange(new DataGridViewCheckBoxCell{ThreeState = false, Value = false},
                    new DataGridViewTextBoxCell { Value = Util.DiskSizeString(device.Size) },
                    new DataGridViewTextBoxCell { Value = device.Serial },
                    new DataGridViewTextBoxCell { Value = id },
                    new DataGridViewTextBoxCell { Value = details });

                if (showNicColumn)
                    Cells.Add(new DataGridViewTextBoxCell {Value = device.eth});
            }
        }

        private class VendorRow : DataGridViewRow
        {
            public VendorRow(string vendor)
            {
                Cells.AddRange(new DataGridViewCheckBoxCellVendor(),
                    new DataGridViewTextBoxCell { Value = vendor },
                    new DataGridViewTextBoxCell(),
                    new DataGridViewTextBoxCell(),
                    new DataGridViewTextBoxCell());
            }

            private class DataGridViewCheckBoxCellVendor : DataGridViewCheckBoxCell
            {
                protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
                {
                    using (var normalBrush = new SolidBrush(OwningRow.DefaultCellStyle.BackColor))
                    using (var selectedBrush = new SolidBrush(OwningRow.DefaultCellStyle.SelectionBackColor))
                    {
                        graphics.FillRectangle(
                            (elementState & DataGridViewElementStates.Selected) != 0 ? selectedBrush : normalBrush,
                            cellBounds.X, cellBounds.Y, cellBounds.Width, cellBounds.Height);
                    }
                }
            }
        }

        private class LVMoHBAWarningDialogLauncher
        {
            private readonly Dictionary<FibreChannelDescriptor, FibreChannelDescriptor> inputSrDescriptors;
            private readonly IWin32Window owner;
            private readonly SR.SRTypes requestedSrType;
            private readonly IXenConnection _connection;
            
            public List<FibreChannelDescriptor> SrDescriptors { get; }
            public bool Cancelled { get; private set; }

            public LVMoHBAWarningDialogLauncher(IXenConnection conn, IWin32Window owner, Dictionary<FibreChannelDescriptor, FibreChannelDescriptor> srDescriptors, SR.SRTypes requestedSrType)
            {
                _connection = conn;
                this.owner = owner;
                inputSrDescriptors = srDescriptors;
                this.requestedSrType = requestedSrType;
                SrDescriptors = new List<FibreChannelDescriptor>();
            }

            private LVMoHBAWarningDialog.UserSelectedOption GetSelectedOption(FibreChannelDescriptor descriptor, int remainingCount, bool foundExistingSr, 
                SR.SRTypes existingSrType, out bool repeatForRemainingLUNs)
            {
                var deviceDetails = string.Format(Messages.LVMOHBA_WARNING_DIALOG_LUN_DETAILS,
                    descriptor.Device.Vendor,
                    descriptor.Device.Serial,
                    string.IsNullOrEmpty(descriptor.Device.SCSIid) ? descriptor.Device.Path : descriptor.Device.SCSIid,
                    Util.DiskSizeString(descriptor.Device.Size));

                using (var dialog = new LVMoHBAWarningDialog(_connection, deviceDetails, remainingCount, foundExistingSr, existingSrType, requestedSrType))
                {
                    dialog.ShowDialog(owner);
                    repeatForRemainingLUNs = dialog.RepeatForRemainingLUNs;
                    return dialog.SelectedOption;
                }
            }

            public void ShowWarnings()
            {
                // process LUNs where existing SRs have been found
                bool repeatForRemainingLUNs = false;
                var selectedOption = LVMoHBAWarningDialog.UserSelectedOption.Cancel;
                var descriptors = inputSrDescriptors.Keys.Where(d => inputSrDescriptors[d] != null).ToList();

                foreach (var descriptor in descriptors)
                {
                    if (!repeatForRemainingLUNs)
                    {
                        var remainingCount = descriptors.Count - 1 - descriptors.IndexOf(descriptor);
                        var existingSrType = inputSrDescriptors[descriptor].SrType;

                        selectedOption = GetSelectedOption(descriptor, remainingCount, true, existingSrType, out repeatForRemainingLUNs);
                    }
                    ProcessSelectedOption(selectedOption, descriptor);
                }

                if (Cancelled)
                    return;

                // process LUNs where no existing have been found
                repeatForRemainingLUNs = false;
                selectedOption = LVMoHBAWarningDialog.UserSelectedOption.Cancel;
                descriptors = inputSrDescriptors.Keys.Where(d => inputSrDescriptors[d] == null).ToList();
                
                foreach (var descriptor in descriptors)
                {
                    if (!repeatForRemainingLUNs)
                    {
                        var remainingCount = descriptors.Count - 1 - descriptors.IndexOf(descriptor);
                        var existingSrType = descriptor.SrType;
                        selectedOption = GetSelectedOption(descriptor, remainingCount, false, existingSrType, out repeatForRemainingLUNs);
                    }
                    ProcessSelectedOption(selectedOption, descriptor);
                }
            }

            private void ProcessSelectedOption(LVMoHBAWarningDialog.UserSelectedOption selectedOption, FibreChannelDescriptor descriptor)
            {
                switch (selectedOption)
                {
                    case LVMoHBAWarningDialog.UserSelectedOption.Format:
                        descriptor.UUID = null;
                        SrDescriptors.Add(descriptor); // descriptor of requested SR
                        break;
                    case LVMoHBAWarningDialog.UserSelectedOption.Reattach:
                        SrDescriptors.Add(inputSrDescriptors[descriptor]); // value = descriptor of existing SR
                        break;
                    case LVMoHBAWarningDialog.UserSelectedOption.Cancel:
                        SrDescriptors.Clear();
                        Cancelled = true;
                        return;
                }
            }
        }

        #endregion
    }
}
