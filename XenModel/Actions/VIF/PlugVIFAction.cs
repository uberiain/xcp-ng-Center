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

using XenAPI;

namespace XenAdmin.Actions
{
    public class PlugVIFAction : AsyncAction
    {
        private readonly VIF _vif;

        public PlugVIFAction(VIF vif)
            : base(vif.Connection, string.Format(Messages.ACTION_VIF_PLUG_TITLE, vif.Connection.Resolve(vif.VM).Name()))
        {
            VM = vif.Connection.Resolve(vif.VM);
            _vif = vif;

            if (VM.power_state == vm_power_state.Running)
                ApiMethodsToRoleCheck.AddRange("VIF.get_allowed_operations", "VIF.plug");
        }

        protected override void Run()
        {
            Description = Messages.ACTION_VIF_PLUGGING;
            if (VM.power_state == vm_power_state.Running &&
                VIF.get_allowed_operations(Session, _vif.opaque_ref).Contains(vif_operations.plug))
            {
                VIF.plug(Session, _vif.opaque_ref);
            }
            Description = Messages.ACTION_VIF_PLUGGED;
        }
    }
}
