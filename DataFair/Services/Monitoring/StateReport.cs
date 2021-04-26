using Common;
using DataFair.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair.Services
{
    public class StateReport
    {
        public int Collectors;
        public int SessionsAvaliable;
        public int Orders;
        public int Messages;
        public int FailedMessages;
        public int Users;
        public int Chats;
        public StateReport(State state, ICommonWriter<Message> messagesWriter, ICommonWriter<User> usersWriter, ICommonWriter<Chat> chatsWriter)
        {
            Collectors = state.Collectors.Count;
            SessionsAvaliable = state.SessionStorages.Count;
            Orders = state.Orders.Count + state.MaxPriorityOrders.Count;
            Messages = messagesWriter.GetQueueCount();
            FailedMessages = messagesWriter.GetFailedQueueCount();
            Users = usersWriter.GetQueueCount();
            Chats = chatsWriter.GetQueueCount();
        }
    }
}
