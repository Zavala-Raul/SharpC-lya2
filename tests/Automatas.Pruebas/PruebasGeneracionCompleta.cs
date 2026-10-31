using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Automatas;

internal static partial class Program
{
    private static List<(string Nombre, Action Accion)> CrearCasosGeneracionCompleta()
    {
        var casos = new List<(string, Action)>();
        void Ejecutable(string nombre, string cuerpo, params string[] salida)
        {
            casos.Add((nombre, () =>
            {
                var analisis = AnalizarParaGeneracion(cuerpo);
                var resultado = new GeneradorCodigoIntermedio().Generar(analisis);
                ValidarEnlaces(resultado);
                var observada = new MaquinaPrueba(resultado, analisis.TablaAmbitos).Ejecutar();
                Requerir(observada.SequenceEqual(salida), "Salida distinta: " + string.Join(" / ", observada));
                ComprobarEquivalenciaRepresentaciones(resultado, analisis.TablaAmbitos, salida);
            }));
        }
        Ejecutable("G01 Todos los tipos y concatenación",
            "ENT vA = 2; DEC vB = 3.5; TXT vT = \"valor\" + vA; CAR vC = 'X'; BOOL vD = VDD; " +
            "IMP(vT); IMP(vC); IMP(vD); IMP(vA + vB);", "valor2", "X", "True", "5.5");
        Ejecutable("G02 SI SINO y negación con precedencia de SharpC",
            "ENT vA = 2; SI (!vA < 3) { IMP(0); } SINO { IMP(1); } SI (!(vA > 1 & FALS) | FALS) { IMP(2); }",
            "1", "2");
        Ejecutable("G03 MIENT reevalúa condición y ROMPER sale del ciclo",
            "ENT vI = 0; MIENT(vI < 9) { vI = vI + 1; SI(vI == 3) { ROMPER; } IMP(vI); } IMP(vI);",
            "1", "2", "3");
        Ejecutable("G04 REPT ejecuta una vez y termina al ser verdadera HASTA",
            "ENT vI = 0; REPT { vI = vI + 1; } HASTA(vI >= 3); IMP(vI);", "3");
        Ejecutable("G05 POR inicializa una vez e incrementa después del cuerpo",
            "ENT vS = 0; POR(ENT vI = 0; vI < 4; vI = vI + 1) { vS = vS + vI; } IMP(vS);", "6");
        Ejecutable("G06 ROMPER de POR omite incremento",
            "ENT vI = 0; POR(vI = 0; vI < 4; vI = vI + 1) { ROMPER; } IMP(vI);", "0");
        Ejecutable("G07 ENCASO sin caída entre ramas y con DFCT",
            "ENT vS = 2; ENCASO(vS) { 1: IMP(1); ROMPER; 2: IMP(2); ROMPER; DFCT: IMP(3); ROMPER; } " +
            "vS = 8; ENCASO(vS) { 1: IMP(4); ROMPER; DFCT: IMP(5); ROMPER; }", "2", "5");
        Ejecutable("G08 ENCASO de texto y carácter",
            "TXT vT = \"a\"; CAR vC = 'Z'; ENCASO(vT) { \"a\": IMP(1); ROMPER; DFCT: IMP(0); ROMPER; } " +
            "ENCASO(vC) { 'Z': IMP(2); ROMPER; DFCT: IMP(0); ROMPER; }", "1", "2");
        Ejecutable("G09 ROMPER selecciona el contenedor más cercano",
            "ENT vI = 0; MIENT(vI < 2) { vI = vI + 1; ENCASO(vI) { DFCT: ROMPER; } IMP(vI); }", "1", "2");
        Ejecutable("G10 Función adelantada y argumentos anidados",
            "ENT vR = fSuma(fUno(), fSuma(2, 3)); IMP(vR); " +
            "FUNC ENT fUno() { REGR 1; } FUNC ENT fSuma(ENT vA, ENT vB) { REGR vA + vB; }", "6");
        Ejecutable("G11 Recursión conserva temporales y parámetros por activación",
            "FUNC ENT fFact(ENT vN) { SI(vN <= 1) { REGR 1; } REGR vN * fFact(vN - 1); } " +
            "ENT vR = fFact(5); IMP(vR);", "120");
        Ejecutable("G12 Función anidada accede al marco léxico",
            "FUNC ENT fExterior(ENT vBase) { FUNC ENT fInterior(ENT vX) { REGR vBase + vX; } REGR fInterior(3); } " +
            "ENT vR = fExterior(7); IMP(vR);", "10");
        Ejecutable("G13 Llamada captura el operando antes de modificar una global",
            "ENT vA = 1; FUNC ENT fCambiar() { vA = 9; REGR 2; } ENT vR = vA + fCambiar(); IMP(vR); IMP(vA);",
            "3", "9");
        Ejecutable("G14 Argumentos se capturan de izquierda a derecha",
            "ENT vA = 1; FUNC ENT fCambiar() { vA = 9; REGR 2; } " +
            "FUNC ENT fCombinar(ENT vX, ENT vY) { REGR vX * 10 + vY; } " +
            "ENT vR = fCombinar(vA, fCambiar()); IMP(vR);", "12");
        Ejecutable("G15 Función VAC explícita e implícita no ejecuta al declararse",
            "FUNC VAC fUno() { IMP(1); REGR; IMP(9); } FUNC VAC fDos() { IMP(2); } IMP(0); fUno(); fDos();",
            "0", "1", "2");
        Ejecutable("G16 Variables homónimas mantienen identidad",
            "SI(VDD) { ENT vDato = 1; IMP(vDato); } SINO { ENT vDato = 2; IMP(vDato); } " +
            "SI(VDD) { ENT vDato = 3; IMP(vDato); }", "1", "3");
        Ejecutable("G17 Promoción en parámetros y retorno respeta división ENT",
            "FUNC DEC fValor(DEC vP) { REGR 5 / 2; } DEC vR = fValor(1); IMP(vR);", "2");
        Ejecutable("G18 Operadores lógicos evalúan ambos operandos",
            "ENT vA = 0; FUNC BOOL fCambiar() { vA = vA + 1; REGR VDD; } " +
            "SI(FALS & fCambiar()) { IMP(9); } SI(VDD | fCambiar()) { IMP(vA); }", "2");
        Ejecutable("G19 Las condiciones con llamadas se reevalúan",
            "ENT vA = 0; FUNC BOOL fSeguir() { vA = vA + 1; REGR VDD; } " +
            "MIENT(fSeguir()) { SI(vA >= 3) { ROMPER; } } IMP(vA);", "3");
        Ejecutable("G20 Funciones homónimas de bloques hermanos",
            "SI(VDD) { FUNC ENT fValor() { REGR 1; } ENT vR = fValor(); IMP(vR); } " +
            "SI(VDD) { FUNC ENT fValor() { REGR 2; } ENT vR = fValor(); IMP(vR); }", "1", "2");
        Ejecutable("G21 Todas las comparaciones",
            "SI(1 == 1 & 1 <> 2 & 1 < 2 & 2 > 1 & 1 <= 1 & 2 >= 2) { IMP(1); }", "1");
        Ejecutable("G22 Ciclos de cero iteraciones y REPT con ROMPER",
            "MIENT(FALS) { IMP(9); } POR(ENT vI = 0; FALS; vI = vI + 1) { IMP(9); } " +
            "REPT { IMP(1); ROMPER; } HASTA(FALS);", "1");
        casos.Add(("G23 LEER en ambas formas y todos los tipos", () =>
        {
            var r = GenerarCuerpo("ENT vA; DEC vB; TXT vC; CAR vD; BOOL vE; " +
                "LEER(vA); vB = LEER(); LEER(vC); LEER(vD); LEER(vE); IMP(vC);");
            ValidarEnlaces(r);
            Requerir(r.Instrucciones.Count(q => q.Operacion == "LEER") == 5, "Cinco destinos de lectura.");
            Requerir(r.Instrucciones.Where(q => q.Operacion == "LEER").Select(q => q.TipoResultado)
                .SequenceEqual(new[] { "ENT", "DEC", "TXT", "CAR", "BOOL" }), "Tipos de lectura perdidos.");
        }));
        casos.Add(("G24 Todas las instrucciones incorporadas conservan sus argumentos", () =>
        {
            var r = GenerarCuerpo("ENT vA = 1; ENT vB = 2; LIMP(); FONCOL(\"FFFFFF\"); TXTCOL(\"000000\"); " +
                "SLTLN(20); POS(1,2); INTER(ENT,vA,vB); TECL(); IMPREP(\"hola\",3);");
            ValidarEnlaces(r);
            Requerir(r.Instrucciones.Skip(2).Select(q => q.Operacion).SequenceEqual(new[]
                { "LIMP", "FONCOL", "TXTCOL", "SLTLN", "POS", "=", "=", "=", "TECL", "IMPREP" }),
                "Faltan incorporadas.");
            var intercambio = r.Instrucciones.Skip(7).Take(3).ToArray();
            Requerir(intercambio[0].Destino.EsTemporal && intercambio[0].Fuente1.Simbolo.Nombre == "vA" &&
                intercambio[1].Destino.Simbolo.Nombre == "vA" && intercambio[1].Fuente1.Simbolo.Nombre == "vB" &&
                intercambio[2].Destino.Simbolo.Nombre == "vB" &&
                ReferenceEquals(intercambio[2].Fuente1, intercambio[0].Destino), "INTER debe intercambiar sin perder el primer valor.");
        }));
        casos.Add(("G25 ROMPER sin contenedor rechaza todo el resultado", () =>
        {
            var r = GenerarCuerpo("ENT vA = 1; ROMPER;");
            Requerir(r.Instrucciones.Count == 0 && r.Errores.Single().Mensaje.Contains("ROMPER"), "No emitir salto sin destino.");
        }));
        casos.Add(("G26 ROMPER no atraviesa una función anidada en un ciclo", () =>
        {
            var r = GenerarCuerpo("MIENT(FALS) { FUNC VAC fMal() { ROMPER; } }");
            Requerir(r.Instrucciones.Count == 0 && r.Errores.Count == 1, "No saltar al marco exterior.");
        }));
        casos.Add(("G27 Falta de retorno se rechaza antes de generar", () =>
        {
            var analisis = new VerificadorTipos(null);
            analisis.Verificar(PrepararTokens(EnPrograma("FUNC ENT fSinRetorno() { } ENT vA = fSinRetorno();")));
            Requerir(analisis.Errores.Any(e => e.Mensaje.StartsWith("ERROR DE RETORNO:")), "Falta diagnóstico semántico.");
            var r = new GeneradorCodigoIntermedio().Generar(analisis);
            Requerir(r.Instrucciones.Count == 0, "No generar con retorno incompleto.");
        }));
        casos.Add(("G28 NUL conserva el lexema de ACT 3", () =>
        {
            var r = GenerarCuerpo("BOOL vA = NUL;");
            EsperarCuadruplos(r, "vA|NUL|—|=");
            Requerir(r.Instrucciones[0].TipoResultado == "BOOL", "No cambiar reglas actuales de PR22.");
        }));
        casos.Add(("G29 Caso de tipo incompatible no genera salida parcial", () =>
        {
            var r = GenerarCuerpo("ENT vA = 1; ENCASO(vA) { \"x\": ROMPER; DFCT: ROMPER; }");
            Requerir(r.Instrucciones.Count == 0 && r.Errores.Count == 1, "Validar comparación de selector y caso.");
        }));
        casos.Add(("G30 Regeneración reinicia etiquetas y no conserva funciones previas", () =>
        {
            var g = new GeneradorCodigoIntermedio();
            var a = AnalizarParaGeneracion("SI(VDD) { IMP(1); }");
            var r = g.Generar(a);
            g.Generar(AnalizarParaGeneracion("FUNC VAC fA() { }"));
            var nuevo = g.Generar(a);
            Requerir(r.Instrucciones.Select(Fila).SequenceEqual(nuevo.Instrucciones.Select(Fila)), "Salida no determinista.");
        }));
        casos.Add(("G31 Generación de todos los controles válidos del analizador", () =>
        {
            foreach (var c in CrearCasos().Where(c => c.Resultado == ResultadoEsperado.Valido &&
                !c.SoloCondicion && c.Funciones.Count == 0))
            {
                var tokens = PrepararTokens(c.Fuente);
                var v = new VerificadorTipos(null); v.Verificar(tokens);
                var r = new GeneradorCodigoIntermedio().Generar(v);
                Requerir(r.Errores.Count == 0, c.Nombre + ": " + string.Join("; ", r.Errores));
                ValidarEnlaces(r);
            }
        }));
        casos.Add(("G32 VAC usado como valor produce diagnóstico, no operando ausente", () =>
        {
            var r = GenerarCuerpo("FUNC VAC fNada() { } TXT vT = \"x\" + fNada();");
            Requerir(r.Instrucciones.Count == 0 && r.Errores.Single().Mensaje.Contains("VAC"),
                "VAC no debe convertirse en una fuente nula dentro de una operación.");
        }));
        casos.Add(("G33 Vista textual de tres direcciones respeta orden y promociones", () =>
        {
            var r = GenerarCuerpo("ENT vA = 2; DEC vR = vA + 3 * 4;");
            var texto = new FormateadorCodigoIntermedio(r.Instrucciones).ComoTexto();
            Requerir(texto == string.Join(Environment.NewLine,
                "vA = 2", "t1 = 3 * 4", "vR = vA + t1  // resultado ENT -> DEC", ""),
                "Formato de operaciones inesperado: " + texto);
        }));
        casos.Add(("G34 Vista textual de funciones saltos y servicios", () =>
        {
            var r = GenerarCuerpo("FUNC VAC fVer() { IMP(\"hola\"); } SI(VDD) { fVer(); } " +
                "LIMP(); FONCOL(\"FFFFFF\"); TXTCOL(\"000000\"); SLTLN(20); POS(1,2); " +
                "ENT vA = 1; ENT vB = 2; INTER(ENT,vA,vB); TECL(); IMPREP(\"hola\",3);");
            var texto = new FormateadorCodigoIntermedio(r.Instrucciones).ComoTexto();
            foreach (var parte in new[] { "func fVer -> VAC", "ifFalse VDD goto", "call fVer, 0",
                "return", "endfunc fVer", "IMP(\"hola\")", "LIMP()", "FONCOL(\"FFFFFF\")",
                "TXTCOL(\"000000\")", "SLTLN(20)", "POS(1, 2)", "vA = vB",
                "TECL()", "IMPREP(\"hola\", 3)" })
                Requerir(texto.Contains(parte), "Falta representación textual: " + parte);
            Requerir(texto.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length ==
                r.Instrucciones.Count, "Debe haber una línea por cuádruplo.");
        }));
        casos.Add(("G35 Ambas vistas distinguen homónimos y salida vacía", () =>
        {
            var r = GenerarCuerpo("SI(VDD) { ENT vDato = 1; IMP(vDato); } SI(VDD) { ENT vDato = 2; IMP(vDato); }");
            var formato = new FormateadorCodigoIntermedio(r.Instrucciones);
            var destinos = r.Instrucciones.Where(q => q.Operacion == "=").Select(q => q.Destino).ToArray();
            Requerir(formato.Operando(destinos[0]) != formato.Operando(destinos[1]), "Homónimos ambiguos.");
            foreach (var destino in destinos)
                Requerir(formato.ComoTexto().Contains("IMP(" + formato.Operando(destino) + ")"),
                    "Texto y tabla deben usar la misma identidad visible.");
            Requerir(new FormateadorCodigoIntermedio(GenerarCuerpo("ROMPER;").Instrucciones).ComoTexto() == "",
                "No mostrar texto si falló la generación.");
        }));
        casos.Add(("G36 Operadores relacionales conservan lexema con categorías del AFD real", () =>
        {
            var categoriasAfd = new Dictionary<string, string>
            {
                { "<", "OPR2" }, { "<>", "OPR5" }, { ">", "OPR1" },
                { "<=", "OPR4" }, { ">=", "OPR3" }, { "==", "OPR6" }
            };
            foreach (var par in categoriasAfd)
            {
                var tokens = PrepararTokens(EnPrograma("ENT vI = 0; MIENT(vI " + par.Key + " 3) { vI = vI + 1; }"));
                for (int i = 0; i < tokens.Count; i++)
                    if (tokens[i].Valor == par.Key)
                        tokens[i] = (par.Value, tokens[i].Valor, tokens[i].Linea);
                var sintaxis = new AnalizadorSintacticoLl1(TablaSintacticaSharpC.CrearTablaCompleta()).Analizar(tokens, 3);
                Requerir(sintaxis.Count == 0, "Sintaxis con categorías reales: " + par.Key);
                var analisis = new VerificadorTipos(null);
                analisis.Verificar(tokens);
                var resultado = new GeneradorCodigoIntermedio().Generar(analisis);
                ValidarEnlaces(resultado);
                var comparacion = resultado.Instrucciones.Single(q => q.TipoResultado == "BOOL");
                Requerir(comparacion.Operacion == par.Key, "Se cambió el significado de " + par.Key);
                Requerir(new FormateadorCodigoIntermedio(resultado.Instrucciones).ComoTexto()
                    .Contains("vI " + par.Key + " 3"), "El texto debe conservar el operador fuente.");
            }
        }));
        Ejecutable("G37 INTER intercambia valores ENT sin introducir arreglos",
            "ENT vUno = 5; ENT vDos = 7; INTER(ENT,vUno,vDos); IMP(vUno); IMP(vDos);", "7", "5");
        Ejecutable("G38 INTER admite todos los tipos y la misma variable",
            "TXT vA = \"uno\"; TXT vB = \"dos\"; INTER(TXT,vA,vB); IMP(vA); IMP(vB); " +
            "BOOL vSi = VDD; BOOL vNo = FALS; INTER(BOOL,vSi,vNo); IMP(vSi); IMP(vNo); " +
            "CAR vX = 'X'; CAR vY = 'Y'; INTER(CAR,vX,vY); IMP(vX); " +
            "DEC vD = 1.5; DEC vE = 2.5; INTER(DEC,vD,vE); IMP(vD); INTER(DEC,vD,vD); IMP(vD);",
            "dos", "uno", "False", "True", "Y", "2.5", "2.5");
        casos.Add(("G39 INTER exige coincidencia de ambos tipos y el tipo explícito", () =>
        {
            foreach (string cuerpo in new[] { "ENT vA = 1; DEC vB = 2.5; INTER(ENT,vA,vB);",
                "ENT vA = 1; ENT vB = 2; INTER(DEC,vA,vB);" })
            {
                var v = new VerificadorTipos(null); v.Verificar(PrepararTokens(EnPrograma(cuerpo)));
                Requerir(v.Errores.Count == 1 && v.Errores[0].Mensaje.Contains("INTER requiere"), Describir(v.Errores));
                Requerir(new GeneradorCodigoIntermedio().Generar(v).Instrucciones.Count == 0, "No generar intercambio inválido.");
            }
        }));
        casos.Add(("G40 INTER lee ambas variables antes de escribir", () =>
        {
            var v = new VerificadorTipos(null);
            v.Verificar(PrepararTokens(EnPrograma("ENT vA; ENT vB = 2; INTER(ENT,vA,vB);")));
            Requerir(v.Errores.Any(e => e.Mensaje.Contains("sin inicializar")), "No intercambiar valores inexistentes.");
        }));
        casos.Add(("G41 FONCOL y TXTCOL exigen colores hexadecimales", () =>
        {
            foreach (string cuerpo in new[] { "FONCOL(\"azul\");", "TXTCOL(\"\");" })
            {
                var v = new VerificadorTipos(null); v.Verificar(PrepararTokens(EnPrograma(cuerpo)));
                Requerir(v.Errores.Count == 1 && v.Errores[0].Mensaje.Contains("hexadecimal"), Describir(v.Errores));
            }
        }));
        casos.Add(("G42 SLTLN e IMPREP exigen cantidad ENT", () =>
        {
            foreach (string cuerpo in new[] { "SLTLN(1.5);", "IMPREP(\"x\",2.5);" })
            {
                var v = new VerificadorTipos(null); v.Verificar(PrepararTokens(EnPrograma(cuerpo)));
                Requerir(v.Errores.Count == 1 && v.Errores[0].Mensaje.Contains("debe ser ENT"), Describir(v.Errores));
            }
        }));
        return casos;
    }

    private static string Fila(Cuadruplo q) => string.Join("|", q.Destino?.Texto, q.Fuente1?.Texto, q.Fuente2?.Texto, q.Operacion);

    private static void ValidarEnlaces(ResultadoCodigoIntermedio r)
    {
        Requerir(r.Errores.Count == 0, "Generación falló: " + string.Join("; ", r.Errores));
        // Toda operación emitida por los controles válidos debe tener una vista textual.
        new FormateadorCodigoIntermedio(r.Instrucciones).ComoTexto();
        var vistas = ConstructorRepresentaciones.Construir(r);
        Requerir(vistas.Errores.Count == 0, "Todas las instrucciones deben tener triplos y Código P.");
        var etiquetas = r.Instrucciones.Where(q => q.Operacion == "ETIQUETA").ToDictionary(q => q.Destino.Texto);
        var funciones = r.Instrucciones.Where(q => q.Operacion == "FUNCION").ToDictionary(q => q.Destino.Simbolo);
        foreach (var q in r.Instrucciones)
        {
            if (q.Operacion == "SALTO" || q.Operacion == "SALTO_FALSO")
                Requerir(etiquetas.ContainsKey(q.Destino.Texto) &&
                    etiquetas[q.Destino.Texto].FuncionContenedora == q.FuncionContenedora, "Salto sin destino o entre marcos.");
            if (q.Operacion == "LLAMAR") Requerir(funciones.ContainsKey(q.Fuente1.Simbolo), "Función sin cuerpo.");
            Requerir(q.Linea > 0 && q.PosicionToken >= 0, "Falta posición fuente.");
        }
    }

    // Oráculo de pruebas, no intérprete del producto. Ejecuta los cuádruplos para
    // detectar errores de flujo, orden de efectos, argumentos y marcos recursivos.
    private sealed class MaquinaPrueba
    {
        private sealed class Marco
        {
            public Simbolo Funcion;
            public Marco Padre;
            public Dictionary<Simbolo, object> Variables = new Dictionary<Simbolo, object>();
            public Dictionary<OperandoIntermedio, object> Temporales = new Dictionary<OperandoIntermedio, object>();
        }
        private readonly IReadOnlyList<Cuadruplo> codigo;
        private readonly TablaAmbitos tabla;
        private readonly Dictionary<string, int> etiquetas;
        private readonly Dictionary<Simbolo, int> funciones;
        private readonly List<string> salida = new List<string>();
        private int pasos;
        public MaquinaPrueba(ResultadoCodigoIntermedio r, TablaAmbitos tabla)
        {
            codigo = r.Instrucciones; this.tabla = tabla;
            etiquetas = codigo.Select((q, i) => (q, i)).Where(x => x.q.Operacion == "ETIQUETA")
                .ToDictionary(x => x.q.Destino.Texto, x => x.i);
            funciones = codigo.Select((q, i) => (q, i)).Where(x => x.q.Operacion == "FUNCION")
                .ToDictionary(x => x.q.Destino.Simbolo, x => x.i);
        }
        public List<string> Ejecutar() { EjecutarDesde(0, new Marco(), new List<object>()); return salida; }
        private Marco Propietario(Marco marco, Simbolo simbolo)
        {
            while (marco.Funcion != null && tabla.ObtenerFuncion(marco.Funcion).Region != simbolo.Region)
                marco = marco.Padre;
            return marco;
        }
        private object Leer(OperandoIntermedio o, Marco marco)
        {
            if (o.EsTemporal) return marco.Temporales[o];
            if (o.Simbolo != null) return Propietario(marco, o.Simbolo).Variables[o.Simbolo];
            if (o.Tipo == "TXT" || o.Tipo == "CAR") return o.Texto.Substring(1, o.Texto.Length - 2);
            if (o.Tipo == "BOOL") return o.Texto == "VDD";
            return double.Parse(o.Texto, CultureInfo.InvariantCulture);
        }
        private void Guardar(OperandoIntermedio destino, object valor, Marco marco)
        {
            if (destino == null) return;
            if (destino.EsTemporal) marco.Temporales[destino] = valor;
            else Propietario(marco, destino.Simbolo).Variables[destino.Simbolo] = valor;
        }
        private static string Texto(object valor) => Convert.ToString(valor, CultureInfo.InvariantCulture);
        private object EjecutarDesde(int inicio, Marco marco, List<object> parametros)
        {
            var argumentos = new List<object>();
            for (int ip = inicio; ip < codigo.Count; ip++)
            {
                Requerir(++pasos < 20000, "Límite de pasos: posible salto incorrecto.");
                var q = codigo[ip];
                if (q.Operacion == "SALTO") { ip = etiquetas[q.Destino.Texto]; continue; }
                if (q.Operacion == "SALTO_FALSO")
                { if (!(bool)Leer(q.Fuente1, marco)) ip = etiquetas[q.Destino.Texto]; continue; }
                if (q.Operacion == "ETIQUETA" || q.Operacion == "FUNCION") continue;
                if (q.Operacion == "PARAMETRO")
                { Guardar(q.Destino, parametros[int.Parse(q.Fuente1.Texto)], marco); continue; }
                if (q.Operacion == "ARGUMENTO")
                { Requerir(int.Parse(q.Destino.Texto) == argumentos.Count, "Argumentos mezclados."); argumentos.Add(Leer(q.Fuente1, marco)); continue; }
                if (q.Operacion == "LLAMAR")
                {
                    Requerir(argumentos.Count == int.Parse(q.Fuente2.Texto), "Aridad perdida.");
                    int entrada = funciones[q.Fuente1.Simbolo];
                    var padre = marco;
                    var simboloPadre = codigo[entrada].Fuente1?.Simbolo;
                    while (padre.Funcion != simboloPadre) padre = padre.Padre;
                    var valor = EjecutarDesde(entrada + 1,
                        new Marco { Funcion = q.Fuente1.Simbolo, Padre = padre }, new List<object>(argumentos));
                    argumentos.Clear(); Guardar(q.Destino, valor, marco); continue;
                }
                if (q.Operacion == "RETORNO") return q.Fuente1 == null ? null : Leer(q.Fuente1, marco);
                if (q.Operacion == "FIN_FUNCION") throw new Exception("Retorno faltante.");
                if (q.Operacion == "IMP") { salida.Add(Texto(Leer(q.Fuente1, marco))); continue; }
                object a = Leer(q.Fuente1, marco), b = q.Fuente2 == null ? null : Leer(q.Fuente2, marco), valorOp;
                switch (q.Operacion)
                {
                    case "=": valorOp = a; break;
                    case "+": valorOp = q.TipoResultado == "TXT" ? (object)(Texto(a) + Texto(b)) : (double)a + (double)b; break;
                    case "-": valorOp = (double)a - (double)b; break;
                    case "*": valorOp = (double)a * (double)b; break;
                    case "/": valorOp = q.TipoResultado == "ENT" ? Math.Truncate((double)a / (double)b) : (double)a / (double)b; break;
                    case "^": valorOp = Math.Pow((double)a, (double)b); break;
                    case "==": valorOp = Equals(a, b); break;
                    case "<>": valorOp = !Equals(a, b); break;
                    case "<": valorOp = (double)a < (double)b; break;
                    case ">": valorOp = (double)a > (double)b; break;
                    case "<=": valorOp = (double)a <= (double)b; break;
                    case ">=": valorOp = (double)a >= (double)b; break;
                    case "!": valorOp = !(bool)a; break;
                    case "&&": valorOp = (bool)a & (bool)b; break;
                    case "||": valorOp = (bool)a | (bool)b; break;
                    default: throw new Exception("Operación no implementada en oráculo: " + q.Operacion);
                }
                Guardar(q.Destino, valorOp, marco);
            }
            return null;
        }
    }
}
