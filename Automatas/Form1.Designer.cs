namespace Automatas
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scintilla1 = new ScintillaNET.Scintilla();
            this.btnCargarPrograma = new System.Windows.Forms.Button();
            this.btnEjecutar = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGuardarPrograma = new System.Windows.Forms.Button();
            this.btnGuardarArchivoTokens = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.scintilla2 = new ScintillaNET.Scintilla();
            this.lblTotalErrores = new System.Windows.Forms.Label();
            this.dgvErrores = new System.Windows.Forms.DataGridView();
            this.linea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mensaje = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.dgvSimbolos = new System.Windows.Forms.DataGridView();
            this.identificador = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tipoDato = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ambitoSimbolo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.claseSimbolo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lineaDeclaracion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.regionSimbolo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tamanoBytes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.desplazamientoBytes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dgvFunciones = new System.Windows.Forms.DataGridView();
            this.colNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTipoRetorno = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParametros = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCuerpo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRegionFuncion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTamanoMarco = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvErroresSintaxis = new System.Windows.Forms.DataGridView();
            this.numLinea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.errorSintaxis = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabResultados = new System.Windows.Forms.TabControl();
            this.tabAnalisis = new System.Windows.Forms.TabPage();
            this.tabCodigoIntermedio = new System.Windows.Forms.TabPage();
            this.tabVistasIntermedio = new System.Windows.Forms.TabControl();
            this.tabVistaCodigo = new System.Windows.Forms.TabPage();
            this.tabVistaCuadruplos = new System.Windows.Forms.TabPage();
            this.tabVistaTriplos = new System.Windows.Forms.TabPage();
            this.tabVistaCodigoP = new System.Windows.Forms.TabPage();
            this.tabVistaNotaciones = new System.Windows.Forms.TabPage();
            this.dgvTriplos = new System.Windows.Forms.DataGridView();
            this.txtCodigoP = new System.Windows.Forms.TextBox();
            this.dgvNotaciones = new System.Windows.Forms.DataGridView();
            this.txtCodigoIntermedio = new System.Windows.Forms.TextBox();
            this.dgvCuadruplos = new System.Windows.Forms.DataGridView();
            this.datoObjeto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.datoFuente1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.datoFuente2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.operacionIntermedia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvErroresGeneracion = new System.Windows.Forms.DataGridView();
            this.lineaGeneracion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mensajeGeneracion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelCodigoIntermedio = new System.Windows.Forms.Panel();
            this.btnGenerarCuadruplos = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrores)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSimbolos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFunciones)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErroresSintaxis)).BeginInit();
            this.tabResultados.SuspendLayout();
            this.tabAnalisis.SuspendLayout();
            this.tabCodigoIntermedio.SuspendLayout();
            this.tabVistasIntermedio.SuspendLayout();
            this.tabVistaCodigo.SuspendLayout();
            this.tabVistaCuadruplos.SuspendLayout();
            this.tabVistaTriplos.SuspendLayout();
            this.tabVistaCodigoP.SuspendLayout();
            this.tabVistaNotaciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriplos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNotaciones)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCuadruplos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErroresGeneracion)).BeginInit();
            this.panelCodigoIntermedio.SuspendLayout();
            this.SuspendLayout();
            // 
            // scintilla1
            // 
            this.scintilla1.Location = new System.Drawing.Point(113, 81);
            this.scintilla1.Name = "scintilla1";
            this.scintilla1.Size = new System.Drawing.Size(462, 575);
            this.scintilla1.TabIndex = 2;
            this.scintilla1.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.scintilla1_CharAdded);
            this.scintilla1.TextChanged += new System.EventHandler(this.scintilla1_TextChanged);
            // 
            // btnCargarPrograma
            // 
            this.btnCargarPrograma.Location = new System.Drawing.Point(12, 145);
            this.btnCargarPrograma.Name = "btnCargarPrograma";
            this.btnCargarPrograma.Size = new System.Drawing.Size(80, 35);
            this.btnCargarPrograma.TabIndex = 3;
            this.btnCargarPrograma.Text = "Cargar Programa";
            this.btnCargarPrograma.UseVisualStyleBackColor = true;
            this.btnCargarPrograma.Click += new System.EventHandler(this.btnCargarPrograma_Click);
            // 
            // btnEjecutar
            // 
            this.btnEjecutar.BackColor = System.Drawing.Color.PeachPuff;
            this.btnEjecutar.Location = new System.Drawing.Point(12, 226);
            this.btnEjecutar.Name = "btnEjecutar";
            this.btnEjecutar.Size = new System.Drawing.Size(80, 35);
            this.btnEjecutar.TabIndex = 4;
            this.btnEjecutar.Text = "Ejecutar";
            this.btnEjecutar.UseVisualStyleBackColor = false;
            this.btnEjecutar.Click += new System.EventHandler(this.btnEjecutar_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(219, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(224, 31);
            this.label1.TabIndex = 5;
            this.label1.Text = "Programa Fuente";
            // 
            // btnGuardarPrograma
            // 
            this.btnGuardarPrograma.Location = new System.Drawing.Point(12, 186);
            this.btnGuardarPrograma.Name = "btnGuardarPrograma";
            this.btnGuardarPrograma.Size = new System.Drawing.Size(80, 35);
            this.btnGuardarPrograma.TabIndex = 6;
            this.btnGuardarPrograma.Text = "Guardar Programa";
            this.btnGuardarPrograma.UseVisualStyleBackColor = true;
            this.btnGuardarPrograma.Click += new System.EventHandler(this.btnGuardarPrograma_Click);
            // 
            // btnGuardarArchivoTokens
            // 
            this.btnGuardarArchivoTokens.Location = new System.Drawing.Point(1050, 30);
            this.btnGuardarArchivoTokens.Name = "btnGuardarArchivoTokens";
            this.btnGuardarArchivoTokens.Size = new System.Drawing.Size(69, 35);
            this.btnGuardarArchivoTokens.TabIndex = 7;
            this.btnGuardarArchivoTokens.Text = "Guardar Archivo";
            this.btnGuardarArchivoTokens.UseVisualStyleBackColor = true;
            this.btnGuardarArchivoTokens.Click += new System.EventHandler(this.btnGuardarArchivoTokens_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(741, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(239, 31);
            this.label2.TabIndex = 8;
            this.label2.Text = "Archivo de Tokens";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1258, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(202, 31);
            this.label3.TabIndex = 9;
            this.label3.Text = "Errores Léxicos";
            // 
            // scintilla2
            // 
            this.scintilla2.Location = new System.Drawing.Point(606, 82);
            this.scintilla2.Name = "scintilla2";
            this.scintilla2.Size = new System.Drawing.Size(499, 574);
            this.scintilla2.TabIndex = 10;
            this.scintilla2.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.scintilla2_CharAdded);
            // 
            // lblTotalErrores
            // 
            this.lblTotalErrores.AutoSize = true;
            this.lblTotalErrores.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalErrores.Location = new System.Drawing.Point(1268, 81);
            this.lblTotalErrores.Name = "lblTotalErrores";
            this.lblTotalErrores.Size = new System.Drawing.Size(165, 24);
            this.lblTotalErrores.TabIndex = 12;
            this.lblTotalErrores.Text = "Total de Errores: 0";
            // 
            // dgvErrores
            // 
            this.dgvErrores.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvErrores.BackgroundColor = System.Drawing.Color.White;
            this.dgvErrores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErrores.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.linea,
            this.mensaje});
            this.dgvErrores.Location = new System.Drawing.Point(1138, 110);
            this.dgvErrores.Name = "dgvErrores";
            this.dgvErrores.RowHeadersWidth = 82;
            this.dgvErrores.Size = new System.Drawing.Size(399, 159);
            this.dgvErrores.TabIndex = 13;
            // 
            // linea
            // 
            this.linea.FillWeight = 50.76142F;
            this.linea.HeaderText = "Línea";
            this.linea.MinimumWidth = 10;
            this.linea.Name = "linea";
            this.linea.ReadOnly = true;
            // 
            // mensaje
            // 
            this.mensaje.FillWeight = 149.2386F;
            this.mensaje.HeaderText = "Error";
            this.mensaje.MinimumWidth = 10;
            this.mensaje.Name = "mensaje";
            this.mensaje.ReadOnly = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(1218, 302);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(242, 31);
            this.label4.TabIndex = 14;
            this.label4.Text = "Tabla De Símbolos";
            // 
            // dgvSimbolos
            // 
            this.dgvSimbolos.AllowUserToAddRows = false;
            this.dgvSimbolos.AllowUserToDeleteRows = false;
            this.dgvSimbolos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSimbolos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.identificador,
            this.nombre,
            this.tipoDato,
            this.valor,
            this.ambitoSimbolo,
            this.claseSimbolo,
            this.lineaDeclaracion,
            this.regionSimbolo,
            this.tamanoBytes,
            this.desplazamientoBytes});
            this.dgvSimbolos.Location = new System.Drawing.Point(1123, 347);
            this.dgvSimbolos.Name = "dgvSimbolos";
            this.dgvSimbolos.ReadOnly = true;
            this.dgvSimbolos.RowHeadersWidth = 40;
            this.dgvSimbolos.Size = new System.Drawing.Size(439, 267);
            this.dgvSimbolos.TabIndex = 15;
            // 
            // identificador
            // 
            this.identificador.FillWeight = 50F;
            this.identificador.HeaderText = "Identificador";
            this.identificador.MinimumWidth = 10;
            this.identificador.Name = "identificador";
            this.identificador.ReadOnly = true;
            this.identificador.Width = 80;
            // 
            // nombre
            // 
            this.nombre.FillWeight = 99.49239F;
            this.nombre.HeaderText = "Nombre";
            this.nombre.MinimumWidth = 10;
            this.nombre.Name = "nombre";
            this.nombre.ReadOnly = true;
            this.nombre.Width = 105;
            // 
            // tipoDato
            // 
            this.tipoDato.FillWeight = 99.49239F;
            this.tipoDato.HeaderText = "Tipo De Dato";
            this.tipoDato.MinimumWidth = 10;
            this.tipoDato.Name = "tipoDato";
            this.tipoDato.ReadOnly = true;
            this.tipoDato.Width = 96;
            // 
            // valor
            // 
            this.valor.FillWeight = 99.49239F;
            this.valor.HeaderText = "Valor";
            this.valor.MinimumWidth = 10;
            this.valor.Name = "valor";
            this.valor.ReadOnly = true;
            this.valor.Width = 145;
            // 
            // ambitoSimbolo
            // 
            this.ambitoSimbolo.HeaderText = "Ámbito";
            this.ambitoSimbolo.MinimumWidth = 10;
            this.ambitoSimbolo.Name = "ambitoSimbolo";
            this.ambitoSimbolo.ReadOnly = true;
            this.ambitoSimbolo.Width = 105;
            // 
            // claseSimbolo
            // 
            this.claseSimbolo.HeaderText = "Clase";
            this.claseSimbolo.MinimumWidth = 10;
            this.claseSimbolo.Name = "claseSimbolo";
            this.claseSimbolo.ReadOnly = true;
            this.claseSimbolo.Width = 90;
            // 
            // lineaDeclaracion
            // 
            this.lineaDeclaracion.HeaderText = "Línea decl.";
            this.lineaDeclaracion.MinimumWidth = 10;
            this.lineaDeclaracion.Name = "lineaDeclaracion";
            this.lineaDeclaracion.ReadOnly = true;
            this.lineaDeclaracion.Width = 90;
            // 
            // regionSimbolo
            // 
            this.regionSimbolo.HeaderText = "Región";
            this.regionSimbolo.MinimumWidth = 10;
            this.regionSimbolo.Name = "regionSimbolo";
            this.regionSimbolo.ReadOnly = true;
            this.regionSimbolo.Width = 145;
            // 
            // tamanoBytes
            // 
            this.tamanoBytes.HeaderText = "Tamaño (bytes)";
            this.tamanoBytes.MinimumWidth = 10;
            this.tamanoBytes.Name = "tamanoBytes";
            this.tamanoBytes.ReadOnly = true;
            this.tamanoBytes.Width = 110;
            // 
            // desplazamientoBytes
            // 
            this.desplazamientoBytes.HeaderText = "Desplazamiento (bytes)";
            this.desplazamientoBytes.MinimumWidth = 10;
            this.desplazamientoBytes.Name = "desplazamientoBytes";
            this.desplazamientoBytes.ReadOnly = true;
            this.desplazamientoBytes.Width = 155;
            // 
            // pictureBox1
            // 
            this.pictureBox1.ImageLocation = "C:\\DEV\\LyA\\SharpC.png";
            this.pictureBox1.Location = new System.Drawing.Point(7, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(158, 893);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Versión 3.0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Cambria", 9F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(12, 870);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 14);
            this.label6.TabIndex = 18;
            this.label6.Text = "Desarrolladores";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Cambria", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(13, 894);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(113, 24);
            this.label7.TabIndex = 19;
            this.label7.Text = "Raúl Adriell Zavala Lira\r\nGael Onofre García";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // dgvFunciones
            // 
            this.dgvFunciones.AllowUserToAddRows = false;
            this.dgvFunciones.AllowUserToDeleteRows = false;
            this.dgvFunciones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFunciones.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNombre,
            this.colTipoRetorno,
            this.colParametros,
            this.colCuerpo,
            this.colRegionFuncion,
            this.colTamanoMarco});
            this.dgvFunciones.Location = new System.Drawing.Point(1050, 708);
            this.dgvFunciones.Name = "dgvFunciones";
            this.dgvFunciones.ReadOnly = true;
            this.dgvFunciones.RowHeadersWidth = 40;
            this.dgvFunciones.Size = new System.Drawing.Size(487, 176);
            this.dgvFunciones.TabIndex = 21;
            this.dgvFunciones.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFunciones_CellContentClick);
            // 
            // colNombre
            // 
            this.colNombre.HeaderText = "Nombre";
            this.colNombre.MinimumWidth = 10;
            this.colNombre.Name = "colNombre";
            this.colNombre.ReadOnly = true;
            this.colNombre.Width = 200;
            // 
            // colTipoRetorno
            // 
            this.colTipoRetorno.HeaderText = "Tipo Retorno";
            this.colTipoRetorno.MinimumWidth = 10;
            this.colTipoRetorno.Name = "colTipoRetorno";
            this.colTipoRetorno.ReadOnly = true;
            this.colTipoRetorno.Width = 95;
            // 
            // colParametros
            // 
            this.colParametros.HeaderText = "Parámetros";
            this.colParametros.MinimumWidth = 10;
            this.colParametros.Name = "colParametros";
            this.colParametros.ReadOnly = true;
            this.colParametros.Width = 155;
            // 
            // colCuerpo
            // 
            this.colCuerpo.HeaderText = "Cuerpo";
            this.colCuerpo.MinimumWidth = 10;
            this.colCuerpo.Name = "colCuerpo";
            this.colCuerpo.ReadOnly = true;
            this.colCuerpo.Width = 95;
            // 
            // colRegionFuncion
            // 
            this.colRegionFuncion.HeaderText = "Región";
            this.colRegionFuncion.MinimumWidth = 10;
            this.colRegionFuncion.Name = "colRegionFuncion";
            this.colRegionFuncion.ReadOnly = true;
            this.colRegionFuncion.Width = 145;
            // 
            // colTamanoMarco
            // 
            this.colTamanoMarco.HeaderText = "Marco (bytes)";
            this.colTamanoMarco.MinimumWidth = 10;
            this.colTamanoMarco.Name = "colTamanoMarco";
            this.colTamanoMarco.ReadOnly = true;
            this.colTamanoMarco.Width = 110;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(1203, 652);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(257, 31);
            this.label8.TabIndex = 22;
            this.label8.Text = "Tabla De Funciones";
            // 
            // dgvErroresSintaxis
            // 
            this.dgvErroresSintaxis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErroresSintaxis.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.numLinea,
            this.errorSintaxis});
            this.dgvErroresSintaxis.Location = new System.Drawing.Point(28, 694);
            this.dgvErroresSintaxis.Name = "dgvErroresSintaxis";
            this.dgvErroresSintaxis.RowHeadersWidth = 82;
            this.dgvErroresSintaxis.Size = new System.Drawing.Size(879, 150);
            this.dgvErroresSintaxis.TabIndex = 23;
            // 
            // numLinea
            // 
            this.numLinea.HeaderText = "Linea";
            this.numLinea.MinimumWidth = 50;
            this.numLinea.Name = "numLinea";
            this.numLinea.Width = 50;
            // 
            // errorSintaxis
            // 
            this.errorSintaxis.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.errorSintaxis.HeaderText = "Error";
            this.errorSintaxis.MinimumWidth = 10;
            this.errorSintaxis.Name = "errorSintaxis";
            // 
            // tabResultados
            // 
            this.tabResultados.Controls.Add(this.tabAnalisis);
            this.tabResultados.Controls.Add(this.tabCodigoIntermedio);
            this.tabResultados.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabResultados.Location = new System.Drawing.Point(0, 0);
            this.tabResultados.Name = "tabResultados";
            this.tabResultados.SelectedIndex = 0;
            this.tabResultados.Size = new System.Drawing.Size(1574, 926);
            this.tabResultados.TabIndex = 0;
            // 
            // tabAnalisis
            // 
            this.tabAnalisis.AutoScroll = true;
            this.tabAnalisis.AutoScrollMinSize = new System.Drawing.Size(1574, 926);
            this.tabAnalisis.BackColor = System.Drawing.Color.White;
            this.tabAnalisis.Controls.Add(this.dgvErroresSintaxis);
            this.tabAnalisis.Controls.Add(this.label8);
            this.tabAnalisis.Controls.Add(this.dgvFunciones);
            this.tabAnalisis.Controls.Add(this.label7);
            this.tabAnalisis.Controls.Add(this.label6);
            this.tabAnalisis.Controls.Add(this.label5);
            this.tabAnalisis.Controls.Add(this.pictureBox1);
            this.tabAnalisis.Controls.Add(this.dgvSimbolos);
            this.tabAnalisis.Controls.Add(this.label4);
            this.tabAnalisis.Controls.Add(this.dgvErrores);
            this.tabAnalisis.Controls.Add(this.lblTotalErrores);
            this.tabAnalisis.Controls.Add(this.scintilla2);
            this.tabAnalisis.Controls.Add(this.label3);
            this.tabAnalisis.Controls.Add(this.label2);
            this.tabAnalisis.Controls.Add(this.btnGuardarArchivoTokens);
            this.tabAnalisis.Controls.Add(this.btnGuardarPrograma);
            this.tabAnalisis.Controls.Add(this.label1);
            this.tabAnalisis.Controls.Add(this.btnEjecutar);
            this.tabAnalisis.Controls.Add(this.btnCargarPrograma);
            this.tabAnalisis.Controls.Add(this.scintilla1);
            this.tabAnalisis.Location = new System.Drawing.Point(4, 21);
            this.tabAnalisis.Name = "tabAnalisis";
            this.tabAnalisis.Size = new System.Drawing.Size(1566, 901);
            this.tabAnalisis.TabIndex = 0;
            this.tabAnalisis.Text = "Análisis";
            // 
            // tabCodigoIntermedio
            // 
            this.tabCodigoIntermedio.Controls.Add(this.tabVistasIntermedio);
            this.tabCodigoIntermedio.Controls.Add(this.dgvErroresGeneracion);
            this.tabCodigoIntermedio.Controls.Add(this.panelCodigoIntermedio);
            this.tabCodigoIntermedio.Location = new System.Drawing.Point(4, 21);
            this.tabCodigoIntermedio.Name = "tabCodigoIntermedio";
            this.tabCodigoIntermedio.Padding = new System.Windows.Forms.Padding(12);
            this.tabCodigoIntermedio.Size = new System.Drawing.Size(1566, 901);
            this.tabCodigoIntermedio.TabIndex = 1;
            this.tabCodigoIntermedio.Text = "Código intermedio";
            this.tabCodigoIntermedio.UseVisualStyleBackColor = true;
            //
            // Vistas del mismo código intermedio
            //
            this.tabVistasIntermedio.Controls.Add(this.tabVistaCodigo);
            this.tabVistasIntermedio.Controls.Add(this.tabVistaCuadruplos);
            this.tabVistasIntermedio.Controls.Add(this.tabVistaTriplos);
            this.tabVistasIntermedio.Controls.Add(this.tabVistaCodigoP);
            this.tabVistasIntermedio.Controls.Add(this.tabVistaNotaciones);
            this.tabVistasIntermedio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabVistasIntermedio.Name = "tabVistasIntermedio";
            this.tabVistasIntermedio.SelectedIndex = 0;
            this.tabVistaCodigo.Controls.Add(this.txtCodigoIntermedio);
            this.tabVistaCodigo.Name = "tabVistaCodigo";
            this.tabVistaCodigo.Text = "Código de tres direcciones";
            this.tabVistaCodigo.UseVisualStyleBackColor = true;
            this.tabVistaCuadruplos.Controls.Add(this.dgvCuadruplos);
            this.tabVistaCuadruplos.Name = "tabVistaCuadruplos";
            this.tabVistaCuadruplos.Text = "Cuádruplos";
            this.tabVistaCuadruplos.UseVisualStyleBackColor = true;
            this.txtCodigoIntermedio.BackColor = System.Drawing.Color.White;
            this.txtCodigoIntermedio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCodigoIntermedio.Font = new System.Drawing.Font("Consolas", 11F);
            this.txtCodigoIntermedio.Multiline = true;
            this.txtCodigoIntermedio.Name = "txtCodigoIntermedio";
            this.txtCodigoIntermedio.ReadOnly = true;
            this.txtCodigoIntermedio.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCodigoIntermedio.WordWrap = false;
            //
            // Triplos, Código P y las tres notaciones de cada expresión fuente
            //
            this.tabVistaTriplos.Controls.Add(this.dgvTriplos);
            this.tabVistaTriplos.Name = "tabVistaTriplos";
            this.tabVistaTriplos.Text = "Triplos";
            this.tabVistaTriplos.UseVisualStyleBackColor = true;
            this.dgvTriplos.Name = "dgvTriplos";
            this.dgvTriplos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTriplos.AllowUserToAddRows = false;
            this.dgvTriplos.AllowUserToDeleteRows = false;
            this.dgvTriplos.ReadOnly = true;
            this.dgvTriplos.RowHeadersVisible = false;
            this.dgvTriplos.BackgroundColor = System.Drawing.Color.White;
            this.dgvTriplos.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.dgvTriplos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTriplos.Columns.Add("objetoTriplo", "Dato objeto");
            this.dgvTriplos.Columns.Add("fuenteTriplo", "Dato fuente");
            this.dgvTriplos.Columns.Add("operadorTriplo", "Operador");
            this.tabVistaCodigoP.Controls.Add(this.txtCodigoP);
            this.tabVistaCodigoP.Name = "tabVistaCodigoP";
            this.tabVistaCodigoP.Text = "Código P";
            this.tabVistaCodigoP.UseVisualStyleBackColor = true;
            this.txtCodigoP.Name = "txtCodigoP";
            this.txtCodigoP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCodigoP.BackColor = System.Drawing.Color.White;
            this.txtCodigoP.Font = new System.Drawing.Font("Consolas", 11F);
            this.txtCodigoP.Multiline = true;
            this.txtCodigoP.ReadOnly = true;
            this.txtCodigoP.WordWrap = false;
            this.txtCodigoP.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tabVistaNotaciones.Controls.Add(this.dgvNotaciones);
            this.tabVistaNotaciones.Name = "tabVistaNotaciones";
            this.tabVistaNotaciones.Text = "Notaciones";
            this.tabVistaNotaciones.UseVisualStyleBackColor = true;
            this.dgvNotaciones.Name = "dgvNotaciones";
            this.dgvNotaciones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNotaciones.AllowUserToAddRows = false;
            this.dgvNotaciones.AllowUserToDeleteRows = false;
            this.dgvNotaciones.ReadOnly = true;
            this.dgvNotaciones.RowHeadersVisible = false;
            this.dgvNotaciones.BackgroundColor = System.Drawing.Color.White;
            this.dgvNotaciones.Font = new System.Drawing.Font("Consolas", 10F);
            this.dgvNotaciones.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNotaciones.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvNotaciones.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvNotaciones.Columns.Add("contextoNotacion", "Línea / contexto");
            this.dgvNotaciones.Columns.Add("infija", "Infija");
            this.dgvNotaciones.Columns.Add("prefija", "Prefija");
            this.dgvNotaciones.Columns.Add("postfija", "Postfija");
            // 
            // dgvCuadruplos
            // 
            this.dgvCuadruplos.AllowUserToAddRows = false;
            this.dgvCuadruplos.AllowUserToDeleteRows = false;
            this.dgvCuadruplos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCuadruplos.BackgroundColor = System.Drawing.Color.White;
            this.dgvCuadruplos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCuadruplos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.datoObjeto,
            this.datoFuente1,
            this.datoFuente2,
            this.operacionIntermedia});
            this.dgvCuadruplos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCuadruplos.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.dgvCuadruplos.Location = new System.Drawing.Point(12, 97);
            this.dgvCuadruplos.Name = "dgvCuadruplos";
            this.dgvCuadruplos.ReadOnly = true;
            this.dgvCuadruplos.RowHeadersVisible = false;
            this.dgvCuadruplos.RowHeadersWidth = 82;
            this.dgvCuadruplos.Size = new System.Drawing.Size(1542, 642);
            this.dgvCuadruplos.TabIndex = 0;
            // 
            // datoObjeto
            // 
            this.datoObjeto.HeaderText = "Dato objeto";
            this.datoObjeto.MinimumWidth = 10;
            this.datoObjeto.Name = "datoObjeto";
            this.datoObjeto.ReadOnly = true;
            this.datoObjeto.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // datoFuente1
            // 
            this.datoFuente1.HeaderText = "Dato fuente 1";
            this.datoFuente1.MinimumWidth = 10;
            this.datoFuente1.Name = "datoFuente1";
            this.datoFuente1.ReadOnly = true;
            this.datoFuente1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // datoFuente2
            // 
            this.datoFuente2.HeaderText = "Dato fuente 2";
            this.datoFuente2.MinimumWidth = 10;
            this.datoFuente2.Name = "datoFuente2";
            this.datoFuente2.ReadOnly = true;
            this.datoFuente2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // operacionIntermedia
            // 
            this.operacionIntermedia.HeaderText = "Operación";
            this.operacionIntermedia.MinimumWidth = 10;
            this.operacionIntermedia.Name = "operacionIntermedia";
            this.operacionIntermedia.ReadOnly = true;
            this.operacionIntermedia.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgvErroresGeneracion
            // 
            this.dgvErroresGeneracion.AllowUserToAddRows = false;
            this.dgvErroresGeneracion.AllowUserToDeleteRows = false;
            this.dgvErroresGeneracion.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvErroresGeneracion.BackgroundColor = System.Drawing.Color.White;
            this.dgvErroresGeneracion.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErroresGeneracion.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.lineaGeneracion,
            this.mensajeGeneracion});
            this.dgvErroresGeneracion.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvErroresGeneracion.Location = new System.Drawing.Point(12, 739);
            this.dgvErroresGeneracion.Name = "dgvErroresGeneracion";
            this.dgvErroresGeneracion.ReadOnly = true;
            this.dgvErroresGeneracion.RowHeadersVisible = false;
            this.dgvErroresGeneracion.RowHeadersWidth = 82;
            this.dgvErroresGeneracion.Size = new System.Drawing.Size(1542, 150);
            this.dgvErroresGeneracion.TabIndex = 1;
            this.dgvErroresGeneracion.Visible = false;
            // 
            // lineaGeneracion
            // 
            this.lineaGeneracion.FillWeight = 15F;
            this.lineaGeneracion.HeaderText = "Línea";
            this.lineaGeneracion.MinimumWidth = 10;
            this.lineaGeneracion.Name = "lineaGeneracion";
            this.lineaGeneracion.ReadOnly = true;
            // 
            // mensajeGeneracion
            // 
            this.mensajeGeneracion.FillWeight = 85F;
            this.mensajeGeneracion.HeaderText = "Diagnóstico de generación";
            this.mensajeGeneracion.MinimumWidth = 10;
            this.mensajeGeneracion.Name = "mensajeGeneracion";
            this.mensajeGeneracion.ReadOnly = true;
            // 
            // panelCodigoIntermedio
            // 
            this.panelCodigoIntermedio.Controls.Add(this.btnGenerarCuadruplos);
            this.panelCodigoIntermedio.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCodigoIntermedio.Location = new System.Drawing.Point(12, 12);
            this.panelCodigoIntermedio.Name = "panelCodigoIntermedio";
            this.panelCodigoIntermedio.Size = new System.Drawing.Size(1542, 85);
            this.panelCodigoIntermedio.TabIndex = 2;
            // 
            // btnGenerarCuadruplos
            // 
            this.btnGenerarCuadruplos.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnGenerarCuadruplos.Location = new System.Drawing.Point(1397, 0);
            this.btnGenerarCuadruplos.Name = "btnGenerarCuadruplos";
            this.btnGenerarCuadruplos.Size = new System.Drawing.Size(145, 85);
            this.btnGenerarCuadruplos.TabIndex = 1;
            this.btnGenerarCuadruplos.Text = "Analizar y generar";
            this.btnGenerarCuadruplos.UseVisualStyleBackColor = true;
            this.btnGenerarCuadruplos.Click += new System.EventHandler(this.btnGenerarCuadruplos_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1574, 926);
            this.Controls.Add(this.tabResultados);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Form1";
            this.Text = "Análizador SharpC";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrores)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSimbolos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFunciones)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErroresSintaxis)).EndInit();
            this.tabResultados.ResumeLayout(false);
            this.tabAnalisis.ResumeLayout(false);
            this.tabAnalisis.PerformLayout();
            this.tabCodigoIntermedio.ResumeLayout(false);
            this.tabVistasIntermedio.ResumeLayout(false);
            this.tabVistaCodigo.ResumeLayout(false);
            this.tabVistaCodigo.PerformLayout();
            this.tabVistaCuadruplos.ResumeLayout(false);
            this.tabVistaTriplos.ResumeLayout(false);
            this.tabVistaCodigoP.ResumeLayout(false);
            this.tabVistaCodigoP.PerformLayout();
            this.tabVistaNotaciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriplos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNotaciones)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCuadruplos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErroresGeneracion)).EndInit();
            this.panelCodigoIntermedio.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private ScintillaNET.Scintilla scintilla1;
        private System.Windows.Forms.Button btnCargarPrograma;
        private System.Windows.Forms.Button btnEjecutar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGuardarPrograma;
        private System.Windows.Forms.Button btnGuardarArchivoTokens;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private ScintillaNET.Scintilla scintilla2;
        private System.Windows.Forms.Label lblTotalErrores;
        private System.Windows.Forms.DataGridView dgvErrores;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dgvSimbolos;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridViewTextBoxColumn identificador;
        private System.Windows.Forms.DataGridViewTextBoxColumn nombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn tipoDato;
        private System.Windows.Forms.DataGridViewTextBoxColumn valor;
        private System.Windows.Forms.DataGridViewTextBoxColumn ambitoSimbolo;
        private System.Windows.Forms.DataGridViewTextBoxColumn claseSimbolo;
        private System.Windows.Forms.DataGridViewTextBoxColumn lineaDeclaracion;
        private System.Windows.Forms.DataGridViewTextBoxColumn regionSimbolo;
        private System.Windows.Forms.DataGridViewTextBoxColumn tamanoBytes;
        private System.Windows.Forms.DataGridViewTextBoxColumn desplazamientoBytes;
        private System.Windows.Forms.DataGridView dgvFunciones;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTipoRetorno;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParametros;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCuerpo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRegionFuncion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTamanoMarco;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridViewTextBoxColumn linea;
        private System.Windows.Forms.DataGridViewTextBoxColumn mensaje;
        private System.Windows.Forms.DataGridView dgvErroresSintaxis;
        private System.Windows.Forms.DataGridViewTextBoxColumn numLinea;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorSintaxis;
        private System.Windows.Forms.TabControl tabResultados;
        private System.Windows.Forms.TabPage tabAnalisis;
        private System.Windows.Forms.TabPage tabCodigoIntermedio;
        private System.Windows.Forms.TabControl tabVistasIntermedio;
        private System.Windows.Forms.TabPage tabVistaCodigo;
        private System.Windows.Forms.TabPage tabVistaCuadruplos;
        private System.Windows.Forms.TabPage tabVistaTriplos;
        private System.Windows.Forms.TabPage tabVistaCodigoP;
        private System.Windows.Forms.TabPage tabVistaNotaciones;
        private System.Windows.Forms.DataGridView dgvTriplos;
        private System.Windows.Forms.TextBox txtCodigoP;
        private System.Windows.Forms.DataGridView dgvNotaciones;
        private System.Windows.Forms.TextBox txtCodigoIntermedio;
        private System.Windows.Forms.Panel panelCodigoIntermedio;
        private System.Windows.Forms.Button btnGenerarCuadruplos;
        private System.Windows.Forms.DataGridView dgvCuadruplos;
        private System.Windows.Forms.DataGridViewTextBoxColumn datoObjeto;
        private System.Windows.Forms.DataGridViewTextBoxColumn datoFuente1;
        private System.Windows.Forms.DataGridViewTextBoxColumn datoFuente2;
        private System.Windows.Forms.DataGridViewTextBoxColumn operacionIntermedia;
        private System.Windows.Forms.DataGridView dgvErroresGeneracion;
        private System.Windows.Forms.DataGridViewTextBoxColumn lineaGeneracion;
        private System.Windows.Forms.DataGridViewTextBoxColumn mensajeGeneracion;
    }
}

