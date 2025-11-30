using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KubeAutomation
{
    public partial class MyForm : Form
    {
        public AutoCompleteStringCollection autoCompleteItems = [];
        public AutoCompleteStringCollection autoCompleteRecipes = [];

        // Предполагаем, что эти данные приходят из принимающего кода
        //static List<string> Items = new List<string>();
        //static List<string> Recipes = new List<string>();
        //static Dictionary<string, byte[]> ItemImages = new Dictionary<string, byte[]>();
        //static Dictionary<string, byte[]> FluidImages = new Dictionary<string, byte[]>();

        public MyForm()
        {
            InitializeComponent();

            textBoxTest.AutoCompleteMode = AutoCompleteMode.Suggest;
            textBoxTest.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBoxTest.AutoCompleteCustomSource = autoCompleteRecipes;
        }

        private void OnItemSelected(string item)
        {
            // Реагируем на выбор элемента (например, выводим в лог)
            Console.WriteLine($"Выбран элемент: {item}");
        }

        private void MyForm_Load(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _ = Program.StartExternalAppAsync(this);
        }

        public void UpdateItemsAndImages(List<string> newItems, List<string> newFluids, Dictionary<string, byte[]> newItemImages, Dictionary<string, byte[]> newFluidImages)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<string>, List<string>, Dictionary<string, byte[]>, Dictionary<string, byte[]>>(UpdateItemsAndImages), newItems, newFluids, newItemImages, newFluidImages);
            }
            else
            {
                // Обновляем UserControl
                searchControlItems.UpdateItemsAndImages(newItems, newFluids, newItemImages, newFluidImages);
                searchControlFluids.UpdateItemsAndImages(newItems, newFluids, newItemImages, newFluidImages);


                // Обновляем autoCompleteItems
                autoCompleteItems.Clear();
                if (newItems != null)
                {
                    autoCompleteItems.AddRange([.. newItems]);
                }
            }
        }

        public void UpdateRecipes(List<string> newRecipes)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<string>>(UpdateRecipes), [newRecipes]);
            }
            else
            {
                autoCompleteRecipes.Clear();

                if (newRecipes != null)
                {
                    autoCompleteRecipes.AddRange([.. newRecipes]);
                }

                textBoxTest.AutoCompleteCustomSource = autoCompleteRecipes;
            }
        }
    }
}