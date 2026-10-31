using System;
using System.Collections.Generic;
using System.Linq;
using Automatas;

internal static partial class Program
{
    private static List<(string Nombre, Action Accion)> CrearCasosRepresentaciones()
    {
        var casos = new List<(string, Action)>();
        ResultadoRepresentaciones Vistas(string cuerpo)
        {
            var r = ConstructorRepresentaciones.Construir(GenerarCuerpo(cuerpo));
            Requerir(r.Errores.Count == 0, string.Join("; ", r.Errores));
            return r;
        }
        void Notacion(string nombre, string cuerpo, string infija, string prefija, string postfija)
        {
            casos.Add((nombre, () =>
            {
                var ultima = Vistas(cuerpo).Notaciones.Last();
                Requerir(ultima.Infija == infija && ultima.Prefija == prefija && ultima.Postfija == postfija,
                    "Notaciones distintas: " + ultima.Infija + " / " + ultima.Prefija + " / " + ultima.Postfija);
            }));
        }
        Notacion("U01 Tres notaciones de expresión completa, no de temporales", "ENT vR = 2 + 3 * 4;",
            "vR = (2 + (3 * 4))", "= vR + 2 * 3 4", "vR 2 3 4 * + =");
        Notacion("U02 Agrupación explícita", "ENT vR = (2 + 3) * 4;",
            "vR = ((2 + 3) * 4)", "= vR * + 2 3 4", "vR 2 3 + 4 * =");
        Notacion("U03 Potencia conserva asociatividad derecha", "ENT vR = 2 ^ 3 ^ 2;",
            "vR = (2 ^ (3 ^ 2))", "= vR ^ 2 ^ 3 2", "vR 2 3 2 ^ ^ =");
        Notacion("U04 Resta conserva asociatividad izquierda", "ENT vR = 10 - 3 - 2;",
            "vR = ((10 - 3) - 2)", "= vR - - 10 3 2", "vR 10 3 - 2 - =");
        Notacion("U05 Negación sobre comparación de SharpC", "SI(!2 < 3) { }",
            "!((2 < 3))", "! < 2 3", "2 3 < !");
        Notacion("U06 Texto con espacios conserva literales", "TXT vT = \"a b\" + \" c\";",
            "vT = (\"a b\" + \" c\")", "= vT + \"a b\" \" c\"", "vT \"a b\" \" c\" + =");
        Notacion("U07 Lectura sin argumentos con destino", "ENT vA; vA = LEER();",
            "vA = LEER()", "= vA call[LEER/0]", "vA call[LEER/0] =");
        Notacion("U08 Llamada anidada declara aridad sin ambigüedad",
            "FUNC ENT fA(ENT vX) { REGR vX; } FUNC ENT fB(ENT vX, ENT vY) { REGR vX + vY; } " +
            "ENT vR = fB(fA(1), 2);",
            "vR = fB(fA(1), 2)", "= vR call[fB/2] call[fA/1] 1 2", "vR 1 call[fA/1] 2 call[fB/2] =");
        casos.Add(("U09 Triplos de acumulador protegen destino igual a operando derecho", () =>
        {
            var analisis = AnalizarParaGeneracion("ENT vA = 2; vA = 3 - vA; IMP(vA);");
            var r = new GeneradorCodigoIntermedio().Generar(analisis);
            var t = ConstructorRepresentaciones.Construir(r).Triplos;
            Requerir(t[1].Operador == "" && t[1].Objeto.EsTemporal && t[1].Fuente.Texto == "3" &&
                t[2].Operador == "-" && t[2].Fuente.Simbolo.Nombre == "vA" && t[3].Objeto.Simbolo.Nombre == "vA",
                "El acumulador debe ser independiente del destino.");
            ComprobarEquivalenciaRepresentaciones(r, analisis.TablaAmbitos, new[] { "1" });
        }));
        casos.Add(("U10 Código P equivalente conserva resta división y promoción", () =>
        {
            var v = AnalizarParaGeneracion("DEC vR = (20 - 3) / 2; IMP(vR);");
            var r = new GeneradorCodigoIntermedio().Generar(v);
            var p = ConstructorRepresentaciones.Construir(r).CodigoP;
            Requerir(p.Take(4).Select(i => i.Operacion).SequenceEqual(new[] { "load", "load", "sub", "store" }),
                "Orden de operaciones de pila incorrecto.");
            Requerir(p.Single(i => i.Operacion == "div").TipoResultado == "ENT", "División ENT no es DEC.");
            Requerir(p.Any(i => i.Operacion == "store" && i.Operando.Tipo == "DEC" && i.TipoResultado == "ENT"),
                "Promoción al destino perdida.");
            ComprobarEquivalenciaRepresentaciones(r, v.TablaAmbitos, new[] { "8" });
        }));
        casos.Add(("U11 Expresiones fuente de condiciones retornos e incorporadas sin duplicar agrupaciones", () =>
        {
            var r = Vistas("FUNC ENT fA(ENT vX) { REGR (vX + 1) * 2; } " +
                "ENT vA = 1; MIENT(vA < 2) { vA = vA + 1; } " +
                "SI(vA >= 2) { IMP(fA(vA)); } FONCOL(\"FFFFFF\");");
            Requerir(r.Notaciones.Count == 7, "No agregar filas por cada paréntesis o argumento interno.");
            Requerir(r.Notaciones.Any(n => n.Origen.Contexto == "REGR" && n.Origen.FuncionContenedora.Nombre == "fA"),
                "Falta contexto de función.");
            Requerir(r.Notaciones.Any(n => n.Origen.Contexto == "Condición MIENT") &&
                r.Notaciones.Any(n => n.Infija == "FONCOL(\"FFFFFF\")"), "Falta condición o incorporada.");
        }));
        casos.Add(("U12 No publicar ninguna representación al fallar generación", () =>
        {
            var r = ConstructorRepresentaciones.Construir(GenerarCuerpo("ENT vA = 1; ROMPER;"));
            Requerir(r.Errores.Count == 1 && r.Triplos.Count == 0 && r.CodigoP.Count == 0 && r.Notaciones.Count == 0,
                "Salida parcial tras un error.");
        }));
        casos.Add(("U13 Error de conversión conserva línea y se descartan todas las vistas", () =>
        {
            var q = new Cuadruplo(null, null, null, "NO_EXISTE", "VAC", 0, 7);
            var r = ConstructorRepresentaciones.Construir(new ResultadoCodigoIntermedio(new List<Cuadruplo> { q },
                new List<(int, string)>()));
            Requerir(r.Errores.Count == 1 && r.Errores[0].Linea == 7 && r.Errores[0].Mensaje.Contains("Triplos") &&
                r.Triplos.Count == 0 && r.CodigoP.Count == 0, "Error de representación debe tener contexto y salida vacía.");
        }));
        casos.Add(("U14 Programa vacío y declaraciones sin inicializar", () =>
        {
            var r = Vistas("ENT vA; TXT vT;");
            Requerir(r.Triplos.Count == 0 && r.CodigoP.Count == 0 && r.Notaciones.Count == 0, "No inventar asignaciones.");
            Requerir(Vistas("").Notaciones.Count == 0, "No inventar expresiones.");
        }));
        casos.Add(("U15 Homónimos consistentes entre notaciones y representaciones", () =>
        {
            var fuente = GenerarCuerpo("SI(VDD) { ENT vA = 1; IMP(vA); } SINO { ENT vA = 2; IMP(vA); }");
            var r = ConstructorRepresentaciones.Construir(fuente);
            var f = new FormateadorCodigoIntermedio(fuente.Instrucciones);
            foreach (var destino in fuente.Instrucciones.Where(q => q.Operacion == "=").Select(q => q.Destino))
            {
                string nombre = f.Operando(destino);
                Requerir(nombre.Contains("#") && r.Notaciones.Any(n => n.Infija.StartsWith(nombre + " =")),
                    "Notación no distingue declaraciones homónimas.");
                Requerir(ConstructorRepresentaciones.TextoP(r.CodigoP, f).Contains("store " + nombre), "Código P ambiguo.");
            }
        }));
        casos.Add(("U16 Llamada con varios argumentos conserva aridad en triplos", () =>
        {
            var r = Vistas("FUNC ENT fA(ENT vX, ENT vY) { REGR vX + vY; } ENT vR = fA(1,2);");
            var preparacion = r.Triplos.Single(t => t.Operador == "PREPARAR_LLAMADA");
            Requerir(preparacion.Objeto.Simbolo.Nombre == "fA" && preparacion.Fuente.Texto == "2", "Aridad perdida.");
            Requerir(r.CodigoP.Any(i => i.Operacion == "call" && i.Auxiliar.Texto == "2"), "Aridad P perdida.");
        }));
        casos.Add(("U17 Reinicio de acumuladores y notaciones en conversiones sucesivas", () =>
        {
            var a = Vistas("ENT vR = 2 * 3;");
            Vistas("ENT vR = 2 + 3 * 4;");
            var b = Vistas("ENT vR = 2 * 3;");
            Requerir(a.Triplos[0].Objeto.Texto == "r1" && b.Triplos[0].Objeto.Texto == "r1" &&
                a.Notaciones[0].Prefija == b.Notaciones[0].Prefija, "Estado de otra generación contaminó la salida.");
        }));
        casos.Add(("U18 Posición y balance de pila para todos los servicios", () =>
        {
            var r = Vistas("ENT vA; LEER(vA); LIMP(); FONCOL(\"FFFFFF\"); TXTCOL(\"000000\"); " +
                "SLTLN(2); POS(4,5); TECL(); IMPREP(\"hola mundo\",3); IMP(vA);");
            int altura = 0;
            foreach (var i in r.CodigoP)
            {
                altura += i.CambioPila;
                Requerir(altura >= 0 && i.Origen.Linea > 0, "Pila o posición incorrecta.");
            }
            Requerir(altura == 0 && r.CodigoP.Any(i => i.Operacion == "read" && i.TipoResultado == "ENT") &&
                r.CodigoP.Any(i => i.Operacion == "imprep" && i.CambioPila == -2), "Efecto de pila incorrecto.");
            Requerir(r.Triplos.Any(t => t.Operador == "POS" && t.Objeto.Texto == "4" && t.Fuente.Texto == "5"),
                "Los dos argumentos del servicio deben conservarse.");
        }));
        return casos;
    }

    private static void ComprobarEquivalenciaRepresentaciones(ResultadoCodigoIntermedio original,
        TablaAmbitos tabla, string[] esperada)
    {
        var vistas = ConstructorRepresentaciones.Construir(original);
        Requerir(vistas.Errores.Count == 0, "Conversión falló: " + string.Join("; ", vistas.Errores));
        foreach (var conversion in new[] { DesdeTriplos(vistas.Triplos), DesdeCodigoP(vistas.CodigoP) })
        {
            var r = new ResultadoCodigoIntermedio(conversion, new List<(int, string)>());
            var salida = new MaquinaPrueba(r, tabla).Ejecutar();
            Requerir(salida.SequenceEqual(esperada), "Representación no equivalente: " + string.Join(" / ", salida));
        }
    }

    private static Cuadruplo Reconstruir(string op, OperandoIntermedio destino, OperandoIntermedio a,
        OperandoIntermedio b, string tipo, Cuadruplo origen) => new Cuadruplo(destino, a, b, op, tipo,
            origen.PosicionToken, origen.Linea, origen.FuncionContenedora);

    // Adaptadores de pruebas independientes de los emisores: usan exclusivamente los
    // campos de las nuevas instrucciones (Origen solo aporta línea, contexto y tipo).
    private static List<Cuadruplo> DesdeTriplos(IReadOnlyList<Triplo> triplos)
    {
        var resultado = new List<Cuadruplo>();
        OperandoIntermedio funcion = null, cantidad = null;
        foreach (var t in triplos)
        {
            string op = t.Operador;
            var d = t.Objeto; var a = t.Fuente; OperandoIntermedio b = null;
            if (op == "") op = "=";
            else if (ConstructorRepresentaciones.Binario(op)) { b = a; a = d; }
            else if (op == "!") a = d;
            else if (op == "PREPARAR_LLAMADA") { funcion = d; cantidad = a; continue; }
            else if (op == "LLAMAR")
            {
                Requerir(funcion?.Simbolo == a.Simbolo && cantidad != null, "Falta aridad de llamada.");
                b = cantidad; funcion = null; cantidad = null;
            }
            else if (op == "POS" || op == "IMPREP") { b = a; a = d; d = null; }
            resultado.Add(Reconstruir(op, d, a, b, t.TipoResultado, t.Origen));
        }
        return resultado;
    }

    private static List<Cuadruplo> DesdeCodigoP(IReadOnlyList<InstruccionP> codigo)
    {
        var resultado = new List<Cuadruplo>();
        var pila = new Stack<OperandoIntermedio>();
        int numero = 0;
        var operadores = new Dictionary<string, string>
        {
            { "add", "+" }, { "sub", "-" }, { "mul", "*" }, { "div", "/" }, { "pow", "^" },
            { "eq", "==" }, { "ne", "<>" }, { "lt", "<" }, { "gt", ">" }, { "le", "<=" },
            { "ge", ">=" }, { "and", "&&" }, { "or", "||" }
        };
        foreach (var i in codigo)
        {
            void Emitir(string op, OperandoIntermedio d, OperandoIntermedio a = null, OperandoIntermedio b = null) =>
                resultado.Add(Reconstruir(op, d, a, b, i.TipoResultado, i.Origen));
            OperandoIntermedio Temporal() => OperandoIntermedio.Temporal("p" + (++numero), i.TipoResultado);
            if (operadores.TryGetValue(i.Operacion, out string operador))
            {
                var b = pila.Pop(); var a = pila.Pop(); var d = Temporal();
                Emitir(operador, d, a, b); pila.Push(d); continue;
            }
            switch (i.Operacion)
            {
                case "load": pila.Push(i.Operando); break;
                case "store": Emitir("=", i.Operando, pila.Pop()); break;
                case "not": var a = pila.Pop(); var d = Temporal(); Emitir("!", d, a); pila.Push(d); break;
                case "label": Emitir("ETIQUETA", i.Operando); break;
                case "jmp": Emitir("SALTO", i.Operando); break;
                case "jfalse": Emitir("SALTO_FALSO", i.Operando, pila.Pop()); break;
                case "func": Emitir("FUNCION", i.Operando, i.Auxiliar); break;
                case "endfunc": Emitir("FIN_FUNCION", i.Operando); break;
                case "param": Emitir("PARAMETRO", i.Operando, i.Auxiliar); break;
                case "arg": Emitir("ARGUMENTO", i.Operando, pila.Pop()); break;
                case "call":
                    var destino = i.TipoResultado == "VAC" ? null : Temporal();
                    Emitir("LLAMAR", destino, i.Operando, i.Auxiliar);
                    if (destino != null) pila.Push(destino); break;
                case "ret": Emitir("RETORNO", null); break;
                case "retv": Emitir("RETORNO", null, pila.Pop()); break;
                case "read": var lectura = Temporal(); Emitir("LEER", lectura); pila.Push(lectura); break;
                case "print": Emitir("IMP", null, pila.Pop()); break;
                case "limp": case "tecl": Emitir(i.Operacion.ToUpperInvariant(), null); break;
                case "foncol": case "txtcol": case "sltln": Emitir(i.Operacion.ToUpperInvariant(), null, pila.Pop()); break;
                case "pos": case "imprep":
                    var segundo = pila.Pop(); var primero = pila.Pop();
                    Emitir(i.Operacion.ToUpperInvariant(), null, primero, segundo); break;
                default: throw new Exception("Código P sin oráculo: " + i.Operacion);
            }
        }
        Requerir(pila.Count == 0, "Pila residual tras interpretar Código P.");
        return resultado;
    }
}
