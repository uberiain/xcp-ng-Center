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
    public class SetSslLegacyAction: AsyncAction
    {
        bool legacyMode;

        public SetSslLegacyAction(Pool pool, bool legacyMode)
            : base(pool.Connection, Messages.SETTING_SECURITY_SETTINGS)
        {
            Pool = pool;
            this.legacyMode = legacyMode;

            if (legacyMode)
                ApiMethodsToRoleCheck.Add("pool.async_enable_ssl_legacy");
            else
                ApiMethodsToRoleCheck.Add("pool.async_disable_ssl_legacy");
        }

        protected override void Run()
        {
            Pool.Connection.ExpectDisruption = true;

            if (legacyMode)
                RelatedTask = Pool.async_enable_ssl_legacy(Session, Pool.opaque_ref);
            else
                RelatedTask = Pool.async_disable_ssl_legacy(Session, Pool.opaque_ref);
            PollToCompletion();
        }

        protected override void Clean()
        {
            Pool.Connection.ExpectDisruption = false;
        }
    }
}
