using System;
using System.Collections.Generic;
using System.Linq;

namespace Automatas
{
    internal class VerificadorTipos
    {
        private readonly Dictionary<string, Simbolo> tablaSimbolos;
        private readonly Dictionary<string, Funcion> tablaFunciones;
        private readonly Dictionary<string, string> tiposVariables = new Dictionary<string, string>();

        public List<(int Linea, string Mensaje)> Errores { get; } = new List<(int, string)>();

        public VerificadorTipos(
            Dictionary<string, Simbolo> tablaSimbolos,
            Dictionary<string, Funcion> tablaFunciones = null)
        {
            this.tablaSimbolos = tablaSimbolos ?? new Dictionary<string, Simbolo>();
            this.tablaFunciones = tablaFunciones ?? new Dictionary<string, Funcion>();

            foreach (var kvp in this.tablaSimbolos)
            {
                if (!string.IsNullOrEmpty(kvp.Value.Tipo))
                {
                    tiposVariables[kvp.Key] = kvp.Value.Tipo;
                }
            }

            foreach (var fn in this.tablaFunciones.Values)
            {
                if (fn.Parametros != null)
                {
                    foreach (var p in fn.Parametros)
                    {
                        if (!string.IsNullOrEmpty(p.tipo) && p.tipo != "?")
                        {
                            tiposVariables[p.nombre] = p.tipo;
                        }
                    }
                }
            }
        }

        public void Verificar(List<(string Tipo, string Valor, int Linea)> tokens)
        {
            Errores.Clear();
            int i = 0;

            while (i < tokens.Count)
            {
                var tk = tokens[i];

                // Caso 1: Declaración con asignación (ej: ENT x = exp ;)
                if (EsTipoDato(tk.Tipo) && i + 2 < tokens.Count && tokens[i + 1].Tipo == "IDV" && tokens[i + 2].Tipo == "ASIG")
                {
                    string tipoDecl = ObtenerTipoDato(tk.Tipo, tk.Valor);
                    string nombreVar = tokens[i + 1].Valor;
                    int linea = tokens[i + 1].Linea;
                    tiposVariables[nombreVar] = tipoDecl;

                    if (tablaSimbolos.ContainsKey(nombreVar) && string.IsNullOrEmpty(tablaSimbolos[nombreVar].Tipo))
                    {
                        tablaSimbolos[nombreVar].Tipo = tipoDecl;
                    }

                    i += 3; // Saltar tipo, IDV, =
                    var tokensExp = ExtraerTokensHastaPuntoComa(tokens, ref i);
                    VerificarAsignacion(nombreVar, tipoDecl, tokensExp, linea);
                    continue;
                }

                // Caso 1b: Declaración simple sin asignación (ej: ENT x ;)
                if (EsTipoDato(tk.Tipo) && i + 1 < tokens.Count && tokens[i + 1].Tipo == "IDV" &&
                    (i + 2 >= tokens.Count || tokens[i + 2].Tipo == "CE8" || tokens[i + 2].Valor == ";"))
                {
                    string tipoDecl = ObtenerTipoDato(tk.Tipo, tk.Valor);
                    string nombreVar = tokens[i + 1].Valor;
                    tiposVariables[nombreVar] = tipoDecl;

                    if (tablaSimbolos.ContainsKey(nombreVar) && string.IsNullOrEmpty(tablaSimbolos[nombreVar].Tipo))
                    {
                        tablaSimbolos[nombreVar].Tipo = tipoDecl;
                    }

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

                    if (tipoVar == null)
                    {
                        Errores.Add((linea, $"ERROR DE TIPO: La variable '{nombreVar}' no ha sido declarada."));
                    }
                    else
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
                    var tokensPara = ExtraerTokensHastaParentesisCierre(tokens, ref i);
                    VerificarBuclePara(tokensPara, linea);
                    continue;
                }

                i++;
            }
        }

        private void VerificarBuclePara(List<(string Tipo, string Valor, int Linea)> tokensPara, int linea)
        {
            var partes = new List<List<(string Tipo, string Valor, int Linea)>>();
            var actual = new List<(string Tipo, string Valor, int Linea)>();

            foreach (var t in tokensPara)
            {
                if (t.Tipo == "CE8" || t.Valor == ";")
                {
                    partes.Add(actual);
                    actual = new List<(string Tipo, string Valor, int Linea)>();
                }
                else
                {
                    actual.Add(t);
                }
            }
            partes.Add(actual);

            if (partes.Count >= 2)
            {
                // Parte 0: Inicialización (ej: ENT i = 0 o i = 0)
                if (partes[0].Count >= 3)
                {
                    int idx = 0;
                    if (EsTipoDato(partes[0][0].Tipo))
                    {
                        string tipoD = ObtenerTipoDato(partes[0][0].Tipo, partes[0][0].Valor);
                        string nom = partes[0][1].Valor;
                        tiposVariables[nom] = tipoD;
                        idx = 2;
                    }
                    if (idx + 1 < partes[0].Count && partes[0][idx].Tipo == "ASIG")
                    {
                        string nom = idx == 2 ? partes[0][1].Valor : partes[0][0].Valor;
                        string tipoV = ObtenerTipoVariable(nom);
                        var expInit = partes[0].GetRange(idx + 1, partes[0].Count - (idx + 1));
                        if (tipoV != null)
                            VerificarAsignacion(nom, tipoV, expInit, linea);
                    }
                }

                // Parte 1: Condición (debe evaluar a BOOL)
                VerificarCondicion(partes[1], linea);

                // Parte 2: Incremento (ej: i = i + 1)
                if (partes.Count >= 3 && partes[2].Count >= 3 && partes[2][1].Tipo == "ASIG")
                {
                    string nom = partes[2][0].Valor;
                    string tipoV = ObtenerTipoVariable(nom);
                    var expInc = partes[2].GetRange(2, partes[2].Count - 2);
                    if (tipoV != null)
                        VerificarAsignacion(nom, tipoV, expInc, linea);
                }
            }
        }

        public void VerificarAsignacion(string nombreVar, string tipoVar, List<(string Tipo, string Valor, int Linea)> tokensExp, int linea)
        {
            string tipoExp = EvaluarTipoExpresion(tokensExp, linea);

            if (tipoExp != "ERROR" && tipoExp != "DESCONOCIDO")
            {
                if (!SonCompatiblesAsignacion(tipoVar, tipoExp))
                {
                    Errores.Add((linea,
                        $"ERROR DE TIPO: No se puede asignar una expresión de tipo '{tipoExp}' a la variable '{nombreVar}' de tipo '{tipoVar}'."));
                }
                else
                {
                    if (tablaSimbolos.ContainsKey(nombreVar))
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

        public string EvaluarTipoExpresion(List<(string Tipo, string Valor, int Linea)> tokensExp, int linea)
        {
            if (tokensExp == null || tokensExp.Count == 0) return "VAC";

            var pilaTipos = new Stack<string>();
            var pilaOps = new Stack<string>();

            int i = 0;
            while (i < tokensExp.Count)
            {
                var tk = tokensExp[i];

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
                    i++; // Avanzar a '('
                    int nivel = 1;
                    i++;
                    while (i < tokensExp.Count && nivel > 0)
                    {
                        if (tokensExp[i].Tipo == "CE1" || tokensExp[i].Valor == "(") nivel++;
                        else if (tokensExp[i].Tipo == "CE2" || tokensExp[i].Valor == ")") nivel--;
                        i++;
                    }
                    i--; // Ajustar índice para el bucle exterior

                    if (tablaFunciones.ContainsKey(nomFuncion))
                    {
                        pilaTipos.Push(tablaFunciones[nomFuncion].TipoRetorno);
                    }
                    else
                    {
                        Errores.Add((linea, $"ERROR DE TIPO: La función '{nomFuncion}' no ha sido declarada."));
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
                    while (pilaOps.Count > 0 && pilaOps.Peek() != "(" &&
                           ObtenerPrecedencia(pilaOps.Peek()) >= ObtenerPrecedencia(op))
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
                return lexema.Contains(".") ? "DEC" : "ENT";

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
                string tipo = ObtenerTipoVariable(lexema);
                if (tipo != null)
                    return tipo;

                Errores.Add((linea, $"ERROR DE TIPO: La variable '{lexema}' no ha sido declarada."));
                return "ERROR";
            }

            if (tipoToken == "IDF")
            {
                if (tablaFunciones.ContainsKey(lexema))
                    return tablaFunciones[lexema].TipoRetorno;

                Errores.Add((linea, $"ERROR DE TIPO: La función '{lexema}' no ha sido declarada."));
                return "ERROR";
            }

            return "DESCONOCIDO";
        }

        public string ObtenerTipoVariable(string nombre)
        {
            if (tiposVariables.ContainsKey(nombre) && !string.IsNullOrEmpty(tiposVariables[nombre]))
                return tiposVariables[nombre];

            if (tablaSimbolos.ContainsKey(nombre) && !string.IsNullOrEmpty(tablaSimbolos[nombre].Tipo))
                return tablaSimbolos[nombre].Tipo;

            return null;
        }

        private string ObtenerTipoDato(string tipoToken, string valor)
        {
            switch (tipoToken)
            {
                case "PR23": return "ENT";
                case "PR24": return "DEC";
                case "PR25": return "TXT";
                case "PR26": return "BOOL";
                case "PR27": return "CAR";
                case "PR28": return "VAC";
            }

            string vUpper = valor.ToUpper();
            if (vUpper == "ENT" || vUpper == "DEC" || vUpper == "TXT" ||
                vUpper == "BOOL" || vUpper == "CAR" || vUpper == "VAC")
                return vUpper;

            return "DESCONOCIDO";
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
                case "!": return 6;
                case "^": return 5;
                case "*":
                case "/": return 4;
                case "+":
                case "-": return 3;
                case "==":
                case "<>":
                case "<":
                case ">":
                case "<=":
                case ">=": return 2;
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
    }
}
