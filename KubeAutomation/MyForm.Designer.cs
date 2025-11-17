namespace KubeAutomation
{
    partial class MyForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBoxTest = new TextBox();
            comboBox1 = new ComboBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // textBoxTest
            // 
            textBoxTest.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBoxTest.Location = new Point(191, 34);
            textBoxTest.Name = "textBoxTest";
            textBoxTest.PlaceholderText = "wdwd";
            textBoxTest.Size = new Size(254, 23);
            textBoxTest.TabIndex = 0;
            textBoxTest.TextChanged += textBox1_TextChanged;
            // 
            // comboBox1
            // 
            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(205, 110);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(223, 23);
            comboBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(257, 196);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // MyForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(653, 268);
            Controls.Add(button1);
            Controls.Add(comboBox1);
            Controls.Add(textBoxTest);
            Name = "MyForm";
            Text = "Form1";
            Load += MyForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public TextBox textBoxTest;
        private ComboBox comboBox1;
        private Button button1;
    }
}
