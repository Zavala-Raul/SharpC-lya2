using System;
using System.Collections.Generic;

namespace Automatas
{
    // Cadena de ámbitos: el principal corresponde a INI y cada bloque o función
    // crea un hijo. La búsqueda sube hacia el principal. No existe sombreado, por
    // lo que un nombre visible en un padre no puede reutilizarse en un hijo.
    public class Ambito
    {
        public const string ClasePrincipal = "INI";
        public const string ClaseFuncion = "FUNCION";
        public const string ClaseBloque = "BLOQUE";

        private readonly Dictionary<string, Simbolo> declaraciones =
            new Dictionary<string, Simbolo>(StringComparer.Ordinal);
        private readonly List<Simbolo> ordenDeclaraciones = new List<Simbolo>();

        public Ambito(string nombre, string clase, Ambito padre)
        {
            Nombre = nombre;
            Clase = clase;
            Padre = padre;
            if (padre != null) padre.Hijos.Add(this);
        }

        public static Ambito CrearPrincipal()
        {
            return new Ambito(ClasePrincipal, ClasePrincipal, null);
        }

        public string Nombre { get; }
        public string Clase { get; }
        public Ambito Padre { get; }
        public List<Ambito> Hijos { get; } = new List<Ambito>();
        public IEnumerable<Simbolo> Declaraciones => ordenDeclaraciones.AsReadOnly();
        public int NumeroDeclaraciones => declaraciones.Count;

        public Ambito CrearHijo(string nombre, string clase)
        {
            return new Ambito(nombre, clase, this);
        }

        // Los mensajes no llevan el prefijo de la etapa que los informa.
        public bool IntentarDeclarar(Simbolo simbolo, out string error)
        {
            error = null;
            if (simbolo == null)
            {
                error = "No se recibió un símbolo para declarar.";
                return false;
            }
            if (string.IsNullOrEmpty(simbolo.Nombre))
            {
                error = "El símbolo no tiene nombre.";
                return false;
            }
            if (declaraciones.ContainsKey(simbolo.Nombre))
            {
                error = $"El nombre '{simbolo.Nombre}' ya está declarado en el ámbito '{Nombre}'.";
                return false;
            }

            Simbolo antecesor = BuscarEnPadres(simbolo.Nombre);
            if (antecesor != null)
            {
                string origen = antecesor.Ambito != null ? antecesor.Ambito.Nombre : "desconocido";
                error = $"El nombre '{simbolo.Nombre}' ya está declarado en el ámbito '{origen}' " +
                    $"y no se permite el sombreado.";
                return false;
            }

            if (string.IsNullOrEmpty(simbolo.Clase)) simbolo.Clase = "VARIABLE";
            simbolo.Ambito = this;
            declaraciones.Add(simbolo.Nombre, simbolo);
            ordenDeclaraciones.Add(simbolo);
            return true;
        }

        public bool IntentarUsar(string nombre, int linea, out Simbolo simbolo, out string error)
        {
            (Simbolo declaracion, bool visible) = BuscarEnCadena(nombre, linea);
            simbolo = declaracion;
            if (simbolo == null)
            {
                error = $"La variable '{nombre}' no ha sido declarada.";
                return false;
            }
            if (!visible)
            {
                error = $"La variable '{nombre}' se usa en la línea {linea} " +
                    $"antes de su declaración en la línea {simbolo.LineaDeclaracion}.";
                return false;
            }
            error = null;
            return true;
        }

        public Simbolo BuscarVisible(string nombre, int linea)
        {
            (Simbolo declaracion, bool visible) = BuscarEnCadena(nombre, linea);
            return visible ? declaracion : null;
        }

        // La tabla completa usa posiciones para distinguir usos y declaraciones
        // incluso cuando están en la misma línea. La API por línea se conserva
        // para los clientes del modelo que no disponen de tokens.
        public Simbolo BuscarDeclaracion(string nombre)
        {
            for (Ambito actual = this; actual != null; actual = actual.Padre)
                if (actual.declaraciones.TryGetValue(nombre, out Simbolo simbolo)) return simbolo;
            return null;
        }

        public Simbolo BuscarVisibleEnPosicion(string nombre, int posicion)
        {
            Simbolo simbolo = BuscarDeclaracion(nombre);
            return simbolo != null && (simbolo.IgnorarLineaDeclaracion ||
                simbolo.PosicionDeclaracion <= posicion) ? simbolo : null;
        }

        public Simbolo BuscarEnPadres(string nombre)
        {
            for (Ambito actual = Padre; actual != null; actual = actual.Padre)
            {
                if (actual.declaraciones.TryGetValue(nombre, out Simbolo simbolo)) return simbolo;
            }
            return null;
        }

        // La declaración más cercana gana aunque todavía no sea visible: usarla
        // antes de su declaración es un error, no una referencia al ámbito padre.
        private (Simbolo Declaracion, bool Visible) BuscarEnCadena(string nombre, int linea)
        {
            for (Ambito actual = this; actual != null; actual = actual.Padre)
            {
                if (!actual.declaraciones.TryGetValue(nombre, out Simbolo simbolo)) continue;
                bool visible = simbolo.IgnorarLineaDeclaracion || linea >= simbolo.LineaDeclaracion;
                return (simbolo, visible);
            }
            return (null, false);
        }
    }
}
