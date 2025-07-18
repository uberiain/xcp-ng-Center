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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using XenAdmin.Alerts;
using XenAdmin.Core;
using XenAdmin.Model;
using XenAdmin.Network;
using XenAdmin.XCM;
using XenAdmin.XenSearch;
using XenAPI;


namespace XenAdmin
{
    static class Images
    {
        public static readonly ImageList ImageList16 = new ImageList();
        /// <summary>
        /// This is the same images as in ImageList16, but in an array.
        /// Reading from the ImageList is strangely slow.
        /// </summary>
        private static readonly Image[] ImageArray16 = new Image[Enum.GetValues(typeof(Icons)).Length];

        static Images()
        {
            ImageList16.ColorDepth = ColorDepth.Depth32Bit;
            ImageList16.TransparentColor = Color.Transparent;

            // Initialize our ImageList. We do this programatically (as opposed to using the designer) because
            // of a known bug in ImageList with transparency. 
            // See http://www.codeproject.com/cs/miscctrl/AlphaImageImagelist.asp?df=100&forumid=137678&exp=0&select=1392020#xx1392020xx
            // To workaround the bug, we just add the images from the resource file.
            // Note that this list is in the same order as the enum in Icons.cs.
            ImageList16.Images.Add("Logo.png", XenAdmin.Properties.Resources.Logo);

            ImageList16.Images.Add("000_ServerInProgress_h32bit_16.png", XenAdmin.Properties.Resources._000_ServerInProgress_h32bit_16);
            ImageList16.Images.Add("000_TreeConnected_h32bit_16.png", XenAdmin.Properties.Resources._000_TreeConnected_h32bit_16);
            ImageList16.Images.Add("000_ServerDisconnected_h32bit_16.png", XenAdmin.Properties.Resources._000_ServerDisconnected_h32bit_16);
            ImageList16.Images.Add("000_ServerMaintenance_h32bit_16.png", XenAdmin.Properties.Resources._000_ServerMaintenance_h32bit_16);
            ImageList16.Images.Add("000_HostUnpatched_h32bit_16.png", XenAdmin.Properties.Resources._000_HostUnpatched_h32bit_16);
            ImageList16.Images.Add("server_up_16.png", XenAdmin.Properties.Resources.server_up_16);
            ImageList16.Images.Add("000_ServerErrorFile_h32bit_16.png", XenAdmin.Properties.Resources._000_ServerErrorFile_h32bit_16);
            ImageList16.Images.Add("000_Server_h32bit_16-w-alert.png", Properties.Resources._000_Server_h32bit_16_w_alert);

            ImageList16.Images.Add("000_StartVM_h32bit_16.png", XenAdmin.Properties.Resources._000_StartVM_h32bit_16);
            ImageList16.Images.Add("000_VMDisabled_h32bit_16.png", XenAdmin.Properties.Resources._000_VMDisabled_h32bit_16);
            ImageList16.Images.Add("000_StoppedVM_h32bit_16.png", XenAdmin.Properties.Resources._000_StoppedVM_h32bit_16);
            ImageList16.Images.Add("000_VMStoppedDisabled_h32bit_16.png", XenAdmin.Properties.Resources._000_VMStoppedDisabled_h32bit_16);
            ImageList16.Images.Add("000_SuspendVM_h32bit_16.png", XenAdmin.Properties.Resources._000_SuspendVM_h32bit_16);
            ImageList16.Images.Add("000_VMPausedDisabled_h32bit_16.png", XenAdmin.Properties.Resources._000_VMPausedDisabled_h32bit_16);
            ImageList16.Images.Add("000_SuspendVM_h32bit_16_green.png", XenAdmin.Properties.Resources._000_SuspendVM_h32bit_16_green);
            ImageList16.Images.Add("000_VMPausedDisabled_h32bit_16_green.png", XenAdmin.Properties.Resources._000_VMPausedDisabled_h32bit_16_green);
            ImageList16.Images.Add("000_VMStarting_h32bit_16.png", XenAdmin.Properties.Resources._000_VMStarting_h32bit_16);
            ImageList16.Images.Add("000_VMStartingDisabled_h32bit_16.png", XenAdmin.Properties.Resources._000_VMStartingDisabled_h32bit_16);

            ImageList16.Images.Add("000_VMTemplate_h32bit_16.png", XenAdmin.Properties.Resources._000_VMTemplate_h32bit_16);
            ImageList16.Images.Add("000_TemplateDisabled_h32bit_16.png", XenAdmin.Properties.Resources._000_TemplateDisabled_h32bit_16);
            ImageList16.Images.Add("000_UserTemplate_h32bit_16.png", XenAdmin.Properties.Resources._000_UserTemplate_h32bit_16);
            ImageList16.Images.Add("000_VMSession_h32bit_16.png", XenAdmin.Properties.Resources._000_VMSession_h32bit_16);
            ImageList16.Images.Add("000_VMSnapShotDiskOnly_h32bit_16.png", XenAdmin.Properties.Resources._000_VMSnapShotDiskOnly_h32bit_16);
            ImageList16.Images.Add("000_VMSnapshotDiskMemory_h32bit_16.png", XenAdmin.Properties.Resources._000_VMSnapshotDiskMemory_h32bit_16);
            ImageList16.Images.Add("_000_ScheduledVMsnapshotDiskOnly_h32bit_16.png", XenAdmin.Properties.Resources._000_ScheduledVMsnapshotDiskOnly_h32bit_16);
            ImageList16.Images.Add("_000_ScheduledVMsnapshotDiskMemory_h32bit_16.png", XenAdmin.Properties.Resources._000_ScheduledVMsnapshotDiskMemory_h32bit_16);

            ImageList16.Images.Add("000_PoolConnected_h32bit_16.png", XenAdmin.Properties.Resources._000_PoolConnected_h32bit_16);
            ImageList16.Images.Add("pool_up_16.png", XenAdmin.Properties.Resources.pool_up_16);
            ImageList16.Images.Add("pool_unpatched.png", Properties.Resources.pool_unpatched);
            ImageList16.Images.Add("000_Pool_h32bit_16-w-alert.png", Properties.Resources._000_Pool_h32bit_16_w_alert);

            ImageList16.Images.Add("000_Storage_h32bit_16.png", XenAdmin.Properties.Resources._000_Storage_h32bit_16);
            ImageList16.Images.Add("000_StorageBroken_h32bit_16.png", XenAdmin.Properties.Resources._000_StorageBroken_h32bit_16);
            ImageList16.Images.Add("000_StorageDefault_h32bit_16.png", XenAdmin.Properties.Resources._000_StorageDefault_h32bit_16);
            ImageList16.Images.Add("000_StorageDisabled_h32bit_16.png", XenAdmin.Properties.Resources._000_StorageDisabled_h32bit_16);

            ImageList16.Images.Add("001_ShutDown_h32bit_16.png", Properties.Resources._001_ShutDown_h32bit_16);
            ImageList16.Images.Add("000_paused_h32bit_16.png", Properties.Resources._000_paused_h32bit_16);
            ImageList16.Images.Add("000_paused_h32bit_16_green.png", Properties.Resources._000_paused_h32bit_16_green);
            ImageList16.Images.Add("001_PowerOn_h32bit_16.png", Properties.Resources._001_PowerOn_h32bit_16);
            ImageList16.Images.Add("000_HelpIM_h32bit_16.png", Properties.Resources._000_HelpIM_h32bit_16);

            ImageList16.Images.Add("000_Network_h32bit_16.png", Properties.Resources._000_Network_h32bit_16);

            ImageList16.Images.Add("000_defaultSpyglass_h32bit_16.png", Properties.Resources._000_defaultSpyglass_h32bit_16);
            ImageList16.Images.Add("000_Search_h32bit_16.png", Properties.Resources._000_Search_h32bit_16);

            #region Server message images

            ImageList16.Images.Add("001_PowerOn_h32bit_16.png", Properties.Resources._001_PowerOn_h32bit_16);
            ImageList16.Images.Add("001_ShutDown_h32bit_16.png", Properties.Resources._001_ShutDown_h32bit_16);
            ImageList16.Images.Add("001_Reboot_h32bit_16.png", Properties.Resources._001_Reboot_h32bit_16);
            ImageList16.Images.Add("000_paused_h32bit_16.png", Properties.Resources._000_paused_h32bit_16);
            ImageList16.Images.Add("000_Resumed_h32bit_16.png", Properties.Resources._000_Resumed_h32bit_16);
            ImageList16.Images.Add("clonevm_16.png", Properties.Resources.clonevm_16);

            ImageList16.Images.Add("Logo.png", Properties.Resources.Logo);
            ImageList16.Images.Add("alert1_16.png", Properties.Resources.alert1_16);
            ImageList16.Images.Add("alert2_16.png", Properties.Resources.alert2_16);
            ImageList16.Images.Add("alert3_16.png", Properties.Resources.alert3_16);
            ImageList16.Images.Add("alert4_16.png", Properties.Resources.alert4_16);
            ImageList16.Images.Add("alert5_16.png", Properties.Resources.alert5_16);

            #endregion

            ImageList16.Images.Add("centos_16x.png", Properties.Resources.centos_16x);
            ImageList16.Images.Add("debian_16x.png", Properties.Resources.debian_16x);
            ImageList16.Images.Add("gooroom_16x.png", Properties.Resources.gooroom_16x);
            ImageList16.Images.Add("rocky_16x.png", Properties.Resources.rocky_16x);
            ImageList16.Images.Add("linx_16x.png", Properties.Resources.linx_16x);
            ImageList16.Images.Add("oracle_16x.png", Properties.Resources.oracle_16x);
            ImageList16.Images.Add("redhat_16x.png", Properties.Resources.redhat_16x);
            ImageList16.Images.Add("suse_16x.png", Properties.Resources.suse_16x);
            ImageList16.Images.Add("ubuntu_16x.png", Properties.Resources.ubuntu_16x);
            ImageList16.Images.Add("yinhekylin_16x.png", Properties.Resources.yinhekylin_16x);
            ImageList16.Images.Add("scilinux_16x.png", Properties.Resources.scilinux_16x);
            ImageList16.Images.Add("neokylin_16x.png", Properties.Resources.neokylin_16x);
            ImageList16.Images.Add("asianux_16x.png", Properties.Resources.asianux_16x);
            ImageList16.Images.Add("turbo_16x.png", Properties.Resources.turbo_16x);
            ImageList16.Images.Add("windows_h32bit_16.png", Properties.Resources.windows_h32bit_16);
            ImageList16.Images.Add("coreos-globe-icon.png", Properties.Resources.coreos_globe_icon);

            ImageList16.Images.Add("tools_uptodate_16x.png", Properties.Resources.tools_uptodate_16x);
            ImageList16.Images.Add("tools_notinstalled_16x.png", Properties.Resources.tools_notinstalled_16x);
            ImageList16.Images.Add("tools_outofdate_16x.png", Properties.Resources.tools_outofdate_16x);

            ImageList16.Images.Add("000_VM_h32bit_16.png", Properties.Resources._000_VM_h32bit_16);
            ImageList16.Images.Add("000_Server_h32bit_16.png", Properties.Resources._000_Server_h32bit_16);
            ImageList16.Images.Add("000_Pool_h32bit_16.png", Properties.Resources._000_Pool_h32bit_16);

            ImageList16.Images.Add("000_VirtualStorage_h32bit_16.png", Properties.Resources._000_VirtualStorage_h32bit_16);
            ImageList16.Images.Add("virtualstorage_snapshot_16.png", Properties.Resources.virtualstorage_snapshot_16);

            ImageList16.Images.Add("000_Folder_open_h32bit_16.png", Properties.Resources._000_Folder_open_h32bit_16);
            ImageList16.Images.Add("folder_grey.png", Properties.Resources.folder_grey);

            ImageList16.Images.Add("000_Tag_h32bit_16.png", Properties.Resources._000_Tag_h32bit_16);
            ImageList16.Images.Add("000_Fields_h32bit_16.png", Properties.Resources._000_Fields_h32bit_16);
            ImageList16.Images.Add("ha_16.png", Properties.Resources.ha_16);

            ImageList16.Images.Add("virtualappliance_16.png", Properties.Resources._000_VirtualAppliance_h32bit_16);

            ImageList16.Images.Add("000_MigrateVM_h32bit_16.png", Properties.Resources._000_MigrateVM_h32bit_16);
            ImageList16.Images.Add("000_MigrateStoppedVM_h32bit_16.png", Properties.Resources._000_MigrateStoppedVM_h32bit_16);
            ImageList16.Images.Add("000_MigrateSuspendedVM_h32bit_16.png", Properties.Resources._000_MigrateSuspendedVM_h32bit_16);

            ImageList16.Images.Add("_000_ManagementInterface_h32bit_16.png", Properties.Resources._000_ManagementInterface_h32bit_16);
            ImageList16.Images.Add("000_TCP_IPGroup_h32bit_16.png", Properties.Resources._000_TCP_IPGroup_h32bit_16);
            ImageList16.Images.Add("infra_view_16_textured.png", Properties.Resources.infra_view_16_textured);
            ImageList16.Images.Add("objects_16_textured.png", Properties.Resources.objects_16_textured);

            ImageList16.Images.Add("000_Sites_h32bit_16.png", Properties.Resources._000_Sites_h32bit_16);

            ImageList16.Images.Add("RunningDC_16.png", Properties.Resources.RunningDC_16);
            ImageList16.Images.Add("StoppedDC_16.png", Properties.Resources.StoppedDC_16);
            ImageList16.Images.Add("PausedDC_16.png", Properties.Resources.PausedDC_16);

            ImageList16.Images.Add("usb_16.png", Properties.Resources.usb_16);

            #region Status Icons
            ImageList16.Images.Add("000_Tick_h32bit_16", Properties.Resources._000_Tick_h32bit_16); //Ok
            ImageList16.Images.Add("000_Info3_h32bit_16.png", Properties.Resources._000_Info3_h32bit_16); //Info
            ImageList16.Images.Add("000_Alert2_h32bit_16.png", Properties.Resources._000_Alert2_h32bit_16); //Warning
            ImageList16.Images.Add("000_Abort_h32bit_16.png", Properties.Resources._000_Abort_h32bit_16); //Error
            #endregion

            System.Diagnostics.Debug.Assert(ImageList16.Images.Count == Enum.GetValues(typeof(Icons)).Length,
                "Programmer error - you must add an entry to the image list when you add a new icon to the enum");

            int i = 0;
            foreach (Image im in ImageList16.Images)
            {
                ImageArray16[i++] = im;
            }
        }

        public static Image GetImage16For(IXenConnection connection)
        {
            Icons icon = GetIconFor(connection);
            return ImageArray16[(int)icon];
        }

        public static Image GetImage16For(IXenObject o)
        {
            Icons icon = GetIconFor(o);
            return ImageArray16[(int)icon];
        }

        public static Image GetImage16For(Search search)
        {
            return GetImage16For(GetIconFor(search));
        }

        public static Image GetImage16For(XenAPI.Message.MessageType type)
        {
            return GetImage16For(GetIconFor(type));
        }

        public static Image GetImage16For(AlertPriority priority)
        {
            var icon = GetIconFor(priority);
            return icon == Icons.MessageUnknown ? null : GetImage16For(icon);
        }

        public static Image GetImage32For(IXenObject o)
        {
            return new Bitmap(GetImage16For(o), 32, 32);
        }

        public static Image GetImage16For(Icons icon)
        {
            return ImageArray16[(int)icon];
        }

        public static Image GetImageForPercentage(int percent)
        {
            return GetImageForPercentage((long)percent);
        }

        public static Image GetImageForPercentage(long percent)
        {
            if (percent < 9)
                return StaticImages.usagebar_0;
            if (percent < 18)
                return StaticImages.usagebar_1;
            if (percent < 27)
                return StaticImages.usagebar_2;
            if (percent < 36)
                return StaticImages.usagebar_3;
            if (percent < 45)
                return StaticImages.usagebar_4;
            if (percent < 54)
                return StaticImages.usagebar_5;
            if (percent < 63)
                return StaticImages.usagebar_6;
            if (percent < 72)
                return StaticImages.usagebar_7;
            if (percent < 81)
                return StaticImages.usagebar_8;
            if (percent < 90)
                return StaticImages.usagebar_9;

            return StaticImages.usagebar_10;
        }

        public static Image GetImageFor(Conversion conversion)
        {
            switch (conversion.Status)
            {
                case (int)ConversionStatus.Successful:
                    return StaticImages._075_TickRound_h32bit_16;
                case (int)ConversionStatus.Failed:
                    return StaticImages._000_error_h32bit_16;
                case (int)ConversionStatus.Cancelled:
                    return Images.StaticImages.cancelled_action_16;
                case (int)ConversionStatus.Incomplete:
                    return Images.StaticImages._075_WarningRound_h32bit_16;
                case (int)ConversionStatus.Running:
                    return Images.GetImageForPercentage(conversion.PercentComplete);
                case (int)ConversionStatus.Created:
                case (int)ConversionStatus.Queued:
                default:
                    return Images.StaticImages.queued;
            }
        }


        public static Icons GetIconFor(IXenObject o)
        {
            VM vm = o as VM;
            if (vm != null)
                return GetIconFor(vm);

            VM_appliance appl = o as VM_appliance;
            if (appl != null)
                return GetIconFor(appl);

            SR sr = o as SR;
            if (sr != null)
                return GetIconFor(sr);

            Host host = o as Host;
            if (host != null)
                return GetIconFor(host);

            Pool pool = o as Pool;
            if (pool != null)
                return GetIconFor(pool);

            XenAPI.Network network = o as XenAPI.Network;
            if (network != null)
                return GetIconFor(network);

            VDI vdi = o as VDI;
            if (vdi != null)
                return GetIconFor(vdi);

            VBD vbd = o as VBD;
            if (vbd != null)
                return GetIconFor(vbd);

            Folder folder = o as Folder;
            if (folder != null)
                return GetIconFor(folder);

            PIF pif = o as PIF;
            if (pif != null)
                return GetIconFor(pif);

            DockerContainer dockerContainer = o as DockerContainer;
            if (dockerContainer != null)
                return GetIconFor(dockerContainer);

            System.Diagnostics.Trace.Assert(false,
                "You asked for an icon for a type I don't recognise!");

            return Icons.XenCenter;
        }

        public static Icons GetIconFor(Search search)
        {
            return search.DefaultSearch ? Icons.DefaultSearch : Icons.Search;
        }

        public static Icons GetIconFor(IXenConnection connection)
        {
            Pool pool = Helpers.GetPool(connection);
            if (pool != null)
                return GetIconFor(pool);

            Host host = Helpers.GetCoordinator(connection);
            if (host != null)
                return GetIconFor(host);

            if (connection.InProgress)
                // Yellow connection in progress icon
                return Icons.HostConnecting;
            else
                // Red disconnected icon
                return Icons.HostDisconnected;
        }

        public static Icons GetIconFor(Folder folder)
        {
            return folder.Grey ? Icons.FolderGrey : Icons.Folder;
        }

        public static Icons GetIconFor(VM_appliance appl)
        {
            return Icons.VmAppliance;
        }

        public static Icons GetIconFor(VM vm)
        {
            bool disabled = vm.IsHidden();

            if (vm.is_a_snapshot)
            {
                if (vm.is_snapshot_from_vmpp || vm.is_vmss_snapshot)
                {
                    if (vm.power_state == vm_power_state.Suspended)
                        return Icons.ScheduledSnapshotDiskMemory;
                    else
                        return Icons.ScheduledSnapshotDiskOnly;
                }
                else
                {
                    if (vm.power_state == vm_power_state.Suspended)
                        return Icons.SnapshotWithMem;
                    else
                        return Icons.SnapshotDisksOnly;
                }
            }

            if (vm.is_a_template && vm.DefaultTemplate())
                return disabled ? Icons.TemplateDisabled : Icons.Template;

            if (vm.is_a_template && !vm.DefaultTemplate())
                return disabled ? Icons.TemplateDisabled : Icons.TemplateUser;

            if (!vm.ExistsOnServer())
                return disabled ? Icons.VmStoppedDisabled : Icons.VmStopped;

            if (vm.current_operations.ContainsValue(vm_operations.migrate_send))
            {
                switch (vm.power_state)
                {
                    case vm_power_state.Halted:
                        return Icons.VmCrossPoolMigrateStopped;
                    case vm_power_state.Suspended:
                    case vm_power_state.Paused:
                        return Icons.VmCrossPoolMigrateSuspended;
                    default:
                        return Icons.VmCrossPoolMigrate;
                }
            }

            // If a VM lifecycle operation is in progress, show the orange "starting" icon
            foreach (vm_operations op in vm.current_operations.Values)
            {
                if (VM.is_lifecycle_operation(op))
                    return disabled ? Icons.VmStartingDisabled : Icons.VmStarting;
            }

            switch (vm.power_state)
            {
                case XenAPI.vm_power_state.Suspended:
                    return disabled ? Icons.VmSuspendedDisabled : Icons.VmSuspended;
                case XenAPI.vm_power_state.Running:
                    return disabled ? Icons.VmRunningDisabled : Icons.VmRunning;
                case XenAPI.vm_power_state.Paused:
                    return disabled ? Icons.VmPausedDisabled : Icons.VmPaused;
                default:
                    return disabled ? Icons.VmStoppedDisabled : Icons.VmStopped;
            }
        }

        public static Icons GetIconFor(SR sr)
        {
            if (!sr.HasPBDs() || sr.IsHidden())
            {
                return Icons.StorageDisabled;
            }
            else if (sr.IsDetached() || sr.IsBroken() || !sr.MultipathAOK())
            {
                return Icons.StorageBroken;
            }
            else if (SR.IsDefaultSr(sr))
            {
                return Icons.StorageDefault;
            }
            else
            {
                return Icons.Storage;
            }
        }

        public static Icons GetIconFor(Host host)
        {
            Host_metrics metrics = host.Connection.Resolve(host.metrics);

            if (metrics != null && metrics.live)
            {
                if (host.IsExpired())
                    return Icons.ServerUnlicensed;

                if (host.HasCrashDumps())
                    return Icons.HostHasCrashDumps;

                if (host.current_operations.ContainsValue(host_allowed_operations.evacuate) || !host.enabled)
                    return Icons.HostEvacuate;

                if (Helpers.IsOlderThanCoordinator(host))
                    return Icons.HostOlderThanCoordinator;

                if (Helpers.CloudOrGreater(host))
                {
                    if (host.Connection.Cache.Hosts.Select(h => h.latest_synced_updates_applied).Distinct().Count() > 1 &&
                        host.latest_synced_updates_applied != latest_synced_updates_applied_state.yes)
                        return Icons.HostUnpatched;
                }
                else
                {
                    var cache = host.Connection.Cache;
                    var allHostCount = host.Connection.Cache.HostCount;

                    foreach (var u in cache.Pool_updates)
                    {
                        var appliedHosts = u.AppliedOnHosts();

                        if (appliedHosts.Count > 0 && appliedHosts.Count != allHostCount &&
                            u.EnforceHomogeneity() && !appliedHosts.Contains(host))
                            return Icons.HostUnpatched;
                    }
                }

                return Icons.HostConnected;
            }

            // XenAdmin.XenSearch.Group puts a fake host in the treeview for disconnected
            // XenConnections. So here we give the yellow 'connection in progress' icon which formerly
            // applied only to XenConnections.
            if (host.Connection.InProgress && !host.Connection.IsConnected)
                return Icons.HostConnecting;
            
            return Icons.HostDisconnected;
        }

        public static Icons GetIconFor(Pool pool)
        {
            if (!pool.Connection.IsConnected)
                return Icons.HostDisconnected;
            if (pool.Connection.Cache.Hosts.Any(h => h.IsExpired()))
                return Icons.PoolUnlicensed;

            if (!pool.IsPoolFullyUpgraded())
                return Icons.PoolNotFullyUpgraded;

            if (Helpers.CloudOrGreater(pool.Connection))
            {
                if (pool.Connection.Cache.Hosts.Select(h => h.latest_synced_updates_applied).Distinct().Count() > 1)
                    return Icons.PoolUnPatched;
            }
            else
            {
                var cache = pool.Connection.Cache;
                var allHostCount = pool.Connection.Cache.HostCount;

                foreach (var u in cache.Pool_updates)
                {
                    var appliedHosts = u.AppliedOnHosts();

                    if (appliedHosts.Count > 0 && appliedHosts.Count != allHostCount && u.EnforceHomogeneity())
                        return Icons.PoolUnPatched;
                }
            }

            return Icons.PoolConnected;
        }

        public static Icons GetIconFor(XenAPI.Network network)
        {
            return Icons.Network;
        }

        public static Icons GetIconFor(VDI vdi)
        {
            if (vdi.is_a_snapshot)
                return Icons.VDISnapshot;
            else
                return Icons.VDI;
        }

        public static Icons GetIconFor(VBD vbd)
        {
            return Icons.VDI;
        }

        public static Icons GetIconFor(XenAPI.Message.MessageType type)
        {
            switch (type)
            {
                case XenAPI.Message.MessageType.VM_STARTED:
                    return Icons.VmStart;

                case XenAPI.Message.MessageType.VM_SHUTDOWN:
                    return Icons.VmShutdown;

                case XenAPI.Message.MessageType.VM_REBOOTED:
                    return Icons.VmReboot;

                case XenAPI.Message.MessageType.VM_SUSPENDED:
                    return Icons.VmSuspend;

                case XenAPI.Message.MessageType.VM_RESUMED:
                    return Icons.VmResumed;

                case XenAPI.Message.MessageType.VM_CLONED:
                    return Icons.VmCloned;

                default:
                    return Icons.MessageUnknown;
            }
        }

        public static Icons GetIconFor(AlertPriority priority)
        {
            switch (priority)
            {
                case AlertPriority.Priority1:
                    return Icons.MessagePriority1;
                case AlertPriority.Priority2:
                    return Icons.MessagePriority2;
                case AlertPriority.Priority3:
                    return Icons.MessagePriority3;
                case AlertPriority.Priority4:
                    return Icons.MessagePriority4;
                case AlertPriority.Priority5:
                    return Icons.MessagePriority5;
                default:
                    return Icons.MessageUnknown;
            }
        }

        public static Icons GetIconFor(PIF pif)
        {
            return pif.IsPrimaryManagementInterface() ? Icons.PifPrimary : Icons.PifSecondary;
        }

        public static Icons GetIconFor(DockerContainer dockerContainer)
        {
            switch (dockerContainer.power_state)
            {
                case vm_power_state.Paused:
                    return Icons.DCPaused;
                case vm_power_state.Running:
                    return Icons.DCRunning;
                default:
                    return Icons.DCStopped;
            }
        }

        public static class StaticImages
        {
            // the following are generated from Resources using:
            // cat Resources.Designer.cs | grep 'internal static System.Drawing.Bitmap' | sed "s/ {//g" | awk -F"Bitmap " '{print"public static Bitmap " $2 " = Properties.Resources." $2 ";"}'
            public static Bitmap _000_Abort_h32bit_16 = Properties.Resources._000_Abort_h32bit_16;
            public static Bitmap _000_Abort_gray_h32bit_16 = Properties.Resources._000_Abort_gray_h32bit_16;
            public static Bitmap _000_AddApplicationServer_h32bit_16 = Properties.Resources._000_AddApplicationServer_h32bit_16;
            public static Bitmap _000_AddApplicationServer_h32bit_24 = Properties.Resources._000_AddApplicationServer_h32bit_24;
            public static Bitmap _000_AddIPAddress_h32bit_16 = Properties.Resources._000_AddIPAddress_h32bit_16;
            public static Bitmap _000_AddSite_h32bit_16 = Properties.Resources._000_AddSite_h32bit_16;
            public static Bitmap _000_Alert2_h32bit_16 = Properties.Resources._000_Alert2_h32bit_16;
            public static Bitmap _000_BackupMetadata_h32bit_16 = Properties.Resources._000_BackupMetadata_h32bit_16;
            public static Bitmap _000_BackupMetadata_h32bit_32 = Properties.Resources._000_BackupMetadata_h32bit_32;
            public static Bitmap _000_ConfigureIPAddresses_h32bit_16 = Properties.Resources._000_ConfigureIPAddresses_h32bit_16;
            public static Bitmap _000_CPU_h32bit_16 = Properties.Resources._000_CPU_h32bit_16;
            public static Bitmap _000_CreateVirtualStorage_h32bit_32 = Properties.Resources._000_CreateVirtualStorage_h32bit_32;
            public static Bitmap _000_CreateVM_h32bit_24 = Properties.Resources._000_CreateVM_h32bit_24;
            public static Bitmap _000_CreateVM_h32bit_32 = Properties.Resources._000_CreateVM_h32bit_32;
            public static Bitmap _000_defaultSpyglass_h32bit_16 = Properties.Resources._000_defaultSpyglass_h32bit_16;
            public static Bitmap _000_DeleteAllMessages_h32bit_16 = Properties.Resources._000_DeleteAllMessages_h32bit_16;
            public static Bitmap _000_DeleteMessage_h32bit_16 = Properties.Resources._000_DeleteMessage_h32bit_16;
            public static Bitmap _000_DeleteVirtualAppliance_h32bit_16 = Properties.Resources._000_DeleteVirtualAppliance_h32bit_16;
            public static Bitmap _000_DisasterRecovery_h32bit_32 = Properties.Resources._000_DisasterRecovery_h32bit_32;
            public static Bitmap _000_Email_h32bit_16 = Properties.Resources._000_Email_h32bit_16;
            public static Bitmap _000_EnablePowerControl_h32bit_16 = Properties.Resources._000_EnablePowerControl_h32bit_16;
            public static Bitmap _000_error_h32bit_16 = Properties.Resources._000_error_h32bit_16;
            public static Bitmap _000_error_h32bit_32 = Properties.Resources._000_error_h32bit_32;
            public static Bitmap _000_ExcludeHost_h32bit_16 = Properties.Resources._000_ExcludeHost_h32bit_16;
            public static Bitmap _000_ExportMessages_h32bit_16 = Properties.Resources._000_ExportMessages_h32bit_16;
            public static Bitmap _000_ExportVirtualAppliance_h32bit_16 = Properties.Resources._000_ExportVirtualAppliance_h32bit_16;
            public static Bitmap _000_ExportVirtualAppliance_h32bit_32 = Properties.Resources._000_ExportVirtualAppliance_h32bit_32;
            public static Bitmap _000_Failback_h32bit_32 = Properties.Resources._000_Failback_h32bit_32;
            public static Bitmap _000_Failover_h32bit_32 = Properties.Resources._000_Failover_h32bit_32;
            public static Bitmap _000_Fields_h32bit_16 = Properties.Resources._000_Fields_h32bit_16;
            public static Bitmap _000_FilterDates_h32bit_16 = Properties.Resources._000_FilterDates_h32bit_16;
            public static Bitmap _000_FilterServer_h32bit_16 = Properties.Resources._000_FilterServer_h32bit_16;
            public static Bitmap _000_FilterSeverity_h32bit_16 = Properties.Resources._000_FilterSeverity_h32bit_16;
            public static Bitmap _000_Folder_open_h32bit_16 = Properties.Resources._000_Folder_open_h32bit_16;
            public static Bitmap _000_GetMemoryInfo_h32bit_16 = Properties.Resources._000_GetMemoryInfo_h32bit_16;
            public static Bitmap _000_GetMemoryInfo_h32bit_32 = Properties.Resources._000_GetMemoryInfo_h32bit_32;
            public static Bitmap _000_GetServerReport_h32bit_16 = Properties.Resources._000_GetServerReport_h32bit_16;
            public static Bitmap _000_GetServerReport_h32bit_32 = Properties.Resources._000_GetServerReport_h32bit_32;
            public static Bitmap _000_HAServer_h32bit_32 = Properties.Resources._000_HAServer_h32bit_32;
            public static Bitmap _000_HelpIM_h32bit_16 = Properties.Resources._000_HelpIM_h32bit_16;
            public static Bitmap _000_HelpIM_h32bit_32 = Properties.Resources._000_HelpIM_h32bit_32;
            public static Bitmap _000_HighlightVM_h32bit_24 = Properties.Resources._000_HighlightVM_h32bit_24;
            public static Bitmap _000_HighLightVM_h32bit_32 = Properties.Resources._000_HighLightVM_h32bit_32;
            public static Bitmap _000_host_0_star = Properties.Resources._000_host_0_star;
            public static Bitmap _000_host_1_star = Properties.Resources._000_host_1_star;
            public static Bitmap _000_host_10_star = Properties.Resources._000_host_10_star;
            public static Bitmap _000_host_2_star = Properties.Resources._000_host_2_star;
            public static Bitmap _000_host_3_star = Properties.Resources._000_host_3_star;
            public static Bitmap _000_host_4_star = Properties.Resources._000_host_4_star;
            public static Bitmap _000_host_5_star = Properties.Resources._000_host_5_star;
            public static Bitmap _000_host_6_star = Properties.Resources._000_host_6_star;
            public static Bitmap _000_host_7_star = Properties.Resources._000_host_7_star;
            public static Bitmap _000_host_8_star = Properties.Resources._000_host_8_star;
            public static Bitmap _000_host_9_star = Properties.Resources._000_host_9_star;
            public static Bitmap _000_HostUnpatched_h32bit_16 = Properties.Resources._000_HostUnpatched_h32bit_16;
            public static Bitmap _000_ImportVirtualAppliance_h32bit_16 = Properties.Resources._000_ImportVirtualAppliance_h32bit_16;
            public static Bitmap _000_ImportVirtualAppliance_h32bit_32 = Properties.Resources._000_ImportVirtualAppliance_h32bit_32;
            public static Bitmap _000_ImportVM_h32bit_32 = Properties.Resources._000_ImportVM_h32bit_32;
            public static Bitmap _000_Info3_h32bit_16 = Properties.Resources._000_Info3_h32bit_16;
            public static Bitmap _000_ManagementInterface_h32bit_16 = Properties.Resources._000_ManagementInterface_h32bit_16;
            public static Bitmap _000_MigrateStoppedVM_h32bit_16 = Properties.Resources._000_MigrateStoppedVM_h32bit_16;
            public static Bitmap _000_MigrateSuspendedVM_h32bit_16 = Properties.Resources._000_MigrateSuspendedVM_h32bit_16;
            public static Bitmap _000_MigrateVM_h32bit_16 = Properties.Resources._000_MigrateVM_h32bit_16;
            public static Bitmap _000_MigrateVM_h32bit_32 = Properties.Resources._000_MigrateVM_h32bit_32;
            public static Bitmap _000_Module_h32bit_16 = Properties.Resources._000_Module_h32bit_16;
            public static Bitmap _000_Network_h32bit_16 = Properties.Resources._000_Network_h32bit_16;
            public static Bitmap _000_NewNetwork_h32bit_32 = Properties.Resources._000_NewNetwork_h32bit_32;
            public static Bitmap _000_NewStorage_h32bit_16 = Properties.Resources._000_NewStorage_h32bit_16;
            public static Bitmap _000_NewStorage_h32bit_24 = Properties.Resources._000_NewStorage_h32bit_24;
            public static Bitmap _000_NewStorage_h32bit_32 = Properties.Resources._000_NewStorage_h32bit_32;
            public static Bitmap _000_NewVirtualAppliance_h32bit_16 = Properties.Resources._000_NewVirtualAppliance_h32bit_16;
            public static Bitmap _000_NewVirtualAppliance_h32bit_32 = Properties.Resources._000_NewVirtualAppliance_h32bit_32;
            public static Bitmap _000_Optimize_h32bit_16 = Properties.Resources._000_Optimize_h32bit_16;
            public static Bitmap _000_Patch_h32bit_16 = Properties.Resources._000_Patch_h32bit_16;
            public static Bitmap _000_Patch_h32bit_32 = Properties.Resources._000_Patch_h32bit_32;
            public static Bitmap _000_paused_h32bit_16 = Properties.Resources._000_paused_h32bit_16;
            public static Bitmap _000_Paused_h32bit_24 = Properties.Resources._000_Paused_h32bit_24;
            public static Bitmap _000_paused_h32bit_16_green = Properties.Resources._000_paused_h32bit_16_green;
            public static Bitmap _000_Paused_h32bit_24_green = Properties.Resources._000_Paused_h32bit_24_green;
            public static Bitmap _000_Pool_h32bit_16 = Properties.Resources._000_Pool_h32bit_16;
            public static Bitmap _000_Pool_h32bit_16_w_alert = Properties.Resources._000_Pool_h32bit_16_w_alert;
            public static Bitmap _000_PoolConnected_h32bit_16 = Properties.Resources._000_PoolConnected_h32bit_16;
            public static Bitmap _000_PoolNew_h32bit_16 = Properties.Resources._000_PoolNew_h32bit_16;
            public static Bitmap _000_PoolNew_h32bit_24 = Properties.Resources._000_PoolNew_h32bit_24;
            public static Bitmap _000_RebootVM_h32bit_16 = Properties.Resources._000_RebootVM_h32bit_16;
            public static Bitmap _000_RemoveIPAddress_h32bit_16 = Properties.Resources._000_RemoveIPAddress_h32bit_16;
            public static Bitmap _000_RemoveSite_h32bit_16 = Properties.Resources._000_RemoveSite_h32bit_16;
            public static Bitmap _000_Resumed_h32bit_16 = Properties.Resources._000_Resumed_h32bit_16;
            public static Bitmap _000_Resumed_h32bit_16_green = Properties.Resources._000_Resumed_h32bit_16_green;
            public static Bitmap _000_Resumed_h32bit_24 = Properties.Resources._000_Resumed_h32bit_24;
            public static Bitmap _000_Resumed_h32bit_24_green = Properties.Resources._000_Resumed_h32bit_24_green;
            public static Bitmap _000_ScheduledVMsnapshotDiskMemory_h32bit_16 = Properties.Resources._000_ScheduledVMsnapshotDiskMemory_h32bit_16;
            public static Bitmap _000_ScheduledVMSnapshotDiskMemory_h32bit_32 = Properties.Resources._000_ScheduledVMSnapshotDiskMemory_h32bit_32;
            public static Bitmap _000_ScheduledVMsnapshotDiskOnly_h32bit_16 = Properties.Resources._000_ScheduledVMsnapshotDiskOnly_h32bit_16;
            public static Bitmap _000_ScheduledVMsnapshotDiskOnly_h32bit_32 = Properties.Resources._000_ScheduledVMsnapshotDiskOnly_h32bit_32;
            public static Bitmap _000_Search_h32bit_16 = Properties.Resources._000_Search_h32bit_16;
            public static Bitmap _000_Server_h32bit_16 = Properties.Resources._000_Server_h32bit_16;
            public static Bitmap _000_Server_h32bit_16_w_alert = Properties.Resources._000_Server_h32bit_16_w_alert;
            public static Bitmap _000_ServerDisconnected_h32bit_16 = Properties.Resources._000_ServerDisconnected_h32bit_16;
            public static Bitmap _000_ServerErrorFile_h32bit_16 = Properties.Resources._000_ServerErrorFile_h32bit_16;
            public static Bitmap _000_ServerHome_h32bit_16 = Properties.Resources._000_ServerHome_h32bit_16;
            public static Bitmap _000_ServerInProgress_h32bit_16 = Properties.Resources._000_ServerInProgress_h32bit_16;
            public static Bitmap _000_ServerMaintenance_h32bit_16 = Properties.Resources._000_ServerMaintenance_h32bit_16;
            public static Bitmap _000_ServerMaintenance_h32bit_32 = Properties.Resources._000_ServerMaintenance_h32bit_32;
            public static Bitmap _000_ServerWlb_h32bit_16 = Properties.Resources._000_ServerWlb_h32bit_16;
            public static Bitmap _000_Sites_h32bit_16 = Properties.Resources._000_Sites_h32bit_16;
            public static Bitmap _000_SliderTexture = Properties.Resources._000_SliderTexture;
            public static Bitmap _000_StartVM_h32bit_16 = Properties.Resources._000_StartVM_h32bit_16;
            public static Bitmap _000_StoppedVM_h32bit_16 = Properties.Resources._000_StoppedVM_h32bit_16;
            public static Bitmap _000_Storage_h32bit_16 = Properties.Resources._000_Storage_h32bit_16;
            public static Bitmap _000_StorageBroken_h32bit_16 = Properties.Resources._000_StorageBroken_h32bit_16;
            public static Bitmap _000_StorageDefault_h32bit_16 = Properties.Resources._000_StorageDefault_h32bit_16;
            public static Bitmap _000_StorageDisabled_h32bit_16 = Properties.Resources._000_StorageDisabled_h32bit_16;
            public static Bitmap _000_SuspendVM_h32bit_16 = Properties.Resources._000_SuspendVM_h32bit_16;
            public static Bitmap _000_SwitcherBackground = Properties.Resources._000_SwitcherBackground;
            public static Bitmap _000_Tag_h32bit_16 = Properties.Resources._000_Tag_h32bit_16;
            public static Bitmap _000_TCP_IPGroup_h32bit_16 = Properties.Resources._000_TCP_IPGroup_h32bit_16;
            public static Bitmap _000_TemplateDisabled_h32bit_16 = Properties.Resources._000_TemplateDisabled_h32bit_16;
            public static Bitmap _000_TestFailover_h32bit_32 = Properties.Resources._000_TestFailover_h32bit_32;
            public static Bitmap _000_Tick_h32bit_16 = Properties.Resources._000_Tick_h32bit_16;
            public static Bitmap _000_Tick_yellow_h32bit_16 = Properties.Resources._000_Tick_yellow_h32bit_16;
            public static Bitmap _000_ToolBar_Pref_Icon_dis = Properties.Resources._000_ToolBar_Pref_Icon_dis;
            public static Bitmap _000_ToolBar_Pref_Icon_ovr = Properties.Resources._000_ToolBar_Pref_Icon_ovr;
            public static Bitmap _000_ToolBar_Pref_Icon_up = Properties.Resources._000_ToolBar_Pref_Icon_up;
            public static Bitmap _000_ToolBar_USB_Icon_dis = Properties.Resources._000_ToolBar_USB_Icon_dis;
            public static Bitmap _000_ToolBar_USB_Icon_ovr = Properties.Resources._000_ToolBar_USB_Icon_ovr;
            public static Bitmap _000_ToolBar_USB_Icon_up = Properties.Resources._000_ToolBar_USB_Icon_up;
            public static Bitmap _000_TreeConnected_h32bit_16 = Properties.Resources._000_TreeConnected_h32bit_16;
            public static Bitmap _000_UpgradePool_h32bit_32 = Properties.Resources._000_UpgradePool_h32bit_32;
            public static Bitmap _000_User_h32bit_16 = Properties.Resources._000_User_h32bit_16;
            public static Bitmap _000_UserAndGroup_h32bit_16 = Properties.Resources._000_UserAndGroup_h32bit_16;
            public static Bitmap _000_UserAndGroup_h32bit_32 = Properties.Resources._000_UserAndGroup_h32bit_32;
            public static Bitmap _000_UserTemplate_h32bit_16 = Properties.Resources._000_UserTemplate_h32bit_16;
            public static Bitmap _000_ViewModeList_h32bit_16 = Properties.Resources._000_ViewModeList_h32bit_16;
            public static Bitmap _000_ViewModeTree_h32bit_16 = Properties.Resources._000_ViewModeTree_h32bit_16;
            public static Bitmap _000_VirtualAppliance_h32bit_16 = Properties.Resources._000_VirtualAppliance_h32bit_16;
            public static Bitmap _000_VirtualStorage_h32bit_16 = Properties.Resources._000_VirtualStorage_h32bit_16;
            public static Bitmap _000_VM_h32bit_16 = Properties.Resources._000_VM_h32bit_16;
            public static Bitmap _000_VM_h32bit_24 = Properties.Resources._000_VM_h32bit_24;
            public static Bitmap _000_VMDisabled_h32bit_16 = Properties.Resources._000_VMDisabled_h32bit_16;
            public static Bitmap _000_VMPausedDisabled_h32bit_16 = Properties.Resources._000_VMPausedDisabled_h32bit_16;
            public static Bitmap _000_VMSession_h32bit_16 = Properties.Resources._000_VMSession_h32bit_16;
            public static Bitmap _000_VMSnapshotDiskMemory_h32bit_16 = Properties.Resources._000_VMSnapshotDiskMemory_h32bit_16;
            public static Bitmap _000_VMSnapshotDiskMemory_h32bit_32 = Properties.Resources._000_VMSnapshotDiskMemory_h32bit_32;
            public static Bitmap _000_VMSnapShotDiskOnly_h32bit_16 = Properties.Resources._000_VMSnapShotDiskOnly_h32bit_16;
            public static Bitmap _000_VMSnapShotDiskOnly_h32bit_32 = Properties.Resources._000_VMSnapShotDiskOnly_h32bit_32;
            public static Bitmap _000_VMStarting_h32bit_16 = Properties.Resources._000_VMStarting_h32bit_16;
            public static Bitmap _000_VMStartingDisabled_h32bit_16 = Properties.Resources._000_VMStartingDisabled_h32bit_16;
            public static Bitmap _000_VMStoppedDisabled_h32bit_16 = Properties.Resources._000_VMStoppedDisabled_h32bit_16;
            public static Bitmap _000_VMTemplate_h32bit_16 = Properties.Resources._000_VMTemplate_h32bit_16;
            public static Bitmap _000_WarningAlert_h32bit_32 = Properties.Resources._000_WarningAlert_h32bit_32;
            public static Bitmap _000_weighting_h32bit_16 = Properties.Resources._000_weighting_h32bit_16;
            public static Bitmap _001_Back_h32bit_24 = Properties.Resources._001_Back_h32bit_24;
            public static Bitmap _001_CreateVM_h32bit_16 = Properties.Resources._001_CreateVM_h32bit_16;
            public static Bitmap _001_ForceReboot_h32bit_16 = Properties.Resources._001_ForceReboot_h32bit_16;
            public static Bitmap _001_ForceReboot_h32bit_24 = Properties.Resources._001_ForceReboot_h32bit_24;
            public static Bitmap _001_ForceShutDown_h32bit_16 = Properties.Resources._001_ForceShutDown_h32bit_16;
            public static Bitmap _001_ForceShutDown_h32bit_24 = Properties.Resources._001_ForceShutDown_h32bit_24;
            public static Bitmap _001_Forward_h32bit_24 = Properties.Resources._001_Forward_h32bit_24;
            public static Bitmap _001_LifeCycle_h32bit_24 = Properties.Resources._001_LifeCycle_h32bit_24;
            public static Bitmap _001_PerformanceGraph_h32bit_16 = Properties.Resources._001_PerformanceGraph_h32bit_16;
            public static Bitmap _001_Pin_h32bit_16 = Properties.Resources._001_Pin_h32bit_16;
            public static Bitmap _001_PowerOn_h32bit_16 = Properties.Resources._001_PowerOn_h32bit_16;
            public static Bitmap _001_PowerOn_h32bit_24 = Properties.Resources._001_PowerOn_h32bit_24;
            public static Bitmap _001_Reboot_h32bit_16 = Properties.Resources._001_Reboot_h32bit_16;
            public static Bitmap _001_Reboot_h32bit_24 = Properties.Resources._001_Reboot_h32bit_24;
            public static Bitmap _001_ShutDown_h32bit_16 = Properties.Resources._001_ShutDown_h32bit_16;
            public static Bitmap _001_ShutDown_h32bit_24 = Properties.Resources._001_ShutDown_h32bit_24;
            public static Bitmap _001_Tools_h32bit_16 = Properties.Resources._001_Tools_h32bit_16;
            public static Bitmap _001_WindowView_h32bit_16 = Properties.Resources._001_WindowView_h32bit_16;
            public static Bitmap _002_Configure_h32bit_16 = Properties.Resources._002_Configure_h32bit_16;
            public static Bitmap _015_Download_h32bit_32 = Properties.Resources._015_Download_h32bit_32;
            public static Bitmap _015_Download_h32bit_16 = Properties.Resources._015_Download_h32bit_16;
            public static Bitmap _075_TickRound_h32bit_16 = Properties.Resources._075_TickRound_h32bit_16;
            public static Bitmap _075_WarningRound_h32bit_16 = Properties.Resources._075_WarningRound_h32bit_16;
            public static Bitmap _112_LeftArrowLong_Blue_24x24_72 = Properties.Resources._112_LeftArrowLong_Blue_24x24_72;
            public static Bitmap _112_RightArrowLong_Blue_24x24_72 = Properties.Resources._112_RightArrowLong_Blue_24x24_72;
            public static Bitmap about_box_graphic = Properties.Resources.about_box_graphic;
            public static Bitmap ajax_loader = Properties.Resources.ajax_loader;
            public static Bitmap alert1_16 = Properties.Resources.alert1_16;
            public static Bitmap alert2_16 = Properties.Resources.alert2_16;
            public static Bitmap alert3_16 = Properties.Resources.alert3_16;
            public static Bitmap alert4_16 = Properties.Resources.alert4_16;
            public static Bitmap alert5_16 = Properties.Resources.alert5_16;
            public static Bitmap alert6_16 = Properties.Resources.alert6_16;
            public static Bitmap ascending_triangle = Properties.Resources.ascending_triangle;
            public static Bitmap asterisk = Properties.Resources.asterisk;
            public static Bitmap attach_24 = Properties.Resources.attach_24;
            public static Bitmap attach_virtualstorage_32 = Properties.Resources.attach_virtualstorage_32;
            public static Bitmap backup_restore_32 = Properties.Resources.backup_restore_32;
            public static Bitmap cancelled_action_16 = Properties.Resources.cancelled_action_16;
            public static Bitmap centos_16x = Properties.Resources.centos_16x;
            public static Bitmap change_password_16 = Properties.Resources.change_password_16;
            public static Bitmap change_password_32 = Properties.Resources.change_password_32;
            public static Bitmap clonevm_16 = Properties.Resources.clonevm_16;
            public static Bitmap close_16 = Properties.Resources.close_16;
            public static Bitmap commands_16 = Properties.Resources.commands_16;
            public static Bitmap console_16 = Properties.Resources.console_16;
            public static Bitmap contracted_triangle = Properties.Resources.contracted_triangle;
            public static Bitmap copy_16 = Properties.Resources.copy_16;
            public static Bitmap coreos_16 = Properties.Resources.coreos_16;
            public static Bitmap coreos_globe_icon = Properties.Resources.coreos_globe_icon;
            public static Bitmap cross = Properties.Resources.cross;
            public static Bitmap DateTime16 = Properties.Resources.DateTime16;
            public static Bitmap DC_16 = Properties.Resources.DC_16;
            public static Bitmap debian_16x = Properties.Resources.debian_16x;
            public static Bitmap descending_triangle = Properties.Resources.descending_triangle;
            public static Bitmap desktop = Properties.Resources.desktop;
            public static Bitmap detach_24 = Properties.Resources.detach_24;
            public static Bitmap edit_16 = Properties.Resources.edit_16;
            public static Bitmap expanded_triangle = Properties.Resources.expanded_triangle;
            public static Bitmap export_32 = Properties.Resources.export_32;
            public static Bitmap folder_grey = Properties.Resources.folder_grey;
            public static Bitmap folder_separator = Properties.Resources.folder_separator;
            public static Bitmap grab = Properties.Resources.grab;
            public static Bitmap grapharea = Properties.Resources.grapharea;
            public static Bitmap graphline = Properties.Resources.graphline;
            public static Bitmap gripper = Properties.Resources.gripper;
            public static Bitmap ha_16 = Properties.Resources.ha_16;
            public static Bitmap help_16_hover = Properties.Resources.help_16_hover;
            public static Bitmap help_24 = Properties.Resources.help_24;
            public static Bitmap help_24_hover = Properties.Resources.help_24_hover;
            public static Bitmap help_32_hover = Properties.Resources.help_32_hover;
            public static Bitmap homepage_bullet = Properties.Resources.homepage_bullet;
            public static Bitmap import_32 = Properties.Resources.import_32;
            public static Bitmap infra_view_16 = Properties.Resources.infra_view_16;
            public static Bitmap infra_view_16_textured = Properties.Resources.infra_view_16_textured;
            public static Bitmap infra_view_24 = Properties.Resources.infra_view_24;
            public static Bitmap licensekey_32 = Properties.Resources.licensekey_32;
            public static Bitmap lifecycle_hot = Properties.Resources.lifecycle_hot;
            public static Bitmap lifecycle_pressed = Properties.Resources.lifecycle_pressed;
            public static Bitmap livepatch_16 = Properties.Resources.livepatch_16;
            public static Bitmap log_destination_16 = Properties.Resources.log_destination_16;
            public static Bitmap Logo = Properties.Resources.Logo;
            public static Bitmap memory_dynmax_slider = Properties.Resources.memory_dynmax_slider;
            public static Bitmap memory_dynmax_slider_dark = Properties.Resources.memory_dynmax_slider_dark;
            public static Bitmap memory_dynmax_slider_light = Properties.Resources.memory_dynmax_slider_light;
            public static Bitmap memory_dynmax_slider_noedit = Properties.Resources.memory_dynmax_slider_noedit;
            public static Bitmap memory_dynmax_slider_noedit_small = Properties.Resources.memory_dynmax_slider_noedit_small;
            public static Bitmap memory_dynmax_slider_small = Properties.Resources.memory_dynmax_slider_small;
            public static Bitmap memory_dynmin_slider = Properties.Resources.memory_dynmin_slider;
            public static Bitmap memory_dynmin_slider_dark = Properties.Resources.memory_dynmin_slider_dark;
            public static Bitmap memory_dynmin_slider_light = Properties.Resources.memory_dynmin_slider_light;
            public static Bitmap memory_dynmin_slider_noedit = Properties.Resources.memory_dynmin_slider_noedit;
            public static Bitmap memory_dynmin_slider_noedit_small = Properties.Resources.memory_dynmin_slider_noedit_small;
            public static Bitmap memory_dynmin_slider_small = Properties.Resources.memory_dynmin_slider_small;
            public static Bitmap minus = Properties.Resources.minus;
            public static Bitmap more_16 = Properties.Resources.more_16;
            public static Bitmap neokylin_16x = Properties.Resources.neokylin_16x;
            public static Bitmap notif_alerts_16 = Properties.Resources.notif_alerts_16;
            public static Bitmap notif_events_16 = Properties.Resources.notif_events_16;
            public static Bitmap notif_events_errors_16 = Properties.Resources.notif_events_errors_16;
            public static Bitmap notif_none_16 = Properties.Resources.notif_none_16;
            public static Bitmap notif_none_24 = Properties.Resources.notif_none_24;
            public static Bitmap notif_updates_16 = Properties.Resources.notif_updates_16;
            public static Bitmap objects_16 = Properties.Resources.objects_16;
            public static Bitmap objects_16_textured = Properties.Resources.objects_16_textured;
            public static Bitmap objects_24 = Properties.Resources.objects_24;
            public static Bitmap oracle_16x = Properties.Resources.oracle_16x;
            public static Bitmap org_view_16 = Properties.Resources.org_view_16;
            public static Bitmap org_view_24 = Properties.Resources.org_view_24;
            public static Bitmap padlock = Properties.Resources.padlock;
            public static Bitmap paste_16 = Properties.Resources.paste_16;
            public static Bitmap PausedDC_16 = Properties.Resources.PausedDC_16;
            public static Bitmap PDChevronDown = Properties.Resources.PDChevronDown;
            public static Bitmap PDChevronDownOver = Properties.Resources.PDChevronDownOver;
            public static Bitmap PDChevronLeft = Properties.Resources.PDChevronLeft;
            public static Bitmap PDChevronRight = Properties.Resources.PDChevronRight;
            public static Bitmap PDChevronUp = Properties.Resources.PDChevronUp;
            public static Bitmap PDChevronUpOver = Properties.Resources.PDChevronUpOver;
            public static Bitmap pool_unpatched = Properties.Resources.pool_unpatched;
            public static Bitmap pool_up_16 = Properties.Resources.pool_up_16;
            public static Bitmap redhat_16x = Properties.Resources.redhat_16x;
            public static Bitmap Refresh16 = Properties.Resources.Refresh16;
            public static Bitmap RunningDC_16 = Properties.Resources.RunningDC_16;
            public static Bitmap save_to_disk = Properties.Resources.save_to_disk;
            public static Bitmap saved_searches_16 = Properties.Resources.saved_searches_16;
            public static Bitmap saved_searches_24 = Properties.Resources.saved_searches_24;
            public static Bitmap scilinux_16x = Properties.Resources.scilinux_16x;
            public static Bitmap server_up_16 = Properties.Resources.server_up_16;
            public static Bitmap sl_16 = Properties.Resources.sl_16;
            public static Bitmap sl_add_storage_system_16 = Properties.Resources.sl_add_storage_system_16;
            public static Bitmap sl_add_storage_system_32 = Properties.Resources.sl_add_storage_system_32;
            public static Bitmap sl_add_storage_system_small_16 = Properties.Resources.sl_add_storage_system_small_16;
            public static Bitmap sl_connected_16 = Properties.Resources.sl_connected_16;
            public static Bitmap sl_connecting_16 = Properties.Resources.sl_connecting_16;
            public static Bitmap sl_disconnected_16 = Properties.Resources.sl_disconnected_16;
            public static Bitmap sl_lun_16 = Properties.Resources.sl_lun_16;
            public static Bitmap sl_luns_16 = Properties.Resources.sl_luns_16;
            public static Bitmap sl_pool_16 = Properties.Resources.sl_pool_16;
            public static Bitmap sl_pools_16 = Properties.Resources.sl_pools_16;
            public static Bitmap sl_system_16 = Properties.Resources.sl_system_16;
            public static Bitmap SpinningFrame0 = Properties.Resources.SpinningFrame0;
            public static Bitmap SpinningFrame1 = Properties.Resources.SpinningFrame1;
            public static Bitmap SpinningFrame2 = Properties.Resources.SpinningFrame2;
            public static Bitmap SpinningFrame3 = Properties.Resources.SpinningFrame3;
            public static Bitmap SpinningFrame4 = Properties.Resources.SpinningFrame4;
            public static Bitmap SpinningFrame5 = Properties.Resources.SpinningFrame5;
            public static Bitmap SpinningFrame6 = Properties.Resources.SpinningFrame6;
            public static Bitmap SpinningFrame7 = Properties.Resources.SpinningFrame7;
            public static Bitmap splash = Properties.Resources.splash;
            public static Bitmap StoppedDC_16 = Properties.Resources.StoppedDC_16;
            public static Bitmap subscribe = Properties.Resources.subscribe;
            public static Bitmap suse_16x = Properties.Resources.suse_16x;
            public static Bitmap tools_notinstalled_16x = Properties.Resources.tools_notinstalled_16x;
            public static Bitmap tools_outofdate_16x = Properties.Resources.tools_outofdate_16x;
            public static Bitmap tools_uptodate_16x = Properties.Resources.tools_uptodate_16x;
            public static Bitmap tpm = Properties.Resources.tpm;
            public static Bitmap tree_minus = Properties.Resources.tree_minus;
            public static Bitmap tree_plus = Properties.Resources.tree_plus;
            public static Bitmap tshadowdown = Properties.Resources.tshadowdown;
            public static Bitmap tshadowdownleft = Properties.Resources.tshadowdownleft;
            public static Bitmap tshadowdownright = Properties.Resources.tshadowdownright;
            public static Bitmap tshadowright = Properties.Resources.tshadowright;
            public static Bitmap tshadowtopright = Properties.Resources.tshadowtopright;
            public static Bitmap ubuntu_16x = Properties.Resources.ubuntu_16x;
            public static Bitmap usagebar_0 = Properties.Resources.usagebar_0;
            public static Bitmap usagebar_1 = Properties.Resources.usagebar_1;
            public static Bitmap usagebar_10 = Properties.Resources.usagebar_10;
            public static Bitmap usagebar_2 = Properties.Resources.usagebar_2;
            public static Bitmap usagebar_3 = Properties.Resources.usagebar_3;
            public static Bitmap usagebar_4 = Properties.Resources.usagebar_4;
            public static Bitmap usagebar_5 = Properties.Resources.usagebar_5;
            public static Bitmap usagebar_6 = Properties.Resources.usagebar_6;
            public static Bitmap usagebar_7 = Properties.Resources.usagebar_7;
            public static Bitmap usagebar_8 = Properties.Resources.usagebar_8;
            public static Bitmap usagebar_9 = Properties.Resources.usagebar_9;
            public static Bitmap virtualstorage_snapshot_16 = Properties.Resources.virtualstorage_snapshot_16;
            public static Bitmap vmBackground = Properties.Resources.vmBackground;
            public static Bitmap vmBackgroundCurrent = Properties.Resources.vmBackgroundCurrent;
            public static Bitmap VMTemplate_h32bit_32 = Properties.Resources.VMTemplate_h32bit_32;
            public static Bitmap vnc_local_cursor = Properties.Resources.vnc_local_cursor;
            public static Bitmap windows_h32bit_16 = Properties.Resources.windows_h32bit_16;
            public static Bitmap wizard_background = Properties.Resources.wizard_background;
            public static Bitmap queued = Properties.Resources.queued;
            public static Bitmap _000_User_h32bit_32 = Properties.Resources._000_User_h32bit_32;
            public static Bitmap asianux_16x = Properties.Resources.asianux_16x;
            public static Bitmap gooroom_16x = Properties.Resources.gooroom_16x;
            public static Bitmap rocky_16x = Properties.Resources.rocky_16x;
            public static Bitmap linx_16x = Properties.Resources.linx_16x;
            public static Bitmap turbo_16x = Properties.Resources.turbo_16x;
            public static Bitmap usb_16 = Properties.Resources.usb_16;
            public static Bitmap yinhekylin_16x = Properties.Resources.yinhekylin_16x;
            public static Bitmap rightArrowLong_Blue_16 = Properties.Resources.rightArrowLong_Blue_16;
            public static Bitmap rpm_package = Properties.Resources.rpm_package;
        }
    }
}