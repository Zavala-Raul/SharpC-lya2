using System;
using System.Collections.Generic;

namespace Automatas
{
    // Se construye después de aceptar la sintaxis. Las posiciones corresponden
    // a la lista original de tokens, incluidos los identificadores repetidos.
    internal sealed class TablaAmbitos
    {
        private readonly List<(string Tipo, string Valor, int Linea)> tokens;
        private readonly int[] cierres;
        private readonly HashSet<int> declaraciones = new HashSet<int>();
        private readonly Dictionary<Ambito, List<Simbolo>> pendientes = new Dictionary<Ambito, List<Simbolo>>();
        private readonly Dictionary<Simbolo, Funcion> firmas = new Dictionary<Simbolo, Funcion>();
        private readonly Dictionary<Simbolo, Ambito> cuerposFunciones = new Dictionary<Simbolo, Ambito>();
        private readonly Dictionary<Ambito, Funcion> funcionesPorCuerpo = new Dictionary<Ambito, Funcion>();
        private int numeroAmbito;

        public Ambito Principal { get; } = Ambito.CrearPrincipal();
        public Ambito[] AmbitosPorToken { get; }
        public Simbolo[] SimbolosPorToken { get; }
        public List<Simbolo> Simbolos { get; } = new List<Simbolo>();
        public List<Funcion> Funciones { get; } = new List<Funcion>();
        public List<RegionMemoria> Regiones { get; } = new List<RegionMemoria>();
        public List<(int Linea, string Mensaje)> Errores { get; } = new List<(int, string)>();

        public TablaAmbitos(List<(string Tipo, string Valor, int Linea)> tokens,
            IEnumerable<Funcion> funcionesExternas = null)
        {
            this.tokens = tokens;
            AmbitosPorToken = new Ambito[tokens.Count];
            SimbolosPorToken = new Simbolo[tokens.Count];
            cierres = EmparejarDelimitadores();
            // Solo para clientes que proporcionan firmas externas explícitas.
            // Form1 no proporciona las funciones encontradas durante el léxico.
            if (funcionesExternas != null)
                foreach (var f in funcionesExternas)
                    Principal.IntentarDeclarar(new Simbolo
                    {
                        Nombre = f.Nombre, Tipo = f.TipoRetorno, Clase = "FUNCION",
                        IgnorarLineaDeclaracion = true
                    }, out _);

            if (tokens.Count >= 3 && tokens[0].Tipo == "PR1")
            {
                AmbitosPorToken[0] = Principal;
                AmbitosPorToken[1] = Principal;
                AmbitosPorToken[cierres[1]] = Principal;
                Recorrer(2, cierres[1], Principal);
            }
            else Recorrer(0, tokens.Count, Principal);
            RegistrarDeclaraciones(Principal);
            Simbolos.Sort((a, b) => a.PosicionDeclaracion.CompareTo(b.PosicionDeclaracion));
            for (int i = 0; i < Simbolos.Count; i++)
            {
                Simbolos[i].Numero = i + 1;
                if (firmas.TryGetValue(Simbolos[i], out Funcion firma)) Funciones.Add(firma);
            }
            CalcularDisposicion();
            ResolverReferencias();
        }

        private int[] EmparejarDelimitadores()
        {
            var resultado = new int[tokens.Count];
            var pila = new Stack<int>();
            for (int i = 0; i < tokens.Count; i++)
            {
                string tipo = tokens[i].Tipo;
                if (tipo == "CE1" || tipo == "CE3") pila.Push(i);
                else if (tipo == "CE2" || tipo == "CE4")
                {
                    if (pila.Count == 0) throw new ArgumentException("La tabla de ámbitos requiere sintaxis válida.");
                    int apertura = pila.Pop();
                    if (tokens[apertura].Tipo != (tipo == "CE2" ? "CE1" : "CE3"))
                        throw new ArgumentException("Delimitadores incompatibles en la tabla de ámbitos.");
                    resultado[apertura] = i;
                }
            }
            if (pila.Count != 0) throw new ArgumentException("Delimitadores sin cerrar en la tabla de ámbitos.");
            return resultado;
        }

        private Ambito CrearHijo(Ambito padre, string clase)
        {
            return padre.CrearHijo(clase + "#" + (++numeroAmbito), clase);
        }

        private void Recorrer(int inicio, int fin, Ambito ambito, bool parametros = false)
        {
            for (int i = inicio; i < fin; i++)
            {
                AmbitosPorToken[i] = ambito;
                string tipo = tokens[i].Tipo;
                if (tipo == "PR2")
                {
                    int nombre = i + 2, parentesis = i + 3;
                    int llave = cierres[parentesis] + 1, cierre = cierres[llave];
                    Simbolo simbolo = Declarar(nombre, tokens[i + 1].Valor, "FUNCION", ambito);
                    Ambito funcion = CrearHijo(ambito, "FUNCION");
                    var firma = new Funcion
                    {
                        Nombre = tokens[nombre].Valor, TipoRetorno = tokens[i + 1].Valor,
                        LineaInicio = tokens[i].Linea, LineaCuerpoInicio = tokens[llave].Linea,
                        LineaCuerpoFin = tokens[cierre].Linea
                    };
                    AmbitosPorToken[i + 1] = AmbitosPorToken[nombre] = ambito;
                    Recorrer(parentesis, llave, funcion, true);
                    if (pendientes.TryGetValue(funcion, out List<Simbolo> parametrosFuncion))
                        foreach (Simbolo p in parametrosFuncion)
                            firma.Parametros.Add((p.Tipo, p.Nombre));
                    firmas.Add(simbolo, firma);
                    cuerposFunciones.Add(simbolo, funcion);
                    funcionesPorCuerpo.Add(funcion, firma);
                    AmbitosPorToken[llave] = AmbitosPorToken[cierre] = funcion;
                    Recorrer(llave + 1, cierre, funcion);
                    i = cierre;
                }
                else if (tipo == "PR18")
                {
                    int llave = cierres[i + 1] + 1, cierre = cierres[llave];
                    Ambito ciclo = CrearHijo(ambito, "POR");
                    Recorrer(i + 1, llave, ciclo);
                    AmbitosPorToken[llave] = AmbitosPorToken[cierre] = ciclo;
                    Recorrer(llave + 1, cierre, ciclo);
                    i = cierre;
                }
                else if (tipo == "PR16")
                {
                    int llave = i + 1, cierre = cierres[llave];
                    int finCondicion = cierres[cierre + 2]; // } HASTA ( ... ) ;
                    Ambito ciclo = CrearHijo(ambito, "REPT");
                    AmbitosPorToken[llave] = AmbitosPorToken[cierre] = ciclo;
                    Recorrer(llave + 1, cierre, ciclo);
                    Recorrer(cierre + 1, finCondicion + 2, ciclo);
                    i = finCondicion + 1;
                }
                else if (tipo == "PR13")
                {
                    int llave = cierres[i + 1] + 1, cierre = cierres[llave];
                    Recorrer(i + 1, llave, ambito); // El selector usa el ámbito exterior.
                    Ambito seleccion = CrearHijo(ambito, "ENCASO");
                    AmbitosPorToken[llave] = AmbitosPorToken[cierre] = seleccion;
                    int caso = llave + 1;
                    while (caso < cierre)
                    {
                        int finCaso = caso + 2; // valor/DEF y ':'
                        while (finCaso < cierre && tokens[finCaso].Tipo != "PR19")
                        {
                            if (tokens[finCaso].Tipo == "CE3" || tokens[finCaso].Tipo == "CE1")
                                finCaso = cierres[finCaso];
                            finCaso++;
                        }
                        Recorrer(caso, finCaso + 2, CrearHijo(seleccion, "CASO"));
                        caso = finCaso + 2;
                    }
                    i = cierre;
                }
                else if (tipo == "CE3")
                {
                    int cierre = cierres[i];
                    Ambito bloque = CrearHijo(ambito, "BLOQUE");
                    AmbitosPorToken[i] = AmbitosPorToken[cierre] = bloque;
                    Recorrer(i + 1, cierre, bloque);
                    i = cierre;
                }
                else if (EsTipoVariable(tipo) && i + 1 < fin && tokens[i + 1].Tipo == "IDV")
                {
                    Declarar(i + 1, tokens[i].Valor, parametros ? "PARAMETRO" : "VARIABLE", ambito);
                    AmbitosPorToken[++i] = ambito;
                }
            }
        }

        private Simbolo Declarar(int posicion, string tipo, string clase, Ambito ambito)
        {
            declaraciones.Add(posicion);
            var token = tokens[posicion];
            var simbolo = new Simbolo
            {
                Nombre = token.Valor, Tipo = tipo,
                Clase = clase, LineaDeclaracion = token.Linea, PosicionDeclaracion = posicion,
                IgnorarLineaDeclaracion = clase == "FUNCION"
            };
            if (!pendientes.ContainsKey(ambito)) pendientes.Add(ambito, new List<Simbolo>());
            pendientes[ambito].Add(simbolo);
            return simbolo;
        }

        // Registrar padres antes que hijos hace la prohibición de sombreado
        // independiente del orden textual (incluidas funciones adelantadas).
        private void RegistrarDeclaraciones(Ambito ambito)
        {
            if (pendientes.TryGetValue(ambito, out List<Simbolo> lista))
            {
                foreach (Simbolo simbolo in lista)
                {
                    if (!ambito.IntentarDeclarar(simbolo, out string error))
                        Errores.Add((simbolo.LineaDeclaracion, "ERROR DE ÁMBITO: " + error));
                    else
                    {
                        Simbolos.Add(simbolo);
                        SimbolosPorToken[simbolo.PosicionDeclaracion] = simbolo;
                    }
                }
            }
            foreach (Ambito hijo in ambito.Hijos) RegistrarDeclaraciones(hijo);
        }

        private void ResolverReferencias()
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if ((token.Tipo != "IDV" && token.Tipo != "IDF") || declaraciones.Contains(i)) continue;
                Simbolo simbolo = AmbitosPorToken[i].BuscarVisibleEnPosicion(token.Valor, i);
                SimbolosPorToken[i] = simbolo;
                if (simbolo != null) continue;
                Simbolo posterior = AmbitosPorToken[i].BuscarDeclaracion(token.Valor);
                string mensaje = posterior != null
                    ? $"ERROR DE ÁMBITO: La variable '{token.Valor}' se usa antes de su declaración en la línea {posterior.LineaDeclaracion}."
                    : $"ERROR DE TIPO: La {(token.Tipo == "IDF" ? "función" : "variable")} '{token.Valor}' no ha sido declarada.";
                Errores.Add((token.Linea, mensaje));
            }
        }

        public Funcion ObtenerFuncion(Simbolo simbolo)
        {
            return simbolo != null && firmas.TryGetValue(simbolo, out Funcion funcion) &&
                simbolo.Ambito != null ? funcion : null;
        }

        public Funcion ObtenerFuncionContenedora(Ambito ambito)
        {
            for (; ambito != null; ambito = ambito.Padre)
                if (funcionesPorCuerpo.TryGetValue(ambito, out Funcion funcion)) return funcion;
            return null;
        }

        private void CalcularDisposicion()
        {
            var regionesPorAmbito = new Dictionary<Ambito, RegionMemoria>();
            var principal = new RegionMemoria(Principal.Nombre);
            regionesPorAmbito.Add(Principal, principal);
            Regiones.Add(principal);

            // Solo se crean marcos para funciones declaradas correctamente.
            foreach (Simbolo simbolo in Simbolos)
            {
                if (!firmas.TryGetValue(simbolo, out Funcion funcion)) continue;
                if (PerteneceAFuncionSinRegion(simbolo.Ambito, regionesPorAmbito)) continue;
                Ambito cuerpo = cuerposFunciones[simbolo];
                var region = new RegionMemoria(cuerpo.Nombre + ":" + funcion.Nombre);
                regionesPorAmbito.Add(cuerpo, region);
                Regiones.Add(region);
                funcion.Region = region;
            }

            // Todos los bloques interiores comparten la región de su función más
            // cercana. Si no la hay, pertenecen a INI. No se reutilizan huecos.
            foreach (Simbolo simbolo in Simbolos)
            {
                if (simbolo.Clase != "VARIABLE" && simbolo.Clase != "PARAMETRO") continue;
                if (PerteneceAFuncionSinRegion(simbolo.Ambito, regionesPorAmbito)) continue;
                RegionMemoria region = null;
                for (Ambito ambito = simbolo.Ambito; ambito != null; ambito = ambito.Padre)
                    if (regionesPorAmbito.TryGetValue(ambito, out region)) break;
                // Un cuerpo de función duplicada no se incorpora al marco de su padre.
                // Su declaración no puede ejecutarse ni tiene ubicación válida.
                if (region == null) continue;

                int tamano;
                try
                {
                    tamano = CatalogoTipos.ObtenerTamanoBytes(simbolo.Tipo);
                    if (tamano == 0) throw new ArgumentException("VAC no es un tipo de variable o parámetro.");
                    region.Reservar(simbolo, tamano);
                }
                catch (ArgumentException)
                {
                    Errores.Add((simbolo.LineaDeclaracion,
                        $"ERROR DE MEMORIA: El tipo '{simbolo.Tipo}' de '{simbolo.Nombre}' no tiene tamaño de variable válido."));
                }
                catch (OverflowException)
                {
                    Errores.Add((simbolo.LineaDeclaracion,
                        $"ERROR DE MEMORIA: La región '{region.Nombre}' excede {int.MaxValue} bytes."));
                }
            }
        }

        private static bool PerteneceAFuncionSinRegion(Ambito ambito,
            Dictionary<Ambito, RegionMemoria> regionesPorAmbito)
        {
            for (; ambito != null; ambito = ambito.Padre)
                if (ambito.Clase == Ambito.ClaseFuncion && !regionesPorAmbito.ContainsKey(ambito))
                    return true;
            return false;
        }

        private static bool EsTipoVariable(string tipo)
        {
            return tipo == "PR23" || tipo == "PR24" || tipo == "PR25" || tipo == "PR26" || tipo == "PR27";
        }
    }
}
