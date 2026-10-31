using System.Collections.Generic;

namespace Automatas
{
    internal class OperandoIntermedio
    {
        private OperandoIntermedio(string texto, string tipo, Simbolo simbolo, bool temporal)
        {
            Texto = texto;
            Tipo = tipo;
            Simbolo = simbolo;
            EsTemporal = temporal;
        }

        public string Texto { get; }
        public string Tipo { get; }
        public Simbolo Simbolo { get; }
        public bool EsTemporal { get; }
        public static OperandoIntermedio Variable(Simbolo simbolo) =>
            new OperandoIntermedio(simbolo.Nombre, simbolo.Tipo, simbolo, false);
        public static OperandoIntermedio Literal(string texto, string tipo) =>
            new OperandoIntermedio(texto, tipo, null, false);
        public static OperandoIntermedio Temporal(string nombre, string tipo) =>
            new OperandoIntermedio(nombre, tipo, null, true);
        public static OperandoIntermedio Etiqueta(string nombre) =>
            new OperandoIntermedio(nombre, "ETIQUETA", null, false);
    }

    internal class Cuadruplo
    {
        public Cuadruplo(OperandoIntermedio destino, OperandoIntermedio fuente1,
            OperandoIntermedio fuente2, string operacion, string tipoResultado, int posicion, int linea,
            Simbolo funcionContenedora = null)
        {
            Destino = destino;
            Fuente1 = fuente1;
            Fuente2 = fuente2;
            Operacion = operacion;
            TipoResultado = tipoResultado;
            PosicionToken = posicion;
            Linea = linea;
            FuncionContenedora = funcionContenedora;
        }

        public OperandoIntermedio Destino { get; }
        public OperandoIntermedio Fuente1 { get; }
        public OperandoIntermedio Fuente2 { get; }
        public string Operacion { get; }
        public string TipoResultado { get; }
        public int PosicionToken { get; }
        public int Linea { get; }
        public Simbolo FuncionContenedora { get; }
    }

    internal class ResultadoCodigoIntermedio
    {
        public ResultadoCodigoIntermedio(List<Cuadruplo> instrucciones,
            List<(int Linea, string Mensaje)> errores, List<ExpresionFuente> expresiones = null)
        {
            Instrucciones = instrucciones.AsReadOnly();
            Errores = errores.AsReadOnly();
            Expresiones = (expresiones ?? new List<ExpresionFuente>()).AsReadOnly();
        }

        public IReadOnlyList<Cuadruplo> Instrucciones { get; }
        public IReadOnlyList<(int Linea, string Mensaje)> Errores { get; }
        public IReadOnlyList<ExpresionFuente> Expresiones { get; }
    }
}
