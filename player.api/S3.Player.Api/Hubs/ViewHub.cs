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
    public class ViewHub : Hub
    {
        private readonly IViewService _viewService;
        private readonly INotificationService _notificationService;
        private readonly CancellationToken _ct;

        public ViewHub(IViewService service, INotificationService notificationService)
        {
            _viewService = service;
            _notificationService = notificationService;
            CancellationTokenSource source = new CancellationTokenSource();
            _ct = source.Token;
        }

        public async Task Join(string idString)
        {
            var id = Guid.Parse(idString);
            var notification = await _notificationService.JoinView(id, _ct);
            if (notification.ToId == id)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, idString);
            }
            await Clients.Caller.SendAsync("Reply", notification);
        }

        public async Task Post(string idString, string data)
        {
            var id = Guid.Parse(idString);
            var incomingData = new Notification();
            incomingData.Text = data;
            incomingData.Subject = "View Notification";
            var notification = await _notificationService.PostToView(id, incomingData, _ct);
            if (notification.ToId != id)
            {
                notification.Text = "Message was not sent";
                await Clients.Caller.SendAsync("Reply", notification);
            }
            else
            {
                await Clients.Group(idString).SendAsync("Reply", notification);
            }
        }

        public async Task GetHistory(string idString)
        {
            var id = Guid.Parse(idString);
            var notifications = await _notificationService.GetByViewAsync(id, _ct);
            await Clients.Caller.SendAsync("History", notifications);
        }

        public async Task Leave(string idString)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, idString);
        }
    }
}
