using System;
using System.Collections.Generic;

namespace Automatas
{
    internal static class TokenizadorFuenteSharpC
    {
        public static List<(int linea, int columna, string token)> TokenizarLinea(string linea, int numLinea)
        {
            var tokens = new List<(int linea, int columna, string token)>();
            string texto = NormalizarCaracteresFuente(linea);
            int indice = 0;

            while (indice < texto.Length)
            {
                if (char.IsWhiteSpace(texto[indice]))
                {
                    indice++;
                    continue;
                }

                if (EsComentario(texto, indice))
                {
                    tokens.Add((numLinea, indice, texto.Substring(indice).TrimEnd('\r', '\n')));
                    break;
                }

                if (EsCadenaOCaracter(texto[indice]))
                {
                    int inicio = indice;
                    indice = AvanzarCadenaOCaracter(texto, indice);
                    tokens.Add((numLinea, inicio, texto.Substring(inicio, indice - inicio)));
                    continue;
                }

                if (EsOperadorDosCaracteres(texto, indice))
                {
                    tokens.Add((numLinea, indice, texto.Substring(indice, 2)));
                    indice += 2;
                    continue;
                }

                if (EsInicioNumero(texto, indice, tokens))
                {
                    int inicio = indice;
                    indice = AvanzarNumero(texto, indice);
                    tokens.Add((numLinea, inicio, texto.Substring(inicio, indice - inicio)));
                    continue;
                }

                if (char.IsLetter(texto[indice]))
                {
                    int inicio = indice++;
                    while (indice < texto.Length && char.IsLetterOrDigit(texto[indice]))
                        indice++;

                    tokens.Add((numLinea, inicio, texto.Substring(inicio, indice - inicio)));
                    continue;
                }

                if (EsSimboloSimple(texto[indice]))
                {
                    tokens.Add((numLinea, indice, texto[indice].ToString()));
                    indice++;
                    continue;
                }

                int inicioToken = indice;
                while (indice < texto.Length &&
                       !char.IsWhiteSpace(texto[indice]) &&
                       !EsSimboloSimple(texto[indice]))
                {
                    indice++;
                }

                tokens.Add((numLinea, inicioToken, texto.Substring(inicioToken, indice - inicioToken)));
            }

            return tokens;
        }

        private static string NormalizarCaracteresFuente(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            return texto
                .Replace('“', '"')
                .Replace('”', '"')
                .Replace('‘', '\'')
                .Replace('’', '\'');
        }

        private static bool EsComentario(string texto, int indice)
        {
            return indice + 1 < texto.Length && texto[indice] == '/' && texto[indice + 1] == '/';
        }

        private static bool EsCadenaOCaracter(char c)
        {
            return c == '"' || c == '\'';
        }

        private static int AvanzarCadenaOCaracter(string texto, int indice)
        {
            char cierre = texto[indice];
            indice++;

            while (indice < texto.Length && texto[indice] != cierre)
                indice++;

            if (indice < texto.Length && texto[indice] == cierre)
                indice++;

            return indice;
        }

        private static bool EsOperadorDosCaracteres(string texto, int indice)
        {
            if (indice + 1 >= texto.Length)
                return false;

            string dos = texto.Substring(indice, 2);
            return dos == ">=" || dos == "<=" || dos == "<>" || dos == "==";
        }

        private static bool EsInicioNumero(string texto, int indice, List<(int linea, int columna, string token)> tokens)
        {
            if (char.IsDigit(texto[indice]))
                return true;

            if ((texto[indice] == '+' || texto[indice] == '-') &&
                indice + 1 < texto.Length &&
                char.IsDigit(texto[indice + 1]))
            {
                if (tokens.Count == 0)
                    return true;

                return PermiteNumeroConSigno(tokens[tokens.Count - 1].token);
            }

            return false;
        }

        private static bool PermiteNumeroConSigno(string tokenPrevio)
        {
            switch (tokenPrevio)
            {
                case "(":
                case "{":
                case "[":
                case ",":
                case ":":
                case "=":
                case "+":
                case "-":
                case "*":
                case "/":
                case "^":
                case "&":
                case "|":
                case "!":
                case ">":
                case "<":
                case ">=":
                case "<=":
                case "<>":
                case "==":
                    return true;
                default:
                    return false;
            }
        }

        private static int AvanzarNumero(string texto, int indice)
        {
            int j = indice;

            if (j < texto.Length && (texto[j] == '+' || texto[j] == '-'))
                j++;

            while (j < texto.Length && char.IsDigit(texto[j]))
                j++;

            if (j < texto.Length && texto[j] == '.')
            {
                j++;
                while (j < texto.Length && char.IsDigit(texto[j]))
                    j++;
            }

            if (j < texto.Length && (texto[j] == 'E' || texto[j] == 'e'))
            {
                int inicioExponente = j;
                j++;

                if (j < texto.Length && (texto[j] == '+' || texto[j] == '-'))
                    j++;

                int inicioDigitos = j;
                while (j < texto.Length && char.IsDigit(texto[j]))
                    j++;

                if (inicioDigitos == j)
                    j = inicioExponente;
            }

            return j;
        }

        private static bool EsSimboloSimple(char c)
        {
            return "(){}[];,.:+-*/^&|!><=¡@#$%?~\\".IndexOf(c) >= 0;
        }
    }
}
