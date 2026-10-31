using System;
using System.Collections.Generic;

namespace Automatas
{
    // Direcciones relativas. No guarda memoria de verdad
    public sealed class RegionMemoria
    {
        private readonly List<Simbolo> simbolos = new List<Simbolo>();

        public RegionMemoria(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) throw new ArgumentException("La región necesita un nombre.", nameof(nombre));
            Nombre = nombre;
        }

        public string Nombre { get; }
        public int TamanoBytes { get; private set; }
        public IReadOnlyList<Simbolo> Simbolos => simbolos.AsReadOnly();

        public void Reservar(Simbolo simbolo, int tamanoBytes)
        {
            if (simbolo == null) throw new ArgumentNullException(nameof(simbolo));
            if (simbolo.Clase != "VARIABLE" && simbolo.Clase != "PARAMETRO")
                throw new ArgumentException("Solo variables y parámetros ocupan espacio.", nameof(simbolo));
            if (tamanoBytes <= 0) throw new ArgumentOutOfRangeException(nameof(tamanoBytes));
            if (simbolo.Region != null || simbolos.Contains(simbolo))
                throw new InvalidOperationException("La declaración ya tiene una posición de memoria.");

            int siguiente = checked(TamanoBytes + tamanoBytes);
            simbolo.Region = this;
            simbolo.DesplazamientoBytes = TamanoBytes;
            simbolo.TamanoBytes = tamanoBytes;
            simbolos.Add(simbolo);
            TamanoBytes = siguiente;
        }
    }
}
