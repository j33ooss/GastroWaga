using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GastroWaga.Domain.Entities
{
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "L";
        public string Category { get; set; } = "other";
        public int? NominalMl { get; set; }
        public double? DensityGml { get; set; }
        public double? EmptyBottleG { get; set; }
        public double? FullBottleG { get; set; }
        public CalcMode Mode { get; set; } = CalcMode.estimate;
        public string? Notes { get; set; }

        public ICollection<ItemAlias> Aliases { get; set; } = new List<ItemAlias>();
    }
}