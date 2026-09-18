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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dgvFunciones = new System.Windows.Forms.DataGridView();
            this.colNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTipoRetorno = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParametros = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCuerpo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvErroresSintaxis = new System.Windows.Forms.DataGridView();
            this.numLinea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.errorSintaxis = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrores)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSimbolos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFunciones)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErroresSintaxis)).BeginInit();
            this.SuspendLayout();
            // 
            // scintilla1
            // 
            this.scintilla1.Location = new System.Drawing.Point(127, 109);
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
            this.label1.Location = new System.Drawing.Point(223, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(446, 63);
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
            this.btnGuardarArchivoTokens.Location = new System.Drawing.Point(1050, 68);
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
            this.label2.Location = new System.Drawing.Point(732, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(473, 63);
            this.label2.TabIndex = 8;
            this.label2.Text = "Archivo de Tokens";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1298, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 63);
            this.label3.TabIndex = 9;
            this.label3.Text = "Errores";
            // 
            // scintilla2
            // 
            this.scintilla2.Location = new System.Drawing.Point(620, 109);
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
            this.lblTotalErrores.Size = new System.Drawing.Size(333, 44);
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
            this.label4.Size = new System.Drawing.Size(484, 63);
            this.label4.TabIndex = 14;
            this.label4.Text = "Tabla De Símbolos";
            // 
            // dgvSimbolos
            // 
            this.dgvSimbolos.AllowUserToAddRows = false;
            this.dgvSimbolos.AllowUserToDeleteRows = false;
            this.dgvSimbolos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSimbolos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSimbolos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.identificador,
            this.nombre,
            this.tipoDato,
            this.valor});
            this.dgvSimbolos.Location = new System.Drawing.Point(1138, 346);
            this.dgvSimbolos.Name = "dgvSimbolos";
            this.dgvSimbolos.RowHeadersWidth = 82;
            this.dgvSimbolos.Size = new System.Drawing.Size(399, 267);
            this.dgvSimbolos.TabIndex = 15;
            // 
            // identificador
            // 
            this.identificador.FillWeight = 50F;
            this.identificador.HeaderText = "Identificador";
            this.identificador.MinimumWidth = 10;
            this.identificador.Name = "identificador";
            // 
            // nombre
            // 
            this.nombre.FillWeight = 99.49239F;
            this.nombre.HeaderText = "Nombre";
            this.nombre.MinimumWidth = 10;
            this.nombre.Name = "nombre";
            // 
            // tipoDato
            // 
            this.tipoDato.FillWeight = 99.49239F;
            this.tipoDato.HeaderText = "Tipo De Dato";
            this.tipoDato.MinimumWidth = 10;
            this.tipoDato.Name = "tipoDato";
            // 
            // valor
            // 
            this.valor.FillWeight = 99.49239F;
            this.valor.HeaderText = "Valor";
            this.valor.MinimumWidth = 10;
            this.valor.Name = "valor";
            // 
            // pictureBox1
            // 
            this.pictureBox1.ImageLocation = "C:\\Repositorios\\AUTOMATAS\\Gatosabelogo-new2.png";
            this.pictureBox1.Location = new System.Drawing.Point(12, 11);
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
            this.label5.Location = new System.Drawing.Point(9, 896);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(133, 26);
            this.label5.TabIndex = 17;
            this.label5.Text = "Versión 2.1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Cambria", 9F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(9, 832);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(192, 28);
            this.label6.TabIndex = 18;
            this.label6.Text = "Desarrolladores";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Cambria", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(10, 848);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(234, 78);
            this.label7.TabIndex = 19;
            this.label7.Text = "Raúl Adriell Zavala Lira\r\nCésar Vézquez Soto\r\nGael Onofre García";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // dgvFunciones
            // 
            this.dgvFunciones.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvFunciones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFunciones.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNombre,
            this.colTipoRetorno,
            this.colParametros,
            this.colCuerpo});
            this.dgvFunciones.Location = new System.Drawing.Point(1050, 708);
            this.dgvFunciones.Name = "dgvFunciones";
            this.dgvFunciones.RowHeadersWidth = 82;
            this.dgvFunciones.Size = new System.Drawing.Size(487, 176);
            this.dgvFunciones.TabIndex = 21;
            this.dgvFunciones.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFunciones_CellContentClick);
            // 
            // colNombre
            // 
            this.colNombre.HeaderText = "Nombre";
            this.colNombre.MinimumWidth = 10;
            this.colNombre.Name = "colNombre";
            // 
            // colTipoRetorno
            // 
            this.colTipoRetorno.HeaderText = "Tipo Retorno";
            this.colTipoRetorno.MinimumWidth = 10;
            this.colTipoRetorno.Name = "colTipoRetorno";
            // 
            // colParametros
            // 
            this.colParametros.HeaderText = "Parámetros";
            this.colParametros.MinimumWidth = 10;
            this.colParametros.Name = "colParametros";
            // 
            // colCuerpo
            // 
            this.colCuerpo.HeaderText = "Cuerpo";
            this.colCuerpo.MinimumWidth = 10;
            this.colCuerpo.Name = "colCuerpo";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(1203, 652);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(510, 63);
            this.label8.TabIndex = 22;
            this.label8.Text = "Tabla De Funciones";
            // 
            // dgvErroresSintaxis
            // 
            this.dgvErroresSintaxis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErroresSintaxis.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.numLinea,
            this.errorSintaxis});
            this.dgvErroresSintaxis.Location = new System.Drawing.Point(129, 708);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1574, 926);
            this.Controls.Add(this.dgvErroresSintaxis);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.dgvFunciones);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.dgvSimbolos);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dgvErrores);
            this.Controls.Add(this.lblTotalErrores);
            this.Controls.Add(this.scintilla2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnGuardarArchivoTokens);
            this.Controls.Add(this.btnGuardarPrograma);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnEjecutar);
            this.Controls.Add(this.btnCargarPrograma);
            this.Controls.Add(this.scintilla1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Form1";
            this.Text = "Análizador Léxico";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrores)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSimbolos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFunciones)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErroresSintaxis)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.DataGridView dgvFunciones;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTipoRetorno;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParametros;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCuerpo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridViewTextBoxColumn linea;
        private System.Windows.Forms.DataGridViewTextBoxColumn mensaje;
        private System.Windows.Forms.DataGridView dgvErroresSintaxis;
        private System.Windows.Forms.DataGridViewTextBoxColumn numLinea;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorSintaxis;
    }
}

