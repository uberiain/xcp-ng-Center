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
using System.Linq;
using XenAdmin.Core;
using XenAPI;
using XenAdmin.Dialogs;


namespace XenAdmin.Commands
{
    /// <summary>
    /// The Command for the menu-items displayed for each Host in the start-on, resume-on and migrate sub-menu when WLB isn't enabled.
    /// </summary>
    internal class VMOperationHostCommand : VMOperationCommand
    {
        public delegate Host GetHostForVM(VM vm);

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<VM, string> _cantBootReasons = new Dictionary<VM, string>();
        private readonly bool _noneCanBoot = true;
        private readonly string _text;
        private readonly GetHostForVM _getHostForVM;

        public VMOperationHostCommand(IMainWindow mainWindow, IEnumerable<SelectedItem> vms, GetHostForVM getHostForVM, string text, vm_operations operation, Session session)
            : base(mainWindow, vms, operation)
        {
            Util.ThrowIfParameterNull(session, "session");
            Util.ThrowIfParameterNull(getHostForVM, "getHostForVM");
            Util.ThrowIfParameterNull(text, "text");
            _text = text;
            _getHostForVM = getHostForVM;

            foreach (SelectedItem item in vms)
            {
                VM vm = (VM)item.XenObject;

                if (VmCanBootOnHost(vm, GetHost(vm), session, operation, out var reason))
                    _noneCanBoot = false;
                else
                    _cantBootReasons[vm] = reason;
            }
        }

        protected sealed override Host GetHost(VM vm)
        {
            return _getHostForVM(vm);
        }

        public override string MenuText
        {
            get
            {
                if (_noneCanBoot)
                {
                    var uniqueReasons = _cantBootReasons.Values.Distinct().ToList();

                    if (uniqueReasons.Count == 1)
                        return string.Format(Messages.MAINWINDOW_CONTEXT_REASON, _text, uniqueReasons[0]);
                }
                return _text;
            }
        }

        public override Image MenuImage
        {
            get
            {
                return Images.StaticImages._000_TreeConnected_h32bit_16;
            }
        }

        protected override bool CanRun(VM vm)
        {
            return vm != null && !_cantBootReasons.ContainsKey(vm);
        }

        internal static bool VmCanBootOnHost(VM vm, Host host, Session session, vm_operations operation, out string cannotBootReason)
        {
            if (host == null)
            {
                cannotBootReason = Messages.NO_HOME_SERVER;
                return false;
            }

            if (vm.power_state == vm_power_state.Running)
            {
                var residentHost = vm.Connection.Resolve(vm.resident_on);

                if (residentHost != null)
                {
                    if (host.opaque_ref == residentHost.opaque_ref)
                    {
                        cannotBootReason = Messages.HOST_MENU_CURRENT_SERVER;
                        return false;
                    }

                    if (Helpers.ProductVersionCompare(Helpers.HostProductVersion(host), Helpers.HostProductVersion(residentHost)) < 0)
                    {
                        cannotBootReason = Messages.OLDER_THAN_CURRENT_SERVER;
                        return false;
                    }
                }
            }

            if ((operation == vm_operations.pool_migrate || operation == vm_operations.resume_on) && VmCpuIncompatibleWithHost(host, vm))
            {
                cannotBootReason = FriendlyErrorNames.VM_INCOMPATIBLE_WITH_THIS_HOST;
                return false;
            }

            try
            {
                VM.assert_can_boot_here(session, vm.opaque_ref, host.opaque_ref);
            }
            catch (Failure f)
            {
                if (f.ErrorDescription.Count > 2 && f.ErrorDescription[0] == Failure.VM_REQUIRES_SR)
                {
                    SR sr = host.Connection.Resolve(new XenRef<SR>(f.ErrorDescription[2]));

                    if (sr != null && sr.content_type == SR.Content_Type_ISO)
                    {
                        cannotBootReason = Messages.MIGRATE_PLEASE_EJECT_YOUR_CD;
                        return false;
                    }
                }

                cannotBootReason = f.ShortMessage;
                return false;
            }
            catch (Exception e)
            {
                log.ErrorFormat("There was an error calling assert_can_boot_here on host {0}: {1}", host.Name(), e.Message);
                cannotBootReason = Messages.HOST_MENU_UNKNOWN_ERROR;
                return false;
            }

            cannotBootReason = null;
            return true;
        }

        protected override CommandErrorDialog GetErrorDialogCore(IDictionary<IXenObject, string> cantRunReasons)
        {
            return new CommandErrorDialog(ErrorDialogTitle, ErrorDialogText, cantRunReasons);
        }

        protected override string GetCantRunReasonCore(IXenObject item)
        {
            if (item is VM vm && _cantBootReasons.ContainsKey(vm))
                return _cantBootReasons[vm];

            return base.GetCantRunReasonCore(item);
        }

        public static bool VmCpuIncompatibleWithHost(Host targetHost, VM vm)
        {
            // only for running or suspended VMs
            if (vm.power_state != vm_power_state.Running && vm.power_state != vm_power_state.Suspended)
                return false;

            if (vm.last_boot_CPU_flags == null || !vm.last_boot_CPU_flags.ContainsKey("vendor")
                || targetHost.cpu_info == null || !targetHost.cpu_info.ContainsKey("vendor"))
                return false;

            if (vm.last_boot_CPU_flags["vendor"] != targetHost.cpu_info["vendor"])
                return true;

            return false;
        }
    }
}
