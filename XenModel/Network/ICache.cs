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
using XenAPI;
using System.ComponentModel;
using XenAdmin.Model;
using XenAdmin.Core;
namespace XenAdmin.Network
{
    public interface ICache
    {
        Bond[] Bonds { get; }
        Certificate[] Certificates { get; }
        Cluster[] Clusters { get; }
        Cluster_host[] Cluster_hosts { get; }
        DockerContainer[] DockerContainers { get; }
        Feature[] Features { get; }
        Folder[] Folders { get; }
        GPU_group[] GPU_groups { get; }
        Host[] Hosts { get; }
        Host_cpu[] Host_cpus { get; }
        Message[] Messages { get; }
        XenAPI.Network[] Networks { get; }
        PBD[] PBDs { get; }
        PCI[] PCIs { get; }
        PGPU[] PGPUs { get; }
        PIF[] PIFs { get; }
        Pool[] Pools { get; }
        Pool_patch[] Pool_patches { get; }
        Pool_update[] Pool_updates { get; }
        PVS_cache_storage[] PVS_cache_storages { get; }
        PVS_proxy[] PVS_proxies { get; }
        PVS_server[] PVS_servers { get; }
        PVS_site[] PVS_sites { get; }
        Repository[] Repositories { get; }
        Role[] Roles { get; }
        SM[] SMs { get; }
        SR[] SRs { get; }
        Subject[] Subjects { get; }
        Tunnel[] Tunnels { get; }
        VBD[] VBDs { get; }
        VDI[] VDIs { get; }
        VGPU[] VGPUs { get; }
        VGPU_type[] VGPU_types { get; }
        VIF[] VIFs { get; }
        VM[] VMs { get; }
        VM_appliance[] VM_appliances { get; }
        VMSS[] VMSSs { get; }

        int HostCount { get; }
        IEnumerable<IXenObject> XenSearchableObjects { get; }
        void AddAll<T>(List<T> l, Predicate<T> p) where T : XenObject<T>;
        T Find_By_Uuid<T>(string uuid) where T : XenObject<T>;
        XenRef<T> FindRef<T>(T needle) where T : XenObject<T>;
        T Resolve<T>(XenRef<T> xenRef) where T : XenObject<T>;
        bool TryResolve<T>(XenRef<T> xenRef, out T result) where T : XenObject<T>;
        void Clear();
        bool UpdateFrom(IXenConnection connection, IList<ObjectChange> changes);
        void UpdateDockerContainersForVM(IList<DockerContainer> d, VM v);
        void AddFolder(XenRef<Folder> path, Folder folder);
        void RemoveFolder(XenRef<Folder> path);
        void CheckFoldersBatchChange();
        void RegisterBatchCollectionChanged<T>(EventHandler h) where T : XenObject<T>;
        void DeregisterBatchCollectionChanged<T>(EventHandler h) where T : XenObject<T>;
        void RegisterCollectionChanged<T>(CollectionChangeEventHandler h) where T : XenObject<T>;
        void DeregisterCollectionChanged<T>(CollectionChangeEventHandler h) where T : XenObject<T>;
        void CheckDockerContainersBatchChange();
    }
}
