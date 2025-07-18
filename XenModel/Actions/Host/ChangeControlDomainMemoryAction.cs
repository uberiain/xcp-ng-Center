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

using XenAPI;


namespace XenAdmin.Actions
{
    public class ChangeControlDomainMemoryAction : AsyncAction
    {
        long memory;

        public ChangeControlDomainMemoryAction(Host host, long memory, bool suppressHistory)
            : base(host.Connection, string.Format(Messages.ACTION_CHANGE_CONTROL_DOMAIN_MEMORY, host.Name()), suppressHistory)
        {
            Host = host;
            this.memory = memory;

            #region RBAC Dependencies
 
            ApiMethodsToRoleCheck.Add("vm.set_memory");

            #endregion
        }

        protected override void Run()
        {
            VM vm = Host.ControlDomainZero();

            try
            {
                XenAPI.VM.set_memory(Session, vm.opaque_ref, memory);
            }
            catch (Failure f)
            {
                if (f.ErrorDescription[0] == Failure.MEMORY_CONSTRAINT_VIOLATION
                    && memory < vm.memory_static_min)
                {
                    throw new Failure(string.Format(Messages.ACTION_CHANGE_CONTROL_DOMAIN_MEMORY_VALUE_TOO_LOW,
                        Util.MemorySizeStringSuitableUnits(memory, true),
                        Util.MemorySizeStringSuitableUnits(vm.memory_static_min, true)));
                }

                throw;
            }

            Description = Messages.COMPLETED;
        }
    }
}
