﻿/*
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

using Aurora.Framework.Servers.HttpServer;
using System.Collections.Generic;
using Aurora.Framework.Servers.HttpServer.Implementation;
using Aurora.Framework.Services;

namespace Aurora.Modules.Web
{
    public class UserSearchPage : IWebInterfacePage
    {
        public string[] FilePath
        {
            get
            {
                return new[]
                           {
                               "html/user_search.html"
                           };
            }
        }

        public bool RequiresAuthentication
        {
            get { return false; }
        }

        public bool RequiresAdminAuthentication
        {
            get { return false; }
        }

        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, OSHttpRequest httpRequest,
                                               OSHttpResponse httpResponse, Dictionary<string, object> requestParameters,
                                               ITranslator translator, out string response)
        {
            response = null;
            var vars = new Dictionary<string, object>();
            var usersList = new List<Dictionary<string, object>>();

            uint amountPerQuery = 10;

            if (requestParameters.ContainsKey("Submit"))
            {
                IUserAccountService accountService = webInterface.Registry.RequestModuleInterface<IUserAccountService>();
                string username = requestParameters["username"].ToString();
                int start = httpRequest.Query.ContainsKey("Start")
                                ? int.Parse(httpRequest.Query["Start"].ToString())
                                : 0;
                uint count = accountService.NumberOfUserAccounts(null, username);
                int maxPages = (int) (count/amountPerQuery) - 1;

                if (start == -1)
                    start = (int) (maxPages < 0 ? 0 : maxPages);

                vars.Add("CurrentPage", start);
                vars.Add("NextOne", start + 1 > maxPages ? start : start + 1);
                vars.Add("BackOne", start - 1 < 0 ? 0 : start - 1);

                var users = accountService.GetUserAccounts(null, username, (uint) start, amountPerQuery);
                foreach (var user in users)
                {
                    usersList.Add(new Dictionary<string, object>
                                      {
                                          {"UserName", user.Name},
                                          {"UserID", user.PrincipalID},
                                          {"CanEdit", Authenticator.CheckAdminAuthentication(httpRequest)}
                                      });
                }
            }
            else
            {
                vars.Add("CurrentPage", 0);
                vars.Add("NextOne", 0);
                vars.Add("BackOne", 0);
            }

            vars.Add("UsersList", usersList);
            vars.Add("UserSearchText", translator.GetTranslatedString("UserSearchText"));
            vars.Add("SearchForUserText", translator.GetTranslatedString("SearchForUserText"));
            vars.Add("UserNameText", translator.GetTranslatedString("UserNameText"));
            vars.Add("Search", translator.GetTranslatedString("Search"));
            vars.Add("SearchResultForUserText", translator.GetTranslatedString("SearchResultForUserText"));
            vars.Add("EditText", translator.GetTranslatedString("EditText"));
            vars.Add("EditUserAccountText", translator.GetTranslatedString("EditUserAccountText"));

            vars.Add("FirstText", translator.GetTranslatedString("FirstText"));
            vars.Add("BackText", translator.GetTranslatedString("BackText"));
            vars.Add("NextText", translator.GetTranslatedString("NextText"));
            vars.Add("LastText", translator.GetTranslatedString("LastText"));
            vars.Add("CurrentPageText", translator.GetTranslatedString("CurrentPageText"));

            return vars;
        }

        public bool AttemptFindPage(string filename, ref OSHttpResponse httpResponse, out string text)
        {
            text = "";
            return false;
        }
    }
}