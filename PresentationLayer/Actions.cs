using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.NotificationsCenter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationsManagment;
using ServiceLayer.Services;

namespace PresentationLayer
{
    public class Actions
    {
        private static Actions instance = null;

        internal UserActions User { get; }

        internal StoreActions Store { get; }

        internal PurchaseActions Purchase { get; }

        internal NotificationManager Notification { get; }

        internal static void SetUp(PurchaseActions pActions, UserActions uActions, StoreActions sActions, NotificationManager notification)
        {
            instance = new Actions(pActions, uActions, sActions, notification);
        }

        private Actions(PurchaseActions pActions, UserActions uActions, StoreActions sActions, NotificationManager notification)
        {
            Purchase = pActions;
            User = uActions;
            Store = sActions;
            Notification = notification;
        }

        internal static Actions GetActions() => instance;
    }
}
