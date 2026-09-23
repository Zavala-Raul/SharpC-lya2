using System.Collections.Generic;

namespace Automatas
{
    internal class AnalizadorSintacticoLl1
    {
        private const string Epsilon = "e";
        private const string FinEntrada = "EOF";
        private readonly Dictionary<string, Dictionary<string, List<string>>> tablaM;

        private static readonly HashSet<string> TokensInicioInstruccion = new HashSet<string>
        {
            // Tipos de dato 
            "PR23", "PR24", "PR25", "PR26", "PR27",
            "PR1",  // INI
            "PR2",  // FUNC
            "PR3",  // REGR
            "PR4",  // LEER
            "PR5",  // IMP
            "PR11", // SI
            "PR13", // ENCASO
            "PR15", // MIENT
            "PR16", // REPT
            "PR18", // POR
            "PR19", // ROMPER
            // I/O y gráficas
            "PR6", "PR7", "PR8", "PR9", "PR10",
            "PR29", "PR30", "PR31",
            // Identificadores
            "IDV", "IDF"
        };

        public AnalizadorSintacticoLl1(Dictionary<string, Dictionary<string, List<string>>> tablaM)
        {
            this.tablaM = tablaM;
        }

        public List<(int Linea, string Mensaje)> Analizar(List<(string Tipo, string Valor, int Linea)> tokens, int lineaFin)
        {
            var errores = new List<(int Linea, string Mensaje)>();
            var entrada = new List<(string Tipo, string Valor, int Linea)>(tokens)
            {
                (FinEntrada, "$", lineaFin)
            };

            Stack<string> pila = new Stack<string>();
            pila.Push(FinEntrada);
            pila.Push("S");

            int indice = 0;
            const int MaxErrores = 100;

            while (pila.Count > 0 && indice < entrada.Count)
            {
                if (errores.Count >= MaxErrores)
                {
                    errores.Add((entrada[indice].Linea, "Demasiados errores sintácticos. Análisis detenido."));
                    break;
                }

                string cima = pila.Peek();
                var tokenActual = entrada[indice];

                if (cima == FinEntrada && tokenActual.Tipo == FinEntrada)
                    break;

                if (EsTerminalSintactico(cima))
                {
                    ProcesarTerminal(pila, entrada, tokenActual, errores, ref indice);
                }
                else
                {
                    ProcesarNoTerminal(pila, entrada, tokenActual, errores, ref indice);
                }
            }

            return errores;
        }

        private void ProcesarTerminal(
            Stack<string> pila,
            List<(string Tipo, string Valor, int Linea)> entrada,
            (string Tipo, string Valor, int Linea) tokenActual,
            List<(int Linea, string Mensaje)> errores,
            ref int indice)
        {
            string terminalEsperado = pila.Peek();

            if (terminalEsperado == tokenActual.Tipo)
            {
                pila.Pop();
                indice++;
                return;
            }

            if (indice + 1 < entrada.Count && entrada[indice + 1].Tipo == terminalEsperado)
            {
                errores.Add((tokenActual.Linea,
                    $"TOKEN EXTRA: '{tokenActual.Valor}' no esperado antes de {TraducirToken(terminalEsperado)}"));
                indice++; 
                return;
            }

            string esperado = TraducirToken(terminalEsperado);
            errores.Add((tokenActual.Linea, $"OMISIÓN: Falta {esperado} cerca de '{tokenActual.Valor}'"));

            pila.Pop();
        }

        private void ProcesarNoTerminal(
            Stack<string> pila,
            List<(string Tipo, string Valor, int Linea)> entrada,
            (string Tipo, string Valor, int Linea) tokenActual,
            List<(int Linea, string Mensaje)> errores,
            ref int indice)
        {
            string noTerminal = pila.Peek();

            if (tablaM.ContainsKey(noTerminal) && tablaM[noTerminal].ContainsKey(tokenActual.Tipo))
            {
                ExpandirProduccion(pila, tablaM[noTerminal][tokenActual.Tipo]);
                return;
            }

            ReportarYRecuperar(noTerminal, pila, entrada, tokenActual, errores, ref indice);
        }

        private void ExpandirProduccion(Stack<string> pila, List<string> produccion)
        {
            pila.Pop();

            for (int i = produccion.Count - 1; i >= 0; i--)
            {
                if (produccion[i] != Epsilon)
                    pila.Push(produccion[i]);
            }
        }

        private void ReportarYRecuperar(
            string noTerminal,
            Stack<string> pila,
            List<(string Tipo, string Valor, int Linea)> entrada,
            (string Tipo, string Valor, int Linea) tokenActual,
            List<(int Linea, string Mensaje)> errores,
            ref int indice)
        {
            string ayudaSintaxis = ObtenerAyudaSintaxis(noTerminal);
            errores.Add((tokenActual.Linea, $"SINTAXIS INVÁLIDA: Token inesperado '{tokenActual.Valor}'. {ayudaSintaxis}"));

            if (tokenActual.Tipo == FinEntrada || indice >= entrada.Count - 1)
            {
                if (pila.Count > 0 && pila.Peek() == noTerminal)
                    pila.Pop();
                else
                    pila.Clear();
                return;
            }

            int posicionAntes = indice;

            if (!EsInicioInstruccion(tokenActual.Tipo) && tokenActual.Tipo != "CE4")
            {
                while (indice < entrada.Count)
                {
                    string tipo = entrada[indice].Tipo;
                    if (tipo == "CE8")
                    {
                        indice++;
                        break;
                    }
                    if (tipo == "CE4" || tipo == FinEntrada || EsInicioInstruccion(tipo))
                    {
                        break;
                    }
                    indice++;
                }

                if (indice == posicionAntes && indice < entrada.Count - 1)
                {
                    indice++;
                }
            }

            if (pila.Count > 0 && pila.Peek() == noTerminal)
            {
                pila.Pop();
            }

            while (pila.Count > 0 && !EsPuntoAnclaje(pila.Peek()))
            {
                pila.Pop();
            }
        }

        private bool EsPuntoAnclaje(string s)
        {
            return s == "INS" || s == "INS_CASO" || s == "CE4" ||
                   s == "S" || s == FinEntrada;
        }

        private bool EsInicioInstruccion(string token)
        {
            return TokensInicioInstruccion.Contains(token);
        }

        private bool EsTerminalSintactico(string simbolo)
        {
            return simbolo.StartsWith("PR") || simbolo.StartsWith("CE") ||
                   simbolo.StartsWith("OPA") || simbolo.StartsWith("OPL") ||
                   simbolo.StartsWith("OPR") || simbolo == "IDV" || simbolo == "IDF" ||
                   simbolo == "CNU" || simbolo == "CAD" || simbolo == "CAR" ||
                   simbolo == "ASIG" || simbolo == FinEntrada;
        }

        private string TraducirToken(string token)
        {
            switch (token)
            {
                case "PR1": return "INICIO (INI)";
                case "PR2": return "FUNCION (FUNC)";
                case "PR3": return "REGRESAR (REGR)";
                case "PR4": return "LEER";
                case "PR5": return "IMPRIMIR (IMP)";
                case "PR6": return "LEERENT";
                case "PR7": return "TITULAR";
                case "PR8": return "FONDO";
                case "PR9": return "TAMAÑO";
                case "PR10": return "POSICION";
                case "PR11": return "SI";
                case "PR12": return "SINO";
                case "PR13": return "ENCASO";
                case "PR14": return "DEFECTO (DFCT)";
                case "PR15": return "MIENTRAS (MIENT)";
                case "PR16": return "REPETIR (REPT)";
                case "PR17": return "HASTA";
                case "PR18": return "POR";
                case "PR19": return "ROMPER";
                case "PR20": return "VERDADERO";
                case "PR21": return "FALSO";
                case "PR22": return "NULO";
                case "PR23": return "tipo ENT";
                case "PR24": return "tipo DEC";
                case "PR25": return "tipo TXT";
                case "PR26": return "tipo BOOL";
                case "PR27": return "tipo CAR";
                case "PR28": return "tipo VAC";
                case "PR29": return "ARREGLO";
                case "PR30": return "LIMPIAR";
                case "PR31": return "MENSAJE";
                case "CE1": return "paréntesis de apertura '('";
                case "CE2": return "paréntesis de cierre ')'";
                case "CE3": return "llave de apertura '{'";
                case "CE4": return "llave de cierre '}'";
                case "CE7": return "coma ','";
                case "CE8": return "punto y coma ';'";
                case "CE20": return "dos puntos ':'";
                case "IDV": return "un identificador de variable";
                case "IDF": return "un identificador de función";
                case "ASIG": return "operador de asignación '='";
                case "CNU": return "un valor numérico";
                case "CAD": return "una cadena de texto";
                case "CAR": return "un carácter literal";
                case "OPA+": return "operador '+'";
                case "OPA-": return "operador '-'";
                case "OPA*": return "operador '*'";
                case "OPA/": return "operador '/'";
                case "OPA^": return "operador '^'";
                case "OPL1": return "operador lógico '&&'";
                case "OPL2": return "operador lógico '||'";
                case "OPL3": return "operador lógico '!'";
                case "EOF": return "fin de archivo (EOF)";
                default:
                    if (token.StartsWith("OPR")) return "un operador relacional";
                    return $"'{token}'";
            }
        }

        private string ObtenerAyudaSintaxis(string noTerminal)
        {
            switch (noTerminal)
            {
                case "S": return "El programa debe comenzar con INI { ... }";
                case "IN01": return "Sintaxis esperada: INI { <instrucciones> }";
                case "IN02": return "Sintaxis de declaración: TIPO variable [= expresión];";
                case "IN03": return "Sintaxis de asignación: variable = expresión;";
                case "IN04": return "Sintaxis de SI: SI (<Condicion>) { <Instrucciones> } SINO { <Instrucciones> }";
                case "IN04_1": return "Se esperaba SINO { ... } o el fin del bloque SI.";
                case "IN05": return "Sintaxis de ENCASO: ENCASO (<IDV>) { <Valor>: <Instrucciones> ROMPER; DFCT: <Instrucciones> ROMPER; }";
                case "INS_CASO": return "Dentro de ENCASO cada caso debe terminar con ROMPER; antes del siguiente caso o DFCT.";
                case "IN06": return "Sintaxis de MIENTRAS: MIENT (<Condicion>) { <Instrucciones> }";
                case "IN07": return "Sintaxis de REPETIR: REPT { <Instrucciones> } HASTA (<Condicion>);";
                case "IN08": return "Sintaxis de POR: POR (<Asignacion>; <Condicion>; <Paso>) { <Instrucciones> }";
                case "IN10": return "Sintaxis de imprimir: IMP(expresión);";
                case "IN11": return "Sintaxis de lectura: LEERENT();";
                case "IN12": return "Sintaxis: TITULAR(\"texto\");";
                case "IN13": return "Sintaxis: FONDO(\"color\");";
                case "IN14": return "Sintaxis: TAMAÑO(número);";
                case "IN15": return "Sintaxis: POSICION(x, y);";
                case "IN16": return "Sintaxis: ARREGLO(TIPO, variable, variable);";
                case "IN17": return "Sintaxis: LIMPIAR();";
                case "IN18": return "Sintaxis: MENSAJE(\"texto\", número);";
                case "IN19": return "Sintaxis de FUNCION: FUNC <Tipo> <IDF> (<Parametros>) { <Instrucciones> REGR <Valor>; }";
                case "IN20": return "Sintaxis: ROMPER;";
                case "IN21": return "Sintaxis de retorno: REGR [expresión];";
                case "IN22": return "Sintaxis de llamada: función(argumentos);";
                case "CALL_FUNC": return "Se esperaba una llamada de función: IDF(argumentos)";
                case "PARAM": return "Se esperaban parámetros: TIPO variable [, TIPO variable]...";
                case "PARAM_P": return "Se esperaba otro parámetro (,TIPO variable) o cierre de paréntesis.";
                case "TIPO_RET": return "Se esperaba un tipo de retorno (ENT, DEC, TXT, BOOL, CAR, VAC).";
                case "TIPO_VAR": return "Se requiere declarar el tipo de dato (ENT, DEC, TXT, BOOL, CAR).";
                case "ASIG_OPC": return "Falta el operador de asignación '=' o el cierre con ';'.";
                case "INIT_POR": return "Se esperaba la inicialización del POR: TIPO variable = expresión;";
                case "CASOS": return "Se esperaba un caso: valor: instrucciones ROMPER;";
                case "EXP":
                case "VALOR": return "Falta un operando (variable, número o cadena).";
                case "EXP_IO": return "Se esperaba una expresión o LEER().";
                case "EXP_OPC": return "Se esperaba una expresión de retorno o ';'.";
                case "POT_P":
                case "TERM_P":
                case "EXP_P": return "Falta operador aritmético o terminar la línea con ';'.";
                case "COND":
                case "COND_OR":
                case "COND_AND":
                case "COND_NOT": return "Se esperaba una condición lógica o relacional válida.";
                case "COND_REL": return "Se esperaba una expresión relacional o una condición entre paréntesis.";
                case "REL_OPC": return "Se esperaba un operador relacional (==, <>, <, >, <=, >=) o fin de condición.";
                case "ARG":
                case "ARG_P": return "Se esperaban argumentos de función o cierre de paréntesis.";
                default: return "Verifique la escritura de la instrucción según el lenguaje SharpC.";
            }
        }
    }
}
