/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using S3.Player.Api.Data.Data;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Extensions;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace S3.Player.Api.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<ViewModels.Notification>> GetAsync(CancellationToken ct);
        Task<IEnumerable<ViewModels.Notification>> GetByExerciseAsync(Guid exerciseId, CancellationToken ct);
        Task<IEnumerable<ViewModels.Notification>> GetByTeamAsync(Guid teamId, CancellationToken ct);
        Task<IEnumerable<ViewModels.Notification>> GetByUserAsync(Guid exerciseId, Guid userId, CancellationToken ct);
        Task<ViewModels.Notification> JoinExercise(Guid id, CancellationToken ct);
        Task<ViewModels.Notification> PostToExercise(Guid id, ViewModels.Notification incomingData, CancellationToken ct);
        Task<ViewModels.Notification> JoinTeam(Guid teamId, CancellationToken ct);
        Task<ViewModels.Notification> PostToTeam(Guid teamId, ViewModels.Notification incomingData, CancellationToken ct);
        Task<ViewModels.Notification> JoinUser(Guid exerciseId, Guid userId, CancellationToken ct);
        Task<ViewModels.Notification> PostToUser(Guid exerciseId, Guid userId, ViewModels.Notification incomingData, CancellationToken ct);
    }

    public class NotificationService : INotificationService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private IUserClaimsService _claimsService;
        private IConfiguration _configuration;


        public NotificationService(PlayerContext context, IPrincipal user, IAuthorizationService authorizationService, IMapper mapper, IUserClaimsService claimsService, IConfiguration configuration)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _claimsService = claimsService;
            _configuration = configuration;
        }

        public async Task<IEnumerable<ViewModels.Notification>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Notifications.ToListAsync(ct);
            // Databases do not preserve DateTimeKind, so we need to add UTC kind
            items.ForEach(item => item.BroadcastTime = DateTime.SpecifyKind(item.BroadcastTime, DateTimeKind.Utc));
            return _mapper.Map<IEnumerable<ViewModels.Notification>>(items);
        }

        public async Task<IEnumerable<ViewModels.Notification>> GetByExerciseAsync(Guid exerciseId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ExerciseMemberRequirement(exerciseId))).Succeeded)
                throw new ForbiddenException();
            // get all notifications for the selected exercise, not including the System notifications
            var items = await _context.Notifications.Where(x => x.ToId == exerciseId && x.ToType == NotificationType.Exercise && x.Priority != NotificationPriority.System).OrderByDescending(y => y.BroadcastTime).ToListAsync();
            // Databases do not preserve DateTimeKind, so we need to add UTC kind
            items.ForEach(item => item.BroadcastTime = DateTime.SpecifyKind(item.BroadcastTime, DateTimeKind.Utc));
            return _mapper.Map<IEnumerable<ViewModels.Notification>>(items);
        }

        public async Task<IEnumerable<ViewModels.Notification>> GetByTeamAsync(Guid teamId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new TeamMemberRequirement(teamId))).Succeeded)
                throw new ForbiddenException();
            // get all notifications for the selected team, not including the System notifications
            var items = await _context.Notifications.Where(x => x.ToId == teamId && x.ToType == NotificationType.Team && x.Priority != NotificationPriority.System).OrderByDescending(y => y.BroadcastTime).ToListAsync();
            // Databases do not preserve DateTimeKind, so we need to add UTC kind
            items.ForEach(item => item.BroadcastTime = DateTime.SpecifyKind(item.BroadcastTime, DateTimeKind.Utc));
            return _mapper.Map<IEnumerable<ViewModels.Notification>>(items);
        }

        public async Task<IEnumerable<ViewModels.Notification>> GetByUserAsync(Guid exerciseId, Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ExerciseMemberRequirement(exerciseId))).Succeeded || !(await _authorizationService.AuthorizeAsync(_user, null, new SameUserRequirement(userId))).Succeeded)
                throw new ForbiddenException();
            // get all notifications for the selected exercise-user, not including the System notifications
            var items = await _context.Notifications.Where(x => x.ToId == userId && x.ToType == NotificationType.User && x.Priority != NotificationPriority.System).OrderByDescending(y => y.BroadcastTime).ToListAsync();
            // Databases do not preserve DateTimeKind, so we need to add UTC kind
            items.ForEach(item => item.BroadcastTime = DateTime.SpecifyKind(item.BroadcastTime, DateTimeKind.Utc));
            return _mapper.Map<IEnumerable<ViewModels.Notification>>(items);
        }

        /// <summary>
        /// Allows a client to join exercise notifications, IF:
        ///     1. they are a member of this exercise
        /// </summary>
        /// <param name="exerciseId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ViewModels.Notification> JoinExercise(Guid exerciseId, CancellationToken cancellationToken)
        {
            var wasSuccess = true;
            var canPost = false;
            var messageTime = DateTime.Now.ToUniversalTime();
            var notificationEntity = new NotificationEntity{};
            notificationEntity.Subject = "Join Exercise";
            notificationEntity.ToId = exerciseId;
            notificationEntity.ToType = NotificationType.Exercise;
            notificationEntity.BroadcastTime = messageTime;
            notificationEntity.FromId = _user.GetId();
            notificationEntity.FromType = NotificationType.User;
            notificationEntity.FromName = _user.Claims.Single(x => x.Type == "name").Value;
            notificationEntity.ToName = _context.Exercises.Find(exerciseId).Name;
            notificationEntity.Priority = NotificationPriority.System;
            if ((await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(exerciseId))).Succeeded)
            {
                notificationEntity.Text = String.Format("Successfully joined {0} notifications.", notificationEntity.ToName);
                canPost = true;
            }
            else if ((await _authorizationService.AuthorizeAsync(_user, null, new ExerciseMemberRequirement(exerciseId))).Succeeded)
            {
                notificationEntity.Text = String.Format("Successfully joined {0} notifications.", notificationEntity.ToName);
            }
            else
            {
                notificationEntity.Text = String.Format("Failed to join {0} notifications.", notificationEntity.ToName);
                wasSuccess = false;
            }
            var returnNotification =  _mapper.Map<ViewModels.Notification>(notificationEntity);
            returnNotification.WasSuccess = wasSuccess;
            returnNotification.CanPost = canPost;
            returnNotification.IconUrl = GetIconUrl(notificationEntity.Priority, notificationEntity.FromName);
            return returnNotification;
        }

        /// <summary>
        /// Allows a client to broadcast a notification to all members of the exercise who are currently joined, IF:
        ///     1. they are an exercise administrator
        /// </summary>
        /// <param name="exerciseId"></param>
        /// <param name="incomingData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ViewModels.Notification> PostToExercise(Guid exerciseId, ViewModels.Notification incomingData, CancellationToken cancellationToken)
        {
            var wasSuccess = true;
            var canPost = true;
            var messageTime = DateTime.Now.ToUniversalTime();
            NotificationEntity notificationEntity;
            if ((await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(exerciseId))).Succeeded)
            {
                notificationEntity = _mapper.Map<NotificationEntity>(incomingData);
                notificationEntity.ToId = exerciseId;
                notificationEntity.ToType = NotificationType.Exercise;
                notificationEntity.BroadcastTime = messageTime;
                notificationEntity.FromId = _user.GetId();
                notificationEntity.FromType = NotificationType.User;
                notificationEntity.FromName = _user.Claims.Single(x => x.Type == "name").Value;
                notificationEntity.ToName = _context.Exercises.Find(exerciseId).Name;
                _context.Notifications.Add(notificationEntity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                notificationEntity = new NotificationEntity{};
                wasSuccess = false;
                canPost = false;
            }
            var returnNotification =  _mapper.Map<ViewModels.Notification>(notificationEntity);
            returnNotification.WasSuccess = wasSuccess;
            returnNotification.CanPost = canPost;
            returnNotification.IconUrl = GetIconUrl(notificationEntity.Priority, notificationEntity.FromName);
            return returnNotification;
        }

        /// <summary>
        /// Allows a client to join team notifications, IF:
        ///     1. they are a member of this team OR
        ///     2. they are an exercise administrator
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ViewModels.Notification> JoinTeam(Guid teamId, CancellationToken cancellationToken)
        {
            var wasSuccess = true;
            var canPost = false;
            var messageTime = DateTime.Now.ToUniversalTime();
            var requestedTeam = _context.Teams.Find(teamId);
            var notificationEntity = new NotificationEntity{};
            notificationEntity.Subject = "Join Team";
            notificationEntity.ToId = teamId;
            notificationEntity.ToType = NotificationType.Exercise;
            notificationEntity.BroadcastTime = messageTime;
            notificationEntity.FromId = _user.GetId();
            notificationEntity.FromType = NotificationType.User;
            notificationEntity.FromName = _user.Claims.Single(x => x.Type == "name").Value;
            notificationEntity.ToName = requestedTeam.Name;
            notificationEntity.Priority = NotificationPriority.System;
            if ((await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(requestedTeam.ExerciseId))).Succeeded)
            {
                notificationEntity.Text = String.Format("Successfully joined {0} notifications.", notificationEntity.ToName);
                canPost = true;
            }
            else if ((await _authorizationService.AuthorizeAsync(_user, null, new TeamMemberRequirement(teamId))).Succeeded)
            {
                notificationEntity.Text = String.Format("Successfully joined {0} notifications.", notificationEntity.ToName);
            }
            else
            {
                notificationEntity.Text = String.Format("Failed to join {0} notifications.", notificationEntity.ToName);
                wasSuccess = false;
            }
            var returnNotification = _mapper.Map<ViewModels.Notification>(notificationEntity);
            returnNotification.WasSuccess = wasSuccess;
            returnNotification.CanPost = canPost;
            returnNotification.IconUrl = GetIconUrl(notificationEntity.Priority, notificationEntity.FromName);
            return returnNotification;
        }

        /// <summary>
        /// Allows a client to broadcast a notification to all members of the team who are currently joined, IF:
        ///     1. they are an exercise administrator
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="incomingData"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ViewModels.Notification> PostToTeam(Guid teamId, ViewModels.Notification incomingData, CancellationToken ct)
        {
            var wasSuccess = true;
            var canPost = true;
            var messageTime = DateTime.Now.ToUniversalTime();
            var requestedTeam = _context.Teams.Find(teamId);
            NotificationEntity notificationEntity;
            if ((await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(requestedTeam.ExerciseId))).Succeeded)
            {
                notificationEntity = _mapper.Map<NotificationEntity>(incomingData);
                notificationEntity.ToId = teamId;
                notificationEntity.ToType = NotificationType.Team;
                notificationEntity.BroadcastTime = messageTime;
                notificationEntity.FromId = _user.GetId();
                notificationEntity.FromType = NotificationType.User;
                notificationEntity.FromName = _user.Claims.Single(x => x.Type == "name").Value;
                notificationEntity.ToName = _context.Teams.Find(teamId).Name;
                _context.Notifications.Add(notificationEntity);
                await _context.SaveChangesAsync(ct);
            }
            else
            {
                notificationEntity = new NotificationEntity{};
                wasSuccess = false;
                canPost = false;
            }
            var returnNotification =  _mapper.Map<ViewModels.Notification>(notificationEntity);
            returnNotification.WasSuccess = wasSuccess;
            returnNotification.CanPost = canPost;
            returnNotification.IconUrl = GetIconUrl(notificationEntity.Priority, notificationEntity.FromName);
            return returnNotification;
        }

        /// <summary>
        /// Allows a client to join the user notifications, IF:
        ///     1. they are a member of the indicated exercise and they are the indicated user OR
        ///     2. they are an exercise administrator
        /// </summary>
        /// <param name="exerciseId"></param>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ViewModels.Notification> JoinUser(Guid exerciseId, Guid userId, CancellationToken ct)
        {
            var wasSuccess = true;
            var canPost = false;
            var messageTime = DateTime.Now.ToUniversalTime();
            var notificationEntity = new NotificationEntity{};
            notificationEntity.Subject = "Join Team";
            notificationEntity.ExerciseId = exerciseId;
            notificationEntity.ToId = userId;
            notificationEntity.ToType = NotificationType.Exercise;
            notificationEntity.BroadcastTime = messageTime;
            notificationEntity.FromId = _user.GetId();
            notificationEntity.FromType = NotificationType.User;
            notificationEntity.FromName = _user.Claims.Single(x => x.Type == "name").Value;
            notificationEntity.ToName = notificationEntity.FromName;
            notificationEntity.Priority = NotificationPriority.System;
            var isExerciseAdmin = (await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(exerciseId))).Succeeded;
            if (isExerciseAdmin)
            {
                notificationEntity.Text = String.Format("Successfully joined {0} notifications.", notificationEntity.ToName);
                canPost = true;
            }
            else
            {
                var isExerciseMember = (await _authorizationService.AuthorizeAsync(_user, null, new ExerciseMemberRequirement(exerciseId))).Succeeded;
                var isSameUser = (await _authorizationService.AuthorizeAsync(_user, null, new SameUserRequirement(userId))).Succeeded;
                if (isExerciseMember && isSameUser)
                {
                    notificationEntity.Text = String.Format("Successfully joined {0} notifications.", notificationEntity.ToName);
                }
                else
                {
                    notificationEntity.Text = String.Format("Failed to join {0} notifications.", notificationEntity.ToName);
                    wasSuccess = false;
                }
            }
            var returnNotification =  _mapper.Map<ViewModels.Notification>(notificationEntity);
            returnNotification.WasSuccess = wasSuccess;
            returnNotification.CanPost = canPost;
            returnNotification.IconUrl = GetIconUrl(notificationEntity.Priority, notificationEntity.FromName);
            return returnNotification;
        }

        /// <summary>
        /// Allows a client to broadcast a notification to the joined exercise user, IF:
        ///     1. they are an exercise administrator
        /// </summary>
        /// <param name="exerciseId"></param>
        /// <param name="userId"></param>
        /// <param name="incomingData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ViewModels.Notification> PostToUser(Guid exerciseId, Guid userId, ViewModels.Notification incomingData, CancellationToken cancellationToken)
        {
            var wasSuccess = true;
            var canPost = true;
            var messageTime = DateTime.Now.ToUniversalTime();
            NotificationEntity notificationEntity;
            if ((await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(exerciseId))).Succeeded )
            {
                notificationEntity = _mapper.Map<NotificationEntity>(incomingData);
                notificationEntity.ExerciseId = exerciseId;
                notificationEntity.ToId = userId;
                notificationEntity.ToType = NotificationType.User;
                notificationEntity.BroadcastTime = messageTime;
                notificationEntity.FromId = _user.GetId();
                notificationEntity.FromType = NotificationType.User;
                notificationEntity.FromName = _user.Claims.Single(x => x.Type == "name").Value;
                notificationEntity.ToName = _context.Users.Single(x => x.Id == userId).Name;
                _context.Notifications.Add(notificationEntity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                notificationEntity = new NotificationEntity{};
                wasSuccess = false;
                canPost = false;
            }
            var returnNotification =  _mapper.Map<ViewModels.Notification>(notificationEntity);
            returnNotification.WasSuccess = wasSuccess;
            returnNotification.CanPost = canPost;
            returnNotification.IconUrl = GetIconUrl(notificationEntity.Priority, notificationEntity.FromName);
            return returnNotification;
        }

        private string GetIconUrl(NotificationPriority notificationPriority, string userName)
        {
            var iconUrl = "";

            if (_user.HasClaim(c => c.Type == "client_logo"))
            {
                iconUrl = _user.Claims.Where(x => x.Type == "client_logo").FirstOrDefault().Value;
            }
            else if (notificationPriority == NotificationPriority.System)
            {
                iconUrl = _configuration["Notifications:SystemIconUrl"];
            }
            else if (_user.HasClaim(ClaimTypes.Role, PlayerClaimTypes.HelpDeskAgent.ToString()))
            {
                iconUrl = _context.Applications.FirstOrDefault(x => x.Name == _configuration["Notifications:HelpDeskApplicationName"]).Icon;
            }
            else
            {
                iconUrl = _configuration["Notifications:UserIconUrl"];
            }
            return iconUrl;
        }
    }
}

