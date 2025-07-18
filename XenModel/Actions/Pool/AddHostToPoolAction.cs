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
using System.Linq;
using XenAdmin.Core;
using XenAPI;


namespace XenAdmin.Actions
{
    public class AddHostToPoolAction : PoolAbstractAction
    {
        private readonly List<Host> _hostsToRelicense;
        private readonly List<Host> _hostsToAdConfigure;

        public AddHostToPoolAction(Pool poolToJoin, Host joiningHost, Func<Host, AdUserAndPassword> getAdCredentials,
            Action<List<LicenseFailure>, string> doOnLicensingFailure)
            : base(joiningHost.Connection, string.Format(Messages.ADDING_SERVER_TO_POOL, joiningHost.Name(), poolToJoin.Name()),
            getAdCredentials, doOnLicensingFailure)
        {
            this.Pool = poolToJoin;
            this.Host = joiningHost;
            Host coordinator = Helpers.GetCoordinator(poolToJoin);
            _hostsToRelicense = new List<Host>();

            _hostsToAdConfigure = new List<Host>();
            if (PoolJoinRules.FreeHostPaidCoordinator(joiningHost, coordinator, false))
                _hostsToRelicense.Add(joiningHost);

            if (!PoolJoinRules.CompatibleAdConfig(joiningHost, coordinator, false))
                _hostsToAdConfigure.Add(joiningHost);
            this.Description = Messages.WAITING;
            AddCommonAPIMethodsToRoleCheck();
            // SaveChanges in the ClearNonSharedSrs
            ApiMethodsToRoleCheck.Add("pool.set_name_label");
            ApiMethodsToRoleCheck.Add("pool.set_name_description");
            ApiMethodsToRoleCheck.Add("pool.set_other_config");
            ApiMethodsToRoleCheck.Add("pool.add_to_other_config");
            ApiMethodsToRoleCheck.Add("pool.set_gui_config");
            ApiMethodsToRoleCheck.Add("pool.add_to_gui_config");
            ApiMethodsToRoleCheck.Add("pool.set_default_SR");
            ApiMethodsToRoleCheck.Add("pool.set_suspend_image_SR");
            ApiMethodsToRoleCheck.Add("pool.set_crash_dump_SR");
            ApiMethodsToRoleCheck.Add("pool.remove_from_other_config");
            ApiMethodsToRoleCheck.Add("pool.remove_tags");
            ApiMethodsToRoleCheck.Add("pool.set_wlb_enabled");
            ApiMethodsToRoleCheck.Add("pool.set_wlb_verify_cert");
            ApiMethodsToRoleCheck.Add("pool.join");
        }

        protected override void Run()
        {
            this.Description = Messages.POOLCREATE_ADDING;
            // Use a try catch here for RBAC errors as they are a special case compared to other actions
            try
            {
                FixLicensing(Pool, _hostsToRelicense, DoOnLicensingFailure);
                FixAd(Pool, _hostsToAdConfigure, GetAdCredentials);

                var coordinator = Pool.Connection.TryResolveWithTimeout(Pool.master);
                var address = coordinator != null ? coordinator.address : Pool.Connection.Hostname;

                RelatedTask = Pool.async_join(Session, address, Pool.Connection.Username, Pool.Connection.Password);
                PollToCompletion(0, 90);
            }
            catch (Failure f)
            {
                // This is probably not needed any more, because it's now checked in PoolJoinRules.
                // But let's leave it here in case. SRET.
                if (f.ErrorDescription.Count > 1 && f.ErrorDescription[0] == Failure.RBAC_PERMISSION_DENIED)
                {
                    var sessions = new[] {Session, Pool.Connection.Session};
                    var authRoles = Role.ValidRoleList(f.ErrorDescription[1], Session.Connection);

                    var output = string.Join(", ", sessions.Select(s => string.Format(Messages.ROLE_ON_CONNECTION,
                        s.FriendlyRoleDescription(), Helpers.GetName(s.Connection).Ellipsise(50))));

                    throw new Failure(Failure.RBAC_PERMISSION_DENIED_FRIENDLY, output, Role.FriendlyCsvRoleList(authRoles));
                }

                throw;
            }

            // We need a coordinator session for ClearNonSharedSrs.
            // No need to log out the supporter session, because the server is going to reset its database anyway.

            Session = NewSession(Pool.Connection);
            ClearNonSharedSrs(Pool);

            this.Description = Messages.POOLCREATE_ADDED;
        }

        protected override void OnCompleted()
        {
            if (Succeeded)
            {
                ConnectionsManager.ClearCacheAndRemoveConnection(Host.Connection);
            }
            ClearAllDelegates();
            base.OnCompleted();
        }
    }
}
