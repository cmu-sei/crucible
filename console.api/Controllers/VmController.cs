/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using S3.Player.Api;
using S3.Player.Api.Models;
using S3.Vm.Console.Extensions;
using S3.Vm.Console.Models;
using S3.Vm.Console.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using NetVimClient;

namespace S3.Vm.Console.Controllers
{
    [Authorize]
    public class VmController : Controller
    {
        private VmService _vmService;
        private VmOptions _options;
        private readonly ILogger<VmController> _logger;
        private readonly ClaimsPrincipal _user;

        public VmController(
                VmService vmService,
                ILogger<VmController> logger,
                IOptions<VmOptions> options,
                IPrincipal user) : base()
        {
            _vmService = vmService;
            _options = options.Value;
            _logger = logger;
            _user = user as ClaimsPrincipal;
        }

        public string GetAccessToken()
        {
            string authHeader = Task.FromResult(HttpContext.Request.Headers["Authorization"]).Result;
            string token = null;
            if (!string.IsNullOrEmpty(authHeader))
            {
                token = authHeader.Replace("bearer ", string.Empty).Replace("Bearer ", string.Empty);
            }

            // Go back to this when bug is fixed in .NET Core 2.2
            //var token = HttpContext.GetTokenAsync("access_token").Result;

            return token;
        }

        // primary point of contact for ng console to retrieve name and ticket in one call
        [HttpGet("api/{uuid}")]
        public async Task<IActionResult> Get(Guid uuid)
        {
            string accessToken = GetAccessToken();
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }

            model = await _vmService.GetAsync(uuid, model);

            if (_options.LogConsoleAccess && _logger.IsEnabled(LogLevel.Information))
            {
                var s3playerApiClient = new S3PlayerApiClient(new Uri(_options.PlayerApiUrl), new TokenCredentials(accessToken, "Bearer"));
                var team = (s3playerApiClient.GetUserExerciseTeams(model.ExerciseId, _user.GetId()) as IEnumerable<Team>).FirstOrDefault(t => t.IsPrimary.Value);

                _logger.LogInformation(new EventId(1), $"User {_user.GetName()} ({_user.GetId()}) in Team {team.Name} ({team.Id}) accessed console of {model.Name} ({uuid})");
            }

            return Ok(model);
        }

        [HttpGet("api/{uuid}/poweron")]
        public async Task<IActionResult> PowerOn(Guid uuid)
        {
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }

            string res = await _vmService.PowerOnVm(uuid);
            return Json(res);
        }

        [HttpGet("api/{uuid}/poweroff")]
        public async Task<IActionResult> PowerOff(Guid uuid)
        {
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }

            string res = await _vmService.PowerOffVm(uuid);
            return Json(res);
        }


        [HttpGet("api/{uuid}/adapter/{adapter}/nic/{nic}")]
        public async Task<IActionResult> ChangeNic(Guid uuid, string adapter, string nic)
        {
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }
            else if (!model.CanAccessNicConfiguration)
            {
                Response.StatusCode = 403;
                return Json("You do not have permission to change networks on this vm.");
            }

            TaskInfo res = await _vmService.ReconfigureVm(uuid, Extensions.Feature.net, adapter, nic);
            return await Get(uuid);
        }


        [HttpGet("api/{uuid}/reboot")]
        public async Task<IActionResult> Reboot(Guid uuid)
        {
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }

            string res = await _vmService.RebootVm(uuid);
            return Json(res);
        }

        [HttpGet("api/{uuid}/checkvmtools")]
        public async Task<IActionResult> CheckVmTools(Guid uuid)
        {
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }
            var isAvailable = await _vmService.VmToolsAvailable(uuid);
            if (!isAvailable)
            {
                Response.StatusCode = 400;
                return Json("VM Tools are NOT available");
            }

            return Json("VM Tools are available");
        }

        [HttpPost("api/{uuid}/checkvmcredentials")]
        public async Task<IActionResult> CheckVmCredentials(Guid uuid)
        {
            var username = Request.Form["username"][0];
            var password = Request.Form["password"][0];
            var filepath = Request.Form["filepath"][0];
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }
            try
            {
                // verify credentials
                await _vmService.UploadFileToVm(uuid,
                                            username,
                                            password,
                                            filepath,
                                            new MemoryStream());
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("is not a file"))
                {
                    Response.StatusCode = 400;
                    return Json(ex.Message);
                }
            }

            return Json("Credentials Authenticated");
        }

        [HttpPost("api/{uuid}/uploadfile"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile(Guid uuid)
        {
            var files = Request.Form.Files;
            var username = Request.Form["username"][0];
            var password = Request.Form["password"][0];
            var filepath = Request.Form["filepath"][0];
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }
            foreach (var formFile in files)
            {
                using (Stream fileStream = formFile.OpenReadStream())
                {
                    try
                    {
                        await _vmService.UploadFileToVm(uuid,
                                                    username,
                                                    password,
                                                    string.Format("{0}{1}", filepath, formFile.FileName),
                                                    fileStream);
                    }
                    catch (Exception ex)
                    {
                        Response.StatusCode = 400;
                        return Json(ex.Message);
                    }
                }
            }

            return Json("Files were successfully uploaded.");
        }

        [HttpGet("api/{uuid}/isos")]
        public async Task<IActionResult> GetIsos(Guid uuid)
        {
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null) {
                Response.StatusCode = 400;
                return Json("Error");
            }
            return Json(await _vmService.GetIsos(model.ExerciseId.ToString(), model.TeamId.ToString()));
        }

        [HttpPost("api/{uuid}/mountiso")]
        public async Task<IActionResult> MountIso(Guid uuid)
        {
            VmModel model = await _vmService.GetModel(uuid);
            if (model.Name == null)
            {
                Response.StatusCode = 400;
                return Json("Error");
            }
            var iso = Request.Form["iso"][0];
            TaskInfo res = await _vmService.ReconfigureVm(uuid, Extensions.Feature.iso, "", iso);
            return await Get(uuid);
        }



    }
}
