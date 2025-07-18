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
    /// A physical GPU (pGPU)
    /// First published in XenServer 6.0.
    /// </summary>
    public partial class PGPU : XenObject<PGPU>
    {
        #region Constructors

        public PGPU()
        {
        }

        public PGPU(string uuid,
            XenRef<PCI> PCI,
            XenRef<GPU_group> GPU_group,
            XenRef<Host> host,
            Dictionary<string, string> other_config,
            List<XenRef<VGPU_type>> supported_VGPU_types,
            List<XenRef<VGPU_type>> enabled_VGPU_types,
            List<XenRef<VGPU>> resident_VGPUs,
            Dictionary<XenRef<VGPU_type>, long> supported_VGPU_max_capacities,
            pgpu_dom0_access dom0_access,
            bool is_system_display_device,
            Dictionary<string, string> compatibility_metadata)
        {
            this.uuid = uuid;
            this.PCI = PCI;
            this.GPU_group = GPU_group;
            this.host = host;
            this.other_config = other_config;
            this.supported_VGPU_types = supported_VGPU_types;
            this.enabled_VGPU_types = enabled_VGPU_types;
            this.resident_VGPUs = resident_VGPUs;
            this.supported_VGPU_max_capacities = supported_VGPU_max_capacities;
            this.dom0_access = dom0_access;
            this.is_system_display_device = is_system_display_device;
            this.compatibility_metadata = compatibility_metadata;
        }

        /// <summary>
        /// Creates a new PGPU from a Hashtable.
        /// Note that the fields not contained in the Hashtable
        /// will be created with their default values.
        /// </summary>
        /// <param name="table"></param>
        public PGPU(Hashtable table)
            : this()
        {
            UpdateFrom(table);
        }

        #endregion

        /// <summary>
        /// Updates each field of this instance with the value of
        /// the corresponding field of a given PGPU.
        /// </summary>
        public override void UpdateFrom(PGPU record)
        {
            uuid = record.uuid;
            PCI = record.PCI;
            GPU_group = record.GPU_group;
            host = record.host;
            other_config = record.other_config;
            supported_VGPU_types = record.supported_VGPU_types;
            enabled_VGPU_types = record.enabled_VGPU_types;
            resident_VGPUs = record.resident_VGPUs;
            supported_VGPU_max_capacities = record.supported_VGPU_max_capacities;
            dom0_access = record.dom0_access;
            is_system_display_device = record.is_system_display_device;
            compatibility_metadata = record.compatibility_metadata;
        }

        /// <summary>
        /// Given a Hashtable with field-value pairs, it updates the fields of this PGPU
        /// with the values listed in the Hashtable. Note that only the fields contained
        /// in the Hashtable will be updated and the rest will remain the same.
        /// </summary>
        /// <param name="table"></param>
        public void UpdateFrom(Hashtable table)
        {
            if (table.ContainsKey("uuid"))
                uuid = Marshalling.ParseString(table, "uuid");
            if (table.ContainsKey("PCI"))
                PCI = Marshalling.ParseRef<PCI>(table, "PCI");
            if (table.ContainsKey("GPU_group"))
                GPU_group = Marshalling.ParseRef<GPU_group>(table, "GPU_group");
            if (table.ContainsKey("host"))
                host = Marshalling.ParseRef<Host>(table, "host");
            if (table.ContainsKey("other_config"))
                other_config = Maps.ToDictionary_string_string(Marshalling.ParseHashTable(table, "other_config"));
            if (table.ContainsKey("supported_VGPU_types"))
                supported_VGPU_types = Marshalling.ParseSetRef<VGPU_type>(table, "supported_VGPU_types");
            if (table.ContainsKey("enabled_VGPU_types"))
                enabled_VGPU_types = Marshalling.ParseSetRef<VGPU_type>(table, "enabled_VGPU_types");
            if (table.ContainsKey("resident_VGPUs"))
                resident_VGPUs = Marshalling.ParseSetRef<VGPU>(table, "resident_VGPUs");
            if (table.ContainsKey("supported_VGPU_max_capacities"))
                supported_VGPU_max_capacities = Maps.ToDictionary_XenRefVGPU_type_long(Marshalling.ParseHashTable(table, "supported_VGPU_max_capacities"));
            if (table.ContainsKey("dom0_access"))
                dom0_access = (pgpu_dom0_access)Helper.EnumParseDefault(typeof(pgpu_dom0_access), Marshalling.ParseString(table, "dom0_access"));
            if (table.ContainsKey("is_system_display_device"))
                is_system_display_device = Marshalling.ParseBool(table, "is_system_display_device");
            if (table.ContainsKey("compatibility_metadata"))
                compatibility_metadata = Maps.ToDictionary_string_string(Marshalling.ParseHashTable(table, "compatibility_metadata"));
        }

        public bool DeepEquals(PGPU other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Helper.AreEqual2(_uuid, other._uuid) &&
                Helper.AreEqual2(_PCI, other._PCI) &&
                Helper.AreEqual2(_GPU_group, other._GPU_group) &&
                Helper.AreEqual2(_host, other._host) &&
                Helper.AreEqual2(_other_config, other._other_config) &&
                Helper.AreEqual2(_supported_VGPU_types, other._supported_VGPU_types) &&
                Helper.AreEqual2(_enabled_VGPU_types, other._enabled_VGPU_types) &&
                Helper.AreEqual2(_resident_VGPUs, other._resident_VGPUs) &&
                Helper.AreEqual2(_supported_VGPU_max_capacities, other._supported_VGPU_max_capacities) &&
                Helper.AreEqual2(_dom0_access, other._dom0_access) &&
                Helper.AreEqual2(_is_system_display_device, other._is_system_display_device) &&
                Helper.AreEqual2(_compatibility_metadata, other._compatibility_metadata);
        }

        public override string SaveChanges(Session session, string opaqueRef, PGPU server)
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
                    PGPU.set_other_config(session, opaqueRef, _other_config);
                }
                if (!Helper.AreEqual2(_GPU_group, server._GPU_group))
                {
                    PGPU.set_GPU_group(session, opaqueRef, _GPU_group);
                }

                return null;
            }
        }

        /// <summary>
        /// Get a record containing the current state of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static PGPU get_record(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_record(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get a reference to the PGPU instance with the specified UUID.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">UUID of object to return</param>
        public static XenRef<PGPU> get_by_uuid(Session session, string _uuid)
        {
            return session.JsonRpcClient.pgpu_get_by_uuid(session.opaque_ref, _uuid);
        }

        /// <summary>
        /// Get the uuid field of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static string get_uuid(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_uuid(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the PCI field of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static XenRef<PCI> get_PCI(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_pci(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the GPU_group field of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static XenRef<GPU_group> get_GPU_group(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_gpu_group(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the host field of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static XenRef<Host> get_host(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_host(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the other_config field of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static Dictionary<string, string> get_other_config(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_other_config(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the supported_VGPU_types field of the given PGPU.
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static List<XenRef<VGPU_type>> get_supported_VGPU_types(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_supported_vgpu_types(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the enabled_VGPU_types field of the given PGPU.
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static List<XenRef<VGPU_type>> get_enabled_VGPU_types(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_enabled_vgpu_types(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the resident_VGPUs field of the given PGPU.
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static List<XenRef<VGPU>> get_resident_VGPUs(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_resident_vgpus(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the supported_VGPU_max_capacities field of the given PGPU.
        /// First published in XenServer 6.2 SP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static Dictionary<XenRef<VGPU_type>, long> get_supported_VGPU_max_capacities(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_supported_vgpu_max_capacities(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the dom0_access field of the given PGPU.
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static pgpu_dom0_access get_dom0_access(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_dom0_access(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the is_system_display_device field of the given PGPU.
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static bool get_is_system_display_device(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_is_system_display_device(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Get the compatibility_metadata field of the given PGPU.
        /// First published in XenServer 7.3.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static Dictionary<string, string> get_compatibility_metadata(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_get_compatibility_metadata(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Set the other_config field of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_other_config">New value to set</param>
        public static void set_other_config(Session session, string _pgpu, Dictionary<string, string> _other_config)
        {
            session.JsonRpcClient.pgpu_set_other_config(session.opaque_ref, _pgpu, _other_config);
        }

        /// <summary>
        /// Add the given key-value pair to the other_config field of the given PGPU.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_key">Key to add</param>
        /// <param name="_value">Value to add</param>
        public static void add_to_other_config(Session session, string _pgpu, string _key, string _value)
        {
            session.JsonRpcClient.pgpu_add_to_other_config(session.opaque_ref, _pgpu, _key, _value);
        }

        /// <summary>
        /// Remove the given key and its corresponding value from the other_config field of the given PGPU.  If the key is not in that Map, then do nothing.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_key">Key to remove</param>
        public static void remove_from_other_config(Session session, string _pgpu, string _key)
        {
            session.JsonRpcClient.pgpu_remove_from_other_config(session.opaque_ref, _pgpu, _key);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The VGPU type to enable</param>
        public static void add_enabled_VGPU_types(Session session, string _pgpu, string _value)
        {
            session.JsonRpcClient.pgpu_add_enabled_vgpu_types(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The VGPU type to enable</param>
        public static XenRef<Task> async_add_enabled_VGPU_types(Session session, string _pgpu, string _value)
        {
          return session.JsonRpcClient.async_pgpu_add_enabled_vgpu_types(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The VGPU type to disable</param>
        public static void remove_enabled_VGPU_types(Session session, string _pgpu, string _value)
        {
            session.JsonRpcClient.pgpu_remove_enabled_vgpu_types(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The VGPU type to disable</param>
        public static XenRef<Task> async_remove_enabled_VGPU_types(Session session, string _pgpu, string _value)
        {
          return session.JsonRpcClient.async_pgpu_remove_enabled_vgpu_types(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The VGPU types to enable</param>
        public static void set_enabled_VGPU_types(Session session, string _pgpu, List<XenRef<VGPU_type>> _value)
        {
            session.JsonRpcClient.pgpu_set_enabled_vgpu_types(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The VGPU types to enable</param>
        public static XenRef<Task> async_set_enabled_VGPU_types(Session session, string _pgpu, List<XenRef<VGPU_type>> _value)
        {
          return session.JsonRpcClient.async_pgpu_set_enabled_vgpu_types(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The group to which the PGPU will be moved</param>
        public static void set_GPU_group(Session session, string _pgpu, string _value)
        {
            session.JsonRpcClient.pgpu_set_gpu_group(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_value">The group to which the PGPU will be moved</param>
        public static XenRef<Task> async_set_GPU_group(Session session, string _pgpu, string _value)
        {
          return session.JsonRpcClient.async_pgpu_set_gpu_group(session.opaque_ref, _pgpu, _value);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_vgpu_type">The VGPU type for which we want to find the number of VGPUs which can still be started on this PGPU</param>
        public static long get_remaining_capacity(Session session, string _pgpu, string _vgpu_type)
        {
            return session.JsonRpcClient.pgpu_get_remaining_capacity(session.opaque_ref, _pgpu, _vgpu_type);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        /// <param name="_vgpu_type">The VGPU type for which we want to find the number of VGPUs which can still be started on this PGPU</param>
        public static XenRef<Task> async_get_remaining_capacity(Session session, string _pgpu, string _vgpu_type)
        {
          return session.JsonRpcClient.async_pgpu_get_remaining_capacity(session.opaque_ref, _pgpu, _vgpu_type);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static pgpu_dom0_access enable_dom0_access(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_enable_dom0_access(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static XenRef<Task> async_enable_dom0_access(Session session, string _pgpu)
        {
          return session.JsonRpcClient.async_pgpu_enable_dom0_access(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static pgpu_dom0_access disable_dom0_access(Session session, string _pgpu)
        {
            return session.JsonRpcClient.pgpu_disable_dom0_access(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// 
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_pgpu">The opaque_ref of the given pgpu</param>
        public static XenRef<Task> async_disable_dom0_access(Session session, string _pgpu)
        {
          return session.JsonRpcClient.async_pgpu_disable_dom0_access(session.opaque_ref, _pgpu);
        }

        /// <summary>
        /// Return a list of all the PGPUs known to the system.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static List<XenRef<PGPU>> get_all(Session session)
        {
            return session.JsonRpcClient.pgpu_get_all(session.opaque_ref);
        }

        /// <summary>
        /// Get all the PGPU Records at once, in a single XML RPC call
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static Dictionary<XenRef<PGPU>, PGPU> get_all_records(Session session)
        {
            return session.JsonRpcClient.pgpu_get_all_records(session.opaque_ref);
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
        /// Link to underlying PCI device
        /// </summary>
        [JsonConverter(typeof(XenRefConverter<PCI>))]
        public virtual XenRef<PCI> PCI
        {
            get { return _PCI; }
            set
            {
                if (!Helper.AreEqual(value, _PCI))
                {
                    _PCI = value;
                    NotifyPropertyChanged("PCI");
                }
            }
        }
        private XenRef<PCI> _PCI = new XenRef<PCI>("OpaqueRef:NULL");

        /// <summary>
        /// GPU group the pGPU is contained in
        /// </summary>
        [JsonConverter(typeof(XenRefConverter<GPU_group>))]
        public virtual XenRef<GPU_group> GPU_group
        {
            get { return _GPU_group; }
            set
            {
                if (!Helper.AreEqual(value, _GPU_group))
                {
                    _GPU_group = value;
                    NotifyPropertyChanged("GPU_group");
                }
            }
        }
        private XenRef<GPU_group> _GPU_group = new XenRef<GPU_group>("OpaqueRef:NULL");

        /// <summary>
        /// Host that owns the GPU
        /// </summary>
        [JsonConverter(typeof(XenRefConverter<Host>))]
        public virtual XenRef<Host> host
        {
            get { return _host; }
            set
            {
                if (!Helper.AreEqual(value, _host))
                {
                    _host = value;
                    NotifyPropertyChanged("host");
                }
            }
        }
        private XenRef<Host> _host = new XenRef<Host>("OpaqueRef:NULL");

        /// <summary>
        /// Additional configuration
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
        /// List of VGPU types supported by the underlying hardware
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        [JsonConverter(typeof(XenRefListConverter<VGPU_type>))]
        public virtual List<XenRef<VGPU_type>> supported_VGPU_types
        {
            get { return _supported_VGPU_types; }
            set
            {
                if (!Helper.AreEqual(value, _supported_VGPU_types))
                {
                    _supported_VGPU_types = value;
                    NotifyPropertyChanged("supported_VGPU_types");
                }
            }
        }
        private List<XenRef<VGPU_type>> _supported_VGPU_types = new List<XenRef<VGPU_type>>() {};

        /// <summary>
        /// List of VGPU types which have been enabled for this PGPU
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        [JsonConverter(typeof(XenRefListConverter<VGPU_type>))]
        public virtual List<XenRef<VGPU_type>> enabled_VGPU_types
        {
            get { return _enabled_VGPU_types; }
            set
            {
                if (!Helper.AreEqual(value, _enabled_VGPU_types))
                {
                    _enabled_VGPU_types = value;
                    NotifyPropertyChanged("enabled_VGPU_types");
                }
            }
        }
        private List<XenRef<VGPU_type>> _enabled_VGPU_types = new List<XenRef<VGPU_type>>() {};

        /// <summary>
        /// List of VGPUs running on this PGPU
        /// First published in XenServer 6.2 SP1 Tech-Preview.
        /// </summary>
        [JsonConverter(typeof(XenRefListConverter<VGPU>))]
        public virtual List<XenRef<VGPU>> resident_VGPUs
        {
            get { return _resident_VGPUs; }
            set
            {
                if (!Helper.AreEqual(value, _resident_VGPUs))
                {
                    _resident_VGPUs = value;
                    NotifyPropertyChanged("resident_VGPUs");
                }
            }
        }
        private List<XenRef<VGPU>> _resident_VGPUs = new List<XenRef<VGPU>>() {};

        /// <summary>
        /// A map relating each VGPU type supported on this GPU to the maximum number of VGPUs of that type which can run simultaneously on this GPU
        /// First published in XenServer 6.2 SP1.
        /// </summary>
        [JsonConverter(typeof(XenRefLongMapConverter<VGPU_type>))]
        public virtual Dictionary<XenRef<VGPU_type>, long> supported_VGPU_max_capacities
        {
            get { return _supported_VGPU_max_capacities; }
            set
            {
                if (!Helper.AreEqual(value, _supported_VGPU_max_capacities))
                {
                    _supported_VGPU_max_capacities = value;
                    NotifyPropertyChanged("supported_VGPU_max_capacities");
                }
            }
        }
        private Dictionary<XenRef<VGPU_type>, long> _supported_VGPU_max_capacities = new Dictionary<XenRef<VGPU_type>, long>() {};

        /// <summary>
        /// The accessibility of this device from dom0
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        [JsonConverter(typeof(pgpu_dom0_accessConverter))]
        public virtual pgpu_dom0_access dom0_access
        {
            get { return _dom0_access; }
            set
            {
                if (!Helper.AreEqual(value, _dom0_access))
                {
                    _dom0_access = value;
                    NotifyPropertyChanged("dom0_access");
                }
            }
        }
        private pgpu_dom0_access _dom0_access = pgpu_dom0_access.enabled;

        /// <summary>
        /// Is this device the system display device
        /// First published in XenServer 6.5 SP1.
        /// </summary>
        public virtual bool is_system_display_device
        {
            get { return _is_system_display_device; }
            set
            {
                if (!Helper.AreEqual(value, _is_system_display_device))
                {
                    _is_system_display_device = value;
                    NotifyPropertyChanged("is_system_display_device");
                }
            }
        }
        private bool _is_system_display_device = false;

        /// <summary>
        /// PGPU metadata to determine whether a VGPU can migrate between two PGPUs
        /// First published in XenServer 7.3.
        /// </summary>
        [JsonConverter(typeof(StringStringMapConverter))]
        public virtual Dictionary<string, string> compatibility_metadata
        {
            get { return _compatibility_metadata; }
            set
            {
                if (!Helper.AreEqual(value, _compatibility_metadata))
                {
                    _compatibility_metadata = value;
                    NotifyPropertyChanged("compatibility_metadata");
                }
            }
        }
        private Dictionary<string, string> _compatibility_metadata = new Dictionary<string, string>() {};
    }
}
