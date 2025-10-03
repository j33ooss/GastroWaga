using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GastroWaga.Domain.Entities
{
    public class Line
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public Guid ItemId { get; set; }

        public double GrossG { get; set; }         // odczyt brutto z wagi
        public double EmptyGUsed { get; set; }     // użyte TARE pustej
        public double ContentG { get; set; }       // wyliczona masa zawartości
        public double DensityGmlUsed { get; set; } // gęstość użyta do przeliczenia
        public double Liters { get; set; }         // wynik w L (0.001)
        public CalcMode ModeUsed { get; set; }     // precyzyjny/estimate

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? Note { get; set; }
    }
}