using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatas
{
    public class Funcion
    {
        public string Nombre { get; set; }
        public string TipoRetorno { get; set; }

        public List<(string tipo, string nombre)> Parametros { get; set; }
            = new List<(string tipo, string nombre)>();

        public int LineaInicio { get; set; }          // Línea del encabezado
        public int LineaCuerpoInicio { get; set; }    // Línea del '{'
        public int LineaCuerpoFin { get; set; }       // Línea del '}'
    }




}
