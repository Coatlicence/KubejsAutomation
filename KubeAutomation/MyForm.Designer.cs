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
            components = new System.ComponentModel.Container();
            textBoxTest = new TextBox();
            button1 = new Button();
            bindingSource1 = new BindingSource(components);
            searchControl = new ItemSearchControl();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            SuspendLayout();
            // 
            // textBoxTest
            // 
            textBoxTest.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBoxTest.Location = new Point(194, 12);
            textBoxTest.Name = "textBoxTest";
            textBoxTest.PlaceholderText = "wdwd";
            textBoxTest.Size = new Size(214, 23);
            textBoxTest.TabIndex = 0;
            textBoxTest.TextChanged += textBox1_TextChanged;
            // 
            // button1
            // 
            button1.Location = new Point(271, 129);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // searchControl
            // 
            searchControl.Location = new Point(120, 63);
            searchControl.Name = "searchControl";
            searchControl.Size = new Size(338, 26);
            searchControl.TabIndex = 3;
            searchControl.TabStop = false;
            // 
            // MyForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(653, 268);
            Controls.Add(searchControl);
            Controls.Add(button1);
            Controls.Add(textBoxTest);
            Name = "MyForm";
            Text = "Form1";
            Load += MyForm_Load;
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public TextBox textBoxTest;
        private Button button1;
        private BindingSource bindingSource1;
        private ItemSearchControl searchControl;
    }
}
