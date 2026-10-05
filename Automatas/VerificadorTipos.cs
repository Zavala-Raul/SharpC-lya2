using System;
using System.Collections.Generic;
using System.Linq;

namespace Automatas
{
    internal class VerificadorTipos
    {
        private readonly Dictionary<string, Simbolo> tablaSimbolos;
        private readonly Dictionary<string, Funcion> tablaFunciones;
        // Cada fragmento conserva su desplazamiento en la lista fuente. Comparar
        // tuplas por valor no sirve: dos usos homónimos pueden estar en una línea.
        private readonly Dictionary<List<(string Tipo, string Valor, int Linea)>, int> inicios =
            new Dictionary<List<(string, string, int)>, int>();
        private int posicionActual;
        public TablaAmbitos TablaAmbitos { get; private set; }
        private Simbolo SimboloActual => TablaAmbitos != null && posicionActual >= 0 &&
            posicionActual < TablaAmbitos.SimbolosPorToken.Length
                ? TablaAmbitos.SimbolosPorToken[posicionActual] : null;

        public List<(int Linea, string Mensaje)> Errores { get; } = new List<(int, string)>();

        public VerificadorTipos(
            Dictionary<string, Simbolo> tablaSimbolos,
            Dictionary<string, Funcion> tablaFunciones = null)
        {
            this.tablaSimbolos = tablaSimbolos ?? new Dictionary<string, Simbolo>();
            this.tablaFunciones = tablaFunciones ?? new Dictionary<string, Funcion>();

        }

        public void Verificar(List<(string Tipo, string Valor, int Linea)> tokens)
        {
            Errores.Clear();
            inicios.Clear();
            inicios[tokens] = 0;
            TablaAmbitos = new TablaAmbitos(tokens, tablaFunciones.Values);
            Errores.AddRange(TablaAmbitos.Errores);

            // También revisa literales en impresión, retornos y argumentos que este
            // recorrido todavía no verifica por completo. No evalúa las operaciones.
            foreach (var token in tokens)
            {
                if (token.Tipo == "CNU")
                    ObtenerTipoOperando(token.Tipo, token.Valor, token.Linea);
            }

            int i = 0;

            while (i < tokens.Count)
            {
                var tk = tokens[i];
                posicionActual = i;

                // Caso 1: Declaración con asignación (ej: ENT x = exp ;)
                if (EsTipoDato(tk.Tipo) && i + 2 < tokens.Count && tokens[i + 1].Tipo == "IDV" && tokens[i + 2].Tipo == "ASIG")
                {
                    string nombreVar = tokens[i + 1].Valor;
                    int linea = tokens[i + 1].Linea;
                    posicionActual = i + 1;
                    string tipoDecl = SimboloActual?.Tipo ?? "ERROR";

                    i += 3; // Saltar tipo, IDV, =
                    var tokensExp = ExtraerTokensHastaPuntoComa(tokens, ref i);
                    VerificarAsignacion(nombreVar, tipoDecl, tokensExp, linea);
                    continue;
                }

                // Caso 1b: Declaración simple sin asignación (ej: ENT x ;)
                if (EsTipoDato(tk.Tipo) && i + 1 < tokens.Count && tokens[i + 1].Tipo == "IDV" &&
                    (i + 2 >= tokens.Count || tokens[i + 2].Tipo == "CE8" || tokens[i + 2].Valor == ";"))
                {
                    i += 2;
                    if (i < tokens.Count && (tokens[i].Tipo == "CE8" || tokens[i].Valor == ";"))
                        i++;
                    continue;
                }

                // Caso 2: Reasignación directa (ej: x = exp ;)
                if (tk.Tipo == "IDV" && i + 1 < tokens.Count && tokens[i + 1].Tipo == "ASIG")
                {
                    string nombreVar = tk.Valor;
                    int linea = tk.Linea;
                    i += 2; // Saltar IDV, =

                    var tokensExp = ExtraerTokensHastaPuntoComa(tokens, ref i);
                    string tipoVar = ObtenerTipoVariable(nombreVar);

                    if (tipoVar != null)
                    {
                        VerificarAsignacion(nombreVar, tipoVar, tokensExp, linea);
                    }
                    continue;
                }

                // Caso 3: Condicionales (SI=PR11, MIENT=PR15, HASTA=PR17) que deben evaluar a BOOL
                if ((tk.Tipo == "PR11" || tk.Tipo == "PR15" || tk.Tipo == "PR17") && i + 1 < tokens.Count && (tokens[i + 1].Tipo == "CE1" || tokens[i + 1].Valor == "("))
                {
                    int linea = tk.Linea;
                    i += 2; // Saltar PR, (
                    var tokensCond = ExtraerTokensHastaParentesisCierre(tokens, ref i);
                    VerificarCondicion(tokensCond, linea);
                    continue;
                }

                // Caso 4: Bucle POR (PR18: PARA ( INIT ; COND ; INC ))
                if (tk.Tipo == "PR18" && i + 1 < tokens.Count && (tokens[i + 1].Tipo == "CE1" || tokens[i + 1].Valor == "("))
                {
                    int linea = tk.Linea;
                    i += 2; // Saltar POR, (
                    var tokensPor = ExtraerTokensHastaParentesisCierre(tokens, ref i);
                    VerificarBuclePor(tokensPor, linea);
                    continue;
                }

                if (tk.Tipo == "PR5" && i + 1 < tokens.Count)
                {
                    i += 2;
                    var expresion = ExtraerTokensHastaParentesisCierre(tokens, ref i);
                    EvaluarTipoExpresion(expresion, tk.Linea);
                    continue;
                }
                if (tk.Tipo == "PR3")
                {
                    int posicionRetorno = i;
                    i++;
                    VerificarRetorno(ExtraerTokensHastaPuntoComa(tokens, ref i),
                        tk.Linea, TablaAmbitos.ObtenerFuncionContenedora(
                            TablaAmbitos.AmbitosPorToken[posicionRetorno]));
                    continue;
                }
                // Las declaraciones de funciones no son llamadas.
                if (tk.Tipo == "IDF" && (i < 2 || tokens[i - 2].Tipo != "PR2"))
                {
                    EvaluarTipoExpresion(ExtraerTokensHastaPuntoComa(tokens, ref i), tk.Linea);
                    continue;
                }

                i++;
            }
            Errores.AddRange(new AnalizadorInicializacion(tokens, TablaAmbitos).Errores);
        }

        private void VerificarBuclePor(List<(string Tipo, string Valor, int Linea)> tokensPor, int linea)
        {
            var partes = new List<List<(string Tipo, string Valor, int Linea)>>();
            int inicio = 0;
            for (int j = 0; j < tokensPor.Count; j++)
            {
                var t = tokensPor[j];
                if (t.Tipo == "CE8" || t.Valor == ";")
                {
                    partes.Add(Fragmento(tokensPor, inicio, j - inicio));
                    inicio = j + 1;
                }
            }
            partes.Add(Fragmento(tokensPor, inicio, tokensPor.Count - inicio));

            if (partes.Count >= 2)
            {
                // Parte 0: Inicialización, con o sin declaración.
                VerificarAsignacionPor(partes[0]);

                // Parte 1: Condición (debe evaluar a BOOL)
                VerificarCondicion(partes[1], partes[1].Count > 0 ? partes[1][0].Linea : linea);

                // Parte 2: Incremento (ej: i = i + 1)
                if (partes.Count >= 3)
                    VerificarAsignacionPor(partes[2]);
            }
        }

        private void VerificarAsignacionPor(List<(string Tipo, string Valor, int Linea)> tokens)
        {
            if (tokens.Count < 3) return;

            // ENT vI = ...: variable en 1 y '=' en 2.
            //     vI = ...: variable en 0 y '=' en 1.
            bool esDeclaracion = EsTipoDato(tokens[0].Tipo);
            int indiceVariable = esDeclaracion ? 1 : 0;
            int indiceAsignacion = indiceVariable + 1;
            if (indiceAsignacion + 1 >= tokens.Count || tokens[indiceVariable].Tipo != "IDV" ||
                tokens[indiceAsignacion].Tipo != "ASIG") return;

            var variable = tokens[indiceVariable];
            posicionActual = Posicion(tokens, indiceVariable);

            string tipoVariable = ObtenerTipoVariable(variable.Valor);
            if (tipoVariable == null)
            {
                return;
            }

            int inicioExpresion = indiceAsignacion + 1;
            var expresion = Fragmento(tokens, inicioExpresion, tokens.Count - inicioExpresion);
            VerificarAsignacion(variable.Valor, tipoVariable, expresion, variable.Linea);
        }

        public void VerificarAsignacion(string nombreVar, string tipoVar, List<(string Tipo, string Valor, int Linea)> tokensExp, int linea)
        {
            Simbolo destino = SimboloActual;
            string tipoExp = EvaluarTipoExpresion(tokensExp, linea);

            if (tipoVar != "ERROR" && tipoExp != "ERROR" && tipoExp != "DESCONOCIDO")
            {
                if (!SonCompatiblesAsignacion(tipoVar, tipoExp))
                {
                    Errores.Add((linea,
                        $"ERROR DE TIPO: No se puede asignar una expresión de tipo '{tipoExp}' a la variable '{nombreVar}' de tipo '{tipoVar}'."));
                }
                else
                {
                    if (TablaAmbitos != null)
                    {
                        if (destino != null) destino.Valor = string.Join(" ", tokensExp.Select(t => t.Valor));
                    }
                    else if (tablaSimbolos.ContainsKey(nombreVar))
                    {
                        string valorExp = string.Join(" ", tokensExp.Select(t => t.Valor));
                        tablaSimbolos[nombreVar].Valor = valorExp;
                    }
                }
            }
        }

        public void VerificarCondicion(List<(string Tipo, string Valor, int Linea)> tokensCond, int linea)
        {
            if (tokensCond == null || tokensCond.Count == 0) return;

            string tipoCond = EvaluarTipoExpresion(tokensCond, linea);
            if (tipoCond != "ERROR" && tipoCond != "DESCONOCIDO" && tipoCond != "BOOL")
            {
                Errores.Add((linea,
                    $"ERROR DE TIPO: La condición debe evaluar a 'BOOL', pero se obtuvo '{tipoCond}'."));
            }
        }

        private void VerificarRetorno(List<(string Tipo, string Valor, int Linea)> expresion,
            int linea, Funcion funcion)
        {
            string tipo = expresion.Count == 0 ? "VAC" : EvaluarTipoExpresion(expresion, linea);
            if (funcion == null)
            {
                Errores.Add((linea, "ERROR DE TIPO: REGR solo puede utilizarse dentro de una función."));
                return;
            }
            if (tipo == "ERROR" || tipo == "DESCONOCIDO") return;
            if (funcion.TipoRetorno == "VAC" && expresion.Count > 0)
                Errores.Add((linea, $"ERROR DE TIPO: La función '{funcion.Nombre}' de tipo 'VAC' no debe devolver una expresión."));
            else if (funcion.TipoRetorno != "VAC" && expresion.Count == 0)
                Errores.Add((linea, $"ERROR DE TIPO: La función '{funcion.Nombre}' debe devolver un valor de tipo '{funcion.TipoRetorno}'."));
            else if (!SonCompatiblesAsignacion(funcion.TipoRetorno, tipo))
                Errores.Add((linea, $"ERROR DE TIPO: El retorno de la función '{funcion.Nombre}' debe ser '{funcion.TipoRetorno}', pero se obtuvo '{tipo}'."));
        }

        public string EvaluarTipoExpresion(List<(string Tipo, string Valor, int Linea)> tokensExp, int linea)
        {
            if (tokensExp == null || tokensExp.Count == 0) return "VAC";

            var pilaTipos = new Stack<string>();
            var pilaOps = new Stack<string>();

            int i = 0;
            while (i < tokensExp.Count)
            {
                var tk = tokensExp[i];
                posicionActual = Posicion(tokensExp, i);

                if (tk.Tipo == "CE1" || tk.Valor == "(")
                {
                    pilaOps.Push("(");
                }
                else if (tk.Tipo == "CE2" || tk.Valor == ")")
                {
                    while (pilaOps.Count > 0 && pilaOps.Peek() != "(")
                    {
                        AplicarOperador(pilaTipos, pilaOps, linea);
                    }
                    if (pilaOps.Count > 0 && pilaOps.Peek() == "(")
                    {
                        pilaOps.Pop();
                    }
                }
                else if (tk.Tipo == "IDF" && i + 1 < tokensExp.Count && (tokensExp[i + 1].Tipo == "CE1" || tokensExp[i + 1].Valor == "("))
                {
                    string nomFuncion = tk.Valor;
                    string tipoFuncion = TablaAmbitos != null ? SimboloActual?.Tipo :
                        (tablaFunciones.ContainsKey(nomFuncion) ? tablaFunciones[nomFuncion].TipoRetorno : null);
                    Funcion firma = TablaAmbitos?.ObtenerFuncion(SimboloActual);
                    if (firma == null) tablaFunciones.TryGetValue(nomFuncion, out firma);
                    i++; // Avanzar a '('
                    int nivel = 1;
                    i++;
                    int inicioArgumento = i;
                    bool argumentoInvalido = false;
                    var argumentos = new List<(string Tipo, int Linea)>();
                    while (i < tokensExp.Count && nivel > 0)
                    {
                        if (tokensExp[i].Tipo == "CE1" || tokensExp[i].Valor == "(") nivel++;
                        else if (tokensExp[i].Tipo == "CE2" || tokensExp[i].Valor == ")") nivel--;
                        if (nivel == 0 || (nivel == 1 && tokensExp[i].Tipo == "CE7"))
                        {
                            if (i > inicioArgumento)
                            {
                                string tipoArgumento = EvaluarTipoExpresion(
                                    Fragmento(tokensExp, inicioArgumento, i - inicioArgumento), tk.Linea);
                                argumentos.Add((tipoArgumento, tokensExp[inicioArgumento].Linea));
                                argumentoInvalido |= tipoArgumento == "ERROR" || tipoArgumento == "DESCONOCIDO";
                            }
                            inicioArgumento = i + 1;
                        }
                        i++;
                    }
                    i--; // Ajustar índice para el bucle exterior

                    if (firma != null)
                    {
                        if (argumentos.Count != firma.Parametros.Count)
                        {
                            Errores.Add((tk.Linea, $"ERROR DE TIPO: La función '{nomFuncion}' espera " +
                                $"{firma.Parametros.Count} argumento(s), pero recibió {argumentos.Count}."));
                            argumentoInvalido = true;
                        }
                        for (int argumento = 0; argumento < Math.Min(argumentos.Count, firma.Parametros.Count); argumento++)
                        {
                            var recibido = argumentos[argumento];
                            if (recibido.Tipo == "ERROR" || recibido.Tipo == "DESCONOCIDO") continue;
                            string esperado = firma.Parametros[argumento].tipo;
                            if (!SonCompatiblesAsignacion(esperado, recibido.Tipo))
                            {
                                Errores.Add((recibido.Linea, $"ERROR DE TIPO: El argumento {argumento + 1} " +
                                    $"de la función '{nomFuncion}' debe ser '{esperado}', pero se obtuvo '{recibido.Tipo}'."));
                                argumentoInvalido = true;
                            }
                        }
                    }

                    if (tipoFuncion != null)
                    {
                        pilaTipos.Push(argumentoInvalido ? "ERROR" : tipoFuncion);
                    }
                    else
                    {
                        if (TablaAmbitos == null)
                            Errores.Add((tk.Linea, $"ERROR DE TIPO: La función '{nomFuncion}' no ha sido declarada."));
                        pilaTipos.Push("ERROR");
                    }
                }
                else if (tk.Tipo == "OPL3" || tk.Valor == "!")
                {
                    pilaOps.Push("!");
                }
                else if (EsOperador(tk.Tipo, tk.Valor))
                {
                    string op = NormalizarOperador(tk.Tipo, tk.Valor);
                    // La potencia es asociativa a la derecha: a ^ b ^ c = a ^ (b ^ c).
                    // A igual precedencia, los demás operadores se aplican de izquierda a derecha.
                    while (pilaOps.Count > 0 && pilaOps.Peek() != "(" &&
                           (ObtenerPrecedencia(pilaOps.Peek()) > ObtenerPrecedencia(op) ||
                            (ObtenerPrecedencia(pilaOps.Peek()) == ObtenerPrecedencia(op) && op != "^")))
                    {
                        AplicarOperador(pilaTipos, pilaOps, linea);
                    }
                    pilaOps.Push(op);
                }
                else
                {
                    string tipoOperando = ObtenerTipoOperando(tk.Tipo, tk.Valor, tk.Linea);
                    pilaTipos.Push(tipoOperando);
                }
                i++;
            }

            while (pilaOps.Count > 0)
            {
                if (pilaOps.Peek() == "(") { pilaOps.Pop(); continue; }
                AplicarOperador(pilaTipos, pilaOps, linea);
            }

            return pilaTipos.Count > 0 ? pilaTipos.Pop() : "ERROR";
        }

        private void AplicarOperador(Stack<string> pilaTipos, Stack<string> pilaOps, int linea)
        {
            if (pilaOps.Count == 0) return;

            string op = pilaOps.Pop();

            // Operador unario: !
            if (op == "!" || op == "OPL3")
            {
                if (pilaTipos.Count < 1) return;
                string operando = pilaTipos.Pop();
                if (operando == "ERROR") { pilaTipos.Push("ERROR"); return; }
                if (operando != "BOOL")
                {
                    Errores.Add((linea,
                        $"ERROR DE TIPO: El operador lógico '!' requiere un operando 'BOOL', pero recibió '{operando}'."));
                    pilaTipos.Push("ERROR");
                }
                else
                {
                    pilaTipos.Push("BOOL");
                }
                return;
            }

            // Operadores binarios
            if (pilaTipos.Count < 2) return;

            string der = pilaTipos.Pop();
            string izq = pilaTipos.Pop();

            string res = EvaluarOperacion(izq, op, der, linea);
            pilaTipos.Push(res);
        }

        public string EvaluarOperacion(string tipoIzq, string op, string tipoDer, int linea)
        {
            if (tipoIzq == "ERROR" || tipoDer == "ERROR") return "ERROR";
            if (tipoIzq == "DESCONOCIDO" || tipoDer == "DESCONOCIDO") return "DESCONOCIDO";

            // Aritmética: +, -, *, /, ^
            if (op == "+" || op == "-" || op == "*" || op == "/" || op == "^")
            {
                if (tipoIzq == "ENT" && tipoDer == "ENT") return "ENT";
                if ((tipoIzq == "ENT" && tipoDer == "DEC") || (tipoIzq == "DEC" && tipoDer == "ENT")) return "DEC";
                if (tipoIzq == "DEC" && tipoDer == "DEC") return "DEC";

                // Concatenación de cadenas permitida solo con '+'
                if (op == "+" && (tipoIzq == "TXT" || tipoDer == "TXT")) return "TXT";

                Errores.Add((linea,
                    $"ERROR DE TIPO: El operador aritmético '{op}' no es válido entre '{tipoIzq}' y '{tipoDer}'."));
                return "ERROR";
            }

            // Relacionales: ==, <>, <, >, <=, >=
            if (op == "==" || op == "<>" || op == "<" || op == ">" || op == "<=" || op == ">=")
            {
                if (EsNumerico(tipoIzq) && EsNumerico(tipoDer)) return "BOOL";
                if (tipoIzq == tipoDer) return "BOOL";

                Errores.Add((linea,
                    $"ERROR DE TIPO: No se pueden comparar tipos incompatibles '{tipoIzq}' y '{tipoDer}' con '{op}'."));
                return "ERROR";
            }

            // Lógicos: &&, ||
            if (op == "&&" || op == "||")
            {
                if (tipoIzq == "BOOL" && tipoDer == "BOOL") return "BOOL";

                Errores.Add((linea,
                    $"ERROR DE TIPO: El operador lógico '{op}' requiere operandos 'BOOL', pero recibió '{tipoIzq}' y '{tipoDer}'."));
                return "ERROR";
            }

            return "ERROR";
        }

        public string ObtenerTipoOperando(string tipoToken, string lexema, int linea)
        {
            if (tipoToken == "CNU")
            {
                if (CatalogoTipos.EsLiteralDecimal(lexema)) return "DEC";
                if (CatalogoTipos.EsLiteralEnteroEnRango(lexema)) return "ENT";

                string mensaje = CatalogoTipos.MensajeEnteroFueraDeRango(lexema);
                // La pasada previa y la inferencia pueden encontrar el mismo literal.
                // Un mismo literal inválido en la misma línea se informa una sola vez.
                if (!Errores.Any(e => e.Linea == linea && e.Mensaje == mensaje))
                    Errores.Add((linea, mensaje));
                return "ERROR";
            }

            if (tipoToken == "CAD") return "TXT";
            if (tipoToken == "CAR") return "CAR";

            if (tipoToken == "PR20" || tipoToken == "PR21" || tipoToken == "PR22" ||
                lexema.Equals("VERDADERO", StringComparison.OrdinalIgnoreCase) ||
                lexema.Equals("FALSO", StringComparison.OrdinalIgnoreCase) ||
                lexema.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                lexema.Equals("false", StringComparison.OrdinalIgnoreCase))
                return "BOOL";

            if (tipoToken == "IDV")
            {
                if (TablaAmbitos != null) return SimboloActual?.Tipo ?? "ERROR";
                string tipo = ObtenerTipoVariable(lexema);
                if (tipo != null)
                    return tipo;

                Errores.Add((linea, $"ERROR DE TIPO: La variable '{lexema}' no ha sido declarada."));
                return "ERROR";
            }

            if (tipoToken == "IDF")
            {
                if (TablaAmbitos != null) return SimboloActual?.Tipo ?? "ERROR";
                if (tablaFunciones.ContainsKey(lexema))
                    return tablaFunciones[lexema].TipoRetorno;

                Errores.Add((linea, $"ERROR DE TIPO: La función '{lexema}' no ha sido declarada."));
                return "ERROR";
            }

            return "DESCONOCIDO";
        }

        public string ObtenerTipoVariable(string nombre)
        {
            if (TablaAmbitos != null) return SimboloActual?.Nombre == nombre ? SimboloActual.Tipo : null;
            if (tablaSimbolos.ContainsKey(nombre) && !string.IsNullOrEmpty(tablaSimbolos[nombre].Tipo))
                return tablaSimbolos[nombre].Tipo;

            return null;
        }

        public bool SonCompatiblesAsignacion(string tipoVariable, string tipoExpresion)
        {
            if (tipoVariable == tipoExpresion) return true;

            // Promoción permitida: un ENT puede asignarse a un DEC
            if (tipoVariable == "DEC" && tipoExpresion == "ENT") return true;

            return false;
        }

        private bool EsTipoDato(string tipo)
        {
            return tipo == "PR23" || tipo == "PR24" || tipo == "PR25" ||
                   tipo == "PR26" || tipo == "PR27" || tipo == "PR28";
        }

        private bool EsNumerico(string tipo)
        {
            return tipo == "ENT" || tipo == "DEC";
        }

        private bool EsOperador(string tipoToken, string valor)
        {
            return tipoToken.StartsWith("OPA") || tipoToken.StartsWith("OPR") || tipoToken.StartsWith("OPL") ||
                   valor == "+" || valor == "-" || valor == "*" || valor == "/" || valor == "^" ||
                   valor == "==" || valor == "<>" || valor == "<" || valor == ">" || valor == "<=" || valor == ">=" ||
                   valor == "&&" || valor == "||";
        }

        private string NormalizarOperador(string tipoToken, string valor)
        {
            switch (tipoToken)
            {
                case "OPA+": return "+";
                case "OPA-": return "-";
                case "OPA*": return "*";
                case "OPA/": return "/";
                case "OPA^": return "^";
                case "OPR1": return "==";
                case "OPR2": return "<>";
                case "OPR3": return "<";
                case "OPR4": return ">";
                case "OPR5": return "<=";
                case "OPR6": return ">=";
                case "OPL1": return "&&";
                case "OPL2": return "||";
                case "OPL3": return "!";
                default: return valor;
            }
        }

        private int ObtenerPrecedencia(string op)
        {
            switch (op)
            {
                case "^": return 6;
                case "*":
                case "/": return 5;
                case "+":
                case "-": return 4;
                case "==":
                case "<>":
                case "<":
                case ">":
                case "<=":
                case ">=": return 3;
                // COND_NOT -> ! COND_NOT: la comparación se completa antes de negarla.
                case "!": return 2;
                case "&&": return 1;
                case "||": return 0;
                default: return -1;
            }
        }

        private List<(string Tipo, string Valor, int Linea)> ExtraerTokensHastaPuntoComa(
            List<(string Tipo, string Valor, int Linea)> tokens,
            ref int indice)
        {
            var res = new List<(string Tipo, string Valor, int Linea)>();
            inicios[res] = Posicion(tokens, indice);
            while (indice < tokens.Count && tokens[indice].Tipo != "CE8" && tokens[indice].Valor != ";")
            {
                res.Add(tokens[indice]);
                indice++;
            }
            if (indice < tokens.Count && (tokens[indice].Tipo == "CE8" || tokens[indice].Valor == ";"))
            {
                indice++; // Consumir ';'
            }
            return res;
        }

        private List<(string Tipo, string Valor, int Linea)> ExtraerTokensHastaParentesisCierre(
            List<(string Tipo, string Valor, int Linea)> tokens,
            ref int indice)
        {
            var res = new List<(string Tipo, string Valor, int Linea)>();
            inicios[res] = Posicion(tokens, indice);
            int nivel = 1;
            while (indice < tokens.Count && nivel > 0)
            {
                if (tokens[indice].Tipo == "CE1" || tokens[indice].Valor == "(") nivel++;
                else if (tokens[indice].Tipo == "CE2" || tokens[indice].Valor == ")")
                {
                    nivel--;
                    if (nivel == 0) { indice++; break; }
                }
                res.Add(tokens[indice]);
                indice++;
            }
            return res;
        }

        private int Posicion(List<(string Tipo, string Valor, int Linea)> tokens, int indice)
        {
            return inicios.TryGetValue(tokens, out int inicio) ? inicio + indice : -1;
        }

        private List<(string Tipo, string Valor, int Linea)> Fragmento(
            List<(string Tipo, string Valor, int Linea)> tokens, int inicio, int cantidad)
        {
            var fragmento = tokens.GetRange(inicio, cantidad);
            inicios[fragmento] = Posicion(tokens, inicio);
            return fragmento;
        }
    }
}
