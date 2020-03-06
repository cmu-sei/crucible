/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using S3.VM.Api.Data.Models;
using System;

namespace S3.VM.Api.Data.Contexts
{
    public class Seed
    {
        public static void Run(Context ctx)
        {
            // VMs
            ctx.Vms.Add(new VmEntity
            {
                Url = "http://localhost:4305/vm/42313a85-e9ee-7550-28b8-15ead52ea729/console",
                Id = new Guid("42313a85-e9ee-7550-28b8-15ead52ea729"),
                Name = "S3-VM1"                
            });
            ctx.Vms.Add(new VmEntity
            {
                Url = "http://localhost:4305/vm/4231ef58-22c9-6c5c-7c3d-d3c32c73d4f7/console",
                Id = new Guid("4231ef58-22c9-6c5c-7c3d-d3c32c73d4f7"),
                Name = "S3-VM2"
            });
            ctx.Vms.Add(new VmEntity
            {
                Url = "http://localhost:4305/vm/42313053-c2e6-42af-cf2a-6db9f791794a/console",
                Id = new Guid("42313053-c2e6-42af-cf2a-6db9f791794a"),
                Name = "S3-VM3",
                AllowedNetworks ="ops-net win10usr-1"
            });

            // Teams
            ctx.Teams.Add(
                new TeamEntity
                {
                    Id = new Guid("df7b7157-1727-48b5-803d-cfdb208767c0") // blue                    
                }
            );
            ctx.Teams.Add(
                new TeamEntity
                {
                    Id = new Guid("66925bea-68fd-40dd-9b19-d3c1fb5fa1bf") // white
                }
            );
            ctx.Teams.Add(
                new TeamEntity
                {
                    Id = new Guid("51484d68-8ad9-487e-98d6-30db812fa355") // red
                }
            );

            // Vm and Team association

            // VM1 on Red and White
            ctx.VmTeams.Add(
                new VmTeamEntity
                {
                    VmId = new Guid("42313a85-e9ee-7550-28b8-15ead52ea729"),
                    TeamId = new Guid("51484d68-8ad9-487e-98d6-30db812fa355")
                }
            );
            ctx.VmTeams.Add(
                new VmTeamEntity
                {
                    VmId = new Guid("42313a85-e9ee-7550-28b8-15ead52ea729"),
                    TeamId = new Guid("66925bea-68fd-40dd-9b19-d3c1fb5fa1bf")
                }
            );

            // VM2 on Blue and White
            ctx.VmTeams.Add(
                new VmTeamEntity
                {
                    VmId = new Guid("4231ef58-22c9-6c5c-7c3d-d3c32c73d4f7"),
                    TeamId = new Guid("df7b7157-1727-48b5-803d-cfdb208767c0")
                }
            );
            ctx.VmTeams.Add(
                new VmTeamEntity
                {
                    VmId = new Guid("4231ef58-22c9-6c5c-7c3d-d3c32c73d4f7"),
                    TeamId = new Guid("66925bea-68fd-40dd-9b19-d3c1fb5fa1bf")
                }
            );

            // VM3 on Blue and White
            ctx.VmTeams.Add(
                new VmTeamEntity
                {
                    VmId = new Guid("42313053-c2e6-42af-cf2a-6db9f791794a"),
                    TeamId = new Guid("df7b7157-1727-48b5-803d-cfdb208767c0")
                }
            );
            ctx.VmTeams.Add(
                new VmTeamEntity
                {
                    VmId = new Guid("42313053-c2e6-42af-cf2a-6db9f791794a"),
                    TeamId = new Guid("66925bea-68fd-40dd-9b19-d3c1fb5fa1bf")
                }
            );
            ctx.SaveChangesAsync().Wait();
        }
    }
}

