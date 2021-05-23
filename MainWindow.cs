using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WindowsFormsApp1
{
    public struct table
    {
        public string View;
        public string Name;
    }

    public enum InsertWindowType
    {
        Delete,
        Edit
    }
    public partial class MainWindow : Form
    {
        public struct table
        {
            public string View;
            public string Name;
        }
        private static MySqlConnection connection;
        private static MySqlCommand command;
        private static MySqlDataReader reader;
        private List<string[]> rows = new List<string[]>();
        private List<int> ids = new List<int>();
        public List<string> column_names = new List<string>();
        public table currentTable = new table();
        public string[] queries;
        private int visible_cols = 7;
        private bool isPrmdQuerySelected = false;

        public MainWindow()
        {
            InitializeComponent();
            labelTable.Visible = false;
            for (int i = 0; i < queriesList.Length; i++)
            {
                if (i != 6 && i != 7 && i != 11)
                {
                    premadeQueries[i] = new ToolStripMenuItemWithSQL() { Text = $"Запрос {i+2}", SQLquery = queriesList[i] };
                    premadeQueries[i].Click += new System.EventHandler(this.drawPremadeQuery);
                }
                else if(i == 6)
                {
                    List<ToolStripMenuItemWithSQL> namesButtons = new List<ToolStripMenuItemWithSQL>();
                    var names = get_single_column("products", "Название");
                    for(int j = 0; j < names.Count; j++)
                    {
                        namesButtons.Add(new ToolStripMenuItemWithSQL
                        {
                            Text = names[j],
                            SQLquery = $"select * from products_view where `Название` = '{names[j]}'"
                        });
                        namesButtons[j].Click += new System.EventHandler(this.drawPremadeQuery);
                    }
                    premadeQueries[i] = new ToolStripMenuItemWithSQL() { Text = $"Запрос {i+2}" };
                    premadeQueries[i].DropDownItems.AddRange(namesButtons.ToArray());
                }
                else if (i == 7)
                {
                    List<ToolStripMenuItemWithSQL> clientsButtons = new List<ToolStripMenuItemWithSQL>();
                    var names = get_single_column("costumers", "Организация");
                    for (int j = 0; j < names.Count; j++)
                    {
                        clientsButtons.Add(new ToolStripMenuItemWithSQL
                        {
                            Text = names[j],
                            SQLquery = "select costumers.Организация, applications.`Номер заявки`, products.Название, applications.`Требуемое количество`, products.`Цена` " +
                             "from applications inner join products on applications.`Товар` = products.id " +
                             $"inner join costumers on applications.`Покупатель` = costumers.id where costumers.Организация = '{names[j]}'"
                        });
                        clientsButtons[j].Click += new System.EventHandler(this.drawPremadeQuery);
                    }
                    premadeQueries[i] = new ToolStripMenuItemWithSQL() { Text = $"Запрос {i+2}" };
                    premadeQueries[i].DropDownItems.AddRange(clientsButtons.ToArray());
                }
                else if (i == 11)
                {
                    List<ToolStripMenuItemWithSQL> productsButtons = new List<ToolStripMenuItemWithSQL>();
                    var names = get_single_column("products", "Название");
                    for (int j = 0; j < names.Count; j++)
                    {
                        productsButtons.Add(new ToolStripMenuItemWithSQL
                        {
                            Text = names[j],
                            SQLquery = "select costumers.Фамилия, costumers.Имя, costumers.Отчество, " +
                            "products.Название as Товар, (applications.`Требуемое количество`) as Количество" +
                            " from applications inner join products on applications.`Товар` = products.id " +
                            $"inner join costumers on applications.`Покупатель` = costumers.id where products.Название = '{names[j]}' and " +
                            "(applications.`Требуемое количество`) = (select max(applications.`Требуемое количество`) from applications " +
                            $" inner join products on applications.`Товар` = products.id where products.Название = '{names[j]}')"
                        }) ;
                        productsButtons[j].Click += new System.EventHandler(this.drawPremadeQuery);
                    }
                    premadeQueries[i] = new ToolStripMenuItemWithSQL() { Text = $"Запрос {i+2}" };
                    premadeQueries[i].DropDownItems.AddRange(productsButtons.ToArray());
                }

            }
            queriesToolStripMenu.DropDownItems.AddRange(premadeQueries);
        }


        // Connection to DB
        private static void Connect()
        {
            string cmd = "server=localhost;user=root;password=Flamingoe228;database=store;";
            try
            {
                connection = new MySqlConnection(cmd);
                connection.Open();
                //MessageBox.Show("Connection open", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(InvalidOperationException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch(MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static List<string> get_single_column(string tname, string cname)
        {
            List<string> column = new List<string>();
            Connect();
            command = new MySqlCommand($"SELECT {cname} FROM {tname}", connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                column.Add(reader[0].ToString());
            }
            reader.Close();
            connection.Close();
            return column;
        }

        private void get_ids(string tname)
        {
            if (!string.IsNullOrEmpty(tname))
            {
                Connect();
                command = new MySqlCommand($"SELECT id FROM {tname}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ids.Add(reader.GetInt32(0));
                }
                reader.Close();
                connection.Close();
            }
        }


        //method for performing SQL query to display table
        //insert cmd without ";", it will be automatically added
        public void drawTable(string cmd)
        {
            if (dataGridView1.DisplayedRowCount(false) > 1)
            {
                clear_table();
                //get_column_names(currentTable.View);
            }
            get_ids(currentTable.Name);
            Connect();
            command = new MySqlCommand((cmd + ";"), connection);
            try
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    rows.Add(new string[reader.FieldCount]);
                    
                    while(reader.FieldCount > visible_cols)
                    {
                        visible_cols++;
                        dataGridView1.Columns[visible_cols - 1].Visible = true;
                    }

                    while (reader.FieldCount < visible_cols)
                    {
                        visible_cols--;
                        dataGridView1.Columns[visible_cols].Visible = false;
                    }

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        rows[rows.Count - 1][i] = reader[i].ToString();
                        dataGridView1.Columns[i].HeaderText = reader.GetName(i);
                    }
                }


                foreach (string[] s in rows)
                {
                    dataGridView1.Rows.Add(s);
                }

                reader.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
            connection.Close();

            for(int i =0; i<dataGridView1.Columns.Count; i++)
            {
                if (dataGridView1.Columns[i].Visible)
                {
                    column_names.Add(dataGridView1.Columns[i].HeaderText);
                }
            }
        }

        public void execute_cmd(string cmd)
        {
            Connect();
            command = new MySqlCommand(cmd + ";", connection);
            try
            {
                reader = command.ExecuteReader();
            }
            catch(MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
            reader.Close();
            connection.Close();
        }

        //draw table button
        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentTable.View) && !isPrmdQuerySelected)
                {
                    drawTable("SELECT * FROM " + currentTable.View + ";");
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите и отобразите таблицу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

        }

        //clear grid
        private void clear_table()
        {
            dataGridView1.Rows.Clear();
            rows.Clear();
            column_names.Clear();
            ids.Clear();
        }
        
        // strip menu buttons
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            isPrmdQuerySelected = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            currentTable.View = "products_view";
            set_table_name("products");
            clear_table();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            isPrmdQuerySelected = false;
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem4.Checked = false;
            currentTable.View = "costumers_view";
            set_table_name("costumers");
            clear_table();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            isPrmdQuerySelected = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem2.Checked = false;
            currentTable.View = "app_view";
            set_table_name("applications");
            clear_table();
        }

        // invoke window for manual queries
        private void button2_Click(object sender, EventArgs e)
        {
            var QueryPopup = new QueryWindow(this);
            QueryPopup.Show();
        }

        // invoke window for data input
        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentTable.View)
                && column_names.Count != 0
                && isPrmdQuerySelected == false)
            {
                var InsertPopup = new InsertWindow(this, InsertWindowType.Delete);
                InsertPopup.Show();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите и отобразите таблицу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void set_table_name(string tn)
        {
            labelTable.Visible = true;
            currentTable.Name = tn;
            labelTable.Text = tn;
        }

        private void dataGridView1_CellRightClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                dataGridView1.ContextMenuStrip = contextMenuStrip1;
                dataGridView1.ContextMenuStrip.Show(Cursor.Position);
                
            }
        }

        private void deleteRowButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentTable.View)
                && column_names.Count != 0
                && isPrmdQuerySelected == false)
            {
                var confirm = MessageBox.Show("Вы действительно хотите удалить эту строку?", "Удаление строки", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    execute_cmd("DELETE FROM " + currentTable.Name + " WHERE id=" + ids[dataGridView1.CurrentRow.Index] + ";");
                    drawTable($"SELECT * FROM {currentTable.View}");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите и отобразите таблицу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            clear_table();
        }

        public int getRowId()
        {
            return ids[dataGridView1.CurrentRow.Index];
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentTable.View) 
                && column_names.Count != 0
                && isPrmdQuerySelected == false)
            {
                var InsertPopup = new InsertWindow(this, InsertWindowType.Edit);
                InsertPopup.Show();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите и отобразите таблицу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
