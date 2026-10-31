using System;
using System.Collections.Generic;

namespace Automatas
{
    // Análisis estructural conservador: cada camino alcanzable debe terminar en REGR.
    // No evalúa constantes ni supone que MIENT/POR se ejecuten alguna vez.
    internal sealed class AnalizadorRetornos
    {
        [Flags]
        private enum Flujo { Ninguno = 0, Continua = 1, Retorna = 2, Rompe = 4 }
        private readonly List<(string Tipo, string Valor, int Linea)> tokens;
        private readonly int[] cierres;
        public List<(int Linea, string Mensaje)> Errores { get; } = new List<(int, string)>();

        public AnalizadorRetornos(List<(string Tipo, string Valor, int Linea)> tokens, TablaAmbitos tabla)
        {
            this.tokens = tokens;
            cierres = new int[tokens.Count];
            var pila = new Stack<int>();
            for (int i = 0; i < tokens.Count; i++)
                if (Es(i, "CE1") || Es(i, "CE3")) pila.Push(i);
                else if (Es(i, "CE2") || Es(i, "CE4")) cierres[pila.Pop()] = i;

            // Cada función se verifica por separado, incluso si está en una rama
            // inalcanzable. Los retornos de sus hijas no cuentan para ella.
            for (int i = 0; i < tokens.Count; i++)
            {
                if (!Es(i, "PR2")) continue;
                Simbolo funcion = tabla.SimbolosPorToken[i + 2];
                if (funcion == null || funcion.Clase != "FUNCION" || funcion.Tipo == "VAC") continue;
                int llave = cierres[i + 3] + 1;
                Flujo flujo = Bloque(llave + 1, cierres[llave]);
                if ((flujo & (Flujo.Continua | Flujo.Rompe)) != 0)
                    Errores.Add((tokens[i + 2].Linea, "ERROR DE RETORNO: La función '" + funcion.Nombre +
                        "' de tipo '" + funcion.Tipo + "' no garantiza devolver un valor en todas las rutas."));
            }
        }

        private bool Es(int i, string tipo) => i < tokens.Count && tokens[i].Tipo == tipo;
        private int PuntoComa(int i) { while (!Es(i, "CE8")) i++; return i; }

        private Flujo Bloque(int inicio, int fin)
        {
            Flujo flujo = Flujo.Continua;
            for (int i = inicio; i < fin && (flujo & Flujo.Continua) != 0;)
            {
                Flujo siguiente = Sentencia(ref i);
                // Solo los caminos que continuaban pueden entrar a esta sentencia.
                flujo = (flujo & ~Flujo.Continua) | siguiente;
            }
            return flujo;
        }

        private Flujo Sentencia(ref int i)
        {
            int p = i;
            if (Es(p, "PR2"))
            {
                i = cierres[cierres[p + 3] + 1] + 1;
                return Flujo.Continua;
            }
            if (Es(p, "PR3")) { i = PuntoComa(p) + 1; return Flujo.Retorna; }
            if (Es(p, "PR19")) { i += 2; return Flujo.Rompe; }
            if (Es(p, "PR11"))
            {
                int llave = cierres[p + 1] + 1, cierre = cierres[llave];
                Flujo si = Bloque(llave + 1, cierre), no = Flujo.Continua;
                i = cierre + 1;
                if (Es(i, "PR12"))
                {
                    no = Bloque(i + 2, cierres[i + 1]);
                    i = cierres[i + 1] + 1;
                }
                return si | no;
            }
            if (Es(p, "PR15") || Es(p, "PR18"))
            {
                int llave = cierres[p + 1] + 1;
                Flujo cuerpo = Bloque(llave + 1, cierres[llave]);
                i = cierres[llave] + 1;
                return Flujo.Continua | (cuerpo & Flujo.Retorna);
            }
            if (Es(p, "PR16"))
            {
                int cierre = cierres[p + 1];
                Flujo cuerpo = Bloque(p + 2, cierre);
                i = cierres[cierre + 2] + 2;
                // REPT se ejecuta al menos una vez. Un ROMPER puede salir sin retornar.
                return (cuerpo & Flujo.Retorna) |
                    ((cuerpo & (Flujo.Continua | Flujo.Rompe)) != 0 ? Flujo.Continua : Flujo.Ninguno);
            }
            if (Es(p, "PR13"))
            {
                int llave = cierres[p + 1] + 1, fin = cierres[llave];
                Flujo ramas = Flujo.Ninguno;
                bool defecto = false;
                int caso = llave + 1;
                while (caso < fin)
                {
                    defecto |= Es(caso, "PR14");
                    int romper = caso + 2;
                    while (romper < fin && !Es(romper, "PR19"))
                    {
                        if (Es(romper, "CE1") || Es(romper, "CE3")) romper = cierres[romper];
                        romper++;
                    }
                    Flujo rama = Bloque(caso + 2, romper);
                    ramas |= (rama & Flujo.Retorna) |
                        ((rama & (Flujo.Continua | Flujo.Rompe)) != 0 ? Flujo.Continua : Flujo.Ninguno);
                    caso = romper + 2;
                }
                i = fin + 1;
                return ramas | (defecto ? Flujo.Ninguno : Flujo.Continua);
            }
            i = PuntoComa(p) + 1;
            return Flujo.Continua;
        }
    }
}
