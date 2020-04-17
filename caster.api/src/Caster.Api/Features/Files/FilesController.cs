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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Caster.Api.Features.Files
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get a single file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        /// <returns></returns>
        [HttpGet("files/{id}")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetFile")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Get.Query { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Export a single file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        /// <returns></returns>
        [HttpGet("files/{id}/actions/export")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "ExportFile")]
        public async Task<IActionResult> Export([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Get.Query { Id = id });
            return File(Encoding.ASCII.GetBytes(result.Content), "text/plain", result.Name);
        }

        /// <summary>
        /// Get all files.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("files")]
        [ProducesResponseType(typeof(IEnumerable<File>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetAllFiles")]
        public async Task<IActionResult> GetAll([FromQuery] GetAll.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get all files within a directory.
        /// </summary>
        /// <param name="directoryId">ID of a directory.</param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("directories/{directoryId}/files")]
        [ProducesResponseType(typeof(IEnumerable<File>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetFilesByDirectory")]
        public async Task<IActionResult> GetByDirectory([FromRoute] Guid directoryId, [FromQuery] GetByDirectory.Query query)
        {
            query.DirectoryId = directoryId;
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new file.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("files")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "CreateFile")]
        public async Task<IActionResult> Create(Create.Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update a file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("files/{id}")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "EditFile")]
        public async Task<IActionResult> Edit([FromRoute] Guid id, Edit.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Partial update a file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPatch("files/{id}")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "PartialEditFile")]
        public async Task<IActionResult> PartialEdit([FromRoute] Guid id, PartialEdit.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Rename a file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("files/{id}/actions/rename")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "RenameFile")]
        public async Task<IActionResult> Rename([FromRoute] Guid id, Rename.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        /// <returns></returns>
        [HttpDelete("files/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "DeleteFile")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _mediator.Send(new Delete.Command { Id = id });
            return NoContent();
        }

        /// <summary>
        /// Tag a list of files.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("files/actions/tag")]
        [ProducesResponseType(typeof(FileVersion[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "TagFiles")]
        public async Task<IActionResult> Tag(Tag.Command command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get all versions of a file.
        /// </summary>
        /// <param name="fileId">ID of a file</param>
        /// <returns></returns>
        [HttpGet("files/{fileId}/versions")]
        [ProducesResponseType(typeof(IEnumerable<FileVersion>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetFileVersions")]
        public async Task<IActionResult> GetFileVersions([FromRoute] Guid fileId)
        {
            var result = await _mediator.Send(new GetFileVersions.Query { FileId = fileId });
            return Ok(result);
        }

        /// <summary>
        /// Get a single file version.
        /// </summary>
        /// <param name="id">ID of a file version.</param>
        /// <returns></returns>
        [HttpGet("files/versions/{id}")]
        [ProducesResponseType(typeof(FileVersion), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetFileVersion")]
        public async Task<IActionResult> GetFileVersion([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new GetFileVersion.Query { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Lock a file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        [HttpPost("files/{id}/actions/lock")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "LockFile")]
        public async Task<IActionResult> Lock([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Lock.Command { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Unlock a file.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        [HttpPost("files/{id}/actions/unlock")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "UnlockFile")]
        public async Task<IActionResult> Unlock([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Unlock.Command { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Administratively lock a File so that only System Admins can make changes.
        /// </summary>
        /// <param name="id">ID of a file.</param>
        [HttpPost("files/{id}/actions/admin-lock")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "AdministrativelyLockFile")]
        public async Task<IActionResult> AdminLock([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new AdminLock.Command { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Remove an Administrative Lock on a File
        /// </summary>
        /// <param name="id">ID of a file.</param>
        [HttpPost("files/{id}/actions/admin-unlock")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "AdministrativelyUnlockFile")]
        public async Task<IActionResult> AdminUnlock([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new AdminUnlock.Command { Id = id });
            return Ok(result);
        }
    }
}
