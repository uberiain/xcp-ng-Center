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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using XenAdmin.Actions;
using XenAdmin.Alerts;
using XenAdmin.Controls;
using XenAdmin.Core;
using XenAdmin.Wizards.NewPolicyWizard;
using XenAPI;


namespace XenAdmin.Dialogs.ScheduledSnapshots
{
    public partial class ScheduledSnapshotsDialog: XenDialogBase
    {
        private readonly Pool Pool;
        private bool updatingPolicies;

        public ScheduledSnapshotsDialog(Pool pool)
            : base(pool.Connection)
        {
            Pool = pool;
            InitializeComponent();
            ShowHideRunHistoryButton.Text = Messages.SHOW_RUN_HISTORY;
            ShowHideRunHistoryButton.Image = Images.StaticImages.PDChevronDown;

            ColumnExpand.DefaultCellStyle.NullValue = null;
            comboBoxTimeSpan.SelectedIndex = 0;
            dataGridViewRunHistory.Columns[2].ValueType = typeof(DateTime);
            dataGridViewRunHistory.Columns[2].DefaultCellStyle.Format = Messages.DATEFORMAT_DMY_HM;
            tableLayoutPanel2.Visible = false;
            RefreshPoolTitle(pool);
            RefreshButtons();
        }

        public ScheduledSnapshotsDialog() { }

        private PolicyRow SelectedVmssRow
        {
            get
            {
                if (dataGridViewPolicies.SelectedRows.Count > 0)
                    return dataGridViewPolicies.SelectedRows[0] as PolicyRow;

                return null;
            }
        }

        private int RunHistoryTimeSpan
        {
            get
            {
                switch (comboBoxTimeSpan.SelectedIndex)
                {
                    case 0: //top 10 messages (default)
                        return 0;
                    case 1:
                        return 24; //messages from past 24 Hrs
                    case 2:
                        return 7 * 24; //messages from last 7 days
                    default:
                        return 0;
                }
            }
        }

        private class PolicyRow : DataGridViewRow
        {
            private readonly DataGridViewTextBoxCell _name = new DataGridViewTextBoxCell();
            private readonly DataGridViewTextBoxCell _numVMs = new DataGridViewTextBoxCell();
            private readonly DataGridViewTextBoxCell _nextRunClientLocal = new DataGridViewTextBoxCell();
            private readonly DataGridViewTextBoxCell _nextRunServerLocal = new DataGridViewTextBoxCell();
            private readonly DataGridViewTextBoxCell _status = new DataGridViewTextBoxCell();
            private readonly DataGridViewTextAndImageCell _lastResult = new DataGridViewTextAndImageCell();

            private readonly ServerTimeInfo? serverTimeInfo;

            public bool IsBusy { get; set; }
            public VMSS Policy { get; }
            public List<XenAPI.Message> AlertMessages { get; }
            public string PolicyName { get; private set; }
            public string PolicyStatus { get; private set; }
            public int PolicyVmCount { get; private set; }
            public DateTime? NextRunLocalToClient { get; private set; }
            public DateTime? NextRunLocalToServer { get; private set; }
            public string PolicyLastResult { get; private set; }
            private Bitmap PolicyLastResultImage { get; set; }


            public PolicyRow(VMSS policy, List<XenAPI.Message> alertMessages, ServerTimeInfo? serverTimeInfo)
            {
                Cells.AddRange(_name, _status, _numVMs, _nextRunClientLocal, _nextRunServerLocal, _lastResult);
                Policy = policy;
                AlertMessages = alertMessages;
                this.serverTimeInfo = serverTimeInfo;
                RefreshRow();
            }

            private void RefreshRow()
            {
                PolicyName = Policy.Name();
                PolicyVmCount = Policy.VMs.FindAll(vm => Policy.Connection.Resolve(vm).IsRealVm()).Count;
                PolicyStatus = Policy.enabled ? Messages.ENABLED : Messages.DISABLED;

                //the policy is in server's local time zone
                if (serverTimeInfo.HasValue)
                    NextRunLocalToServer = Policy.GetNextRunTime(serverTimeInfo.Value.ServerLocalTime);

                if (serverTimeInfo.HasValue && NextRunLocalToServer.HasValue)
                    NextRunLocalToClient = HelpersGUI.RoundToNearestQuarter(NextRunLocalToServer.Value + serverTimeInfo.Value.ServerClientTimeZoneDiff);

                if (AlertMessages.Count > 0)
                {
                    var paType = PolicyAlert.FromPriority(AlertMessages[0].priority);
                    PolicyLastResult = paType.GetString();
                    PolicyLastResultImage = paType.GetImage();
                }
                else
                {
                    PolicyLastResult = Messages.NOT_YET_RUN;
                    PolicyLastResultImage = null;
                }

                _name.Value = PolicyName;
                _numVMs.Value = PolicyVmCount;
                _status.Value = PolicyStatus;
                _lastResult.Value = PolicyLastResult;
                _lastResult.Image = PolicyLastResultImage;

                _nextRunClientLocal.Value = NextRunLocalToClient.HasValue
                    ? HelpersGUI.DateTimeToString(NextRunLocalToClient.Value, Messages.DATEFORMAT_DMY_HM, true)
                    : Messages.VMSS_HOST_NOT_LIVE;

                _nextRunServerLocal.Value = NextRunLocalToServer.HasValue
                    ? HelpersGUI.DateTimeToString(NextRunLocalToServer.Value, Messages.DATEFORMAT_DMY_HM, true)
                    : Messages.VMSS_HOST_NOT_LIVE;
            }
        }

        private void RefreshPoolTitle(Pool pool)
        {
            int protectedVMs = 0;
            int realVMs = 0;

            foreach (var vm in pool.Connection.Cache.VMs)
            {
                if (vm.IsRealVm() && vm.Show(Properties.Settings.Default.ShowHiddenVMs))
                {
                    realVMs++;
                    if (vm.Connection.Resolve(vm.snapshot_schedule) != null)
                        protectedVMs++;
                }
            }

            labelPolicyTitle.Text = string.Format(Helpers.IsPool(pool.Connection)
                                                        ? Messages.VMSS_SCHEDULED_SNAPSHOTS_DEFINED_FOR_POOL
                                                        : Messages.VMSS_SCHEDULED_SNAPSHOTS_DEFINED_FOR_SERVER,
                                                    pool.Name().Ellipsise(45), protectedVMs, realVMs);
        }

        private void VMSSCollectionChanged(object sender, EventArgs e)
        {
            LoadPolicies();
        }

        private void LoadPolicies()
        {
            var coordinator = Helpers.GetCoordinator(Pool);
            var action = new GetServerLocalTimeAction(coordinator);
            action.Completed += action_Completed;
            action.RunAsync();
        }

        private void action_Completed(ActionBase sender)
        {
            sender.Completed -= action_Completed;

            if (!(sender is GetServerLocalTimeAction action) || !action.Succeeded)
                return;
            
            Program.Invoke(Program.MainWindow, () =>
            {
                try
                {
                    panelLoading.Visible = false;
                    updatingPolicies = true;

                    var selectedPolicyUuids = (from PolicyRow row in dataGridViewPolicies.SelectedRows
                        select row.Policy.uuid).ToList();

                    var schedules = connection.Cache.VMSSs;
                    var messages = Pool.Connection.Cache.Messages;

                    var allVmssMessages = (from XenAPI.Message msg in messages
                            where msg.cls == cls.VMSS
                            group msg by msg.obj_uuid
                            into g
                            let gOrdered = g.OrderByDescending(m => m.timestamp).ToList()
                            select new {PolicyUuid = g.Key, PolicyMessages = gOrdered})
                        .ToDictionary(x => x.PolicyUuid, x => x.PolicyMessages);

                    var filteredVmssMessages = new Dictionary<VMSS, List<XenAPI.Message>>();
                    foreach (var schedule in schedules)
                    {
                        List<XenAPI.Message> value;
                        if (!allVmssMessages.TryGetValue(schedule.uuid, out value))
                            value = new List<XenAPI.Message>();

                        filteredVmssMessages[schedule] = value;
                    }

                    var rowList = from kvp in filteredVmssMessages
                        select new PolicyRow(kvp.Key, kvp.Value, action.ServerTimeInfo);

                    Func<PolicyRow, object> comparer = p => p.PolicyName;
                    if (dataGridViewPolicies.SortedColumn != null)
                    {
                        if (dataGridViewPolicies.SortedColumn.Index == ColumnName.Index)
                            comparer = p => p.PolicyName;
                        else if (dataGridViewPolicies.SortedColumn.Index == ColumnEnabled.Index)
                            comparer = p => p.PolicyStatus;
                        else if (dataGridViewPolicies.SortedColumn.Index == ColumnVMs.Index)
                            comparer = p => p.PolicyVmCount;
                        else if (dataGridViewPolicies.SortedColumn.Index == ColumnNextSnapshotTime.Index)
                            comparer = p => p.NextRunLocalToClient;
                        else if (dataGridViewPolicies.SortedColumn.Index == ColumnCorrespondingServerTime.Index)
                            comparer = p => p.NextRunLocalToServer;
                        else if (dataGridViewPolicies.SortedColumn.Index == ColumnLastResult.Index)
                            comparer = p => p.PolicyLastResult;
                    }

                    var rows = dataGridViewPolicies.SortOrder == SortOrder.Descending
                        ? rowList.OrderByDescending(comparer)
                        : rowList.OrderBy(comparer);

                    dataGridViewPolicies.SuspendLayout();
                    dataGridViewPolicies.Rows.Clear();
                    dataGridViewPolicies.Rows.AddRange(rows.Cast<DataGridViewRow>().ToArray());

                    foreach (PolicyRow row in dataGridViewPolicies.Rows)
                        row.Selected = selectedPolicyUuids.Contains(row.Policy.uuid);

                    if (dataGridViewPolicies.SelectedRows.Count == 0 && dataGridViewPolicies.Rows.Count > 0)
                        dataGridViewPolicies.Rows[0].Selected = true;
                }
                finally
                {
                    dataGridViewPolicies.ResumeLayout();
                    updatingPolicies = false;

                    RefreshPoolTitle(Pool);
                    RefreshButtons();
                    RefreshHistoryLabel();
                    RefreshHistoryGrid();
                }
            });
        }

        private void VMProtectionPoliciesDialog_Load(object sender, EventArgs e)
        {
            panelLoading.Visible = true;
            Pool.Connection.Cache.RegisterBatchCollectionChanged<VMSS>(VMSSCollectionChanged);
            LoadPolicies();
        }

        private void VMProtectionPoliciesDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            Pool.Connection.Cache.DeregisterBatchCollectionChanged<VMSS>(VMSSCollectionChanged);
        }

        private void dataGridViewPolicies_SelectionChanged(object sender, EventArgs e)
        {
            if (updatingPolicies)
                return;

            RefreshButtons();
            RefreshHistoryLabel();
            RefreshHistoryGrid();
        }

        private void RefreshButtons()
        {
            if (dataGridViewPolicies.SelectedRows.Count == 1)
            {
                var row = SelectedVmssRow;
                buttonEnable.Text = row.Policy.enabled ? Messages.DISABLE : Messages.ENABLE;
                buttonEnable.Enabled = !row.IsBusy && (row.Policy.VMs.Count != 0 || row.Policy.enabled);
                buttonProperties.Enabled = !row.IsBusy;
                buttonRunNow.Enabled = !row.IsBusy && row.Policy.enabled && row.Policy.VMs.Count != 0;
                comboBoxTimeSpan.Enabled = !row.IsBusy;
            }
            else
            {
                buttonProperties.Enabled = buttonEnable.Enabled = buttonRunNow.Enabled =
                    comboBoxTimeSpan.Enabled = false;
            }

            buttonDelete.Enabled = (from PolicyRow row in dataGridViewPolicies.SelectedRows where !row.IsBusy select row).Any();
        }

        #region Button event handlers

        private void buttonNew_Click(object sender, EventArgs e)
        {
            new NewPolicyWizard(Pool).Show(this);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonEnable_Click(object sender, EventArgs e)
        {
            var row = SelectedVmssRow;
            if (row != null)
            {
                row.IsBusy = true;
                RefreshButtons();
                new ChangePolicyEnabledAction(row.Policy).RunAsync();
            }
        }

        private void buttonRunNow_Click(object sender, EventArgs e)
        {
            var row = SelectedVmssRow;
            if (row != null)
            {
                row.IsBusy = true;
                RefreshButtons();
                new RunPolicyNowAction(row.Policy).RunAsync();
            }
        }

        private void buttonProperties_Click(object sender, EventArgs e)
        {
            var row = SelectedVmssRow;
            if (row != null)
            {
                using (PropertiesDialog propertiesDialog = new PropertiesDialog(row.Policy))
                    propertiesDialog.ShowDialog(this);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            var selectedPolicies = new List<VMSS>();
            int numberOfProtectedVMs = 0;

            foreach (PolicyRow row in dataGridViewPolicies.SelectedRows)
            {
                selectedPolicies.Add(row.Policy);
                numberOfProtectedVMs += row.Policy.VMs.Count;
            }

            string text = selectedPolicies.Count == 1
                ? String.Format(numberOfProtectedVMs == 0
                    ? Messages.CONFIRM_DELETE_POLICY_0
                    : Messages.CONFIRM_DELETE_POLICY, selectedPolicies[0].Name(), numberOfProtectedVMs)
                : string.Format(numberOfProtectedVMs == 0
                    ? Messages.CONFIRM_DELETE_POLICIES_0
                    : Messages.CONFIRM_DELETE_POLICIES, numberOfProtectedVMs);

            using (var dlg = new WarningDialog(text, ThreeButtonDialog.ButtonYes, ThreeButtonDialog.ButtonNo)
                {WindowTitle = Messages.DELETE_VMSS_TITLE})
            {
                if (dlg.ShowDialog(this) == DialogResult.Yes)
                {
                    foreach (PolicyRow row in dataGridViewPolicies.SelectedRows)
                        row.IsBusy = true;

                    RefreshButtons();
                    new DestroyPolicyAction(Pool.Connection, selectedPolicies).RunAsync();
                }
            }
        }

        private void ShowHideRunHistoryButton_Click(object sender, EventArgs e)
        {
            tableLayoutPanel2.Visible = !tableLayoutPanel2.Visible;

            if (tableLayoutPanel2.Visible)
            {
                ShowHideRunHistoryButton.Text = Messages.HIDE_RUN_HISTORY;
                ShowHideRunHistoryButton.Image = Images.StaticImages.PDChevronUp;
            }
            else
            {
                ShowHideRunHistoryButton.Text = Messages.SHOW_RUN_HISTORY;
                ShowHideRunHistoryButton.Image = Images.StaticImages.PDChevronDown;
            }
        }

        #endregion

        internal override string HelpName => "VMSnapshotSchedulesDialog";

        private void dataGridViewRunHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridViewRunHistory.RowCount)
            {
                HistoryRow row = (HistoryRow)dataGridViewRunHistory.Rows[e.RowIndex];
                if (row.Alert.Type == PolicyAlertType.Error)
                {
                    row.Expanded = !row.Expanded;
                    row.RefreshRow();
                }
            }
        }

        private void dataGridViewRunHistory_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != ColumnImage.Index)
            {
                if (0 <= e.ColumnIndex && e.ColumnIndex < dataGridViewRunHistory.ColumnCount &&
                    dataGridViewRunHistory.Columns[e.ColumnIndex].SortMode != DataGridViewColumnSortMode.NotSortable)
                    ColumnImage.HeaderCell.SortGlyphDirection = SortOrder.None;
                return;
            }

            if (ColumnImage.HeaderCell.SortGlyphDirection == SortOrder.Ascending)
            {
                dataGridViewRunHistory.Sort(new HistoryRowSorter(ListSortDirection.Descending));
                ColumnImage.HeaderCell.SortGlyphDirection = SortOrder.Descending;
            }
            else
            {
                dataGridViewRunHistory.Sort(new HistoryRowSorter(ListSortDirection.Ascending));
                ColumnImage.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }
        }

        private void comboBoxTimeSpan_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshHistoryGrid();
        }

        private void RefreshHistoryGrid()
        {
            try
            {
                dataGridViewRunHistory.SuspendLayout();
                dataGridViewRunHistory.Rows.Clear();

                var row = SelectedVmssRow;
                if (row == null)
                    return;

                var messages = row.AlertMessages;

                int hoursFromNow = RunHistoryTimeSpan;

                DateTime currentTime = DateTime.Now;
                DateTime offset = currentTime.Add(new TimeSpan(-hoursFromNow, 0, 0));

                if (hoursFromNow == 0)
                {
                    for (int i = 0; i < 10 && i < messages.Count; i++)
                    {
                        var alert = new PolicyAlert(messages[i]);
                        dataGridViewRunHistory.Rows.Add(new HistoryRow(alert));
                    }
                }
                else
                {
                    foreach (var msg in messages)
                    {
                        if (msg.TimestampLocal() >= offset)
                        {
                            var alert = new PolicyAlert(msg);
                            dataGridViewRunHistory.Rows.Add(new HistoryRow(alert));
                        }
                        else
                            break;
                    }
                }
            }
            finally
            {
                dataGridViewRunHistory.ResumeLayout();
            }
        }

        private void RefreshHistoryLabel()
        {
            var row = SelectedVmssRow;
            if (row == null)
            {
                labelHistory.Text = "";
                return;
            }

            string name = row.Policy.Name();

            // ellipsise if necessary
            using (Graphics g = labelHistory.CreateGraphics())
            {
                int maxWidth = labelShow.Left - labelHistory.Left;
                int availableWidth = maxWidth - (int)g.MeasureString(string.Format(Messages.HISTORY_FOR_POLICY, ""), labelHistory.Font).Width;
                name = name.Ellipsise(new Rectangle(0, 0, availableWidth, labelHistory.Height), labelHistory.Font);
            }
            labelHistory.Text = string.Format(Messages.HISTORY_FOR_POLICY, name);
        }

        private class HistoryRow : DataGridViewRow
        {
            private readonly DataGridViewImageCell _expand = new DataGridViewImageCell();
            private readonly DataGridViewImageCell _image = new DataGridViewImageCell();
            private readonly DataGridViewTextBoxCell _dateTime = new DataGridViewTextBoxCell();
            private readonly DataGridViewTextBoxCell _description = new DataGridViewTextBoxCell();
            public readonly PolicyAlert Alert;

            public HistoryRow(PolicyAlert alert)
            {
                Alert = alert;
                Cells.AddRange(_expand, _image, _dateTime, _description);
                RefreshRow();
            }

            [DefaultValue(false)]
            public bool Expanded { get; set; }

            public void RefreshRow()
            {
                if (Alert.Type == PolicyAlertType.Error)
                {
                    _expand.Value = Expanded
                        ? Images.StaticImages.expanded_triangle
                        : Images.StaticImages.contracted_triangle;

                    _description.Value = Expanded
                        ? $"{Alert.Title}\r\n\r\n{Alert.Description}"
                        : Alert.Title;
                }
                else
                {
                    _expand.Value = null;
                    _description.Value = Alert.Title;
                }

                _image.Value = Alert.Type.GetImage();
                _dateTime.Value = HelpersGUI.DateTimeToString(Alert.Time, Messages.DATEFORMAT_DMY_HM, true);
            }
        }

        private class HistoryRowSorter : System.Collections.IComparer
        {
            private readonly ListSortDirection _direction;

            public HistoryRowSorter(ListSortDirection direction)
            {
                _direction = direction;
            }

            public int Compare(object first, object second)
            {
                var row1 = first as HistoryRow;
                var row2 = second as HistoryRow;
                int result = 0;

                if (row1 != null && row2 != null)
                {
                    result = row1.Alert.Type.CompareTo(row2.Alert.Type);
                    if (result == 0)
                        result = Alert.CompareOnDate(row1.Alert, row2.Alert);
                }
                else if (row1 != null)
                    result = -1;
                else if (row2 != null)
                    result = 1;

                if (_direction == ListSortDirection.Descending)
                    return -1 * result;

                return result;
            }
        }
    }
}
