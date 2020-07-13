using DomainLayer.NotificationsCenter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NotificationsManagment;
using ServiceLayer.Services;
using System;

namespace PresentationLayer
{

    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Init all classes here
            string ADMIN_NAME;
            string ADMIN_PASS;
            string INIT_FILE_PATH = null;
            if (args.Length < 2)//default
            {
                ADMIN_NAME = "ADMIN";
                ADMIN_PASS = "ADMIN";
                Console.WriteLine("Usage: dotnet build <username> <password>\n" +
                    "Please specify Admin name and password next time");
            }
            else 
            {
                ADMIN_NAME = args[0];
                ADMIN_PASS = args[1];
                if (args.Length >= 3)
                {
                    Console.WriteLine("using init operations file");
                    INIT_FILE_PATH = args[2];
                }
            }
            Console.WriteLine("Admin Username: {0}\n" +
                                    "Admin Password: {1}", ADMIN_NAME, ADMIN_PASS);
            string setup_response = SetUp.Init(ADMIN_NAME, ADMIN_PASS, INIT_FILE_PATH, out NotificationManager notification,
                        out PurchaseActions purchaseActions, out UserActions userActions, out StoreActions storeActions);
            Console.WriteLine(setup_response);
            if (setup_response.Contains("Invalid operations File"))
            {
                Console.WriteLine(String.Format("Invalid operations File, response from setup = {0}", setup_response));
                Environment.Exit(1);
            }
            Actions.SetUp(purchaseActions, userActions, storeActions, notification);

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
