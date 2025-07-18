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

using System.Threading;
using XenAdmin.Core;
using XenAdmin.Network;
using XenAPI;


namespace XenAdmin.Actions.OvfActions
{
	public abstract class ApplianceAction : AsyncAction
	{
        private const int SLEEP_TIME = 900;
        private const int MAX_ITERATIONS = 60 * 60 * 24 / SLEEP_TIME * 1000; //iterations in 24h

		/// <summary>
		/// RBAC dependencies needed to import appliance/export an appliance/import disk image.
		/// </summary>
		public static RbacMethodList StaticRBACDependencies = new RbacMethodList("VM.add_to_other_config",
																				 "VM.create",
																				 "VM.destroy",
																				 "VM.hard_shutdown",
																				 "VM.remove_from_other_config",
																				 "VM.set_HVM_boot_params",
																				 "VM.start",
																				 "VDI.create",
																				 "VDI.destroy",
																				 "VBD.create",
																				 "VBD.eject",
																				 "VIF.create",
																				 "Host.call_plugin");

		protected ApplianceAction(IXenConnection connection, string title)
			: base(connection, title)
		{
			Pool pool = Helpers.GetPool(connection);
			if (pool != null)
				Pool = pool;
			else
				Host = Helpers.GetCoordinator(connection);
		}

        public bool MetaDataOnly { protected get; set; }

        protected abstract void RunCore();

	    protected sealed override void Run()
	    {
	        SafeToExit = false; 
            InitialiseTicker();
            RunCore();
		}

		public override void RecomputeCanCancel()
		{
			CanCancel = true;
		}

		protected override void CancelRelatedTask()
		{
			Description = Messages.CANCELING;
		}

        protected void CheckForCancellation()
        {
            if (Cancelling)
                throw new CancelledException();
        }

	    private void InitialiseTicker()
	    {
	        System.Threading.Tasks.Task.Run(TickUntilCompletion);
	    }

	    private void TickUntilCompletion()
	    {
	        int i = 0;
	        while (!IsCompleted && ++i<MAX_ITERATIONS)
	        {
	            OnChanged();
                Thread.Sleep(SLEEP_TIME);
	        }
	    }
    }
}
