using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class CostumersOrdersWindoow : Form
    {
        private MainWindow mnWd;
        private List<string[]> rows = new List<string[]>();
        private int visible_cols = 8;

        public CostumersOrdersWindoow(MainWindow mn, string id)
        {
            InitializeComponent();
            label1.Text = mn.currentTable.Name;
            drawOrders($"select costumers.Организация, `Номер заявки`, `Дата размещения заявки`, " +
                "products.Название as `Товар`, `Требуемое количество`, products.Цена from applications" +
                " inner join products on applications.`Товар` = products.id " +
                $"inner join costumers on applications.`Покупатель` = costumers.id where costumers.Организация = '{id}'");
        }

        private void drawOrders(string cmd)
        {
            MainWindow.Connect();
            MainWindow.command = new MySqlCommand((cmd + ";"), MainWindow.connection);
            try
            {
                MainWindow.reader = MainWindow.command.ExecuteReader();
                while (MainWindow.reader.Read())
                {
                    rows.Add(new string[MainWindow.reader.FieldCount]);

                    while (MainWindow.reader.FieldCount > visible_cols)
                    {
                        visible_cols++;
                        dataGridView1.Columns[visible_cols - 1].Visible = true;
                    }

                    while (MainWindow.reader.FieldCount < visible_cols)
                    {
                        visible_cols--;
                        dataGridView1.Columns[visible_cols].Visible = false;
                    }

                    for (int i = 0; i < MainWindow.reader.FieldCount; i++)
                    {
                        rows[rows.Count - 1][i] = MainWindow.reader[i].ToString();
                        dataGridView1.Columns[i].HeaderText = MainWindow.reader.GetName(i);
                    }
                }


                foreach (string[] s in rows)
                {
                    dataGridView1.Rows.Add(s);
                }

                MainWindow.reader.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
            MainWindow.connection.Close();
        }
    }
}
