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

using System.Collections.Generic;
using XenAPI;

namespace XenAdmin.Actions
{
    public class CreateVUSBAction : AsyncAction
    {
        private readonly PUSB _pusb;

        public CreateVUSBAction(PUSB pusb, VM vm) :
            base(pusb.Connection, string.Format(Messages.ACTION_VUSB_CREATING, pusb.Name(), vm.Name()))
        {
            _pusb = pusb;
            VM = vm;

            if (!VM.UsingUpstreamQemu())
                ApiMethodsToRoleCheck.Add("VM.set_platform");

            ApiMethodsToRoleCheck.Add("VUSB.create");
        }

        protected override void Run()
        {
            if (!VM.UsingUpstreamQemu())
            {
                Dictionary<string, string> platform = VM.platform == null
                    ? new Dictionary<string, string>()
                    : new Dictionary<string, string>(VM.platform);
                platform["device-model"] = "qemu-upstream-compat";
                VM.set_platform(Session, VM.opaque_ref, platform);
            }

            VUSB.create(Session, VM.opaque_ref, _pusb.USB_group, null);
            Description = Messages.ACTION_VUSB_CREATED;
        }
    }
}
