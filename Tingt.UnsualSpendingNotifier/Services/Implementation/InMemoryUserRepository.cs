using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services.Interface;

namespace Tingt.UnsualSpendingNotifier.Services.Implementation
{


    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public InMemoryUserRepository(IEnumerable<User>? initial = null)
        {
            _users = (initial ?? Enumerable.Empty<User>()).ToList();
        }

        public Task<IEnumerable<User>> GetAllUsersAsync() => Task.FromResult<IEnumerable<User>>(_users);
    }
   
}
