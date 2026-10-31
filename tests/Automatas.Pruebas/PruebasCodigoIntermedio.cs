using System;
using System.Collections.Generic;
using System.Linq;
using Automatas;

internal static partial class Program
{
    private static List<(string Nombre, Action Accion)> CrearCasosCodigoIntermedio()
    {
        var casos = new List<(string, Action)>();
        casos.Add(("I01 Copias e inicializadores conservan símbolos y no inicializan declaraciones", () =>
        {
            var analisis = AnalizarParaGeneracion("ENT vA = 2; ENT vB; vB = vA; vA = vA;");
            var resultado = new GeneradorCodigoIntermedio().Generar(analisis);
            EsperarCuadruplos(resultado, "vA|2|—|=", "vB|vA|—|=", "vA|vA|—|=");
            Requerir(ReferenceEquals(resultado.Instrucciones[1].Fuente1.Simbolo,
                analisis.TablaAmbitos.Simbolos.Single(s => s.Nombre == "vA")), "Debe conservar la declaración exacta.");
        }));
        casos.Add(("I02 Multiplicación antes de suma sin evaluar constantes", () =>
            EsperarGeneracion("ENT vR = 2 + 3 * 4;", "t1|3|4|*", "vR|2|t1|+")));
        casos.Add(("I03 Los paréntesis cambian el orden", () =>
            EsperarGeneracion("ENT vR = (2 + 3) * 4;", "t1|2|3|+", "vR|t1|4|*")));
        casos.Add(("I04 Potencia asociativa a la derecha", () =>
            EsperarGeneracion("ENT vR = 2 ^ 3 ^ 2;", "t1|3|2|^", "vR|2|t1|^")));
        casos.Add(("I05 Resta y división asociativas a izquierda", () =>
            EsperarGeneracion("ENT vR = 20 / 2 / 2 - 3 - 1;",
                "t1|20|2|/", "t1|t1|2|/", "t1|t1|3|-", "vR|t1|1|-")));
        casos.Add(("I06 Grupos hermanos conservan ambos resultados", () =>
            EsperarGeneracion("ENT vR = (1 + 2) - (3 + 4);",
                "t1|1|2|+", "t2|3|4|+", "vR|t1|t2|-")));
        casos.Add(("I07 Autoasignación solo escribe el destino al final", () =>
            EsperarGeneracion("ENT vA = 1; vA = (vA + 1) * (vA + 2);",
                "vA|1|—|=", "t1|vA|1|+", "t2|vA|2|+", "vA|t1|t2|*")));
        casos.Add(("I08 Ejercicio de clase con temporal acumulador", () =>
            EsperarGeneracion("ENT vX = 1; ENT vY = 2; ENT vZ = 3; ENT vW; vW = vX + vY + 24 - vZ;",
                "vX|1|—|=", "vY|2|—|=", "vZ|3|—|=", "t1|vX|vY|+", "t1|t1|24|+", "vW|t1|vZ|-")));
        casos.Add(("I09 División ENT conserva su tipo antes de asignar a DEC", () =>
        {
            var resultado = GenerarCuerpo("DEC vR = 5 / 2;");
            EsperarCuadruplos(resultado, "vR|5|2|/");
            var q = resultado.Instrucciones[0];
            Requerir(q.TipoResultado == "ENT" && q.Destino.Tipo == "DEC", "No debe convertir división ENT en DEC.");
        }));
        casos.Add(("I10 Promoción mixta y temporales de tipo estable", () =>
        {
            var resultado = GenerarCuerpo("DEC vR = (1 + 2 + 3.5) * 2;");
            EsperarCuadruplos(resultado, "t1|1|2|+", "t2|t1|3.5|+", "vR|t2|2|*");
            Requerir(resultado.Instrucciones[0].Destino.Tipo == "ENT" &&
                resultado.Instrucciones[1].Destino.Tipo == "DEC", "No se cambia el tipo de un temporal reutilizado.");
        }));
        casos.Add(("I11 Línea y posición del operador en expresiones multilínea", () =>
        {
            var analisis = AnalizarParaGeneracion("DEC vR =\n(1 + 2)\n* 3.5;");
            var resultado = new GeneradorCodigoIntermedio().Generar(analisis);
            EsperarCuadruplos(resultado, "t1|1|2|+", "vR|t1|3.5|*");
            Requerir(resultado.Instrucciones[0].Linea == 3 && resultado.Instrucciones[1].Linea == 4,
                "Debe conservar líneas de los operadores.");
            foreach (var q in resultado.Instrucciones)
                Requerir(analisis.TokensVerificados[q.PosicionToken].Valor == q.Operacion,
                    "El operador debe apuntar al token original.");
        }));
        casos.Add(("I12 Literales con signo y notación científica", () =>
            EsperarGeneracion("ENT vA = -3; DEC vB = 1E2; vB = vA - -2;",
                "vA|-3|—|=", "vB|1E2|—|=", "vB|vA|-2|-")));
        casos.Add(("I13 Generador reutilizable reinicia temporales y no muta resultados", () =>
        {
            var generador = new GeneradorCodigoIntermedio();
            var analisis = AnalizarParaGeneracion("ENT vR = 1 + 2 * 3; vR = (4 + 5) * 6;");
            var primero = generador.Generar(analisis);
            EsperarCuadruplos(primero, "t1|2|3|*", "vR|1|t1|+", "t2|4|5|+", "vR|t2|6|*");
            var segundo = generador.Generar(AnalizarParaGeneracion("ENT vR = (2 + 3) * 4;"));
            EsperarCuadruplos(segundo, "t1|2|3|+", "vR|t1|4|*");
            Requerir(primero.Instrucciones.Count == 4 && analisis.TablaAmbitos.Regiones[0].TamanoBytes == 4,
                "No se muta la salida anterior ni se reservan temporales en la región del usuario.");
        }));
        casos.Add(("I14 Programa vacío o declaraciones sin inicializadores", () =>
        {
            EsperarGeneracion("");
            EsperarGeneracion("ENT vA; DEC vB;");
        }));

        var ampliaciones = new[]
        {
            ("I15 IMP", "ENT vA = 1; IMP(vA);", "IMP"),
            ("I16 SI emite salto condicional", "ENT vA = 1; SI (VDD) { vA = 2; }", "SALTO_FALSO"),
            ("I17 MIENT emite salto", "MIENT (FALS) { }", "SALTO"),
            ("I18 REPT comprueba condición final", "REPT { } HASTA (VDD);", "SALTO_FALSO"),
            ("I19 POR emite condición", "POR (ENT vI = 0; vI < 2; vI = vI + 1) { }", "<"),
            ("I20 Funciones y llamadas adelantadas", "ENT vR = fUno(); FUNC ENT fUno() { REGR 1; }", "LLAMAR"),
            ("I21 LEER como expresión", "ENT vA; vA = LEER();", "LEER"),
            ("I22 TXT y concatenación", "TXT vA = \"hola\" + 2;", "+"),
            ("I23 Declaración BOOL", "BOOL vA = VDD;", "="),
            ("I24 CAR", "CAR vA = 'A';", "=")
        };
        foreach (var ejemplo in ampliaciones)
        {
            var copia = ejemplo;
            casos.Add((copia.Item1, () =>
            {
                var resultado = GenerarCuerpo(copia.Item2);
                Requerir(resultado.Errores.Count == 0 && resultado.Instrucciones.Any(q => q.Operacion == copia.Item3),
                    "Debe generar " + copia.Item3 + ": " + string.Join("; ", resultado.Errores));
            }));
        }
        casos.Add(("I25 Errores semánticos bloquean y no conservan salida anterior", () =>
        {
            var generador = new GeneradorCodigoIntermedio();
            var analisis = AnalizarParaGeneracion("ENT vR = 1 + 2;");
            Requerir(generador.Generar(analisis).Instrucciones.Count == 1, "Control válido.");
            var tokens = PrepararTokens(EnPrograma("ENT vA; ENT vR = vA + 1;"));
            analisis.Verificar(tokens);
            Requerir(analisis.Errores.Count != 0, "Debe detectar lectura sin inicializar.");
            var resultado = generador.Generar(analisis);
            Requerir(resultado.Instrucciones.Count == 0 && resultado.Errores.Count == 1,
                "No generar con error previo ni mostrar instrucciones obsoletas.");
        }));
        casos.Add(("I26 Sin análisis previo se bloquea generación", () =>
        {
            var resultado = new GeneradorCodigoIntermedio().Generar(new VerificadorTipos(null));
            Requerir(resultado.Instrucciones.Count == 0 && resultado.Errores.Count == 1, "Requiere análisis previo.");
        }));
        return casos;
    }

    private static VerificadorTipos AnalizarParaGeneracion(string cuerpo)
    {
        string fuente = EnPrograma(cuerpo);
        var tokens = PrepararTokens(fuente);
        var sintaxis = new AnalizadorSintacticoLl1(TablaSintacticaSharpC.CrearTablaCompleta())
            .Analizar(tokens, fuente.Split('\n').Length);
        Requerir(sintaxis.Count == 0, "El caso debe pasar sintaxis: " + Describir(sintaxis));
        var analisis = new VerificadorTipos(null);
        analisis.Verificar(tokens);
        Requerir(analisis.Errores.Count == 0, "El caso debe pasar semántica: " + Describir(analisis.Errores));
        return analisis;
    }

    private static ResultadoCodigoIntermedio GenerarCuerpo(string cuerpo) =>
        new GeneradorCodigoIntermedio().Generar(AnalizarParaGeneracion(cuerpo));

    private static void EsperarGeneracion(string cuerpo, params string[] esperados) =>
        EsperarCuadruplos(GenerarCuerpo(cuerpo), esperados);

    private static void EsperarCuadruplos(ResultadoCodigoIntermedio resultado, params string[] esperados)
    {
        Requerir(resultado.Errores.Count == 0, "No se esperaba error de generación.");
        var filas = resultado.Instrucciones.Select(q => (q.Destino?.Texto ?? "—") + "|" + (q.Fuente1?.Texto ?? "—") + "|" +
            (q.Fuente2?.Texto ?? "—") + "|" + q.Operacion).ToArray();
        Requerir(filas.SequenceEqual(esperados), "Cuádruplos inesperados: " + string.Join("; ", filas));
    }
}
