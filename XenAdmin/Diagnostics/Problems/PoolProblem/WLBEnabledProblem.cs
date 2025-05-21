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

using XenAdmin.Actions;
using XenAdmin.Actions.Wlb;
using XenAdmin.Diagnostics.Checks;
using XenAPI;


namespace XenAdmin.Diagnostics.Problems.PoolProblem
{
    class WLBEnabledProblem : PoolProblem
    {
        public WLBEnabledProblem(Check check, Pool pool)
            : base(check, pool)
        {
        }

        public override string Description => string.Format(Messages.CHECK_WLB_ENABLED, Pool);

        public override string HelpMessage => Messages.HELP_MESSAGE_DISABLE_WLB;

        protected override AsyncAction CreateAction(out bool cancelled)
        {
            cancelled = false;
            return new DisableWLBAction(Pool, false);
        }

        public override AsyncAction CreateUnwindChangesAction()
        {
            var pool = Pool.Connection.Resolve(new XenRef<Pool>(Pool.opaque_ref));

            if (pool == null)
            {
                foreach (var xenConnection in ConnectionsManager.XenConnectionsCopy)
                {
                    pool = xenConnection.Resolve(new XenRef<Pool>(Pool.opaque_ref));
                    if (pool != null)
                        break;
                }
            }

            return pool == null ? null : new EnableWLBAction(pool);
        }
    }
}
