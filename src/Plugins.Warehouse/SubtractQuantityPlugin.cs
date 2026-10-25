using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TALXIS.DevKit.Build.Dataverse.Plugin;

namespace Plugins.Warehouse
{
    [PluginStep("Create", "lab_warehousetransaction", PluginStage.PostOperation)]
    public class SubtractQuantityPlugin : PluginBase
    {
        public SubtractQuantityPlugin()
        {
        }

        public SubtractQuantityPlugin(string unsecureConfiguration, string secureConfiguration)
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            var context = localPluginContext.PluginExecutionContext;
            if (!context.InputParameters.Contains("Target"))
            {
                return;
            }

            var target = context.InputParameters["Target"] as Entity;
            if (target == null || target.LogicalName != "lab_warehousetransaction")
            {
                return;
            }

            if (!target.Contains("lab_quantity") || !target.Contains("lab_itemid") || !target.Contains("lab_transactiontype"))
            {
                return;
            }

            var quantity = (int)target["lab_quantity"];
            var itemReference = (EntityReference)target["lab_itemid"];
            var transactionType = (OptionSetValue)target["lab_transactiontype"];
            var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);
            var item = service.Retrieve("lab_warehouseitem", itemReference.Id, new ColumnSet("lab_availablequantity"));
            var available = item.GetAttributeValue<int>("lab_availablequantity");

            if (transactionType.Value == 100000000)
            {
                item["lab_availablequantity"] = available + quantity;
            }
            else if (transactionType.Value == 100000001)
            {
                item["lab_availablequantity"] = available - quantity;
            }
            else
            {
                return;
            }

            service.Update(item);
        }
    }
}