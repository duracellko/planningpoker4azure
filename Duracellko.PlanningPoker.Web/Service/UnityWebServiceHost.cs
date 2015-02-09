// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Web;
using Microsoft.Practices.Unity;
using Unity.Wcf;

namespace Duracellko.PlanningPoker.Web.Service
{
    /// <summary>
    /// Web service host that creates service instances from Unity container.
    /// </summary>
    public class UnityWebServiceHost : WebServiceHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebServiceHost"/> class.
        /// </summary>
        /// <param name="container">The Unity container.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        public UnityWebServiceHost(IUnityContainer container, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.ApplyServiceBehaviors(container);
            this.ApplyContractBehaviors(container);

            foreach (ContractDescription description in this.ImplementedContracts.Values)
            {
                UnityContractBehavior item = new UnityContractBehavior(new UnityInstanceProvider(container, description.ContractType));
                description.Behaviors.Add(item);
            }
        }

        private void ApplyContractBehaviors(IUnityContainer container)
        {
            foreach (IContractBehavior behavior in container.ResolveAll<IContractBehavior>(new ResolverOverride[0]))
            {
                foreach (ContractDescription description in this.ImplementedContracts.Values)
                {
                    description.Behaviors.Add(behavior);
                }
            }
        }

        private void ApplyServiceBehaviors(IUnityContainer container)
        {
            foreach (IServiceBehavior behavior in container.ResolveAll<IServiceBehavior>(new ResolverOverride[0]))
            {
                this.Description.Behaviors.Add(behavior);
            }
        }
    }
}