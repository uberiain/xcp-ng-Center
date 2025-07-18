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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using XenAdmin.Commands;
using XenAdmin.Controls.ConsoleTab;
using XenAdmin.Controls.GradientPanel;
using XenAdmin.Core;
using XenAdmin.Dialogs;
using XenAdmin.Network;
using XenAPI;


namespace XenAdmin.ConsoleView
{
    public partial class VNCTabView : UserControl
    {
        private static readonly string UseRDP = Messages.VNC_RDESKTOP;
        private static readonly string enableRDP = Messages.VNC_RDESKTOP_TURN_ON;
        // public only for the automated tests.
        public static readonly string UseVNC = Messages.VNC_VIRTUAL_CONSOLE;
        public static readonly string UseXVNC = Messages.VNC_X_CONSOLE;
        private static readonly string UseStandardDesktop = Messages.VNC_DEFAULT_DESKTOP;

        public const int INS_KEY_TIMEOUT = 500;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly XSVNCScreen vncScreen;
        private readonly VNCView parentVNCView;
        private readonly VM source;
        private readonly Host targetHost;
        private VM_guest_metrics guestMetrics = null;
        private FullScreenForm fullscreenForm;
        private FullScreenHint fullscreenHint;
        private Size LastDesktopSize;
        private bool switchOnTabOpened = false;
        private Font titleLabelFont = new Font(DefaultFont.FontFamily, DefaultFont.Size + 1f, FontStyle.Bold);

        /// <summary>
        /// Whether to ignore VNC resize events.  We turn this on when changing
        /// the scaling settings, because this in turn triggers a VNC resize.
        /// </summary>
        private bool ignoringResizes;

        private bool ignoreScaleChange;

        internal readonly ConsoleKeyHandler KeyHandler = new ConsoleKeyHandler();

        private bool HasRDP => source != null && source.HasRDP();

        private bool RDPEnabled => source != null && source.RDPEnabled();

        private bool RDPControlEnabled => source != null && source.RDPControlEnabled();

        public bool IsRDPControlEnabled() { return RDPControlEnabled; }

        public VNCTabView(VNCView parent, VM source, string elevatedUsername, string elevatedPassword)
        {
            Program.AssertOnEventThread();

            InitializeComponent();

            var tooltipForGeneralInformationMessage = new ToolTip();
            tooltipForGeneralInformationMessage.SetToolTip(labelGeneralInformationMessage, labelGeneralInformationMessage.Text);

            HostLabel.Font = titleLabelFont;
            HostLabel.ForeColor = HorizontalGradientPanel.TextColor;
            multipleDvdIsoList1.LabelSingleDvdForeColor = HorizontalGradientPanel.TextColor;
            multipleDvdIsoList1.LabelNewCdForeColor = HorizontalGradientPanel.TextColor;
            multipleDvdIsoList1.LinkLabelLinkColor = HorizontalGradientPanel.TextColor;

#pragma warning disable 0219
            // Force the handle to be created, because resize events
            // could be fired before this component is placed on-screen.
            IntPtr _ = Handle;
#pragma warning restore 0219

            parentVNCView = parent;
            scaleCheckBox.Checked = false;
            this.source = source;
            guestMetrics = source.Connection.Resolve(source.guest_metrics);
            if (guestMetrics != null)
                guestMetrics.PropertyChanged += guestMetrics_PropertyChanged;
            log.DebugFormat("'{0}' console: Register Server_PropertyChanged event listener on {0}", this.source.Name());
            this.source.PropertyChanged += Server_PropertyChanged;
            Host_CollectionChangedWithInvoke = Program.ProgramInvokeHandler(Host_CollectionChanged);
            VM_CollectionChangedWithInvoke = Program.ProgramInvokeHandler(VM_CollectionChanged);
            source.Connection.Cache.RegisterCollectionChanged<VM>(VM_CollectionChangedWithInvoke);

            if (source.IsControlDomainZero(out Host host))
            {
                log.DebugFormat("'{0}' console: Register Server_PropertyChanged event listener on {1}", this.source.Name(), host.Name());
                host.PropertyChanged += Server_PropertyChanged;

                Host_metrics hostMetrics = source.Connection.Resolve(host.metrics);
                if (hostMetrics != null)
                {
                    log.DebugFormat("'{0}' console: Register Server_PropertyChanged event listener on host metrics", this.source.Name());
                    hostMetrics.PropertyChanged += Server_PropertyChanged;
                }

                HostLabel.Text = string.Format(Messages.CONSOLE_HOST, host.Name());
                HostLabel.Visible = true;
            }
            else if (source.IsSrDriverDomain(out SR sr))
            {
                log.DebugFormat("'{0}' console: Register Server_PropertyChanged event listener on {1}", this.source.Name(), sr.Name());
                sr.PropertyChanged += Server_PropertyChanged;

                HostLabel.Text = string.Format(Messages.CONSOLE_SR_DRIVER_DOMAIN, sr.Name());
                HostLabel.Visible = true;
            }
            else
            {
                source.Connection.Cache.RegisterCollectionChanged<Host>(Host_CollectionChangedWithInvoke);
                targetHost = source.GetStorageHost(false);

                foreach (Host cachedHost in source.Connection.Cache.Hosts)
                {
                    log.DebugFormat("'{0}' console: Register Server_EnabledPropertyChanged event listener on {1}",
                                    source.Name(), cachedHost.Name());
                    cachedHost.PropertyChanged += Server_EnabledPropertyChanged;
                }

                HostLabel.Visible = false;
            }

            log.DebugFormat("'{0}' console: Update power state (on VNCTabView constructor)", this.source.Name());
            updatePowerState();
            vncScreen = new XSVNCScreen(source, new EventHandler(RDPorVNCResizeHandler), this, elevatedUsername, elevatedPassword);
            ShowGpuWarningIfRequired(vncScreen.MustConnectRemoteDesktop());
            vncScreen.GpuStatusChanged += ShowGpuWarningIfRequired;

            if (source.IsControlDomainZero(out var _) || source.IsHVM() && !HasRDP) //Linux HVM guests should only have one console: the console switch button vanishes altogether.
            {
                toggleConsoleButton.Visible = false;
            }
            else
            {
                toggleConsoleButton.Visible = true;
                vncScreen.UserCancelledAuth += OnUserCancelledAuth;
                vncScreen.VncConnectionAttemptCancelled += OnVncConnectionAttemptCancelled;
            }

            vncScreen.OnDetectRDP = OnDetectRDP;
            vncScreen.OnDetectVNC = OnDetectVNC;

            LastDesktopSize = vncScreen.DesktopSize;

            insKeyTimer = new System.Threading.Timer(new TimerCallback(notInsKeyPressed));

            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

            registerShortCutKeys();

            //
            // Ctlr - Alt - Ins send Ctrl - Alt - Delete, and cancels and pending full screen.
            //

            KeyHandler.AddKeyHandler(ConsoleShortcutKey.CTRL_ALT_INS, cancelWaitForInsKeyAndSendCAD);

            vncScreen.Parent = contentPanel;
            vncScreen.Dock = DockStyle.Fill;

            string rdpLabel = GuessNativeConsoleLabel(source);
            toggleConsoleButton.Text = rdpLabel;

            UpdateFullScreenButton();

            UpdateDockButton();

            setupCD();

            UpdateParentMinimumSize();

            UpdateTooltipOfToggleButton();

            UpdateOpenSSHConsoleButtonState();

            toggleConsoleButton.EnabledChanged += toggleConsoleButton_EnabledChanged;

            //If RDP enabled and AutoSwitchToRDP selected, switch RDP connection will be done when VNC already get the correct screen resolution.
            //This change is only for Cream, because RDP port scan was removed in Cream.
            if (Properties.Settings.Default.AutoSwitchToRDP && RDPEnabled)
                vncScreen.AutoSwitchRDPLater = true;
        }

        void ShowOrHideRdpVersionWarning()
        {
            pictureBoxGeneralInformationMessage.Visible = labelGeneralInformationMessage.Visible = vncScreen.RdpVersionWarningNeeded;
        }

        public bool IsScaled
        {
            get { return scaleCheckBox.Checked; }
            set { scaleCheckBox.Checked = value; }
        }

        //CA-75479 - add to aid debugging
        private void toggleConsoleButton_EnabledChanged(object sender, EventArgs e)
        {
            ButtonBase button = sender as ButtonBase;
            if (button == null)
                return;

            string format = "Console tab 'Switch to...' button disabled for VM '{0}'";

            if (button.Enabled)
                format = "Console tab 'Switch to...' button enabled for VM '{0}'";

            log.DebugFormat(format, source == null ? "unknown/null" : source.name_label);

        }

        private void VM_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove)
            {
                VM vm = e.Element as VM;
                if (source != null && vm.uuid == source.uuid)
                {
                    // the VM we are looking at has gone away. We should redock if necessary, otherwise it
                    // avoids the destroy (and re-create in the case of dom0) when the tab itself goes.
                    if (!parentVNCView.IsDocked)
                        parentVNCView.DockUnDock();
                }
            }
        }

        private void Host_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (source.IsControlDomainZero(out _))
                return;

            if (e.Element is Host host)
            {
                if (e.Action == CollectionChangeAction.Add)
                {
                    log.DebugFormat("'{0}' console: Register Server_EnabledPropertyChanged event listener on {1}",
                                    source.Name(), host.Name());
                    host.PropertyChanged -= Server_EnabledPropertyChanged;
                    host.PropertyChanged += Server_EnabledPropertyChanged;
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    log.DebugFormat("'{0}' console: Unregister Server_EnabledPropertyChanged event listener on {1}",
                                    source.Name(), host.Name());
                    host.PropertyChanged -= Server_EnabledPropertyChanged;
                }
            }
        }

        private void UnregisterEventListeners()
        {
            Properties.Settings.Default.PropertyChanged -= new PropertyChangedEventHandler(Default_PropertyChanged);

            if (source == null)
                return;

            log.DebugFormat("'{0}' console: Unregister Server_PropertyChanged event listener on {0}", source.Name());
            source.PropertyChanged -= new PropertyChangedEventHandler(Server_PropertyChanged);
            source.Connection.Cache.DeregisterCollectionChanged<VM>(VM_CollectionChangedWithInvoke);

            if (guestMetrics != null)
                guestMetrics.PropertyChanged -= guestMetrics_PropertyChanged;

            if (source.IsControlDomainZero(out Host host))
            {
                log.DebugFormat("'{0}' console: Unregister Server_PropertyChanged event listener on {1}",
                    source.Name(), host.Name());
                host.PropertyChanged -= Server_PropertyChanged;

                Host_metrics hostMetrics = source.Connection.Resolve<Host_metrics>(host.metrics);
                if (hostMetrics != null)
                {
                    log.DebugFormat("'{0}' console: Unregister Server_PropertyChanged event listener on host metrics",
                        source.Name());
                    hostMetrics.PropertyChanged -= Server_PropertyChanged;
                }
            }
            else if (source.IsSrDriverDomain(out SR sr))
            {
                log.DebugFormat("'{0}' console: Unregister Server_PropertyChanged event listener on {1}",
                    source.Name(), sr.Name());
                sr.PropertyChanged -= Server_PropertyChanged;
            }
            else
            {
                source.Connection.Cache.DeregisterCollectionChanged<Host>(Host_CollectionChangedWithInvoke);

                foreach (Host cachedHost in source.Connection.Cache.Hosts)
                {
                    log.DebugFormat("'{0}' console: Unregister Server_EnabledPropertyChanged event listener on {1}",
                                    source.Name(), cachedHost.Name());
                    cachedHost.PropertyChanged -= Server_EnabledPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Occurs when one of the properties in the preferences / options window changes.
        /// </summary>
        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Program.Invoke(this, delegate
            {
                deregisterShortCutKeys();
                registerShortCutKeys();
                UpdateParentMinimumSize();
            });
        }

        internal void UpdateParentMinimumSize()
        {
            if (Parent != null)
            {
                int[] bottomTableWidths = tableLayoutPanel1.GetColumnWidths();
                int bottomPanelWidth = bottomTableWidths.Where((t, i) => tableLayoutPanel1.ColumnStyles[i].SizeType != SizeType.Percent).Sum();
                Parent.MinimumSize = new Size(bottomPanelWidth + 100, 400);
            }
        }

        private void registerShortCutKeys()
        {
            Program.AssertOnEventThread();

            if (vncScreen == null)
                return;

            if (Properties.Settings.Default.FullScreenShortcutKey == 0)
            {
                // Ctrl + Alt
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.CTRL_ALT, waitForInsKey);
            }
            else if (Properties.Settings.Default.FullScreenShortcutKey == 1)
            {
                // Ctrl + Alt + F
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.CTRL_ALT_F, toggleFullscreen);
            }
            else if (Properties.Settings.Default.FullScreenShortcutKey == 2)
            {
                // F12
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.F12, toggleFullscreen);
            }
            else if (Properties.Settings.Default.FullScreenShortcutKey == 3)
            {
                // Ctrl + Enter
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.CTRL_ENTER, toggleFullscreen);
            }

            UpdateFullScreenButton();

            // CA-10943
            if (Properties.Settings.Default.DockShortcutKey == 1)
            {
                // Alt + Shift + U
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.ALT_SHIFT_U, toggleDockUnDock);
            }
            else if (Properties.Settings.Default.DockShortcutKey == 2)
            {
                // F11
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.F11, toggleDockUnDock);
            }
            else if (Properties.Settings.Default.DockShortcutKey == 0)
            {
                // <none>
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.ALT_SHIFT_U);
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.F11);
            }

            UpdateDockButton();

            // Uncapture keyboard and mouse Key
            if (Properties.Settings.Default.UncaptureShortcutKey == 0)
            {
                // Right Ctrl
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.RIGHT_CTRL, ToggleConsoleFocus);
            }
            else if (Properties.Settings.Default.UncaptureShortcutKey == 1)
            {
                // Left Alt
                KeyHandler.AddKeyHandler(ConsoleShortcutKey.LEFT_ALT, ToggleConsoleFocus);
            }
        }

        private void deregisterShortCutKeys()
        {
            Program.AssertOnEventThread();

            if (vncScreen == null)
                return;

            if (Properties.Settings.Default.FullScreenShortcutKey != 0)
            {
                // Ctrl + Alt
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.CTRL_ALT);
            }

            if (Properties.Settings.Default.FullScreenShortcutKey != 1)
            {
                // Ctrl + Alt + F
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.CTRL_ALT_F);
            }

            if (Properties.Settings.Default.FullScreenShortcutKey != 2)
            {
                // F12
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.F12);
            }
            if (Properties.Settings.Default.FullScreenShortcutKey != 3)
            {
                // Ctrl + Enter
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.CTRL_ENTER);
            }

            if (Properties.Settings.Default.DockShortcutKey != 1)
            {
                // Alt + Shift + U
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.ALT_SHIFT_U);
            }

            if (Properties.Settings.Default.DockShortcutKey != 2)
            {
                // F11
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.F11);
            }

            // Uncapture keyboard and mouse Key
            if (Properties.Settings.Default.UncaptureShortcutKey != 0)
            {
                // Right Ctrl
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.RIGHT_CTRL);
            }
            if (Properties.Settings.Default.UncaptureShortcutKey != 1)
            {
                // Left Alt
                KeyHandler.RemoveKeyHandler(ConsoleShortcutKey.LEFT_ALT);
            }
        }

        public void UpdateDockButton()
        {
            dockButton.Text = parentVNCView.IsDocked ? Messages.VNC_UNDOCK : Messages.VNC_REDOCK;
            if (Properties.Settings.Default.DockShortcutKey == 1)
            {
                dockButton.Text += Messages.VNC_DOCK_ALT_SHIFT_U;
            }
            else if (Properties.Settings.Default.DockShortcutKey == 2)
            {
                dockButton.Text += Messages.VNC_DOCK_F11;
            }
            dockButton.Image = parentVNCView.IsDocked ? Images.StaticImages.detach_24 : Images.StaticImages.attach_24;
        }

        public void UpdateFullScreenButton()
        {
            switch (Properties.Settings.Default.FullScreenShortcutKey)
            {
                case 0:
                    fullscreenButton.Text = Messages.VNC_FULLSCREEN_CTRL_ALT;
                    break;
                case 1:
                    fullscreenButton.Text = Messages.VNC_FULLSCREEN_CTRL_ALT_F;
                    break;
                case 2:
                    fullscreenButton.Text = Messages.VNC_FULLSCREEN_F12;
                    break;
                default:
                    fullscreenButton.Text = Messages.VNC_FULLSCREEN_CTRL_ENTER;
                    break;
            }
        }

        private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "power_state"
                || e.PropertyName == "live"
                || e.PropertyName == "allowed_operations")
            {
                log.DebugFormat("'{0}' console: Update power state, after receiving property change notification, PropertyName='{1}'", sender.ToString(), e.PropertyName);
                updatePowerState();
            }
            else if (e.PropertyName == "VBDs")
            {
                //The CD device may have changed
                Program.Invoke(this, setupCD);
            }
            else if (e.PropertyName == "guest_metrics")
            {
                var newGuestMetrics = source.Connection.Resolve(source.guest_metrics);

                //unsubscribing from the previous instance's event
                if (guestMetrics != null)
                    guestMetrics.PropertyChanged -= guestMetrics_PropertyChanged;

                guestMetrics = newGuestMetrics;
                if (guestMetrics != null)
                    guestMetrics.PropertyChanged += guestMetrics_PropertyChanged;

                EnableRDPIfCapable();

                UpdateOpenSSHConsoleButtonState(); //guest_metrics change when there is an IP address change on a VIF
            }
            else if (e.PropertyName == "VIFs" || e.PropertyName == "PIFs")
            {
                UpdateOpenSSHConsoleButtonState();
            }

            if (e.PropertyName == "name_label")
            {
                string text = null;

                if (source.IsControlDomainZero(out Host host))
                    text = string.Format(Messages.CONSOLE_HOST, host.Name());
                else if (source.IsSrDriverDomain(out SR sr))
                    text = string.Format(Messages.CONSOLE_SR_DRIVER_DOMAIN, sr.Name());

                if (text != null)
                {
                    HostLabel.Text = text;

                    if (parentVNCView?.undockedForm != null)
                        parentVNCView.undockedForm.Text = text;
                }
            }
        }

        private void guestMetrics_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "other")
            {
                if (RDPEnabled)
                {// If current connection is VNC, will enable RDP connection as following conditions:
                 // 1. Click "Turn on RDP which cause tryToConnectRDP = true"
                 // 2. RDP status changed by turned on RDP in-guest and with "Automatically switch to the Remote 
                 //    Desktop console when it becomes available" is on. But if user already choose connection type by click "Turn on/Switch to Remote Desktop"
                 //    or "Switch to Default desktop", we will take AutoSwitchToRDP as no effect
                    if (vncScreen.UseVNC && (tryToConnectRDP || (!vncScreen.UserWantsToSwitchProtocol && Properties.Settings.Default.AutoSwitchToRDP)))
                    {
                        tryToConnectRDP = false;

                        ThreadPool.QueueUserWorkItem(TryToConnectRDP);
                    }
                }
                else
                    EnableRDPIfCapable();
                UpdateButtons();
            }
            else if (e.PropertyName == "networks")
            {
                UpdateOpenSSHConsoleButtonState();
            }
        }

        private void EnableRDPIfCapable()
        {
            var enable = source.CanUseRDP();
            if(enable)
                log.DebugFormat("'{0}' console: Enabling RDP button, because RDP capability has appeared.", source);
            toggleConsoleButton.Visible = toggleConsoleButton.Enabled = enable;
        }

        private void Server_EnabledPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "enabled" || source.IsControlDomainZero(out _))
                return;

            Host host = sender as Host;
            if (host == null)
                return;

            if (targetHost == null || targetHost.Equals(host))
            {
                log.DebugFormat(
                    "'{0}' console: Update power state, after receiving property change notification, PropertyName='{1}'",
                    sender, e.PropertyName);
                updatePowerState();
            }
        }

        public void setupCD()
        {
            multipleDvdIsoList1.VM = source;
        }

        private void updatePowerState()
        {
            if (source.IsControlDomainZero(out Host host))
            {
                Host_metrics hostMetrics = source.Connection.Resolve(host.metrics);
                if (hostMetrics == null)
                    return;

                if (hostMetrics.live)
                {
                    Program.Invoke(this, showTopBarContents);
                }
                else
                {
                    Program.Invoke(this, hideTopBarContents);
                }
            }
            else
            {
                switch (source.power_state)
                {
                    case vm_power_state.Halted:
                    case vm_power_state.Paused:
                    case vm_power_state.Suspended:
                        Program.Invoke(this, hideTopBarContents);
                        break;
                    case vm_power_state.Running:
                        Program.Invoke(this, showTopBarContents);
                        Program.Invoke(this, maybeEnableButton);
                        break;
                }
            }

            UpdateOpenSSHConsoleButtonState();
        }

        /// <summary>
        /// CA-8966: No way to get from graphical to text console if vm's networking is broken on startup
        /// </summary>
        private void maybeEnableButton()
        {
            if (vncScreen != null && (!vncScreen.UseVNC || !vncScreen.UseSource))
            {
                toggleConsoleButton.Enabled = true;
            }
        }

        private void EnablePowerStateLabel(String label)
        {
            powerStateLabel.Enabled = true;
            powerStateLabel.Text = label;
            powerStateLabel.Cursor = Cursors.Hand;
        }

        private void DisablePowerStateLabel(String label)
        {
            powerStateLabel.Enabled = false;
            powerStateLabel.Text = label;
            powerStateLabel.Cursor = Cursors.Default;
        }

        private void hideTopBarContents()
        {
            VMPowerOff();
            if (source.IsControlDomainZero(out _))
            {
                log.DebugFormat("'{0}' console: Hide top bar contents, server is unavailable", source.Name());
                DisablePowerStateLabel(Messages.CONSOLE_HOST_DEAD);
            }
            else
            {
                log.DebugFormat("'{0}' console: Hide top bar contents, powerstate='{1}'", source.Name(), vm_power_state_helper.ToString(source.power_state));
                if (source.power_state == vm_power_state.Halted)
                {
                    if (source.allowed_operations.Contains(vm_operations.start) &&
                        Helpers.EnabledTargetExists(targetHost, source.Connection))
                    {
                        EnablePowerStateLabel(Messages.CONSOLE_POWER_STATE_HALTED_START);
                    }
                    else
                    {
                        DisablePowerStateLabel(Messages.CONSOLE_POWER_STATE_HALTED);
                    }
                }
                else if (source.power_state == vm_power_state.Paused)
                {
                    // CA-12637: Pause/UnPause is not supported in the GUI.  Comment out
                    // the EnablePowerStateLabel because it gives the impression that we
                    // support unpause via the GUI.
                    DisablePowerStateLabel(Messages.CONSOLE_POWER_STATE_PAUSED);
                }
                else if (source.power_state == vm_power_state.Suspended)
                {
                    if (source.allowed_operations.Contains(vm_operations.resume) &&
                        Helpers.EnabledTargetExists(targetHost, source.Connection))
                    {
                        EnablePowerStateLabel(Messages.CONSOLE_POWER_STATE_SUSPENDED_RESUME);
                    }
                    else
                    {
                        DisablePowerStateLabel(Messages.CONSOLE_POWER_STATE_SUSPENDED);
                    }
                }
            }
            powerStateLabel.Show();
        }

        private void showTopBarContents()
        {
            log.DebugFormat("'{0}' console: Show top bar contents, source is running", source.Name());
            Program.AssertOnEventThread();
            VMPowerOn();
            powerStateLabel.Hide();
        }

        private void powerStateLabel_Click(object sender, EventArgs e)
        {
            if (!powerStateLabel.Enabled)
            {
                return;
            }

            switch (source.power_state)
            {
                case vm_power_state.Halted:
                    DisablePowerStateLabel(Messages.CONSOLE_POWER_STATE_HALTED);
                    if (source.allowed_operations.Contains(vm_operations.start))
                    {
                        DisablePowerStateLabel(powerStateLabel.Text);

                        new StartVMCommand(Program.MainWindow, source).Run();
                    }
                    break;
                case vm_power_state.Paused:
                    DisablePowerStateLabel(Messages.CONSOLE_POWER_STATE_PAUSED);
                    //if(source.allowed_operations.Contains(vm_operations.unpause))
                    //    new Actions.VmAction(source.Connection, VmActionKind.UnpauseActiveView, source, null).RunAsync();
                    break;
                case vm_power_state.Suspended:
                    DisablePowerStateLabel(Messages.CONSOLE_POWER_STATE_SUSPENDED);
                    if (source.allowed_operations.Contains(vm_operations.resume))
                    {
                        DisablePowerStateLabel(powerStateLabel.Text);
                        new ResumeVMCommand(Program.MainWindow, source).Run();
                    }
                    break;
            }
        }

        public void Pause()
        {
            if (vncScreen != null && !isFullscreen)
                vncScreen.Pause();
        }

        public void Unpause()
        {
            if (vncScreen != null)
                vncScreen.Unpause();
        }

        private bool CanEnableRDP()
        {
            return RDPControlEnabled && !RDPEnabled;
        }

        // Make the 'enable RDP' button show something sensible if we can...
        private string GuessNativeConsoleLabel(VM source)
        {
            string label = Messages.VNC_LOOKING;

            if (source == null)
                return label;

            XenRef<VM_guest_metrics> gm = source.guest_metrics;
            if (gm == null)
                return label;

            if (Program.MainWindow.SelectionManager.Selection.GetConnectionOfFirstItem() == null)
                return label;

            VM_guest_metrics gmo = Program.MainWindow.SelectionManager.Selection.GetConnectionOfFirstItem().Resolve<VM_guest_metrics>(gm);
            if (gmo == null)
                return label;

            if (gmo != null && gmo.os_version != null)
            {
                if (gmo.os_version.ContainsKey("name"))
                {
                    string osString = gmo.os_version["name"];
                    if (osString != null)
                    {
                        if (osString.Contains("Microsoft"))
                            label = CanEnableRDP() ? enableRDP : UseRDP;
                        else
                            label = UseXVNC;
                    }
                }
            }

            return label;
        }

        private void RDPorVNCResizeHandler(object sender, EventArgs e)
        {
            Program.AssertOnEventThread();

            if (vncScreen == null)
                return; // This is the first event when vncScreen is created.

            VNCResizeHandler_();
        }

        private void VNCResizeHandler_()
        {
            if (!ignoringResizes &&
                DesktopSizeHasChanged() &&
                !scaleCheckBox.Checked)
            {
                MaybeScale();
            }
        }

        public void MaybeScale()
        {
            Program.AssertOnEventThread();

            if (vncScreen.DesktopSize.Width > 10 &&
                contentPanel.Width < vncScreen.DesktopSize.Width)
            {
                if (!Properties.Settings.Default.PreserveScaleWhenSwitchBackToVNC)
                {
                    scaleCheckBox.Checked = true;
                }
                else
                {
                    scaleCheckBox.Checked = oldScaleValue || firstTime;
                    firstTime = false;
                }
            }
            else if (Properties.Settings.Default.PreserveScaleWhenSwitchBackToVNC)
            {
                scaleCheckBox.Checked = oldScaleValue;
            }
            scaleCheckBox_CheckedChanged(null, null);
        }

        private bool DesktopSizeHasChanged()
        {
            if (!LastDesktopSize.Equals(vncScreen.DesktopSize))
            {
                LastDesktopSize = vncScreen.DesktopSize;
                return true;
            }
            else
            {
                return false;
            }
        }

        public Size GrowToFit()
        {
            Program.AssertOnEventThread();
            ignoringResizes = true;
            try
            {
                scaleCheckBox.Checked = false;
                Size working_area = Screen.FromControl(this).WorkingArea.Size;
                if (vncScreen.DesktopSize.Width > 10 &&
                    vncScreen.DesktopSize.Width < working_area.Width &&
                    vncScreen.DesktopSize.Height < working_area.Height)
                {
                    int twoTimeBorderPadding = VNCGraphicsClient.BORDER_PADDING * 2;

                    return new Size(vncScreen.DesktopSize.Width + twoTimeBorderPadding,
                                    vncScreen.DesktopSize.Height + gradientPanel1.Height + tableLayoutPanel1.Height + twoTimeBorderPadding);
                }
                else
                {
                    scaleCheckBox.Checked = true;
                    return new Size((working_area.Width * 2) / 3, (working_area.Height * 2) / 3);
                }
            }
            finally
            {
                ignoringResizes = false;
            }
        }

        private void scaleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ignoreScaleChange)
                return;

            try
            {
                ignoringResizes = true;
                vncScreen.Scaling = scaleCheckBox.Checked;
            }
            finally
            {
                ignoringResizes = false;
            }

            FocusVNC();
        }

        private void sendCAD_Click(object sender, EventArgs e)
        {
            vncScreen.SendCAD();
            FocusVNC();
        }

        private void dockButton_Click(object sender, EventArgs e)
        {
            if (isFullscreen)
                return;
            parentVNCView.DockUnDock();
        }

        private void fullscreenButton_Click(object sender, EventArgs e)
        {
            toggleFullscreen();
        }

        private System.Threading.Timer insKeyTimer;

        private void waitForInsKey()
        {
            lock (insKeyTimer)
            {
                insKeyTimer.Change(INS_KEY_TIMEOUT, Timeout.Infinite);
            }
        }

        private void cancelWaitForInsKeyAndSendCAD()
        {
            lock (insKeyTimer)
            {
                // We have seen the INS key, so lets cancel the timer and send CAD

                insKeyTimer.Change(Timeout.Infinite, Timeout.Infinite);
                vncScreen.SendCAD();
            }
        }

        private void notInsKeyPressed(Object o)
        {
            Program.AssertOffEventThread();

            Program.Invoke(this, delegate ()
            {
                lock (insKeyTimer)
                {
                    // We have not seen the INS key, so lets toggleFullscreen and cancel the timer

                    toggleFullscreen();
                    insKeyTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            });
        }

        private bool isFullscreen
        {
            get
            {
                return fullscreenForm != null;
            }
        }

        // CA-10943
        private bool inToggleDockUnDock = false;
        private void toggleDockUnDock()
        {
            Program.AssertOnEventThread();

            if (inToggleDockUnDock)
                return;

            inToggleDockUnDock = true;
            dockButton_Click(null, null);
            inToggleDockUnDock = false;
        }

        private bool inToggleFullscreen = false;
        private void toggleFullscreen()
        {
            Program.AssertOnEventThread();

            if (inToggleFullscreen)
                return;

            inToggleFullscreen = true;

            if (!isFullscreen)
            {
                fullscreenForm = new FullScreenForm(this);
                fullscreenForm.FormClosing += delegate { toggleFullscreen(); };

                if (source != null && source.Connection != null)
                    source.Connection.BeforeConnectionEnd += Connection_BeforeConnectionEnd;

                fullscreenForm.AttachVncScreen(vncScreen);
                vncScreen.DisplayFocusRectangle = false;

                fullscreenHint = new FullScreenHint();
                fullscreenForm.Show();
                fullscreenHint.Show(fullscreenForm);

                FocusVNC();
                vncScreen.CaptureKeyboardAndMouse();
                UpdateRDPResolution(true);
            }
            else
            {
                if (source != null && source.Connection != null)
                    source.Connection.BeforeConnectionEnd -= Connection_BeforeConnectionEnd;

                fullscreenForm.DetachVncScreen(vncScreen);
                vncScreen.Parent = contentPanel;
                vncScreen.DisplayFocusRectangle = true;
                FocusVNC();
                vncScreen.CaptureKeyboardAndMouse();

                fullscreenForm.Hide();
                fullscreenForm.Dispose();
                fullscreenForm = null;
                UpdateRDPResolution(false);
            }

            //Everytime we toggle full screen I'm going to force an unpause to make sure we don't acidentally undock / dock a pause VNC
            vncScreen.Unpause();

            inToggleFullscreen = false;

            // CA-30477: This refresh stops a scroll bar being painted on the fullscreen form under vista
            if (fullscreenForm != null)
                fullscreenForm.Refresh();
        }

        void Connection_BeforeConnectionEnd(IXenConnection conn)
        {
            Program.Invoke(this, toggleFullscreen);
        }

        private const bool RDP = true;
        private const bool XVNC = false;
        private bool toggleToXVNCorRDP = RDP;

        public void DisableToggleVNCButton()
        {
            Program.AssertOnEventThread();
            toggleConsoleButton.Enabled = false;
        }

        public void EnableToggleVNCButton()
        {
            Program.AssertOnEventThread();
            toggleConsoleButton.Enabled = true;
        }

        private void OnDetectRDP()
        {
            Program.Invoke(this, OnDetectRDP_);
        }

        private void OnDetectRDP_()
        {
            try
            {
                log.DebugFormat("RDP detected for VM '{0}'", source == null ? "unknown/null" : source.name_label);
                toggleToXVNCorRDP = RDP;

                if (vncScreen.UseVNC)
                    toggleConsoleButton.Text = CanEnableRDP() ? enableRDP : UseRDP;

                EnableRDPIfCapable();
                tip.SetToolTip(toggleConsoleButton, null);
                if (!vncScreen.UserWantsToSwitchProtocol && Properties.Settings.Default.AutoSwitchToRDP)
                {
                    if (Program.MainWindow.TheTabControl.SelectedTab == Program.MainWindow.TabPageConsole)
                        toggleConsoleButton_Click(null, null);
                    else
                        switchOnTabOpened = true;
                }
            }
            catch (InvalidOperationException exn)
            {
                log.Warn(exn, exn);
            }
        }

        private void OnDetectVNC()
        {
            Program.Invoke(this, OnDetectVNC_);
        }

        private void OnDetectVNC_()
        {
            try
            {
                log.DebugFormat("VNC detected for VM '{0}'", source == null ? "unknown/null" : source.name_label);
                toggleToXVNCorRDP = XVNC;
                toggleConsoleButton.Text = vncScreen.UseSource ? UseXVNC : UseVNC;
                toggleConsoleButton.Enabled = true;
                tip.SetToolTip(toggleConsoleButton, null);
            }
            catch (InvalidOperationException exn)
            {
                log.Warn(exn, exn);
            }
        }

        private bool firstTime = true;
        private bool oldScaleValue = false;
        private bool tryToConnectRDP = false; // This parameter will be set after click "TURN ON Rremote Desktop" and will connect RDP when RDP status changed.

        /// <summary>
        /// Switch between graphical and text consoles.
        /// </summary>
        private void toggleConsoleButton_Click(object sender, EventArgs e)
        {
            bool rdp = (toggleToXVNCorRDP == RDP);

            try
            {
                if (rdp)
                {
                    if (vncScreen.UseVNC)
                        oldScaleValue = scaleCheckBox.Checked;

                    vncScreen.UseVNC = !vncScreen.UseVNC;
                    vncScreen.UserWantsToSwitchProtocol = true;

                    if (CanEnableRDP())
                    {
                        DialogResult dialogResult;
                        using (ThreeButtonDialog dlg = new NoIconDialog(Messages.FORCE_ENABLE_RDP,
                            ThreeButtonDialog.ButtonYes, ThreeButtonDialog.ButtonNo)
                        { HelpNameSetter = "EnableRDPonVM" })
                        {
                            dialogResult = dlg.ShowDialog(Program.MainWindow);
                        }
                        if (dialogResult == DialogResult.Yes)
                        {
                            Session session = source.Connection.DuplicateSession();
                            Dictionary<string, string> _arguments = new Dictionary<string, string>();
                            VM.call_plugin(session, source.opaque_ref, "guest-agent-operation", "request-rdp-on", _arguments);
                            tryToConnectRDP = true;
                        }
                    }

                    // disable toggleConsoleButton; it will be re-enabled in TryToConnectRDP() when rdp port polling is complete (CA-102755)
                    if (vncScreen.RdpIp == null)
                        toggleConsoleButton.Enabled = false;
                    ThreadPool.QueueUserWorkItem(TryToConnectRDP);
                }
                else
                {
                    oldScaleValue = scaleCheckBox.Checked;
                    vncScreen.UseSource = !vncScreen.UseSource;
                }
                Unpause();
                UpdateButtons();
            }
            catch (COMException ex)
            {
                log.DebugFormat("Disabling toggle-console button as COM related exception thrown: {0}", ex.Message);
                toggleConsoleButton.Enabled = false;
            }
        }

        private void UpdateButtons()
        {
            bool rdp = (toggleToXVNCorRDP == RDP);
            if (rdp)
                toggleConsoleButton.Text = vncScreen.UseVNC
                    ? CanEnableRDP() ? enableRDP : UseRDP
                    : UseStandardDesktop;
            else
                toggleConsoleButton.Text = vncScreen.UseSource ? UseXVNC : UseVNC;

            UpdateTooltipOfToggleButton();

            scaleCheckBox.Visible = !rdp || vncScreen.UseVNC;
            sendCAD.Enabled = !rdp || vncScreen.UseVNC;
            FocusVNC();
            ignoreScaleChange = true;
            if (!Properties.Settings.Default.PreserveScaleWhenSwitchBackToVNC)
            {
                scaleCheckBox.Checked = false;
            }
            ignoreScaleChange = false;
            scaleCheckBox_CheckedChanged(null, null);  // make sure scale setting is now correct: CA-84324

            ShowOrHideRdpVersionWarning();
        }

        private void UpdateTooltipOfToggleButton()
        {
            if (RDPEnabled || RDPControlEnabled)
                tip.SetToolTip(toggleConsoleButton, null);
        }

        private void TryToConnectRDP(object x)
        {
            bool hasToReconnect = vncScreen.RdpIp == null;
            vncScreen.RdpIp = vncScreen.PollPort(XSVNCScreen.RDPPort, true);
            Program.Invoke(this, (MethodInvoker)(() =>
            {
                if (hasToReconnect)
                {
                    vncScreen.UseVNC = true;
                    vncScreen.UseVNC = false;
                }
                Unpause();
                UpdateButtons();
                toggleConsoleButton.Enabled = true; // make sure the toggleConsoleButton is enabled after rdp port polling (CA-102755) 
            }));

        }

        /// <summary>
        /// Called when the user cancelled VNC authentication.
        /// </summary>
        private void OnUserCancelledAuth(object sender, EventArgs e)
        {
            // Switch back to the text console
            toggleConsoleButton_Click(null, null);
        }

        /// <summary>
        /// Called when the a connection attempt to VNC is cancelled.
        /// </summary>
        private void OnVncConnectionAttemptCancelled(object sender, EventArgs e)
        {
            // Switch back to the text console
            toggleConsoleButton_Click(null, null);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            FocusVNC();
        }

        internal void SendCAD()
        {
            if (vncScreen != null)
                vncScreen.SendCAD();
        }

        internal void focus_vnc()
        {
            if (vncScreen != null)
                FocusVNC();
        }

        // Focus the VNC screen, as long as we're in the right place at the moment.
        // Otherwise ignore the request to focus or it will steal the keyboard from
        // the rightful owner: see CA-41120.
        private void FocusVNC()
        {
            if (Program.MainWindow.ContainsFocus && Program.MainWindow.TheTabControl.SelectedTab == Program.MainWindow.TabPageConsole)
                vncScreen.Focus();
        }

        internal void VMPowerOff()
        {
            toggleConsoleButton.Enabled = false;

            VBD cdDrive = source.FindVMCDROM();

            multipleDvdIsoList1.Enabled = cdDrive == null ||
                                          source.power_state == vm_power_state.Halted &&
                                          (cdDrive.allowed_operations.Contains(vbd_operations.eject) ||
                                           cdDrive.allowed_operations.Contains(vbd_operations.insert));

            sendCAD.Enabled = false;
        }

        internal void VMPowerOn()
        {
            //No need to reenable toggleConsoleButton, polling will do it.
            multipleDvdIsoList1.Enabled = true;
            sendCAD.Enabled = true;
        }

        internal void RdpDisconnectedHandler(object sender, EventArgs e)
        {
            Program.AssertOnEventThread();

            if (vncScreen == null)
                return; // This is the first event when vncScreen is created.

            rdpDisconnected();
        }

        private void rdpDisconnected()
        {
            if (!vncScreen.UseVNC)
                toggleConsoleButton_Click(null, null);

            if (!RDPControlEnabled)
                toggleConsoleButton.Enabled = false;

            vncScreen.ImmediatelyPollForConsole();
        }

        internal void SwitchIfRequired()
        {
            if (switchOnTabOpened)
            {
                toggleConsoleButton_Click(null, null);
                switchOnTabOpened = false;
            }
        }

        bool droppedDown = false;
        private readonly CollectionChangeEventHandler Host_CollectionChangedWithInvoke;
        private readonly CollectionChangeEventHandler VM_CollectionChangedWithInvoke;

        private void LifeCycleButton_MouseClick(object sender, EventArgs e)
        {
            if (droppedDown)
            {
                LifeCycleMenuStrip.Hide();
                return;
            }

            if (source == null)
                return;

            ContextMenuItemCollection contextMenuItems = new ContextMenuItemCollection();

            if (source.IsControlDomainZero(out Host host))
            {
                // We're looking at the host console
                if (host.Connection.IsConnected)
                {
                    contextMenuItems.Add(new ShutDownHostCommand(Program.MainWindow, host, this));
                    contextMenuItems.Add(new RebootHostCommand(Program.MainWindow, host, this));
                }
            }
            else
            {
                contextMenuItems.AddIfEnabled(new ShutDownVMCommand(Program.MainWindow, source, this));
                contextMenuItems.AddIfEnabled(new RebootVMCommand(Program.MainWindow, source, this));
                contextMenuItems.AddIfEnabled(new SuspendVMCommand(Program.MainWindow, source, this));
                contextMenuItems.AddIfEnabled(new ForceVMShutDownCommand(Program.MainWindow, source, this));
                contextMenuItems.AddIfEnabled(new ForceVMRebootCommand(Program.MainWindow, source, this));
            }

            LifeCycleMenuStrip.Items.Clear();
            LifeCycleMenuStrip.Items.AddRange(contextMenuItems.ToArray());
            LifeCycleMenuStrip.Show(pictureBox1, pictureBox1.Left, pictureBox1.Bottom);
        }

        public void showHeaderBar(bool showHeaderBar, bool showLifecycleIcon)
        {
            gradientPanel1.Visible = showHeaderBar;
            pictureBox1.Visible = showLifecycleIcon;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Image = Images.StaticImages.lifecycle_hot;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = droppedDown ? Images.StaticImages.lifecycle_pressed : Images.StaticImages._001_LifeCycle_h32bit_24;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = Images.StaticImages.lifecycle_pressed;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = droppedDown ? Images.StaticImages.lifecycle_pressed : Images.StaticImages.lifecycle_hot;
        }

        private void LifeCycleMenuStrip_Opened(object sender, EventArgs e)
        {
            droppedDown = true;
        }

        private void LifeCycleMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason != ToolStripDropDownCloseReason.AppClicked || !pictureBox1.ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                droppedDown = false;
                pictureBox1.Image = Images.StaticImages._001_LifeCycle_h32bit_24;
            }
            else
            {
                e.Cancel = true;
            }
        }

        internal Image Snapshot()
        {
            if (vncScreen != null)
            {
                return vncScreen.Snapshot();
            }

            return null;
        }

        private bool inToggleConsoleFocus = false;
        private void ToggleConsoleFocus()
        {
            Program.AssertOnEventThread();

            if (inToggleConsoleFocus)
                return;

            inToggleConsoleFocus = true;

            if (vncScreen.Focused && vncScreen.ActiveControl == null)
                vncScreen.CaptureKeyboardAndMouse(); // focus console
            else
            {
                vncScreen.UncaptureKeyboardAndMouse(); // defocus console
                vncScreen.Refresh();
            }

            inToggleConsoleFocus = false;
        }

        private void ShowGpuWarningIfRequired(bool mustConnectRemoteDesktop)
        {
            dedicatedGpuWarning.Visible = mustConnectRemoteDesktop;
        }

        public void UpdateRDPResolution(bool fullscreen = false)
        {
            if (vncScreen != null)
                vncScreen.UpdateRDPResolution(fullscreen);
        }

        #region SSH Console methods

        private void buttonSSH_Click(object sender, EventArgs e)
        {
            if (!IsSSHConsoleSupported || !CanStartSSHConsole) 
                return;

            var customSshConsole = Properties.Settings.Default.CustomSshConsole;
            var sshConsolePath = GetConsolePath(customSshConsole);

            if (string.IsNullOrEmpty(sshConsolePath))
            {
                OpenSshConsoleWarningDialog(Messages.CONFIGURE_SSH_CONSOLE_FILE_NOT_CONFIGURED);
                return;
            }

            if (!File.Exists(sshConsolePath))
            {
                OpenSshConsoleWarningDialog(Messages.CONFIGURE_SSH_CONSOLE_FILE_NOT_FOUND);
                return;
            }

            try
            {
                var command = $"{source.IPAddressForSSH()}";
                if (customSshConsole == SshConsole.OpenSSH)
                {
                    // OpenSSH doesn't have an option to prompt the user for the username.
                    // We use the session's subject.
                    var currentSubject = source.Connection.Resolve(source.Connection.Session.SessionSubject);
                    command = $"{currentSubject?.SubjectName ?? "root"}@{source.IPAddressForSSH()}";
                }
                Process.Start(new ProcessStartInfo(sshConsolePath, command));
            }
            catch (Exception ex)
            {
                log.Error("Could not start the selected SSH console.", ex);
                OpenSshConsoleErrorDialog();
            }
        }

        private static string GetConsolePath(SshConsole customSshConsole)
        {
            switch (customSshConsole)
            {
                case SshConsole.Putty:
                    return Properties.Settings.Default.PuttyLocation;
                case SshConsole.OpenSSH:
                    return Properties.Settings.Default.OpenSSHLocation;
                default:
                    return null;
            }
        }

        private void OpenSshConsoleWarningDialog(string message)
        {
            var configureSshClientButton = new ThreeButtonDialog.TBDButton(Messages.CONFIGURE_SSH_CONSOLE_TITLE,
                DialogResult.OK, ThreeButtonDialog.ButtonType.ACCEPT, true);

            using (var dlg = new WarningDialog(message, configureSshClientButton, ThreeButtonDialog.ButtonCancel))
            {
                if (dlg.ShowDialog(Parent) == DialogResult.OK)
                {
                    OpenExternalToolsPage();
                }
            }
        }

        private void OpenSshConsoleErrorDialog()
        {
            var configureSshClientButton = new ThreeButtonDialog.TBDButton(Messages.CONFIGURE_SSH_CONSOLE_TITLE,
                DialogResult.OK, ThreeButtonDialog.ButtonType.ACCEPT, true);

            using (var dlg = new ErrorDialog(Messages.CONFIGURE_SSH_CONSOLE_ERROR,
                       configureSshClientButton, ThreeButtonDialog.ButtonCancel))
            {
                if (dlg.ShowDialog(Parent) == DialogResult.OK)
                {
                    OpenExternalToolsPage();
                }
            }
        }

        private void OpenExternalToolsPage()
        {
            using (var optionsDialog = new OptionsDialog(Program.MainWindow.PluginManager))
            {
                optionsDialog.SelectExternalToolsPage();
                optionsDialog.ShowDialog();
            }
        }

        private void UpdateOpenSSHConsoleButtonState()
        {
            var isSshConsoleSupported = IsSSHConsoleSupported;
            buttonSSH.Visible = isSshConsoleSupported && source.power_state != vm_power_state.Halted;
            buttonSSH.Enabled = isSshConsoleSupported && CanStartSSHConsole;
        }

        private bool IsSSHConsoleSupported
        {
            get
            {
                if (source.IsWindows())
                    return false;

                if (source.IsControlDomainZero(out Host host))
                {
                    Host_metrics hostMetrics = source.Connection.Resolve(host.metrics);
                    if (hostMetrics == null)
                        return false;

                    if (!hostMetrics.live)
                        return false;
                }

                return true;
            }
        }

        private bool CanStartSSHConsole =>
            source.power_state == vm_power_state.Running && !string.IsNullOrEmpty(source.IPAddressForSSH());

        #endregion SSH Console methods
    }
}
