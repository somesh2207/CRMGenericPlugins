using Microsoft.Xrm.Sdk;
using System;

namespace CommonApps.CRMPlugins
{
    public class SignatureDataToAttachment : IPlugin
    {
        #region Secure/Unsecure Configuration Setup
        private string _secureConfig = null;
        private string _unsecureConfig = null;

        public SignatureDataToAttachment(string unsecureConfig, string secureConfig)
        {
            _secureConfig = secureConfig;
            _unsecureConfig = unsecureConfig;
        }
        #endregion
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            try
            {
                //// The plugin is registered on the Post Update on "Customer Approval" field on Opportunity.
                Entity entity = (Entity)context.InputParameters["Target"];

                //// The field which stores the data for Signature
                string signatureFieldName = "new_customerapproval";

                if (entity.Contains(signatureFieldName))
                {
                    string encodedData = entity.GetAttributeValue<string>(signatureFieldName);
                    
                    //// Remove the additional Metadata from the text generated.
                    int startIndex = encodedData.IndexOf("base64,") + 7;
                    encodedData = encodedData.Substring(startIndex, encodedData.Length - startIndex);
                    tracer.Trace(encodedData);

                    string contentType = "image/png";
                    Entity Annotation = new Entity("annotation");
                    Annotation.Attributes["objectid"] = new EntityReference(entity.LogicalName, entity.Id);
                    Annotation.Attributes["objecttypecode"] = entity.LogicalName;
                    Annotation.Attributes["subject"] = "Customer Signature"; //// You can have any subject as required.
                    Annotation.Attributes["documentbody"] = encodedData;
                    Annotation.Attributes["mimetype"] = contentType;
                    Annotation.Attributes["notetext"] = "Customer Signature Attached"; //// Again, add any note text as needed
                    Annotation.Attributes["filename"] = "Customer Approval Signature.png"; //// OR Any name as required

                    Guid annotation = service.Create(Annotation);
                }
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
