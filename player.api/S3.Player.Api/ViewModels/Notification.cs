/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using S3.Player.Api.Data.Data.Models;
using System;

namespace S3.Player.Api.ViewModels
{
    public class Notification
    {
        public Guid FromId { get; set; }
        public NotificationType FromType { get; set; }
        public Guid ToId { get; set; }
        public NotificationType ToType { get; set; }
        public DateTime BroadcastTime { get; set; }
        public string ToName { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public NotificationPriority Priority { get; set; }
        public bool WasSuccess { get; set; }
        public bool CanPost { get; set; }
        public string IconUrl { get; set; }
        
        public bool IsValid()
        {
            var isValid = false;
            if (Text != null && Text.Length > 0)
            {
                isValid = true;
            }
            return isValid;
        }
    }

}
