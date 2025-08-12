using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Entities;
using Tingt.UnusualSpending.Ports.IRepository;

namespace Tingt.UnusualSpending.Infrastructure.InMemory
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly IEnumerable<User> _users;
        public InMemoryUserRepository(IEnumerable<User> users) => _users = users;
        public Task<IEnumerable<User>> GetAllUsersAsync() => Task.FromResult(_users);
    }
}
