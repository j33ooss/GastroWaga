using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GastroWaga.Domain.Entities
{
    public class CategoryDensity
    {
        public string Category { get; set; } = "other";
        public double DefaultDensityGml { get; set; } = 1.00;
    }
}
