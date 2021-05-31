using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class CostumersOrdersWindoow : Form
    {
        private MainWindow mnWd;
        private List<string[]> rows = new List<string[]>();
        private int visible_cols = 8;
        private string costumer;

        public CostumersOrdersWindoow(MainWindow mn, string id)
        {
            InitializeComponent();
            label1.Text = mn.currentTable.Name;
            costumer = id;
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
                        if(rows[rows.Count - 1][i].Contains("0:00:00"))
                        {
                            rows[rows.Count - 1][i] = rows[rows.Count - 1][i].Replace(" 0:00:00","");
                        }
                        
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

        private void button1_Click(object sender, EventArgs e)
        {
            string filename = @"\orders_report_" + DateTime.Now.ToString().Replace(" ","_").Replace(":","-") + ".txt";

            string folderPath = getPath();

            if (folderPath != null)
            {
                folderPath += filename;
                
                using (StreamWriter wr = File.CreateText(folderPath))
                {
                    wr.WriteLine($"Заказы для {costumer}:\n\n");

                    for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
                    {
                        var order = $"Заказ №{dataGridView1.Rows[i].Cells[1].Value} от " +
                            $"{dataGridView1.Rows[i].Cells[2].Value}: {dataGridView1.Rows[i].Cells[3].Value} стоимостью " +
                            $"{dataGridView1.Rows[i].Cells[5].Value} рублей в количестве {dataGridView1.Rows[i].Cells[4].Value}.";
                        wr.WriteLine(order);
                    }

                    MessageBox.Show("Отчёт успешно сохранён", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите папку", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string getPath()
        {
            using (var path = new FolderBrowserDialog())
            {
                path.Description = "Выберите папку для сохранения отчёта";

                DialogResult result = path.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(path.SelectedPath))
                {
                    string folderPath = path.SelectedPath;
                    return folderPath;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
