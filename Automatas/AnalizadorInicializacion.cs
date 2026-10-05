using System;
using System.Collections.Generic;

namespace Automatas
{
    // Análisis conservador de inicialización definida: las ramas convergen por
    // intersección. No elimina código inalcanzable tras REGR ni evalúa constantes.
    internal sealed class AnalizadorInicializacion
    {
        private readonly List<(string Tipo, string Valor, int Linea)> tokens;
        private readonly TablaAmbitos tabla;
        private readonly int[] cierres;
        public List<(int Linea, string Mensaje)> Errores { get; } = new List<(int, string)>();

        public AnalizadorInicializacion(List<(string Tipo, string Valor, int Linea)> tokens, TablaAmbitos tabla)
        {
            this.tokens = tokens;
            this.tabla = tabla;
            cierres = new int[tokens.Count];
            var pila = new Stack<int>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Tipo == "CE1" || tokens[i].Tipo == "CE3") pila.Push(i);
                else if (tokens[i].Tipo == "CE2" || tokens[i].Tipo == "CE4")
                    cierres[pila.Pop()] = i;
            }
            int inicio = tokens.Count >= 3 && tokens[0].Tipo == "PR1" ? 2 : 0;
            int fin = inicio == 2 ? cierres[1] : tokens.Count;
            Recorrer(inicio, fin, new HashSet<Simbolo>());
        }

        private void Leer(int inicio, int fin, HashSet<Simbolo> estado)
        {
            for (int i = inicio; i < fin; i++)
            {
                if (tokens[i].Tipo != "IDV") continue;
                Simbolo simbolo = tabla.SimbolosPorToken[i];
                // El analizador de ámbitos ya informó las referencias no resueltas.
                if (simbolo == null || simbolo.Clase == "FUNCION" || estado.Contains(simbolo)) continue;
                Errores.Add((tokens[i].Linea,
                    $"ERROR DE INICIALIZACIÓN: La variable '{simbolo.Nombre}' se usa sin inicializar."));
            }
        }

        private int HastaPuntoComa(int inicio, int fin)
        {
            int i = inicio;
            while (i < fin && tokens[i].Tipo != "CE8") i++;
            return i;
        }

        private static void UnirRamas(HashSet<Simbolo> destino,
            HashSet<Simbolo> primera, HashSet<Simbolo> segunda)
        {
            primera.IntersectWith(segunda);
            destino.Clear();
            destino.UnionWith(primera);
        }

        private void Asignar(int inicio, int fin, HashSet<Simbolo> estado)
        {
            bool declaracion = EsTipoVariable(tokens[inicio].Tipo);
            int nombre = inicio + (declaracion ? 1 : 0);
            if (nombre + 1 >= fin || tokens[nombre + 1].Tipo != "ASIG") return;
            Leer(nombre + 2, fin, estado); // vA = vA + 1 lee antes de escribir.
            Simbolo destino = tabla.SimbolosPorToken[nombre];
            if (destino != null) estado.Add(destino);
        }

        private void Recorrer(int inicio, int fin, HashSet<Simbolo> estado)
        {
            for (int i = inicio; i < fin;)
            {
                string tipo = tokens[i].Tipo;
                if (tipo == "PR2")
                {
                    int parentesis = i + 3, llave = cierres[parentesis] + 1;
                    int cierre = cierres[llave];
                    var funcion = new HashSet<Simbolo>(estado);
                    for (int p = parentesis + 1; p < cierres[parentesis]; p++)
                    {
                        Simbolo simbolo = tabla.SimbolosPorToken[p];
                        if (simbolo != null && simbolo.Clase == "PARAMETRO" && simbolo.PosicionDeclaracion == p)
                            funcion.Add(simbolo);
                    }
                    Recorrer(llave + 1, cierre, funcion);
                    i = cierre + 1;
                }
                else if (tipo == "PR11")
                {
                    int parentesis = i + 1, llave = cierres[parentesis] + 1;
                    int cierre = cierres[llave];
                    Leer(parentesis + 1, cierres[parentesis], estado);
                    var si = new HashSet<Simbolo>(estado);
                    Recorrer(llave + 1, cierre, si);
                    var no = new HashSet<Simbolo>(estado);
                    i = cierre + 1;
                    if (i < fin && tokens[i].Tipo == "PR12")
                    {
                        int aperturaSino = i + 1, cierreSino = cierres[aperturaSino];
                        Recorrer(aperturaSino + 1, cierreSino, no);
                        i = cierreSino + 1;
                    }
                    UnirRamas(estado, si, no);
                }
                else if (tipo == "PR15")
                {
                    int parentesis = i + 1, llave = cierres[parentesis] + 1;
                    Leer(parentesis + 1, cierres[parentesis], estado);
                    int cierre = cierres[llave];
                    Recorrer(llave + 1, cierre, new HashSet<Simbolo>(estado));
                    i = cierre + 1; // Puede ejecutarse cero veces.
                }
                else if (tipo == "PR18")
                {
                    int parentesis = i + 1, finCabecera = cierres[parentesis];
                    int primero = HastaPuntoComa(parentesis + 1, finCabecera);
                    int segundo = HastaPuntoComa(primero + 1, finCabecera);
                    Asignar(parentesis + 1, primero, estado); // Siempre se ejecuta.
                    Leer(primero + 1, segundo, estado);
                    int llave = finCabecera + 1, cierre = cierres[llave];
                    var cuerpo = new HashSet<Simbolo>(estado);
                    Recorrer(llave + 1, cierre, cuerpo);
                    Asignar(segundo + 1, finCabecera, cuerpo);
                    i = cierre + 1; // El cuerpo puede no ejecutarse.
                }
                else if (tipo == "PR16")
                {
                    int llave = i + 1, cierre = cierres[llave];
                    Recorrer(llave + 1, cierre, estado); // REPT ejecuta el cuerpo al menos una vez.
                    int parentesis = cierre + 2;
                    Leer(parentesis + 1, cierres[parentesis], estado);
                    i = cierres[parentesis] + 2;
                }
                else if (tipo == "PR13")
                {
                    int parentesis = i + 1, llave = cierres[parentesis] + 1;
                    Leer(parentesis + 1, cierres[parentesis], estado);
                    int cierre = cierres[llave];
                    HashSet<Simbolo> comun = null;
                    bool hayDefecto = false;
                    int caso = llave + 1;
                    while (caso < cierre)
                    {
                        if (tokens[caso].Tipo == "PR14") hayDefecto = true;
                        int romper = caso + 2;
                        while (romper < cierre && tokens[romper].Tipo != "PR19")
                        {
                            if (tokens[romper].Tipo == "CE3" || tokens[romper].Tipo == "CE1")
                                romper = cierres[romper];
                            romper++;
                        }
                        var rama = new HashSet<Simbolo>(estado);
                        Recorrer(caso + 2, romper, rama);
                        if (comun == null) comun = rama;
                        else comun.IntersectWith(rama);
                        caso = romper + 2; // ROMPER ;
                    }
                    if (comun != null)
                    {
                        if (!hayDefecto) comun.IntersectWith(estado);
                        estado.Clear();
                        estado.UnionWith(comun);
                    }
                    i = cierre + 1;
                }
                else if (tipo == "CE3")
                {
                    Recorrer(i + 1, cierres[i], estado);
                    i = cierres[i] + 1;
                }
                else if (EsTipoVariable(tipo) && i + 1 < fin && tokens[i + 1].Tipo == "IDV")
                {
                    int puntoComa = HastaPuntoComa(i + 2, fin);
                    if (i + 2 < puntoComa && tokens[i + 2].Tipo == "ASIG")
                        Asignar(i, puntoComa, estado);
                    i = puntoComa + 1;
                }
                else if (tipo == "IDV" && i + 1 < fin && tokens[i + 1].Tipo == "ASIG")
                {
                    int puntoComa = HastaPuntoComa(i + 2, fin);
                    Asignar(i, puntoComa, estado);
                    i = puntoComa + 1;
                }
                else if (tipo == "PR3" || tipo == "PR5" || tipo == "IDF")
                {
                    int puntoComa = HastaPuntoComa(i + 1, fin);
                    Leer(i + 1, puntoComa, estado);
                    i = puntoComa + 1;
                }
                else
                {
                    Leer(i, i + 1, estado);
                    i++;
                }
            }
        }

        private static bool EsTipoVariable(string tipo)
        {
            return tipo == "PR23" || tipo == "PR24" || tipo == "PR25" ||
                tipo == "PR26" || tipo == "PR27";
        }
    }
}
