using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Automatas;

internal static class Program
{
    private enum ResultadoEsperado { Valido, ErrorSintactico, ErrorSemantico }

    private sealed class Caso
    {
        public string Nombre;
        public string Fuente;
        public ResultadoEsperado Resultado;
        public bool SoloCondicion;
        public List<(int Linea, string Mensaje)> Esperados = new List<(int, string)>();
        public Dictionary<string, Funcion> Funciones = new Dictionary<string, Funcion>();
    }

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var casos = CrearCasos();
        int fallos = 0;
        foreach (var caso in casos)
        {
            try
            {
                Ejecutar(caso);
                Console.WriteLine("OK    " + caso.Nombre);
            }
            catch (Exception ex)
            {
                fallos++;
                Console.WriteLine("FALLO " + caso.Nombre);
                Console.WriteLine("  Fuente: " + caso.Fuente.Replace("\n", " | "));
                Console.WriteLine("  " + ex.Message);
            }
        }

        var ambitos = CrearCasosAmbito();
        foreach (var (nombre, accion) in ambitos)
        {
            try
            {
                accion();
                Console.WriteLine("OK    " + nombre);
            }
            catch (Exception ex)
            {
                fallos++;
                Console.WriteLine("FALLO " + nombre);
                Console.WriteLine("  " + ex.Message);
            }
        }

        int total = casos.Count + ambitos.Count;
        Console.WriteLine($"\nTotal: {total}; correctas: {total - fallos}; fallidas: {fallos}.");
        return fallos == 0 ? 0 : 1;
    }

    private static List<Caso> CrearCasos()
    {
        var casos = new List<Caso>
        {
            // Controles válidos: se exige que ambas listas de errores estén vacías.
            Valido("V01 Bloque vacío", ""),
            Valido("V02 Tipos básicos", "ENT vA = 2; DEC vB = 2.5; TXT vC = \"hola\"; BOOL vD = VDD; CAR vE = 'a';"),
            Valido("V03 Declaración simple y reasignación", "ENT vA; vA = 3;"),
            Valido("V04 Promoción ENT a DEC", "DEC vA = 5 / 2;"),
            Valido("V05 Prioridad de multiplicación sobre concatenación", "TXT vA = \"total\" + 2 * 3;"),
            Valido("V06 Paréntesis y potencias encadenadas", "DEC vA = (2 + 3) * 4.5 ^ 2 ^ 3;"),
            Valido("V07 Condición numérica y negación agrupada", "ENT vA = 3; SI (!(vA < 2)) { vA = vA + 1; }"),
            Valido("V08 Operadores lógicos", "SI ((2 < 3) & VDD | FLS) { }"),
            Valido("V09 MIENT y HASTA booleanos", "MIENT (VDD) { } REPT { } HASTA (FLS);"),
            Valido("V10 POR con declaración", "POR (ENT vI = 0; vI < 3; vI = vI + 1) { }"),
            Valido("V11 POR con variable ya declarada", "ENT vI; POR (vI = 0; vI < 3; vI = vI + 1) { }"),

            // Sintaxis: comprobar línea y fragmento específico, permitiendo errores en cascada.
            Sintaxis("S01 Falta INI", "{\n}", 1, "El programa debe comenzar con INI"),
            Sintaxis("S02 Falta llave de apertura", "INI\nENT vA = 1;\n}", 2, "OMISIÓN: Falta llave de apertura '{'"),
            Sintaxis("S03 Falta llave final", "INI {\nENT vA = 1;", 2, "OMISIÓN: Falta llave de cierre '}'"),
            Sintaxis("S04 Falta punto y coma", EnPrograma("ENT vA = 1"), 3, "OMISIÓN: Falta punto y coma ';'"),
            Sintaxis("S05 Falta identificador", EnPrograma("ENT = 1;"), 2, "OMISIÓN: Falta un identificador de variable"),
            Sintaxis("S06 Falta asignación", EnPrograma("ENT vA;\nvA 3;"), 3, "OMISIÓN: Falta operador de asignación '='"),
            Sintaxis("S07 Falta expresión inicial", EnPrograma("ENT vA = ;"), 2, "Falta un operando (variable, número o cadena)."),
            Sintaxis("S08 Falta operador entre números", EnPrograma("ENT vA = 2 3;"), 2, "Falta operador aritmético o terminar la línea con ';'."),
            Sintaxis("S09 Operadores consecutivos", EnPrograma("ENT vA = 2 + * 3;"), 2, "SINTAXIS INVÁLIDA: Token inesperado '*'"),
            Sintaxis("S10 Falta paréntesis de cierre", EnPrograma("ENT vA = (2 + 3;"), 2, "OMISIÓN: Falta paréntesis de cierre ')'"),
            Sintaxis("S11 Falta condición de SI", EnPrograma("SI () { }"), 2, "Se esperaba una condición lógica o relacional válida."),
            Sintaxis("S12 Token extra antes de variable", EnPrograma("ENT , vA = 1;"), 2, "TOKEN EXTRA: ',' no esperado antes de un identificador de variable"),

            // Semántica: antes se exige sintaxis válida; se compara también el número de errores.
            Semantica("M01 TXT asignado a ENT", "ENT vA = \"hola\";", 2,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vA' de tipo 'ENT'."),
            Semantica("M02 DEC asignado a ENT", "ENT vA = 2.5;", 2,
                "No se puede asignar una expresión de tipo 'DEC' a la variable 'vA' de tipo 'ENT'."),
            Semantica("M03 Reasignación incompatible", "ENT vA = 1;\nvA = \"hola\";", 3,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vA' de tipo 'ENT'."),
            Semantica("M04 Destino no declarado", "vAusente = 1;", 2,
                "La variable 'vAusente' no ha sido declarada."),
            Semantica("M05 Operando no declarado", "ENT vA = vAusente + 1;", 2,
                "La variable 'vAusente' no ha sido declarada."),
            Semantica("M06 Función no declarada", "ENT vA = fAusente();", 2,
                "La función 'fAusente' no ha sido declarada."),
            Semantica("M07 Multiplicación de texto", "ENT vA = \"hola\" * 2;", 2,
                "El operador aritmético '*' no es válido entre 'TXT' y 'ENT'."),
            Semantica("M08 Resta de booleano", "ENT vA = VDD - 1;", 2,
                "El operador aritmético '-' no es válido entre 'BOOL' y 'ENT'."),
            Semantica("M09 Paréntesis cambian la operación incompatible", "TXT vA = (\"hola\" + 2) * 3;", 2,
                "El operador aritmético '*' no es válido entre 'TXT' y 'ENT'."),
            Semantica("M10 Comparación incompatible", "SI (\"hola\" < 2) { }", 2,
                "No se pueden comparar tipos incompatibles 'TXT' y 'ENT' con '<'."),
            Semantica("M11 AND requiere booleanos", "SI (2 & VDD) { }", 2,
                "El operador lógico '&&' requiere operandos 'BOOL', pero recibió 'ENT' y 'BOOL'."),
            Semantica("M12 OR requiere booleanos", "SI (FLS | 2) { }", 2,
                "El operador lógico '||' requiere operandos 'BOOL', pero recibió 'BOOL' y 'ENT'."),
            Semantica("M13 NOT requiere booleano", "SI (!2) { }", 2,
                "El operador lógico '!' requiere un operando 'BOOL', pero recibió 'ENT'."),
            Semantica("M14 SI requiere BOOL", "SI (2 + 3) { }", 2,
                "La condición debe evaluar a 'BOOL', pero se obtuvo 'ENT'."),
            Semantica("M15 MIENT requiere BOOL", "MIENT (2.5) { }", 2,
                "La condición debe evaluar a 'BOOL', pero se obtuvo 'DEC'."),
            Semantica("M16 HASTA requiere BOOL", "REPT { } HASTA (\"hola\");", 2,
                "La condición debe evaluar a 'BOOL', pero se obtuvo 'TXT'."),
            Semantica("M17 POR condición no booleana", "POR (ENT vI = 0; vI + 1; vI = vI + 1) { }", 2,
                "La condición debe evaluar a 'BOOL', pero se obtuvo 'ENT'."),
            Semantica("M18 POR inicialización con declaración incompatible", "POR (ENT vI = \"hola\"; vI < 3; vI = vI + 1) { }", 2,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vI' de tipo 'ENT'."),
            Semantica("M19 POR incremento incompatible", "POR (ENT vI = 0; vI < 3; vI = \"hola\") { }", 2,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vI' de tipo 'ENT'."),
            Semantica("M20 Error interno sin diagnóstico duplicado", "ENT vA = (\"hola\" * 2) + 1;", 2,
                "El operador aritmético '*' no es válido entre 'TXT' y 'ENT'."),
            Semantica("M21 Línea del operando desconocido", "ENT vA =\nvAusente + 1;", 3,
                "La variable 'vAusente' no ha sido declarada."),

            // Regresiones: requisitos esperados, no se ocultan fallos conocidos como éxitos.
            Semantica("R01 POR inicialización sin declaración incompatible", "ENT vI;\nPOR (vI = \"hola\"; vI < 3; vI = vI + 1) { }", 3,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vI' de tipo 'ENT'."),
            Valido("R02 Negación de comparación según gramática", "SI (!2 < 3) { }"),
            Valido("R03 Comparación de expresión aritmética agrupada", "SI ((2 + 3) < 6) { }"),
            Semantica("R04 POR destino inicial no declarado", "ENT vI; POR (vAusente = 0; VDD; vI = vI + 1) { }", 2,
                "La variable 'vAusente' no ha sido declarada."),
            Semantica("R05 POR destino del incremento no declarado", "POR (ENT vI = 0; vI < 3; vAusente = 1) { }", 2,
                "La variable 'vAusente' no ha sido declarada."),
            Semantica("R06 POR inicialización multilínea", "ENT vI;\nPOR (\nvI = \"hola\";\nvI < 3;\nvI = vI + 1) { }", 4,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vI' de tipo 'ENT'."),
            Semantica("R07 POR incremento multilínea", "POR (ENT vI = 0;\nvI < 3;\nvI = \"hola\") { }", 4,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vI' de tipo 'ENT'."),
            Valido("R08 POR promoción sin declaración", "DEC vI; POR (vI = 0; vI < 3; vI = vI + 0.5) { }"),
            Valido("R09 Negaciones sucesivas y operadores lógicos", "SI (!2 + 3 < 6 & !!VDD | !FLS) { }"),
            Valido("R10 Negación explícita de comparación", "SI (!(2 < 3)) { }"),
            Semantica("R11 Negación de aritmética no booleana", "SI (!2 + 3) { }", 2,
                "El operador lógico '!' requiere un operando 'BOOL', pero recibió 'ENT'."),
            Semantica("R12 Paréntesis fuerzan negación inválida", "SI ((!2) < 3) { }", 2,
                "El operador lógico '!' requiere un operando 'BOOL', pero recibió 'ENT'."),
            Valido("R13 Aritmética después de cerrar paréntesis", "SI ((2 + 3) * 4 ^ 2 < (9 + 1) * 10) { }"),
            Valido("R14 Grupos anidados a ambos lados", "SI (((2 + 3)) < ((6))) { }"),
            Valido("R15 Grupos booleanos y comparación booleana", "SI (((2 < 3) & (4 > 1)) == VDD) { }"),
            Semantica("R16 Condición agrupada no booleana", "SI (((2 + 3))) { }", 2,
                "La condición debe evaluar a 'BOOL', pero se obtuvo 'ENT'."),
            Semantica("R17 Aritmética inválida de grupo booleano", "SI ((2 < 3) + 1 < 4) { }", 2,
                "El operador aritmético '+' no es válido entre 'BOOL' y 'ENT'."),
            Semantica("R18 Comparación incompatible tras grupo", "SI ((\"hola\" + \" mundo\") < 2) { }", 2,
                "No se pueden comparar tipos incompatibles 'TXT' y 'ENT' con '<'."),
            Sintaxis("R19 Grupo aritmético incompleto", EnPrograma("SI ((2 + ) < 6) { }"), 2,
                "SINTAXIS INVÁLIDA: Token inesperado ')'"),
            Sintaxis("R20 Comparaciones encadenadas sin agrupación", EnPrograma("SI (1 < 2 < 3) { }"), 2,
                "SINTAXIS INVÁLIDA: Token inesperado '<'"),
            Valido("R21 Grupos en las condiciones de ciclos", "MIENT ((2 + 3) < 6) { } REPT { } HASTA ((2 + 3) < 6); POR (ENT vI = 0; (vI + 1) < 3; vI = vI + 1) { }"),
            Semantica("R22 POR condición multilínea", "POR (ENT vI = 0;\nvI + 1;\nvI = vI + 1) { }", 3,
                "La condición debe evaluar a 'BOOL', pero se obtuvo 'ENT'.")
        };

        var recuperacion = Sintaxis("S13 Recuperación y dos omisiones", EnPrograma("ENT vA = 1\nENT vB = 2\nENT vC = 3;"),
            3, "OMISIÓN: Falta punto y coma ';'");
        recuperacion.Esperados.Add((4, "OMISIÓN: Falta punto y coma ';'"));
        casos.Add(recuperacion);

        var multiples = Semantica("M22 Dos errores independientes", "ENT vA = \"hola\";\nDEC vB = VDD;", 2,
            "No se puede asignar una expresión de tipo 'TXT' a la variable 'vA' de tipo 'ENT'.");
        multiples.Esperados.Add((3, "ERROR DE TIPO: No se puede asignar una expresión de tipo 'BOOL' a la variable 'vB' de tipo 'DEC'."));
        casos.Add(multiples);

        var retorno = Semantica("M23 Tipo de retorno incompatible", "ENT vA = fDecimal();", 2,
            "No se puede asignar una expresión de tipo 'DEC' a la variable 'vA' de tipo 'ENT'.");
        retorno.Funciones.Add("fDecimal", new Funcion { Nombre = "fDecimal", TipoRetorno = "DEC" });
        casos.Add(retorno);

        var llamadaValida = Valido("V12 Función conocida con retorno compatible", "DEC vA = fDecimal();");
        llamadaValida.Funciones.Add("fDecimal", new Funcion { Nombre = "fDecimal", TipoRetorno = "DEC" });
        casos.Add(llamadaValida);

        casos.Add(new Caso { Nombre = "C01 Condición aislada termina en EOF", Fuente = "2 < 3", SoloCondicion = true });
        casos.Add(new Caso { Nombre = "C02 Grupo aritmético en condición aislada", Fuente = "(2 + 3) < 6", SoloCondicion = true });
        casos.Add(new Caso { Nombre = "C03 Negación y lógica en condición aislada", Fuente = "!2 < 3 & !!VDD", SoloCondicion = true });
        var condicionIncompleta = Sintaxis("C04 Condición aislada incompleta", "(2 + ) < 6", 1,
            "SINTAXIS INVÁLIDA: Token inesperado ')'");
        condicionIncompleta.SoloCondicion = true;
        casos.Add(condicionIncompleta);
        casos.AddRange(CrearCasosRangoEntero());
        return casos;
    }

    private static List<Caso> CrearCasosRangoEntero()
    {
        var casos = new List<Caso>
        {
            Valido("N01 Máximo ENT", "ENT vA = 2147483647;"),
            Valido("N02 Mínimo ENT con signo en el literal", "ENT vA = -2147483648;"),
            Valido("N03 Máximo ENT con signo positivo", "ENT vA = +2147483647;"),
            Valido("N04 Ceros y ceros iniciales", "ENT vA = -0; ENT vB = +0; ENT vC = 00000000000000000000000000001;"),
            Rango("N05 Entero superior al máximo", "ENT vA = 2147483648;", 2, "2147483648"),
            Rango("N06 Entero inferior al mínimo", "ENT vA = -2147483649;", 2, "-2147483649"),
            Rango("N07 Fuera de rango con signo positivo", "ENT vA = +2147483648;", 2, "+2147483648"),
            Rango("N08 Literal muy largo sin excepción", "ENT vA = 9999999999999999999999999999999999999999;", 2,
                "9999999999999999999999999999999999999999"),
            Rango("N09 El destino DEC no amplía un literal ENT", "DEC vA = 2147483648;", 2, "2147483648"),
            Rango("N10 Concatenación propaga el error de rango", "TXT vA = \"valor\" + 2147483648;", 2, "2147483648"),
            Rango("N11 Negación sin errores derivados", "SI (!2147483648 < 3) { }", 2, "2147483648"),
            Rango("N12 Línea del literal en expresión multilínea", "ENT vA =\n2147483648 + 1;", 3, "2147483648"),
            Rango("N13 Rango también se comprueba en IMP", "IMP(2147483648);", 2, "2147483648"),
            Rango("N14 Rango también se comprueba en REGR", "FUNC ENT fValor() { REGR 2147483648; }", 2, "2147483648"),
            Rango("N15 Rango en inicialización de POR", "POR (ENT vI = 2147483648; vI < 3; vI = vI + 1) { }", 2, "2147483648"),
            Rango("N16 Mismo literal y línea sin duplicados", "ENT vA = 2147483648 + 2147483648;", 2, "2147483648"),
            Valido("N17 Literal DEC mayor que el máximo ENT", "DEC vA = 2147483648.0;"),
            Valido("N18 Notación científica se clasifica DEC", "DEC vA = 1E3; DEC vB = 1e-3;"),
            Semantica("N19 Notación científica no se confunde con ENT inválido", "ENT vA = 1E3;", 2,
                "No se puede asignar una expresión de tipo 'DEC' a la variable 'vA' de tipo 'ENT'."),
            Valido("N20 Resta y operandos con signo", "ENT vA = 2147483647-1; ENT vB = 0 + -2147483648;")
        };

        var argumento = Rango("N21 Rango en argumento de función conocida", "ENT vA = fIdentidad(2147483648);", 2, "2147483648");
        argumento.Funciones.Add("fIdentidad", new Funcion
        {
            Nombre = "fIdentidad", TipoRetorno = "ENT",
            Parametros = new List<(string, string)> { ("ENT", "vNumero") }
        });
        casos.Add(argumento);

        var lineasDistintas = Rango("N22 Mismo literal en distintas líneas", "ENT vA = 2147483648;\nENT vB = 2147483648;", 2, "2147483648");
        lineasDistintas.Esperados.Add((3, MensajeRangoEsperado("2147483648")));
        casos.Add(lineasDistintas);

        var distintos = Rango("N23 Dos literales inválidos en una línea", "ENT vA = 2147483648 + 2147483649;", 2, "2147483648");
        distintos.Esperados.Add((2, MensajeRangoEsperado("2147483649")));
        casos.Add(distintos);

        casos.Add(new Caso
        {
            Nombre = "N24 Condición aislada también valida el rango", Fuente = "2147483648 < 3",
            SoloCondicion = true, Resultado = ResultadoEsperado.ErrorSemantico,
            Esperados = new List<(int, string)> { (1, MensajeRangoEsperado("2147483648")) }
        });
        return casos;
    }

    // Expectativa independiente de CatalogoTipos: valida el contrato completo del diagnóstico.
    private static string MensajeRangoEsperado(string literal) =>
        $"ERROR DE TIPO: El literal '{literal}' está fuera del rango permitido para ENT (-2147483648 a 2147483647; 4 bytes).";

    private static Caso Rango(string nombre, string cuerpo, int linea, string literal) => new Caso
    {
        Nombre = nombre, Fuente = EnPrograma(cuerpo), Resultado = ResultadoEsperado.ErrorSemantico,
        Esperados = new List<(int, string)> { (linea, MensajeRangoEsperado(literal)) }
    };

    private static string EnPrograma(string cuerpo) => "INI {\n" + cuerpo + "\n}";

    private static Caso Valido(string nombre, string cuerpo) => new Caso
    {
        Nombre = nombre, Fuente = EnPrograma(cuerpo), Resultado = ResultadoEsperado.Valido
    };

    private static Caso Sintaxis(string nombre, string fuente, int linea, string mensaje) => new Caso
    {
        Nombre = nombre, Fuente = fuente, Resultado = ResultadoEsperado.ErrorSintactico,
        Esperados = new List<(int, string)> { (linea, mensaje) }
    };

    private static Caso Semantica(string nombre, string cuerpo, int linea, string mensaje) => new Caso
    {
        Nombre = nombre, Fuente = EnPrograma(cuerpo), Resultado = ResultadoEsperado.ErrorSemantico,
        Esperados = new List<(int, string)> { (linea, "ERROR DE TIPO: " + mensaje) }
    };

    // Modelo de ámbitos: se comprueba directamente, sin pasar por el verificador.
    private static List<(string Nombre, Action Accion)> CrearCasosAmbito()
    {
        var casos = new List<(string Nombre, Action Accion)>();

        casos.Add(("A01 Una declaración se ve desde su propia línea", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            var simbolo = Sim("vA", "ENT", 2);
            Requerir(ini.IntentarDeclarar(simbolo, out string error) && error == null, "Debe declarar en el ámbito principal.");
            Requerir(ini.BuscarVisible("vA", 2) == simbolo, "Debe ser visible en la línea de declaración.");
        }));

        casos.Add(("A02 El uso anterior a la declaración se rechaza", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            ini.IntentarDeclarar(Sim("vA", "ENT", 2), out _);
            Requerir(ini.BuscarVisible("vA", 1) == null, "No debe ser visible antes de declararse.");
            Requerir(!ini.IntentarUsar("vA", 1, out _, out string error), "Debe rechazar el uso anterior.");
            Requerir(error.Contains("antes de su declaración"), "Mensaje inesperado: " + error);
            Requerir(error.Contains("línea 1") && error.Contains("línea 2"), "Mensaje sin líneas: " + error);
        }));

        casos.Add(("A03 Nombre duplicado en el mismo ámbito", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            ini.IntentarDeclarar(Sim("vA", "ENT", 1), out _);
            Requerir(!ini.IntentarDeclarar(Sim("vA", "ENT", 2), out string error), "Debe rechazar el duplicado.");
            Requerir(error.Contains("ya está declarado en el ámbito 'INI'"), "Mensaje inesperado: " + error);
            Requerir(ini.NumeroDeclaraciones == 1, "El duplicado no debe almacenarse.");
        }));

        casos.Add(("A04 El sombreado de un padre se rechaza", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            ini.IntentarDeclarar(Sim("vA", "ENT", 1), out _);
            Ambito hijo = ini.CrearHijo("bloque", Ambito.ClaseBloque);
            Requerir(!hijo.IntentarDeclarar(Sim("vA", "ENT", 5), out string error), "Debe rechazar el sombreado.");
            Requerir(error.Contains("no se permite el sombreado"), "Mensaje inesperado: " + error);
            Requerir(error.Contains("INI"), "El mensaje debe nombrar el ámbito antecesor: " + error);
        }));

        casos.Add(("A05 Ámbios hermanos pueden reutilizar un nombre", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            Ambito primero = ini.CrearHijo("SI", Ambito.ClaseBloque);
            Ambito segundo = ini.CrearHijo("SI", Ambito.ClaseBloque);
            Requerir(primero.IntentarDeclarar(Sim("vA", "ENT", 2), out _), "El primer bloque debe declarar.");
            Requerir(segundo.IntentarDeclarar(Sim("vA", "DEC", 4), out _), "El segundo bloque debe declarar.");
            Requerir(primero.BuscarVisible("vA", 5).Tipo == "ENT", "Cada hermano conserva su tipo.");
            Requerir(segundo.BuscarVisible("vA", 5).Tipo == "DEC", "El hermano no debe ver la declaración externa.");
        }));

        casos.Add(("A06 Búsqueda a través de varios niveles", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            ini.IntentarDeclarar(Sim("vBase", "ENT", 1), out _);
            Ambito funcion = ini.CrearHijo("fCalcular", Ambito.ClaseFuncion);
            Ambito interno = funcion.CrearHijo("SI", Ambito.ClaseBloque);
            interno.IntentarDeclarar(Sim("vTemporal", "ENT", 3), out _);
            Requerir(interno.BuscarVisible("vTemporal", 3) != null, "Debe ver su propia declaración.");
            Requerir(interno.BuscarVisible("vBase", 3) != null, "Debe ver el ámbito principal a través de dos niveles.");
            Requerir(funcion.BuscarVisible("vTemporal", 4) == null, "El padre no debe ver la declaración del hijo.");
        }));

        casos.Add(("A07 Un bloque no ve lo declarado en otro bloque", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            Ambito primero = ini.CrearHijo("MIENT", Ambito.ClaseBloque);
            Ambito segundo = ini.CrearHijo("SI", Ambito.ClaseBloque);
            primero.IntentarDeclarar(Sim("vContador", "ENT", 2), out _);
            Requerir(segundo.BuscarVisible("vContador", 4) == null, "No debe ver la declaración del hermano.");
            Requerir(!segundo.IntentarUsar("vContador", 4, out _, out string error), "Debe rechazar el uso.");
            Requerir(error.Contains("no ha sido declarada"), "Mensaje inesperado: " + error);
        }));

        casos.Add(("A08 Las funciones se pueden invocar antes de declararse", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            var funcion = new Simbolo { Nombre = "fTotal", Tipo = "ENT", Clase = "FUNCION", LineaDeclaracion = 9, IgnorarLineaDeclaracion = true };
            ini.IntentarDeclarar(funcion, out _);
            Requerir(ini.BuscarVisible("fTotal", 1) == funcion, "Una función debe ser visible antes de su declaración.");
            Requerir(ini.IntentarUsar("fTotal", 1, out _, out _), "El uso de la función debe ser válido.");
        }));

        casos.Add(("A09 Los parámetros pertenecen a su función", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            Ambito funcion = ini.CrearHijo("fPromedio", Ambito.ClaseFuncion);
            var parametro = new Simbolo { Nombre = "vNumero", Tipo = "ENT", Clase = "PARAMETRO", LineaDeclaracion = 2 };
            Requerir(funcion.IntentarDeclarar(parametro, out _), "El parámetro debe declararse.");
            Ambito interno = funcion.CrearHijo("SI", Ambito.ClaseBloque);
            Requerir(interno.BuscarVisible("vNumero", 5) == parametro, "El cuerpo debe ver el parámetro.");
            Requerir(ini.BuscarVisible("vNumero", 5) == null, "El principal no debe ver el parámetro.");
        }));

        casos.Add(("A10 La declaración registra ámbito y clase", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            Ambito hijo = ini.CrearHijo("POR", Ambito.ClaseBloque);
            var simbolo = Sim("vIndice", "ENT", 7);
            Requerir(hijo.IntentarDeclarar(simbolo, out _), "Debe declarar.");
            Requerir(ReferenceEquals(simbolo.Ambito, hijo), "Debe registrar el ámbito.");
            Requerir(simbolo.Clase == "VARIABLE", "La clase por defecto debe ser VARIABLE.");
            Requerir(hijo.Padre == ini && ini.Hijos.Count == 1, "El hijo debe enlazarse con el padre.");
            Requerir(ini.BuscarVisible("vIndice", 8) == null, "La salida del ciclo no debe ver la variable.");
        }));

        casos.Add(("A11 Las declaraciones mantienen su orden de lectura", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            ini.IntentarDeclarar(Sim("vPrimera", "ENT", 1), out _);
            ini.IntentarDeclarar(Sim("vSegunda", "TXT", 2), out _);
            var orden = ini.Declaraciones.Select(s => s.Nombre).ToList();
            Requerir(orden.SequenceEqual(new List<string> { "vPrimera", "vSegunda" }), "Orden inesperado: " + string.Join(", ", orden));
        }));

        casos.Add(("A12 Sin símbolo ni nombre se informa el motivo", () =>
        {
            Ambito ini = Ambito.CrearPrincipal();
            Requerir(!ini.IntentarDeclarar(null, out string error) && error.Contains("No se recibió"), "Mensaje inesperado: " + error);
            Requerir(!ini.IntentarDeclarar(new Simbolo(), out error) && error.Contains("no tiene nombre"), "Mensaje inesperado: " + error);
        }));

        return casos;
    }

    private static Simbolo Sim(string nombre, string tipo, int linea)
    {
        return new Simbolo { Nombre = nombre, Tipo = tipo, LineaDeclaracion = linea };
    }

    private static void Requerir(bool condicion, string mensaje)
    {
        if (!condicion) throw new Exception(mensaje);
    }

    private static void Ejecutar(Caso caso)
    {
        var tokens = PrepararTokens(caso.Fuente);
        var sintactico = new AnalizadorSintacticoLl1(caso.SoloCondicion
            ? TablaSintacticaSharpC.CrearTablaCondicion() : TablaSintacticaSharpC.CrearTablaCompleta());
        var erroresSintacticos = sintactico.Analizar(tokens, caso.Fuente.Split('\n').Length);

        if (caso.Resultado == ResultadoEsperado.ErrorSintactico)
        {
            ComprobarMensajes(caso.Esperados, erroresSintacticos, false);
            return;
        }

        if (erroresSintacticos.Count != 0)
            throw new Exception("Se esperaba sintaxis válida. " + Describir(erroresSintacticos));

        // Tablas independientes en cada caso. Verificar registra las declaraciones del fragmento.
        var verificador = new VerificadorTipos(new Dictionary<string, Simbolo>(), caso.Funciones);
        if (caso.SoloCondicion)
            verificador.VerificarCondicion(tokens, 1);
        else
            verificador.Verificar(tokens);
        ComprobarMensajes(caso.Esperados, verificador.Errores, true);
    }

    private static void ComprobarMensajes(List<(int Linea, string Mensaje)> esperados,
        List<(int Linea, string Mensaje)> obtenidos, bool cantidadExacta)
    {
        foreach (var esperado in esperados)
        {
            if (!obtenidos.Any(e => e.Linea == esperado.Linea &&
                e.Mensaje.Contains(esperado.Mensaje, StringComparison.Ordinal)))
                throw new Exception($"Falta diagnóstico en línea {esperado.Linea}: {esperado.Mensaje} " + Describir(obtenidos));
        }

        if (cantidadExacta && esperados.Count != obtenidos.Count)
            throw new Exception($"Se esperaban {esperados.Count} errores; se obtuvieron {obtenidos.Count}. " + Describir(obtenidos));
    }

    private static string Describir(List<(int Linea, string Mensaje)> errores) =>
        errores.Count == 0 ? "Obtenido: sin errores." : "Obtenido: " +
        string.Join(" | ", errores.Select(e => $"Línea {e.Linea}: {e.Mensaje}"));

    // Fixture de categorías: no sustituye ni prueba el AFD léxico conectado a SQLite.
    // Solo admite el vocabulario utilizado por estos casos y rechaza entradas desconocidas.
    private static readonly Dictionary<string, string> Categorias = new Dictionary<string, string>
    {
        { "INI", "PR1" }, { "FUNC", "PR2" }, { "REGR", "PR3" }, { "IMP", "PR5" },
        { "SI", "PR11" }, { "MIENT", "PR15" },
        { "REPT", "PR16" }, { "HASTA", "PR17" }, { "POR", "PR18" },
        { "VDD", "PR20" }, { "FLS", "PR21" }, { "ENT", "PR23" },
        { "DEC", "PR24" }, { "TXT", "PR25" }, { "BOOL", "PR26" }, { "CAR", "PR27" },
        { "(", "CE1" }, { ")", "CE2" }, { "{", "CE3" }, { "}", "CE4" },
        { ",", "CE7" }, { ";", "CE8" }, { "=", "ASIG" },
        { "+", "OPA+" }, { "-", "OPA-" }, { "*", "OPA*" }, { "/", "OPA/" }, { "^", "OPA^" },
        { "==", "OPR1" }, { "<>", "OPR2" }, { "<", "OPR3" }, { ">", "OPR4" },
        { "<=", "OPR5" }, { ">=", "OPR6" }, { "&", "OPL1" }, { "|", "OPL2" }, { "!", "OPL3" }
    };

    private static List<(string Tipo, string Valor, int Linea)> PrepararTokens(string fuente)
    {
        var tokens = new List<(string, string, int)>();
        var lineas = fuente.Split('\n');
        for (int i = 0; i < lineas.Length; i++)
        {
            foreach (var token in TokenizadorFuenteSharpC.TokenizarLinea(lineas[i], i + 1))
            {
                string valor = token.token;
                if (!Categorias.TryGetValue(valor, out string tipo))
                {
                    if (Regex.IsMatch(valor, @"^v[A-Za-z0-9]+$")) tipo = "IDV";
                    else if (Regex.IsMatch(valor, @"^f[A-Za-z0-9]+$")) tipo = "IDF";
                    else if (Regex.IsMatch(valor, @"^[+-]?[0-9]+(\.[0-9]+)?([Ee][+-]?[0-9]+)?$")) tipo = "CNU";
                    else if (valor.Length >= 2 && valor[0] == '"' && valor[valor.Length - 1] == '"') tipo = "CAD";
                    else if (valor.Length == 3 && valor[0] == '\'' && valor[2] == '\'') tipo = "CAR";
                    else throw new Exception("Token no contemplado por el fixture: " + valor);
                }
                tokens.Add((tipo, valor, token.linea));
            }
        }
        return tokens;
    }
}
