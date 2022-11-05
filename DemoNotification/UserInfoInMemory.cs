using System.Collections.Concurrent;

namespace DemoNotification
{
    public class UserInfoInMemory
    {
        private ConcurrentDictionary<string, UserInfo> 
            _onlineUserInfo { get; set; } = new ConcurrentDictionary<string, UserInfo>();

        public bool AddUpdate(string name, string connectionId)
        {
            var userAlreadyExists = _onlineUserInfo.ContainsKey(name);
            var userInfo = new UserInfo
            {
                ConnectionId = connectionId,
                UserName = name
            };
            _onlineUserInfo.AddOrUpdate(name, userInfo, (key, value) => userInfo);

            return userAlreadyExists;
        }
        
        public void Remove(string name)
        {
            UserInfo userinfo;
            _onlineUserInfo.TryGetValue(name, out userinfo);
           
        }
        public IEnumerable<UserInfo> GetAllUsersExceptThis(string username)
        {
            return _onlineUserInfo.Values.Where(x => x.UserName != username);
        }
        public UserInfo GetUserInfo(string username)
        {
            UserInfo userInfo;
            _onlineUserInfo.TryGetValue(username, out userInfo);
            return userInfo;
        }
    }


    
}
