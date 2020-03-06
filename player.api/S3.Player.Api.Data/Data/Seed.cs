/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Linq;
using System.Text;
using S3.Player.Api.Data.Data.Models;

namespace S3.Player.Api.Data.Data
{
    public class Seed
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Run(PlayerContext context)
        {
            // Permissions
            var systemAdminPermission = context.Permissions.Where(x => x.Key == "SystemAdmin").FirstOrDefault();
            var exerciseAdminPermission = context.Permissions.Where(x => x.Key == "ExerciseAdmin").FirstOrDefault();            
            var ostAdminPermission = new PermissionEntity { Id = Guid.NewGuid(), Key = "OsTicketAdmin", Description = "Admin in OsTicket" };
            var ostAgentPermission = new PermissionEntity { Id = Guid.NewGuid(), Key = "OsTicketAgent", Description = "Agent in OsTicket" };
            var viewAllVmsPermission = new PermissionEntity { Id = Guid.NewGuid(), Key = "ViewAllMachines", Description = "View all Virtual Machines" };
            
            context.Permissions.Add(ostAdminPermission);
            context.Permissions.Add(ostAgentPermission);
            context.Permissions.Add(viewAllVmsPermission);            

            // Roles
            var superUserRole = new RoleEntity { Id = Guid.Parse("f16d7689-4c22-498f-b975-021348b19120"), Name = "Super User" };
            superUserRole.Permissions.Add(new RolePermissionEntity { Id = Guid.NewGuid(), Permission = systemAdminPermission });

            var exerciseAdminRole = new RoleEntity { Id = Guid.Parse("b8f2c55b-f47d-4ec9-8fce-606753c4af72"), Name = "Exercise Administrator" };
            exerciseAdminRole.Permissions.Add(new RolePermissionEntity { Id = Guid.NewGuid(), Permission = exerciseAdminPermission });
            exerciseAdminRole.Permissions.Add(new RolePermissionEntity { Id = Guid.NewGuid(), Permission = ostAdminPermission });
            exerciseAdminRole.Permissions.Add(new RolePermissionEntity { Id = Guid.NewGuid(), Permission = ostAgentPermission });
            exerciseAdminRole.Permissions.Add(new RolePermissionEntity { Id = Guid.NewGuid(), Permission = viewAllVmsPermission });

            context.Roles.Add(superUserRole);
            context.Roles.Add(exerciseAdminRole);            

            // sketch users
            var uEnder = new UserEntity { Id = Guid.Parse("3269cb19-1d39-40d3-a55e-e3e9779b6e0b"), Name = "Ender" };
            var uBean = new UserEntity { Id = Guid.Parse("ac4d3e32-c2d6-4f99-9aef-0fcd62a568a6"), Name = "Bean" };
            var uGraff = new UserEntity { Id = Guid.Parse("b7977ce5-0a17-45e1-aa2e-55c57bfffeb6"), Name = "Graff"};
            var uBonzo = new UserEntity { Id = Guid.Parse("1db2856b-7a3c-4b82-95d4-e41fb18de516"), Name = "Bonzo" };
            var uBob = new UserEntity { Id = Guid.Parse("9149f2ec-2e55-44f6-b92d-988ede6ca1f9"), Name = "Bob" };
            var uAdministrator = new UserEntity { Id = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), Name = "admin user", Role = superUserRole };

            context.Users.Add(uEnder);
            context.Users.Add(uBean);
            context.Users.Add(uGraff);
            context.Users.Add(uBonzo);
            context.Users.Add(uBob);
            context.Users.Add(uAdministrator);

            // exercise 1
            var exercise1 = new ExerciseEntity
            {
                Id = Guid.Parse("453d394e-bf18-499b-9786-149b0f8d69ec"),
                Name = "RCC -E EM 2018",
                Description = "Cyber exercises for evaluating the team.",
                Status = ExerciseStatus.Active
            };

            var tBlue = new TeamEntity { Id = Guid.Parse("df7b7157-1727-48b5-803d-cfdb208767c0"), Name = "Blue" };
            tBlue.Permissions.Add(new TeamPermissionEntity(tBlue.Id, ostAdminPermission.Id));

            var tAdmin = new TeamEntity { Id = Guid.Parse("453e0508-0515-402d-85e9-24e567096f7a"), Name = "Admin", Role = exerciseAdminRole };                        

            //exercise1.ExerciseUsers.Add(new ExerciseUserEntity { ExerciseId = exercise1.Id, UserId = uEnder.Id, PrimaryTeamId = tBlue.Id });
            //exercise1.ExerciseUsers.Add(new ExerciseUserEntity { ExerciseId = exercise1.Id, UserId = uBean.Id, PrimaryTeamId = tRed.Id });
            //exercise1.ExerciseUsers.Add(new ExerciseUserEntity { ExerciseId = exercise1.Id, UserId = uGraff.Id, PrimaryTeamId = tWhite.Id });
            //exercise1.ExerciseUsers.Add(new ExerciseUserEntity { ExerciseId = exercise1.Id, UserId = uBonzo.Id, PrimaryTeamId = tSupport.Id });
            //exercise1.ExerciseUsers.Add(new ExerciseUserEntity { ExerciseId = exercise1.Id, UserId = uBob.Id, PrimaryTeamId = tBlue.Id });
            //exercise1.ExerciseUsers.Add(new ExerciseUserEntity { ExerciseId = exercise1.Id, UserId = uAdministrator.Id, PrimaryTeamId = tAdmin.Id });



            //tBlue.Memberships.Add(new TeamMembershipEntity { Team = tBlue, User = uAdministrator,  });

            //tBlue.TeamUsers.Add(new TeamUserEntity { TeamId = tBlue.Id, UserId = uEnder.Id });
            //tBlue.TeamUsers.Add(new TeamUserEntity { TeamId = tBlue.Id, UserId = uBob.Id });
            //tBlue.TeamUsers.Add(new TeamUserEntity { TeamId = tBlue.Id, UserId = uBonzo.Id });

            var tRed = new TeamEntity { Id = Guid.Parse("51484d68-8ad9-487e-98d6-30db812fa355"), Name = "Red" };
            //tRed.TeamUsers.Add(new TeamUserEntity { TeamId = tRed.Id, UserId = uBean.Id });

            var tWhite = new TeamEntity { Id = Guid.Parse("66925bea-68fd-40dd-9b19-d3c1fb5fa1bf"), Name = "White" };
            //tWhite.TeamUsers.Add(new TeamUserEntity { TeamId = tWhite.Id, UserId = uGraff.Id });

            var tSupport = new TeamEntity { Id = Guid.Parse("b7ca71d3-330c-4ae4-aab5-21fdcf8ee775"), Name = "Support" };
            //tSupport.TeamUsers.Add(new TeamUserEntity { TeamId = tSupport.Id, UserId = uBonzo.Id });
            //tSupport.TeamUsers.Add(new TeamUserEntity { TeamId = tSupport.Id, UserId = uAdministrator.Id });

            
            //tAdmin.TeamUsers.Add(new TeamUserEntity { TeamId = tAdmin.Id, UserId = uAdministrator.Id });

            exercise1.Teams.Add(tBlue);
            exercise1.Teams.Add(tRed);
            exercise1.Teams.Add(tWhite);
            exercise1.Teams.Add(tSupport);
            exercise1.Teams.Add(tAdmin);

            //// exercise 2
            //var exercise2 = new ExerciseEntity
            //{
            //    Id = Guid.Parse("fc41c788-063b-4018-9f28-5f68a52f4e76"),
            //    Name = "Exercise 2",
            //    Description = "Another Exercise",
            //    Status = ExerciseStatus.Active
            //};

            var a = new ApplicationEntity
            {
                Name = "Virtual Machines",
                Url = "http://localhost:4303/exercises/{exerciseId}",
                Embeddable = true,
            };
            a.Icon = "/assets/img/SP_Icon_Virtual.png";

            var b = new ApplicationEntity
            {
                Name = "Intel Doc",
                Url = "https://www3.epa.gov/ttn/naaqs/standards/co/data/2009_04_COScopeandMethodsPlan.pdf",
                Embeddable = true,
            };
            b.Icon = "/assets/img/SP_Icon_Intel.png";

            var c = new ApplicationEntity
            {
                Name = "Chat",
                Url = "https://INTERNAL_CHAT_SERVER?geid={exerciseId}",
                Embeddable = false,
            };
            c.Icon = "/assets/img/SP_Icon_Chat.png";


            var d = new ApplicationEntity
            {
                Name = "Exercise Hub",
                Url = "https://hub.com?geid={exerciseId}",
                Embeddable = true,
            };

            d.Icon = "/assets/img/SP_Icon_Hub.png";

            var e = new ApplicationEntity
            {
                Name = "Help Desk",
                Url = "http://localhost/osticket/login.php?do=ext&bk=identity.client",
                Embeddable = true,
                LoadInBackground = true
            };
            e.Icon = "/assets/img/SP_Icon_Help.png";

            exercise1.Applications.Add(a);
            exercise1.Applications.Add(b);
            exercise1.Applications.Add(c);
            exercise1.Applications.Add(d);
            exercise1.Applications.Add(e);

            ApplicationInstanceEntity blueVmAppInstance = new ApplicationInstanceEntity { Application = a, DisplayOrder = 0 };
            ApplicationInstanceEntity blueIntelAppInstance = new ApplicationInstanceEntity { Application = b, DisplayOrder = 1 };
            ApplicationInstanceEntity blueChatAppInstance = new ApplicationInstanceEntity { Application = c, DisplayOrder = 2 };
            ApplicationInstanceEntity blueHubAppInstance = new ApplicationInstanceEntity { Application = d, DisplayOrder = 3 };
            ApplicationInstanceEntity blueHelpdeskAppInstance = new ApplicationInstanceEntity { Application = e, DisplayOrder = 4 };

            ApplicationInstanceEntity redVmAppInstance = new ApplicationInstanceEntity { Application = a, DisplayOrder = 0 };
            ApplicationInstanceEntity redHelpdeskAppInstance = new ApplicationInstanceEntity { Application = e, DisplayOrder = 1 };

            ApplicationInstanceEntity whiteVmAppInstance = new ApplicationInstanceEntity { Application = a, DisplayOrder = 0 };
            ApplicationInstanceEntity whiteHelpdeskAppInstance = new ApplicationInstanceEntity { Application = e, DisplayOrder = 1 };

            ApplicationInstanceEntity supportVmAppInstance = new ApplicationInstanceEntity { Application = a, DisplayOrder = 0 };
            ApplicationInstanceEntity supportHelpdeskAppInstance = new ApplicationInstanceEntity { Application = e, DisplayOrder = 1 };

            ApplicationInstanceEntity adminVmAppInstance = new ApplicationInstanceEntity { Application = a, DisplayOrder = 0 };
            ApplicationInstanceEntity adminHelpdeskAppInstance = new ApplicationInstanceEntity { Application = e, DisplayOrder = 1 };

            tBlue.Applications.Add(blueVmAppInstance);
            tBlue.Applications.Add(blueIntelAppInstance);
            tBlue.Applications.Add(blueChatAppInstance);
            tBlue.Applications.Add(blueHubAppInstance);
            tBlue.Applications.Add(blueHelpdeskAppInstance);

            tRed.Applications.Add(redVmAppInstance);
            tRed.Applications.Add(redHelpdeskAppInstance);

            tWhite.Applications.Add(whiteVmAppInstance);
            tWhite.Applications.Add(whiteHelpdeskAppInstance);

            tSupport.Applications.Add(supportVmAppInstance);
            tSupport.Applications.Add(supportHelpdeskAppInstance);

            tAdmin.Applications.Add(adminVmAppInstance);
            tAdmin.Applications.Add(adminHelpdeskAppInstance);

            context.Exercises.Add(exercise1);
            //context.Exercises.Add(exercise2);

            var exMembership1 = new ExerciseMembershipEntity { Exercise = exercise1, UserId = uEnder.Id };
            exercise1.Memberships.Add(exMembership1);
            context.SaveChanges();

            var enderBlueMembership = new TeamMembershipEntity { Team = tBlue, User = uEnder, ExerciseMembership = exMembership1, Role = exerciseAdminRole };
            exMembership1.PrimaryTeamMembership = enderBlueMembership;

            context.SaveChanges();

            log.Debug("Seed completed");            
        }
    }
}

