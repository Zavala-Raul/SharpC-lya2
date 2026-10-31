using System;
using System.Collections.Generic;
using System.Linq;
using Automatas;

internal static partial class Program
{
    private static List<(string Nombre, Action Accion)> CrearCasosRetornoGarantizado()
    {
        var casos = new List<(string, Action)>();
        void CasoRetorno(string nombre, string cuerpo, string funcionSinRetorno = null)
        {
            casos.Add((nombre, () =>
            {
                string fuente = EnPrograma(cuerpo);
                var tokens = PrepararTokens(fuente);
                var sintaxis = new AnalizadorSintacticoLl1(TablaSintacticaSharpC.CrearTablaCompleta())
                    .Analizar(tokens, fuente.Split('\n').Length);
                Requerir(sintaxis.Count == 0, Describir(sintaxis));
                var v = new VerificadorTipos(null); v.Verificar(tokens);
                if (funcionSinRetorno == null)
                {
                    Requerir(v.Errores.Count == 0, Describir(v.Errores));
                    var r = new GeneradorCodigoIntermedio().Generar(v);
                    ValidarEnlaces(r);
                    Requerir(!r.Instrucciones.Any(q => q.Operacion == "FALTA_RETORNO"), "No emitir validación de retorno en IR.");
                    Requerir(!new FormateadorCodigoIntermedio(r.Instrucciones).ComoTexto().Contains("trap falta_retorno"),
                        "No mostrar validación de retorno en texto.");
                }
                else
                {
                    Requerir(v.Errores.Count == 1 && v.Errores[0].Mensaje.StartsWith("ERROR DE RETORNO:") &&
                        v.Errores[0].Mensaje.Contains("'" + funcionSinRetorno + "'"), Describir(v.Errores));
                    var funcion = v.TablaAmbitos.Simbolos.Single(s => s.Nombre == funcionSinRetorno);
                    Requerir(v.Errores[0].Linea == funcion.LineaDeclaracion, "Diagnóstico en declaración de función.");
                    Requerir(new GeneradorCodigoIntermedio().Generar(v).Instrucciones.Count == 0,
                        "Semántica debe bloquear generación.");
                }
            }));
        }
        CasoRetorno("T01 Función vacía no VAC", "FUNC ENT fA() { }", "fA");
        CasoRetorno("T02 SI sin SINO no garantiza retorno", "FUNC ENT fA(BOOL vB) { SI(vB) { REGR 1; } }", "fA");
        CasoRetorno("T03 Ambas ramas retornan", "FUNC ENT fA(BOOL vB) { SI(vB) { REGR 1; } SINO { REGR 2; } }");
        CasoRetorno("T04 Retorno posterior cubre rama pendiente", "FUNC ENT fA(BOOL vB) { SI(vB) { REGR 1; } REGR 2; }");
        CasoRetorno("T05 Rama SINO sin retorno", "FUNC TXT fA(BOOL vB) { SI(vB) { REGR \"x\"; } SINO { IMP(1); } }", "fA");
        CasoRetorno("T06 MIENT puede ejecutar cero veces", "FUNC ENT fA() { MIENT(VDD) { REGR 1; } }", "fA");
        CasoRetorno("T07 POR puede ejecutar cero veces", "FUNC ENT fA() { POR(ENT vI = 0; vI < 2; vI = vI + 1) { REGR 1; } }", "fA");
        CasoRetorno("T08 REPT con retorno garantizado", "FUNC CAR fA() { REPT { REGR 'A'; } HASTA(VDD); }");
        CasoRetorno("T09 ROMPER de REPT permite salir sin retorno",
            "FUNC ENT fA(BOOL vB) { REPT { SI(vB) { ROMPER; } REGR 1; } HASTA(VDD); }", "fA");
        CasoRetorno("T10 Retorno posterior cubre ROMPER de REPT",
            "FUNC ENT fA(BOOL vB) { REPT { SI(vB) { ROMPER; } REGR 1; } HASTA(VDD); REGR 2; }");
        CasoRetorno("T11 ENCASO con todas las ramas retornando",
            "FUNC ENT fA(ENT vS) { ENCASO(vS) { 1: REGR 1; ROMPER; DFCT: REGR 2; ROMPER; } }");
        CasoRetorno("T12 ENCASO con DFCT sin retorno",
            "FUNC ENT fA(ENT vS) { ENCASO(vS) { 1: REGR 1; ROMPER; DFCT: ROMPER; } }", "fA");
        CasoRetorno("T13 Retorno de función anidada no cuenta para exterior",
            "FUNC ENT fA() { FUNC ENT fB() { REGR 1; } }", "fA");
        CasoRetorno("T14 Función anidada se valida independientemente",
            "FUNC ENT fA() { FUNC BOOL fB() { } REGR 1; }", "fB");
        CasoRetorno("T15 VAC no exige REGR y valores se retornan sin trap",
            "FUNC VAC fA() { IMP(1); } FUNC DEC fB() { REGR 1; }");
        CasoRetorno("T16 REGR inalcanzable después de ROMPER no cubre el camino",
            "FUNC ENT fA() { REPT { ROMPER; REGR 1; } HASTA(VDD); }", "fA");
        CasoRetorno("T17 Rama anidada incompleta",
            "FUNC ENT fA(BOOL vB, BOOL vC) { SI(vB) { SI(vC) { REGR 1; } } SINO { REGR 2; } }", "fA");
        CasoRetorno("T18 Línea de declaración multilínea",
            "FUNC ENT\nfA() {\n IMP(1);\n}", "fA");
        CasoRetorno("T19 ROMPER del caso no sale del REPT",
            "FUNC ENT fA(ENT vS) { REPT { ENCASO(vS) { DFCT: ROMPER; } REGR 1; } HASTA(VDD); }");
        CasoRetorno("T20 ROMPER del ciclo anidado no sale del REPT exterior",
            "FUNC ENT fA() { REPT { MIENT(VDD) { ROMPER; } REGR 1; } HASTA(VDD); }");
        return casos;
    }
}
