using System.Collections.Generic;

namespace Automatas
{
    internal sealed class GeneradorCodigoP
    {
        public List<InstruccionP> Generar(IReadOnlyList<Cuadruplo> instrucciones)
        {
            var resultado = new List<InstruccionP>();
            foreach (var q in instrucciones)
            {
                int altura = 0;
                void Emitir(string op, OperandoIntermedio a = null, OperandoIntermedio b = null,
                    int cambio = 0, string tipo = null)
                {
                    altura += cambio;
                    if (altura < 0) throw new ErrorRepresentacion("Código P", q, "Subdesbordamiento de pila.");
                    resultado.Add(new InstruccionP(op, a, b, tipo ?? q.TipoResultado, cambio, q));
                }
                void Cargar(OperandoIntermedio operando) => Emitir("load", operando, cambio: 1, tipo: operando.Tipo);
                void Guardar() => Emitir("store", q.Destino, cambio: -1);
                if (ConstructorRepresentaciones.Binario(q.Operacion))
                {
                    Cargar(q.Fuente1); Cargar(q.Fuente2);
                    Emitir(OperacionP(q.Operacion), cambio: -1); Guardar();
                }
                else switch (q.Operacion)
                {
                    case "=": Cargar(q.Fuente1); Guardar(); break;
                    case "!": Cargar(q.Fuente1); Emitir("not"); Guardar(); break;
                    case "ETIQUETA": Emitir("label", q.Destino); break;
                    case "SALTO": Emitir("jmp", q.Destino); break;
                    case "SALTO_FALSO": Cargar(q.Fuente1); Emitir("jfalse", q.Destino, cambio: -1); break;
                    case "FUNCION": Emitir("func", q.Destino, q.Fuente1); break;
                    case "FIN_FUNCION": Emitir("endfunc", q.Destino); break;
                    case "PARAMETRO": Emitir("param", q.Destino, q.Fuente1); break;
                    case "ARGUMENTO": Cargar(q.Fuente1); Emitir("arg", q.Destino, cambio: -1); break;
                    case "LLAMAR":
                        Emitir("call", q.Fuente1, q.Fuente2, q.Destino == null ? 0 : 1);
                        if (q.Destino != null) Guardar();
                        break;
                    case "RETORNO":
                        if (q.Fuente1 == null) Emitir("ret");
                        else { Cargar(q.Fuente1); Emitir("retv", cambio: -1); }
                        break;
                    case "LEER": Emitir("read", cambio: 1); Guardar(); break;
                    case "IMP": Cargar(q.Fuente1); Emitir("print", cambio: -1); break;
                    case "LIMP": Emitir("limp"); break;
                    case "TECL": Emitir("tecl"); break;
                    case "FONCOL": case "TXTCOL": case "SLTLN":
                        Cargar(q.Fuente1); Emitir(q.Operacion.ToLowerInvariant(), cambio: -1); break;
                    case "POS": case "IMPREP":
                        Cargar(q.Fuente1); Cargar(q.Fuente2);
                        Emitir(q.Operacion.ToLowerInvariant(), cambio: -2); break;
                    default: throw new ErrorRepresentacion("Código P", q, "Operación no reconocida: " + q.Operacion);
                }
                // Cada cuádruplo deja la pila de evaluación vacía. Las entradas de
                // etiquetas y funciones tienen altura 0, incluso en ramas y recursión.
                if (altura != 0) throw new ErrorRepresentacion("Código P", q, "Pila no balanceada.");
            }
            return resultado;
        }

        private static string OperacionP(string op)
        {
            switch (op)
            {
                case "+": return "add"; case "-": return "sub"; case "*": return "mul";
                case "/": return "div"; case "^": return "pow"; case "==": return "eq";
                case "<>": return "ne"; case "<": return "lt"; case ">": return "gt";
                case "<=": return "le"; case ">=": return "ge"; case "&&": return "and";
                case "||": return "or";
                default: return op;
            }
        }
    }
}
