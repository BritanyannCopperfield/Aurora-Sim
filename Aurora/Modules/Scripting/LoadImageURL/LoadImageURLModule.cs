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

using Aurora.Framework.ConsoleFramework;
using Aurora.Framework.Modules;
using Aurora.Framework.SceneInfo;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace Aurora.Modules.Scripting
{
    public class LoadImageURLModule : INonSharedRegionModule, IDynamicTextureRender
    {
        private string m_name = "LoadImageURL";
        private string m_proxyexcepts = "";
        private string m_proxyurl = "";
        private IScene m_scene;
        private IDynamicTextureManager m_textureManager;

        #region IDynamicTextureRender Members

        public string GetName()
        {
            return m_name;
        }

        public string GetContentType()
        {
            return ("image");
        }

        public bool SupportsAsynchronous()
        {
            return true;
        }

        public byte[] ConvertUrl(string url, string extraParams)
        {
            return null;
        }

        public byte[] ConvertStream(Stream data, string extraParams)
        {
            return null;
        }

        public bool AsyncConvertUrl(UUID id, string url, string extraParams)
        {
            MakeHttpRequest(url, id);
            return true;
        }

        public bool AsyncConvertData(UUID id, string bodyData, string extraParams)
        {
            return false;
        }

        public void GetDrawStringSize(string text, string fontName, int fontSize,
                                      out double xSize, out double ySize)
        {
            xSize = 0;
            ySize = 0;
        }

        #endregion

        #region INonSharedRegionModule Members

        public void Initialise(IConfigSource config)
        {
            m_proxyurl = config.Configs["Startup"].GetString("HttpProxy");
            m_proxyexcepts = config.Configs["Startup"].GetString("HttpProxyExceptions");
        }

        public void AddRegion(IScene scene)
        {
            m_scene = scene;
        }

        public void RemoveRegion(IScene scene)
        {
        }

        public void RegionLoaded(IScene scene)
        {
            m_textureManager = m_scene.RequestModuleInterface<IDynamicTextureManager>();
            if (m_textureManager != null)
                m_textureManager.RegisterRender(GetContentType(), this);
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public void Close()
        {
        }

        public string Name
        {
            get { return m_name; }
        }

        #endregion

        private void MakeHttpRequest(string url, UUID requestID)
        {
            WebRequest request = WebRequest.Create(url);

            if (!string.IsNullOrEmpty(m_proxyurl))
            {
                if (!string.IsNullOrEmpty(m_proxyexcepts))
                {
                    string[] elist = m_proxyexcepts.Split(';');
                    request.Proxy = new WebProxy(m_proxyurl, true, elist);
                }
                else
                {
                    request.Proxy = new WebProxy(m_proxyurl, true);
                }
            }

            RequestState state = new RequestState((HttpWebRequest) request, requestID);
            request.BeginGetResponse(HttpRequestReturn, state);

            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            state.TimeOfRequest = (int) t.TotalSeconds;
        }

        private void HttpRequestReturn(IAsyncResult result)
        {
            RequestState state = (RequestState) result.AsyncState;
            WebRequest request = state.Request;
            Stream stream = null;
            byte[] imageJ2000 = new byte[0];

            try
            {
                HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(result);
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        Bitmap image = new Bitmap(stream);
                        Size newsize;

                        // TODO: make this a bit less hard coded
                        if ((image.Height < 64) && (image.Width < 64))
                        {
                            newsize = new Size(32, 32);
                        }
                        else if ((image.Height < 128) && (image.Width < 128))
                        {
                            newsize = new Size(64, 64);
                        }
                        else if ((image.Height < 256) && (image.Width < 256))
                        {
                            newsize = new Size(128, 128);
                        }
                        else if ((image.Height < 512 && image.Width < 512))
                        {
                            newsize = new Size(256, 256);
                        }
                        else if ((image.Height < 1024 && image.Width < 1024))
                        {
                            newsize = new Size(512, 512);
                        }
                        else
                        {
                            newsize = new Size(1024, 1024);
                        }

                        Bitmap resize = new Bitmap(image, newsize);

                        try
                        {
                            imageJ2000 = OpenJPEG.EncodeFromImage(resize, true);
                        }
                        catch (Exception)
                        {
                            MainConsole.Instance.Error(
                                "[LOADIMAGEURLMODULE]: OpenJpeg Encode Failed.  Empty byte data returned!");
                        }
                    }
                    else
                    {
                        MainConsole.Instance.WarnFormat("[LOADIMAGEURLMODULE] No data returned");
                    }
                }
            }
            catch (WebException)
            {
            }
            catch (ArgumentException)
            {
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            MainConsole.Instance.DebugFormat("[LOADIMAGEURLMODULE] Returning {0} bytes of image data for request {1}",
                                             imageJ2000.Length, state.RequestID);
            m_textureManager.ReturnData(state.RequestID, imageJ2000);
        }

        #region Nested type: RequestState

        public class RequestState
        {
            public HttpWebRequest Request;
            public UUID RequestID = UUID.Zero;
            public int TimeOfRequest;

            public RequestState(HttpWebRequest request, UUID requestID)
            {
                Request = request;
                RequestID = requestID;
            }
        }

        #endregion
    }
}