using AuthSolution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSolution.Services
{
    public class ChatService
    {
        private readonly ApiClient _api;

        public ChatService(ApiClient api) => _api = api;

        public Task<List<MessageDto>?> GetChatAsync(int currentUserId, int chatUserId) =>
            _api.GetAsync<List<MessageDto>>($"api/Chat/conversation/{currentUserId}/{chatUserId}");
        public Task<MessageDto?> PostChatAsync(MessageDto messageDto) =>
            _api.PostAsync<MessageDto>($"api/Chat/send",messageDto);
    }

}
