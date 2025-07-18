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
using System.Drawing;
using System.Windows.Forms;
using XenAdmin.Controls;
using XenAdmin.Network;
using XenAPI;
using System.Threading;
using XenAdmin.Core;
using IXenConnection = XenAdmin.Network.IXenConnection;


namespace XenAdmin.Wizards.GenericPages
{
    public partial class RBACWarningPage : XenTabPage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<IXenConnection, List<WizardRbacCheck>> checksPerConnectionDict;
        private Thread bgThread;
        private bool refreshPage;
        private bool finished;
        private bool blockProgress;

        public RBACWarningPage(string description) 
            : this()
        {
            labelDescription.Text = description;
        }

        public RBACWarningPage()
        {
            InitializeComponent();
            checksPerConnectionDict = new Dictionary<IXenConnection, List<WizardRbacCheck>>();
        }

        public override string HelpID => "RBAC";

        public override string Text => Messages.RBAC_WARNING_PAGE_TEXT_TITLE;

        public override string PageTitle => Messages.RBAC_WARNING_PAGE_TEXT_TITLE;

        #region AddApiMethodsCheck and overloads

        /// <summary>
        /// Clears the existing permission checks and adds the ones specified.
        /// </summary>
        public void SetPermissionChecks(IEnumerable<IXenConnection> connectionsToCheck, params WizardRbacCheck[] checks)
        {
            ClearPermissionChecks();

            foreach (var connection in connectionsToCheck)
                AddPermissionChecks(connection, checks);
        }

        /// <summary>
        /// Clears the existing permission checks and adds the ones specified.
        /// </summary>
        public void SetPermissionChecks(IXenConnection connectionToCheck, params WizardRbacCheck[] checks)
        {
            SetPermissionChecks(new List<IXenConnection> {connectionToCheck}, checks);
        }

        /// <summary>
        /// Adds the permission checks specified to the existing ones (without clearing them first).
        /// </summary>
        public void AddPermissionChecks(IXenConnection connection, params WizardRbacCheck[] checks)
        {
            if (!checksPerConnectionDict.ContainsKey(connection))
                checksPerConnectionDict.Add(connection, new List<WizardRbacCheck>());

            checksPerConnectionDict[connection].AddRange(checks);
        }

        private void ClearPermissionChecks()
		{
            DeregisterConnectionEvents();
            checksPerConnectionDict.Clear();
		}

        #endregion

        protected override void PageLoadedCore(PageLoadedDirection direction)
        {
            RegisterConnectionEvents();
            RefreshPage();
        }

        void Connection_ConnectionResult(object sender, ConnectionResultEventArgs e)
        {
            if (e.Connected)
                Program.Invoke(this, RefreshPage);
        }

        protected override void PageLeaveCore(PageLoadedDirection direction, ref bool cancel)
        {
            DeregisterConnectionEvents();
        }

        public override void PageCancelled(ref bool cancel)
        {
            DeregisterConnectionEvents();
        }

        public void RefreshPage()
        {
            if (bgThread == null || !bgThread.IsAlive)
            {
                bgThread = new Thread(RetrieveRBACWarnings);
                bgThread.IsBackground = true;
                bgThread.Start();
            }
            else
            {
                refreshPage = true;
            }
        }

        public override bool EnableNext()
        {
            return !blockProgress && finished;
        }

        private void RegisterConnectionEvents()
        {
            foreach (var connectionChecks in checksPerConnectionDict)
            {
                IXenConnection connection = connectionChecks.Key;
                connection.ConnectionResult += Connection_ConnectionResult;
            }
        }

        private void DeregisterConnectionEvents()
        {
            foreach (var connectionChecks in checksPerConnectionDict)
            {
                IXenConnection connection = connectionChecks.Key;
                connection.ConnectionResult -= Connection_ConnectionResult;
            }
        }

        private PermissionCheckResult RunPermissionChecks(IXenConnection connection, List<WizardRbacCheck> permissionChecks,
            out List<WizardRbacCheck> errors, out List<WizardRbacCheck> warnings)
        {
            PermissionCheckResult checkResult = PermissionCheckResult.OK;

            errors = new List<WizardRbacCheck>();
            warnings = new List<WizardRbacCheck>();

            foreach (WizardRbacCheck wpc in permissionChecks)
            {
                List<Role> rolesAbleToComplete = wpc.GetValidRoles(connection);
                List<Role> subjectRoles = connection.Session.Roles;

                if (subjectRoles.Find(rolesAbleToComplete.Contains) != null)
                    continue;

                if (wpc.Blocking)
                {
                    errors.Add(wpc);
                    checkResult = PermissionCheckResult.Failed;
                }
                else
                {
                    warnings.Add(wpc);
                    if (checkResult == PermissionCheckResult.OK)
                        checkResult = PermissionCheckResult.Warning;
                }
            }

            return checkResult;
        }

        private void RetrieveRBACWarnings()
        {
            SetUpdating();
            foreach (var connectionChecks in checksPerConnectionDict)
            {
                IXenConnection connection = connectionChecks.Key;
                PermissionCheckHeaderRow headerRow = AddHeaderRow(connection);
                PermissionCheckResult checkResult = PermissionCheckResult.OK;

                if (connection.Session.IsLocalSuperuser || connectionChecks.Value.Count == 0)
                {
                    SetNoWarnings();
                }
                else
                {
                    List<WizardRbacCheck> errors;
                    List<WizardRbacCheck> warnings;
                    checkResult = RunPermissionChecks(connection, connectionChecks.Value, out errors, out warnings);
                    switch (checkResult)
                    {
                        case PermissionCheckResult.OK:
                            SetNoWarnings();
                            break;
                        case PermissionCheckResult.Warning:
                            AddWarnings(connection, warnings);
                            break;
                        case PermissionCheckResult.Failed:
                            AddErrors(connection, errors);
                            break;
                    }
                }
                UpdateHeaderRow(headerRow, checkResult);
            }
            FinishedUpdating();
        }

        private void AddErrors(IXenConnection connection, List<WizardRbacCheck> errors)
        {
            Program.AssertOffEventThread();

            if (connection.Session.IsLocalSuperuser)
            {
                // We should not be here.
                log.Warn("A local root account is being blocked access");
            }

            List<Role> roleList = connection.Session.Roles;
            roleList.Sort();

            foreach (WizardRbacCheck wizardPermissionCheck in errors)
            {
                // the string is a format string that needs to take the current role (we use the subject they were authorized under which could be a group or user)
                var description = string.Format(wizardPermissionCheck.WarningMessage, roleList[0].FriendlyName());
                AddDetailsRow(description, PermissionCheckResult.Failed);
            }

            Program.Invoke(this, () => blockProgress = true);
        }

        private void FinishedUpdating()
        {
            if (refreshPage)
            {
                // If we have received a request to refresh while updating then we run the logic again
                refreshPage = false;
                RetrieveRBACWarnings();
                return;
            }
            finished = true;
            Program.Invoke(this, delegate
            {
                labelClickNext.Visible = !blockProgress;
                OnPageUpdated();
            });
        }

        private void AddWarnings(IXenConnection connection, List<WizardRbacCheck> warnings)
        {
            List<Role> roleList = connection.Session.Roles;
            roleList.Sort();

            foreach (WizardRbacCheck wizardPermissionCheck in warnings)
            {
                wizardPermissionCheck.WarningAction?.Invoke();

                // the string is a format string that needs to take the current role (we use the subject they were authorised under which could be a group or user)
                string description = String.Format(wizardPermissionCheck.WarningMessage, roleList[0].FriendlyName());
                AddDetailsRow(description, PermissionCheckResult.Warning);
            }
        }

        private void SetNoWarnings()
        {
            AddDetailsRow(Messages.RBAC_NO_WIZARD_LIMITS, PermissionCheckResult.OK);
        }

        private void SetUpdating()
        {
            Program.AssertOffEventThread();

            Program.Invoke(this, delegate
            {
                blockProgress = false;
                finished = false;
                OnPageUpdated();
                labelClickNext.Visible = false;
                dataGridViewEx1.Rows.Clear();
            });
        }

        private PermissionCheckHeaderRow AddHeaderRow(IXenConnection connection)
        {
            Program.AssertOffEventThread();

            string text = string.Format(Messages.RBAC_WARNING_PAGE_HEADER_ROW_DESC, connection.Session.UserFriendlyName(),
                                        connection.Session.FriendlyRoleDescription(), connection.FriendlyName);
            PermissionCheckHeaderRow headerRow = new PermissionCheckHeaderRow(text);
            Program.Invoke(this, delegate
                                     {
                                         headerRow.SetPermissionCheckInProgress(true);
                                         dataGridViewEx1.Rows.Add(headerRow);
                                     });
            return headerRow;
        }

        private void UpdateHeaderRow(PermissionCheckHeaderRow headerRow, PermissionCheckResult checkResult)
        {
            Program.AssertOffEventThread();
            Program.Invoke(this, delegate
                                     {
                                         headerRow.SetPermissionCheckInProgress(false);
                                         headerRow.UpdateDescription(checkResult);
                                     });
        }

        private void AddDetailsRow(string desc, PermissionCheckResult checkResult)
        {
            Program.AssertOffEventThread();
            Program.Invoke(this, delegate
                                     {
                                         PermissionCheckDetailsRow detailsRow =
                                             new PermissionCheckDetailsRow(desc, checkResult);
                                         dataGridViewEx1.Rows.Add(detailsRow);
                                     });
        }


        internal enum PermissionCheckResult { OK, Warning, Failed }

        
        private abstract class PermissionCheckGridRow : DataGridViewRow
        {
            protected DataGridViewImageCell iconCell = new DataGridViewImageCell();
            protected DataGridViewTextBoxCell descriptionCell = new DataGridViewTextBoxCell();
            protected PermissionCheckGridRow()
            {
                Cells.Add(iconCell);
                Cells.Add(descriptionCell);
            }
        }

        private class PermissionCheckHeaderRow : PermissionCheckGridRow
        {
            private string description;

            public PermissionCheckHeaderRow(string desc)
            {
                iconCell.Value = new Bitmap(1, 1);
                description = desc;
                descriptionCell.Value = desc;
                descriptionCell.Style.Font = new Font(Program.DefaultFont, FontStyle.Bold);
            }

            public void UpdateDescription(PermissionCheckResult permissionCheckResult)
            {
                string result = permissionCheckResult == PermissionCheckResult.OK
                                    ? Messages.GENERAL_STATE_OK
                                    : permissionCheckResult == PermissionCheckResult.Warning
                                          ? Messages.WARNING
                                          : Messages.FAILED;

                descriptionCell.Value = string.Format("{0} {1}", description, result);
            }

            public void SetPermissionCheckInProgress(bool value)
            {
                iconCell.Value = value ? Images.StaticImages.ajax_loader : new Bitmap(1, 1);
            }
        }

        private class PermissionCheckDetailsRow : PermissionCheckGridRow
        {
            public PermissionCheckDetailsRow(string desc, PermissionCheckResult checkResult)
            {
                iconCell.Value = checkResult == PermissionCheckResult.OK
                                     ? Images.StaticImages._000_Tick_h32bit_16
                                     : checkResult == PermissionCheckResult.Warning
                                           ? Images.StaticImages._000_Alert2_h32bit_16
                                           : Images.StaticImages._000_Abort_h32bit_16;
                descriptionCell.Value = desc;
            }
        }
    }


    public class WizardRbacCheck
    {
        private readonly RbacMethodList _apiMethodsToCheck = new RbacMethodList();

        /// <summary>
        /// This is the warning message will be displayed. It should be a format string with one
        /// placeholder for the user's current role, which will be supplied when the page is displayed.
        /// </summary>
        public string WarningMessage { get; }

        /// <summary>
        /// Whether this check should prevent the user from proceeding further on the wizard
        /// </summary>
        public bool Blocking { get; set; }

        public Action WarningAction { get; set; }

        public WizardRbacCheck(string warningMessage, RbacMethodList methodList)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(warningMessage.Contains("{0}"),
                "WarningMessage should be a format string with one placeholder for the user's current role");
#endif
            WarningMessage = warningMessage;
            _apiMethodsToCheck = methodList;
        }

        public WizardRbacCheck(string warningMessage, params string[] strings)
            : this(warningMessage, new RbacMethodList(strings))
        {
        }

        public void AddApiMethods(params string[] strings)
        {
            _apiMethodsToCheck.AddRange(strings);
        }

        public void AddApiMethods(RbacMethodList methodList)
        {
            foreach (var method in methodList)
                _apiMethodsToCheck.Add(method);
        }

        public List<Role> GetValidRoles(IXenConnection connection)
        {
            return Role.ValidRoleList(_apiMethodsToCheck, connection);
        }
    }
}
