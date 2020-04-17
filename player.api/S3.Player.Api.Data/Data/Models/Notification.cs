/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace S3.Player.Api.Data.Data.Models
{
    public class NotificationEntity
    {
        [Key]
        public int Key { get; set; }
        public Guid? ExerciseId { get; set; }
        public string FromName { get; set; }
        public Guid FromId { get; set; }
        public NotificationType FromType { get; set; }
        public string ToName { get; set; }
        public Guid ToId { get; set; }
        public NotificationType ToType { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public DateTime BroadcastTime { get; set; }
        public NotificationPriority Priority { get; set; }
    }

    public enum NotificationPriority
    {
        Normal = 0,
        Elevated = 1,
        High = 2,
        System = 3
    }
    
    public enum NotificationType
    {
        Exercise = 0,
        Team = 1,
        User = 2,
        Application = 3,
        Admin = 4
    }
}
