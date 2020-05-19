/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using Foreman.Core;
using Foreman.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Foreman.Api.ViewModels
{
    public class WebHookViewModel
    {
        public string Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusType Status { get; set; }
        public string Description { get; set; }
        public string PostbackUrl { get; set; }
        public WebHook.WebhookMethod PostbackMethod { get; set; }
        public bool MustAuthenticate { get; set; }
        public object PostbackFormat { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string ApplicationUserId { get; set; }

        public WebHookViewModel()
        {
            this.CreatedUtc = DateTime.UtcNow;
            this.Status = StatusType.Active;
            this.PostbackMethod = WebHook.WebhookMethod.GET;
            this.MustAuthenticate = false;
        }
        
        public WebHook ToWebHook()
        {
            var hook = new WebHook();
            
            var id = Guid.NewGuid();
            if(Guid.TryParse(this.Id, out id))
                hook.Id = id;
            hook.Status = this.Status;
            hook.Description = this.Description;
            hook.PostbackUrl = this.PostbackUrl;
            hook.PostbackMethod = this.PostbackMethod;
            hook.MustAuthenticate = this.MustAuthenticate;
            //hook.PostbackFormat = this.Payload;
            hook.CreatedUtc = this.CreatedUtc;
            return hook;
        }
    }
}
