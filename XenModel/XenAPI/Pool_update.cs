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
    /// Pool-wide updates to the host software
    /// First published in XenServer 7.1.
    /// </summary>
    public partial class Pool_update : XenObject<Pool_update>
    {
        #region Constructors

        public Pool_update()
        {
        }

        public Pool_update(string uuid,
            string name_label,
            string name_description,
            string version,
            long installation_size,
            string key,
            List<update_after_apply_guidance> after_apply_guidance,
            XenRef<VDI> vdi,
            List<XenRef<Host>> hosts,
            Dictionary<string, string> other_config,
            bool enforce_homogeneity)
        {
            this.uuid = uuid;
            this.name_label = name_label;
            this.name_description = name_description;
            this.version = version;
            this.installation_size = installation_size;
            this.key = key;
            this.after_apply_guidance = after_apply_guidance;
            this.vdi = vdi;
            this.hosts = hosts;
            this.other_config = other_config;
            this.enforce_homogeneity = enforce_homogeneity;
        }

        /// <summary>
        /// Creates a new Pool_update from a Hashtable.
        /// Note that the fields not contained in the Hashtable
        /// will be created with their default values.
        /// </summary>
        /// <param name="table"></param>
        public Pool_update(Hashtable table)
            : this()
        {
            UpdateFrom(table);
        }

        #endregion

        /// <summary>
        /// Updates each field of this instance with the value of
        /// the corresponding field of a given Pool_update.
        /// </summary>
        public override void UpdateFrom(Pool_update record)
        {
            uuid = record.uuid;
            name_label = record.name_label;
            name_description = record.name_description;
            version = record.version;
            installation_size = record.installation_size;
            key = record.key;
            after_apply_guidance = record.after_apply_guidance;
            vdi = record.vdi;
            hosts = record.hosts;
            other_config = record.other_config;
            enforce_homogeneity = record.enforce_homogeneity;
        }

        /// <summary>
        /// Given a Hashtable with field-value pairs, it updates the fields of this Pool_update
        /// with the values listed in the Hashtable. Note that only the fields contained
        /// in the Hashtable will be updated and the rest will remain the same.
        /// </summary>
        /// <param name="table"></param>
        public void UpdateFrom(Hashtable table)
        {
            if (table.ContainsKey("uuid"))
                uuid = Marshalling.ParseString(table, "uuid");
            if (table.ContainsKey("name_label"))
                name_label = Marshalling.ParseString(table, "name_label");
            if (table.ContainsKey("name_description"))
                name_description = Marshalling.ParseString(table, "name_description");
            if (table.ContainsKey("version"))
                version = Marshalling.ParseString(table, "version");
            if (table.ContainsKey("installation_size"))
                installation_size = Marshalling.ParseLong(table, "installation_size");
            if (table.ContainsKey("key"))
                key = Marshalling.ParseString(table, "key");
            if (table.ContainsKey("after_apply_guidance"))
                after_apply_guidance = Helper.StringArrayToEnumList<update_after_apply_guidance>(Marshalling.ParseStringArray(table, "after_apply_guidance"));
            if (table.ContainsKey("vdi"))
                vdi = Marshalling.ParseRef<VDI>(table, "vdi");
            if (table.ContainsKey("hosts"))
                hosts = Marshalling.ParseSetRef<Host>(table, "hosts");
            if (table.ContainsKey("other_config"))
                other_config = Maps.ToDictionary_string_string(Marshalling.ParseHashTable(table, "other_config"));
            if (table.ContainsKey("enforce_homogeneity"))
                enforce_homogeneity = Marshalling.ParseBool(table, "enforce_homogeneity");
        }

        public bool DeepEquals(Pool_update other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Helper.AreEqual2(_uuid, other._uuid) &&
                Helper.AreEqual2(_name_label, other._name_label) &&
                Helper.AreEqual2(_name_description, other._name_description) &&
                Helper.AreEqual2(_version, other._version) &&
                Helper.AreEqual2(_installation_size, other._installation_size) &&
                Helper.AreEqual2(_key, other._key) &&
                Helper.AreEqual2(_after_apply_guidance, other._after_apply_guidance) &&
                Helper.AreEqual2(_vdi, other._vdi) &&
                Helper.AreEqual2(_hosts, other._hosts) &&
                Helper.AreEqual2(_other_config, other._other_config) &&
                Helper.AreEqual2(_enforce_homogeneity, other._enforce_homogeneity);
        }

        public override string SaveChanges(Session session, string opaqueRef, Pool_update server)
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
                    Pool_update.set_other_config(session, opaqueRef, _other_config);
                }

                return null;
            }
        }

        /// <summary>
        /// Get a record containing the current state of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static Pool_update get_record(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_record(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get a reference to the pool_update instance with the specified UUID.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">UUID of object to return</param>
        public static XenRef<Pool_update> get_by_uuid(Session session, string _uuid)
        {
            return session.JsonRpcClient.pool_update_get_by_uuid(session.opaque_ref, _uuid);
        }

        /// <summary>
        /// Get all the pool_update instances with the given label.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_label">label of object to return</param>
        public static List<XenRef<Pool_update>> get_by_name_label(Session session, string _label)
        {
            return session.JsonRpcClient.pool_update_get_by_name_label(session.opaque_ref, _label);
        }

        /// <summary>
        /// Get the uuid field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static string get_uuid(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_uuid(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the name/label field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static string get_name_label(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_name_label(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the name/description field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static string get_name_description(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_name_description(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the version field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static string get_version(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_version(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the installation_size field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static long get_installation_size(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_installation_size(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the key field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static string get_key(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_key(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the after_apply_guidance field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static List<update_after_apply_guidance> get_after_apply_guidance(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_after_apply_guidance(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the vdi field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static XenRef<VDI> get_vdi(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_vdi(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the hosts field of the given pool_update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static List<XenRef<Host>> get_hosts(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_hosts(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the other_config field of the given pool_update.
        /// First published in XenServer 7.3.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static Dictionary<string, string> get_other_config(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_other_config(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Get the enforce_homogeneity field of the given pool_update.
        /// First published in XenServer 7.3.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static bool get_enforce_homogeneity(Session session, string _pool_update)
        {
            return session.JsonRpcClient.pool_update_get_enforce_homogeneity(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Set the other_config field of the given pool_update.
        /// First published in XenServer 7.3.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        /// <param name="_other_config">New value to set</param>
        public static void set_other_config(Session session, string _pool_update, Dictionary<string, string> _other_config)
        {
            session.JsonRpcClient.pool_update_set_other_config(session.opaque_ref, _pool_update, _other_config);
        }

        /// <summary>
        /// Add the given key-value pair to the other_config field of the given pool_update.
        /// First published in XenServer 7.3.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        /// <param name="_key">Key to add</param>
        /// <param name="_value">Value to add</param>
        public static void add_to_other_config(Session session, string _pool_update, string _key, string _value)
        {
            session.JsonRpcClient.pool_update_add_to_other_config(session.opaque_ref, _pool_update, _key, _value);
        }

        /// <summary>
        /// Remove the given key and its corresponding value from the other_config field of the given pool_update.  If the key is not in that Map, then do nothing.
        /// First published in XenServer 7.3.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        /// <param name="_key">Key to remove</param>
        public static void remove_from_other_config(Session session, string _pool_update, string _key)
        {
            session.JsonRpcClient.pool_update_remove_from_other_config(session.opaque_ref, _pool_update, _key);
        }

        /// <summary>
        /// Introduce update VDI
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vdi">The VDI which contains a software update.</param>
        public static XenRef<Pool_update> introduce(Session session, string _vdi)
        {
            return session.JsonRpcClient.pool_update_introduce(session.opaque_ref, _vdi);
        }

        /// <summary>
        /// Introduce update VDI
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vdi">The VDI which contains a software update.</param>
        public static XenRef<Task> async_introduce(Session session, string _vdi)
        {
          return session.JsonRpcClient.async_pool_update_introduce(session.opaque_ref, _vdi);
        }

        /// <summary>
        /// Execute the precheck stage of the selected update on a host
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        /// <param name="_host">The host to run the prechecks on.</param>
        public static livepatch_status precheck(Session session, string _pool_update, string _host)
        {
            return session.JsonRpcClient.pool_update_precheck(session.opaque_ref, _pool_update, _host);
        }

        /// <summary>
        /// Execute the precheck stage of the selected update on a host
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        /// <param name="_host">The host to run the prechecks on.</param>
        public static XenRef<Task> async_precheck(Session session, string _pool_update, string _host)
        {
          return session.JsonRpcClient.async_pool_update_precheck(session.opaque_ref, _pool_update, _host);
        }

        /// <summary>
        /// Apply the selected update to a host
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        /// <param name="_host">The host to apply the update to.</param>
        public static void apply(Session session, string _pool_update, string _host)
        {
            session.JsonRpcClient.pool_update_apply(session.opaque_ref, _pool_update, _host);
        }

        /// <summary>
        /// Apply the selected update to a host
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        /// <param name="_host">The host to apply the update to.</param>
        public static XenRef<Task> async_apply(Session session, string _pool_update, string _host)
        {
          return session.JsonRpcClient.async_pool_update_apply(session.opaque_ref, _pool_update, _host);
        }

        /// <summary>
        /// Apply the selected update to all hosts in the pool
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static void pool_apply(Session session, string _pool_update)
        {
            session.JsonRpcClient.pool_update_pool_apply(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Apply the selected update to all hosts in the pool
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static XenRef<Task> async_pool_apply(Session session, string _pool_update)
        {
          return session.JsonRpcClient.async_pool_update_pool_apply(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Removes the update's files from all hosts in the pool, but does not revert the update
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static void pool_clean(Session session, string _pool_update)
        {
            session.JsonRpcClient.pool_update_pool_clean(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Removes the update's files from all hosts in the pool, but does not revert the update
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static XenRef<Task> async_pool_clean(Session session, string _pool_update)
        {
          return session.JsonRpcClient.async_pool_update_pool_clean(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Removes the database entry. Only works on unapplied update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static void destroy(Session session, string _pool_update)
        {
            session.JsonRpcClient.pool_update_destroy(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Removes the database entry. Only works on unapplied update.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pool_update">The opaque_ref of the given pool_update</param>
        public static XenRef<Task> async_destroy(Session session, string _pool_update)
        {
          return session.JsonRpcClient.async_pool_update_destroy(session.opaque_ref, _pool_update);
        }

        /// <summary>
        /// Return a list of all the pool_updates known to the system.
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        public static List<XenRef<Pool_update>> get_all(Session session)
        {
            return session.JsonRpcClient.pool_update_get_all(session.opaque_ref);
        }

        /// <summary>
        /// Get all the pool_update Records at once, in a single XML RPC call
        /// First published in XenServer 7.1.
        /// </summary>
        /// <param name="session">The session</param>
        public static Dictionary<XenRef<Pool_update>, Pool_update> get_all_records(Session session)
        {
            return session.JsonRpcClient.pool_update_get_all_records(session.opaque_ref);
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
        /// a human-readable name
        /// </summary>
        public virtual string name_label
        {
            get { return _name_label; }
            set
            {
                if (!Helper.AreEqual(value, _name_label))
                {
                    _name_label = value;
                    NotifyPropertyChanged("name_label");
                }
            }
        }
        private string _name_label = "";

        /// <summary>
        /// a notes field containing human-readable description
        /// </summary>
        public virtual string name_description
        {
            get { return _name_description; }
            set
            {
                if (!Helper.AreEqual(value, _name_description))
                {
                    _name_description = value;
                    NotifyPropertyChanged("name_description");
                }
            }
        }
        private string _name_description = "";

        /// <summary>
        /// Update version number
        /// </summary>
        public virtual string version
        {
            get { return _version; }
            set
            {
                if (!Helper.AreEqual(value, _version))
                {
                    _version = value;
                    NotifyPropertyChanged("version");
                }
            }
        }
        private string _version = "";

        /// <summary>
        /// Size of the update in bytes
        /// </summary>
        public virtual long installation_size
        {
            get { return _installation_size; }
            set
            {
                if (!Helper.AreEqual(value, _installation_size))
                {
                    _installation_size = value;
                    NotifyPropertyChanged("installation_size");
                }
            }
        }
        private long _installation_size = 0;

        /// <summary>
        /// GPG key of the update
        /// </summary>
        public virtual string key
        {
            get { return _key; }
            set
            {
                if (!Helper.AreEqual(value, _key))
                {
                    _key = value;
                    NotifyPropertyChanged("key");
                }
            }
        }
        private string _key = "";

        /// <summary>
        /// What the client should do after this update has been applied.
        /// </summary>
        public virtual List<update_after_apply_guidance> after_apply_guidance
        {
            get { return _after_apply_guidance; }
            set
            {
                if (!Helper.AreEqual(value, _after_apply_guidance))
                {
                    _after_apply_guidance = value;
                    NotifyPropertyChanged("after_apply_guidance");
                }
            }
        }
        private List<update_after_apply_guidance> _after_apply_guidance = new List<update_after_apply_guidance>() {};

        /// <summary>
        /// VDI the update was uploaded to
        /// </summary>
        [JsonConverter(typeof(XenRefConverter<VDI>))]
        public virtual XenRef<VDI> vdi
        {
            get { return _vdi; }
            set
            {
                if (!Helper.AreEqual(value, _vdi))
                {
                    _vdi = value;
                    NotifyPropertyChanged("vdi");
                }
            }
        }
        private XenRef<VDI> _vdi = new XenRef<VDI>(Helper.NullOpaqueRef);

        /// <summary>
        /// The hosts that have applied this update.
        /// </summary>
        [JsonConverter(typeof(XenRefListConverter<Host>))]
        public virtual List<XenRef<Host>> hosts
        {
            get { return _hosts; }
            set
            {
                if (!Helper.AreEqual(value, _hosts))
                {
                    _hosts = value;
                    NotifyPropertyChanged("hosts");
                }
            }
        }
        private List<XenRef<Host>> _hosts = new List<XenRef<Host>>() {};

        /// <summary>
        /// additional configuration
        /// First published in XenServer 7.3.
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

        /// <summary>
        /// Flag - if true, all hosts in a pool must apply this update
        /// First published in XenServer 7.3.
        /// </summary>
        public virtual bool enforce_homogeneity
        {
            get { return _enforce_homogeneity; }
            set
            {
                if (!Helper.AreEqual(value, _enforce_homogeneity))
                {
                    _enforce_homogeneity = value;
                    NotifyPropertyChanged("enforce_homogeneity");
                }
            }
        }
        private bool _enforce_homogeneity = false;
    }
}
