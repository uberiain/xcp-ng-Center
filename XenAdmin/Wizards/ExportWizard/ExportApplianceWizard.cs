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
using System.IO;
using System.Linq;
using XenAdmin.Actions;
using XenAdmin.Actions.OvfActions;
using XenAdmin.Commands;
using XenAdmin.Controls;
using XenAdmin.Core;
using XenAdmin.Network;
using XenAdmin.Wizards.GenericPages;
using XenAPI;

using Tuple = System.Collections.Generic.KeyValuePair<string, string>;

namespace XenAdmin.Wizards.ExportWizard
{
	internal partial class ExportApplianceWizard : XenWizardBase
    {
        #region Wizard Pages
        private readonly ExportAppliancePage m_pageExportAppliance;
        private readonly RBACWarningPage m_pageRbac;
        private readonly ExportSelectVMsPage m_pageExportSelectVMs;
        private readonly ExportEulaPage m_pageExportEula;
	    private readonly ExportOptionsPage m_pageExportOptions;
        private readonly ExportFinishPage m_pageFinish;
        #endregion

        private IXenObject m_selectedObject;
		private bool? m_exportAsXva;

		public ExportApplianceWizard(IXenConnection con, SelectedItemCollection selection)
			: base(con)
		{
			InitializeComponent();

		    m_pageExportAppliance = new ExportAppliancePage();
            m_pageRbac = new RBACWarningPage();
		    m_pageExportSelectVMs = new ExportSelectVMsPage();
            m_pageExportEula = new ExportEulaPage();
		    m_pageExportOptions = new ExportOptionsPage();
            m_pageFinish = new ExportFinishPage();

			m_selectedObject = selection.FirstAsXenObject;

			if (selection.Count == 1 && (m_selectedObject is VM || m_selectedObject is VM_appliance))
				m_pageExportAppliance.ApplianceFileName = m_selectedObject.Name();

			m_pageExportAppliance.OvfModeOnly = m_selectedObject is VM_appliance;
			m_pageFinish.SummaryRetriever = GetSummary;
			m_pageExportSelectVMs.SelectedItems = selection;

            AddPages(m_pageExportAppliance, m_pageExportSelectVMs, m_pageFinish);
		}

        protected override void FinishWizard()
		{
			if ((bool)m_exportAsXva)
			{
				var filename = Path.Combine(m_pageExportAppliance.ApplianceDirectory, m_pageExportAppliance.ApplianceFileName);
				if (!filename.EndsWith(".xva"))
					filename += ".xva";
				
                var vm = m_pageExportSelectVMs.VMsToExport.FirstOrDefault();

                if (vm != null)
                    new ExportVmAction(xenConnection, vm.Home(), vm, filename, m_pageFinish.VerifyExport).RunAsync();
			}
			else
			{
                new ExportApplianceAction(xenConnection,
                    m_pageExportAppliance.ApplianceDirectory,
                    m_pageExportAppliance.ApplianceFileName,
                    m_pageExportSelectVMs.VMsToExport,
                    m_pageExportEula.Eulas,
                    m_pageExportOptions.SignAppliance,
                    m_pageExportOptions.CreateManifest,
                    m_pageExportOptions.Certificate,
                    m_pageExportOptions.EncryptFiles,
                    m_pageExportOptions.EncryptPassword,
                    m_pageExportOptions.CreateOVA,
                    m_pageExportOptions.CompressOVFfiles,
                    m_pageFinish.VerifyExport).RunAsync();
			}

			base.FinishWizard();
		}

        protected override void UpdateWizardContent(XenTabPage page)
		{
			Type type = page.GetType();

			if (type == typeof(ExportAppliancePage))
			{
			    var oldExportasXva = m_exportAsXva;
			    m_exportAsXva = m_pageExportAppliance.ExportAsXva; //this ensures that m_exportAsXva is assigned a value

			    var ovfPages = new XenTabPage[] {m_pageExportEula, m_pageExportOptions};

			    if (oldExportasXva != m_exportAsXva)
			    {
			        RemovePage(m_pageRbac);

			        if ((bool)m_exportAsXva)
			        {
			            Text = Messages.MAINWINDOW_XVA_TITLE;
			            pictureBoxWizard.Image = Images.StaticImages.export_32;
			            RemovePages(ovfPages);
			        }
			        else
			        {
			            Text = Messages.EXPORT_APPLIANCE;
			            pictureBoxWizard.Image = Images.StaticImages._000_ExportVirtualAppliance_h32bit_32;
			            AddAfterPage(m_pageExportSelectVMs, ovfPages);
			        }

                    if (Helpers.ConnectionRequiresRbac(xenConnection))
                        AddRbacPage();
                }

			    m_pageExportSelectVMs.ExportAsXva = (bool)m_exportAsXva;

                if (m_pageExportSelectVMs.ApplianceDirectory != m_pageExportAppliance.ApplianceDirectory)
                    NotifyNextPagesOfChange(m_pageExportSelectVMs);

			    m_pageExportSelectVMs.ApplianceDirectory = m_pageExportAppliance.ApplianceDirectory;

                m_pageFinish.ExportAsXva = (bool)m_exportAsXva;
			}

            if (type != typeof(ExportFinishPage))
				NotifyNextPagesOfChange(m_pageFinish);
		}

        private void AddRbacPage()
        {
            var exportAsXva = m_exportAsXva.HasValue && m_exportAsXva.Value;

            var rbacDependencies = exportAsXva ? ExportVmAction.StaticRBACDependencies : ApplianceAction.StaticRBACDependencies;
            var message = exportAsXva ? Messages.RBAC_WARNING_EXPORT_WIZARD_XVA : Messages.RBAC_WARNING_EXPORT_WIZARD_APPLIANCE;

            m_pageRbac.SetPermissionChecks(xenConnection,
                new WizardRbacCheck(message, rbacDependencies) {Blocking = true});

            AddAfterPage(m_pageExportAppliance, m_pageRbac);
        }

        protected override string WizardPaneHelpID()
        {
            var curPageType = CurrentStepTabPage.GetType();

            if (curPageType == typeof(RBACWarningPage))
                return FormatHelpId((bool)m_exportAsXva ? "RbacExportXva" : "RbacExportOvf");

            if (curPageType == typeof(ExportFinishPage))
               return FormatHelpId((bool)m_exportAsXva ? "ExportFinishXva" : "ExportFinishOvf");

            return base.WizardPaneHelpID();
        }

        protected override IEnumerable<Tuple> GetSummary()
		{
			return (bool)m_exportAsXva ? GetSummaryXva() : GetSummaryOvf();
		}

		private IEnumerable<Tuple> GetSummaryOvf()
		{
			var temp = new List<Tuple>();
			temp.Add(new Tuple(Messages.FINISH_PAGE_REVIEW_APPLIANCE, m_pageExportAppliance.ApplianceFileName));
			temp.Add(new Tuple(Messages.FINISH_PAGE_REVIEW_DESTINATION, m_pageExportAppliance.ApplianceDirectory));

			bool first = true;
			foreach (var vm in m_pageExportSelectVMs.VMsToExport)
			{
				temp.Add(new Tuple(first ? Messages.FINISH_PAGE_REVIEW_VMS : "", vm.Name()));
				first = false;
			}

			first = true;
			foreach (var eula in m_pageExportEula.Eulas)
			{
				temp.Add(new Tuple(first ? Messages.FINISH_PAGE_REVIEW_EULA : "", eula));
				first = false;
			}

			temp.Add(new Tuple(Messages.FINISH_PAGE_CREATE_MANIFEST, m_pageExportOptions.CreateManifest.ToYesNoStringI18n()));
			temp.Add(new Tuple(Messages.FINISH_PAGE_DIGITAL_SIGNATURE, m_pageExportOptions.SignAppliance.ToYesNoStringI18n()));

			temp.Add(new Tuple(Messages.FINISH_PAGE_CREATE_OVA, m_pageExportOptions.CreateOVA.ToYesNoStringI18n()));
			temp.Add(new Tuple(Messages.FINISH_PAGE_COMPRESS, m_pageExportOptions.CompressOVFfiles.ToYesNoStringI18n()));

			return temp;
		}

		private IEnumerable<Tuple> GetSummaryXva()
		{
			var temp = new List<Tuple>();
			temp.Add(new Tuple(Messages.FINISH_PAGE_REVIEW_APPLIANCE, m_pageExportAppliance.ApplianceFileName));
			temp.Add(new Tuple(Messages.FINISH_PAGE_REVIEW_DESTINATION, m_pageExportAppliance.ApplianceDirectory));
			temp.Add(new Tuple(Messages.FINISH_PAGE_REVIEW_VMS, string.Join("\n", m_pageExportSelectVMs.VMsToExport.Select(vm => vm.Name()).ToArray())));
			return temp;
        }
    }
}
