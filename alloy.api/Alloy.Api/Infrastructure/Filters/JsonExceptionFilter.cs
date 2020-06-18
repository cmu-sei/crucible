/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Alloy.Api.Infrastructure.Exceptions;
using Microsoft.Extensions.Hosting;
using Alloy.Api.ViewModels;
using System;
using System.Net;

namespace Alloy.Api.Infrastructure.Filters
{
    public class JsonExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;

        public JsonExceptionFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            var error = new ApiError();
            error.Status = GetStatusCodeFromException(context.Exception);

            if(error.Status == (int)HttpStatusCode.InternalServerError)
            {
                if (_env.IsDevelopment())
                {
                    error.Title = context.Exception.Message;
                    error.Detail = context.Exception.StackTrace;
                }
                else
                {
                    error.Title = "A server error occurred.";
                    error.Detail = context.Exception.Message;
                }
            }
            else
            {
                error.Title = context.Exception.Message;
            }            

            context.Result = new JsonResult(error)
            {
                StatusCode = error.Status
            };
        }

        /// <summary>
        /// map all custom exceptions to proper http status code
        /// </summary>
        /// <returns></returns>
        private static int GetStatusCodeFromException(Exception exception)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            if (exception is IApiException)
            {
                statusCode = (exception as IApiException).GetStatusCode();
            }

            return (int)statusCode;
        }
    }
}

