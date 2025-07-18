/*
 * Copyright (c) Cloud Software Group, Inc.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 *   1) Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *
 *   2) Redistributions in binary form must reproduce the above
 *      copyright notice, this list of conditions and the following
 *      disclaimer in the documentation and/or other materials
 *      provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;


namespace XenAPI
{
    /// <summary>
    /// The metrics associated with a host
    /// First published in XenServer 4.0.
    /// </summary>
    public partial class Host_metrics : XenObject<Host_metrics>
    {
        #region Constructors

        public Host_metrics()
        {
        }

        public Host_metrics(string uuid,
            long memory_total,
            long memory_free,
            bool live,
            DateTime last_updated,
            Dictionary<string, string> other_config)
        {
            this.uuid = uuid;
            this.memory_total = memory_total;
            this.memory_free = memory_free;
            this.live = live;
            this.last_updated = last_updated;
            this.other_config = other_config;
        }

        /// <summary>
        /// Creates a new Host_metrics from a Hashtable.
        /// Note that the fields not contained in the Hashtable
        /// will be created with their default values.
        /// </summary>
        /// <param name="table"></param>
        public Host_metrics(Hashtable table)
            : this()
        {
            UpdateFrom(table);
        }

        #endregion

        /// <summary>
        /// Updates each field of this instance with the value of
        /// the corresponding field of a given Host_metrics.
        /// </summary>
        public override void UpdateFrom(Host_metrics record)
        {
            uuid = record.uuid;
            memory_total = record.memory_total;
            memory_free = record.memory_free;
            live = record.live;
            last_updated = record.last_updated;
            other_config = record.other_config;
        }

        /// <summary>
        /// Given a Hashtable with field-value pairs, it updates the fields of this Host_metrics
        /// with the values listed in the Hashtable. Note that only the fields contained
        /// in the Hashtable will be updated and the rest will remain the same.
        /// </summary>
        /// <param name="table"></param>
        public void UpdateFrom(Hashtable table)
        {
            if (table.ContainsKey("uuid"))
                uuid = Marshalling.ParseString(table, "uuid");
            if (table.ContainsKey("memory_total"))
                memory_total = Marshalling.ParseLong(table, "memory_total");
            if (table.ContainsKey("memory_free"))
                memory_free = Marshalling.ParseLong(table, "memory_free");
            if (table.ContainsKey("live"))
                live = Marshalling.ParseBool(table, "live");
            if (table.ContainsKey("last_updated"))
                last_updated = Marshalling.ParseDateTime(table, "last_updated");
            if (table.ContainsKey("other_config"))
                other_config = Maps.ToDictionary_string_string(Marshalling.ParseHashTable(table, "other_config"));
        }

        public bool DeepEquals(Host_metrics other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Helper.AreEqual2(_uuid, other._uuid) &&
                Helper.AreEqual2(_memory_total, other._memory_total) &&
                Helper.AreEqual2(_memory_free, other._memory_free) &&
                Helper.AreEqual2(_live, other._live) &&
                Helper.AreEqual2(_last_updated, other._last_updated) &&
                Helper.AreEqual2(_other_config, other._other_config);
        }

        public override string SaveChanges(Session session, string opaqueRef, Host_metrics server)
        {
            if (opaqueRef == null)
            {
                System.Diagnostics.Debug.Assert(false, "Cannot create instances of this type on the server");
                return "";
            }
            else
            {
                if (!Helper.AreEqual2(_other_config, server._other_config))
                {
                    Host_metrics.set_other_config(session, opaqueRef, _other_config);
                }

                return null;
            }
        }

        /// <summary>
        /// Get a record containing the current state of the given host_metrics.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        public static Host_metrics get_record(Session session, string _host_metrics)
        {
            return session.JsonRpcClient.host_metrics_get_record(session.opaque_ref, _host_metrics);
        }

        /// <summary>
        /// Get a reference to the host_metrics instance with the specified UUID.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">UUID of object to return</param>
        public static XenRef<Host_metrics> get_by_uuid(Session session, string _uuid)
        {
            return session.JsonRpcClient.host_metrics_get_by_uuid(session.opaque_ref, _uuid);
        }

        /// <summary>
        /// Get the uuid field of the given host_metrics.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        public static string get_uuid(Session session, string _host_metrics)
        {
            return session.JsonRpcClient.host_metrics_get_uuid(session.opaque_ref, _host_metrics);
        }

        /// <summary>
        /// Get the memory/total field of the given host_metrics.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        public static long get_memory_total(Session session, string _host_metrics)
        {
            return session.JsonRpcClient.host_metrics_get_memory_total(session.opaque_ref, _host_metrics);
        }

        /// <summary>
        /// Get the memory/free field of the given host_metrics.
        /// First published in XenServer 4.0.
        /// Deprecated since XenServer 5.6.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        [Deprecated("XenServer 5.6")]
        public static long get_memory_free(Session session, string _host_metrics)
        {
            return session.JsonRpcClient.host_metrics_get_memory_free(session.opaque_ref, _host_metrics);
        }

        /// <summary>
        /// Get the live field of the given host_metrics.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        public static bool get_live(Session session, string _host_metrics)
        {
            return session.JsonRpcClient.host_metrics_get_live(session.opaque_ref, _host_metrics);
        }

        /// <summary>
        /// Get the last_updated field of the given host_metrics.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        public static DateTime get_last_updated(Session session, string _host_metrics)
        {
            return session.JsonRpcClient.host_metrics_get_last_updated(session.opaque_ref, _host_metrics);
        }

        /// <summary>
        /// Get the other_config field of the given host_metrics.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        public static Dictionary<string, string> get_other_config(Session session, string _host_metrics)
        {
            return session.JsonRpcClient.host_metrics_get_other_config(session.opaque_ref, _host_metrics);
        }

        /// <summary>
        /// Set the other_config field of the given host_metrics.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        /// <param name="_other_config">New value to set</param>
        public static void set_other_config(Session session, string _host_metrics, Dictionary<string, string> _other_config)
        {
            session.JsonRpcClient.host_metrics_set_other_config(session.opaque_ref, _host_metrics, _other_config);
        }

        /// <summary>
        /// Add the given key-value pair to the other_config field of the given host_metrics.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        /// <param name="_key">Key to add</param>
        /// <param name="_value">Value to add</param>
        public static void add_to_other_config(Session session, string _host_metrics, string _key, string _value)
        {
            session.JsonRpcClient.host_metrics_add_to_other_config(session.opaque_ref, _host_metrics, _key, _value);
        }

        /// <summary>
        /// Remove the given key and its corresponding value from the other_config field of the given host_metrics.  If the key is not in that Map, then do nothing.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host_metrics">The opaque_ref of the given host_metrics</param>
        /// <param name="_key">Key to remove</param>
        public static void remove_from_other_config(Session session, string _host_metrics, string _key)
        {
            session.JsonRpcClient.host_metrics_remove_from_other_config(session.opaque_ref, _host_metrics, _key);
        }

        /// <summary>
        /// Return a list of all the host_metrics instances known to the system.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static List<XenRef<Host_metrics>> get_all(Session session)
        {
            return session.JsonRpcClient.host_metrics_get_all(session.opaque_ref);
        }

        /// <summary>
        /// Get all the host_metrics Records at once, in a single XML RPC call
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static Dictionary<XenRef<Host_metrics>, Host_metrics> get_all_records(Session session)
        {
            return session.JsonRpcClient.host_metrics_get_all_records(session.opaque_ref);
        }

        /// <summary>
        /// Unique identifier/object reference
        /// </summary>
        public virtual string uuid
        {
            get { return _uuid; }
            set
            {
                if (!Helper.AreEqual(value, _uuid))
                {
                    _uuid = value;
                    NotifyPropertyChanged("uuid");
                }
            }
        }
        private string _uuid = "";

        /// <summary>
        /// Total host memory (bytes)
        /// </summary>
        public virtual long memory_total
        {
            get { return _memory_total; }
            set
            {
                if (!Helper.AreEqual(value, _memory_total))
                {
                    _memory_total = value;
                    NotifyPropertyChanged("memory_total");
                }
            }
        }
        private long _memory_total;

        /// <summary>
        /// Free host memory (bytes)
        /// </summary>
        public virtual long memory_free
        {
            get { return _memory_free; }
            set
            {
                if (!Helper.AreEqual(value, _memory_free))
                {
                    _memory_free = value;
                    NotifyPropertyChanged("memory_free");
                }
            }
        }
        private long _memory_free = 0;

        /// <summary>
        /// Pool master thinks this host is live
        /// </summary>
        public virtual bool live
        {
            get { return _live; }
            set
            {
                if (!Helper.AreEqual(value, _live))
                {
                    _live = value;
                    NotifyPropertyChanged("live");
                }
            }
        }
        private bool _live;

        /// <summary>
        /// Time at which this information was last updated
        /// </summary>
        [JsonConverter(typeof(XenDateTimeConverter))]
        public virtual DateTime last_updated
        {
            get { return _last_updated; }
            set
            {
                if (!Helper.AreEqual(value, _last_updated))
                {
                    _last_updated = value;
                    NotifyPropertyChanged("last_updated");
                }
            }
        }
        private DateTime _last_updated;

        /// <summary>
        /// additional configuration
        /// First published in XenServer 5.0.
        /// </summary>
        [JsonConverter(typeof(StringStringMapConverter))]
        public virtual Dictionary<string, string> other_config
        {
            get { return _other_config; }
            set
            {
                if (!Helper.AreEqual(value, _other_config))
                {
                    _other_config = value;
                    NotifyPropertyChanged("other_config");
                }
            }
        }
        private Dictionary<string, string> _other_config = new Dictionary<string, string>() {};
    }
}
