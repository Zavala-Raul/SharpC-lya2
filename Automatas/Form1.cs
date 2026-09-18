using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Automatas
{
    public partial class Form1 : Form
    {
        // --- Estado del editor y análisis ---
        private List<(int linea, int columna, string token, int estilo)> tokensGlobales
            = new List<(int linea, int columna, string token, int estilo)>();

        private List<(int linea, int columna, string token, int estilo)> tokensTokens2
            = new List<(int linea, int columna, string token, int estilo)>();

        private AnalizadorLexico lexico = new AnalizadorLexico("C:\\DEV\\LyA\\GatoSabeDB.sqlite");

        private List<(int linea, string mensaje)> listaErrores = new List<(int, string)>();
        private int intTotalErrores = 0;

        // --- Variables para el Análisis Sintáctico ---
        private List<(string Tipo, string Valor, int Linea)> tokensSintactico = new List<(string, string, int)>();
        private List<(int Linea, string Mensaje)> erroresSintacticos = new List<(int, string)>();
        private List<(int Linea, string Mensaje)> erroresTipos = new List<(int, string)>();
        private AnalizadorSintacticoLl1 analizadorSintactico =
            new AnalizadorSintacticoLl1(TablaSintacticaSharpC.CrearTablaCompleta());

        // Tabla de símbolos y funciones
        private Dictionary<string, Simbolo> tablaSimbolos = new Dictionary<string, Simbolo>();
        private int contadorSimbolos = 1;
        private Dictionary<string, Funcion> tablaFunciones = new Dictionary<string, Funcion>();

        public Form1()
        {
            InitializeComponent();
            InitializeEditors();
        }

        // -------------------- Inicialización --------------------

        private void InitializeEditors()
        {
            InitializeSourceEditor();
            InitializeTokensEditor();
        }

        private void InitializeSourceEditor()
        {
            scintilla1.StyleResetDefault();
            scintilla1.Styles[Style.Default].Font = "Consolas";
            scintilla1.Styles[Style.Default].Size = 10;
            scintilla1.Styles[Style.Default].ForeColor = Color.Black;
            scintilla1.Styles[Style.Default].BackColor = Color.LightGray;
            scintilla1.StyleClearAll();

            scintilla1.Margins[0].Width = 40;
            scintilla1.Margins[0].Type = MarginType.Number;
            scintilla1.Margins[0].Sensitive = false;
            scintilla1.Margins[0].Mask = 0;

            scintilla1.Lexer = Lexer.Container;
            scintilla1.StyleNeeded += Scintilla1_StyleNeeded;
            scintilla1.UpdateUI += Scintilla1_UpdateUI;

            scintilla1.Styles[1].ForeColor = Color.Blue;         // PR
            scintilla1.Styles[2].ForeColor = Color.Black;        // ID
            scintilla1.Styles[3].ForeColor = Color.Orange;       // CNU
            scintilla1.Styles[4].ForeColor = Color.Purple;       // Operadores
            scintilla1.Styles[5].ForeColor = Color.Yellow;       // Cadenas
            scintilla1.Styles[6].ForeColor = Color.Coral;        // Caracteres
            scintilla1.Styles[7].ForeColor = Color.GreenYellow;  // Comentarios
            scintilla1.Styles[8].ForeColor = Color.DarkGoldenrod;// Caracteres especiales
            scintilla1.Styles[9].ForeColor = Color.Red;          // Errores
            scintilla1.Styles[10].ForeColor = Color.Black;       // ASIG

            scintilla1.TabWidth = 5;
            scintilla1.UseTabs = false;

            var indError = scintilla1.Indicators[0];
            indError.Style = IndicatorStyle.StraightBox;
            indError.ForeColor = Color.Red;
            indError.Alpha = 40;
            indError.OutlineAlpha = 255;
        }

        private void InitializeTokensEditor()
        {
            scintilla2.StyleResetDefault();
            scintilla2.Styles[Style.Default].Font = "Consolas";
            scintilla2.Styles[Style.Default].Size = 10;
            scintilla2.Styles[Style.Default].ForeColor = Color.Black;
            scintilla2.Styles[Style.Default].BackColor = Color.LightGray;
            scintilla2.StyleClearAll();

            scintilla2.Margins[0].Width = 40;
            scintilla2.Margins[0].Type = MarginType.Number;
            scintilla2.Margins[0].Sensitive = false;
            scintilla2.Margins[0].Mask = 0;

            scintilla2.Lexer = Lexer.Container;
            scintilla2.StyleNeeded += Scintilla2_StyleNeeded;

            scintilla2.Styles[1].ForeColor = Color.Blue;
            scintilla2.Styles[2].ForeColor = Color.Black;
            scintilla2.Styles[3].ForeColor = Color.Orange;
            scintilla2.Styles[4].ForeColor = Color.Purple;
            scintilla2.Styles[5].ForeColor = Color.Yellow;
            scintilla2.Styles[6].ForeColor = Color.Coral;
            scintilla2.Styles[7].ForeColor = Color.GreenYellow;
            scintilla2.Styles[8].ForeColor = Color.DarkGoldenrod;
            scintilla2.Styles[9].ForeColor = Color.Red;
            scintilla2.Styles[10].ForeColor = Color.Black;
        }

        // -------------------- Análisis Sintáctico --------------------

        private void EjecutarAnalisisSintactico()
        {
            erroresSintacticos.AddRange(
                analizadorSintactico.Analizar(tokensSintactico, scintilla1.Lines.Count));
        }

        private void EjecutarVerificacionTipos()
        {
            var verificador = new VerificadorTipos(tablaSimbolos, tablaFunciones);
            verificador.Verificar(tokensSintactico);
            erroresTipos.AddRange(verificador.Errores);
        }
        // -------------------- Resto de métodos de tu clase original --------------------

        private void ReAnalizarTodo()
        {
            tokensGlobales.Clear();
            for (int i = 0; i < scintilla1.Lines.Count; i++) ReAnalizarLinea(i);
        }

        private void ReAnalizarLinea(int linea)
        {
            string texto = scintilla1.Lines[linea].Text;
            tokensGlobales.RemoveAll(t => t.linea == linea);

            var tokens = TokenizarLinea(texto, linea);
            foreach (var tk in tokens)
            {
                string token = tk.token.Trim();
                if (token.StartsWith("//"))
                {
                    tokensGlobales.Add((linea, tk.columna, token, 7));
                    PintarToken(linea, tk.columna, token.Length, 7);
                    continue;
                }

                string resultado = lexico.AnalizarCadena(token);
                int estilo = ObtenerEstilo(resultado);

                tokensGlobales.Add((linea, tk.columna, token, estilo));
                PintarToken(linea, tk.columna, token.Length, estilo);
            }
        }

        private void ResaltarLineasError()
        {
            scintilla1.IndicatorCurrent = 0;
            scintilla1.IndicatorClearRange(0, scintilla1.TextLength);

            foreach (var err in listaErrores)
            {
                int lineaIndex = err.linea - 1;
                if (lineaIndex < 0 || lineaIndex >= scintilla1.Lines.Count) continue;

                var linea = scintilla1.Lines[lineaIndex];
                scintilla1.IndicatorCurrent = 0;
                scintilla1.IndicatorFillRange(linea.Position, linea.Length);
            }
        }

        private void Scintilla1_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            int linea = scintilla1.CurrentLine;
            ReAnalizarLinea(linea);
        }

        private void scintilla1_TextChanged(object sender, EventArgs e)
        {
            scintilla1.StartStyling(0);
            scintilla1.SetStyling(scintilla1.TextLength, 0);

            ReAnalizarTodo();
            scintilla1.Refresh();
        }

        private void Scintilla1_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            foreach (var tk in tokensGlobales) PintarToken(tk.linea, tk.columna, tk.token.Length, tk.estilo);
        }

        private void Scintilla2_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            foreach (var tk in tokensTokens2) PintarTokenEnScintilla2(tk.linea, tk.columna, tk.token.Length, tk.estilo);
        }

        private void PintarToken(int linea, int columnaInicio, int longitud, int estilo)
        {
            try { int pos = scintilla1.Lines[linea].Position + columnaInicio; scintilla1.StartStyling(pos); scintilla1.SetStyling(longitud, estilo); } catch { }
        }

        private void PintarTokenEnScintilla2(int linea, int columnaInicio, int longitud, int estilo)
        {
            try { if (linea < 0 || linea >= scintilla2.Lines.Count) return; int pos = scintilla2.Lines[linea].Position + columnaInicio; scintilla2.StartStyling(pos); scintilla2.SetStyling(longitud, estilo); } catch { }
        }

        private List<(int linea, int columna, string token)> TokenizarLinea(string linea, int numLinea)
        {
            return TokenizadorFuenteSharpC.TokenizarLinea(linea, numLinea);
        }
        private bool IsTokenInvalido(string tokenResultado)
        {
            if (string.IsNullOrEmpty(tokenResultado)) return false;
            string lower = tokenResultado.ToLower();
            return lower.Contains("inválido") || lower.Contains("inválida") || lower.Contains("error");
        }

        private int ObtenerEstilo(string resultado)
        {
            if (IsTokenInvalido(resultado)) return 9;
            resultado = resultado.Replace("TOKEN: ", "");

            if (resultado.StartsWith("PR")) return 1;
            if (resultado.StartsWith("ID")) return 2;
            if (resultado.StartsWith("CN")) return 3;
            if (resultado.StartsWith("OP")) return 4;
            if (resultado.StartsWith("CAD")) return 5;
            if (resultado.StartsWith("CAR")) return 6;
            if (resultado.StartsWith("COM")) return 7;
            if (resultado.StartsWith("CE")) return 8;
            if (resultado.StartsWith("ASIG")) return 10;
            return 9;
        }

        private int RegistrarIdentificador(string nombre, string tipo = "", string valor = "")
        {
            string id = nombre.Trim();
            if (tablaSimbolos.ContainsKey(id))
            {
                var s = tablaSimbolos[id];
                if (!string.IsNullOrEmpty(tipo)) s.Tipo = tipo;
                if (!string.IsNullOrEmpty(valor)) s.Valor = valor;
                return s.Numero;
            }
            var sim = new Simbolo() { Numero = contadorSimbolos, Nombre = id, Tipo = tipo, Valor = valor };
            tablaSimbolos.Add(id, sim);
            contadorSimbolos++;
            return sim.Numero;
        }

        private void RegistrarFuncion(string nombre, string tipoRetorno, int lineaInicio)
        {
            string id = nombre.Trim();
            if (!tablaFunciones.ContainsKey(id))
            {
                var f = new Funcion() { Nombre = id, TipoRetorno = tipoRetorno, LineaInicio = lineaInicio, LineaCuerpoInicio = 0, LineaCuerpoFin = 0 };
                tablaFunciones.Add(id, f);
            }
        }

        private void AgregarParametroFuncion(string nombreFuncion, string tipoParam, string nombreParam)
        {
            if (!tablaFunciones.ContainsKey(nombreFuncion)) return;
            tablaFunciones[nombreFuncion].Parametros.Add((tipoParam, nombreParam));
        }

        private void LlenarDgvErrores()
        {
            dgvErrores.Rows.Clear();
            foreach (var err in listaErrores) dgvErrores.Rows.Add(err.linea, err.mensaje);
        }

        private void LlenarDgvErroresSintaxis()
        {
            if (dgvErroresSintaxis != null)
            {
                dgvErroresSintaxis.Rows.Clear();
                foreach (var err in erroresSintacticos) dgvErroresSintaxis.Rows.Add(err.Linea, err.Mensaje);
                foreach (var err in erroresTipos) dgvErroresSintaxis.Rows.Add(err.Linea, err.Mensaje);
            }
        }

        private void LlenarTablaSimbolos()
        {
            dgvSimbolos.Rows.Clear();
            foreach (var sim in tablaSimbolos.Values.OrderBy(s => s.Numero)) dgvSimbolos.Rows.Add(sim.Numero, sim.Nombre, sim.Tipo, sim.Valor);
        }

        private void LlenarTablaFunciones()
        {
            dgvFunciones.Rows.Clear();
            foreach (var f in tablaFunciones.Values)
            {
                string listaParams = string.Join(", ", f.Parametros.Select(p => $"{p.tipo} {p.nombre}"));
                string cuerpo = (f.LineaCuerpoInicio > 0 && f.LineaCuerpoFin > 0) ? $"{f.LineaCuerpoInicio} - {f.LineaCuerpoFin}" : "—";
                dgvFunciones.Rows.Add(f.Nombre, f.TipoRetorno, listaParams, cuerpo);
            }
        }

        private string ObtenerValorAsignadoEnLinea(
    string lineaTexto,
    List<(int linea, int columna, string token)> tokensLinea)
        {
            int idxAsig = -1;

            // Buscar específicamente el token ASIG
            foreach (var tk in tokensLinea)
            {
                string resultado = lexico.AnalizarCadena(tk.token)
                                         .Replace("TOKEN: ", "")
                                         .Trim();

                if (resultado == "ASIG")
                {
                    idxAsig = tk.columna;
                    break;
                }
            }

            // Si no existe asignación real
            if (idxAsig == -1)
                return "";

            // Buscar fin de instrucción
            int idxPuntoComa = lineaTexto.IndexOf(';', idxAsig);

            if (idxPuntoComa == -1)
                idxPuntoComa = lineaTexto.Length;

            // Obtener texto después del '='
            string valor = lineaTexto.Substring(
                idxAsig + 1,
                idxPuntoComa - idxAsig - 1);

            return valor.Trim();
        }

        private void GuardarArchivo()
        {
            if (HayTextoConError())
            {
                var result = MessageBox.Show("¿Estás seguro de guardar el código fuente si hay errores presentes?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;
            }
            using (SaveFileDialog save = new SaveFileDialog())
            {
                save.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                save.Title = "Guardar archivo";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(save.FileName, scintilla1.Text);
                    MessageBox.Show("Archivo guardado correctamente.");
                }
            }
        }

        private void CargarArchivo()
        {
            using (OpenFileDialog open = new OpenFileDialog())
            {
                open.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                open.Title = "Abrir archivo";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    string contenido = System.IO.File.ReadAllText(open.FileName);
                    scintilla1.Text = contenido;
                    tokensGlobales.Clear();
                    ReAnalizarTodo();
                }
            }
        }

        private void btnEjecutar_Click(object sender, EventArgs e)
        {
            tokensGlobales.Clear();
            tokensTokens2.Clear();
            listaErrores.Clear();
            tablaSimbolos.Clear();
            tablaFunciones.Clear();
            dgvErroresSintaxis.Rows.Clear();
            contadorSimbolos = 1;
            scintilla2.Text = "";
            intTotalErrores = 0;

            tokensSintactico.Clear();
            erroresSintacticos.Clear();
            erroresTipos.Clear();

            int totalLineas = scintilla1.Lines.Count;
            string nombreFuncionActual = null;
            bool esDeclaracionFuncion = false;
            bool dentroDeFuncion = false;
            string tipoActual = "";

            for (int i = 0; i < totalLineas; i++)
            {
                string linea = scintilla1.Lines[i].Text;
                var tokens = TokenizarLinea(linea, i + 1);

                for (int tIdx = 0; tIdx < tokens.Count; tIdx++)
                {
                    var tk = tokens[tIdx];
                    int lineaToken = tk.linea;
                    int columna = tk.columna;
                    string lexema = tk.token.Trim();
                    bool esDestinoAsignacion = (tIdx + 1 < tokens.Count) && (tokens[tIdx + 1].token.Trim() == "=");

                    if (lexema.StartsWith("//"))
                    {
                        AgregarTokenATabla("COM", 7);
                        continue;
                    }

                    try
                    {
                        string resultado = lexico.AnalizarCadena(lexema);
                        string resultadoNormalizado = resultado.Replace("TOKEN: ", "");
                        int estiloToken = ObtenerEstilo(resultado);

                        if (IsTokenInvalido(resultado))
                        {
                            listaErrores.Add((lineaToken, resultado));
                            AgregarTokenATabla(resultado, 9);
                            continue;
                        }

                        // =========================================================
                        // --- INICIO DE NORMALIZACIÓN PARA TABLA SINTÁCTICA ---
                        string tokenSintactico = resultadoNormalizado;

                        // 1. Quitar textos extra (ej. "CN Entera" -> "CN")
                        if (tokenSintactico.Contains(" "))
                            tokenSintactico = tokenSintactico.Split(' ')[0];

                        // 2. Ajustar numéricos genéricos según tu GLC
                        if (tokenSintactico == "CN")
                            tokenSintactico = "CNU";

                        // 3. LIMPIEZA CRÍTICA: Quitar números de tabla de símbolos (ej. "IDV1" -> "IDV")
                        if (tokenSintactico.StartsWith("IDV"))
                            tokenSintactico = "IDV";
                        if (tokenSintactico.StartsWith("IDF"))
                            tokenSintactico = "IDF";

                        // 4. Agregar a la lista sintáctica (ignorando comentarios)
                        if (!tokenSintactico.StartsWith("COM"))
                        {
                            tokensSintactico.Add((tokenSintactico, lexema, lineaToken));
                        }
                        // --- FIN DE NORMALIZACIÓN ---
                        // =========================================================

                        if (lexema.Equals("FUNC", StringComparison.OrdinalIgnoreCase))
                        {
                            esDeclaracionFuncion = true;
                            AgregarTokenATabla(resultadoNormalizado, estiloToken);
                            continue;
                        }

                        if (resultadoNormalizado.StartsWith("PR"))
                        {
                            string posibleTipo = ObtenerTipoDesdePR(resultadoNormalizado);
                            if (!string.IsNullOrEmpty(posibleTipo)) tipoActual = posibleTipo;
                            AgregarTokenATabla(resultadoNormalizado, estiloToken);
                            continue;
                        }

                        if (resultadoNormalizado.StartsWith("IDF"))
                        {
                            nombreFuncionActual = lexema;
                            RegistrarFuncion(nombreFuncionActual, tipoActual, lineaToken);
                            AgregarTokenATabla(resultadoNormalizado, estiloToken);
                            continue;
                        }

                        if (esDeclaracionFuncion && resultadoNormalizado.StartsWith("IDV"))
                        {
                            if (!string.IsNullOrEmpty(nombreFuncionActual))
                            {
                                string tipoParam = string.IsNullOrEmpty(tipoActual) ? "?" : tipoActual;
                                AgregarParametroFuncion(nombreFuncionActual, tipoParam, lexema);
                            }
                            AgregarTokenATabla(resultadoNormalizado, estiloToken);
                            continue;
                        }

                        if (resultadoNormalizado.StartsWith("IDV"))
                        {
                            int num;
                            // 1. Declaración de variable
                            if (!string.IsNullOrEmpty(tipoActual) && !dentroDeFuncion)
                            {
                                num = RegistrarIdentificador(lexema, tipoActual, "");
                                tipoActual = "";
                            }
                            else
                            {
                                // 2. Uso o reasignación de variable (el valor se valida y actualiza en VerificadorTipos)
                                num = tablaSimbolos.ContainsKey(lexema) ? tablaSimbolos[lexema].Numero : RegistrarIdentificador(lexema, "", "");
                            }

                            AgregarTokenATabla(resultadoNormalizado + num.ToString(), estiloToken);
                            continue;
                        }

                        AgregarTokenATabla(resultadoNormalizado, estiloToken);
                    }
                    catch
                    {
                        listaErrores.Add((lineaToken, "ERROR_INTERNO"));
                        AgregarTokenATabla("ERROR_INTERNO", 9);
                    }
                }

                if (linea.Trim() == "{")
                {
                    if (!string.IsNullOrEmpty(nombreFuncionActual))
                    {
                        dentroDeFuncion = true;
                        tablaFunciones[nombreFuncionActual].LineaCuerpoInicio = i + 1;
                    }
                }

                if (linea.Trim() == "}")
                {
                    if (dentroDeFuncion)
                    {
                        if (!string.IsNullOrEmpty(nombreFuncionActual)) tablaFunciones[nombreFuncionActual].LineaCuerpoFin = i + 1;
                        dentroDeFuncion = false;
                        nombreFuncionActual = null;
                    }
                }

                if (esDeclaracionFuncion && linea.Contains(")"))
                {
                    esDeclaracionFuncion = false;
                    tipoActual = "";
                }

                if (!esDeclaracionFuncion)
                {
                    tipoActual = "";
                }

                NuevaLineaTokens();
            }

            if (listaErrores.Count == 0 && tokensSintactico.Count > 0)
            {
                EjecutarAnalisisSintactico();

                if (erroresSintacticos.Count == 0)
                {
                    EjecutarVerificacionTipos();
                }
            }

            intTotalErrores = listaErrores.Count;
            lblTotalErrores.Text = "Total de Errores Léxicos: " + intTotalErrores.ToString();
            LlenarDgvErrores();
            LlenarDgvErroresSintaxis();
            LlenarTablaSimbolos();
            LlenarTablaFunciones();
            ResaltarLineasError();

            scintilla2.Refresh();
        }

        private void GuardarTokens()
        {
            if (string.IsNullOrWhiteSpace(scintilla2.Text))
            {
                MessageBox.Show("No es posible guardar Tokens porque no existen.", "Operación Cancelada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (listaErrores.Count > 0)
            {
                MessageBox.Show("No es posible guardar Tokens. Existen Errores de Tokens.", "Operación Cancelada", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            using (SaveFileDialog save = new SaveFileDialog())
            {
                save.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                save.Title = "Guardar archivo de tokens";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(save.FileName, scintilla2.Text);
                    MessageBox.Show("Tokens guardados correctamente.");
                }
            }
        }

        private void btnGuardarPrograma_Click(object sender, EventArgs e) => GuardarArchivo();
        private void btnCargarPrograma_Click(object sender, EventArgs e) => CargarArchivo();
        private void btnGuardarArchivoTokens_Click(object sender, EventArgs e) => GuardarTokens();

        private void NuevaLineaTokens() { scintilla2.AppendText("\n"); }

        private void AgregarTokenATabla(string token, int estilo = -1)
        {
            string t = token.Replace("TOKEN: ", "");

            if (scintilla2.Lines.Count == 1 && scintilla2.Lines[0].Text == "")
            {
                scintilla2.AppendText(t);
                if (estilo >= 0) tokensTokens2.Add((0, 0, t, estilo));
                return;
            }

            int ultima = scintilla2.Lines.Count - 1;
            string textoUltima = scintilla2.Lines[ultima].Text;
            int columnaInicio = (textoUltima.Length == 0) ? 0 : textoUltima.Length + 1;

            if (textoUltima.Trim().Length == 0) scintilla2.AppendText(t);
            else scintilla2.AppendText(" " + t);

            if (estilo >= 0) tokensTokens2.Add((ultima, columnaInicio, t, estilo));
        }

        private string ObtenerTipoDesdePR(string tokenPR)
        {
            switch (tokenPR)
            {
                case "PR23": return "ENT";
                case "PR24": return "DEC";
                case "PR25": return "TXT";
                case "PR26": return "BOOL";
                case "PR27": return "CAR";
                case "PR28": return "VAC";
                default: return "";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { }
        private void dgvFunciones_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void scintilla2_CharAdded(object sender, CharAddedEventArgs e) { }

        private void scintilla1_CharAdded(object sender, CharAddedEventArgs e)
        {
            int pos = scintilla1.CurrentPosition;
            if (pos >= 5)
            {
                int maxCheck = Math.Min(pos, 20);
                string texto = scintilla1.GetTextRange(pos - maxCheck, maxCheck);
                int count = 0;
                for (int i = texto.Length - 1; i >= 0; i--) { if (texto[i] == ' ') count++; else break; }
                if (count > 0 && count % 5 == 0)
                {
                    int tabs = count / 5;
                    scintilla1.DeleteRange(pos - count, count);
                    for (int i = 0; i < tabs; i++) scintilla1.InsertText(pos - count + i, "\t");
                    scintilla1.GotoPosition(pos - count + tabs);
                }
            }
        }

        private bool HayTextoConError()
        {
            for (int i = 0; i < scintilla1.TextLength; i++)
            {
                int style = scintilla1.GetStyleAt(i);
                if (style == 9) return true;
            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}


