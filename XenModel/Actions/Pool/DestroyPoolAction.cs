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
    public class DestroyPoolAction: AsyncAction
    {
        public DestroyPoolAction(Pool pool)
            : base(pool.Connection, string.Format(Messages.DESTROYING_POOL, pool.Name()))
        {
            Pool = pool;
            Description = Messages.WAITING;

            ApiMethodsToRoleCheck.AddRange("pool.set_name_label", "pool.set_name_description");
        }

        protected override void Run()
        {
            Description = Messages.POOLCREATE_DESTROYING;

            if (Connection.Cache.HostCount > 1)
                throw new Failure(Failure.INTERNAL_ERROR, Messages.POOLCREATE_MULTIHOST);  // We should not have any UI to reach here, and must not be allowed to proceed

            Pool.set_name_label(Session, Pool.opaque_ref, "");
            Pool.set_name_description(Session, Pool.opaque_ref, "");
            Description = Messages.POOLCREATE_DESTROYED;
        }
    }
}