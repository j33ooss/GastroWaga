using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GastroWaga.Domain.Entities
{
    public class ChangeLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public Guid? LineId { get; set; }
        public string Action { get; set; } = "";   // Add/Update/Delete/Reopen
        public string? By { get; set; }
        public DateTime At { get; set; } = DateTime.UtcNow;
        public string? Before { get; set; }
        public string? After { get; set; }
    }
}