using DomainLayer.Market;
using CustomLogger;
using DomainLayer.NotificationsCenter;
using NotificationsManagment;
using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using Logger = CustomLogger.Logger;

namespace ServiceLayer.Services
{
    public static class SetUp
    {
        private static JsonResponse Json;

        public static string Init(string adminUsername, string adminPassword, string? init_operations_loc, out NotificationManager notification,
                                    out PurchaseActions PurchaseActions, out UserActions UserActions, out StoreActions StoreActions, bool useInMemoryDB = false)
        {
            Logger.writeEvent("SetUp: Initailzing System");
            Json = new JsonResponse();
            notification = new NotificationManager();
            string result = InitiateMarketFacade(adminUsername, adminPassword, notification, out IMarketFacade newMarketFacade, useInMemoryDB);
            PurchaseActions = new PurchaseActions(newMarketFacade);
            UserActions = new UserActions(newMarketFacade);
            StoreActions = new StoreActions(newMarketFacade);
            if (!Json.deserializeSuccess(result))// error on MarketFacade Init
            {
                return result;
            }
            else if (!ExternalSystemsAreConnected(newMarketFacade))
            {
                Logger.writeEvent("SetUp: External Systems Error");
                return Json.Create_json_response(false, "External Systems Error");
            }
            else
            {
                if (init_operations_loc != null)
                {
                    try
                    {
                        string op_lines = File.ReadAllText(init_operations_loc);
                        Run_init_operations(op_lines, UserActions, StoreActions, PurchaseActions);
                    }
                    catch (Exception e)
                    {
                        Logger.writeError(e);
                        Logger.writeEvent("SetUp: Invalid operations File");
                        return Json.Create_json_response(false, "Invalid operations File");
                    }
                }
                Logger.writeEvent("SetUp: Initailzation Completed Successfully");
                return result;
            }
        }

        private static void Run_init_operations(string init_operations, UserActions userActions, StoreActions storeActions, PurchaseActions purchaseActions)
        {
            string script_header = @"
        using ServiceLayer.Services;
        using ServiceLayer;
        using System;
        public class ActionsRunner : IActionsRunner {
            public void Run(UserActions userActions, StoreActions storeActions, PurchaseActions purchaseActions, Guid sessionID, JsonResponse Json)
            {
";
            string script_footer = @"
            
            }
        }
        return typeof(ActionsRunner);
";
            var script = CSharpScript.Create(script_header + init_operations + script_footer,
                ScriptOptions.Default.WithReferences(Assembly.GetExecutingAssembly()));
            script.Compile();
            var runner_type = (Type)script.RunAsync().Result.ReturnValue;
            var runner = (IActionsRunner)Activator.CreateInstance(runner_type);
            runner.Run(userActions, storeActions, purchaseActions, Guid.NewGuid(), new JsonResponse());
        } 

        private static string InitiateMarketFacade(string adminUsername, string adminPassword, ICommunicationNotificationAlerter communication, 
                                                    out IMarketFacade newMarketFacade, bool useInMemoryDB )
        {
            if (string.IsNullOrEmpty(adminPassword) || string.IsNullOrWhiteSpace(adminUsername))
            {
                newMarketFacade = null;
                Logger.writeEvent("InitiateMarketFacade : Invalid input");
                return Json.Create_json_response(false, "Invalid input");
            }

            newMarketFacade = new MarketFacade(adminUsername, adminPassword, communication, useInMemoryDB);

            Logger.writeEvent("InitiateMarketFacade: Initialized Successfully");
            return Json.Create_json_response(true, true);
        }

        private static bool ExternalSystemsAreConnected(IMarketFacade newMarketFacade)
        {
            string res = newMarketFacade.AreExternalSystemsConnected();
            bool result = Json.deserializeResponse<bool>(res);
            return result;
        }
    }

    public interface IActionsRunner
    {
        void Run(UserActions userActions, StoreActions storeActions, PurchaseActions purchaseActions,
            Guid sessionID, JsonResponse Json);
    }

}
