/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Caster.Api.Data;
using Caster.Api.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Caster.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Caster.Api.Features.Files.EventHandlers
{
    public class SignalRFileDeletedHandler : INotificationHandler<FileDeleted>
    {
        private readonly CasterContext _db;
        private readonly IMapper _mapper;
        private readonly IHubContext<ExerciseHub> _exerciseHub;

        public SignalRFileDeletedHandler(
            CasterContext db,
            IMapper mapper,
            IHubContext<ExerciseHub> exerciseHub)
        {
            _db = db;
            _mapper = mapper;
            _exerciseHub = exerciseHub;
        }

        public async Task Handle(FileDeleted notification, CancellationToken cancellationToken)
        {
            var exerciseId = await _db.Directories
                .Where(d => d.Id == notification.File.DirectoryId)
                .Select(d => d.ExerciseId)
                .FirstOrDefaultAsync();

            await _exerciseHub.Clients.Group(exerciseId.ToString()).SendAsync("FileDeleted", notification.File.Id);
        }
    }
}
