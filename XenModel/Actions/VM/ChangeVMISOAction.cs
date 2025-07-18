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
using XenAdmin.Network;
using XenAPI;


namespace XenAdmin.Actions
{
    /// <summary>
    /// Change the ISO image attached to a VBD in a VM
    /// </summary>
    public class ChangeVMISOAction : AsyncAction
    {
        private readonly VBD cdrom;
        private readonly VDI vdi;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="vm"></param>
        /// <param name="vdi">May be null, indicating that the CD should be ejected (i.e. set to empty).</param>
        /// <param name="cdrom">Must not be null.</param>
        public ChangeVMISOAction(IXenConnection connection, VM vm, VDI vdi, VBD cdrom)
            : base(connection, "")
        {
            VM = vm ?? throw new ArgumentNullException(nameof(vm));
            this.vdi = vdi;
            this.cdrom = cdrom ?? throw new ArgumentNullException(nameof(cdrom));

            Title = vdi == null
                ? string.Format(Messages.ISO_UNLOADING, vm.Name())
                : string.Format(Messages.ISO_LOADING, vdi.Name(), vm.Name());

            if (!cdrom.empty)
                ApiMethodsToRoleCheck.Add("VBD.async_eject");
            if (vdi != null)
                ApiMethodsToRoleCheck.Add("VBD.async_insert");
        }

        protected override void Run()
        {
            bool wasEmpty = false;

            if (!cdrom.empty)
            {
                RelatedTask = VBD.async_eject(Session, cdrom.opaque_ref);
                wasEmpty = true;
                PollToCompletion(0, 50);
            }

            if (vdi != null)
            {
                RelatedTask = VBD.async_insert(Session, cdrom.opaque_ref, vdi.opaque_ref);
                PollToCompletion(wasEmpty ? 50 : 0, 100);
                Description = string.Format(Messages.ISO_LOADED, vdi.Name(), VM.Name());
            }
            else
            {
                Description = string.Format(Messages.ISO_UNLOADED, VM.Name());
            }
        }
    }
}
