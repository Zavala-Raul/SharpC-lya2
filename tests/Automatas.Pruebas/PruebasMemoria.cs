using System;
using System.Collections.Generic;
using System.Linq;
using Automatas;

internal static partial class Program
{
    private static void Ubicacion(Simbolo simbolo, string region, int desplazamiento, int tamano)
    {
        Requerir(simbolo != null && simbolo.Region != null, "Falta la posición de " + simbolo?.Nombre);
        Requerir(simbolo.Region.Nombre == region && simbolo.DesplazamientoBytes == desplazamiento &&
            simbolo.TamanoBytes == tamano,
            $"Ubicación incorrecta de {simbolo.Nombre}: {simbolo.Region.Nombre}, " +
            $"{simbolo.DesplazamientoBytes}, {simbolo.TamanoBytes}; se esperaba {region}, {desplazamiento}, {tamano}.");
    }

    private static List<Caso> CrearCasosMemoria()
    {
        var casos = new List<Caso>();

        var tipos = Valido("D01 Tamaños y offsets sin alineación",
            "ENT vA; DEC vB; BOOL vC; CAR vD; TXT vE;");
        tipos.ComprobarEstado = v =>
        {
            Simbolo[] s = v.TablaAmbitos.Simbolos.ToArray();
            Requerir(s.Length == 5 && s.Select(x => x.Numero).SequenceEqual(new[] { 1, 2, 3, 4, 5 }),
                "Se esperan cinco declaraciones numeradas en orden.");
            Ubicacion(s[0], "INI", 0, 4);
            Ubicacion(s[1], "INI", 4, 8);
            Ubicacion(s[2], "INI", 12, 1);
            Ubicacion(s[3], "INI", 13, 2);
            Ubicacion(s[4], "INI", 15, 8);
            Requerir(v.TablaAmbitos.Regiones.Count == 1 && v.TablaAmbitos.Regiones[0].TamanoBytes == 23,
                "El total principal debe ser 23 bytes.");
        };
        casos.Add(tipos);

        var bloques = Valido("D02 Los bloques hermanos comparten región sin reutilizar huecos",
            "SI(VDD) { ENT vA; } SINO { CAR vA; } BOOL vB;");
        bloques.ComprobarEstado = v =>
        {
            Simbolo[] s = v.TablaAmbitos.Simbolos.ToArray();
            Ubicacion(s[0], "INI", 0, 4);
            Ubicacion(s[1], "INI", 4, 2);
            Ubicacion(s[2], "INI", 6, 1);
            Requerir(s[0].Ambito != s[1].Ambito && v.TablaAmbitos.Regiones[0].TamanoBytes == 7,
                "Los hermanos deben compartir región, no ámbito ni offset.");
        };
        casos.Add(bloques);

        var marcos = Valido("D03 Parámetros y locales tienen marco propio",
            "ENT vGlobal; FUNC DEC fSuma(ENT vA, DEC vB) { BOOL vC; SI(VDD) { TXT vD; } REGR vB; } CAR vFinal;");
        marcos.ComprobarEstado = v =>
        {
            var tabla = v.TablaAmbitos;
            Requerir(tabla.Regiones.Count == 2 && tabla.Funciones.Count == 1, "Se espera INI y un marco de función.");
            var funcion = tabla.Funciones.Single();
            var region = funcion.Region;
            Requerir(region != null && region.Nombre == "FUNCION#1:fSuma" && funcion.TamanoMarcoBytes == 21,
                "El tamaño de marco debe ser 4 + 8 + 1 + 8 bytes.");
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vGlobal"), "INI", 0, 4);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vA"), region.Nombre, 0, 4);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vB"), region.Nombre, 4, 8);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vC"), region.Nombre, 12, 1);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vD"), region.Nombre, 13, 8);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vFinal"), "INI", 4, 2);
            Requerir(tabla.Regiones[0].TamanoBytes == 6, "El principal debe ocupar 6 bytes.");
            var nombre = tabla.Simbolos.Single(s => s.Clase == "FUNCION");
            Requerir(nombre.Region == null && nombre.TamanoBytes == null && nombre.DesplazamientoBytes == null,
                "Un identificador de función no ocupa un hueco de variable.");
        };
        casos.Add(marcos);

        var anidadas = Valido("D04 Función anidada abre región independiente",
            "FUNC ENT fUno(ENT vA) { BOOL vB; FUNC ENT fDos(CAR vC) { TXT vD; REGR 1; } REGR vA; } ENT vE;");
        anidadas.ComprobarEstado = v =>
        {
            var tabla = v.TablaAmbitos;
            Requerir(tabla.Regiones.Count == 3, "Deben existir INI y dos marcos.");
            var externa = tabla.Funciones.Single(f => f.Nombre == "fUno");
            var interna = tabla.Funciones.Single(f => f.Nombre == "fDos");
            Requerir(externa.TamanoMarcoBytes == 5 && interna.TamanoMarcoBytes == 10,
                "Los marcos no deben fusionarse.");
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vA"), externa.Region.Nombre, 0, 4);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vB"), externa.Region.Nombre, 4, 1);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vC"), interna.Region.Nombre, 0, 2);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vD"), interna.Region.Nombre, 2, 8);
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vE"), "INI", 0, 4);
        };
        casos.Add(anidadas);

        var ciclo = Valido("D05 POR, REPT y casos reservan en la región contenedora",
            "POR(ENT vI = 0; vI < 1; vI = vI + 1) { CAR vC; } " +
            "REPT { BOOL vB; } HASTA(VDD); " +
            "ENT vS = 1; ENCASO(vS) { 1: TXT vT; ROMPER; DFCT: DEC vD; ROMPER; }");
        ciclo.ComprobarEstado = v =>
        {
            Simbolo[] s = v.TablaAmbitos.Simbolos.ToArray();
            Requerir(s.Length == 6, "Deben existir las seis declaraciones.");
            int[] offsets = { 0, 4, 6, 7, 11, 19 };
            int[] tamanos = { 4, 2, 1, 4, 8, 8 };
            for (int i = 0; i < s.Length; i++) Ubicacion(s[i], "INI", offsets[i], tamanos[i]);
            Requerir(v.TablaAmbitos.Regiones.Single().TamanoBytes == 27, "Tamaño total incorrecto.");
        };
        casos.Add(ciclo);

        var reasignar = Valido("D06 Reasignar no consume espacio",
            "ENT vA = 1; vA = 2; SI(VDD) { vA = 3; } IMP(vA);");
        reasignar.ComprobarEstado = v =>
        {
            var region = v.TablaAmbitos.Regiones.Single();
            Requerir(region.TamanoBytes == 4 && region.Simbolos.Count == 1 &&
                region.Simbolos[0].Valor == "3", "Reasignar no debe cambiar el offset o reservar de nuevo.");
            Ubicacion(region.Simbolos[0], "INI", 0, 4);
        };
        casos.Add(reasignar);

        var invalida = ErrorAmbito("D07 Duplicado no consume espacio", "ENT vA; TXT vA; CAR vB;",
            "El nombre 'vA' ya está declarado en el ámbito 'INI'.");
        invalida.ComprobarEstado = v =>
        {
            Simbolo[] s = v.TablaAmbitos.Simbolos.ToArray();
            Requerir(s.Length == 2, "El símbolo rechazado no debe aparecer en la tabla.");
            Ubicacion(s[0], "INI", 0, 4);
            Ubicacion(s[1], "INI", 4, 2);
            Requerir(v.TablaAmbitos.Regiones.Single().TamanoBytes == 6, "El duplicado no debe dejar hueco.");
        };
        casos.Add(invalida);

        var duplicada = ErrorAmbito("D08 Función duplicada no obtiene marco",
            "FUNC ENT fUno() { ENT vA; } FUNC ENT fUno() { TXT vB; } CAR vC;",
            "El nombre 'fUno' ya está declarado en el ámbito 'INI'.");
        duplicada.ComprobarEstado = v =>
        {
            var tabla = v.TablaAmbitos;
            Requerir(tabla.Regiones.Count == 2 && tabla.Funciones.Single(f => f.Region != null).TamanoMarcoBytes == 4,
                "Solo la función válida debe tener marco.");
            Requerir(tabla.Simbolos.Single(s => s.Nombre == "vB").Region == null,
                "El cuerpo de una función rechazada no debe reservar en INI.");
            Ubicacion(tabla.Simbolos.Single(s => s.Nombre == "vC"), "INI", 0, 2);
        };
        casos.Add(duplicada);

        var vacia = Valido("D09 Región principal vacía", "");
        vacia.ComprobarEstado = v => Requerir(v.TablaAmbitos.Regiones.Count == 1 &&
            v.TablaAmbitos.Regiones[0].TamanoBytes == 0, "La región principal vacía ocupa cero bytes.");
        casos.Add(vacia);

        var reinicio = Valido("D10 Reconstrucción reinicia offsets", "ENT vA;");
        reinicio.ComprobarEstado = v =>
        {
            RegionMemoria anterior = v.TablaAmbitos.Regiones.Single();
            v.Verificar(PrepararTokens(EnPrograma("CAR vB;")));
            RegionMemoria nueva = v.TablaAmbitos.Regiones.Single();
            Requerir(anterior != nueva && anterior.TamanoBytes == 4 && nueva.TamanoBytes == 2,
                "No se debe conservar la región anterior.");
            Ubicacion(v.TablaAmbitos.Simbolos.Single(), "INI", 0, 2);
        };
        casos.Add(reinicio);

        return casos;
    }

    private static List<(string Nombre, Action Accion)> CrearCasosMemoriaDirectos()
    {
        return new List<(string Nombre, Action Accion)>
        {
            ("D11 Desbordamiento comprobado no altera la región", () =>
            {
                var region = new RegionMemoria("prueba");
                var primera = new Simbolo { Nombre = "vA", Clase = "VARIABLE" };
                var segunda = new Simbolo { Nombre = "vB", Clase = "VARIABLE" };
                region.Reservar(primera, int.MaxValue);
                bool desborda = false;
                try { region.Reservar(segunda, 1); }
                catch (OverflowException) { desborda = true; }
                Requerir(desborda && region.TamanoBytes == int.MaxValue && region.Simbolos.Count == 1 &&
                    segunda.Region == null && segunda.DesplazamientoBytes == null,
                    "El desbordamiento no puede asignar offsets ni consumir espacio.");
            }),
            ("D12 Una misma declaración se reserva una vez", () =>
            {
                var region = new RegionMemoria("prueba");
                var simbolo = new Simbolo { Nombre = "vA", Clase = "VARIABLE" };
                region.Reservar(simbolo, 4);
                bool rechazado = false;
                try { region.Reservar(simbolo, 4); }
                catch (InvalidOperationException) { rechazado = true; }
                Requerir(rechazado && region.TamanoBytes == 4 && region.Simbolos.Count == 1,
                    "La doble reserva no debe cambiar el total.");
            }),
            ("D13 Solo variables y parámetros con tamaño positivo", () =>
            {
                var region = new RegionMemoria("prueba");
                bool funcion = false, vacio = false;
                try { region.Reservar(new Simbolo { Nombre = "fUno", Clase = "FUNCION" }, 8); }
                catch (ArgumentException) { funcion = true; }
                try { region.Reservar(new Simbolo { Nombre = "vA", Clase = "VARIABLE" }, 0); }
                catch (ArgumentOutOfRangeException) { vacio = true; }
                Requerir(funcion && vacio && region.TamanoBytes == 0 && region.Simbolos.Count == 0,
                    "No deben reservarse funciones ni símbolos de tamaño cero.");
            }),
            ("D14 VAC es retorno sin bytes y los tipos desconocidos se rechazan", () =>
            {
                bool desconocido = false;
                try { CatalogoTipos.ObtenerTamanoBytes("OTRO"); }
                catch (ArgumentException) { desconocido = true; }
                Requerir(CatalogoTipos.ObtenerTamanoBytes("VAC") == 0 && desconocido,
                    "VAC no ocupa bytes; un tipo desconocido no debe tener tamaño implícito.");
            }),
            ("D15 Un símbolo no puede reservarse en dos regiones", () =>
            {
                var primera = new RegionMemoria("INI");
                var segunda = new RegionMemoria("FUNCION");
                var simbolo = new Simbolo { Nombre = "vA", Clase = "VARIABLE" };
                primera.Reservar(simbolo, 4);
                bool rechazado = false;
                try { segunda.Reservar(simbolo, 4); }
                catch (InvalidOperationException) { rechazado = true; }
                Requerir(rechazado && primera.TamanoBytes == 4 && segunda.TamanoBytes == 0 &&
                    ReferenceEquals(simbolo.Region, primera), "La declaración debe conservar su región original.");
            })
        };
    }
}
