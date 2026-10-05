using System.Collections.Generic;

internal static partial class Program
{
    private static Caso SinInicializar(string nombre, string cuerpo, string variable, int linea = 2)
    {
        return new Caso
        {
            Nombre = nombre, Fuente = EnPrograma(cuerpo), Resultado = ResultadoEsperado.ErrorSemantico,
            Esperados = new List<(int, string)>
            {
                (linea, $"ERROR DE INICIALIZACIÓN: La variable '{variable}' se usa sin inicializar.")
            }
        };
    }

    private static List<Caso> CrearCasosErroresDocumentos()
    {
        var casos = new List<Caso>
        {
            SinInicializar("E01 Uso antes de iniciar", "ENT vA; IMP(vA);", "vA"),
            SinInicializar("E02 Autoasignación lee antes de escribir", "ENT vA; vA = vA + 1;", "vA"),
            SinInicializar("E03 SI sin SINO no garantiza asignación", "ENT vA; SI(VDD) { vA = 1; } IMP(vA);", "vA"),
            Valido("E04 SI y SINO inicializan ambas ramas", "ENT vA; SI(VDD) { vA = 1; } SINO { vA = 2; } IMP(vA);"),
            SinInicializar("E05 MIENT puede no ejecutarse", "ENT vA; MIENT(FLS) { vA = 1; } IMP(vA);", "vA"),
            Valido("E06 REPT garantiza una iteración", "ENT vA; REPT { vA = 1; } HASTA (vA > 0); IMP(vA);"),
            SinInicializar("E07 POR lee su inicializador", "ENT vA; POR (ENT vI = vA; vI < 3; vI = vI + 1) { }", "vA"),
            Valido("E08 POR inicia la variable exterior antes de salir", "ENT vA; POR (vA = 0; vA < 2; vA = vA + 1) { } IMP(vA);"),
            Valido("E09 ENCASO con ambas ramas iniciadas", "ENT vA; ENT vS = 1; ENCASO(vS) { 1: vA = 1; ROMPER; DFCT: vA = 2; ROMPER; } IMP(vA);"),
            SinInicializar("E10 ENCASO con rama sin asignación", "ENT vA; ENT vS = 1; ENCASO(vS) { 1: vA = 1; ROMPER; DFCT: ROMPER; } IMP(vA);", "vA"),
            SinInicializar("E11 El hermano no hereda la inicialización", "SI(VDD) { ENT vA; IMP(vA); } SINO { ENT vA = 1; IMP(vA); }", "vA"),
            Valido("E12 Parámetros disponibles al entrar a la función", "FUNC ENT fUno(ENT vA) { REGR vA; } ENT vB = fUno(1);"),
            SinInicializar("E13 Línea real de un uso multilínea", "ENT vA;\nIMP(\nvA);", "vA", 4),
            Semantica("E14 Llamada sin argumentos requeridos", "FUNC ENT fUno(ENT vA) { REGR vA; } ENT vB = fUno();", 2,
                "La función 'fUno' espera 1 argumento(s), pero recibió 0."),
            Semantica("E15 Demasiados argumentos", "FUNC ENT fUno() { REGR 1; } ENT vB = fUno(1);", 2,
                "La función 'fUno' espera 0 argumento(s), pero recibió 1."),
            Semantica("E16 Tipo incompatible de argumento", "FUNC ENT fUno(ENT vA) { REGR vA; } ENT vB = fUno(\"x\");", 2,
                "El argumento 1 de la función 'fUno' debe ser 'ENT', pero se obtuvo 'TXT'."),
            Valido("E17 Promoción ENT a DEC en parámetro", "FUNC DEC fUno(DEC vA) { REGR vA; } DEC vB = fUno(1);"),
            Semantica("E18 Llamada anidada sin cascadas", "FUNC ENT fUno(ENT vA) { REGR vA; } ENT vB = fUno(fUno(\"x\"));", 2,
                "El argumento 1 de la función 'fUno' debe ser 'ENT', pero se obtuvo 'TXT'."),
            Semantica("E19 Línea del argumento incompatible", "FUNC ENT fUno(ENT vA) { REGR vA; } ENT vB = fUno(\n\"x\");", 3,
                "El argumento 1 de la función 'fUno' debe ser 'ENT', pero se obtuvo 'TXT'."),
            Valido("E20 Función VAC retorna sin valor", "FUNC VAC fUno() { REGR; } fUno();"),
            Semantica("E21 Función VAC no debe devolver expresión", "FUNC VAC fUno() { REGR 1; }", 2,
                "La función 'fUno' de tipo 'VAC' no debe devolver una expresión."),
            Semantica("E22 Función ENT no puede usar REGR vacío", "FUNC ENT fUno() { REGR; }", 2,
                "La función 'fUno' debe devolver un valor de tipo 'ENT'."),
            Semantica("E23 Tipo de retorno incompatible", "FUNC ENT fUno() { REGR \"x\"; }", 2,
                "El retorno de la función 'fUno' debe ser 'ENT', pero se obtuvo 'TXT'."),
            Valido("E24 Promoción ENT a DEC en retorno", "FUNC DEC fUno() { REGR 1; }"),
            Semantica("E25 REGR fuera de función", "REGR 1;", 2,
                "REGR solo puede utilizarse dentro de una función."),
            Semantica("E26 REGR se asocia a la función anidada", "FUNC ENT fUno() { FUNC TXT fDos() { REGR 1; } REGR 1; }", 2,
                "El retorno de la función 'fDos' debe ser 'TXT', pero se obtuvo 'ENT'."),
            Semantica("E27 Referencia inexistente en argumento sin cascada", "FUNC ENT fUno(ENT vA) { REGR vA; } ENT vB = fUno(vNada);", 2,
                "La variable 'vNada' no ha sido declarada."),
            Semantica("E28 Error de argumentos en llamada como instrucción", "FUNC VAC fUno(BOOL vA) { REGR; } fUno(1);", 2,
                "El argumento 1 de la función 'fUno' debe ser 'BOOL', pero se obtuvo 'ENT'.")
        };

        var doble = SinInicializar("E29 Usos distintos informados por separado", "ENT vA; IMP(vA); IMP(vA);", "vA");
        doble.Esperados.Add(doble.Esperados[0]);
        casos.Add(doble);

        var dosTipos = Semantica("E30 Dos argumentos de tipo equivocado",
            "FUNC ENT fUno(ENT vA, TXT vB) { REGR vA; } ENT vC = fUno(\"x\", 1);", 2,
            "El argumento 1 de la función 'fUno' debe ser 'ENT', pero se obtuvo 'TXT'.");
        dosTipos.Esperados.Add((2,
            "ERROR DE TIPO: El argumento 2 de la función 'fUno' debe ser 'TXT', pero se obtuvo 'ENT'."));
        casos.Add(dosTipos);

        var dosReferencias = Semantica("E31 Función y argumento no declarados",
            "fNada(vNada);", 2, "La función 'fNada' no ha sido declarada.");
        dosReferencias.Esperados.Add((2, "ERROR DE TIPO: La variable 'vNada' no ha sido declarada."));
        casos.Add(dosReferencias);

        var tipoYUso = Semantica("E32 Error previo no se convierte en inicialización",
            "ENT vA = vAusente; IMP(vA);", 2, "La variable 'vAusente' no ha sido declarada.");
        casos.Add(tipoYUso);
        casos.Add(SinInicializar("E33 Local de función sin iniciar",
            "FUNC ENT fUno() { ENT vA; REGR vA; }", "vA"));
        casos.Add(SinInicializar("E34 Declaración auto-referencial",
            "ENT vA = vA;", "vA"));
        casos.Add(SinInicializar("E35 Condición de MIENT usa variable sin iniciar",
            "ENT vA; MIENT(vA < 3) { vA = 1; }", "vA"));
        casos.Add(SinInicializar("E36 Condición de SI usa variable sin iniciar",
            "ENT vA; SI(vA < 3) { vA = 1; }", "vA"));
        casos.Add(Valido("E37 Global inicializada visible en función",
            "ENT vA = 1; FUNC ENT fUno() { REGR vA; }"));
        return casos;
    }
}
