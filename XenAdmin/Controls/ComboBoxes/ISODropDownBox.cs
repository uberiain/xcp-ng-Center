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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using XenAdmin.Core;
using XenAdmin.Network;
using XenAPI;
using XenCenterLib;

namespace XenAdmin.Controls
{
    public class ISODropDownBox : NonSelectableComboBox
    {
        public event Action SrsRefreshed;

        protected VM vm;
        private bool refreshOnClose;
        protected bool changing = false;
        private IXenConnection _connection;
        private readonly CollectionChangeEventHandler SR_CollectionChangedWithInvoke;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VDI SelectedCD
        {
            get => (SelectedItem as ToStringWrapper<VDI>)?.item;
            set => SelectCD(value);
        }

        public ISODropDownBox()
        {
            SR_CollectionChangedWithInvoke = Program.ProgramInvokeHandler(SR_CollectionChanged);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FormattingEnabled = true;
        }

        protected override void Dispose(bool disposing)
        {
            DeregisterEvents();
            base.Dispose(disposing);
        }

        private void RefreshSRs()
        {
            Program.AssertOnEventThread();

            //Create a special VDIWrapper for the empty dropdown item
            Items.Add(new ToStringWrapper<VDI>(null, Messages.EMPTY));

            if (connection == null)
                return;

            List<ToStringWrapper<SR>> items = new List<ToStringWrapper<SR>>();
            foreach (SR sr in connection.Cache.SRs)
            {
                if (sr.content_type != SR.Content_Type_ISO)
                    continue;

                if (sr.IsToolsSR() && Helpers.StockholmOrGreater(connection))
                    continue;

                if (vm == null && sr.IsBroken())
                    continue;

                if (vm != null)
                {                    
                    if (vm.power_state == vm_power_state.Halted)
                    {
                        Host storageHost = vm.GetStorageHost(true);
                        // The storage host is the host that the VM is bound to because the VM is using local storage on that host.
                        // It will be null if there is no such host (i.e. the VM is not restricted host-wise by storage). 
                        if (storageHost != null && !sr.CanBeSeenFrom(storageHost))
                        {
                            // The storage host was not null, and this SR can't be seen from that host: don't show the SR.
                            continue;
                        }
                    }
                    else
                    {
                        // If VM is running, only show SRs on its current host
                        Host runningOn = vm.Connection.Resolve(vm.resident_on);
                        if (!sr.CanBeSeenFrom(runningOn))
                        {
                            continue;
                        }
                    }
                }

                items.Add(new ToStringWrapper<SR>(sr, sr.Name()));
            }

            if (items.Count > 0)
            {
                items.Sort();
                foreach (ToStringWrapper<SR> srWrapper in items)
                {
                    AddSR(srWrapper);
                }
            }
        }

        protected void SelectCD(VDI selectedCd)
        {
            if (selectedCd != null)
            {
                foreach (var o in Items)
                {
                    var iso = (o as ToStringWrapper<VDI>)?.item;

                    if (iso == null || !iso.Show(Properties.Settings.Default.ShowHiddenVMs))
                        continue;

                    if (iso != selectedCd) continue;

                    SelectedItem = o;
                    return;
                }
            }

            if (Items.Count > 0)
                SelectedIndex = 0;
            else
                SelectedIndex = -1;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VM VM
        {
            set
            {
                if (vm != null)
                    vm.PropertyChanged -= vm_PropertyChanged;

                vm = value;

                if (vm != null)
                    vm.PropertyChanged += vm_PropertyChanged;

                connection = vm?.Connection;

                //do not call RefreshAll() here as it is called within the connection setter
            }
            get => vm;
        }

        private void AddSR(ToStringWrapper<SR> srWrapper)
        {
            Items.Add(srWrapper);

            List<ToStringWrapper<VDI>> items = new List<ToStringWrapper<VDI>>();
            if (srWrapper.item.Physical())
            {
                List<ToStringWrapper<VDI>> vdis = new List<ToStringWrapper<VDI>>();
                foreach (VDI vdi in connection.ResolveAll<VDI>(srWrapper.item.VDIs))
                {
                    ToStringWrapper<VDI> vdiWrapper = new ToStringWrapper<VDI>(vdi, vdi.Name());
                    vdis.Add(vdiWrapper);
                }
                vdis.Sort((object1, object2) =>
                    StringUtility.NaturalCompare(object1.item.Name(), object2.item.Name()));

                Host host = srWrapper.item.GetStorageHost();
                if (host != null)
                {
                    for (int i = 0; i < vdis.Count; i++)
                    {
                        items.Add(new ToStringWrapper<VDI>(vdis[i].item, "    " + string.Format(Messages.ISOCOMBOBOX_CD_DRIVE, i, host.Name())));
                    }
                }
            }
            else
            {
                if (srWrapper.item.IsToolsSR())
                {
                    if (!Helpers.StockholmOrGreater(connection))
                    {
                        foreach (VDI vdi in connection.ResolveAll<VDI>(srWrapper.item.VDIs))
                        {
                            if (vdi.IsToolsIso())
                                items.Add(new ToStringWrapper<VDI>(vdi, "    " + vdi.Name()));
                        }
                    }
                }
                else
                {
                    foreach (VDI vdi in connection.ResolveAll<VDI>(srWrapper.item.VDIs))
                    {
                        items.Add(new ToStringWrapper<VDI>(vdi, "    " + vdi.Name()));
                    }

                    items.Sort((object1, object2) =>
                        StringUtility.NaturalCompare(object1.item.Name(), object2.item.Name()));
                }
            }

            foreach (ToStringWrapper<VDI> vdiWrapper in items)
            {
                Items.Add(vdiWrapper);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IXenConnection connection
        {
            set
            {
                if (connection != null)
                {
                    DeregisterEvents();
                }
                _connection = value;
                if (connection != null)
                {
                    RegisterEvents();
                    RefreshAll();
                }
            }
            get
            {
                if (vm != null)
                {
                    return vm.Connection;
                }
                else
                {
                    return _connection;
                }
            }
        }

        internal virtual void DeregisterEvents()
        {
            if (vm != null)
                vm.PropertyChanged -= vm_PropertyChanged;

            if (connection == null)
                return;

            connection.Cache.DeregisterCollectionChanged<SR>(SR_CollectionChangedWithInvoke);

            foreach (SR sr in connection.Cache.SRs)
            {
                sr.PropertyChanged -= sr_PropertyChanged;
                foreach (PBD pbd in connection.Cache.PBDs)
                {
                    pbd.PropertyChanged -= pbd_PropertyChanged;
                }
            }
        }

        private void RegisterEvents()
        {
            if (connection == null)
                return;
            
            // register collection listener
            connection.Cache.RegisterCollectionChanged<SR>(SR_CollectionChangedWithInvoke);

            // Add SR listeners
            foreach (SR sr in connection.Cache.SRs)
            {
                sr.PropertyChanged -= sr_PropertyChanged;
                sr.PropertyChanged += sr_PropertyChanged;
                foreach (PBD pbd in connection.Cache.PBDs)
                {
                    pbd.PropertyChanged -= pbd_PropertyChanged;
                    pbd.PropertyChanged += pbd_PropertyChanged;
                }
            }
        }

        private void SR_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            Program.AssertOnEventThread();

            if (vm == null || e.Action == CollectionChangeAction.Refresh)
                return; 

            foreach (SR sr in connection.Cache.SRs)
            {
                sr.PropertyChanged -= sr_PropertyChanged;
                sr.PropertyChanged += sr_PropertyChanged;
            }

            RefreshAll();
        }

        private void RefreshAll()
        {
            if (!DroppedDown)
            {
                BeginUpdate();

                try
                {
                    Items.Clear();
                    RefreshSRs();
                }
                finally
                {
                    EndUpdate();
                }

                SelectCD(SelectedCD);
                refreshOnClose = false;
                SrsRefreshed?.Invoke();
            }
            else
            {
                refreshOnClose = true;
            }
        }

        private void sr_PropertyChanged(object sender1, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "VDIs" || e.PropertyName == "PBDs")
            {
                RefreshAll();
            }
        }

        private void pbd_PropertyChanged(object sender1, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "currently_attached")
            {
                RefreshAll();
            }
        }

        private void vm_PropertyChanged(object sender1, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "VBDs" || e.PropertyName == "resident_on" || e.PropertyName == "affinity")
            {
                RefreshAll();
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                Object o = Items[e.Index];

                e.DrawBackground();

                if (o is ToStringWrapper<SR>)
                {
                    Drawing.DrawText(e.Graphics, o.ToString(), Program.DefaultFontBold, e.Bounds, SystemColors.ControlText, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
                else
                {
                    Color colour = e.ForeColor;

                    if ((e.State & DrawItemState.Disabled) != 0)
                        colour = SystemColors.GrayText;

                    Drawing.DrawText(e.Graphics, o.ToString(), Program.DefaultFont, e.Bounds, colour, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                    e.DrawFocusRectangle();
                }
            }

            base.OnDrawItem(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            if (refreshOnClose)
                RefreshAll();
        }

        protected override bool IsItemNonSelectable(object o)
        {
            return o is ToStringWrapper<SR>;
        }
    }
}
