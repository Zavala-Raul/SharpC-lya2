using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automatas
{
    internal sealed class Triplo
    {
        public Triplo(string operador, OperandoIntermedio objeto, OperandoIntermedio fuente,
            string tipoResultado, Cuadruplo origen)
        {
            Operador = operador; Objeto = objeto; Fuente = fuente;
            TipoResultado = tipoResultado; Origen = origen;
        }
        public string Operador { get; }
        public OperandoIntermedio Objeto { get; }
        public OperandoIntermedio Fuente { get; }
        public string TipoResultado { get; }
        public Cuadruplo Origen { get; }
    }

    internal sealed class InstruccionP
    {
        public InstruccionP(string operacion, OperandoIntermedio operando, OperandoIntermedio auxiliar,
            string tipoResultado, int cambioPila, Cuadruplo origen)
        {
            Operacion = operacion; Operando = operando; Auxiliar = auxiliar;
            TipoResultado = tipoResultado; CambioPila = cambioPila; Origen = origen;
        }
        public string Operacion { get; }
        public OperandoIntermedio Operando { get; }
        public OperandoIntermedio Auxiliar { get; }
        public string TipoResultado { get; }
        public int CambioPila { get; }
        public Cuadruplo Origen { get; }
    }

    internal sealed class NotacionExpresion
    {
        public NotacionExpresion(ExpresionFuente origen, string infija, string prefija, string postfija)
        { Origen = origen; Infija = infija; Prefija = prefija; Postfija = postfija; }
        public ExpresionFuente Origen { get; }
        public string Infija { get; }
        public string Prefija { get; }
        public string Postfija { get; }
    }

    internal sealed class ErrorRepresentacion : Exception
    {
        public ErrorRepresentacion(string representacion, Cuadruplo origen, string mensaje)
            : base(representacion + ": " + mensaje) { Linea = origen?.Linea ?? 1; }
        public int Linea { get; }
    }

    internal sealed class ResultadoRepresentaciones
    {
        public ResultadoRepresentaciones(List<Triplo> triplos, List<InstruccionP> codigoP,
            List<NotacionExpresion> notaciones, List<(int Linea, string Mensaje)> errores)
        {
            Triplos = triplos.AsReadOnly(); CodigoP = codigoP.AsReadOnly();
            Notaciones = notaciones.AsReadOnly(); Errores = errores.AsReadOnly();
        }
        public IReadOnlyList<Triplo> Triplos { get; }
        public IReadOnlyList<InstruccionP> CodigoP { get; }
        public IReadOnlyList<NotacionExpresion> Notaciones { get; }
        public IReadOnlyList<(int Linea, string Mensaje)> Errores { get; }
    }

    internal static class ConstructorRepresentaciones
    {
        public static ResultadoRepresentaciones Construir(ResultadoCodigoIntermedio resultado)
        {
            var errores = new List<(int Linea, string Mensaje)>(resultado.Errores);
            if (errores.Count == 0)
            {
                try
                {
                    var triplos = new GeneradorTriplos().Generar(resultado.Instrucciones);
                    var codigoP = new GeneradorCodigoP().Generar(resultado.Instrucciones);
                    var notaciones = new FormateadorNotaciones().Generar(resultado);
                    return new ResultadoRepresentaciones(triplos, codigoP, notaciones, errores);
                }
                catch (ErrorRepresentacion error) { errores.Add((error.Linea, error.Message)); }
            }
            return new ResultadoRepresentaciones(new List<Triplo>(), new List<InstruccionP>(),
                new List<NotacionExpresion>(), errores);
        }

        public static string TextoP(IReadOnlyList<InstruccionP> instrucciones, FormateadorCodigoIntermedio formato)
        {
            var texto = new StringBuilder();
            foreach (var i in instrucciones)
            {
                if (i.Operacion == "label") texto.Append(formato.Operando(i.Operando)).Append(':');
                else
                {
                    texto.Append(i.Operacion);
                    if (i.Operando != null) texto.Append(' ').Append(formato.Operando(i.Operando));
                    if (i.Auxiliar != null) texto.Append(", ").Append(formato.Operando(i.Auxiliar));
                }
                // RAZL-001 texto.Append("  // ").Append(i.TipoResultado);
                if (i.Operacion == "store" && i.Operando.Tipo != i.TipoResultado)
                    texto.Append(" -> ").Append(i.Operando.Tipo);
                texto.AppendLine();
            }
            return texto.ToString();
        }

        internal static bool Binario(string op) => new[]
            { "+", "-", "*", "/", "^", "==", "<>", "<", ">", "<=", ">=", "&&", "||" }.Contains(op);
    }
}
