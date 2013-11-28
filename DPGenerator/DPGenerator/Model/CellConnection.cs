using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPGenerator.Model
{
    class CellConnection
    {
        public Cell Up { get; set; }
        public Cell Middle { get; set; }
        public Cell Down { get; set; }
    }
}
