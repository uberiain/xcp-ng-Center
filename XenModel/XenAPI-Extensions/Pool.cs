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

namespace XenAPI
{
    public partial class Pool : IComparable<Pool>, IEquatable<Pool>
    {
        private const string ROLLING_UPGRADE_IN_PROGRESS = "rolling_upgrade_in_progress";
        private const string FORBID_RPU_FOR_HCI = "hci-forbid-rpu";
        private const string FAULT_TOLERANCE_LIMIT_FOR_HCI = "hci-limit-fault-tolerance";
        private const string FORBID_UPDATE_AUTO_RESTARTS = "hci-forbid-update-auto-restart";
        public const string HEALTH_CHECK_ENROLLMENT = "Enrollment";

        public const string MAIL_DESTINATION_KEY_NAME = "mail-destination";
        public const string SMTP_MAILHUB_KEY_NAME = "ssmtp-mailhub";
        public const string MAIL_LANGUAGE_KEY_NAME = "mail-language";

        public override string ToString()
        {
            return Name();
        }

        public override string Description()
        {
            return name_description;
        }

        public override string Name()
        {
            if (name_label != "")
                return name_label;

            if (Connection == null)
                return string.Empty;

            Host coordinator = Connection.Resolve(this.master);
            return coordinator == null ? "" : coordinator.Name();
        }

        internal override string LocationString()
        {
            return string.Empty;
        }

        public List<SR> GetHAHeartbeatSRs()
        {
            List<SR> result = new List<SR>();

            foreach (string opaqueRef in ha_statefiles)
            {
                XenRef<VDI> vdiRef = new XenRef<VDI>(opaqueRef);
                VDI vdi = Connection.Resolve(vdiRef);
                if (vdi == null)
                    continue;
                SR sr = vdi.Connection.Resolve(vdi.SR);
                if (sr != null)
                    result.Add(sr);
            }

            return result;
        }

        public bool IsCoordinatorUpgraded()
        {
            Host coordinator = Helpers.GetCoordinator(this);
            foreach (var host in this.Connection.Cache.Hosts)
            {
                if (host.LongProductVersion() != coordinator.LongProductVersion())
                    return true;
            }
            return false;
        }

        public bool IsFreeLicenseOrExpired()
        {
                return Connection.Cache.Hosts.Any(h => h.IsExpired());
        }

        /// <summary>
        /// Determine whether the given pool is a visible pool, i.e. not a pool-of-one.
        /// </summary>
        public bool IsVisible()
        {
            return Connection != null && (name_label != "" || Connection.Cache.HostCount > 1);
        }

        public bool RollingUpgrade()
        {
            return other_config != null && other_config.ContainsKey(ROLLING_UPGRADE_IN_PROGRESS);
        }

        public bool IsUpgradeForbidden()
        {
            return other_config != null && other_config.ContainsKey(FORBID_RPU_FOR_HCI);
        }

        public bool IsAutoUpdateRestartsForbidden()
        {
            return other_config != null && other_config.ContainsKey(FORBID_UPDATE_AUTO_RESTARTS);
        }

        public static long GetMaximumTolerableHostFailures(Session session, Dictionary<XenRef<VM>, string> config)
        {
            long max = ha_compute_hypothetical_max_host_failures_to_tolerate(session, config);
            long result;
            Pool p = Helpers.GetPoolOfOne(session.Connection);
            
            if (p!= null && p.other_config != null
                && p.other_config.ContainsKey(FAULT_TOLERANCE_LIMIT_FOR_HCI)
                && long.TryParse(p.other_config[FAULT_TOLERANCE_LIMIT_FOR_HCI], out result)
                && result <= max)//extra check in case the number set is accidentally wrong
            {
                return result;
            }

            return max;
        }

        public string GetXCPluginSecret(string plugin_name, IXenObject obj)
        {
            return Get(gui_config, XCPluginSecretName(plugin_name, obj));
        }

        public void RemoveXCPluginSecret(Session session, string plugin_name, IXenObject obj)
        {
            Pool.remove_from_gui_config(session, opaque_ref, XCPluginSecretName(plugin_name, obj));
        }

        public void SetXCPluginSecret(Session session, string plugin_name, IXenObject obj, string secret_uuid)
        {
            string n = XCPluginSecretName(plugin_name, obj);
            Pool.remove_from_gui_config(session, opaque_ref, n);
            Pool.add_to_gui_config(session, opaque_ref, n, secret_uuid);
        }

        private string XCPluginSecretName(string plugin_name, IXenObject obj)
        {
            return string.Format("XC_PLUGIN_SECRET_{0}_{1}_{2}", obj.Connection.Username, plugin_name, Helpers.GetUuid(obj));
        }

        // Whether the vSwitch Controller appears to be configured.
        // (Note that we can't tell whether it's actually working properly through the API).
        public bool vSwitchController()
        {
            if (string.IsNullOrEmpty(vswitch_controller))
                return false;

            foreach (Host h in Connection.Cache.Hosts)
            {
                if (Host.RestrictVSwitchController(h) ||
                    !h.software_version.ContainsKey("network_backend") ||
                    h.software_version["network_backend"] != "openvswitch")
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasSriovNic()
        {
            return Connection.Cache.PIFs.Any(pif => pif.SriovCapable());
        }

        public List<XenAPI.Host> HostsToUpgrade()
        {
            //First one to upgrade has to be the coordinator
            var coordinator = Helpers.GetCoordinator(Connection);

            List<XenAPI.Host> result = IsCoordinatorUpgraded()
                ? Connection.Cache.Hosts.Where(host => host.LongProductVersion() != coordinator.LongProductVersion()).ToList()
                : Connection.Cache.Hosts.ToList();
            result.Sort();

            return result;
        }

        public bool IsPoolFullyUpgraded()
        {
            Host coordinator = Helpers.GetCoordinator(this);

            foreach (var host in this.Connection.Cache.Hosts)
            {
                if (host.LongProductVersion() != coordinator.LongProductVersion())
                    return false;
            }
            return true;
        }

        public Host SmallerVersionHost()
        {
            Host hostWithSmallerVersion = null;
            foreach (var host in this.Connection.Cache.Hosts)
            {
                if (hostWithSmallerVersion == null)
                    hostWithSmallerVersion = host;
                else if (Helpers.ProductVersionCompare(hostWithSmallerVersion.ProductVersion(), host.ProductVersion()) > 0)
                    hostWithSmallerVersion = host;
            }
            return hostWithSmallerVersion;
        }

        public virtual int CpuSockets()
        {
            return Connection.Cache.Hosts.Sum(h => h.CpuSockets());
        }

        public bool HasGpu()
        {
            return Connection.Cache.PGPUs.Length > 0;
        }

        public bool HasVGpu()
        {
            return HasGpu() && Connection.Cache.PGPUs.Any(pGpu => pGpu.HasVGpu());
        }

        /// <summary>
        /// ssl_legacy is true if any host in the pool is in legacy mode
        /// </summary>
        public bool ssl_legacy()
        {
            return Connection.Cache.Hosts.Any(h => h.ssl_legacy);
        }

        #region Health Check settings

        public enum HealthCheckStatus
        {
            Disabled, Enabled, Undefined
        }

        public HealthCheckStatus GetHealthCheckStatus()
        {
            if (health_check_config == null || !health_check_config.ContainsKey(HEALTH_CHECK_ENROLLMENT))
                return HealthCheckStatus.Undefined;

            if (health_check_config[HEALTH_CHECK_ENROLLMENT] == "true")
                return HealthCheckStatus.Enabled;

            return HealthCheckStatus.Disabled;
        }

        #endregion

        #region IEquatable<Pool> Members

        /// <summary>
        /// Indicates whether the current object is equal to the specified object. This calls the implementation from XenObject.
        /// This implementation is required for ToStringWrapper.
        /// </summary>
        public bool Equals(Pool other)
        {
            return base.Equals(other);
        }

        public bool GetVmAutostartEnabled()
        {
            string auto_poweron = Get(other_config, "auto_poweron");
            if (auto_poweron == null)
            { 
                return false; 
            }

            return auto_poweron == "true" ? true : false;
        }

        public void SetVmAutostartEnabled(bool value)
        {
            if(other_config.ContainsKey("auto_poweron"))
            {
                other_config["auto_poweron"] = value ? "true" : "false";
            }
            else
            {
                other_config.Add("auto_poweron", value ? "true" : "false");
            }
        }

        #endregion
    }
}
