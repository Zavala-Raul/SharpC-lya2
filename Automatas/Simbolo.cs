using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatas
{
    public class Simbolo
    {
        public int Numero { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }   // ENT, DEC, TXT, BOOL, CAR, VAC
        public string Valor { get; set; }  // literal o expresión
    }

}
