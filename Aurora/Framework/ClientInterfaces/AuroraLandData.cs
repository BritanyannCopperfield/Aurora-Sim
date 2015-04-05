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

using Aurora.Framework.Modules;
using Aurora.Framework.SceneInfo;
using OpenMetaverse.StructuredData;

namespace Aurora.Framework.ClientInterfaces
{
    public class ExtendedLandData : IDataTransferable
    {
        public float GlobalPosX;
        public float GlobalPosY;
        public LandData LandData;
        public string RegionName;
        public string RegionType;
        public string RegionTerrain;
        public uint RegionArea;

        public override void FromOSD(OpenMetaverse.StructuredData.OSDMap map)
        {
            GlobalPosX = map["GlobalPosX"];
            GlobalPosY = map["GlobalPosY"];
            LandData = new LandData();
            LandData.FromOSD((OSDMap)map["LandData"]);
            RegionName = map["RegionName"];
            RegionType = map["RegionType"];
            RegionTerrain = map["RegionTerrain"];
            RegionArea = map["RegionArea"];
        }

        public override OSDMap ToOSD()
        {
            OSDMap map = new OSDMap();
            map["GlobalPosX"] = GlobalPosX;
            map["GlobalPosY"] = GlobalPosY;
            map["LandData"] = LandData.ToOSD();
            map["RegionName"] = RegionName;
            map["RegionType"] = RegionType;
            map["RegionTerrain"] = RegionTerrain;
            map["RegionArea"] = RegionArea;
            return map;
        }
    }
}