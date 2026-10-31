using System.Collections.Generic;

internal static partial class Program
{
    private static List<Caso> CrearCasosLectura()
    {
        return new List<Caso>
        {
            Valido("L01 LEER inicializa variable declarada",
                "TXT vNombre; LEER(vNombre); IMP(vNombre);"),
            Valido("L02 LEER en bloque con variable exterior",
                "ENT vA; SI (VDD) { LEER(vA); IMP(vA); }"),
            Valido("L03 LEER en ambas ramas garantiza inicialización",
                "ENT vA; SI (VDD) { LEER(vA); } SINO { LEER(vA); } IMP(vA);"),
            Valido("L04 La asignación con LEER vacío sigue admitida",
                "TXT vNombre; vNombre = LEER(); IMP(vNombre);"),
            Valido("L05 Programa de captura con nombres declarados",
                "ENT vEdad = 18; ENT vTotal = 2; TXT vNombre; " +
                "IMP(\"Hola mundo\"); IMP(vEdad); IMP(\"Total: \" + vTotal); " +
                "LEER(vNombre); IMP(vNombre);"),
            Semantica("L06 LEER requiere variable declarada", "LEER(vNombre);", 2,
                "La variable 'vNombre' no ha sido declarada."),
            ErrorAmbito("L07 LEER antes de declarar", "LEER(vNombre); TXT vNombre;",
                "La variable 'vNombre' se usa antes de su declaración en la línea 2."),
            SinInicializar("L08 Leer antes de LEER sigue siendo error",
                "TXT vNombre; IMP(vNombre); LEER(vNombre); IMP(vNombre);", "vNombre"),
            SinInicializar("L09 LEER solo en SI no inicializa fuera de la rama",
                "ENT vA; SI (VDD) { LEER(vA); } IMP(vA);", "vA"),
            Sintaxis("L10 LEER necesita variable", EnPrograma("LEER();"), 2,
                "OMISIÓN: Falta un identificador de variable"),
            Sintaxis("L11 LEER no admite literal como destino", EnPrograma("LEER(5);"), 2,
                "OMISIÓN: Falta un identificador de variable")
        };
    }
}
