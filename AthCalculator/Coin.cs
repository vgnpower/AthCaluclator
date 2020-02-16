using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthCalculator
{
    public class Coin
    {
        public string Name { get; set; }
        public double Ath { get; set; }
        public double CurrentPrice { get; set; }
        public double perc_drop { get; set; }
        public double perc_to_ath { get; set; }
    }
}
