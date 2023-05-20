using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sockets
{
    internal class Function
    {
        public IList<double[]> function { get; set; }
        public double a { get; set; }
        public double b { get; set; }
        public int segmentos { get; set; }
        public int threads { get; set; }
    }
}
