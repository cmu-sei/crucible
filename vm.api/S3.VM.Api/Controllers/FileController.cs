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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using S3.VM.Api.Infrastructure.Options;
using S3.VM.Api.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace S3.VM.Api.Controllers
{
    [Authorize]
    [Route("api/")]
    public class FileController : Controller
    {
        private IsoUploadOptions _isoUploadOptions;
        private readonly IPlayerService _playerService;

        public FileController(
            IsoUploadOptions isoUploadOptions,
            IPlayerService playerService
        ) : base()
        {
            _isoUploadOptions = isoUploadOptions;
            _playerService = playerService;
        }

        [HttpPost("views/{uuid}/isos"), DisableRequestSizeLimit]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "uploadFileAsIso")]
        public async Task<IActionResult> Upload(Guid uuid)
        {
            var formFile = Request.Form.Files[0];
            var filename = SanitizeFilename(formFile.Name);
            var scope = Request.Form["scope"][0];
            var size = Convert.ToInt64(Request.Form["size"][0]);

            if (size > _isoUploadOptions.MaxFileSize)
            {
                throw new Exception($"File exceeds the {_isoUploadOptions.MaxFileSize} byte maximum size.");
            }

            var teamId = await _playerService.GetPrimaryTeamByViewIdAsync(uuid, new System.Threading.CancellationToken());

            if (scope == "view")
            {
                if (!(await _playerService.CanManageTeamAsync(teamId, new System.Threading.CancellationToken())))
                    throw new InvalidOperationException("You do not have permission to upload public files for this View");
            }

            var destPath = Path.Combine(
                _isoUploadOptions.BasePath,
                uuid.ToString(),
                (scope == "view") ? uuid.ToString() : teamId.ToString()
            );

            var destFile = Path.Combine(destPath, filename);

            Directory.CreateDirectory(destPath);

            using (var sourceStream = formFile.OpenReadStream())
            {

                if (filename.ToLower().EndsWith(".iso"))
                {
                    using (var destStream = System.IO.File.Create(destFile))
                    {
                        await sourceStream.CopyToAsync(destStream);
                    }
                }
                else
                {
                    CDBuilder builder = new CDBuilder();
                    builder.UseJoliet = true;
                    builder.VolumeIdentifier = "PlayerIso";
                    builder.AddFile(filename, sourceStream);
                    builder.Build(destFile + ".iso");
                }
            }

            return Json("ISO was uploaded");
        }

        private string SanitizeFilename(string filename)
        {
            string fn = "";
            char[] bad = Path.GetInvalidFileNameChars();
            foreach (char c in filename.ToCharArray())
                if (!bad.Contains(c))
                    fn += c;
            return fn;
        }
    }
}
