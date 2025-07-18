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
using System.Windows.Forms;
using XenAdmin.Actions.Wlb;
using XenAdmin.Core;
using XenAdmin.Wlb;
using XenAdmin.Dialogs;
using XenAdmin.Dialogs.Wlb;
using XenAPI;


namespace XenAdmin.Controls.Wlb
{
    public partial class WlbReportSubscriptionView : UserControl
    {
        #region Variables

        public event EventHandler OnChangeOK;
        public event EventHandler Close;
        public event EventHandler PoolConnectionLost;

        private WlbReportSubscription _subscription;

        #endregion

        #region Properties

        public Pool Pool { get; set; }

        public WlbReportSubscription ReportSubscription
        {
            get => _subscription ?? new WlbReportSubscription(string.Empty);
            set => _subscription = value;
        }

        #endregion

        #region Constructor

        public WlbReportSubscriptionView()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Methods

        private void BuildPanel()
        {
            // Subscription section
            if (_subscription != null)
            {
                // General
                pdSectionGeneral.ClearData();
                pdSectionGeneral.AddEntry(Messages.NAME, _subscription.Description);
                pdSectionGeneral.AddEntry(Messages.WLB_SUBSCRIPTION_EDITED_ON, _subscription.LastTouched == DateTime.MinValue ? String.Empty : HelpersGUI.DateTimeToString(_subscription.LastTouched.ToLocalTime(), Messages.DATEFORMAT_DMY_LONG, true));
                pdSectionGeneral.AddEntry(Messages.WLB_SUBSCRIPTION_EDITED_BY, _subscription.LastTouchedBy);
                pdSectionGeneral.Expand();

                // Report Parameters
                pdSectionParameters.ClearData();
                pdSectionParameters.AddEntry(Messages.WLB_SUBSCRIPTION_DATARANGE, GetSubscriptionRange());
                //pdSectionParameters.Expand();

                // Delivery Options
                pdSectionDelivery.ClearData();
                pdSectionDelivery.AddEntry(Messages.WLB_SUBSCRIPTION_TO, _subscription.EmailTo);
                pdSectionDelivery.AddEntry(Messages.WLB_SUBSCRIPTION_CC, _subscription.EmailCc);
                pdSectionDelivery.AddEntry(Messages.WLB_SUBSCRIPTION_BCC, _subscription.EmailBcc);
                pdSectionDelivery.AddEntry(Messages.WLB_SUBSCRIPTION_REPLYTO, _subscription.EmailReplyTo);
                pdSectionDelivery.AddEntry(Messages.WLB_SUBSCRIPTION_SUBJECT, _subscription.EmailSubject);
                pdSectionDelivery.AddEntry(Messages.WLB_SUBSCRIPTION_FORMAT, Enum.GetName(typeof(WlbReportSubscription.WlbReportRenderFormat), _subscription.ReportRenderFormat));
                pdSectionDelivery.AddEntry(Messages.WLB_SUBSCRIPTION_COMMENT, _subscription.EmailComment);
                //pdSectionDelivery.Expand();

                // Schedule Options
                pdSectionSchedule.ClearData();

                DateTime localRunTime;
                WlbScheduledTask.WlbTaskDaysOfWeek localDaysOfWeek;
                WlbScheduledTask.GetLocalTaskTimes(this._subscription.DaysOfWeek, this._subscription.RunTimeOfDay, out localDaysOfWeek, out localRunTime);
                if (_subscription.DaysOfWeek == WlbScheduledTask.WlbTaskDaysOfWeek.All)
                {
                    pdSectionSchedule.AddEntry(Messages.WLB_SUBSCRIPTION_DELIVERON, Messages.WLB_REPORT_EVERYDAY);
                }
                else
                {
                    pdSectionSchedule.AddEntry(Messages.WLB_SUBSCRIPTION_DELIVERON, WlbScheduledTask.DaysOfWeekL10N(localDaysOfWeek));
                }

                pdSectionSchedule.AddEntry(Messages.WLB_SUBSCRIPTION_RUNAT, HelpersGUI.DateTimeToString(localRunTime, Messages.DATEFORMAT_HMS, true));
                pdSectionSchedule.AddEntry(Messages.WLB_SUBSCRIPTION_STARTING, HelpersGUI.DateTimeToString(_subscription.EnableDate.ToLocalTime(), Messages.DATEFORMAT_DMY_LONG, true));
                pdSectionSchedule.AddEntry(Messages.WLB_SUBSCRIPTION_ENDING, (_subscription.DisableDate == DateTime.MinValue ? Messages.WLB_REPORT_NEVER : HelpersGUI.DateTimeToString(_subscription.DisableDate.ToLocalTime(), Messages.DATEFORMAT_DMY_LONG, true)));
                //pdSectionSchedule.Expand();

                // History
                pdSectionHistory.ClearData();
                pdSectionHistory.AddEntry(Messages.WLB_SUBSCRIPTION_LASTSENT, (_subscription.LastRun == DateTime.MinValue ? Messages.WLB_REPORT_NEVER : String.Format(Messages.WLB_SUBSCRIPTION_LASTSENT_TIME, HelpersGUI.DateTimeToString(_subscription.LastRun.ToLocalTime(), Messages.DATEFORMAT_DMY_LONG, true), HelpersGUI.DateTimeToString(_subscription.LastRun.ToLocalTime(), Messages.DATEFORMAT_HM, true))));
                //pdSectionHistory.Expand();
            }
        }

        private string GetSubscriptionRange()
        {
            int days = 0;
            if (_subscription.ReportParameters != null && _subscription.ReportParameters.ContainsKey("Start"))
            {
                int.TryParse(_subscription.ReportParameters["Start"], out days);
            }
            int weeks = (days - 1) / -7;
            string range = String.Empty;
            if (days != 0)
            {
                if (weeks < 2)
                {
                    range = Messages.WLB_REPORT_LASTWEEK;
                }
                if (weeks > 3)
                {
                    range = Messages.WLB_REPORT_LASTMONTH;
                }
                if (weeks >= 2 && weeks <= 3)
                {
                    range = string.Format(Messages.WLB_REPORT_LASTWEEKS, weeks.ToString());
                }
            }
            return range;
        }

        public void RefreshSubscriptionView(WlbReportSubscription subscription = null)
        {
            if (subscription != null)
                ReportSubscription = subscription;
            BuildPanel();
        }
        
        #endregion

        #region Private Methods

        private void DeleteReportSubscription(object sender, EventArgs e)
        {
            SendWlbConfigurationAction action = new SendWlbConfigurationAction(Pool, this._subscription.ToDictionary(), SendWlbConfigurationKind.DeleteReportSubscription);
            using (var dialog = new ActionProgressDialog(action, ProgressBarStyle.Blocks))
            {
                dialog.ShowCancel = true;
                dialog.ShowDialog(this);
            }

            if (action.Succeeded)
                OnChangeOK?.Invoke(this, e);
        }
        #endregion

        #region Event Handlers

        private void ReportSubscriptionView_Load(object sender, EventArgs e)
        {
            BuildPanel();
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (!Pool.Connection.IsConnected)
            {
                PoolConnectionLost?.Invoke(this, EventArgs.Empty);
                return;
            }

            using (var rpSubDialog = new WlbReportSubscriptionDialog(_subscription.ReportDisplayName, _subscription, Pool))
                if (rpSubDialog.ShowDialog() == DialogResult.OK)
                    OnChangeOK?.Invoke(this, e);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Pool.Connection.IsConnected)
            {
                PoolConnectionLost?.Invoke(this, EventArgs.Empty);
                return;
            }

            using (var dr = new WarningDialog(
                string.Format(Messages.WLB_REPORT_DELETE_SUBSCRIPTION_QUERY, _subscription.ReportDisplayName),
                ThreeButtonDialog.ButtonYes, ThreeButtonDialog.ButtonNo))
            {
                if (dr.ShowDialog(this) == DialogResult.Yes)
                    DeleteReportSubscription(this, e);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close?.Invoke(this, e);
        }

        #endregion
    }
}
    

