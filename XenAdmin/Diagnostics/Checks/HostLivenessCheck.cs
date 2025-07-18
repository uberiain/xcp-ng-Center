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
using XenAdmin.Diagnostics.Problems;
using XenAdmin.Diagnostics.Problems.HostProblem;

namespace XenAdmin.Diagnostics.Checks
{
    class HostLivenessCheck : HostCheck
    {
        private readonly bool checkEnabled;

        public HostLivenessCheck(Host host, bool checkEnabled = false)
            :base(host)
        {
            this.checkEnabled = checkEnabled;
        }

        protected override Problem RunCheck()
        {
            if (!Host.IsLive())
                return new HostNotLive(this, Host);

            if (checkEnabled && (Host.MaintenanceMode() || !Host.enabled))
                return new HostMaintenanceMode(this, Host);

            return null;
        }

        public override string Description
        {
            get
            {
                return Messages.HOST_LIVENESS_CHECK_DESCRIPTION;
            }
        }
    }

    abstract class HostPostLivenessCheck : HostCheck
    {
        protected HostPostLivenessCheck(Host host)
            : base(host)
        {
        }

        protected sealed override Problem RunCheck()
        {
            if (!Host.IsLive())
                return new HostNotLiveWarning(this, Host);

            return RunHostCheck();
        }

        protected abstract Problem RunHostCheck();
    }
}
