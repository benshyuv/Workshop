using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DomainLayer.NotificationsCenter;

namespace NotificationsManagment
{
    public class NotificationManager : ICommunicationNotificationAlerter
    {
        //SessionID -> WebSocket
        private ConcurrentDictionary<Guid, WebSocket> SessionSockets { get; set; }

        //SessionID -> WebSocket For statistics only
        private ConcurrentDictionary<Guid, WebSocket> StatsSessionSockets { get; set; }

        //UserID -> SessionID
        private ConcurrentDictionary<Guid, Guid> UserSessions { get; set; }
        private ConcurrentDictionary<Guid, Guid> AdminSessions { get; set; }

        //UserID -> has pending messages
        private ConcurrentDictionary<Guid, bool> UserPending { get; set; }


        public NotificationManager()
        {
            SessionSockets = new ConcurrentDictionary<Guid, WebSocket>();
            UserSessions = new ConcurrentDictionary<Guid, Guid>();
            AdminSessions = new ConcurrentDictionary<Guid, Guid>();
            UserPending = new ConcurrentDictionary<Guid, bool>();
            StatsSessionSockets = new ConcurrentDictionary<Guid, WebSocket>();
        }

        public async Task HandleWebSocketIncoming(HttpContext context)
        {
            await HandleWebSocketConnection(context);
        }

        public async Task HandleWebSocketIncomingStats(HttpContext context)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

            Console.WriteLine(String.Format("wss connected Stats sessionID = {0} socket = {1}", context.Session.Id, webSocket.GetHashCode()));
            StatsSessionSockets.AddOrUpdate(Guid.Parse(context.Session.Id), webSocket, (key, _) => webSocket);

            var msg = Encoding.UTF8.GetBytes(String.Format("Stats socket {0}", context.Session.Id));

            var buffer = new ArraySegment<byte>(msg, 0, msg.Length);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("sent async connection");
            var buf2 = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buf2), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buf2), CancellationToken.None);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            HandleStatsWebSocketClosing(context);
        }

        private async Task HandleWebSocketConnection(HttpContext context)
        {
           
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
           
            Console.WriteLine(String.Format("wss connected sessionID = {0} socket = {1}",context.Session.Id, webSocket.GetHashCode()));
            SessionSockets.AddOrUpdate(Guid.Parse(context.Session.Id), webSocket, (key, _) => webSocket);
            
            var msg = Encoding.UTF8.GetBytes(String.Format("{0}",context.Session.Id));

            var buffer = new ArraySegment<byte>(msg, 0, msg.Length);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("sent");
            var buf2 = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buf2), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buf2), CancellationToken.None);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            HandleWebSocketClosing(context);

        }

        public void HandleWebSocketClosing(HttpContext context)
        {
            SessionSockets.TryRemove(Guid.Parse(context.Session.Id), out _);
            Console.WriteLine(String.Format("closed wss for session {0}", Guid.Parse(context.Session.Id)));
        }

        public void HandleStatsWebSocketClosing(HttpContext context)
        {
            StatsSessionSockets.TryRemove(Guid.Parse(context.Session.Id), out _);
            Console.WriteLine(String.Format("closed stats wss for session {0}", Guid.Parse(context.Session.Id)));
        }

        public async Task<bool> AssociateUserToSession(Guid userID, Guid sessionID, bool admin)
        {

            Console.WriteLine(String.Format("TRY Assosiating session {0} to user {1}", sessionID, userID));

            if (UserSessions.TryGetValue(userID, out Guid oldSessionID))
            {
                SessionSockets.Remove(oldSessionID, out _);//remove stale socket - TODO check
            }

            UserSessions.AddOrUpdate(userID, sessionID, (key, _) => sessionID);
            if (admin)
            {
                AdminSessions.AddOrUpdate(userID, sessionID, (key, _) => sessionID);
            }

            if (!SessionSockets.TryGetValue(sessionID, out WebSocket socket))
            {
                Console.WriteLine(String.Format("No websocket for session {0} to user {1}", sessionID, userID));
                return false;// no socket assigned to session
            }

            if (UserPending.GetValueOrDefault(userID, false))
            {
                byte[] msg = Encoding.UTF8.GetBytes("Alert");
                ArraySegment<byte> buffer = new ArraySegment<byte>(msg, 0, msg.Length);
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                UserPending.TryUpdate(userID, false, true);
            }
            Console.WriteLine(string.Format("DONE Assosiating session {0} to user {1}", sessionID, userID));
            return true;
        }

       

        public bool DisassociateUser(Guid userID)
        {
            if (!UserSessions.TryRemove(userID, out Guid found))
            {
                return false;// No session associated
            }
            AdminSessions.TryRemove(userID, out _);
            SessionSockets.TryRemove(found, out _);//remove stale socket - TODO check
            Console.WriteLine(string.Format("Dissacociated user {0}", userID));
            return true;
        }

        public async Task AlertUser(Guid userID)
        {
            if (UserSessions.TryGetValue(userID, out Guid sessionID))
            {
                if (SessionSockets.TryGetValue(sessionID, out WebSocket socket))
                {
                    Console.WriteLine(string.Format("allertin session {0} user {1}", sessionID, userID));
                    byte[] msg = Encoding.UTF8.GetBytes("Alert");
                    ArraySegment<byte> buffer = new ArraySegment<byte>(msg, 0, msg.Length);
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    UserPending.AddOrUpdate(userID, false, (key, _) => false);
                    return;
                }
            }
            UserPending.AddOrUpdate(userID, true, (key, _) => true);
        }

        public async Task UpdateAdminStats(Dictionary<DateTime, int[]> statistics)
        {
            DateTime today = DateTime.Today;
            string statsString = JsonConvert.SerializeObject(statistics);
            //string statString = string.Format('"date": {0}, "values": [{1}]"', today.ToShortDateString(), string.Join(", ", statistics[today]));
            Console.WriteLine(statsString);
            byte[] msg = Encoding.UTF8.GetBytes("stats:"+ statsString);// TODO: test data
            // "stats: { date: dd/mm/yyyy, stats: [0, 1, 2, 3, 4] }"
            ArraySegment<byte> buffer = new ArraySegment<byte>(msg, 0, msg.Length);

            foreach (KeyValuePair<Guid, Guid> adminSession in AdminSessions)
            {
                if (StatsSessionSockets.TryGetValue(adminSession.Value, out WebSocket socket))
                {
                    Console.WriteLine(string.Format("update stats for session {0} admin {1}", adminSession.Value, adminSession.Key));
                    
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
