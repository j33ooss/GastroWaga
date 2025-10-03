using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GastroWaga.Domain.Entities
{
    public class ItemAlias
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ItemId { get; set; }
        public string Code { get; set; } = "";     // EAN/PLU
        public string Type { get; set; } = "EAN";  // EAN / PLU

        public Item? Item { get; set; }
    }
}