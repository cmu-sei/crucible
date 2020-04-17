/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Alloy.Api.Extensions
{
    public static class CorsPolicyExtensions
    {
        public static CorsOptions UseConfiguredCors(
            this CorsOptions builder,
            IConfiguration section
        )
        {
            CorsPolicyOptions policy = new CorsPolicyOptions();
            ConfigurationBinder.Bind(section, policy);
            builder.AddPolicy("default", policy.Build());
            return builder;
        }
    }
    public class CorsPolicyOptions
    {
        public string[] Origins { get; set; }
        public string[] Methods { get; set; }
        public string[] Headers { get; set; }
        public bool AllowAnyOrigin { get; set; }
        public bool AllowAnyMethod { get; set; }
        public bool AllowAnyHeader { get; set; }
        public bool SupportsCredentials { get; set; }

        public CorsPolicy Build()
        {
            CorsPolicyBuilder policy = new CorsPolicyBuilder();
            if (this.AllowAnyOrigin)
                policy.AllowAnyOrigin();
            else
                policy.WithOrigins(this.Origins);

            if (this.AllowAnyHeader)
                policy.AllowAnyHeader();
            else
                policy.WithHeaders(this.Headers);

            if (this.AllowAnyMethod)
                policy.AllowAnyMethod();
            else
                policy.WithMethods(this.Methods);

            if (this.SupportsCredentials)
                policy.AllowCredentials();
            else
                policy.DisallowCredentials();

            return policy.Build();
        }
    }
}

