using System;
using System.Collections.Generic;
using System.Linq;
using Automatas;

internal static partial class Program
{
    private static Caso ErrorAmbito(string nombre, string cuerpo, string mensaje, int linea = 2)
    {
        return new Caso
        {
            Nombre = nombre, Fuente = EnPrograma(cuerpo), Resultado = ResultadoEsperado.ErrorSemantico,
            Esperados = new List<(int, string)> { (linea, "ERROR DE ÁMBITO: " + mensaje) }
        };
    }

    private static List<Caso> CrearCasosIntegracionAmbitos()
    {
        var casos = new List<Caso>
        {
            ErrorAmbito("B01 Uso anterior en la misma línea", "vA = 1; ENT vA;",
                "La variable 'vA' se usa antes de su declaración en la línea 2."),
            ErrorAmbito("B02 Duplicado local", "ENT vA; TXT vA;",
                "El nombre 'vA' ya está declarado en el ámbito 'INI'."),
            ErrorAmbito("B03 Sombreado del principal", "ENT vA; SI (VDD) { TXT vA; }",
                "El nombre 'vA' ya está declarado en el ámbito 'INI' y no se permite el sombreado."),
            Valido("B04 Hermanos con tipos y asignaciones independientes",
                "SI (VDD) { ENT vA = 1; vA = 2; } SINO { TXT vA = \"x\"; vA = \"y\"; }"),
            Semantica("B05 No se heredan declaraciones del hermano", "SI (VDD) { ENT vA; } SI (VDD) { vA = 1; }", 2,
                "La variable 'vA' no ha sido declarada."),
            Valido("B06 Un hijo puede reasignar la variable del padre", "ENT vA = 1; SI (VDD) { vA = 2; }"),
            Valido("B07 Parámetros homónimos de funciones hermanas",
                "FUNC ENT fUno(ENT vA) { REGR vA + 1; } FUNC TXT fDos(TXT vA) { REGR vA; }"),
            Semantica("B08 Parámetro fuera de función", "FUNC ENT fUno(ENT vA) { REGR vA; } IMP(vA);", 2,
                "La variable 'vA' no ha sido declarada."),
            ErrorAmbito("B09 Parámetro duplicado", "FUNC ENT fUno(ENT vA, TXT vA) { REGR 1; }",
                "El nombre 'vA' ya está declarado en el ámbito 'FUNCION#1'."),
            Valido("B10 Llamada adelantada y recursión", "ENT vA = fUno(); FUNC ENT fUno() { REGR fUno(); }"),
            Semantica("B11 Llamar no declara una función", "fNada();", 2,
                "La función 'fNada' no ha sido declarada."),
            Semantica("B12 Función anidada no sale al principal",
                "FUNC ENT fUno() { FUNC ENT fDos() { REGR 1; } REGR fDos(); } fDos();", 2,
                "La función 'fDos' no ha sido declarada."),
            Valido("B13 Función anidada adelantada en su ámbito",
                "FUNC ENT fUno() { ENT vA = fDos(); FUNC ENT fDos() { REGR 1; } REGR vA; }"),
            Semantica("B14 Variable de POR fuera del ciclo",
                "POR (ENT vI = 0; vI < 3; vI = vI + 1) { IMP(vI); } IMP(vI);", 2,
                "La variable 'vI' no ha sido declarada."),
            ErrorAmbito("B15 Encabezado y cuerpo de POR comparten ámbito",
                "POR (ENT vI = 0; vI < 3; vI = vI + 1) { ENT vI; }",
                "El nombre 'vI' ya está declarado en el ámbito 'POR#1'."),
            Valido("B16 POR con variable exterior", "ENT vI; POR (vI = 0; vI < 3; vI = vI + 1) { } IMP(vI);"),
            Valido("B17 REPT comparte ámbito con HASTA", "REPT { ENT vI = 1; } HASTA (vI > 0);"),
            Semantica("B18 Variable de REPT no sale del ciclo", "REPT { ENT vI = 1; } HASTA (vI > 0); IMP(vI);", 2,
                "La variable 'vI' no ha sido declarada."),
            Valido("B19 Casos de ENCASO son hermanos",
                "ENT vS = 1; ENCASO (vS) { 1: ENT vA = 1; ROMPER; DFCT: TXT vA = \"x\"; ROMPER; }"),
            Semantica("B20 Un caso no ve al anterior",
                "ENT vS = 1; ENCASO (vS) { 1: ENT vA = 1; ROMPER; DFCT: IMP(vA); ROMPER; }", 2,
                "La variable 'vA' no ha sido declarada."),
            Semantica("B21 Selector de ENCASO también se resuelve", "ENCASO (vNada) { DFCT: ROMPER; }", 2,
                "La variable 'vNada' no ha sido declarada."),
            Semantica("B22 Referencia desconocida en IMP", "IMP(vNada);", 2,
                "La variable 'vNada' no ha sido declarada."),
            Semantica("B23 Referencia desconocida en retorno", "FUNC ENT fUno() { REGR vNada; }", 2,
                "La variable 'vNada' no ha sido declarada."),
            Semantica("B24 Argumento anidado sin errores derivados",
                "FUNC TXT fUno(TXT vA) { REGR vA; } ENT vB = fUno(fUno(vNada));", 2,
                "La variable 'vNada' no ha sido declarada."),
            ErrorAmbito("B25 Duplicado inválido no cambia el tipo original", "ENT vA = 1; TXT vA = \"x\"; vA = 2;",
                "El nombre 'vA' ya está declarado en el ámbito 'INI'."),
            Valido("B26 Declaraciones y parámetros multilínea",
                "FUNC\nENT\nfUno(\nENT\nvA\n)\n{\nENT\nvB = vA;\nREGR vB;\n}\nENT\nvC = fUno(1);"),
            Valido("B27 Varios bloques anidados en una línea", "ENT vA = 1; SI (VDD) { MIENT (vA < 3) { vA = vA + 1; } }"),
            Valido("B28 Delimitadores dentro de cadenas", "TXT vA = \"{ } ( )\"; FUNC TXT fUno() { REGR \"}\"; }"),
            ErrorAmbito("B29 Sombreado no depende del orden de recorrido", "SI (VDD) { ENT vA; } ENT vA;",
                "El nombre 'vA' ya está declarado en el ámbito 'INI' y no se permite el sombreado."),
            Valido("B30 ENCASO anidado y ROMPER interno",
                "ENT vS = 1; ENCASO(vS) { 1: SI(VDD) { ROMPER; } ENCASO(vS) { DFCT: ROMPER; } ROMPER; DFCT: ROMPER; }"),
            Semantica("B31 Argumentos comprueban sus propias operaciones",
                "FUNC ENT fUno(ENT vA) { REGR vA; } ENT vB = fUno(\"x\" * 2);", 2,
                "El operador aritmético '*' no es válido entre 'TXT' y 'ENT'."),
            ErrorAmbito("B32 Funciones duplicadas",
                "FUNC ENT fUno() { REGR 1; } FUNC TXT fUno() { REGR \"x\"; }",
                "El nombre 'fUno' ya está declarado en el ámbito 'INI'."),
            ErrorAmbito("B33 Función anidada sombrea función posterior",
                "FUNC ENT fUno() { FUNC ENT fDos() { REGR 1; } REGR 1; } FUNC ENT fDos() { REGR 2; }",
                "El nombre 'fDos' ya está declarado en el ámbito 'INI' y no se permite el sombreado."),
            ErrorAmbito("B34 Uso anterior en inicializador",
                "ENT vA = vB + 1; ENT vB = 2;",
                "La variable 'vB' se usa antes de su declaración en la línea 2."),
            ErrorAmbito("B35 Variable principal posterior al cuerpo de función",
                "FUNC ENT fUno() { REGR vA; } ENT vA = 1;",
                "La variable 'vA' se usa antes de su declaración en la línea 2."),
            Semantica("B36 Error de tipo en el segundo hermano",
                "SI(VDD) { TXT vA = \"x\"; } SI(VDD) { ENT vA = 1; vA = \"y\"; }", 2,
                "No se puede asignar una expresión de tipo 'TXT' a la variable 'vA' de tipo 'ENT'.")
        };

        casos[3].ComprobarEstado = v =>
        {
            var simbolos = v.TablaAmbitos.Simbolos.Where(s => s.Nombre == "vA").ToList();
            Requerir(simbolos.Count == 2 && simbolos[0].Numero != simbolos[1].Numero, "Se requieren IDs distintos por declaración.");
            Requerir(simbolos[0].Ambito != simbolos[1].Ambito, "Los símbolos deben pertenecer a ámbitos distintos.");
            Requerir(simbolos[0].Valor == "2" && simbolos[1].Valor == "\"y\"", "La reasignación debe modificar solo su símbolo.");
        };
        casos[5].ComprobarEstado = v => Requerir(v.TablaAmbitos.Simbolos.Single().Valor == "2", "La asignación debe actualizar el símbolo del padre.");
        casos[10].ComprobarEstado = v => Requerir(v.TablaAmbitos.Funciones.Count == 0, "Una llamada no debe registrar una función.");
        casos[24].ComprobarEstado = v => Requerir(v.TablaAmbitos.Simbolos.Single().Valor == "2", "Una declaración rechazada no debe reemplazar la original.");
        casos[25].ComprobarEstado = v =>
        {
            Funcion funcion = v.TablaAmbitos.Funciones.Single();
            Requerir(funcion.LineaInicio == 2 && funcion.LineaCuerpoInicio == 8 && funcion.LineaCuerpoFin == 12,
                "Los límites deben venir de las llaves, incluso con encabezados multilínea.");
            Requerir(funcion.Parametros.Count == 1 && funcion.Parametros[0] == ("ENT", "vA"), "Firma incorrecta.");
        };

        var repetido = Semantica("B37 Dos usos inválidos en la misma línea", "IMP(vNada); IMP(vNada);", 2,
            "La variable 'vNada' no ha sido declarada.");
        repetido.Esperados.Add(repetido.Esperados[0]);
        casos.Add(repetido);

        var reinicio = Valido("B38 Reanalizar reconstruye los ámbitos", "ENT vA = 1;");
        reinicio.ComprobarEstado = v =>
        {
            v.Verificar(PrepararTokens(EnPrograma("IMP(vA);")));
            Requerir(v.Errores.Count == 1 && v.TablaAmbitos.Simbolos.Count == 0, "Se filtraron declaraciones del análisis anterior.");
            v.Verificar(PrepararTokens(EnPrograma("ENT vB = 1;")));
            Requerir(v.Errores.Count == 0 && v.TablaAmbitos.Simbolos.Single().Numero == 1, "Se deben reiniciar errores e IDs.");
        };
        casos.Add(reinicio);
        return casos;
    }
}
