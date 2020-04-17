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
using System.Linq;
using System.Reflection;
using Caster.Api.Features.Files;
using MediatR;
using MediatR.Pipeline;
using SimpleInjector;

namespace Caster.Api.Infrastructure.Extensions
{
    public static class ContainerExtensions
    {
        public static Container AddMediator(this Container container, params Assembly[] assemblies) {
            return BuildMediator (container, (IEnumerable<Assembly>) assemblies);
        }

        public static Container BuildMediator (this Container container, IEnumerable<Assembly> assemblies) {
            var allAssemblies = new List<Assembly> { typeof (IMediator).GetTypeInfo().Assembly };
            allAssemblies.AddRange (assemblies);

            container.Register<IMediator, Mediator>();
            container.Register(typeof (IRequestHandler<,>), allAssemblies);

            container.Collection.Register(typeof(INotificationHandler<>), GetTypesToRegister(typeof(INotificationHandler<>), container, assemblies));
            container.Collection.Register(typeof(IPipelineBehavior<,>), GetTypesToRegister(typeof(IPipelineBehavior<,>), container, assemblies));
            container.Collection.Register(typeof(IRequestPreProcessor<>), GetTypesToRegister(typeof(IRequestPreProcessor<>), container, assemblies));
            container.Collection.Register(typeof(IRequestPostProcessor<,>), GetTypesToRegister(typeof(IRequestPostProcessor<,>), container, assemblies));

            container.Register(() => new ServiceFactory(container.GetInstance));

            return container;
        }

        private static IEnumerable<Type> GetTypesToRegister(Type type, Container container, IEnumerable<Assembly> assemblies)
        {
            // we have to do this because by default, generic type definitions (such as the Constrained Notification Handler) won't be registered
            return container.GetTypesToRegister(type, assemblies, new TypesToRegisterOptions {
                IncludeGenericTypeDefinitions = true,
                IncludeComposites = false,
            });
        }
    }
}
