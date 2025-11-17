namespace KubeAutomation
{
    public partial class MyForm : Form
    {

        public AutoCompleteStringCollection autoCompleteItems = [];

        public MyForm()
        {
            InitializeComponent();

            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            comboBox1.AutoCompleteCustomSource = autoCompleteItems;

            textBoxTest.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBoxTest.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBoxTest.AutoCompleteCustomSource = autoCompleteItems;

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

        public void UpdateAutoCompleteSource(List<string> newItems)
        {
            if (this.InvokeRequired) // Проверяем, нужен ли Invoke
            {
                // Если вызов из другого потока, используем Invoke для выполнения в потоке UI
                this.Invoke(new Action<List<string>>(UpdateAutoCompleteSource), [newItems]);
            }
            else
            {
                // Обновляем коллекцию в потоке UI
                this.autoCompleteItems.Clear(); // Очищаем старые значения
                if (newItems != null)
                {
                    this.autoCompleteItems.AddRange([.. newItems]); // Добавляем новые
                }
                // DataSource уже привязан к autoCompleteCollection, обновление коллекции сработает автоматически
                // Но если не сработает, можно явно указать:
                comboBox1.AutoCompleteCustomSource = this.autoCompleteItems; // Обычно не обязательно
                textBoxTest.AutoCompleteCustomSource = this.autoCompleteItems;
            }
        }
    }
}
