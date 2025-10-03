using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GastroWaga.Domain.Entities
{
    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string Warehouse { get; set; } = "";
        public string User { get; set; } = "";
        public SessionStatus Status { get; set; } = SessionStatus.Open;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
    }
}