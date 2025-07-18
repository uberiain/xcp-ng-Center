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
    /// A storage repository
    /// First published in XenServer 4.0.
    /// </summary>
    public partial class SR : XenObject<SR>
    {
        #region Constructors

        public SR()
        {
        }

        public SR(string uuid,
            string name_label,
            string name_description,
            List<storage_operations> allowed_operations,
            Dictionary<string, storage_operations> current_operations,
            List<XenRef<VDI>> VDIs,
            List<XenRef<PBD>> PBDs,
            long virtual_allocation,
            long physical_utilisation,
            long physical_size,
            string type,
            string content_type,
            bool shared,
            Dictionary<string, string> other_config,
            string[] tags,
            Dictionary<string, string> sm_config,
            Dictionary<string, XenRef<Blob>> blobs,
            bool local_cache_enabled,
            XenRef<DR_task> introduced_by,
            bool clustered,
            bool is_tools_sr)
        {
            this.uuid = uuid;
            this.name_label = name_label;
            this.name_description = name_description;
            this.allowed_operations = allowed_operations;
            this.current_operations = current_operations;
            this.VDIs = VDIs;
            this.PBDs = PBDs;
            this.virtual_allocation = virtual_allocation;
            this.physical_utilisation = physical_utilisation;
            this.physical_size = physical_size;
            this.type = type;
            this.content_type = content_type;
            this.shared = shared;
            this.other_config = other_config;
            this.tags = tags;
            this.sm_config = sm_config;
            this.blobs = blobs;
            this.local_cache_enabled = local_cache_enabled;
            this.introduced_by = introduced_by;
            this.clustered = clustered;
            this.is_tools_sr = is_tools_sr;
        }

        /// <summary>
        /// Creates a new SR from a Hashtable.
        /// Note that the fields not contained in the Hashtable
        /// will be created with their default values.
        /// </summary>
        /// <param name="table"></param>
        public SR(Hashtable table)
            : this()
        {
            UpdateFrom(table);
        }

        #endregion

        /// <summary>
        /// Updates each field of this instance with the value of
        /// the corresponding field of a given SR.
        /// </summary>
        public override void UpdateFrom(SR record)
        {
            uuid = record.uuid;
            name_label = record.name_label;
            name_description = record.name_description;
            allowed_operations = record.allowed_operations;
            current_operations = record.current_operations;
            VDIs = record.VDIs;
            PBDs = record.PBDs;
            virtual_allocation = record.virtual_allocation;
            physical_utilisation = record.physical_utilisation;
            physical_size = record.physical_size;
            type = record.type;
            content_type = record.content_type;
            shared = record.shared;
            other_config = record.other_config;
            tags = record.tags;
            sm_config = record.sm_config;
            blobs = record.blobs;
            local_cache_enabled = record.local_cache_enabled;
            introduced_by = record.introduced_by;
            clustered = record.clustered;
            is_tools_sr = record.is_tools_sr;
        }

        /// <summary>
        /// Given a Hashtable with field-value pairs, it updates the fields of this SR
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
            if (table.ContainsKey("allowed_operations"))
                allowed_operations = Helper.StringArrayToEnumList<storage_operations>(Marshalling.ParseStringArray(table, "allowed_operations"));
            if (table.ContainsKey("current_operations"))
                current_operations = Maps.ToDictionary_string_storage_operations(Marshalling.ParseHashTable(table, "current_operations"));
            if (table.ContainsKey("VDIs"))
                VDIs = Marshalling.ParseSetRef<VDI>(table, "VDIs");
            if (table.ContainsKey("PBDs"))
                PBDs = Marshalling.ParseSetRef<PBD>(table, "PBDs");
            if (table.ContainsKey("virtual_allocation"))
                virtual_allocation = Marshalling.ParseLong(table, "virtual_allocation");
            if (table.ContainsKey("physical_utilisation"))
                physical_utilisation = Marshalling.ParseLong(table, "physical_utilisation");
            if (table.ContainsKey("physical_size"))
                physical_size = Marshalling.ParseLong(table, "physical_size");
            if (table.ContainsKey("type"))
                type = Marshalling.ParseString(table, "type");
            if (table.ContainsKey("content_type"))
                content_type = Marshalling.ParseString(table, "content_type");
            if (table.ContainsKey("shared"))
                shared = Marshalling.ParseBool(table, "shared");
            if (table.ContainsKey("other_config"))
                other_config = Maps.ToDictionary_string_string(Marshalling.ParseHashTable(table, "other_config"));
            if (table.ContainsKey("tags"))
                tags = Marshalling.ParseStringArray(table, "tags");
            if (table.ContainsKey("sm_config"))
                sm_config = Maps.ToDictionary_string_string(Marshalling.ParseHashTable(table, "sm_config"));
            if (table.ContainsKey("blobs"))
                blobs = Maps.ToDictionary_string_XenRefBlob(Marshalling.ParseHashTable(table, "blobs"));
            if (table.ContainsKey("local_cache_enabled"))
                local_cache_enabled = Marshalling.ParseBool(table, "local_cache_enabled");
            if (table.ContainsKey("introduced_by"))
                introduced_by = Marshalling.ParseRef<DR_task>(table, "introduced_by");
            if (table.ContainsKey("clustered"))
                clustered = Marshalling.ParseBool(table, "clustered");
            if (table.ContainsKey("is_tools_sr"))
                is_tools_sr = Marshalling.ParseBool(table, "is_tools_sr");
        }

        public bool DeepEquals(SR other, bool ignoreCurrentOperations)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (!ignoreCurrentOperations && !Helper.AreEqual2(current_operations, other.current_operations))
                return false;

            return Helper.AreEqual2(_uuid, other._uuid) &&
                Helper.AreEqual2(_name_label, other._name_label) &&
                Helper.AreEqual2(_name_description, other._name_description) &&
                Helper.AreEqual2(_allowed_operations, other._allowed_operations) &&
                Helper.AreEqual2(_VDIs, other._VDIs) &&
                Helper.AreEqual2(_PBDs, other._PBDs) &&
                Helper.AreEqual2(_virtual_allocation, other._virtual_allocation) &&
                Helper.AreEqual2(_physical_utilisation, other._physical_utilisation) &&
                Helper.AreEqual2(_physical_size, other._physical_size) &&
                Helper.AreEqual2(_type, other._type) &&
                Helper.AreEqual2(_content_type, other._content_type) &&
                Helper.AreEqual2(_shared, other._shared) &&
                Helper.AreEqual2(_other_config, other._other_config) &&
                Helper.AreEqual2(_tags, other._tags) &&
                Helper.AreEqual2(_sm_config, other._sm_config) &&
                Helper.AreEqual2(_blobs, other._blobs) &&
                Helper.AreEqual2(_local_cache_enabled, other._local_cache_enabled) &&
                Helper.AreEqual2(_introduced_by, other._introduced_by) &&
                Helper.AreEqual2(_clustered, other._clustered) &&
                Helper.AreEqual2(_is_tools_sr, other._is_tools_sr);
        }

        public override string SaveChanges(Session session, string opaqueRef, SR server)
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
                    SR.set_other_config(session, opaqueRef, _other_config);
                }
                if (!Helper.AreEqual2(_tags, server._tags))
                {
                    SR.set_tags(session, opaqueRef, _tags);
                }
                if (!Helper.AreEqual2(_sm_config, server._sm_config))
                {
                    SR.set_sm_config(session, opaqueRef, _sm_config);
                }
                if (!Helper.AreEqual2(_name_label, server._name_label))
                {
                    SR.set_name_label(session, opaqueRef, _name_label);
                }
                if (!Helper.AreEqual2(_name_description, server._name_description))
                {
                    SR.set_name_description(session, opaqueRef, _name_description);
                }
                if (!Helper.AreEqual2(_physical_size, server._physical_size))
                {
                    SR.set_physical_size(session, opaqueRef, _physical_size);
                }

                return null;
            }
        }

        /// <summary>
        /// Get a record containing the current state of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static SR get_record(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_record(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get a reference to the SR instance with the specified UUID.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">UUID of object to return</param>
        public static XenRef<SR> get_by_uuid(Session session, string _uuid)
        {
            return session.JsonRpcClient.sr_get_by_uuid(session.opaque_ref, _uuid);
        }

        /// <summary>
        /// Get all the SR instances with the given label.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_label">label of object to return</param>
        public static List<XenRef<SR>> get_by_name_label(Session session, string _label)
        {
            return session.JsonRpcClient.sr_get_by_name_label(session.opaque_ref, _label);
        }

        /// <summary>
        /// Get the uuid field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static string get_uuid(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_uuid(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the name/label field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static string get_name_label(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_name_label(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the name/description field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static string get_name_description(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_name_description(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the allowed_operations field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static List<storage_operations> get_allowed_operations(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_allowed_operations(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the current_operations field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static Dictionary<string, storage_operations> get_current_operations(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_current_operations(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the VDIs field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static List<XenRef<VDI>> get_VDIs(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_vdis(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the PBDs field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static List<XenRef<PBD>> get_PBDs(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_pbds(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the virtual_allocation field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static long get_virtual_allocation(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_virtual_allocation(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the physical_utilisation field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static long get_physical_utilisation(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_physical_utilisation(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the physical_size field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static long get_physical_size(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_physical_size(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the type field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static string get_type(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_type(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the content_type field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static string get_content_type(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_content_type(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the shared field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static bool get_shared(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_shared(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the other_config field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static Dictionary<string, string> get_other_config(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_other_config(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the tags field of the given SR.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static string[] get_tags(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_tags(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the sm_config field of the given SR.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static Dictionary<string, string> get_sm_config(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_sm_config(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the blobs field of the given SR.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static Dictionary<string, XenRef<Blob>> get_blobs(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_blobs(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the local_cache_enabled field of the given SR.
        /// First published in XenServer 5.6 FP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static bool get_local_cache_enabled(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_local_cache_enabled(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the introduced_by field of the given SR.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<DR_task> get_introduced_by(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_introduced_by(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the clustered field of the given SR.
        /// First published in XenServer 7.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static bool get_clustered(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_clustered(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Get the is_tools_sr field of the given SR.
        /// First published in XenServer 7.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static bool get_is_tools_sr(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_is_tools_sr(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Set the other_config field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_other_config">New value to set</param>
        public static void set_other_config(Session session, string _sr, Dictionary<string, string> _other_config)
        {
            session.JsonRpcClient.sr_set_other_config(session.opaque_ref, _sr, _other_config);
        }

        /// <summary>
        /// Add the given key-value pair to the other_config field of the given SR.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_key">Key to add</param>
        /// <param name="_value">Value to add</param>
        public static void add_to_other_config(Session session, string _sr, string _key, string _value)
        {
            session.JsonRpcClient.sr_add_to_other_config(session.opaque_ref, _sr, _key, _value);
        }

        /// <summary>
        /// Remove the given key and its corresponding value from the other_config field of the given SR.  If the key is not in that Map, then do nothing.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_key">Key to remove</param>
        public static void remove_from_other_config(Session session, string _sr, string _key)
        {
            session.JsonRpcClient.sr_remove_from_other_config(session.opaque_ref, _sr, _key);
        }

        /// <summary>
        /// Set the tags field of the given SR.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_tags">New value to set</param>
        public static void set_tags(Session session, string _sr, string[] _tags)
        {
            session.JsonRpcClient.sr_set_tags(session.opaque_ref, _sr, _tags);
        }

        /// <summary>
        /// Add the given value to the tags field of the given SR.  If the value is already in that Set, then do nothing.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">New value to add</param>
        public static void add_tags(Session session, string _sr, string _value)
        {
            session.JsonRpcClient.sr_add_tags(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Remove the given value from the tags field of the given SR.  If the value is not in that Set, then do nothing.
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">Value to remove</param>
        public static void remove_tags(Session session, string _sr, string _value)
        {
            session.JsonRpcClient.sr_remove_tags(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Set the sm_config field of the given SR.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_sm_config">New value to set</param>
        public static void set_sm_config(Session session, string _sr, Dictionary<string, string> _sm_config)
        {
            session.JsonRpcClient.sr_set_sm_config(session.opaque_ref, _sr, _sm_config);
        }

        /// <summary>
        /// Add the given key-value pair to the sm_config field of the given SR.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_key">Key to add</param>
        /// <param name="_value">Value to add</param>
        public static void add_to_sm_config(Session session, string _sr, string _key, string _value)
        {
            session.JsonRpcClient.sr_add_to_sm_config(session.opaque_ref, _sr, _key, _value);
        }

        /// <summary>
        /// Remove the given key and its corresponding value from the sm_config field of the given SR.  If the key is not in that Map, then do nothing.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_key">Key to remove</param>
        public static void remove_from_sm_config(Session session, string _sr, string _key)
        {
            session.JsonRpcClient.sr_remove_from_sm_config(session.opaque_ref, _sr, _key);
        }

        /// <summary>
        /// Create a new Storage Repository and introduce it into the managed system, creating both SR record and PBD record to attach it to current host (with specified device_config parameters)
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        public static XenRef<SR> create(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type, bool _shared)
        {
            return session.JsonRpcClient.sr_create(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type, _shared);
        }

        /// <summary>
        /// Create a new Storage Repository and introduce it into the managed system, creating both SR record and PBD record to attach it to current host (with specified device_config parameters)
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        public static XenRef<Task> async_create(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type, bool _shared)
        {
          return session.JsonRpcClient.async_sr_create(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type, _shared);
        }

        /// <summary>
        /// Create a new Storage Repository and introduce it into the managed system, creating both SR record and PBD record to attach it to current host (with specified device_config parameters)
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        /// <param name="_sm_config">Storage backend specific configuration options First published in XenServer 4.1.</param>
        public static XenRef<SR> create(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type, bool _shared, Dictionary<string, string> _sm_config)
        {
            return session.JsonRpcClient.sr_create(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type, _shared, _sm_config);
        }

        /// <summary>
        /// Create a new Storage Repository and introduce it into the managed system, creating both SR record and PBD record to attach it to current host (with specified device_config parameters)
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        /// <param name="_sm_config">Storage backend specific configuration options First published in XenServer 4.1.</param>
        public static XenRef<Task> async_create(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type, bool _shared, Dictionary<string, string> _sm_config)
        {
          return session.JsonRpcClient.async_sr_create(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type, _shared, _sm_config);
        }

        /// <summary>
        /// Introduce a new Storage Repository into the managed system
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">The uuid assigned to the introduced SR</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        public static XenRef<SR> introduce(Session session, string _uuid, string _name_label, string _name_description, string _type, string _content_type, bool _shared)
        {
            return session.JsonRpcClient.sr_introduce(session.opaque_ref, _uuid, _name_label, _name_description, _type, _content_type, _shared);
        }

        /// <summary>
        /// Introduce a new Storage Repository into the managed system
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">The uuid assigned to the introduced SR</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        public static XenRef<Task> async_introduce(Session session, string _uuid, string _name_label, string _name_description, string _type, string _content_type, bool _shared)
        {
          return session.JsonRpcClient.async_sr_introduce(session.opaque_ref, _uuid, _name_label, _name_description, _type, _content_type, _shared);
        }

        /// <summary>
        /// Introduce a new Storage Repository into the managed system
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">The uuid assigned to the introduced SR</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        /// <param name="_sm_config">Storage backend specific configuration options First published in XenServer 4.1.</param>
        public static XenRef<SR> introduce(Session session, string _uuid, string _name_label, string _name_description, string _type, string _content_type, bool _shared, Dictionary<string, string> _sm_config)
        {
            return session.JsonRpcClient.sr_introduce(session.opaque_ref, _uuid, _name_label, _name_description, _type, _content_type, _shared, _sm_config);
        }

        /// <summary>
        /// Introduce a new Storage Repository into the managed system
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">The uuid assigned to the introduced SR</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_shared">True if the SR (is capable of) being shared by multiple hosts</param>
        /// <param name="_sm_config">Storage backend specific configuration options First published in XenServer 4.1.</param>
        public static XenRef<Task> async_introduce(Session session, string _uuid, string _name_label, string _name_description, string _type, string _content_type, bool _shared, Dictionary<string, string> _sm_config)
        {
          return session.JsonRpcClient.async_sr_introduce(session.opaque_ref, _uuid, _name_label, _name_description, _type, _content_type, _shared, _sm_config);
        }

        /// <summary>
        /// Create a new Storage Repository on disk. This call is deprecated: use SR.create instead.
        /// First published in XenServer 4.0.
        /// Deprecated since XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        [Deprecated("XenServer 4.1")]
        public static string make(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type)
        {
            return session.JsonRpcClient.sr_make(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type);
        }

        /// <summary>
        /// Create a new Storage Repository on disk. This call is deprecated: use SR.create instead.
        /// First published in XenServer 4.0.
        /// Deprecated since XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        [Deprecated("XenServer 4.1")]
        public static XenRef<Task> async_make(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type)
        {
          return session.JsonRpcClient.async_sr_make(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type);
        }

        /// <summary>
        /// Create a new Storage Repository on disk. This call is deprecated: use SR.create instead.
        /// First published in XenServer 4.0.
        /// Deprecated since XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_sm_config">Storage backend specific configuration options First published in XenServer 4.1.</param>
        [Deprecated("XenServer 4.1")]
        public static string make(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type, Dictionary<string, string> _sm_config)
        {
            return session.JsonRpcClient.sr_make(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type, _sm_config);
        }

        /// <summary>
        /// Create a new Storage Repository on disk. This call is deprecated: use SR.create instead.
        /// First published in XenServer 4.0.
        /// Deprecated since XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_physical_size">The physical size of the new storage repository</param>
        /// <param name="_name_label">The name of the new storage repository</param>
        /// <param name="_name_description">The description of the new storage repository</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_content_type">The type of the new SRs content, if required (e.g. ISOs)</param>
        /// <param name="_sm_config">Storage backend specific configuration options First published in XenServer 4.1.</param>
        [Deprecated("XenServer 4.1")]
        public static XenRef<Task> async_make(Session session, string _host, Dictionary<string, string> _device_config, long _physical_size, string _name_label, string _name_description, string _type, string _content_type, Dictionary<string, string> _sm_config)
        {
          return session.JsonRpcClient.async_sr_make(session.opaque_ref, _host, _device_config, _physical_size, _name_label, _name_description, _type, _content_type, _sm_config);
        }

        /// <summary>
        /// Destroy specified SR, removing SR-record from database and remove SR from disk. (In order to affect this operation the appropriate device_config is read from the specified SR's PBD on current host)
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void destroy(Session session, string _sr)
        {
            session.JsonRpcClient.sr_destroy(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Destroy specified SR, removing SR-record from database and remove SR from disk. (In order to affect this operation the appropriate device_config is read from the specified SR's PBD on current host)
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_destroy(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_destroy(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Removing specified SR-record from database, without attempting to remove SR from disk
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void forget(Session session, string _sr)
        {
            session.JsonRpcClient.sr_forget(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Removing specified SR-record from database, without attempting to remove SR from disk
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_forget(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_forget(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Refresh the fields on the SR object
        /// First published in XenServer 4.1.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void update(Session session, string _sr)
        {
            session.JsonRpcClient.sr_update(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Refresh the fields on the SR object
        /// First published in XenServer 4.1.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_update(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_update(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Return a set of all the SR types supported by the system
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static string[] get_supported_types(Session session)
        {
            return session.JsonRpcClient.sr_get_supported_types(session.opaque_ref);
        }

        /// <summary>
        /// Refreshes the list of VDIs associated with an SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void scan(Session session, string _sr)
        {
            session.JsonRpcClient.sr_scan(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Refreshes the list of VDIs associated with an SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_scan(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_scan(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Perform a backend-specific scan, using the given device_config.  If the device_config is complete, then this will return a list of the SRs present of this type on the device, if any.  If the device_config is partial, then a backend-specific scan will be performed, returning results that will guide the user in improving the device_config.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        public static string probe(Session session, string _host, Dictionary<string, string> _device_config)
        {
            return session.JsonRpcClient.sr_probe(session.opaque_ref, _host, _device_config);
        }

        /// <summary>
        /// Perform a backend-specific scan, using the given device_config.  If the device_config is complete, then this will return a list of the SRs present of this type on the device, if any.  If the device_config is partial, then a backend-specific scan will be performed, returning results that will guide the user in improving the device_config.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        public static XenRef<Task> async_probe(Session session, string _host, Dictionary<string, string> _device_config)
        {
          return session.JsonRpcClient.async_sr_probe(session.opaque_ref, _host, _device_config);
        }

        /// <summary>
        /// Perform a backend-specific scan, using the given device_config.  If the device_config is complete, then this will return a list of the SRs present of this type on the device, if any.  If the device_config is partial, then a backend-specific scan will be performed, returning results that will guide the user in improving the device_config.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_sm_config">Storage backend specific configuration options</param>
        public static string probe(Session session, string _host, Dictionary<string, string> _device_config, string _type, Dictionary<string, string> _sm_config)
        {
            return session.JsonRpcClient.sr_probe(session.opaque_ref, _host, _device_config, _type, _sm_config);
        }

        /// <summary>
        /// Perform a backend-specific scan, using the given device_config.  If the device_config is complete, then this will return a list of the SRs present of this type on the device, if any.  If the device_config is partial, then a backend-specific scan will be performed, returning results that will guide the user in improving the device_config.
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_sm_config">Storage backend specific configuration options</param>
        public static XenRef<Task> async_probe(Session session, string _host, Dictionary<string, string> _device_config, string _type, Dictionary<string, string> _sm_config)
        {
          return session.JsonRpcClient.async_sr_probe(session.opaque_ref, _host, _device_config, _type, _sm_config);
        }

        /// <summary>
        /// Perform a backend-specific scan, using the given device_config.  If the device_config is complete, then this will return a list of the SRs present of this type on the device, if any.  If the device_config is partial, then a backend-specific scan will be performed, returning results that will guide the user in improving the device_config.
        /// First published in XenServer 7.6.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_sm_config">Storage backend specific configuration options</param>
        public static List<Probe_result> probe_ext(Session session, string _host, Dictionary<string, string> _device_config, string _type, Dictionary<string, string> _sm_config)
        {
            return session.JsonRpcClient.sr_probe_ext(session.opaque_ref, _host, _device_config, _type, _sm_config);
        }

        /// <summary>
        /// Perform a backend-specific scan, using the given device_config.  If the device_config is complete, then this will return a list of the SRs present of this type on the device, if any.  If the device_config is partial, then a backend-specific scan will be performed, returning results that will guide the user in improving the device_config.
        /// First published in XenServer 7.6.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_host">The host to create/make the SR on</param>
        /// <param name="_device_config">The device config string that will be passed to backend SR driver</param>
        /// <param name="_type">The type of the SR; used to specify the SR backend driver to use</param>
        /// <param name="_sm_config">Storage backend specific configuration options</param>
        public static XenRef<Task> async_probe_ext(Session session, string _host, Dictionary<string, string> _device_config, string _type, Dictionary<string, string> _sm_config)
        {
          return session.JsonRpcClient.async_sr_probe_ext(session.opaque_ref, _host, _device_config, _type, _sm_config);
        }

        /// <summary>
        /// Sets the shared flag on the SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">True if the SR is shared</param>
        public static void set_shared(Session session, string _sr, bool _value)
        {
            session.JsonRpcClient.sr_set_shared(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Sets the shared flag on the SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">True if the SR is shared</param>
        public static XenRef<Task> async_set_shared(Session session, string _sr, bool _value)
        {
          return session.JsonRpcClient.async_sr_set_shared(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Set the name label of the SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">The name label for the SR</param>
        public static void set_name_label(Session session, string _sr, string _value)
        {
            session.JsonRpcClient.sr_set_name_label(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Set the name label of the SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">The name label for the SR</param>
        public static XenRef<Task> async_set_name_label(Session session, string _sr, string _value)
        {
          return session.JsonRpcClient.async_sr_set_name_label(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Set the name description of the SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">The name description for the SR</param>
        public static void set_name_description(Session session, string _sr, string _value)
        {
            session.JsonRpcClient.sr_set_name_description(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Set the name description of the SR
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">The name description for the SR</param>
        public static XenRef<Task> async_set_name_description(Session session, string _sr, string _value)
        {
          return session.JsonRpcClient.async_sr_set_name_description(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Create a placeholder for a named binary blob of data that is associated with this SR
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_name">The name associated with the blob</param>
        /// <param name="_mime_type">The mime type for the data. Empty string translates to application/octet-stream</param>
        public static XenRef<Blob> create_new_blob(Session session, string _sr, string _name, string _mime_type)
        {
            return session.JsonRpcClient.sr_create_new_blob(session.opaque_ref, _sr, _name, _mime_type);
        }

        /// <summary>
        /// Create a placeholder for a named binary blob of data that is associated with this SR
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_name">The name associated with the blob</param>
        /// <param name="_mime_type">The mime type for the data. Empty string translates to application/octet-stream</param>
        public static XenRef<Task> async_create_new_blob(Session session, string _sr, string _name, string _mime_type)
        {
          return session.JsonRpcClient.async_sr_create_new_blob(session.opaque_ref, _sr, _name, _mime_type);
        }

        /// <summary>
        /// Create a placeholder for a named binary blob of data that is associated with this SR
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_name">The name associated with the blob</param>
        /// <param name="_mime_type">The mime type for the data. Empty string translates to application/octet-stream</param>
        /// <param name="_public">True if the blob should be publicly available First published in XenServer 6.1.</param>
        public static XenRef<Blob> create_new_blob(Session session, string _sr, string _name, string _mime_type, bool _public)
        {
            return session.JsonRpcClient.sr_create_new_blob(session.opaque_ref, _sr, _name, _mime_type, _public);
        }

        /// <summary>
        /// Create a placeholder for a named binary blob of data that is associated with this SR
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_name">The name associated with the blob</param>
        /// <param name="_mime_type">The mime type for the data. Empty string translates to application/octet-stream</param>
        /// <param name="_public">True if the blob should be publicly available First published in XenServer 6.1.</param>
        public static XenRef<Task> async_create_new_blob(Session session, string _sr, string _name, string _mime_type, bool _public)
        {
          return session.JsonRpcClient.async_sr_create_new_blob(session.opaque_ref, _sr, _name, _mime_type, _public);
        }

        /// <summary>
        /// Sets the SR's physical_size field
        /// First published in XenServer 4.1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_value">The new value of the SR's physical_size</param>
        public static void set_physical_size(Session session, string _sr, long _value)
        {
            session.JsonRpcClient.sr_set_physical_size(session.opaque_ref, _sr, _value);
        }

        /// <summary>
        /// Returns successfully if the given SR can host an HA statefile. Otherwise returns an error to explain why not
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void assert_can_host_ha_statefile(Session session, string _sr)
        {
            session.JsonRpcClient.sr_assert_can_host_ha_statefile(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Returns successfully if the given SR can host an HA statefile. Otherwise returns an error to explain why not
        /// First published in XenServer 5.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_assert_can_host_ha_statefile(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_assert_can_host_ha_statefile(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Returns successfully if the given SR supports database replication. Otherwise returns an error to explain why not.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void assert_supports_database_replication(Session session, string _sr)
        {
            session.JsonRpcClient.sr_assert_supports_database_replication(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Returns successfully if the given SR supports database replication. Otherwise returns an error to explain why not.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_assert_supports_database_replication(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_assert_supports_database_replication(session.opaque_ref, _sr);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void enable_database_replication(Session session, string _sr)
        {
            session.JsonRpcClient.sr_enable_database_replication(session.opaque_ref, _sr);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_enable_database_replication(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_enable_database_replication(session.opaque_ref, _sr);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static void disable_database_replication(Session session, string _sr)
        {
            session.JsonRpcClient.sr_disable_database_replication(session.opaque_ref, _sr);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static XenRef<Task> async_disable_database_replication(Session session, string _sr)
        {
          return session.JsonRpcClient.async_sr_disable_database_replication(session.opaque_ref, _sr);
        }

        /// <summary>
        /// 
        /// First published in XenServer 7.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        public static List<Data_source> get_data_sources(Session session, string _sr)
        {
            return session.JsonRpcClient.sr_get_data_sources(session.opaque_ref, _sr);
        }

        /// <summary>
        /// Start recording the specified data source
        /// First published in XenServer 7.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_data_source">The data source to record</param>
        public static void record_data_source(Session session, string _sr, string _data_source)
        {
            session.JsonRpcClient.sr_record_data_source(session.opaque_ref, _sr, _data_source);
        }

        /// <summary>
        /// Query the latest value of the specified data source
        /// First published in XenServer 7.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_data_source">The data source to query</param>
        public static double query_data_source(Session session, string _sr, string _data_source)
        {
            return session.JsonRpcClient.sr_query_data_source(session.opaque_ref, _sr, _data_source);
        }

        /// <summary>
        /// Forget the recorded statistics related to the specified data source
        /// First published in XenServer 7.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_sr">The opaque_ref of the given sr</param>
        /// <param name="_data_source">The data source whose archives are to be forgotten</param>
        public static void forget_data_source_archives(Session session, string _sr, string _data_source)
        {
            session.JsonRpcClient.sr_forget_data_source_archives(session.opaque_ref, _sr, _data_source);
        }

        /// <summary>
        /// Return a list of all the SRs known to the system.
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static List<XenRef<SR>> get_all(Session session)
        {
            return session.JsonRpcClient.sr_get_all(session.opaque_ref);
        }

        /// <summary>
        /// Get all the SR Records at once, in a single XML RPC call
        /// First published in XenServer 4.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static Dictionary<XenRef<SR>, SR> get_all_records(Session session)
        {
            return session.JsonRpcClient.sr_get_all_records(session.opaque_ref);
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
        /// list of the operations allowed in this state. This list is advisory only and the server state may have changed by the time this field is read by a client.
        /// </summary>
        public virtual List<storage_operations> allowed_operations
        {
            get { return _allowed_operations; }
            set
            {
                if (!Helper.AreEqual(value, _allowed_operations))
                {
                    _allowed_operations = value;
                    NotifyPropertyChanged("allowed_operations");
                }
            }
        }
        private List<storage_operations> _allowed_operations = new List<storage_operations>() {};

        /// <summary>
        /// links each of the running tasks using this object (by reference) to a current_operation enum which describes the nature of the task.
        /// </summary>
        public virtual Dictionary<string, storage_operations> current_operations
        {
            get { return _current_operations; }
            set
            {
                if (!Helper.AreEqual(value, _current_operations))
                {
                    _current_operations = value;
                    NotifyPropertyChanged("current_operations");
                }
            }
        }
        private Dictionary<string, storage_operations> _current_operations = new Dictionary<string, storage_operations>() {};

        /// <summary>
        /// all virtual disks known to this storage repository
        /// </summary>
        [JsonConverter(typeof(XenRefListConverter<VDI>))]
        public virtual List<XenRef<VDI>> VDIs
        {
            get { return _VDIs; }
            set
            {
                if (!Helper.AreEqual(value, _VDIs))
                {
                    _VDIs = value;
                    NotifyPropertyChanged("VDIs");
                }
            }
        }
        private List<XenRef<VDI>> _VDIs = new List<XenRef<VDI>>() {};

        /// <summary>
        /// describes how particular hosts can see this storage repository
        /// </summary>
        [JsonConverter(typeof(XenRefListConverter<PBD>))]
        public virtual List<XenRef<PBD>> PBDs
        {
            get { return _PBDs; }
            set
            {
                if (!Helper.AreEqual(value, _PBDs))
                {
                    _PBDs = value;
                    NotifyPropertyChanged("PBDs");
                }
            }
        }
        private List<XenRef<PBD>> _PBDs = new List<XenRef<PBD>>() {};

        /// <summary>
        /// sum of virtual_sizes of all VDIs in this storage repository (in bytes)
        /// </summary>
        public virtual long virtual_allocation
        {
            get { return _virtual_allocation; }
            set
            {
                if (!Helper.AreEqual(value, _virtual_allocation))
                {
                    _virtual_allocation = value;
                    NotifyPropertyChanged("virtual_allocation");
                }
            }
        }
        private long _virtual_allocation;

        /// <summary>
        /// physical space currently utilised on this storage repository (in bytes). Note that for sparse disk formats, physical_utilisation may be less than virtual_allocation
        /// </summary>
        public virtual long physical_utilisation
        {
            get { return _physical_utilisation; }
            set
            {
                if (!Helper.AreEqual(value, _physical_utilisation))
                {
                    _physical_utilisation = value;
                    NotifyPropertyChanged("physical_utilisation");
                }
            }
        }
        private long _physical_utilisation;

        /// <summary>
        /// total physical size of the repository (in bytes)
        /// </summary>
        public virtual long physical_size
        {
            get { return _physical_size; }
            set
            {
                if (!Helper.AreEqual(value, _physical_size))
                {
                    _physical_size = value;
                    NotifyPropertyChanged("physical_size");
                }
            }
        }
        private long _physical_size;

        /// <summary>
        /// type of the storage repository
        /// </summary>
        public virtual string type
        {
            get { return _type; }
            set
            {
                if (!Helper.AreEqual(value, _type))
                {
                    _type = value;
                    NotifyPropertyChanged("type");
                }
            }
        }
        private string _type = "";

        /// <summary>
        /// the type of the SR's content, if required (e.g. ISOs)
        /// </summary>
        public virtual string content_type
        {
            get { return _content_type; }
            set
            {
                if (!Helper.AreEqual(value, _content_type))
                {
                    _content_type = value;
                    NotifyPropertyChanged("content_type");
                }
            }
        }
        private string _content_type = "";

        /// <summary>
        /// true if this SR is (capable of being) shared between multiple hosts
        /// </summary>
        public virtual bool shared
        {
            get { return _shared; }
            set
            {
                if (!Helper.AreEqual(value, _shared))
                {
                    _shared = value;
                    NotifyPropertyChanged("shared");
                }
            }
        }
        private bool _shared;

        /// <summary>
        /// additional configuration
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
        /// user-specified tags for categorization purposes
        /// First published in XenServer 5.0.
        /// </summary>
        public virtual string[] tags
        {
            get { return _tags; }
            set
            {
                if (!Helper.AreEqual(value, _tags))
                {
                    _tags = value;
                    NotifyPropertyChanged("tags");
                }
            }
        }
        private string[] _tags = {};

        /// <summary>
        /// SM dependent data
        /// First published in XenServer 4.1.
        /// </summary>
        [JsonConverter(typeof(StringStringMapConverter))]
        public virtual Dictionary<string, string> sm_config
        {
            get { return _sm_config; }
            set
            {
                if (!Helper.AreEqual(value, _sm_config))
                {
                    _sm_config = value;
                    NotifyPropertyChanged("sm_config");
                }
            }
        }
        private Dictionary<string, string> _sm_config = new Dictionary<string, string>() {};

        /// <summary>
        /// Binary blobs associated with this SR
        /// First published in XenServer 5.0.
        /// </summary>
        [JsonConverter(typeof(StringXenRefMapConverter<Blob>))]
        public virtual Dictionary<string, XenRef<Blob>> blobs
        {
            get { return _blobs; }
            set
            {
                if (!Helper.AreEqual(value, _blobs))
                {
                    _blobs = value;
                    NotifyPropertyChanged("blobs");
                }
            }
        }
        private Dictionary<string, XenRef<Blob>> _blobs = new Dictionary<string, XenRef<Blob>>() {};

        /// <summary>
        /// True if this SR is assigned to be the local cache for its host
        /// First published in XenServer 5.6 FP1.
        /// </summary>
        public virtual bool local_cache_enabled
        {
            get { return _local_cache_enabled; }
            set
            {
                if (!Helper.AreEqual(value, _local_cache_enabled))
                {
                    _local_cache_enabled = value;
                    NotifyPropertyChanged("local_cache_enabled");
                }
            }
        }
        private bool _local_cache_enabled = false;

        /// <summary>
        /// The disaster recovery task which introduced this SR
        /// First published in XenServer 6.0.
        /// </summary>
        [JsonConverter(typeof(XenRefConverter<DR_task>))]
        public virtual XenRef<DR_task> introduced_by
        {
            get { return _introduced_by; }
            set
            {
                if (!Helper.AreEqual(value, _introduced_by))
                {
                    _introduced_by = value;
                    NotifyPropertyChanged("introduced_by");
                }
            }
        }
        private XenRef<DR_task> _introduced_by = new XenRef<DR_task>("OpaqueRef:NULL");

        /// <summary>
        /// True if the SR is using aggregated local storage
        /// First published in XenServer 7.0.
        /// </summary>
        public virtual bool clustered
        {
            get { return _clustered; }
            set
            {
                if (!Helper.AreEqual(value, _clustered))
                {
                    _clustered = value;
                    NotifyPropertyChanged("clustered");
                }
            }
        }
        private bool _clustered = false;

        /// <summary>
        /// True if this is the SR that contains the Tools ISO VDIs
        /// First published in XenServer 7.0.
        /// </summary>
        public virtual bool is_tools_sr
        {
            get { return _is_tools_sr; }
            set
            {
                if (!Helper.AreEqual(value, _is_tools_sr))
                {
                    _is_tools_sr = value;
                    NotifyPropertyChanged("is_tools_sr");
                }
            }
        }
        private bool _is_tools_sr = false;
    }
}
