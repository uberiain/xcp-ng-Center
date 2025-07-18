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
using XenAdmin.Core;
using XenAPI;


namespace XenAdmin.Actions
{
    public class CreatePoolAction : PoolAbstractAction
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<Host> _supporters, _hostsToRelicense, _hostsToAdConfigure;
        private readonly string _name;
        private readonly string _description;

        /// <summary>
        /// For Create only.
        /// </summary>
        /// <param name="coordinator"></param>
        /// <param name="supporters"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="doOnLicensingFailure"></param>
        /// <param name="getAdCredentials"></param>
        public CreatePoolAction(Host coordinator, List<Host> supporters, string name, string description, Func<Host, AdUserAndPassword> getAdCredentials,
            Action<List<LicenseFailure>, string> doOnLicensingFailure)
            : base(coordinator.Connection, string.Format(Messages.CREATING_NAMED_POOL_WITH_COORDINATOR, name, coordinator.Name()),
            getAdCredentials, doOnLicensingFailure)
        {
            System.Diagnostics.Trace.Assert(coordinator != null);

            this.Host = coordinator;
            this._supporters = supporters;
            _hostsToRelicense = supporters.FindAll(h => PoolJoinRules.FreeHostPaidCoordinator(h, coordinator, false));
            _hostsToAdConfigure = supporters.FindAll(h => !PoolJoinRules.CompatibleAdConfig(h, coordinator, false));
            this._name = name;
            this._description = description;
            this.Description = Messages.WAITING;
            SetRBACPermissions();
        }


        private void SetRBACPermissions()
        {
            // NB: We don't add any RBAC checks for actions on the supporter, because it would
            // elevate the wrong session. They will still be caught in the try-catch blocks below.
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
            double p2 = 100.0 / _supporters.Count;
            int i2 = 0;

            this.Description = Messages.POOLCREATE_CREATING;

            Pool coordinator_pool = Helpers.GetPoolOfOne(Connection);
            if (coordinator_pool == null)
                throw new XenAPI.Failure(XenAPI.Failure.INTERNAL_ERROR, Messages.CACHE_NOT_YET_POPULATED);

            FixLicensing(coordinator_pool, _hostsToRelicense, DoOnLicensingFailure);
            FixAd(coordinator_pool, _hostsToAdConfigure, GetAdCredentials);

            XenAPI.Pool.set_name_label(Session, coordinator_pool.opaque_ref, _name);
            XenAPI.Pool.set_name_description(Session, coordinator_pool.opaque_ref, _description);

            ClearNonSharedSrs(coordinator_pool);

            Description = Messages.ACTION_POOL_WIZARD_CREATE_DESCRIPTION_ADDING_MEMBERS;

            foreach (Host supporter in _supporters)
            {
                log.InfoFormat("Adding member {0}", supporter.Name());
                int lo = (int)(i2 * p2);
                int hi = (int)((i2 + 1) * p2);
                // RBAC: We have forced identical AD configs, but this will fail unless both supporter-to-be and coordinator sessions have the correct role.
                Session = NewSession(supporter.Connection);
                RelatedTask = XenAPI.Pool.async_join(Session, coordinator_pool.Connection.Hostname, coordinator_pool.Connection.Username, coordinator_pool.Connection.Password);
                PollToCompletion(lo, hi);
                i2++;

                log.InfoFormat("Dropping connection to {0}", supporter.Name());
                supporter.Connection.EndConnect();
                // EndConnect will clear the cache itself, but on a background thread. To prevent a race between the event handlers 
                // being removed with the connection, and the final cache clear events we explicitly call cache clear once more before
                // removing the connection.
                ConnectionsManager.ClearCacheAndRemoveConnection(supporter.Connection);
            }

            Description = Messages.ACTION_POOL_WIZARD_CREATE_DESCRIPTION_DONE;
        }

    }
}