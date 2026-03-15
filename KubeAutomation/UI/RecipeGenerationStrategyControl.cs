using KubeAutomation.GenerationStrategies.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KubeAutomation.UI
{
    public partial class RecipeGenerationStrategyControl : UserControl
    {
        private ItemSearchControl[] ItemsIn = [];
        private ItemSearchControl[] ItemsOut = [];
        private ItemSearchControl[] FluidsIn = [];
        private ItemSearchControl[] FluidsOut = [];

        int? choosenMachine;

        public RecipeGenerationStrategyControl()
        {
            InitializeComponent();

            textBoxMachineType_Init();

            //MyForm.GetInstance().ItemsAndFluidsUpdated += itemSearchControl1.UpdateItemsAndImages;

            MyForm.GetInstance().AddToUpdateQuery(itemSearchControlTest, eStoringType.fluids);
            MyForm.GetInstance().AddToUpdateQuery(itemSearchControl1, eStoringType.items);
            MyForm.GetInstance().AddToUpdateQuery(itemSearchControl2, eStoringType.items);
            MyForm.GetInstance().AddToUpdateQuery(itemSearchControl3, eStoringType.fluids);
            MyForm.GetInstance().AddToUpdateQuery(itemSearchControl4, eStoringType.items);
            MyForm.GetInstance().AddToUpdateQuery(itemSearchControl5, eStoringType.fluids);
            MyForm.GetInstance().AddToUpdateQuery(itemSearchControl6, eStoringType.items);

            MyForm.GetInstance().UpdateData();
        }

        private void textBoxMachineType_TextChanged(object sender, EventArgs e)
        {
            var list = GregTechRecipeCodeGenerator.Machines;

            for (int i = 0; i < list.Count; i++)
            {
                var machine = list[i];

                if (textBoxMachineType.Text == machine.Name)
                {
                    textBoxMachineType_ApplyChoose(i);
                    break;
                }
            }

        }

        private void textBoxMachineType_Init()
        {
            textBoxMachineType.AutoCompleteMode = AutoCompleteMode.Suggest;
            textBoxMachineType.AutoCompleteSource = AutoCompleteSource.CustomSource;
            var list = GregTechRecipeCodeGenerator.Machines;
            var suggestContent = list.Select(machine => machine.Name).ToArray();
            textBoxMachineType.AutoCompleteCustomSource = [..suggestContent];
        }

        /// <summary>
        /// Динамически создает элементы для ввода данных
        /// </summary>
        private void textBoxMachineType_ApplyChoose(int n)
        {
            choosenMachine = n;
            var list = GregTechRecipeCodeGenerator.Machines;

            foreach (Control el in panelWithElements.Controls)
            {
                el.Dispose();
            }

            Array.Clear(ItemsIn);
            Array.Clear(ItemsOut);
            Array.Clear(FluidsIn);
            Array.Clear(FluidsOut);

            // добавляем входные предметы
            Label labelItemIn = new()
            {
                Text = "Входные предметы"
            };

            panelWithElements.Controls.Add(labelItemIn);

            for (int i = 0; i < list[n].ItemCount; i++)
            {
                //ItemSearchControl itemControl = new();
                

                //MyForm.GetInstance().ItemsAndFluidsUpdated += itemControl.UpdateItemsAndImages;
            }
        }
    }
}
