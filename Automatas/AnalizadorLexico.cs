using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatas
{
    internal class AnalizadorLexico
    {
        private int[,] matrizTransicion;
        private string[] categoriaFDC;
        private Dictionary<char, int> mapaColumnas;

        public AnalizadorLexico(string rutaBD)
        {
            mapaColumnas = new Dictionary<char, int>();
            InicializarAlfabeto();

            matrizTransicion = new int[240, mapaColumnas.Count];
            categoriaFDC = new string[240];

            CargarMatrizDesdeSQLite(rutaBD);
        }

        public string AnalizarCadena(string palabra)
        {
            int estadoActual = 1;
            palabra += "\n";

            foreach (char c in palabra)
            {
                if (!mapaColumnas.ContainsKey(c))
                {
                    estadoActual = 229;
                }
                else
                {
                    int columna = mapaColumnas[c];
                    estadoActual = matrizTransicion[estadoActual, columna];
                }
            }
            return categoriaFDC[estadoActual];
        }

        private void InicializarAlfabeto()
        {
            int indiceColumna = 0;

            for (char c = 'a'; c <= 'z'; c++) mapaColumnas[c] = indiceColumna++;
            for (char c = 'A'; c <= 'Z'; c++) mapaColumnas[c] = indiceColumna++;
            for (char c = '0'; c <= '9'; c++) mapaColumnas[c] = indiceColumna++;

            string especiales = "+-*/^&|!><=(){}[].,;:\"'¡@#$%?~\\";
            foreach (char c in especiales) mapaColumnas[c] = indiceColumna++;

            // FDC mapeado al espacio
            mapaColumnas[' '] = indiceColumna++;  // Columna 93: Espacio
            mapaColumnas['\n'] = indiceColumna++; //Columna 94: FDC
        }

        private void CargarMatrizDesdeSQLite(string rutaBD)
        {
            string connectionString = $"Data Source={rutaBD}";

            using (var connection = GetConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Matriz ORDER BY ESTADO ASC";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!!!int.TryParse(reader["ESTADO"].ToString(), out int estado))
                        {
                            continue;
                        }

                        foreach (var kvp in mapaColumnas)
                        {
                            int indiceArreglo = kvp.Value;
                            matrizTransicion[estado, indiceArreglo] = Convert.ToInt32(reader.GetValue(indiceArreglo + 1));
                        }

                        categoriaFDC[estado] = reader["CAT"].ToString();
                    }
                }
            }
        }
        private static SQLiteConnection GetConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }



    }
}
