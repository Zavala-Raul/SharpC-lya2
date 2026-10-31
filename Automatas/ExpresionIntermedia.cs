using System.Collections.Generic;

namespace Automatas
{
    internal sealed class ExpresionIntermedia
    {
        public OperandoIntermedio Operando;
        public string Operador;
        public string Tipo;
        public ExpresionIntermedia Izquierda, Derecha;
        public Simbolo Funcion;
        public List<ExpresionIntermedia> Argumentos;
        public int Posicion;
        public bool ContieneLlamada => Funcion != null ||
            (Izquierda != null && Izquierda.ContieneLlamada) || (Derecha != null && Derecha.ContieneLlamada);
    }

    internal sealed class ExpresionFuente
    {
        public ExpresionFuente(ExpresionIntermedia expresion, OperandoIntermedio destino,
            string contexto, int linea, int posicion, Simbolo funcion)
        {
            Expresion = expresion; Destino = destino; Contexto = contexto;
            Linea = linea; PosicionToken = posicion; FuncionContenedora = funcion;
        }
        public ExpresionIntermedia Expresion { get; }
        public OperandoIntermedio Destino { get; }
        public string Contexto { get; }
        public int Linea { get; }
        public int PosicionToken { get; }
        public Simbolo FuncionContenedora { get; }
    }
}
