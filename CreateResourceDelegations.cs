using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.ServiceModel;

namespace CF.MI.ConsoleApp
{
    class ResourceDelegations
    {

        /// <summary>
        /// Create Resource Delegations for all Bookable resources
        /// </summary>
        /// <param name="client">CRM Service Client object</param>
        private static void CreateResourceDelegations(CrmServiceClient _client)
        {
            //// GUID of the the Person/ Bookable Resource to delegate the Time entries - Usually Resource manager or HR
            Guid DelegateToId = new Guid("XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX");

            #region Get Bookable Resources
            string userBookableResourcesFetch = @"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='bookableresource'>
                        <attribute name='name' />
                        <attribute name='msdyn_organizationalunit' />
                        <attribute name='msdyn_targetutilization' />
                        <attribute name='bookableresourceid' />
                        <order attribute='name' descending='false' />
                        <filter type='and'>
                          <condition attribute='resourcetype' operator='eq' value='3' />
                        </filter>
                        <link-entity name='systemuser' from='systemuserid' to='userid' visible='false' link-type='outer' alias='user'>                        
                          <attribute name='title' />                          
                        </link-entity>
                      </entity>
                    </fetch>";

            EntityCollection userBookableResources = _client.RetrieveMultiple(new FetchExpression(userBookableResourcesFetch));

            #endregion

            #region Create Delegations for all User Bookable Resources to Particular person

            foreach (Entity bookableResource in userBookableResources.Entities)
            {
                Entity TimeEntryDelegation = new Entity("msdyn_delegation");
                TimeEntryDelegation["msdyn_delegationfrom"] = new EntityReference("bookableresource", bookableResource.Id);
                TimeEntryDelegation["msdyn_delegationto"] = new EntityReference("bookableresource", DelegateToId);
                TimeEntryDelegation["msdyn_startdate"] = new DateTime(2018, 10, 1);
                TimeEntryDelegation["msdyn_enddate"] = new DateTime(2022, 12, 31);
                TimeEntryDelegation["msdyn_type"] = new OptionSetValue(192350000); //Time Entry
                TimeEntryDelegation["msdyn_name"] = string.Format("Delegation to HR for {0}", bookableResource.GetAttributeValue<string>("name"));
                try
                {
                    Guid DelegationId = _client.Create(TimeEntryDelegation);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Console.WriteLine("Resource: " + bookableResource.GetAttributeValue<string>("name") + " Error: " + ex.Message);

                }
            }

            #endregion
        }
    }
}
