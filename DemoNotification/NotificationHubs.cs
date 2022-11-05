using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DemoNotification
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationHubs: Hub
    {
        public UserInfoInMemory _userInfoInMemory;

        public NotificationHubs(UserInfoInMemory userInfoInMemory)
        {
            _userInfoInMemory = userInfoInMemory;
        }
        
        public async Task Leave()
        {
            _userInfoInMemory.Remove(Context.User.Identity.Name);
            await Clients.AllExcept(new List<string> { Context.ConnectionId })
                .SendAsync("UserLeft", Context.User.Identity.Name);
        }

        
        public async Task Join()
        {
            if (!_userInfoInMemory.AddUpdate(Context.User.Identity.Name, Context.ConnectionId))
            {
                var list = _userInfoInMemory.GetAllUsersExceptThis(Context.User.Identity.Name).ToList();
                await Clients.AllExcept(new List<string> { Context.ConnectionId })
                    .SendAsync("NewOnlineUsers", _userInfoInMemory.GetUserInfo(Context.User.Identity.Name));
            }
            else
            {
                
            }
            
            await Clients.Client(Context.ConnectionId)
                .SendAsync("Joined", _userInfoInMemory.GetUserInfo(Context.User.Identity.Name));
            
            await Clients.Client(Context.ConnectionId)
                .SendAsync("OnlineUsers", _userInfoInMemory.GetAllUsersExceptThis(Context.User.Identity.Name));
        }

        public async Task SendAllNotifications()
        {
            await Clients.All.SendAsync("ReceiveNotification", Context.User.Identity.Name);
        }
        public Task SendDirectNotifications(string message, string targetUserName)
        {
            var userInfoSender = _userInfoInMemory.GetUserInfo(Context.User.Identity.Name);
            var userInfoReceiver = _userInfoInMemory.GetUserInfo(targetUserName);
            return Clients.Client(userInfoReceiver.ConnectionId).SendAsync("Notify", message,  userInfoSender);
        }




        

    }
}
