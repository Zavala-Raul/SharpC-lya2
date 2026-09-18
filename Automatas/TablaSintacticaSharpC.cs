using System.Collections.Generic;
using System.Linq;

namespace Automatas
{
    internal static class TablaSintacticaSharpC
    {
        private static readonly List<string> TiposVariable = new List<string> { "PR23", "PR24", "PR25", "PR26", "PR27" };
        private static readonly List<string> OperadoresRelacionales = new List<string> { "OPR1", "OPR2", "OPR3", "OPR4", "OPR5", "OPR6" };
        private static readonly List<string> FirstExpresion = new List<string> { "IDV", "CNU", "CAD", "CAR", "PR20", "PR21", "PR22", "IDF", "CE1" };

        public static Dictionary<string, Dictionary<string, List<string>>> CrearTablaCompleta()
        {
            var tabla = new TablaLl1Builder();
            List<string> firstCondicion = new List<string>(FirstExpresion) { "OPL3" };
            List<string> firstInstruccion = CrearFirstInstruccion();

            AgregarBloquePrincipal(tabla, firstInstruccion);
            AgregarInstrucciones(tabla);
            AgregarDeclaracionesYAsignaciones(tabla);
            AgregarEstructurasDeControl(tabla, firstInstruccion);
            AgregarEntradaSalidaYGraficas(tabla);
            AgregarFunciones(tabla);
            AgregarCondiciones(tabla, firstCondicion);
            AgregarExpresiones(tabla);

            return tabla.Tabla;
        }

        public static Dictionary<string, Dictionary<string, List<string>>> CrearTablaCondicion()
        {
            var tabla = new TablaLl1Builder();
            List<string> firstCondicion = new List<string>(FirstExpresion) { "OPL3" };

            tabla.Agregar("S", firstCondicion, "COND");
            AgregarCondiciones(tabla, firstCondicion);
            AgregarExpresiones(tabla);
            AgregarFuncionesEnExpresiones(tabla);

            return tabla.Tabla;
        }

        private static List<string> CrearFirstInstruccion()
        {
            var firstInstruccion = new List<string>
            {
                "IDV", "PR11", "PR13", "PR15", "PR16", "PR18", "PR5", "PR6", "PR7", "PR8",
                "PR9", "PR10", "PR29", "PR30", "PR31", "PR2", "PR19", "PR3", "IDF"
            };
            firstInstruccion.InsertRange(0, TiposVariable);
            return firstInstruccion;
        }

        private static void AgregarBloquePrincipal(TablaLl1Builder tabla, List<string> firstInstruccion)
        {
            tabla.Agregar("S", "PR1", "IN01");
            tabla.Agregar("IN01", "PR1", "PR1", "CE3", "INS", "CE4");

            tabla.AgregarListaInstrucciones("INS", firstInstruccion, new[] { "CE4", "EOF" });
            tabla.AgregarListaInstrucciones("INS_CASO", firstInstruccion.Where(t => t != "PR19"), new[] { "PR19" });
        }

        private static void AgregarInstrucciones(TablaLl1Builder tabla)
        {
            tabla.Agregar("IN", TiposVariable, "IN02");
            tabla.Agregar("IN", "IDV", "IN03");
            tabla.Agregar("IN", "PR11", "IN04");
            tabla.Agregar("IN", "PR13", "IN05");
            tabla.Agregar("IN", "PR15", "IN06");
            tabla.Agregar("IN", "PR16", "IN07");
            tabla.Agregar("IN", "PR18", "IN08");
            tabla.Agregar("IN", "PR5", "IN10");
            tabla.Agregar("IN", "PR6", "IN11");
            tabla.Agregar("IN", "PR7", "IN12");
            tabla.Agregar("IN", "PR8", "IN13");
            tabla.Agregar("IN", "PR9", "IN14");
            tabla.Agregar("IN", "PR10", "IN15");
            tabla.Agregar("IN", "PR29", "IN16");
            tabla.Agregar("IN", "PR30", "IN17");
            tabla.Agregar("IN", "PR31", "IN18");
            tabla.Agregar("IN", "PR2", "IN19");
            tabla.Agregar("IN", "PR19", "IN20");
            tabla.Agregar("IN", "PR3", "IN21");
            tabla.Agregar("IN", "IDF", "IN22");
        }

        private static void AgregarDeclaracionesYAsignaciones(TablaLl1Builder tabla)
        {
            tabla.Agregar("IN02", TiposVariable, "TIPO_VAR", "IDV", "ASIG_OPC", "CE8");
            tabla.Agregar("TIPO_VAR", TiposVariable, token => new[] { token });

            tabla.Agregar("ASIG_OPC", "ASIG", "ASIG", "EXP");
            tabla.Agregar("ASIG_OPC", "CE8", "e");

            tabla.Agregar("IN03", "IDV", "IDV", "ASIG", "EXP_IO", "CE8");
            tabla.Agregar("EXP_IO", "PR4", "PR4", "CE1", "CE2");
            tabla.Agregar("EXP_IO", FirstExpresion, "EXP");
        }

        private static void AgregarEstructurasDeControl(TablaLl1Builder tabla, List<string> firstInstruccion)
        {
            tabla.Agregar("IN04", "PR11", "PR11", "CE1", "COND", "CE2", "CE3", "INS", "CE4", "IN04_1");
            tabla.Agregar("IN04_1", "PR12", "PR12", "CE3", "INS", "CE4");
            tabla.Agregar("IN04_1", firstInstruccion.Concat(new[] { "CE4", "EOF", "PR14" }).Where(t => t != "PR12"), "e");

            tabla.Agregar("IN05", "PR13", "PR13", "CE1", "IDV", "CE2", "CE3", "CASOS", "CE4");
            tabla.Agregar("CASOS", "CNU", "VAL_CASO", "CE20", "INS_CASO", "PR19", "CE8", "CASOS");
            tabla.Agregar("CASOS", "CAD", "VAL_CASO", "CE20", "INS_CASO", "PR19", "CE8", "CASOS");
            tabla.Agregar("CASOS", "CAR", "VAL_CASO", "CE20", "INS_CASO", "PR19", "CE8", "CASOS");
            tabla.Agregar("CASOS", "PR14", "PR14", "CE20", "INS_CASO", "PR19", "CE8");
            tabla.Agregar("VAL_CASO", "CNU", "CNU");
            tabla.Agregar("VAL_CASO", "CAD", "CAD");
            tabla.Agregar("VAL_CASO", "CAR", "CAR");

            tabla.Agregar("IN06", "PR15", "PR15", "CE1", "COND", "CE2", "CE3", "INS", "CE4");
            tabla.Agregar("IN07", "PR16", "PR16", "CE3", "INS", "CE4", "PR17", "CE1", "COND", "CE2", "CE8");

            tabla.Agregar("IN08", "PR18", "PR18", "CE1", "INIT_POR", "CE8", "COND", "CE8", "IDV", "ASIG", "EXP", "CE2", "CE3", "INS", "CE4");
            tabla.Agregar("INIT_POR", TiposVariable, "TIPO_VAR", "IDV", "ASIG", "EXP");
            tabla.Agregar("INIT_POR", "IDV", "IDV", "ASIG", "EXP");

            tabla.Agregar("IN20", "PR19", "PR19", "CE8");
        }

        private static void AgregarEntradaSalidaYGraficas(TablaLl1Builder tabla)
        {
            tabla.Agregar("IN10", "PR5", "PR5", "CE1", "EXP", "CE2", "CE8");
            tabla.Agregar("IN11", "PR6", "PR6", "CE1", "CE2", "CE8");
            tabla.Agregar("IN12", "PR7", "PR7", "CE1", "CAD", "CE2", "CE8");
            tabla.Agregar("IN13", "PR8", "PR8", "CE1", "CAD", "CE2", "CE8");
            tabla.Agregar("IN14", "PR9", "PR9", "CE1", "CNU", "CE2", "CE8");
            tabla.Agregar("IN15", "PR10", "PR10", "CE1", "CNU", "CE7", "CNU", "CE2", "CE8");
            tabla.Agregar("IN16", "PR29", "PR29", "CE1", "TIPO_VAR", "CE7", "IDV", "CE7", "IDV", "CE2", "CE8");
            tabla.Agregar("IN17", "PR30", "PR30", "CE1", "CE2", "CE8");
            tabla.Agregar("IN18", "PR31", "PR31", "CE1", "CAD", "CE7", "CNU", "CE2", "CE8");
        }

        private static void AgregarFunciones(TablaLl1Builder tabla)
        {
            tabla.Agregar("IN19", "PR2", "PR2", "TIPO_RET", "IDF", "CE1", "PARAM", "CE2", "CE3", "INS", "CE4");
            tabla.Agregar("TIPO_RET", TiposVariable, "TIPO_VAR");
            tabla.Agregar("TIPO_RET", "PR28", "PR28");

            tabla.Agregar("PARAM", TiposVariable, "TIPO_VAR", "IDV", "PARAM_P");
            tabla.Agregar("PARAM", "CE2", "e");
            tabla.Agregar("PARAM_P", "CE7", "CE7", "TIPO_VAR", "IDV", "PARAM_P");
            tabla.Agregar("PARAM_P", "CE2", "e");

            tabla.Agregar("IN21", "PR3", "PR3", "EXP_OPC", "CE8");
            tabla.Agregar("EXP_OPC", FirstExpresion, "EXP");
            tabla.Agregar("EXP_OPC", "CE8", "e");

            tabla.Agregar("IN22", "IDF", "CALL_FUNC", "CE8");
            AgregarFuncionesEnExpresiones(tabla);
        }

        private static void AgregarFuncionesEnExpresiones(TablaLl1Builder tabla)
        {
            tabla.Agregar("CALL_FUNC", "IDF", "IDF", "CE1", "ARG", "CE2");
            tabla.Agregar("ARG", FirstExpresion, "EXP", "ARG_P");
            tabla.Agregar("ARG", "CE2", "e");
            tabla.Agregar("ARG_P", "CE7", "CE7", "EXP", "ARG_P");
            tabla.Agregar("ARG_P", "CE2", "e");
        }

        private static void AgregarCondiciones(TablaLl1Builder tabla, List<string> firstCondicion)
        {
            tabla.Agregar("COND", firstCondicion, "COND_OR");
            tabla.Agregar("COND_OR", firstCondicion, "COND_AND", "COND_OR_P");

            tabla.Agregar("COND_OR_P", "OPL2", "OPL2", "COND_AND", "COND_OR_P");
            tabla.Agregar("COND_OR_P", "CE2", "e");
            tabla.Agregar("COND_OR_P", "CE8", "e");

            tabla.Agregar("COND_AND", firstCondicion, "COND_NOT", "COND_AND_P");
            tabla.Agregar("COND_AND_P", "OPL1", "OPL1", "COND_NOT", "COND_AND_P");
            tabla.Agregar("COND_AND_P", "OPL2", "e");
            tabla.Agregar("COND_AND_P", "CE2", "e");
            tabla.Agregar("COND_AND_P", "CE8", "e");

            tabla.Agregar("COND_NOT", "OPL3", "OPL3", "COND_NOT");
            tabla.Agregar("COND_NOT", FirstExpresion, "COND_REL");

            tabla.Agregar("COND_REL", FirstExpresion.Where(t => t != "CE1"), "EXP", "REL_OPC");
            tabla.Agregar("COND_REL", "CE1", "CE1", "COND", "CE2");

            tabla.Agregar("REL_OPC", OperadoresRelacionales, token => new[] { token, "EXP" });
            tabla.Agregar("REL_OPC", "CE2", "e");
            tabla.Agregar("REL_OPC", "CE8", "e");
            tabla.Agregar("REL_OPC", "OPL1", "e");
            tabla.Agregar("REL_OPC", "OPL2", "e");
        }

        private static void AgregarExpresiones(TablaLl1Builder tabla)
        {
            tabla.Agregar("EXP", FirstExpresion, "TERM", "EXP_P");

            tabla.Agregar("EXP_P", "OPA+", "OPA+", "TERM", "EXP_P");
            tabla.Agregar("EXP_P", "OPA-", "OPA-", "TERM", "EXP_P");
            tabla.Agregar("EXP_P", "CE2", "e");
            tabla.Agregar("EXP_P", "CE8", "e");
            tabla.Agregar("EXP_P", "CE7", "e");
            tabla.Agregar("EXP_P", OperadoresRelacionales, "e");
            tabla.Agregar("EXP_P", "OPL1", "e");
            tabla.Agregar("EXP_P", "OPL2", "e");

            tabla.Agregar("TERM", FirstExpresion, "POT", "TERM_P");
            tabla.Agregar("TERM_P", "OPA*", "OPA*", "POT", "TERM_P");
            tabla.Agregar("TERM_P", "OPA/", "OPA/", "POT", "TERM_P");
            tabla.Agregar("TERM_P", "OPA+", "e");
            tabla.Agregar("TERM_P", "OPA-", "e");
            tabla.Agregar("TERM_P", "CE2", "e");
            tabla.Agregar("TERM_P", "CE8", "e");
            tabla.Agregar("TERM_P", "CE7", "e");
            tabla.Agregar("TERM_P", OperadoresRelacionales, "e");
            tabla.Agregar("TERM_P", "OPL1", "e");
            tabla.Agregar("TERM_P", "OPL2", "e");

            tabla.Agregar("POT", FirstExpresion, "VALOR", "POT_P");
            tabla.Agregar("POT_P", "OPA^", "OPA^", "POT");
            tabla.Agregar("POT_P", "OPA*", "e");
            tabla.Agregar("POT_P", "OPA/", "e");
            tabla.Agregar("POT_P", "OPA+", "e");
            tabla.Agregar("POT_P", "OPA-", "e");
            tabla.Agregar("POT_P", "CE2", "e");
            tabla.Agregar("POT_P", "CE8", "e");
            tabla.Agregar("POT_P", "CE7", "e");
            tabla.Agregar("POT_P", OperadoresRelacionales, "e");
            tabla.Agregar("POT_P", "OPL1", "e");
            tabla.Agregar("POT_P", "OPL2", "e");

            tabla.Agregar("VALOR", "IDV", "IDV");
            tabla.Agregar("VALOR", "CNU", "CNU");
            tabla.Agregar("VALOR", "CAD", "CAD");
            tabla.Agregar("VALOR", "CAR", "CAR");
            tabla.Agregar("VALOR", "PR20", "PR20");
            tabla.Agregar("VALOR", "PR21", "PR21");
            tabla.Agregar("VALOR", "PR22", "PR22");
            tabla.Agregar("VALOR", "IDF", "CALL_FUNC");
            tabla.Agregar("VALOR", "CE1", "CE1", "EXP", "CE2");
        }

        private class TablaLl1Builder
        {
            public Dictionary<string, Dictionary<string, List<string>>> Tabla { get; } =
                new Dictionary<string, Dictionary<string, List<string>>>();

            public void Agregar(string noTerminal, string token, params string[] produccion)
            {
                if (!Tabla.ContainsKey(noTerminal))
                    Tabla[noTerminal] = new Dictionary<string, List<string>>();

                Tabla[noTerminal][token] = new List<string>(produccion);
            }

            public void Agregar(string noTerminal, IEnumerable<string> tokens, params string[] produccion)
            {
                foreach (string token in tokens.Distinct())
                    Agregar(noTerminal, token, produccion);
            }

            public void Agregar(string noTerminal, IEnumerable<string> tokens, System.Func<string, string[]> crearProduccion)
            {
                foreach (string token in tokens.Distinct())
                    Agregar(noTerminal, token, crearProduccion(token));
            }

            public void AgregarListaInstrucciones(string noTerminal, IEnumerable<string> primeros, IEnumerable<string> seguidores)
            {
                Agregar(noTerminal, primeros, "IN", noTerminal);
                Agregar(noTerminal, seguidores, "e");
            }
        }
    }
}
