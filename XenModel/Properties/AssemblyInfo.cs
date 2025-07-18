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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XenAdmin.Properties;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e80f8412-2075-4c6b-bf10-c94576b26de4")]

[assembly: InternalsVisibleTo("XenAdminTests")]

[assembly: CustomBranding(
    "XCP-ng Center",
    "XCP-ng",
    "XCP-ng",
    "",
    "https://github.com/xcp-ng/xenadmin/releases",
    "",
    "XCP-ng VM Tools",
    "XCP-ng")]

namespace XenAdmin.Properties
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class CustomBrandingAttribute : Attribute
    {
        public CustomBrandingAttribute(
            string brandConsole,
            string companyNameShort,
            string productBrand,
            string productVersionText,
            string xcUpdatesUrl,
            string cfuUrl,
            string vmTools,
            string xenHost
        )
        {
            BrandConsole = brandConsole;
            CompanyNameShort = companyNameShort;
            ProductBrand = productBrand;
            ProductVersionText = productVersionText;
            XcUpdatesUrl = xcUpdatesUrl;
            CfuUrl = cfuUrl;
            VmTools = vmTools;
            XenHost = xenHost;
        }

        public string BrandConsole { get; }
        public string CompanyNameShort { get; }
        public string ProductBrand { get; }
        public string ProductVersionText { get; }
        public string VmTools { get; }
        public string XcUpdatesUrl { get; }
        public string CfuUrl { get; }
        public string XenHost { get; }
    }
}
