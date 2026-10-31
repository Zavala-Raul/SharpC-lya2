using System;
using System.Collections.Generic;
using Expresion = Automatas.ExpresionIntermedia;

namespace Automatas
{
    internal class GeneradorCodigoIntermedio
    {
        public ResultadoCodigoIntermedio Generar(VerificadorTipos analisis)
        {
            var errores = new List<(int Linea, string Mensaje)>();  
            var vacio = new List<Cuadruplo>();
            if (analisis == null || analisis.TablaAmbitos == null || analisis.TokensVerificados == null ||
                analisis.Errores.Count != 0)
            {
                errores.Add((1, "GENERACIÓN BLOQUEADA: falta un análisis válido."));
                return new ResultadoCodigoIntermedio(vacio, errores);
            }
            var tokens = analisis.TokensVerificados;
            var sintaxis = new AnalizadorSintacticoLl1(TablaSintacticaSharpC.CrearTablaCompleta())
                .Analizar(tokens, tokens.Count == 0 ? 1 : tokens[tokens.Count - 1].Linea);
            if (sintaxis.Count != 0) return new ResultadoCodigoIntermedio(vacio, sintaxis);
            var sesion = new Sesion(analisis);
            try
            {
                sesion.Bloque(2, tokens.Count - 1, null);
                return new ResultadoCodigoIntermedio(sesion.Instrucciones, errores, sesion.Expresiones);
            }
            catch (ErrorGeneracion error)
            {
                errores.Add((error.Linea, "CÓDIGO INTERMEDIO: " + error.Message));
                return new ResultadoCodigoIntermedio(vacio, errores);
            }
        }

        private sealed class ErrorGeneracion : Exception
        {
            public int Linea { get; }
            public ErrorGeneracion(int linea, string mensaje) : base(mensaje) { Linea = linea; }
        }

        private sealed class Sesion
        {
            private readonly VerificadorTipos analisis;
            private readonly List<(string Tipo, string Valor, int Linea)> tokens;
            private readonly int[] cierres;
            private int temporal, etiqueta;
            private Simbolo funcionActual;
            public List<Cuadruplo> Instrucciones { get; } = new List<Cuadruplo>();
            public List<ExpresionFuente> Expresiones { get; } = new List<ExpresionFuente>();

            private Expresion Registrar(Expresion expresion, string contexto, OperandoIntermedio destino = null)
            {
                Expresiones.Add(new ExpresionFuente(expresion, destino, contexto,
                    tokens[expresion.Posicion].Linea, expresion.Posicion, funcionActual));
                return expresion;
            }
            private Expresion Fuente(int inicio, int fin, string contexto) =>
                Registrar(ExpresionRango(inicio, fin), contexto);
            private void RegistrarLlamada(string nombre, List<Expresion> argumentos, int p, string contexto,
                OperandoIntermedio destino = null)
            {
                Registrar(new Expresion { Operador = nombre, Argumentos = argumentos,
                    Posicion = p, Tipo = destino?.Tipo ?? "VAC" }, contexto, destino);
            }

            public Sesion(VerificadorTipos analisis)
            {
                this.analisis = analisis;
                tokens = analisis.TokensVerificados;
                cierres = new int[tokens.Count];
                var pila = new Stack<int>();
                for (int i = 0; i < tokens.Count; i++)
                    if (Es(i, "CE1") || Es(i, "CE3")) pila.Push(i);
                    else if (Es(i, "CE2") || Es(i, "CE4")) cierres[pila.Pop()] = i;
            }

            private bool Es(int i, string tipo) => i < tokens.Count && tokens[i].Tipo == tipo;
            private static bool EsTipo(string tipo) => tipo == "PR23" || tipo == "PR24" ||
                tipo == "PR25" || tipo == "PR26" || tipo == "PR27";
            private OperandoIntermedio Temporal(string tipo) => OperandoIntermedio.Temporal("t" + (++temporal), tipo);
            private OperandoIntermedio Etiqueta() => OperandoIntermedio.Etiqueta("L" + (++etiqueta));
            private static OperandoIntermedio Numero(int numero) => OperandoIntermedio.Literal(numero.ToString(), "ENT");
            private ErrorGeneracion Error(int p, string mensaje) => new ErrorGeneracion(tokens[p].Linea, mensaje);
            private OperandoIntermedio Variable(int p)
            {
                var simbolo = analisis.TablaAmbitos.SimbolosPorToken[p];
                if (simbolo == null) throw Error(p, "Identificador sin declaración: " + tokens[p].Valor);
                return OperandoIntermedio.Variable(simbolo);
            }
            private void Agregar(string op, OperandoIntermedio destino, OperandoIntermedio a,
                OperandoIntermedio b, int p, string tipo = "VAC")
            {
                Instrucciones.Add(new Cuadruplo(destino, a, b, op, tipo, p, tokens[p].Linea, funcionActual));
            }
            private void Marcar(OperandoIntermedio lugar, int p) => Agregar("ETIQUETA", lugar, null, null, p);
            private void Saltar(OperandoIntermedio lugar, int p) => Agregar("SALTO", lugar, null, null, p);
            private int PuntoComa(int i) { while (!Es(i, "CE8")) i++; return i; }

            public void Bloque(int inicio, int fin, OperandoIntermedio salida)
            {
                for (int i = inicio; i < fin;)
                {
                    string tipo = tokens[i].Tipo;
                    int p = i;
                    if (EsTipo(tipo) || tipo == "IDV")
                    {
                        int punto = PuntoComa(i);
                        Asignar(i, punto);
                        i = punto + 1;
                    }
                    else if (tipo == "PR11")
                    {
                        int cierreCond = cierres[i + 1], llave = cierreCond + 1, cierre = cierres[llave];
                        var no = Etiqueta();
                        Agregar("SALTO_FALSO", no, Emitir(Fuente(i + 2, cierreCond, "Condición SI")), null, p);
                        Bloque(llave + 1, cierre, salida);
                        i = cierre + 1;
                        if (Es(i, "PR12"))
                        {
                            var terminar = Etiqueta();
                            Saltar(terminar, p); Marcar(no, p);
                            Bloque(i + 2, cierres[i + 1], salida);
                            i = cierres[i + 1] + 1;
                            Marcar(terminar, p);
                        }
                        else Marcar(no, p);
                    }
                    else if (tipo == "PR15")
                    {
                        int cierreCond = cierres[i + 1], llave = cierreCond + 1;
                        var repetir = Etiqueta(); var terminar = Etiqueta();
                        Marcar(repetir, p);
                        Agregar("SALTO_FALSO", terminar, Emitir(Fuente(i + 2, cierreCond, "Condición MIENT")), null, p);
                        Bloque(llave + 1, cierres[llave], terminar);
                        Saltar(repetir, p); Marcar(terminar, p);
                        i = cierres[llave] + 1;
                    }
                    else if (tipo == "PR16")
                    {
                        int cierre = cierres[i + 1], parentesis = cierre + 2;
                        var repetir = Etiqueta(); var terminar = Etiqueta();
                        Marcar(repetir, p);
                        Bloque(i + 2, cierre, terminar);
                        Agregar("SALTO_FALSO", repetir,
                            Emitir(Fuente(parentesis + 1, cierres[parentesis], "Condición HASTA")), null, p);
                        Marcar(terminar, p);
                        i = cierres[parentesis] + 2;
                    }
                    else if (tipo == "PR18")
                    {
                        int cierre = cierres[i + 1], primero = PuntoComa(i + 2), segundo = PuntoComa(primero + 1);
                        int llave = cierre + 1;
                        Asignar(i + 2, primero);
                        var repetir = Etiqueta(); var terminar = Etiqueta();
                        Marcar(repetir, p);
                        Agregar("SALTO_FALSO", terminar, Emitir(Fuente(primero + 1, segundo, "Condición POR")), null, p);
                        Bloque(llave + 1, cierres[llave], terminar);
                        Asignar(segundo + 1, cierre);
                        Saltar(repetir, p); Marcar(terminar, p);
                        i = cierres[llave] + 1;
                    }
                    else if (tipo == "PR13") i = Seleccion(i);
                    else if (tipo == "PR19")
                    {
                        if (salida == null) throw Error(p, "ROMPER requiere un ciclo o un caso en la misma función.");
                        Saltar(salida, p); i += 2;
                    }
                    else if (tipo == "PR2") i = Funcion(i);
                    else if (tipo == "PR3")
                    {
                        if (funcionActual == null) throw Error(p, "REGR requiere una función.");
                        int punto = PuntoComa(i);
                        var valor = punto == i + 1 ? null : Emitir(Fuente(i + 1, punto, "REGR"));
                        Agregar("RETORNO", null, valor, null, p, valor?.Tipo ?? "VAC");
                        i = punto + 1;
                    }
                    else if (tipo == "IDF")
                    {
                        int punto = PuntoComa(i);
                        Emitir(Fuente(i, punto, "Llamada"), null, false); i = punto + 1;
                    }
                    else if (tipo == "PR4")
                    {
                        var destino = Variable(i + 2);
                        RegistrarLlamada("LEER", new List<Expresion>(), p, "Lectura", destino);
                        Agregar("LEER", destino, null, null, p, destino.Tipo);
                        i = cierres[i + 1] + 2;
                    }
                    else if (tipo == "PR5")
                    {
                        var expresion = ExpresionRango(i + 2, cierres[i + 1]);
                        RegistrarLlamada("IMP", new List<Expresion> { expresion }, p, "Salida");
                        Agregar("IMP", null, Emitir(expresion), null, p);
                        i = cierres[i + 1] + 2;
                    }
                    else i = Incorporada(i);
                }
            }

            private int Seleccion(int p)
            {
                int llave = cierres[p + 1] + 1, fin = cierres[llave];
                var selector = Capturar(Emitir(Fuente(p + 2, p + 3, "Selector ENCASO")), p);
                var salida = Etiqueta();
                int i = llave + 1;
                while (i < fin)
                {
                    bool defecto = Es(i, "PR14");
                    var siguiente = defecto ? null : Etiqueta();
                    if (!defecto)
                    {
                        var valor = Emitir(Fuente(i, i + 1, "Valor de caso"));
                        string tipo = analisis.EvaluarOperacion(selector.Tipo, "==", valor.Tipo, tokens[i].Linea);
                        if (tipo != "BOOL") throw Error(i, "Tipo de caso incompatible con el selector.");
                        var coincide = Temporal("BOOL");
                        Agregar("==", coincide, selector, valor, i, "BOOL");
                        Agregar("SALTO_FALSO", siguiente, coincide, null, i);
                    }
                    int romper = i + 2;
                    while (romper < fin && !Es(romper, "PR19"))
                    {
                        if (Es(romper, "CE1") || Es(romper, "CE3")) romper = cierres[romper];
                        romper++;
                    }
                    Bloque(i + 2, romper, salida);
                    Saltar(salida, romper);
                    if (!defecto) Marcar(siguiente, i);
                    i = romper + 2;
                }
                Marcar(salida, p);
                return fin + 1;
            }

            private int Funcion(int p)
            {
                int parentesis = p + 3, llave = cierres[parentesis] + 1;
                var simbolo = Variable(p + 2);
                var despues = Etiqueta();
                Saltar(despues, p);
                var padre = funcionActual;
                funcionActual = simbolo.Simbolo;
                Agregar("FUNCION", simbolo, padre == null ? null : OperandoIntermedio.Variable(padre), null, p);
                int numero = 0;
                for (int i = parentesis + 1; i < cierres[parentesis]; i++)
                    if (Es(i, "IDV")) Agregar("PARAMETRO", Variable(i), Numero(numero++), null, i, Variable(i).Tipo);
                Bloque(llave + 1, cierres[llave], null);
                if (simbolo.Tipo == "VAC") Agregar("RETORNO", null, null, null, p);
                Agregar("FIN_FUNCION", simbolo, null, null, p);
                funcionActual = padre;
                Marcar(despues, p);
                return cierres[llave] + 1;
            }

            private int Incorporada(int p)
            {
                string op;
                switch (tokens[p].Tipo)
                {
                    case "PR6": op = "LIMP"; break;
                    case "PR7": op = "FONCOL"; break;
                    case "PR8": op = "TXTCOL"; break;
                    case "PR9": op = "SLTLN"; break;
                    case "PR10": op = "POS"; break;
                    case "PR29": op = "INTER"; break;
                    case "PR30": op = "TECL"; break;
                    case "PR31": op = "IMPREP"; break;
                    default: throw Error(p, "Instrucción no reconocida: " + tokens[p].Valor);
                }
                int fin = cierres[p + 1];
                if (op == "INTER")
                {
                    var primera = Variable(p + 4);
                    var segunda = Variable(p + 6);
                    RegistrarLlamada(op, new List<Expresion>
                    {
                        new Expresion { Operando = OperandoIntermedio.Literal(tokens[p + 2].Valor, "TIPO"), Posicion = p + 2 },
                        new Expresion { Operando = primera, Tipo = primera.Tipo, Posicion = p + 4 },
                        new Expresion { Operando = segunda, Tipo = segunda.Tipo, Posicion = p + 6 }
                    }, p, "Incorporada");
                    var copia = Temporal(primera.Tipo);
                    Agregar("=", copia, primera, null, p, primera.Tipo);
                    Agregar("=", primera, segunda, null, p, primera.Tipo);
                    Agregar("=", segunda, copia, null, p, segunda.Tipo);
                }
                else
                {
                    var argumentos = Argumentos(p + 2, fin);
                    RegistrarLlamada(op, argumentos, p, "Incorporada");
                    var a = argumentos.Count > 0 ? Emitir(argumentos[0]) : null;
                    var b = argumentos.Count > 1 ? Emitir(argumentos[1]) : null;
                    Agregar(op, null, a, b, p);
                }
                return fin + 2;
            }

            private void Asignar(int inicio, int fin)
            {
                int nombre = inicio + (EsTipo(tokens[inicio].Tipo) ? 1 : 0);
                if (nombre + 1 == fin) return;
                var destino = Variable(nombre);
                int desde = nombre + 2;
                if (Es(desde, "PR4"))
                {
                    RegistrarLlamada("LEER", new List<Expresion>(), desde, "Asignación", destino);
                    Agregar("LEER", destino, null, null, desde, destino.Tipo); return;
                }
                var expresion = ExpresionRango(desde, fin);
                Registrar(expresion, "Asignación", destino);
                if (expresion.Operando != null)
                    Agregar("=", destino, expresion.Operando, null, inicio, expresion.Tipo);
                else Emitir(expresion, destino);
            }

            private List<Expresion> Argumentos(int inicio, int fin)
            {
                var resultado = new List<Expresion>();
                int desde = inicio;
                for (int i = inicio; i < fin; i++)
                {
                    if (Es(i, "CE1")) i = cierres[i];
                    else if (Es(i, "CE7")) { resultado.Add(ExpresionRango(desde, i)); desde = i + 1; }
                }
                if (desde < fin) resultado.Add(ExpresionRango(desde, fin));
                return resultado;
            }

            private Expresion ExpresionRango(int inicio, int fin)
            {
                int i = inicio;
                var resultado = Parsear(ref i, fin, 0);
                if (i != fin) throw Error(i, "La expresión no pudo traducirse completamente.");
                return resultado;
            }

            private Expresion Parsear(ref int i, int fin, int minimo)
            {
                if (i >= fin) throw Error(Math.Max(0, fin - 1), "Falta un operando.");
                int p = i++;
                var tk = tokens[p];
                Expresion izquierda;
                if (tk.Tipo == "CE1")
                {
                    izquierda = ExpresionRango(i, cierres[p]); i = cierres[p] + 1;
                }
                else if (tk.Tipo == "OPL3")
                {
                    var derecha = Parsear(ref i, fin, OperadoresExpresion.Precedencia("!"));
                    if (derecha.Tipo != "BOOL") throw Error(p, "La negación requiere BOOL.");
                    izquierda = new Expresion { Operador = "!", Derecha = derecha, Tipo = "BOOL", Posicion = p };
                }
                else if (tk.Tipo == "IDF")
                {
                    var funcion = Variable(p).Simbolo;
                    izquierda = new Expresion { Funcion = funcion, Argumentos = Argumentos(i + 1, cierres[i]),
                        Tipo = funcion.Tipo, Posicion = p };
                    i = cierres[i] + 1;
                }
                else
                {
                    OperandoIntermedio operando;
                    if (tk.Tipo == "IDV") operando = Variable(p);
                    else
                    {
                        string tipo = analisis.ObtenerTipoOperando(tk.Tipo, tk.Valor, tk.Linea);
                        if (tipo == "ERROR" || tipo == "DESCONOCIDO") throw Error(p, "Literal no reconocido.");
                        operando = OperandoIntermedio.Literal(tk.Valor, tipo);
                    }
                    izquierda = new Expresion { Operando = operando, Tipo = operando.Tipo, Posicion = p };
                }
                while (i < fin && OperadoresExpresion.EsOperador(tokens[i].Tipo, tokens[i].Valor))
                {
                    string op = OperadoresExpresion.Normalizar(tokens[i].Tipo, tokens[i].Valor);
                    int prioridad = OperadoresExpresion.Precedencia(op);
                    if (prioridad < minimo) break;
                    int posicion = i++;
                    var derecha = Parsear(ref i, fin, op == "^" ? prioridad : prioridad + 1);
                    string tipo = analisis.EvaluarOperacion(izquierda.Tipo, op, derecha.Tipo, tokens[posicion].Linea);
                    if (tipo == "ERROR" || tipo == "DESCONOCIDO") throw Error(posicion, "Operación sin tipo válido.");
                    izquierda = new Expresion { Operador = op, Izquierda = izquierda, Derecha = derecha,
                        Tipo = tipo, Posicion = posicion };
                }
                return izquierda;
            }

            private OperandoIntermedio Capturar(OperandoIntermedio valor, int p)
            {
                if (valor.Simbolo == null) return valor;
                var copia = Temporal(valor.Tipo);
                Agregar("=", copia, valor, null, p, valor.Tipo);
                return copia;
            }

            private OperandoIntermedio Emitir(Expresion expresion, OperandoIntermedio destinoFinal = null,
                bool requiereValor = true)
            {
                if (expresion.Operando != null) return expresion.Operando;
                if (expresion.Funcion != null)
                {
                    if (requiereValor && expresion.Tipo == "VAC")
                        throw Error(expresion.Posicion, "Una llamada VAC no produce un valor para esta expresión.");
                    var valores = new List<OperandoIntermedio>();
                    foreach (var argumento in expresion.Argumentos)
                        valores.Add(Capturar(Emitir(argumento), argumento.Posicion));
                    for (int n = 0; n < valores.Count; n++)
                        Agregar("ARGUMENTO", Numero(n), valores[n], null, expresion.Posicion, valores[n].Tipo);
                    var destino = expresion.Tipo == "VAC" ? null : destinoFinal ?? Temporal(expresion.Tipo);
                    Agregar("LLAMAR", destino, OperandoIntermedio.Variable(expresion.Funcion), Numero(valores.Count),
                        expresion.Posicion, expresion.Tipo);
                    return destino;
                }
                OperandoIntermedio izquierda = expresion.Izquierda == null ? null : Emitir(expresion.Izquierda);
                if (izquierda != null && expresion.Derecha.ContieneLlamada)
                    izquierda = Capturar(izquierda, expresion.Posicion);
                var derecha = Emitir(expresion.Derecha);
                var resultado = destinoFinal ?? (izquierda != null && izquierda.EsTemporal && izquierda.Tipo == expresion.Tipo
                    ? izquierda : Temporal(expresion.Tipo));
                Agregar(expresion.Operador, resultado, izquierda ?? derecha,
                    izquierda == null ? null : derecha, expresion.Posicion, expresion.Tipo);
                return resultado;
            }
        }
    }
}
