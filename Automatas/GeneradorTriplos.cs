using System.Collections.Generic;

namespace Automatas
{
    internal sealed class GeneradorTriplos
    {
        public List<Triplo> Generar(IReadOnlyList<Cuadruplo> instrucciones)
        {
            var resultado = new List<Triplo>();
            int numero = 0;
            foreach (var q in instrucciones)
            {
                if (ConstructorRepresentaciones.Binario(q.Operacion) || q.Operacion == "!")
                {
                   
                    var acumulador = OperandoIntermedio.Temporal("r" + (++numero), "ACUMULADOR");
                    resultado.Add(new Triplo("", acumulador, q.Fuente1, q.Fuente1.Tipo, q));
                    resultado.Add(new Triplo(q.Operacion, acumulador, q.Fuente2, q.TipoResultado, q));
                    resultado.Add(new Triplo("=", q.Destino, acumulador, q.TipoResultado, q));
                    continue;
                }
                switch (q.Operacion)
                {
                    case "LLAMAR":
                        resultado.Add(new Triplo("PREPARAR_LLAMADA", q.Fuente1, q.Fuente2, "VAC", q));
                        resultado.Add(new Triplo("LLAMAR", q.Destino, q.Fuente1, q.TipoResultado, q));
                        break;
                    case "POS": case "IMPREP":
                        resultado.Add(new Triplo(q.Operacion, q.Fuente1, q.Fuente2, q.TipoResultado, q));
                        break;
                    case "=": case "ETIQUETA": case "SALTO": case "SALTO_FALSO":
                    case "FUNCION": case "PARAMETRO": case "ARGUMENTO": case "RETORNO":
                    case "FIN_FUNCION": case "LEER": case "IMP": case "LIMP": case "TECL":
                    case "FONCOL": case "TXTCOL": case "SLTLN":
                        resultado.Add(new Triplo(q.Operacion, q.Destino, q.Fuente1, q.TipoResultado, q));
                        break;
                    default: throw new ErrorRepresentacion("Triplos", q, "Operación no reconocida: " + q.Operacion);
                }
            }
            return resultado;
        }
    }
}
