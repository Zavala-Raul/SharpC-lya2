using System;
using System.Globalization;

namespace Automatas
{
    // Representación del lenguaje: no son tamaños de objetos de .NET ni direcciones reales.
    internal static class CatalogoTipos
    {
        public const int MinimoEntero = int.MinValue;
        public const int MaximoEntero = int.MaxValue;

        public static int ObtenerTamanoBytes(string tipo)
        {
            switch (tipo)
            {
                case "ENT": return 4;
                case "DEC": return 8;
                case "BOOL": return 1;
                case "CAR": return 2;
                case "TXT": return 8; // Referencia simbólica; excluye el contenido de la cadena.
                case "VAC": return 0; // Ausencia de valor; no autoriza variables VAC.
                default: throw new ArgumentException("Tipo sin tamaño definido: " + tipo, nameof(tipo));
            }
        }

        public static bool EsLiteralDecimal(string lexema)
        {
            return lexema.Contains(".") || lexema.IndexOf('e') >= 0 || lexema.IndexOf('E') >= 0;
        }

        // Se recibe un literal previamente reconocido por el léxico como número.
        public static bool EsLiteralEnteroEnRango(string lexema)
        {
            return int.TryParse(lexema, NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out _);
        }

        public static string MensajeEnteroFueraDeRango(string lexema)
        {
            return $"ERROR DE TIPO: El literal '{lexema}' está fuera del rango permitido para ENT " +
                $"({MinimoEntero.ToString(CultureInfo.InvariantCulture)} a {MaximoEntero.ToString(CultureInfo.InvariantCulture)}; 4 bytes).";
        }
    }
}
