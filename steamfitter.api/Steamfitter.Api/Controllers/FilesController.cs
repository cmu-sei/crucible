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
using System.IO;
using System.Net;
using System.Threading;
using STT = System.Threading.Tasks;
using Steamfitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Steamfitter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : BaseController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IFilesService _filesService;

        public FilesController(IAuthorizationService authorizationService, IFilesService filesService)
        {
            _authorizationService = authorizationService;
            _filesService = filesService;
        }

        /// <summary>
        /// Gets all files that a user can dispatch to guest vms
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>List of files this user can dispatch</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FileInfo>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getAllFiles")]
        public async STT.Task<IActionResult> Get(CancellationToken ct)
        {
            return Ok(await _filesService.GetAsync(ct));
        }
        
        /// <summary>
        /// Gets a file that a user can dispatch to guest vms
        /// </summary>
        /// <param name="id">Id of the file</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>List of files this user can dispatch</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FileInfo), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getFileById")]
        public async STT.Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            return Ok(await _filesService.GetAsync(id, ct));
        }
        
        /// <summary>
        /// Saves files and enables them to be dispatchable. If the file is not a zip file, it automatically gets zipped on upload
        /// </summary>
        /// <param name="files">IEnumerable<IFormFile/></param>
        /// <param name="ct">CancellationToken</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<FileInfo>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "saveFile")]
        public async STT.Task<IActionResult> Post(IEnumerable<IFormFile> files, CancellationToken ct)
        {
            return Ok(await _filesService.SaveAsync(files, ct));
        }
        
        /// <summary>
        /// Deletes a file record and the actual file
        /// </summary>
        /// <param name="id">File to delete</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(IEnumerable<FileInfo>), (int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "deleteFile")]
        public async STT.Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _filesService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}

