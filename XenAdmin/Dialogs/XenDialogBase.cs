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
using System.Linq;
using System.Windows.Forms;
using XenAdmin.Network;
using XenAdmin.Core;
using XenAPI;

namespace XenAdmin.Dialogs
{
    public partial class XenDialogBase : Form
    {
        private static readonly Dictionary<IXenConnection, List<XenDialogBase>> instances = new Dictionary<IXenConnection, List<XenDialogBase>>();
        private static readonly Dictionary<IXenObject, XenDialogBase> instancePerXenObject = new Dictionary<IXenObject, XenDialogBase>();

        protected readonly IXenConnection connection;
        private IXenObject ownerXenObject;

        private static void AddInstance(IXenConnection connection, XenDialogBase dialog)
        {
            lock(instances)
            {
                if (!instances.ContainsKey(connection))
                    instances.Add(connection, new List<XenDialogBase>());

                instances[connection].Add(dialog);
            }
        }

        public static void CloseAll(IXenConnection connection)
        {
            Program.AssertOnEventThread();

            lock (instances)
            {
                if (!instances.ContainsKey(connection))
                    return;

                List<XenDialogBase> dialogs = new List<XenDialogBase>(instances[connection]);

                foreach (XenDialogBase dialog in dialogs)
                {
                    if (dialog is AddServerDialog || dialog is ConnectingToServerDialog)
                    {
                        // These dialogs are managed by the connection itself
                        continue;
                    }
                    dialog.Close();
                }

                instances.Remove(connection);
            }
        }

        public static void CloseAll(params IXenObject[] xenObjects)
        {
            Program.AssertOnEventThread();

            if (xenObjects == null)
                return;

            foreach (var kvp in instancePerXenObject.ToDictionary(p => p.Key, p => p.Value))
            {
                if (!xenObjects.Contains(kvp.Key))
                    continue;
                
                if (kvp.Value is Form form && !form.IsDisposed)
                    form.Close();

                instancePerXenObject.Remove(kvp.Key);
            }
        }

        /// <summary>
        /// The VS designer does not seem to understand optional parameters,
        /// it needs the parameterless constructor
        /// </summary>
        protected XenDialogBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// If the connection is set, the dialog becomes a per-connection dialog,
        /// which means it will close when the connection is disconnected
        /// </summary>
        protected XenDialogBase(IXenConnection conn)
            : this()
        {
            connection = conn;

            if (connection != null)
                AddInstance(connection, this);
        }

        /// <summary>
        /// override if the reference in the help differs to the dialogs name
        /// </summary>
        internal virtual string HelpName => Name;

        /// <summary>
        /// Allow the XenDialogBase.OnClosed to set Owner.Activate() - this will push the Owner
        /// to the top of the windows stack stealing focus.
        /// </summary>
        protected bool OwnerActivatedOnClosed { get; set; } = true;

        public void ShowPerXenObject(IXenObject obj, Form ownerForm)
        {
            CloseAll(obj);
            ownerXenObject = obj;
            instancePerXenObject.Add(obj, this);
            Show(ownerForm);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (ownerXenObject != null)
            {
                foreach (var kvp in instancePerXenObject.ToDictionary(p => p.Key, p => p.Value))
                {
                    if (kvp.Key.Equals(ownerXenObject))
                        instancePerXenObject.Remove(kvp.Key);
                }
            }

            lock (instances)
            {
                if (connection != null && instances.ContainsKey(connection))
                {
                    instances[connection].Remove(this);
                }
            }

            if (OwnerActivatedOnClosed && Owner != null)
                Owner.Activate();
        }

        #region Event handlers


        private void XenDialogBase_Shown(object sender, EventArgs e)
        {
            if (Modal && Owner != null && Owner.WindowState == FormWindowState.Minimized)
            {
                Owner.WindowState = FormWindowState.Normal;
                CenterToParent();
            }
        }

        private void XenDialogBase_Load(object sender, EventArgs e)
        {
            CenterToParent();
            FormFontFixer.Fix(this);
        }

        #endregion
    }
}
