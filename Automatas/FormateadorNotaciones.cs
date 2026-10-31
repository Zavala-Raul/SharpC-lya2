using System.Collections.Generic;
using System.Linq;

namespace Automatas
{
    internal sealed class FormateadorNotaciones
    {
        public List<NotacionExpresion> Generar(ResultadoCodigoIntermedio resultado)
        {
            var formato = new FormateadorCodigoIntermedio(resultado.Instrucciones);
            var filas = new List<NotacionExpresion>();
            foreach (var fuente in resultado.Expresiones.OrderBy(e => e.PosicionToken))
            {
                string infija = Formatear(fuente.Expresion, 0, formato);
                string prefija = Formatear(fuente.Expresion, 1, formato);
                string postfija = Formatear(fuente.Expresion, 2, formato);
                if (fuente.Destino != null)
                {
                    string destino = formato.Operando(fuente.Destino);
                    infija = destino + " = " + infija;
                    prefija = "= " + destino + " " + prefija;
                    postfija = destino + " " + postfija + " =";
                }
                filas.Add(new NotacionExpresion(fuente, infija, prefija, postfija));
            }
            return filas;
        }

        private static string Formatear(ExpresionIntermedia e, int modo, FormateadorCodigoIntermedio formato)
        {
            if (e.Operando != null) return formato.Operando(e.Operando);
            if (e.Argumentos != null)
            {
                string nombre = e.Funcion == null ? e.Operador : formato.Operando(OperandoIntermedio.Variable(e.Funcion));
                var argumentos = e.Argumentos.Select(a => Formatear(a, modo, formato)).ToList();
                if (modo == 0) return nombre + "(" + string.Join(", ", argumentos) + ")";
                // La aridad explícita permite distinguir llamadas anidadas y sin argumentos.
                string llamada = "call[" + nombre + "/" + argumentos.Count + "]";
                var partes = new List<string>(argumentos);
                if (modo == 1) partes.Insert(0, llamada); else partes.Add(llamada);
                return string.Join(" ", partes);
            }
            string op = e.Operador == "&&" ? "&" : e.Operador == "||" ? "|" : e.Operador;
            string derecha = Formatear(e.Derecha, modo, formato);
            if (e.Izquierda == null)
                return modo == 0 ? "!(" + derecha + ")" : modo == 1 ? "! " + derecha : derecha + " !";
            string izquierda = Formatear(e.Izquierda, modo, formato);
            // Infija completamente parentizada: expresa exactamente la agrupación del árbol,
            // incluso para potencia y la precedencia particular de ! en SharpC.
            if (modo == 0) return "(" + izquierda + " " + op + " " + derecha + ")";
            if (modo == 1) return op + " " + izquierda + " " + derecha;
            return izquierda + " " + derecha + " " + op;
        }
    }
}
