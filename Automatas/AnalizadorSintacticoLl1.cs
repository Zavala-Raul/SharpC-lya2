using System.Collections.Generic;

namespace Automatas
{
    internal class AnalizadorSintacticoLl1
    {
        private const string Epsilon = "e";
        private const string FinEntrada = "EOF";
        private readonly Dictionary<string, Dictionary<string, List<string>>> tablaM;

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
                    ProcesarTerminal(pila, tokenActual, errores, ref indice);
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

            if (!EsInicioInstruccion(tokenActual.Tipo) && tokenActual.Tipo != "CE4")
            {
                int indicePrevio = indice;
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

                if (indice == indicePrevio && indice < entrada.Count - 1)
                {
                    indice++;
                }
            }

            if (pila.Count > 0 && pila.Peek() == noTerminal)
            {
                pila.Pop();
            }

            while (pila.Count > 0 &&
                   pila.Peek() != "INS" &&
                   pila.Peek() != "INS_CASO" &&
                   pila.Peek() != FinEntrada)
            {
                pila.Pop();
            }
        }

        private bool EsInicioInstruccion(string token)
        {
            return token.StartsWith("PR") || token == "IDV" || token == "IDF";
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
                case "PR11": return "SI";
                case "PR12": return "SINO";
                case "PR13": return "ENCASO";
                case "PR14": return "DEFECTO (DFCT)";
                case "PR15": return "MIENTRAS (MIENT)";
                case "PR16": return "REPETIR (REPT)";
                case "PR17": return "HASTA";
                case "PR18": return "POR";
                case "PR19": return "ROMPER";
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
                case "EOF": return "fin de archivo (EOF)";
                default:
                    if (token.StartsWith("PR")) return $"la palabra reservada {token}";
                    if (token.StartsWith("OPA")) return "un operador aritmético";
                    if (token.StartsWith("OPR")) return "un operador relacional";
                    if (token.StartsWith("OPL")) return "un operador lógico";
                    return $"'{token}'";
            }
        }

        private string ObtenerAyudaSintaxis(string noTerminal)
        {
            switch (noTerminal)
            {
                case "IN01": return "Sintaxis esperada: INI { <instrucciones> }";
                case "IN04": return "Sintaxis de SI: SI (<Condicion>) { <Instrucciones> } SINO { <Instrucciones> }";
                case "IN05": return "Sintaxis de ENCASO: ENCASO (<IDV>) { <Valor>: <Instrucciones> ROMPER; DFCT: <Instrucciones> ROMPER; }";
                case "INS_CASO": return "Dentro de ENCASO cada caso debe terminar con ROMPER; antes del siguiente caso o DFCT.";
                case "IN06": return "Sintaxis de MIENTRAS: MIENT (<Condicion>) { <Instrucciones> }";
                case "IN07": return "Sintaxis de REPETIR: REPT { <Instrucciones> } HASTA (<Condicion>);";
                case "IN08": return "Sintaxis de POR: POR (<Asignacion> <Condicion>; <Paso>) { <Instrucciones> }";
                case "IN19": return "Sintaxis de FUNCION: FUNC <Tipo> <IDF> (<Parametros>) { <Instrucciones> REGR <Valor>; }";
                case "EXP":
                case "VALOR": return "Falta un operando (variable, número o cadena).";
                case "POT_P":
                case "TERM_P":
                case "EXP_P": return "Falta operador aritmético o terminar la línea con ';'.";
                case "COND": return "Se esperaba una condición lógica o relacional válida.";
                case "COND_REL": return "Se esperaba una expresión relacional o una condición entre paréntesis.";
                case "TIPO_VAR": return "Se requiere declarar el tipo de dato (ENT, DEC, TXT, BOOL, CAR).";
                case "ASIG_OPC": return "Falta el operador de asignación '=' o el cierre con ';'.";
                default: return "Verifique la escritura de la instrucción según el lenguaje SharpC.";
            }
        }
    }
}
