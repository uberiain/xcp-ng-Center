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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using XenAdmin.Core;

namespace XenAdmin.Controls.MainWindowControls
{
    class NotificationsView : FlickerFreeListBox
    {
        private const int IMG_LEFT_MARGIN = 16;
        private const int IMG_RIGHT_MARGIN = 8;

        [Browsable(true)]
        public event Action<NotificationsSubModeItem> NotificationsSubModeChanged;

        public NotificationsView()
        {
            Items.Add(new NotificationsSubModeItem(NotificationsSubMode.Alerts));
            Items.Add(new NotificationsSubModeItem(NotificationsSubMode.Events));
        }

        public int GetTotalEntries()
        {
            int total = 0;
            foreach (var item in Items)
            {
                if (item is NotificationsSubModeItem subModeItem)
                    total += subModeItem.UnreadEntries;
            }
            return total;
        }

        public void UpdateEntries(NotificationsSubMode subMode, int entries)
        {
            foreach (var item in Items)
            {
                if (item is NotificationsSubModeItem subModeItem && subModeItem.SubMode == subMode)
                {
                    subModeItem.UnreadEntries = entries;
                    break;
                }
            }

            Invalidate();
        }

        public void SelectNotificationsSubMode(NotificationsSubMode subMode)
        {
            foreach (var item in Items)
            {
                if (item is NotificationsSubModeItem subModeItem && subModeItem.SubMode == subMode)
                {
                    var lastSelected = SelectedItem;
                    SelectedItem = item;

                    if (lastSelected == SelectedItem)
                        OnSelectedIndexChanged(EventArgs.Empty);
                    
                    break;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ItemHeight = 40;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (!(Items[e.Index] is NotificationsSubModeItem item))
                return;

            int itemHeight = e.Bounds.Height;
            int imgWidth = item.Image.Width;
            int imgHeight = item.Image.Height;
            
            e.DrawBackground();
            e.Graphics.DrawImage(item.Image,
                                 e.Bounds.Left + IMG_LEFT_MARGIN,
                                 e.Bounds.Top + (itemHeight - imgHeight) / 2);

            FontStyle style = item.SubMode == NotificationsSubMode.Events && item.UnreadEntries > 0
                                  ? FontStyle.Bold
                                  : FontStyle.Regular;

            using (Font font = new Font(Font, style))
            {
                int textLeft = e.Bounds.Left + IMG_LEFT_MARGIN + imgWidth + IMG_RIGHT_MARGIN;

                var textRec = new Rectangle(textLeft, e.Bounds.Top,
                                            e.Bounds.Right - textLeft, itemHeight);

                var txt = item.Text;
                if (!string.IsNullOrWhiteSpace(item.SubText))
                    txt = $"{txt}\n{item.SubText}";

                Drawing.DrawText(e.Graphics, txt, font, textRec, e.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            if (Items[SelectedIndex] is NotificationsSubModeItem item && NotificationsSubModeChanged != null)
                NotificationsSubModeChanged(item);
        }
    }

    public enum NotificationsSubMode { Alerts, Updates, UpdatesFromCdn, Events }

    public class NotificationsSubModeItem
    {
        public readonly NotificationsSubMode SubMode;
        
        public NotificationsSubModeItem(NotificationsSubMode subMode)
        {
            SubMode = subMode;
        }

        /// <summary>
        /// For the Alerts and the Updates this is the total number of entries,
        /// while for the Events it is the number of error entries.
        /// </summary>
        public int UnreadEntries { get; set; }

        public Image Image => GetImage(SubMode, UnreadEntries);

        public string Text => GetText(SubMode, UnreadEntries);

        public string SubText => GetSubText(SubMode);

        public static Image GetImage(NotificationsSubMode subMode, int unreadEntries)
        {
            switch (subMode)
            {
                case NotificationsSubMode.Alerts:
                    return Images.StaticImages._000_Alert2_h32bit_16;
                case NotificationsSubMode.Updates:
                    return Images.StaticImages._000_Patch_h32bit_16;
                case NotificationsSubMode.UpdatesFromCdn:
                    return Images.StaticImages.notif_updates_16;
                case NotificationsSubMode.Events:
                    return unreadEntries == 0
                        ? Images.StaticImages.notif_events_16
                        : Images.StaticImages.notif_events_errors_16;
                default:
                    return null;
            }
        }

        public static string GetText(NotificationsSubMode subMode, int unreadEntries)
        {
            switch (subMode)
            {
                case NotificationsSubMode.Alerts:
                    return unreadEntries == 0
                        ? Messages.NOTIFICATIONS_SUBMODE_ALERTS_READ
                        : string.Format(Messages.NOTIFICATIONS_SUBMODE_ALERTS_UNREAD, unreadEntries);
                case NotificationsSubMode.Updates:
                case NotificationsSubMode.UpdatesFromCdn:
                    return unreadEntries == 0
                        ? Messages.NOTIFICATIONS_SUBMODE_UPDATES_READ
                        : string.Format(Messages.NOTIFICATIONS_SUBMODE_UPDATES_UNREAD, unreadEntries);
                case NotificationsSubMode.Events:
                    if (unreadEntries == 0)
                        return Messages.NOTIFICATIONS_SUBMODE_EVENTS_READ;
                    if (unreadEntries == 1)
                        return Messages.NOTIFICATIONS_SUBMODE_EVENTS_UNREAD_ONE;
                    return string.Format(Messages.NOTIFICATIONS_SUBMODE_EVENTS_UNREAD_MANY, unreadEntries);
                default:
                    return string.Empty;
            }
        }

        public static string GetSubText(NotificationsSubMode subMode)
        {
            switch (subMode)
            {
                case NotificationsSubMode.Updates:
                    return string.Format(Messages.CONFIG_LCM_UPDATES_TAB_TITLE, BrandManager.ProductVersion821);
                case NotificationsSubMode.UpdatesFromCdn:
                    return string.Format(Messages.CONFIG_CDN_UPDATES_TAB_TITLE, BrandManager.ProductBrand, BrandManager.ProductVersionPost82);
                default:
                    return string.Empty;
            }
        }
    }
}
