using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Models
{
    public class DamageObject
    {
        public string Victim { get; set; }
        public string Attacker { get; set; }
        public int Damage { get; set; }
        public Spell Spell { get; set; }
    }
}
