namespace Automatas
{
    public class Simbolo
    {
        public int Numero { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }   // ENT, DEC, TXT, BOOL, CAR, VAC
        public string Valor { get; set; }  // literal o expresión
        public string Clase { get; set; }  // VARIABLE, FUNCION, PARAMETRO
        public int LineaDeclaracion { get; set; }
        public Ambito Ambito { get; set; }
        // Las funciones pueden invocarse antes de aparecer en el archivo.
        public bool IgnorarLineaDeclaracion { get; set; }
    }
}