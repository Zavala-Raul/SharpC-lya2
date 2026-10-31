using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automatas
{
    internal sealed class FormateadorCodigoIntermedio
    {
        private readonly IReadOnlyList<Cuadruplo> instrucciones;
        private readonly HashSet<string> nombresRepetidos;

        public FormateadorCodigoIntermedio(IReadOnlyList<Cuadruplo> instrucciones)
        {
            this.instrucciones = instrucciones;
            nombresRepetidos = new HashSet<string>(instrucciones
                .SelectMany(q => new[] { q.Destino, q.Fuente1, q.Fuente2 })
                .Where(o => o?.Simbolo != null).Select(o => o.Simbolo).Distinct()
                .GroupBy(s => s.Nombre).Where(g => g.Count() > 1).Select(g => g.Key));
        }

        public string Operando(OperandoIntermedio operando) => operando == null ? "—" :
            operando.Simbolo != null && nombresRepetidos.Contains(operando.Simbolo.Nombre)
                ? operando.Texto + "#" + operando.Simbolo.Numero : operando.Texto;

        public string ComoTexto()
        {
            var texto = new StringBuilder();
            foreach (var q in instrucciones)
            {
                if (q.FuncionContenedora != null && q.Operacion != "FUNCION" &&
                    q.Operacion != "FIN_FUNCION" && q.Operacion != "ETIQUETA") texto.Append("    ");
                texto.Append(Instruccion(q));
                if (q.Destino != null && q.TipoResultado != "VAC" &&
                    (q.Destino.EsTemporal || q.Destino.Simbolo != null) && q.Destino.Tipo != q.TipoResultado)
                    texto.Append("  // resultado ").Append(q.TipoResultado).Append(" -> ").Append(q.Destino.Tipo);
                texto.AppendLine();
            }
            return texto.ToString();
        }

        private string Instruccion(Cuadruplo q)
        {
            string d = Operando(q.Destino), a = Operando(q.Fuente1), b = Operando(q.Fuente2);
            switch (q.Operacion)
            {
                case "=": return d + " = " + a;
                case "+": case "-": case "*": case "/": case "^":
                case "==": case "<>": case "<": case ">": case "<=": case ">=":
                    return d + " = " + a + " " + q.Operacion + " " + b;
                case "&&": return d + " = " + a + " & " + b;
                case "||": return d + " = " + a + " | " + b;
                case "!": return d + " = !" + a;
                case "ETIQUETA": return d + ":";
                case "SALTO": return "goto " + d;
                case "SALTO_FALSO": return "ifFalse " + a + " goto " + d;
                case "FUNCION": return "func " + d + " -> " + q.Destino.Tipo /*+
                    "  // padre léxico: " + (q.Fuente1 == null ? "INI" : a)*/;
                case "FIN_FUNCION": return "endfunc " + d;
                case "PARAMETRO": return "param " + a + ": " + q.Destino.Tipo + " " + d;
                case "ARGUMENTO": return "arg " + d + ", " + a;
                case "LLAMAR": return (q.Destino == null ? "" : d + " = ") + "call " + a + ", " + b;
                case "RETORNO": return "return" + (q.Fuente1 == null ? "" : " " + a);
                case "LEER": return d + " = LEER()";
                case "IMP": return "IMP(" + a + ")";
                case "LIMP": case "TECL": return q.Operacion + "()";
                case "FONCOL": case "TXTCOL": case "SLTLN": return q.Operacion + "(" + a + ")";
                case "POS": case "IMPREP": return q.Operacion + "(" + a + ", " + b + ")";
                default: throw new InvalidOperationException("Falta formato textual para " + q.Operacion);
            }
        }
    }
}
