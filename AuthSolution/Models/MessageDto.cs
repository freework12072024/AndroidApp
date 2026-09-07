using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSolution.Models
{
    public class MessageDto
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsMine { get; set; }
    }
}
