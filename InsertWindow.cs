using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Класс окна ввода информации в БД
    /// </summary>
    public partial class InsertWindow : Form
    {
        /// <summary>
        /// Текущая позиция нового добавляемого элемента в форму
        /// </summary>
        private Point _pos = new Point(35, 46);

        /// <summary>
        /// Ссылка на родительское окно
        /// </summary>
        private MainWindow _mnWd;

        /// <summary>
        /// Список подписей для названия столбцов
        /// </summary>
        private List<Label> _lables = new List<Label>();

        /// <summary>
        /// Список для элементов <see cref="TextBox"/>
        /// </summary>
        private List<TextBox> _textBoxes = new List<TextBox>();

        /// <summary>
        /// Параметр окна (изменение или добавление)
        /// <para>
        /// <see cref="InsertWindowType"/>
        /// </para>
        /// </summary>
        private readonly InsertWindowType _itype;

        /// <summary>
        /// Текущая таблица
        /// <para>
        /// <see cref="table"/>
        /// </para>
        /// </summary>
        private table _curT;

        private TextBox _prods;
        private TextBox _costs;
        private int _textboxAmountId;
        private int _prId;

        /// <summary>
        /// Конструктор окна
        /// </summary>
        /// <param name="main">Ссылка на родительское окно</param>
        /// <param name="t">Параметр ввода</param>
        public InsertWindow(MainWindow main, InsertWindowType t)
        {
            InitializeComponent();
            _mnWd = main;
            _curT = _mnWd.currentTable;
            tableName.Text = _curT.Name;
            for (int i = 0; i < _mnWd.column_names.Count; i++)
            {
                try
                {
                    if (_curT.Name != "applications")
                    {
                        drawControls(i);
                    }
                    else
                    {
                        drawOrdersControls(i);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("Please select the table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            _itype = t;

            //передвигаем кнопку ОК
            _pos.Offset(0, 5);
            _pos.X = 234;
            button1.Location = _pos;
        }

        private void selectDropDownItem(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Создание окна для ввода информации
        /// </summary>
        /// <param name="i">Порядковый номер столбца</param>
        private void drawControls(int i)
        {
            //отображаем название атрибута
            _lables.Add(new Label() { Location = _pos, Text = _mnWd.column_names[i], AutoSize = true });
            this.Controls.Add(_lables[i]);
            _lables[i].Show();
            _pos.Offset(0, 29);

            //отображаем поле ввода
            _textBoxes.Add(new TextBox() { Location = _pos, Width = 512 });
            this.Controls.Add(_textBoxes[i]);
            _textBoxes[i].Show();
            _pos.Offset(0, 48);

        }

        
        /// <summary>
        /// Создание окна для ввода информации с параметрическими запросами
        /// </summary>
        /// <param name="i">порядковый номер столбца</param>
        private void drawOrdersControls(int i)
        {
            ComboBox companiesBox = new ComboBox();
            ComboBox itemsBox = new ComboBox();

            _lables.Add(new Label() { Location = _pos, Text = _mnWd.column_names[i], AutoSize = true });
            this.Controls.Add(_lables[i]);
            _lables[i].Show();
            _pos.Offset(0, 29);

            if(_mnWd.column_names[i] == "Требуемое количество")
            {
                _textboxAmountId = i;
            }

            if (_mnWd.column_names[i] == "Товар")
            {
                var itemsList = MainWindow.getSingleColumn("products", "`Название`");

                _textBoxes.Add(new TextBox() { Visible = false });
                this.Controls.Add(_textBoxes[i]);
                _prods = _textBoxes[i];

                itemsBox.Location = _pos;
                itemsBox.Width = 256;
                itemsBox.Items.AddRange(itemsList.ToArray());
                itemsBox.SelectionChangeCommitted += new System.EventHandler(setProductsBox);
                this.Controls.Add(itemsBox);
                itemsBox.Show();
                _pos.Offset(0, 48);
            }
            else if (_mnWd.column_names[i] == "Покупатель")
            {                
                var companiesList = MainWindow.getSingleColumn("costumers", "`Организация`");

                _textBoxes.Add(new TextBox() { Visible = false });
                this.Controls.Add(_textBoxes[i]);
                _costs = _textBoxes[i];

                companiesBox.Location = _pos;
                companiesBox.Width = 256;
                companiesBox.Items.AddRange(companiesList.ToArray());
                companiesBox.SelectionChangeCommitted += new System.EventHandler(setCostumersBox);
                this.Controls.Add(companiesBox);
                companiesBox.Show();
                _pos.Offset(0, 48);

            }
            else
            {
                _textBoxes.Add(new TextBox() { Location = _pos, Width = 512 });
                this.Controls.Add(_textBoxes[i]);
                _textBoxes[_textBoxes.Count - 1].Show();
                _pos.Offset(0, 48);
            }
        }

        private void setProductsBox(object sender, EventArgs e)
        {
            var cbox = (ComboBox)sender;
            var ids = MainWindow.getSingleColumn("products", "id");
            _prId = int.Parse(ids[cbox.SelectedIndex]);
            _prods.Text = ids[cbox.SelectedIndex];
        }

        private void setCostumersBox(object sender, EventArgs e)
        {
            var cbox = (ComboBox)sender;
            var ids = MainWindow.getSingleColumn("costumers", "id");
            _costs.Text = ids[cbox.SelectedIndex];
        }

        /// <summary>
        /// <c>assembleInsertQuery</c> формирует запрос на добавление данных на языке SQL для отправки в БД
        /// </summary>
        /// <returns>Возвращает сформированный запрос в виде строки</returns>
        private string assembleInsertQuery()
        {
            List<string> values = new List<string>();
            List<string> amounts = MainWindow.getSingleColumn("applications", "`Требуемое количество`");
            for (int i = 0; i < _textBoxes.Count; i++)
            {
                if (string.IsNullOrEmpty(_textBoxes[i].Text))
                {
                    values.Add("null");
                }
                else
                {
                    /*if (_curT.Name == "applications" && i == _textBoxes.Count)
                    {

                    }*/
                    values.Add("'" + _textBoxes[i].Text + "'");
                }
            }
            string query = "INSERT INTO " + _curT.Name + "(`" + string.Join("`, `", _mnWd.column_names) + "`) VALUES (" + string.Join(", ", values) + ");";
            return query;
        }

        /// <summary>
        /// <c>assembleEditQuery</c> формирует запрос на изменение данных на языке SQL для отправки в БД
        /// </summary>
        /// <returns>Возвращает сформированный запрос в виде строки</returns>
        private string assembleEditQuery()
        {
            List<string> values = new List<string>();
            List<string> columnsToInput = new List<string>();
            for (int i = 0; i < _textBoxes.Count; i++)
            {
                values.Add("'" + _textBoxes[i].Text + "'");
                if (_textBoxes[i].TextLength > 0)
                {
                    columnsToInput.Add($"`{_mnWd.column_names[i]}` = {values[i]}");
                }
            }
            string query = $"UPDATE {_curT.Name} SET " + string.Join(", ", columnsToInput) + $" WHERE id = {_mnWd.getRowId()};";
            return query;
        }

        private void updateAmount(int productID, int amount)
        {
            var amStr = MainWindow.getSingleColumn($"products  where id = {productID}", "`product_amount`");
            int amountOnStorage = int.Parse(amStr[0]);
            if(amountOnStorage >= amount)
            {
                amountOnStorage -= amount;
                _mnWd.executeCmd($"update products set `product_amount` = {amountOnStorage} where id = {productID}");
            }
            else
            {
                throw new ApplicationException("Amount on storage is less than requested amount");
            }

        }

        /// <summary>
        /// Обработка события нажатия на кнопку
        /// </summary>
        /// <param name="sender">отправитель</param>
        /// <param name="e">событие</param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (_itype == InsertWindowType.Delete)
            {
                if (_curT.Name != "applications")
                {
                    _mnWd.executeCmd(assembleInsertQuery());
                }
                else
                {
                    try
                    {
                        updateAmount(_prId, int.Parse(_textBoxes[_textboxAmountId].Text));
                        _mnWd.executeCmd(assembleInsertQuery());
                    }
                    catch (ApplicationException)
                    {
                        MessageBox.Show("Количество товара на складе меньше запрашиваемого значения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (_itype == InsertWindowType.Edit)
            {
                _mnWd.executeCmd(assembleEditQuery());
            }
            _mnWd.drawTable("SELECT * FROM " + _curT.View + ";");
            this.Close();
        }

    }
}
