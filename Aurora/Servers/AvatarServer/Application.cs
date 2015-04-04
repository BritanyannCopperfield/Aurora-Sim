/*
 * Copyright (c) Contributors, http://aurora-sim.org/, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using Aurora.Framework.Services;
using Aurora.Simulation.Base;
using Aurora.Framework.Modules;
using System.Reflection;

[assembly: AssemblyVersion("0.8.1")]
[assembly: AssemblyFileVersion("0.8.1")]

namespace Aurora.Servers.AvatarServer
{
    /// <summary>
    ///     Starting class for the Aurora Server
    /// </summary>
    public class Application
    {
        public static void Main(string[] args)
        {
            BaseApplication.BaseMain(args, "Aurora.AvatarServer.ini",
                                     new MinimalSimulationBase("Aurora.AvatarServer ",
                                                               new List<Type>
                                                                   {
                                                                       typeof (IAvatarData),
                                                                       typeof (IInventoryData),
                                                                       typeof (IUserAccountData),
                                                                       typeof (IAssetDataPlugin)
                                                                   },
                                                               new List<Type>
                                                                   {
                                                                       typeof (IAvatarService),
                                                                       typeof (IInventoryService),
                                                                       typeof (IUserAccountService),
                                                                       typeof (IAssetService),
                                                                       typeof (ISyncMessagePosterService),
                                                                       typeof (ISyncMessageRecievedService),
                                                                       typeof (IExternalCapsHandler),
                                                                       typeof (IConfigurationService),
                                                                       typeof (IGridServerInfoService),
                                                                       typeof (IAgentAppearanceService),
                                                                       typeof (IJ2KDecoder)
                                                                   }));
        }
    }
}