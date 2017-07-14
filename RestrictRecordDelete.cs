// File Name: RestrictRecordDelete.cs
// Created: July 14, 2017
// Author: Someswara Siripuram
// Revisions:
// ======================================================================
// VERSION      DATE(mm/dd/yyyy)            Modified By         DESCRIPTION 
// ----------------------------------------------------------------------
// 1.0          07/14/2017               Someswara Siripuram        CREATED 
// ======================================================================

namespace CommonApps.CRMPlugins
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Validate Influencer Connection record- To Parameter is null, delete the current record
    /// </summary>
    public class RestrictRecordDelete : IPlugin
    {
        /// <summary>
        /// Organization Service
        /// </summary>
        private IOrganizationService service = null;

        /// <summary>
        /// Plugin Context
        /// </summary>
        private IPluginExecutionContext context = null;

        /// <summary>
        /// Tracing service
        /// </summary>
        private ITracingService tracer = null;

        /// <summary>
        /// Organization factory
        /// </summary>
        private IOrganizationServiceFactory factory = null;

        /// <summary>
        /// Entity fields
        /// </summary>
        private Entity currentRecord = null;

        /// <summary>
        /// Execute Main method
        /// </summary>
        /// <param name="serviceProvider">Service Provider</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                this.tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                this.service = this.factory.CreateOrganizationService(this.context.UserId);
            }
            else
            {
                throw new InvalidPluginExecutionException("Service Provider is null");
            }

            try
            {
                if (this.context.MessageName.ToLower().Equals("delete"))
                {
                    Guid currentUserID = context.UserId;
                    tracer.Trace("Current User ID: " + context.UserId.ToString());


                    if (!HasAdminRole(currentUserID))
                        throw new InvalidPluginExecutionException("Only Sytem Administrators can delete this Entity. Please contact your System Administrator.");


                }
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> e)
            {
                //this.service.Create(ErrorLogger.LogError(e.Message, e.StackTrace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, this.context.PrimaryEntityId));
                throw;
            }
            catch (InvalidPluginExecutionException e)
            {
                throw;
            }
            catch (System.Exception e)
            {
                //this.service.Create(ErrorLogger.LogError(e.Message, e.StackTrace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, this.context.PrimaryEntityId));
                throw;
            }
        }


        public bool HasAdminRole(Guid systemUserId)
        {
            Guid AdminRoleTemplateId = new Guid("627090FF-40A3-4053-8790-584EDC5BE201");

            QueryExpression query = new QueryExpression("role");
            query.Criteria.AddCondition("roletemplateid", ConditionOperator.Equal, AdminRoleTemplateId);
            LinkEntity link = query.AddLink("systemuserroles", "roleid", "roleid");
            link.LinkCriteria.AddCondition("systemuserid", ConditionOperator.Equal, systemUserId);

            return service.RetrieveMultiple(query).Entities.Count > 0;
        }


    }
}
