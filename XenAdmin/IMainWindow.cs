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
using System.Windows.Forms;
using XenAdmin.Network;
using XenAPI;
using System.Collections.ObjectModel;


namespace XenAdmin
{
    /// <summary>
    /// An interface by which the Commands communicate with MainWindow. This abstraction improves maintainability
    /// and testability of the Commands.
    /// </summary>
    public interface IMainWindow
    {
        void Invoke(MethodInvoker method);
        bool SelectObjectInTree(IXenObject xenObject);
        void TrySelectNewObjectInTree(Predicate<object> tagMatch, bool selectNode, bool expandNode, bool ensureNodeVisible);
        void TrySelectNewObjectInTree(IXenConnection c, bool selectNode, bool expandNode, bool ensureNodeVisible);
        void RequestRefreshTreeView();
        void ShowPerConnectionWizard(IXenConnection connection, Form wizard, Form parentForm = null);
        Form ShowForm(Type type);
        Form ShowForm(Type type, object[] args);
        void CloseActiveWizards(IXenConnection connection);
        Collection<IXenConnection> GetXenConnectionsCopy();
        void SaveServerList();
        bool DoSearch(string filename);
        bool RunInAutomatedTestMode { get;}
        void RemoveConnection(IXenConnection connection);
        void PutSelectedNodeIntoEditMode();
        void SwitchToTab(MainWindow.Tab tab);
        bool MenuShortcutsEnabled { get; set; }
        Form Form { get; }
    }
}
