using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services.Interface;

namespace Tingt.UnsualSpendingNotifier.Services.Implementation
{
    internal class UserRepository : IUserRepository
    {
        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
