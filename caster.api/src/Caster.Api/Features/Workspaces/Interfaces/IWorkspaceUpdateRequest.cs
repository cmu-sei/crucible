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
using Caster.Api.Domain.Services;
using FluentValidation;

namespace Caster.Api.Features.Workspaces.Interfaces
{
    public interface IWorkspaceUpdateRequest
    {
        string Name { get; set; }
        string TerraformVersion { get; set; }
    }

    public class IWorkspaceUpdateValidator : AbstractValidator<IWorkspaceUpdateRequest>
    {
        public IWorkspaceUpdateValidator(ITerraformService terraformService)
        {
            RuleFor(x => x.Name)
                .MinimumLength(1)
                .MaximumLength(90)
                .Must(x => x.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.'))
                .WithMessage($"Workspace names need to be 90 characters or less and can only include letters, numbers, -, _, and .")
                .When(x => x.Name != null);

            RuleFor(x => x.Name)
                .NotNull()
                .When(x => !(x is PartialEdit.Command));

            RuleFor(x => x.TerraformVersion)
                .Must(x => terraformService.IsValidVersion(x))
                .WithMessage("The specified version is not available. Please contact a system administrator to request it be added to the system.")
                .When(x => !string.IsNullOrEmpty(x.TerraformVersion));
        }
    }
}
