using AuthSolution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AuthSolution.Services
{
    public class UserService
    {
        private readonly ApiClient _api;

        public UserService(ApiClient api) => _api = api;

        // GET users
        public Task<List<UserDto>?> GetUsersAsync() =>
            _api.GetAsync<List<UserDto>>("api/User");
    }
}
