using KubeAutomation.UI;
using LogExtractorLibrary;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KubeAutomation
{
    public enum eStoringType
    {
        items, fluids
    }

    public partial class MyForm : Form
    {
        private static readonly MyForm singleton;

        public AutoCompleteStringCollection autoCompleteRecipes = [];

        // new type here...
        Dictionary<string, byte[]> ItemImages = [];
        Dictionary<string, byte[]> FluidImages = [];

        Dictionary<eStoringType, List<ItemSearchControl>> ToUpdateSearchControls = [];


        private readonly List<UserControl> ControlsWithCodeGenerationStrategies = [];
       

        static MyForm()
        {
            singleton = new MyForm();
        }

        private MyForm()
        {
            InitializeComponent();

            //textBoxTest.AutoCompleteMode = AutoCompleteMode.Suggest;
            //textBoxTest.AutoCompleteSource = AutoCompleteSource.CustomSource;
            //textBoxTest.AutoCompleteCustomSource = autoCompleteRecipes;

            ToUpdateSearchControls[eStoringType.items] = [];
            ToUpdateSearchControls[eStoringType.fluids] = [];

        }

        public void AddToUpdateQuery(ItemSearchControl control, eStoringType type)
        {
            ToUpdateSearchControls[type].Add(control);
        }

        public void AddToUpdateQueryAndUpdate(ItemSearchControl control, eStoringType type)
        {
            // =======================
            // РЕАЛИЗОВАТЬ НОРМ ИНТЕРФЕЙС ДЛЯ ОБНОВЛЕНИЙ
            // =======================

            //ToUpdateSearchControls[type].Add(control);

            //control.UpdateData()
        }

        public static MyForm GetInstance()
        {
            return singleton;
        }

        public void OpenStrategyUserControl(int i)
        {
            // Скрываем всё кроме одного
            foreach (var strategy in ControlsWithCodeGenerationStrategies)
            {
                strategy.Visible = false;
            }

            ControlsWithCodeGenerationStrategies[i].Visible = true;

        }

        private void InitStrategies()
        {
            // Добавляем стратегию
            ControlsWithCodeGenerationStrategies.Add(new RecipeGenerationStrategyControl());
            ControlsWithCodeGenerationStrategies.Add(new RecipeModificationStrategyControl());

            groupBoxWithStrategiesOnControl.Controls.AddRange([..ControlsWithCodeGenerationStrategies]);
            OpenStrategyUserControl(0);
        }

        private void MyForm_Load(object sender, EventArgs e)
        {
            InitStrategies();
            AddToUpdateQuery(searchControlItems, eStoringType.items);
            AddToUpdateQuery(searchControlFluids, eStoringType.fluids);

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private async void buttonUpdateData_Click(object sender, EventArgs e)
        {
            _ = Program.StartExternalAppAsync();


            ItemFluidWithIconExtractor s = new();
            await s.ExtractAsync();
        }

        public void UpdateData(Dictionary<string, byte[]> newItemImages, Dictionary<string, byte[]> newFluidImages)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<Dictionary<string, byte[]>, Dictionary<string, byte[]>>(UpdateData), newItemImages, newFluidImages);
            }
            else
            {
                ItemImages = newItemImages;
                FluidImages = newFluidImages;

                UpdateData();
            }
        }

        public void UpdateData()
        {
            foreach (var i in ToUpdateSearchControls)
            {
                switch (i.Key)
                {
                    case eStoringType.items:
                        foreach (var control in i.Value) { control?.UpdateDataFromBytes(ItemImages); }
                        break;

                    case eStoringType.fluids:
                        foreach (var control in i.Value) { control?.UpdateDataFromBytes(FluidImages); }
                        break;

                    // new types here...

                    default:
                        foreach (var control in i.Value) { control?.UpdateDataFromBytes(ItemImages); }
                        break;

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

                //textBoxTest.AutoCompleteCustomSource = autoCompleteRecipes;
            }
        }

        public void buttonGotoRecipeGeneration_Click(object sender, EventArgs e)
        {
            OpenStrategyUserControl(0);
        }

        public void buttonGotoRecipeModification_Click(object sender, EventArgs e) 
        {
            OpenStrategyUserControl(1);
        }
    }
}