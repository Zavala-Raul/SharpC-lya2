namespace Automatas
{
    internal static class OperadoresExpresion
    {
        public static bool EsOperador(string tipoToken, string valor)
        {
            return tipoToken.StartsWith("OPA") || tipoToken.StartsWith("OPR") || tipoToken.StartsWith("OPL") ||
                   valor == "+" || valor == "-" || valor == "*" || valor == "/" || valor == "^" ||
                   valor == "==" || valor == "<>" || valor == "<" || valor == ">" || valor == "<=" || valor == ">=" ||
                   valor == "&&" || valor == "||";
        }

        public static string Normalizar(string tipoToken, string valor)
        {
            switch (valor)
            {
                case "+": case "-": case "*": case "/": case "^":
                case "==": case "<>": case "<": case ">": case "<=": case ">=":
                case "!": return valor;
                case "&": case "&&": return "&&";
                case "|": case "||": return "||";
            }
            switch (tipoToken)
            {
                case "OPA+": return "+";
                case "OPA-": return "-";
                case "OPA*": return "*";
                case "OPA/": return "/";
                case "OPA^": return "^";
                case "OPR1": return ">";
                case "OPR2": return "<";
                case "OPR3": return ">=";
                case "OPR4": return "<=";
                case "OPR5": return "<>";
                case "OPR6": return "==";
                case "OPL1": return "&&";
                case "OPL2": return "||";
                case "OPL3": return "!";
                default: return valor;
            }
        }

        public static int Precedencia(string op)
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
                case "!": return 2;
                case "&&": return 1;
                case "||": return 0;
                default: return -1;
            }
        }

        public static bool DebeReducir(string pendiente, string nuevo)
        {
            return pendiente != "(" && (Precedencia(pendiente) > Precedencia(nuevo) ||
                (Precedencia(pendiente) == Precedencia(nuevo) && nuevo != "^"));
        }
    }
}
