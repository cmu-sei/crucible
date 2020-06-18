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
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using S3.Player.Api.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;
using S3.Player.Api.Services;
using S3.Player.Api.Infrastructure.Exceptions;

namespace S3.Player.Api.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserHub : Hub
    {
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly CancellationToken _ct;

        public UserHub(IUserService service, INotificationService notificationService)
        {
            _userService = service;
            _notificationService = notificationService;
            CancellationTokenSource source = new CancellationTokenSource();
            _ct = source.Token;
        }

        public async Task Join(string viewString, string userString)
        {
            var userId = Guid.Parse(userString);
            var viewId = Guid.Parse(viewString);
            var notification = await _notificationService.JoinUser(viewId, userId, _ct);
            if (notification.ToId == userId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, getGroupString(viewString, userString));
            }
            await Clients.Caller.SendAsync("Reply", notification);
        }

        public async Task Post(string viewString, string userString, string data)
        {
            var viewId = Guid.Parse(viewString);
            var userId = Guid.Parse(userString);
            var incomingData = new Notification();
            incomingData.Text = data;
            incomingData.Subject = "User Notification";
            var notification = await _notificationService.PostToUser(viewId, userId, incomingData, _ct);
            if (notification.ToId != userId)
            {
                notification.Text = "Message was not sent";
                await Clients.Caller.SendAsync("Reply", notification);
            }
            else
            {
                await Clients.Group(getGroupString(viewString, userString)).SendAsync("Reply", notification);
            }
        }

        public async Task GetHistory(string viewString, string userString)
        {
            var viewId = Guid.Parse(viewString);
            var userId = Guid.Parse(userString);
            var notifications = await _notificationService.GetByUserAsync(viewId, userId, _ct);
            await Clients.Caller.SendAsync("History", notifications);
        }

        public async Task Leave(string viewString, string userString)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, getGroupString(viewString, userString));
        }

        private string getGroupString(string viewString, string userString)
        {
            return String.Format("{0}_{1}", viewString, userString);
        }
    }
}
