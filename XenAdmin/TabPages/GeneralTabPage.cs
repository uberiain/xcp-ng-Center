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
using System.Text;
using System.Windows.Forms;
using XenAdmin.Actions;
using XenAdmin.Commands;
using XenAdmin.Controls;
using XenAdmin.Core;
using XenAdmin.CustomFields;
using XenAdmin.Dialogs;
using XenAdmin.Model;
using XenAdmin.Network;
using XenAdmin.SettingsPanels;
using XenAPI;
using XenCenterLib;

namespace XenAdmin.TabPages
{
    public partial class GeneralTabPage : BaseTabPage
    {
        #region Private fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<PDSection> sections = new List<PDSection>();
        private readonly Dictionary<Type, List<PDSection>> _expandedSections = new Dictionary<Type, List<PDSection>>();

        private IXenObject xenObject;

        /// <summary>
        /// Indicates whether rebuild requests have been queued,
        /// in which case rebuilding the section list is necessary
        /// </summary>
        private bool refreshNeeded;

        private readonly CollectionChangeEventHandler VM_guest_metrics_CollectionChangedWithInvoke;

        #endregion

        public GeneralTabPage()
        {
            InitializeComponent();

            VM_guest_metrics_CollectionChangedWithInvoke = Program.ProgramInvokeHandler(VM_guest_metrics_CollectionChanged);
            OtherConfigAndTagsWatcher.TagsChanged += OtherConfigAndTagsWatcher_TagsChanged;

            foreach (Control control in panel2.Controls)
            {
                if (!(control is Panel p))
                    continue;

                foreach (Control c in p.Controls)
                {
                    if (!(c is PDSection s))
                        continue;
                    sections.Add(s);
                    s.ContentChangedSelection += s_ContentChangedSelection;
                    s.ContentReceivedFocus += s_ContentReceivedFocus;
                    s.ExpandedChanged += s_ExpandedChanged;
                }
            }
        }

        public override string HelpID => "TabPageSettings";

        private void ScrollToSelectionIfNeeded(PDSection s)
        {
            if (s.HasNoSelection())
                return;

            Rectangle selectedRowBounds = s.SelectedRowBounds;

            // translate to the coordinates of the pdsection container panel (the one added for padding purposes)
            selectedRowBounds.Offset(s.Parent.Location);

            // Top edge visible?
            if (panel2.ClientRectangle.Height - selectedRowBounds.Top > 0 && selectedRowBounds.Top > 0)
            {
                // Bottom edge visible?
                if (panel2.ClientRectangle.Height - selectedRowBounds.Bottom > 0 && selectedRowBounds.Bottom > 0)
                {
                    // The entire selected row is in view, no need to move 
                    return;
                }
            }

            panel2.ForceScrollTo(s);
        }

        public IXenObject XenObject
        {
            set
            {
                if (value == null)
                    return;

                if (value.Equals(xenObject))
                {
                    BuildList();
                    return;
                }

                UnregisterHandlers();

                xenObject = value;
                RegisterHandlers();
                BuildList();

                List<PDSection> expandedSections = null;

                if (xenObject != null && !_expandedSections.TryGetValue(xenObject.GetType(), out expandedSections))
                {
                    expandedSections = new List<PDSection> { pdSectionGeneral };
                    _expandedSections[xenObject.GetType()] = expandedSections;
                }

                ToggleExpandedState(s => expandedSections == null && s == pdSectionGeneral ||
                                         expandedSections != null && expandedSections.Contains(s));
            }
        }

        private void UnregisterHandlers()
        {
            if (xenObject != null)
                xenObject.PropertyChanged -= PropertyChanged;

            if (xenObject is Host host)
            {
                Host_metrics metric = xenObject.Connection.Resolve(host.metrics);
                if (metric != null)
                    metric.PropertyChanged -= PropertyChanged;
            }
            else if (xenObject is VM vm)
            {
                VM_metrics metric = vm.Connection.Resolve(vm.metrics);
                if (metric != null)
                    metric.PropertyChanged -= PropertyChanged;

                VM_guest_metrics guestmetric = xenObject.Connection.Resolve(vm.guest_metrics);
                if (guestmetric != null)
                    guestmetric.PropertyChanged -= PropertyChanged;

                vm.Connection.Cache.DeregisterCollectionChanged<VM_guest_metrics>(VM_guest_metrics_CollectionChangedWithInvoke);
            }
            else if (xenObject is SR sr)
            {
                foreach (PBD pbd in sr.Connection.ResolveAll(sr.PBDs))
                {
                    pbd.PropertyChanged -= PropertyChanged;
                }
            }
            else if (xenObject is Pool)
            {
                xenObject.Connection.Cache.DeregisterBatchCollectionChanged<Pool_patch>(Pool_patch_BatchCollectionChanged);
                xenObject.Connection.Cache.DeregisterBatchCollectionChanged<Pool_update>(Pool_update_BatchCollectionChanged);
            }
        }

        private void VM_guest_metrics_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (!this.Visible)
                return;
            // Required to refresh the panel when the vm boots so we show the correct pv driver state and version
            // Note this does NOT get called every 2s, just when the vm power state changes (hopefully!)
            BuildList();
        }

        private void RegisterHandlers()
        {
            if (xenObject != null)
                xenObject.PropertyChanged += PropertyChanged;

            if (xenObject is Host host)
            {
                Host_metrics metric = xenObject.Connection.Resolve(host.metrics);
                if (metric != null)
                    metric.PropertyChanged += PropertyChanged;
            }
            else if (xenObject is VM vm)
            {
                VM_metrics metric = vm.Connection.Resolve(vm.metrics);
                if (metric != null)
                    metric.PropertyChanged += PropertyChanged;

                VM_guest_metrics guestmetric = xenObject.Connection.Resolve(vm.guest_metrics);
                if (guestmetric != null)
                    guestmetric.PropertyChanged += PropertyChanged;

                xenObject.Connection.Cache.RegisterCollectionChanged<VM_guest_metrics>(VM_guest_metrics_CollectionChangedWithInvoke);
            }
            else if (xenObject is Pool)
            {
                xenObject.Connection.Cache.RegisterBatchCollectionChanged<Pool_patch>(Pool_patch_BatchCollectionChanged);
                xenObject.Connection.Cache.RegisterBatchCollectionChanged<Pool_update>(Pool_update_BatchCollectionChanged);
            }
        }

        private void Pool_patch_BatchCollectionChanged(object sender, EventArgs e)
        {
            Program.BeginInvoke(this, BuildList);
        }

        private void Pool_update_BatchCollectionChanged(object sender, EventArgs e)
        {
            Program.BeginInvoke(this, BuildList);
        }

        private void OtherConfigAndTagsWatcher_TagsChanged()
        {
            BuildList();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible && refreshNeeded)
            {
                BuildList();
                refreshNeeded = false;
            }
            base.OnVisibleChanged(e);
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "state" || e.PropertyName == "last_updated")
            {
                return;
            }

            Program.Invoke(this, delegate
            {
                if (e.PropertyName == "PBDs")
                {
                    SR sr = xenObject as SR;
                    if (sr == null)
                        return;

                    foreach (PBD pbd in xenObject.Connection.ResolveAll(sr.PBDs))
                    {
                        pbd.PropertyChanged -= PropertyChanged;
                        pbd.PropertyChanged += PropertyChanged;
                    }
                }
                //else
                //{
                //    // At the moment we are rebuilding on almost any property changed event. 
                //    // As long as we are just clearing and re-adding the rows in the PDSections this seems to be super quick. 
                //    // If it gets slower we should update specific boxes for specific property changes.
                //    if (licenseStatus != null && licenseStatus.Updated)
                //        licenseStatus.BeginUpdate();
                //}

                BuildList();
            });
        }

        public void UpdateButtons()
        {
            if (xenObject is DockerContainer container)
            {
                buttonProperties.Visible = false;

                linkLabelCollapse.Visible =
                    linkLabelExpand.Visible = !Helpers.StockholmOrGreater(xenObject.Connection);
            }
            else
            {
                buttonProperties.Visible = true;
                buttonProperties.Enabled = xenObject != null && !xenObject.Locked && xenObject.Connection != null && xenObject.Connection.IsConnected;
                linkLabelCollapse.Visible = linkLabelExpand.Visible = true;
            }
        }

        public void BuildList()
        {
            if (!Visible)
            {
                refreshNeeded = true;
                return;
            }

            if (xenObject == null)
                return;

            if (xenObject is Host && !xenObject.Connection.IsConnected)
                base.Text = Messages.CONNECTION_GENERAL_TAB_TITLE;
            else if (xenObject is Host)
                base.Text = Messages.HOST_GENERAL_TAB_TITLE;
            else if (xenObject is VM vm)
            {
                if (vm.is_a_snapshot)
                    base.Text = Messages.SNAPSHOT_GENERAL_TAB_TITLE;
                else if (vm.is_a_template)
                    base.Text = Messages.TEMPLATE_GENERAL_TAB_TITLE;
                else
                    base.Text = Messages.VM_GENERAL_TAB_TITLE;
            }
            else if (xenObject is SR)
                base.Text = Messages.SR_GENERAL_TAB_TITLE;
            else if (xenObject is Pool)
                base.Text = Messages.POOL_GENERAL_TAB_TITLE;
            else if (xenObject is DockerContainer)
                base.Text = Messages.CONTAINER_GENERAL_TAB_TITLE;

            panel2.SuspendLayout();
            // Clear all the data from the sections (visible and non visible)
            foreach (PDSection s in sections)
            {
                s.PauseLayout();
                s.ClearData();
            }
            // Generate the content of each box, each method performs a cast and only populates if XenObject is the relevant type

            if (xenObject is Host && (xenObject.Connection == null || !xenObject.Connection.IsConnected))
            {
                GenerateDisconnectedHostBox();
            }
            else if (xenObject is DockerContainer dockerContainer)
            {
                if (!Helpers.StockholmOrGreater(xenObject.Connection))
                    GenerateDockerContainerGeneralBox(dockerContainer);
            }
            else
            {
                GenerateGeneralBox();
                GenerateCertificateBox();
                GenerateCustomFieldsBox();
                GenerateInterfaceBox();
                GenerateMemoryBox();
                GenerateVersionBox();
                GenerateCPUBox();
                GenerateBootBox();
                GenerateHABox();
                GenerateStatusBox();
                GenerateMultipathBox();
                GenerateMultipathBootBox();
                GenerateVCPUsBox();
                GenerateDockerInfoBox();
                GenerateReadCachingBox();
                GenerateDeviceSecurityBox();
            }

            // hide all the sections which haven't been populated, those that have make sure are visible
            foreach (PDSection s in sections)
            {
                if (s.IsEmpty())
                {
                    s.Parent.Visible = false;
                }
                else
                {
                    s.Parent.Visible = true;
                    if (s.ContainsFocus)
                        s.RestoreSelection();
                }
                s.StartLayout();
            }
            panel2.ResumeLayout();
            SetupDeprecationBanner();
            UpdateButtons();
        }

        private void GenerateInterfaceBox()
        {
            Host Host = xenObject as Host;
            Pool Pool = xenObject as Pool;
            if (Host != null)
            {
                fillInterfacesForHost(Host, false);
            }
            else if (Pool != null)
            {
                // Here we tell fillInterfacesForHost to prefix each entry with the hosts name label, so we know which entry belongs to which host
                // and also to better preserve uniqueness for keys in the PDSection

                foreach (Host h in Pool.Connection.Cache.Hosts)
                    fillInterfacesForHost(h, true);
            }
        }

        private void fillInterfacesForHost(Host Host, bool includeHostSuffix)
        {
            PDSection s = pdSectionManagementInterfaces;

            var editValue = new ToolStripMenuItem(Messages.EDIT) { Image = Images.StaticImages.edit_16 };
            editValue.Click += delegate
                {
                    NetworkingProperties p = new NetworkingProperties(Host, null);
                    p.ShowDialog(Program.MainWindow);
                };

            if (!string.IsNullOrEmpty(Host.hostname))
            {
                if (!includeHostSuffix)
                    s.AddEntry(FriendlyName("host.hostname"), Host.hostname, editValue);
                else
                    s.AddEntry(
                        string.Format(Messages.PROPERTY_ON_OBJECT, FriendlyName("host.hostname"), Helpers.GetName(Host)),
                        Host.hostname,
                        editValue);
            }
            foreach (PIF pif in xenObject.Connection.ResolveAll<PIF>(Host.PIFs))
            {
                if (pif.management)
                {
                    if (!includeHostSuffix)
                        s.AddEntry(Messages.MANAGEMENT_INTERFACE, pif.FriendlyIPAddress(), editValue);
                    else
                        s.AddEntry(
                            string.Format(Messages.PROPERTY_ON_OBJECT, Messages.MANAGEMENT_INTERFACE, Helpers.GetName(Host)),
                            pif.FriendlyIPAddress(),
                            editValue);
                }
            }

            foreach (PIF pif in xenObject.Connection.ResolveAll<PIF>(Host.PIFs))
            {
                if (pif.IsSecondaryManagementInterface(Properties.Settings.Default.ShowHiddenVMs))
                {
                    if (!includeHostSuffix)
                        s.AddEntry(pif.GetManagementPurpose().Ellipsise(30), pif.FriendlyIPAddress(), editValue);
                    else
                        s.AddEntry(
                            string.Format(Messages.PROPERTY_ON_OBJECT, pif.GetManagementPurpose().Ellipsise(30), Helpers.GetName(Host)),
                            pif.FriendlyIPAddress(),
                            editValue);
                }
            }
        }

        private void GenerateCustomFieldsBox()
        {
            List<CustomField> customFields = CustomFieldsManager.CustomFieldValues(xenObject);
            if (customFields.Count <= 0)
                return;

            PDSection s = pdSectionCustomFields;

            foreach (CustomField customField in customFields)
            {
                var editValue = new ToolStripMenuItem(Messages.EDIT) { Image = Images.StaticImages.edit_16 };
                editValue.Click += delegate
                    {
                        using (PropertiesDialog dialog = new PropertiesDialog(xenObject))
                        {
                            dialog.SelectCustomFieldsEditPage();
                            dialog.ShowDialog();
                        }
                    };

                var cfWrapper = new CustomFieldWrapper(xenObject, customField.Definition);
                s.AddEntry(customField.Definition.Name, cfWrapper.ToString(), editValue);
            }
        }

        private void GenerateHABox()
        {
            VM vm = xenObject as VM;
            if (vm == null)
                return;

            Pool pool = Helpers.GetPoolOfOne(xenObject.Connection);
            if (pool == null || !pool.ha_enabled)
                return;

            PDSection s = pdSectionHighAvailability;

            s.AddEntry(FriendlyName("VM.ha_restart_priority"), Helpers.RestartPriorityI18n(vm.HARestartPriority()),
                new PropertiesToolStripMenuItem(new VmEditHaCommand(Program.MainWindow, xenObject)));
        }

        private void GenerateStatusBox()
        {
            SR sr = xenObject as SR;
            if (sr == null)
                return;

            PDSection s = pdSectionStatus;

            bool broken = false;
            bool repairable = sr.HasPBDs();
            var statusString = Messages.GENERAL_STATE_OK;

            if (sr.IsDetached())
            {
                broken = true;
                statusString = Messages.DETACHED;
            }
            else if (sr.IsBroken())
            {
                broken = true;
                statusString = Messages.GENERAL_SR_STATE_BROKEN;
            }
            else if (!sr.MultipathAOK())
            {
                broken = true;
                statusString = Messages.GENERAL_MULTIPATH_FAILURE;
            }

            var repairItem = new ToolStripMenuItem
            {
                Text = Messages.GENERAL_SR_CONTEXT_REPAIR,
                Image = Images.StaticImages._000_StorageBroken_h32bit_16
            };
            repairItem.Click += (sender, args) =>
                Program.MainWindow.ShowPerConnectionWizard(xenObject.Connection, new RepairSRDialog(sr));

            if (broken && repairable)
                s.AddEntry(FriendlyName("SR.state"), statusString, repairItem);
            else
                s.AddEntry(FriendlyName("SR.state"), statusString);

            foreach (Host host in xenObject.Connection.Cache.Hosts)
            {
                PBD pbdToSR = null;
                foreach (PBD pbd in xenObject.Connection.ResolveAll(host.PBDs))
                {
                    if (pbd.SR.opaque_ref != xenObject.opaque_ref)
                        continue;

                    pbdToSR = pbd;
                    break;
                }
                if (pbdToSR == null)
                {
                    if (!sr.shared)
                        continue;

                    if (repairable)
                        s.AddEntry(Helpers.GetName(host).Ellipsise(30),
                            Messages.REPAIR_SR_DIALOG_CONNECTION_MISSING, Color.Red, repairItem);
                    else
                        s.AddEntry(Helpers.GetName(host).Ellipsise(30),
                            Messages.REPAIR_SR_DIALOG_CONNECTION_MISSING, Color.Red);

                    continue;
                }

                pbdToSR.PropertyChanged -= PropertyChanged;
                pbdToSR.PropertyChanged += PropertyChanged;

                if (!pbdToSR.currently_attached)
                {
                    if (repairable)
                        s.AddEntry(Helpers.GetName(host).Ellipsise(30), pbdToSR.StatusString(), Color.Red, repairItem);
                    else
                        s.AddEntry(Helpers.GetName(host).Ellipsise(30), pbdToSR.StatusString(), Color.Red);
                }
                else
                {
                    s.AddEntry(Helpers.GetName(host).Ellipsise(30), pbdToSR.StatusString());
                }
            }
        }

        private void GenerateMultipathBox()
        {
            SR sr = xenObject as SR;
            if (sr == null)
                return;

            PDSection s = pdSectionMultipathing;

            if (!sr.MultipathCapable())
            {
                s.AddEntry(Messages.MULTIPATH_CAPABLE, Messages.NO);
                return;
            }

            if (sr.LunPerVDI())
            {
                Dictionary<VM, Dictionary<VDI, String>>
                    pathStatus = sr.GetMultiPathStatusLunPerVDI();

                foreach (Host host in xenObject.Connection.Cache.Hosts)
                {
                    PBD pbd = sr.GetPBDFor(host);
                    if (pbd == null || !pbd.MultipathActive())
                    {
                        s.AddEntry(host.Name(), Messages.MULTIPATH_NOT_ACTIVE);
                        continue;
                    }

                    s.AddEntry(host.Name(), Messages.MULTIPATH_ACTIVE);
                    foreach (KeyValuePair<VM, Dictionary<VDI, String>> kvp in pathStatus)
                    {
                        VM vm = kvp.Key;
                        if (vm.resident_on == null ||
                            vm.resident_on.opaque_ref != host.opaque_ref)
                            continue;

                        bool renderOnOneLine = false;
                        int lastMax = -1;
                        int lastCurrent = -1;

                        foreach (KeyValuePair<VDI, String> kvp2 in kvp.Value)
                        {
                            int current;
                            int max;
                            if (!PBD.ParsePathCounts(kvp2.Value, out current, out max))
                                continue;

                            if (!renderOnOneLine)
                            {
                                lastMax = max;
                                lastCurrent = current;
                                renderOnOneLine = true;
                                continue;
                            }

                            if (lastMax == max && lastCurrent == current)
                                continue;

                            renderOnOneLine = false;
                            break;
                        }

                        if (renderOnOneLine)
                        {
                            AddMultipathLine(s, String.Format("    {0}", vm.Name()),
                                             lastCurrent, lastMax, pbd.ISCSISessions());
                        }
                        else
                        {
                            s.AddEntry(String.Format("    {0}", vm.Name()), "");

                            foreach (KeyValuePair<VDI, String> kvp2 in kvp.Value)
                            {
                                int current;
                                int max;
                                if (!PBD.ParsePathCounts(kvp2.Value, out current, out max))
                                    continue;

                                AddMultipathLine(s, String.Format("        {0}", kvp2.Key.Name()),
                                                current, max, pbd.ISCSISessions());
                            }
                        }
                    }
                }
            }
            else
            {
                Dictionary<PBD, String> pathStatus = sr.GetMultiPathStatusLunPerSR();

                foreach (Host host in xenObject.Connection.Cache.Hosts)
                {
                    PBD pbd = sr.GetPBDFor(host);
                    if (pbd == null || !pathStatus.ContainsKey(pbd))
                    {
                        if (pbd == null)
                            s.AddEntry(host.Name(), Messages.MULTIPATH_NOT_ACTIVE);
                        else if (pbd.MultipathActive())
                            s.AddEntry(host.Name(), Messages.MULTIPATH_ACTIVE);
                        else if (sr.GetSRType(true) == SR.SRTypes.gfs2)
                            s.AddEntry(host.Name(), Messages.MULTIPATH_NOT_ACTIVE_GFS2, Color.Red);
                        else
                            s.AddEntry(host.Name(), Messages.MULTIPATH_NOT_ACTIVE);
                        continue;
                    }

                    String status = pathStatus[pbd];

                    int current;
                    int max;
                    PBD.ParsePathCounts(status, out current, out max); //Guaranteed to work if PBD is in pathStatus

                    AddMultipathLine(s, host.Name(), current, max, pbd.ISCSISessions());
                }
            }
        }

        private void AddMultipathLine(PDSection s, String title, int current, int max, int iscsiSessions)
        {
            bool bad = current < max || (iscsiSessions != -1 && max < iscsiSessions);
            string row = string.Format(Messages.MULTIPATH_STATUS, current, max);
            if (iscsiSessions != -1)
                row += string.Format(Messages.MULTIPATH_STATUS_ISCSI_SESSIONS, iscsiSessions);

            if (bad)
                s.AddEntry(title, row, Color.Red);
            else
                s.AddEntry(title, row);
        }

        private void GenerateMultipathBootBox()
        {
            Host host = xenObject as Host;
            if (host == null)
                return;

            int current, max;
            if (!host.GetBootPathCounts(out current, out max))
                return;

            PDSection s = pdSectionMultipathBoot;
            string text = string.Format(Messages.MULTIPATH_STATUS, current, max);
            bool bad = current < max;
            if (bad)
                s.AddEntry(Messages.STATUS, text, Color.Red);
            else
                s.AddEntry(Messages.STATUS, text);
        }

        private void GenerateBootBox()
        {
            VM vm = xenObject as VM;
            if (vm == null)
                return;

            PDSection s = pdSectionBootOptions;

            s.AddEntry(FriendlyName("VM.auto_boot"), Helpers.BoolToString(vm.GetAutoPowerOn()),
               new PropertiesToolStripMenuItem(new VmEditStartupOptionsCommand(Program.MainWindow, vm)));

            if (vm.IsHVM())
            {
                s.AddEntry(FriendlyName("VM.BootOrder"), HVMBootOrder(vm),
                    new PropertiesToolStripMenuItem(new VmEditStartupOptionsCommand(Program.MainWindow, vm)));

                if (Helpers.NaplesOrGreater(vm.Connection))
                {
                    //see XSI-1362 for an explanation of this logic
                    if (vm.IsHVM() && vm.IsDefaultBootModeUefi())
                    {
                        var secureBoot = vm.GetSecureBootMode();
                        var pool = Helpers.GetPoolOfOne(vm.Connection);
                        var poolHasCertificates = !string.IsNullOrEmpty(pool?.uefi_certificates);

                        if (secureBoot == "true" && !poolHasCertificates)
                            s.AddEntry(FriendlyName("VM.BootMode"), string.Format(Messages.CUSTOM_FIELD_NAME_AND_TYPE, Messages.UEFI_SECURE_BOOT, Messages.GUEFI_SECUREBOOT_MODE_MISSING_CERTIFICATES), Color.Red);
                        else if (secureBoot == "true" || secureBoot == "auto" && poolHasCertificates)
                            s.AddEntry(FriendlyName("VM.BootMode"), Messages.UEFI_SECURE_BOOT);
                        else
                            s.AddEntry(FriendlyName("VM.BootMode"), Messages.UEFI_BOOT);
                    }
                    else
                    {
                        s.AddEntry(FriendlyName("VM.BootMode"), Messages.BIOS_BOOT);
                    }
                }
            }
            else
            {
                s.AddEntry(FriendlyName("VM.PV_args"), vm.PV_args,
                    new PropertiesToolStripMenuItem(new VmEditStartupOptionsCommand(Program.MainWindow, vm)));
            }
        }

        private void GenerateVersionBox()
        {
            if (!(xenObject is Host host) || host.software_version == null)
                return;

            var unixMinDateTime = Util.GetUnixMinDateTime();

            if (host.software_version.ContainsKey("date"))
            {
                string buildDate = host.software_version["date"];

                if (Util.TryParseIso8601DateTime(host.software_version["date"], out var softwareVersionDate) && softwareVersionDate > unixMinDateTime)
                    buildDate = HelpersGUI.DateTimeToString(softwareVersionDate.ToLocalTime(), Messages.DATEFORMAT_DMY_HMS, true);
                else if (Util.TryParseNonIso8601DateTime(host.software_version["date"], out softwareVersionDate) && softwareVersionDate > unixMinDateTime)
                    buildDate = HelpersGUI.DateTimeToString(softwareVersionDate.ToLocalTime(), Messages.DATEFORMAT_DMY, true);

                pdSectionVersion.AddEntry(Messages.SOFTWARE_VERSION_DATE, buildDate);
            }

            if (!Helpers.ElyOrGreater(host) && host.software_version.ContainsKey("build_number"))
                pdSectionVersion.AddEntry(Messages.SOFTWARE_VERSION_BUILD_NUMBER, host.software_version["build_number"]);

            if (host.software_version.ContainsKey("product_version"))
            {
                //var hotfixEligibilityString = AdditionalVersionString(host);
                var versionString = $"{host.ProductBrand()} {host.ProductVersionText()}";

                //if (string.IsNullOrEmpty(hotfixEligibilityString))
                pdSectionVersion.AddEntry(Messages.SOFTWARE_VERSION_PRODUCT_VERSION, versionString);
                //else
                //    pdSectionVersion.AddEntry(Messages.SOFTWARE_VERSION_PRODUCT_VERSION,
                //        string.Format(Messages.MAINWINDOW_CONTEXT_REASON, versionString, hotfixEligibilityString),
                //        Color.Red);
            }

            if (host.software_version.ContainsKey("dbv"))
                pdSectionVersion.AddEntry("DBV", host.software_version["dbv"]);
        }

        private void GenerateCPUBox()
        {
            Host host = xenObject as Host;
            if (host == null)
                return;

            PDSection s = pdSectionCPU;

            SortedDictionary<long, Host_cpu> d = new SortedDictionary<long, Host_cpu>();
            foreach (Host_cpu cpu in xenObject.Connection.ResolveAll(host.host_CPUs))
            {
                d.Add(cpu.number, cpu);
            }

            bool cpusIdentical = CPUsIdentical(d.Values);

            foreach (Host_cpu cpu in d.Values)
            {
                String label = String.Format(Messages.GENERAL_DETAILS_CPU_NUMBER,
                    cpusIdentical && d.Values.Count > 1 ? String.Format("0 - {0}", d.Values.Count - 1)
                        : cpu.number.ToString());

                s.AddEntry(label, Helpers.GetCPUProperties(cpu));
                if (cpusIdentical)
                    break;
            }
        }

        private void GenerateVCPUsBox()
        {
            VM vm = xenObject as VM;
            if (vm == null)
                return;

            PDSection s = pdSectionVCPUs;

            s.AddEntry(FriendlyName("VM.VCPUs"), vm.VCPUs_at_startup.ToString());
            if (vm.VCPUs_at_startup != vm.VCPUs_max || vm.SupportsVcpuHotplug())
                s.AddEntry(FriendlyName("VM.MaxVCPUs"), vm.VCPUs_max.ToString());
            s.AddEntry(FriendlyName("VM.Topology"), vm.Topology());
        }

        private void GenerateDisconnectedHostBox()
        {
            IXenConnection conn = xenObject.Connection;

            PDSection s = pdSectionGeneral;

            string name = Helpers.GetName(xenObject);
            s.AddEntry(FriendlyName("host.name_label"), name);
            if (conn != null && conn.Hostname != name)
                s.AddEntry(FriendlyName("host.address"), conn.Hostname);

            if (conn != null && conn.PoolMembers.Count > 1)
                s.AddEntry(FriendlyName("host.pool_members"), string.Join(", ", conn.PoolMembers.ToArray()));

        }

        private string GetCertificateType(certificate_type typ)
        {
            switch (typ)
            {
                case certificate_type.ca:
                    return Messages.CERTIFICATE_TYPE_CA;
                case certificate_type.host:
                    return Messages.CERTIFICATE_TYPE_HOST;
                case certificate_type.host_internal:
                    return Messages.CERTIFICATE_TYPE_HOST_INTERNAL;
                case certificate_type.unknown:
                default:
                    return Messages.UNKNOWN;
            }
        }

        private void GenerateCertificateBox()
        {
            if (xenObject is Host host && Helpers.StockholmOrGreater(host) && host.certificates != null)
            {
                var certificates = host.certificates.Select(c => host.Connection.Resolve(c)).Where(c => c != null).OrderBy(c => c.type).ToList();

                foreach (var certificate in certificates)
                {
                    var validity = string.Format(Messages.CERTIFICATE_VALIDITY_PERIOD_VALUE,
                        HelpersGUI.DateTimeToString(certificate.not_before.ToLocalTime(), Messages.DATEFORMAT_DMY_HM, true),
                        HelpersGUI.DateTimeToString(certificate.not_after.ToLocalTime(), Messages.DATEFORMAT_DMY_HM, true));

                    var thumbprint = string.Format(Messages.CERTIFICATE_THUMBPRINT_VALUE, certificate.fingerprint);

                    if (!Helpers.CloudOrGreater(host) || !Helpers.XapiEqualOrGreater_1_290_0(host) || certificate.type == certificate_type.host)
                        pdSectionCertificate.AddEntry(GetCertificateType(certificate.type), $"{validity}\n{thumbprint}",
                            new CommandToolStripMenuItem(new InstallCertificateCommand(Program.MainWindow, host), true),
                            new CommandToolStripMenuItem(new ResetCertificateCommand(Program.MainWindow, host), true));
                    else
                        pdSectionCertificate.AddEntry(GetCertificateType(certificate.type), $"{validity}\n{thumbprint}");
                }
            }

            if (xenObject is Pool pool && Helpers.CloudOrGreater(pool.Connection))
            {
                var certificates = pool.Connection.Cache.Certificates.Where(c => c != null && c.type == certificate_type.ca).OrderBy(c => c.name).ToList();

                foreach (var certificate in certificates)
                {
                    var validity = string.Format(Messages.CERTIFICATE_VALIDITY_PERIOD_VALUE,
                        HelpersGUI.DateTimeToString(certificate.not_before.ToLocalTime(), Messages.DATEFORMAT_DMY_HM, true),
                        HelpersGUI.DateTimeToString(certificate.not_after.ToLocalTime(), Messages.DATEFORMAT_DMY_HM, true));

                    var thumbprint = string.Format(Messages.CERTIFICATE_THUMBPRINT_VALUE, certificate.fingerprint);

                    pdSectionCertificate.AddEntry(certificate.name,
                        $"{GetCertificateType(certificate.type)}\n{validity}\n{thumbprint}");
                }
            }
        }

        private void GenerateGeneralBox()
        {
            PDSection s = pdSectionGeneral;

            s.AddEntry(FriendlyName("host.name_label"), Helpers.GetName(xenObject),
                new PropertiesToolStripMenuItem(new PropertiesCommand(Program.MainWindow, xenObject)));

            VM vm = xenObject as VM;
            if (vm == null || vm.DescriptionType() != VM.VmDescriptionType.None)
            {
                s.AddEntry(FriendlyName("host.name_description"), xenObject.Description(),
                            new PropertiesToolStripMenuItem(new DescriptionPropertiesCommand(Program.MainWindow, xenObject)));
            }

            GenTagRow(s);
            GenFolderRow(s);

            if (xenObject is Host host)
            {
                var isStandAloneHost = Helpers.GetPool(xenObject.Connection) == null;

                if (!isStandAloneHost)
                    s.AddEntry(Messages.POOL_COORDINATOR, host.IsCoordinator() ? Messages.YES : Messages.NO);

                if (!host.IsLive())
                {
                    s.AddEntry(FriendlyName("host.enabled"), Messages.HOST_NOT_LIVE, Color.Red);
                }
                else if (!host.enabled)
                {
                    s.AddEntry(FriendlyName("host.enabled"),
                               host.MaintenanceMode() ? Messages.HOST_IN_MAINTENANCE_MODE : Messages.DISABLED,
                               Color.Red,
                               new CommandToolStripMenuItem(new HostMaintenanceModeCommand(
                                   Program.MainWindow, host, HostMaintenanceModeCommandParameter.Exit)));
                }
                else
                {
                    var item = new CommandToolStripMenuItem(new HostMaintenanceModeCommand(
                        Program.MainWindow, host, HostMaintenanceModeCommandParameter.Enter));

                    s.AddEntry(FriendlyName("host.enabled"), Messages.YES, item);
                }

                s.AddEntry("Autoboot of VMs enabled", Helpers.BoolToString(host.GetVmAutostartEnabled()));

                if (Helpers.CloudOrGreater(host) && Helpers.XapiEqualOrGreater_1_290_0(host))
                {
                    var pool = Helpers.GetPoolOfOne(xenObject.Connection);

                    if (pool.tls_verification_enabled &&
                        (!Helpers.XapiEqualOrGreater_1_313_0(host) || host.tls_verification_enabled))
                    {
                        s.AddEntry(Messages.CERTIFICATE_VERIFICATION_KEY, Messages.ENABLED);
                    }
                    else if (pool.tls_verification_enabled && Helpers.XapiEqualOrGreater_1_313_0(host) &&
                             !host.tls_verification_enabled && isStandAloneHost)
                    {
                        s.AddEntry(Messages.CERTIFICATE_VERIFICATION_KEY,
                            Messages.CERTIFICATE_VERIFICATION_HOST_DISABLED_STANDALONE,
                            Color.Red,
                            new CommandToolStripMenuItem(new EnableTlsVerificationCommand(Program.MainWindow, pool)));
                    }
                    else
                    {
                        s.AddEntry(Messages.CERTIFICATE_VERIFICATION_KEY,
                            Messages.DISABLED,
                            Color.Red,
                            new CommandToolStripMenuItem(new EnableTlsVerificationCommand(Program.MainWindow, pool)));
                    }
                }

                s.AddEntry(FriendlyName("host.iscsi_iqn"), host.GetIscsiIqn(),
                    new PropertiesToolStripMenuItem(new IqnPropertiesCommand(Program.MainWindow, xenObject)));

                var sysLog = host.GetSysLogDestination();
                var sysLogDisplay = string.IsNullOrEmpty(sysLog)
                    ? Messages.HOST_LOG_DESTINATION_LOCAL
                    : string.Format(Messages.HOST_LOG_DESTINATION_LOCAL_AND_REMOTE, sysLog);

                s.AddEntry(FriendlyName("host.log_destination"), sysLogDisplay,
                   new PropertiesToolStripMenuItem(new HostEditLogDestinationCommand(Program.MainWindow, xenObject)));

                PrettyTimeSpan uptime = host.Uptime();
                PrettyTimeSpan agentUptime = host.AgentUptime();
                s.AddEntry(FriendlyName("host.uptime"), uptime == null ? "" : uptime.ToString());
                s.AddEntry(FriendlyName("host.agentUptime"), agentUptime == null ? "" : agentUptime.ToString());

                if (host.external_auth_type == Auth.AUTH_TYPE_AD)
                    s.AddEntry(FriendlyName("host.external_auth_service_name"), host.external_auth_service_name);
            }

            if (vm != null)
            {
                s.AddEntry(FriendlyName("VM.OSName"), vm.GetOSName());

                s.AddEntry(FriendlyName("VM.OperatingMode"), vm.IsHVM() ? Messages.VM_OPERATING_MODE_HVM : Messages.VM_OPERATING_MODE_PV);

                if (!vm.DefaultTemplate())
                {
                    s.AddEntry(Messages.BIOS_STRINGS_COPIED, vm.BiosStringsCopied() ? Messages.YES : Messages.NO);
                }

                if (vm.Connection != null)
                {
                    var appl = vm.Connection.Resolve(vm.appliance);
                    if (appl != null)
                    {
                        void LaunchProperties()
                        {
                            using (PropertiesDialog propertiesDialog = new PropertiesDialog(appl))
                                propertiesDialog.ShowDialog(this);
                        }

                        var applProperties = new ToolStripMenuItem(Messages.VM_APPLIANCE_PROPERTIES, null,
                            (sender, e) => LaunchProperties());

                        s.AddEntryLink(Messages.VM_APPLIANCE, appl.Name(), LaunchProperties, applProperties);
                    }
                }

                if (vm.is_a_snapshot)
                {
                    VM snapshotOf = vm.Connection.Resolve(vm.snapshot_of);
                    s.AddEntry(Messages.SNAPSHOT_OF, snapshotOf == null ? string.Empty : snapshotOf.Name());
                    s.AddEntry(Messages.CREATION_TIME, HelpersGUI.DateTimeToString(vm.snapshot_time.ToLocalTime() + vm.Connection.ServerTimeOffset, Messages.DATEFORMAT_DMY_HMS, true));
                }

                if (!vm.is_a_template)
                {
                    GenerateVirtualisationStatusForGeneralBox(s, vm);

                    var runningTime = vm.RunningTime();
                    if (runningTime != null)
                        s.AddEntry(FriendlyName("VM.uptime"), runningTime.ToString());

                    if (vm.IsP2V())
                    {
                        s.AddEntry(FriendlyName("VM.P2V_SourceMachine"), vm.P2V_SourceMachine());
                        s.AddEntry(FriendlyName("VM.P2V_ImportDate"), HelpersGUI.DateTimeToString(vm.P2V_ImportDate().ToLocalTime(), Messages.DATEFORMAT_DMY_HMS, true));
                    }

                    // Dont show if WLB is enabled.
                    if (VMCanChooseHomeServer(vm))
                    {
                        s.AddEntry(FriendlyName("VM.affinity"), vm.AffinityServerString(),
                            new PropertiesToolStripMenuItem(new VmEditHomeServerCommand(Program.MainWindow, xenObject)));
                    }
                }
            }

            if (xenObject is SR sr)
            {
                s.AddEntry(Messages.TYPE, sr.FriendlyTypeName());

                if (sr.content_type != SR.Content_Type_ISO && sr.GetSRType(false) != SR.SRTypes.udev)
                    s.AddEntry(FriendlyName("SR.size"), sr.SizeString());

                if (sr.GetScsiID() != null)
                    s.AddEntry(FriendlyName("SR.scsiid"), sr.GetScsiID() ?? Messages.UNKNOWN);

                // if in folder-view or if looking at SR on storagelink then display
                // location here
                if (Program.MainWindow.SelectionManager.Selection.HostAncestor == null && Program.MainWindow.SelectionManager.Selection.PoolAncestor == null)
                {
                    IXenObject belongsTo = Helpers.GetPool(sr.Connection);

                    if (belongsTo != null)
                    {
                        s.AddEntry(Messages.POOL, Helpers.GetName(belongsTo));
                    }
                    else
                    {
                        belongsTo = Helpers.GetCoordinator(sr.Connection);

                        if (belongsTo != null)
                        {
                            s.AddEntry(Messages.SERVER, Helpers.GetName(belongsTo));
                        }
                    }
                }
            }

            if (xenObject is Pool p)
            {
                s.AddEntry(Messages.NUMBER_OF_SOCKETS, p.CpuSockets().ToString());

                s.AddEntry("Autoboot of VMs enabled", Helpers.BoolToString(p.GetVmAutostartEnabled()));

                if (Helpers.CloudOrGreater(p.Connection) && Helpers.XapiEqualOrGreater_1_290_0(p.Connection))
                {
                    if (p.tls_verification_enabled)
                    {
                        var disabledHosts = p.Connection.Cache.Hosts.Where(h => !h.tls_verification_enabled).ToList();

                        if (Helpers.XapiEqualOrGreater_1_313_0(p.Connection) && disabledHosts.Count > 0)
                        {
                            var sb = new StringBuilder(Messages.CERTIFICATE_VERIFICATION_HOST_DISABLED_IN_POOL);
                            foreach (var h in disabledHosts)
                                sb.AppendLine().AppendFormat(h.Name());

                            s.AddEntry(Messages.CERTIFICATE_VERIFICATION_KEY,
                                sb.ToString(),
                                Color.Red,
                                new CommandToolStripMenuItem(new EnableTlsVerificationCommand(Program.MainWindow, p)));
                        }
                        else
                        {
                            s.AddEntry(Messages.CERTIFICATE_VERIFICATION_KEY, Messages.ENABLED);
                        }
                    }
                    else
                    {
                        s.AddEntry(Messages.CERTIFICATE_VERIFICATION_KEY,
                            Messages.DISABLED,
                            Color.Red,
                            new CommandToolStripMenuItem(new EnableTlsVerificationCommand(Program.MainWindow, p)));
                    }
                }

                var coordinator = p.Connection.Resolve(p.master);
                if (coordinator != null)
                {
                    var versionString = $"{coordinator.ProductBrand()} {coordinator.ProductVersionText()}";
                    s.AddEntry(Messages.SOFTWARE_VERSION_PRODUCT_VERSION, versionString);
                }
            }

            if (xenObject is VDI vdi)
            {
                s.AddEntry(Messages.SIZE, vdi.SizeText(),
                    new PropertiesToolStripMenuItem(new VdiEditSizeLocationCommand(Program.MainWindow, xenObject)));

                SR vdiSr = vdi.Connection.Resolve(vdi.SR);
                if (vdiSr != null && !vdiSr.IsToolsSR())
                    s.AddEntry(Messages.DATATYPE_STORAGE, vdiSr.NameWithLocation());

                string vdiVms = vdi.VMsOfVDI();
                if (!string.IsNullOrEmpty(vdiVms))
                    s.AddEntry(Messages.VIRTUAL_MACHINE, vdiVms);
            }

            s.AddEntry(FriendlyName("host.uuid"), xenObject.Get("uuid") as string);
        }

        private static void GenerateVirtualisationStatusForGeneralBox(PDSection s, VM vm)
        {
            if (vm != null && vm.Connection != null)
            {
                //For Dundee or higher Windows VMs
                if (vm.HasNewVirtualizationStates())
                {
                    var status = vm.GetVirtualizationStatus(out var statusString);

                    var sb = new StringBuilder();

                    if (vm.power_state == vm_power_state.Running)
                    {
                        if (status.HasFlag(VM.VirtualizationStatus.Unknown))
                        {
                            sb.AppendLine(statusString);
                        }
                        else
                        {
                            //Row 1 : I/O Drivers
                            sb.AppendLine(status.HasFlag(VM.VirtualizationStatus.IoDriversInstalled)
                                ? Messages.VIRTUALIZATION_STATE_VM_IO_OPTIMIZED
                                : Messages.VIRTUALIZATION_STATE_VM_IO_NOT_OPTIMIZED);

                            //Row 2: Management Agent
                            sb.AppendLine(status.HasFlag(VM.VirtualizationStatus.ManagementInstalled)
                                ? Messages.VIRTUALIZATION_STATE_VM_MANAGEMENT_AGENT_INSTALLED
                                : Messages.VIRTUALIZATION_STATE_VM_MANAGEMENT_AGENT_NOT_INSTALLED);
                        }
                    }

                    //Row 3 : vendor device - Windows Update
                    sb.Append(vm.has_vendor_device ? Messages.VIRTUALIZATION_STATE_VM_RECEIVING_UPDATES : Messages.VIRTUALIZATION_STATE_VM_NOT_RECEIVING_UPDATES);

                    // displaying Row1 - Row3
                    s.AddEntry(FriendlyName("VM.VirtualizationState"), sb.ToString());

                    //if (vm.power_state == vm_power_state.Running)
                    //{
                    //    //Row 4: Install Tools
                    //    string installMessage = string.Empty;
                    //    var canInstall = InstallToolsCommand.CanRun(vm);

                    //    if (canInstall && !status.HasFlag(VM.VirtualizationStatus.IoDriversInstalled))
                    //    {
                    //        installMessage = Messages.VIRTUALIZATION_STATE_VM_INSTALL_IO_DRIVERS_AND_MANAGEMENT_AGENT;
                    //    }
                    //    else if (canInstall && status.HasFlag(VM.VirtualizationStatus.IoDriversInstalled) &&
                    //             !status.HasFlag(VM.VirtualizationStatus.ManagementInstalled))
                    //    {
                    //        installMessage = Messages.VIRTUALIZATION_STATE_VM_INSTALL_MANAGEMENT_AGENT;
                    //    }
                    //}
                }

                //for everything else (All VMs on pre-Dundee hosts & All non-Windows VMs on any host)
                else if (vm.power_state == vm_power_state.Running)
                {
                    var status = vm.GetVirtualizationStatus(out var statusString);

                    if (status == VM.VirtualizationStatus.NotInstalled || status.HasFlag(VM.VirtualizationStatus.PvDriversOutOfDate))
                    {
                        s.AddEntry(FriendlyName("VM.VirtualizationState"), statusString, Color.Red);
                    }
                    else
                    {
                        s.AddEntry(FriendlyName("VM.VirtualizationState"), statusString);
                    }
                }
            }
        }

        private void GenerateDockerContainerGeneralBox(DockerContainer dockerContainer)
        {
            PDSection s = pdSectionGeneral;
            s.AddEntry(Messages.NAME, dockerContainer.Name().Length != 0 ? dockerContainer.Name() : Messages.NONE);
            s.AddEntry(Messages.STATUS, dockerContainer.status.Length != 0 ? dockerContainer.status : Messages.NONE);
            try
            {
                DateTime created = Util.FromUnixTime(double.Parse(dockerContainer.created)).ToLocalTime();
                s.AddEntry(Messages.CONTAINER_CREATED, HelpersGUI.DateTimeToString(created, Messages.DATEFORMAT_DMY_HMS, true));
            }
            catch
            {
                s.AddEntry(Messages.CONTAINER_CREATED, dockerContainer.created.Length != 0 ? dockerContainer.created : Messages.NONE);
            }
            s.AddEntry(Messages.CONTAINER_IMAGE, dockerContainer.image.Length != 0 ? dockerContainer.image : Messages.NONE);
            s.AddEntry(Messages.CONTAINER, dockerContainer.container.Length != 0 ? dockerContainer.container : Messages.NONE);
            s.AddEntry(Messages.CONTAINER_COMMAND, dockerContainer.command.Length != 0 ? dockerContainer.command : Messages.NONE);
            var ports = dockerContainer.PortList.Select(p => p.Description).ToList();
            if (ports.Count > 0)
                s.AddEntry(Messages.CONTAINER_PORTS, string.Join(Environment.NewLine, ports));

            s.AddEntry(Messages.UUID, dockerContainer.uuid.Length != 0 ? dockerContainer.uuid : Messages.NONE);
        }

        private void GenerateReadCachingBox()
        {
            VM vm = xenObject as VM;
            if (vm == null || !vm.IsRunning())
                return;

            PDSection s = pdSectionReadCaching;

            var pvsProxy = vm.PvsProxy();
            if (pvsProxy != null)
                s.AddEntry(FriendlyName("VM.pvs_read_caching_status"), pvs_proxy_status_extensions.ToFriendlyString(pvsProxy.status));
            else if (vm.ReadCachingEnabled())
            {
                s.AddEntry(FriendlyName("VM.read_caching_status"), Messages.VM_READ_CACHING_ENABLED);
                var vdiList = vm.ReadCachingVDIs().Select(vdi => vdi.NameWithLocation()).ToArray();
                s.AddEntry(FriendlyName("VM.read_caching_disks"), string.Join("\n", vdiList));
            }
            else
            {
                s.AddEntry(FriendlyName("VM.read_caching_status"), Messages.VM_READ_CACHING_DISABLED);
                var reason = vm.ReadCachingDisabledReason();
                if (reason != null)
                    s.AddEntry(FriendlyName("VM.read_caching_reason"), reason);
            }
        }

        private void GenerateDeviceSecurityBox()
        {
            if (!(xenObject is VM vm) || Helpers.FeatureForbidden(vm, Host.RestrictVtpm) ||
                !Helpers.XapiEqualOrGreater_22_26_0(vm.Connection))
                return;

            PDSection s = pdSectionDeviceSecurity;

            if (vm.VTPMs.Count > 0)
            {
                s.AddEntry(Messages.VTPM,
                    vm.VTPMs.Count == 1 ? Messages.VTPM_ATTACHED_ONE : string.Format(Messages.VTPM_ATTACHED_MANY, vm.VTPMs.Count),
                    new CommandToolStripMenuItem(new VtpmCommand(Program.MainWindow, vm), true));
            }
        }

        private static bool VMCanChooseHomeServer(VM vm)
        {
            if (vm != null && !vm.is_a_template)
            {
                String ChangeHomeReason = vm.IsOnSharedStorage();

                return !Helpers.WlbEnabledAndConfigured(vm.Connection) &&
                    (String.IsNullOrEmpty(ChangeHomeReason) || vm.HasNoDisksAndNoLocalCD());
            }
            return false;
        }


        private void GenTagRow(PDSection s)
        {
            string[] tags = Tags.GetTags(xenObject);

            if (tags != null && tags.Length > 0)
            {
                ToolStripMenuItem goToTag = new ToolStripMenuItem(Messages.VIEW_TAG_MENU_OPTION);

                foreach (string tag in tags)
                {
                    var item = new ToolStripMenuItem(tag.Ellipsise(30));
                    item.Click += delegate { Program.MainWindow.SearchForTag(tag); };
                    goToTag.DropDownItems.Add(item);
                }

                s.AddEntry(Messages.TAGS, TagsString(), new[] { goToTag, new PropertiesToolStripMenuItem(new PropertiesCommand(Program.MainWindow, xenObject)) });
                return;
            }

            s.AddEntry(Messages.TAGS, Messages.NONE, new PropertiesToolStripMenuItem(new PropertiesCommand(Program.MainWindow, xenObject)));
        }

        private string TagsString()
        {
            string[] tags = Tags.GetTags(xenObject);
            if (tags == null || tags.Length == 0)
                return Messages.NONE;

            return string.Join(", ", tags.OrderBy(s => s).ToArray());
        }

        private void GenFolderRow(PDSection s)
        {
            var folderValue = new FolderListItem(xenObject.Path, FolderListItem.AllowSearch.None, false);
            var propertiesItem = new PropertiesToolStripMenuItem(new PropertiesCommand(Program.MainWindow, xenObject));

            if (xenObject.Path == "")
            {
                s.AddEntry(Messages.FOLDER, folderValue, propertiesItem);
            }
            else
            {
                var folderItem = new ToolStripMenuItem(Messages.VIEW_FOLDER_MENU_OPTION);
                folderItem.Click += (sender, args) => Program.MainWindow.SearchForFolder(xenObject.Path);
                s.AddEntry(Messages.FOLDER, folderValue, folderItem, propertiesItem);
            }
        }

        private void GenerateMemoryBox()
        {
            Host host = xenObject as Host;
            if (host == null)
                return;

            PDSection s = pdSectionMemory;


            s.AddEntry(FriendlyName("host.ServerMemory"), host.HostMemoryString());
            s.AddEntry(FriendlyName("host.VMMemory"), host.ResidentVMMemoryUsageString());
            s.AddEntry(FriendlyName("host.XenMemory"), host.XenMemoryString());

        }

        private void addStringEntry(PDSection s, string key, string value)
        {
            s.AddEntry(key, string.IsNullOrEmpty(value) ? Messages.NONE : value);
        }

        private void GenerateDockerInfoBox()
        {
            if (!(xenObject is VM vm) || Helpers.StockholmOrGreater(xenObject.Connection))
                return;

            VM_Docker_Info info = vm.DockerInfo();
            if (info == null)
                return;

            VM_Docker_Version version = vm.DockerVersion();
            if (version == null)
                return;

            PDSection s = pdSectionDockerInfo;

            addStringEntry(s, Messages.DOCKER_INFO_API_VERSION, version.ApiVersion);
            addStringEntry(s, Messages.DOCKER_INFO_VERSION, version.Version);
            addStringEntry(s, Messages.DOCKER_INFO_GIT_COMMIT, version.GitCommit);
            addStringEntry(s, Messages.DOCKER_INFO_DRIVER, info.Driver);
            addStringEntry(s, Messages.DOCKER_INFO_INDEX_SERVER_ADDRESS, info.IndexServerAddress);
            addStringEntry(s, Messages.DOCKER_INFO_EXECUTION_DRIVER, info.ExecutionDriver);
            addStringEntry(s, Messages.DOCKER_INFO_IPV4_FORWARDING, info.IPv4Forwarding);
        }

        private bool CPUsIdentical(IEnumerable<Host_cpu> cpus)
        {
            String cpuText = null;
            foreach (Host_cpu cpu in cpus)
            {
                if (cpuText == null)
                {
                    cpuText = Helpers.GetCPUProperties(cpu);
                    continue;
                }
                if (Helpers.GetCPUProperties(cpu) != cpuText)
                    return false;
            }
            return true;
        }

        private void SetupDeprecationBanner()
        {
            if (xenObject is DockerContainer && !Helpers.ContainerCapability(xenObject.Connection))
            {
                Banner.BannerType = DeprecationBanner.Type.Removal;
                Banner.WarningMessage = string.Format(Messages.CONTAINER_MANAGEMENT_REMOVAL_WARNING, BrandManager.ProductVersion82);
                Banner.LinkText = Messages.DETAILS;
                Banner.LinkUri = new Uri(InvisibleMessages.DEPRECATION_URL);
                Banner.Visible = true;
            }
            else if (xenObject is SR sr && sr.GetSRType(true) == SR.SRTypes.lvmofcoe)
            {
                Banner.BannerType = DeprecationBanner.Type.Deprecation;
                Banner.WarningMessage = string.Format(
                    Messages.FCOE_DEPRECATION_WARNING, string.Format(Messages.STRING_SPACE_STRING, BrandManager.ProductBrand, BrandManager.ProductVersionPost82));
                Banner.LinkText = Messages.PATCHING_WIZARD_WEBPAGE_CELL;
                Banner.LinkUri = new Uri(InvisibleMessages.DEPRECATION_URL);
                Banner.Visible = true;
            }
            else
            {
                Banner.Visible = false;
            }
        }

        #region VM delegates

        private static string HVMBootOrder(VM vm)
        {
            var order = vm.GetBootOrder().ToUpper().Union(new[] { 'D', 'C', 'N' });
            return string.Join("\n", order.Select(c => new BootDevice(c).ToString()).ToArray());
        }

        #endregion

        /// <summary>
        /// Checks for reboot warnings on all hosts in the pool and returns them as a list
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        private List<KeyValuePair<String, String>> CheckPoolUpdate(Pool pool)
        {
            List<KeyValuePair<String, String>> warnings = new List<KeyValuePair<string, string>>();

            if (Helpers.ElyOrGreater(pool.Connection))
            {
                // As of Ely we use CheckHostUpdatesRequiringReboot to get reboot messages for a host
                foreach (Host host in xenObject.Connection.Cache.Hosts)
                {
                    warnings.AddRange(CheckHostUpdatesRequiringReboot(host));
                }
            }
            else
            {
                // Earlier versions use the old server updates method
                foreach (Host host in xenObject.Connection.Cache.Hosts)
                {
                    warnings.AddRange(CheckServerUpdates(host));
                }
            }
            return warnings;
        }

        /// <summary>
        /// Checks the server has been restarted after any patches that require a restart were applied and returns a list of reboot warnings
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private List<KeyValuePair<String, String>> CheckServerUpdates(Host host)
        {
            List<Pool_patch> patches = host.AppliedPatches();
            List<KeyValuePair<String, String>> warnings = new List<KeyValuePair<String, String>>();
            double bootTime = host.BootTime();
            double agentStart = host.AgentStartTime();

            if (bootTime == 0.0 || agentStart == 0.0)
                return warnings;

            foreach (Pool_patch patch in patches)
            {
                double applyTime = Util.ToUnixTime(patch.AppliedOn(host));

                if (patch.after_apply_guidance.Contains(after_apply_guidance.restartHost) && applyTime > bootTime
                    || patch.after_apply_guidance.Contains(after_apply_guidance.restartXAPI) && applyTime > agentStart)
                {
                    warnings.Add(CreateWarningRow(host, patch));
                }
            }
            return warnings;
        }

        /// <summary>
        /// Creates a list of warnings for updates that require the host to be rebooted
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private List<KeyValuePair<String, String>> CheckHostUpdatesRequiringReboot(Host host)
        {
            var warnings = new List<KeyValuePair<String, String>>();

            // Updates that require host restart
            var updateRefs = host.updates_requiring_reboot;
            foreach (var updateRef in updateRefs)
            {
                var update = host.Connection.Resolve(updateRef);
                if (update != null)
                    warnings.Add(CreateWarningRow(host, update));
            }

            // For Toolstack restart, legacy code has to be used to determine this - pool_patches are still populated for backward compatibility
            List<Pool_patch> patches = host.AppliedPatches();
            double bootTime = host.BootTime();
            double agentStart = host.AgentStartTime();

            if (bootTime == 0.0 || agentStart == 0.0)
                return warnings;

            foreach (Pool_patch patch in patches)
            {
                double applyTime = Util.ToUnixTime(patch.AppliedOn(host));

                if (patch.after_apply_guidance.Contains(after_apply_guidance.restartXAPI)
                    && applyTime > agentStart)
                {
                    warnings.Add(CreateWarningRow(host, patch));
                }
            }

            return warnings;
        }

        private KeyValuePair<string, string> CreateWarningRow(Host host, Pool_patch patch)
        {
            var key = String.Format(Messages.GENERAL_PANEL_UPDATE_KEY, patch.Name(), host.Name());
            string value = string.Empty;

            if (patch.after_apply_guidance.Contains(after_apply_guidance.restartHost))
            {
                value = string.Format(Messages.GENERAL_PANEL_UPDATE_REBOOT_WARNING, host.Name(), patch.Name());
            }
            else if (patch.after_apply_guidance.Contains(after_apply_guidance.restartXAPI))
            {
                value = string.Format(Messages.GENERAL_PANEL_UPDATE_RESTART_TOOLSTACK_WARNING, host.Name(), patch.Name());
            }

            return new KeyValuePair<string, string>(key, value);
        }

        private KeyValuePair<string, string> CreateWarningRow(Host host, Pool_update update)
        {
            var key = String.Format(Messages.GENERAL_PANEL_UPDATE_KEY, Helpers.UpdatesFriendlyName(update.Name()), host.Name());
            var value = string.Format(Messages.GENERAL_PANEL_UPDATE_REBOOT_WARNING, host.Name(), Helpers.UpdatesFriendlyName(update.Name()));

            return new KeyValuePair<string, string>(key, value);
        }

        private static string FriendlyName(string propertyName)
        {
            return FriendlyNameManager.GetFriendlyName(string.Format("Label-{0}", propertyName)) ?? propertyName;
        }


        #region Control Event Handlers

        private void s_ContentChangedSelection(PDSection s)
        {
            ScrollToSelectionIfNeeded(s);
        }

        private void s_ContentReceivedFocus(PDSection s)
        {
            ScrollToSelectionIfNeeded(s);
        }

        private void s_ExpandedChanged(PDSection pdSection)
        {
            if (xenObject != null)
                _expandedSections[xenObject.GetType()] = sections.Where(s => s.Parent.Visible && s.IsExpanded).ToList();

            linkLabelExpand.Enabled = sections.Any(s => s.Parent.Visible && !s.IsExpanded);
            linkLabelCollapse.Enabled = sections.Any(s => s.Parent.Visible && s.IsExpanded);
        }

        private void linkLabelExpand_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ToggleExpandedState(s => true);
        }

        private void linkLabelCollapse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ToggleExpandedState(s => false);
        }

        private void buttonProperties_Click(object sender, EventArgs e)
        {
            new PropertiesCommand(Program.MainWindow, xenObject).Run();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            //Force scrollbar to be repainted to avoid occasional pixel glitch.
            panel2.AutoScroll = sections.Sum(s => s.Parent?.Height ?? s.Height) > panel2.ClientRectangle.Height;
        }

        #endregion

        private void ToggleExpandedState(Func<PDSection, bool> expand)
        {
            try
            {
                panel2.SuspendLayout();

                foreach (PDSection s in sections)
                {
                    if (!s.Parent.Visible)
                        continue;

                    try
                    {
                        s.DisableFocusEvent = true;

                        if (expand(s))
                            s.Expand();
                        else
                            s.Collapse();
                    }
                    finally
                    {
                        s.DisableFocusEvent = false;
                    }
                }
            }
            finally
            {
                panel2.ResumeLayout();
            }
        }
    }
}
