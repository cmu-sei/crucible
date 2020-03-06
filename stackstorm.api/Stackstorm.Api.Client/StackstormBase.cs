/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.Extensions.Configuration;
using NLog;

namespace Stackstorm.Api.Client
{
    public class StackstormBase
    {
        private string _username;
        private string _password;
        private string _baseUrl;
        public St2Client Client { get; private set; }
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public StackstormBase()
        {
            var config = new ConfigurationBuilder().AddJsonFile("settings.json").Build();
            _baseUrl = config["Url"].Trim('/');
            _username = config["Username"];
            _password = config["Password"];
            Init();
        }

        public StackstormBase(string url, string username, string password)
        {
            _baseUrl = url;
            _username = username;
            _password = password;
            Init();
        }

        private async void Init()
        {
            var authUrl = $"{_baseUrl}/auth/v1";
            var apiUrl = $"{_baseUrl}/api";

            Client = new St2Client(authUrl, apiUrl, _username, _password, true);
            await Client.RefreshTokenAsync();
        }
    }
}
