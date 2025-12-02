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
            FlowLayoutPanel panelWithStrategies;
            buttonUpdateData = new Button();
            buttonGotoRecipeGeneration = new Button();
            buttonGotoRecipeModification = new Button();
            buttonGotoItemCreation = new Button();
            searchControlItems = new ItemSearchControl();
            searchControlFluids = new ItemSearchControl();
            bindingSource1 = new BindingSource(components);
            groupBoxWithStrategiesOnControl = new GroupBox();
            panelWithStrategies = new FlowLayoutPanel();
            panelWithStrategies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            SuspendLayout();
            // 
            // panelWithStrategies
            // 
            panelWithStrategies.AutoScroll = true;
            panelWithStrategies.Controls.Add(buttonUpdateData);
            panelWithStrategies.Controls.Add(buttonGotoRecipeGeneration);
            panelWithStrategies.Controls.Add(buttonGotoRecipeModification);
            panelWithStrategies.Controls.Add(buttonGotoItemCreation);
            panelWithStrategies.Controls.Add(searchControlItems);
            panelWithStrategies.Controls.Add(searchControlFluids);
            panelWithStrategies.Dock = DockStyle.Top;
            panelWithStrategies.Location = new Point(0, 0);
            panelWithStrategies.Name = "panelWithStrategies";
            panelWithStrategies.Padding = new Padding(1);
            panelWithStrategies.Size = new Size(722, 46);
            panelWithStrategies.TabIndex = 6;
            panelWithStrategies.WrapContents = false;
            // 
            // buttonUpdateData
            // 
            buttonUpdateData.AutoSize = true;
            buttonUpdateData.Location = new Point(4, 4);
            buttonUpdateData.Name = "buttonUpdateData";
            buttonUpdateData.Size = new Size(81, 25);
            buttonUpdateData.TabIndex = 2;
            buttonUpdateData.Text = "ОБНОВИТЬ";
            buttonUpdateData.UseVisualStyleBackColor = true;
            buttonUpdateData.Click += buttonUpdateData_Click;
            // 
            // buttonGotoRecipeGeneration
            // 
            buttonGotoRecipeGeneration.AutoSize = true;
            buttonGotoRecipeGeneration.Location = new Point(91, 4);
            buttonGotoRecipeGeneration.Name = "buttonGotoRecipeGeneration";
            buttonGotoRecipeGeneration.Size = new Size(101, 25);
            buttonGotoRecipeGeneration.TabIndex = 0;
            buttonGotoRecipeGeneration.Text = "Создать рецепт";
            buttonGotoRecipeGeneration.TextImageRelation = TextImageRelation.ImageAboveText;
            buttonGotoRecipeGeneration.UseVisualStyleBackColor = true;
            buttonGotoRecipeGeneration.Click += buttonGotoRecipeGeneration_Click;
            // 
            // buttonGotoRecipeModification
            // 
            buttonGotoRecipeModification.AutoSize = true;
            buttonGotoRecipeModification.Location = new Point(198, 4);
            buttonGotoRecipeModification.Name = "buttonGotoRecipeModification";
            buttonGotoRecipeModification.Size = new Size(112, 25);
            buttonGotoRecipeModification.TabIndex = 1;
            buttonGotoRecipeModification.Text = "Изменить рецепт";
            buttonGotoRecipeModification.TextImageRelation = TextImageRelation.ImageAboveText;
            buttonGotoRecipeModification.UseVisualStyleBackColor = true;
            buttonGotoRecipeModification.Click += buttonGotoRecipeModification_Click;
            // 
            // buttonGotoItemCreation
            // 
            buttonGotoItemCreation.AutoSize = true;
            buttonGotoItemCreation.Location = new Point(316, 4);
            buttonGotoItemCreation.Name = "buttonGotoItemCreation";
            buttonGotoItemCreation.Size = new Size(112, 25);
            buttonGotoItemCreation.TabIndex = 2;
            buttonGotoItemCreation.Text = "Создать предмет";
            buttonGotoItemCreation.TextImageRelation = TextImageRelation.ImageAboveText;
            buttonGotoItemCreation.UseVisualStyleBackColor = true;
            // 
            // searchControlItems
            // 
            searchControlItems.Location = new Point(434, 4);
            searchControlItems.Name = "searchControlItems";
            searchControlItems.Size = new Size(222, 26);
            searchControlItems.TabIndex = 3;
            searchControlItems.TabStop = false;
            // 
            // searchControlFluids
            // 
            searchControlFluids.Location = new Point(662, 4);
            searchControlFluids.Name = "searchControlFluids";
            searchControlFluids.Size = new Size(338, 26);
            searchControlFluids.TabIndex = 4;
            searchControlFluids.TabStop = false;
            // 
            // groupBoxWithStrategiesOnControl
            // 
            groupBoxWithStrategiesOnControl.AutoSize = true;
            groupBoxWithStrategiesOnControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBoxWithStrategiesOnControl.Dock = DockStyle.Fill;
            groupBoxWithStrategiesOnControl.Location = new Point(0, 46);
            groupBoxWithStrategiesOnControl.Margin = new Padding(10);
            groupBoxWithStrategiesOnControl.Name = "groupBoxWithStrategiesOnControl";
            groupBoxWithStrategiesOnControl.Padding = new Padding(10);
            groupBoxWithStrategiesOnControl.Size = new Size(722, 371);
            groupBoxWithStrategiesOnControl.TabIndex = 9;
            groupBoxWithStrategiesOnControl.TabStop = false;
            groupBoxWithStrategiesOnControl.Text = "CodeGenerationStrategy";
            // 
            // MyForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(722, 417);
            Controls.Add(groupBoxWithStrategiesOnControl);
            Controls.Add(panelWithStrategies);
            Name = "MyForm";
            Text = "Form1";
            Load += MyForm_Load;
            panelWithStrategies.ResumeLayout(false);
            panelWithStrategies.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonUpdateData;
        private BindingSource bindingSource1;
        private ItemSearchControl searchControlItems;
        private ItemSearchControl searchControlFluids;
        private Button buttonGotoRecipeGeneration;
        private Button buttonGotoRecipeModification;
        private Button buttonGotoItemCreation;
        private GroupBox groupBoxWithStrategiesOnControl;
    }
}
