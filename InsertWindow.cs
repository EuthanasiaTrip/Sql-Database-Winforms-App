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
    public partial class InsertWindow : Form
    {
        private Point pos = new Point(35, 46);
        private MainWindow mnWd;
        private List<Label> lables = new List<Label>();
        private List<TextBox> textBoxes = new List<TextBox>();
        private string q;
        private InsertWindowType itype;

        public InsertWindow(MainWindow main, InsertWindowType t)
        {
            InitializeComponent();
            mnWd = main;
            tableName.Text = mnWd.currentTable.Name;
            for (int i = 0; i < mnWd.column_names.Count; i++)
            {
                try
                {
                    if (mnWd.currentTable.Name != "applications")
                    {
                        draw_controls(i);
                    }
                    else
                    {
                        draw_orders_controls(i);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("Please select the table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            itype = t;

            // moving OK button
            pos.Offset(0, 5);
            pos.X = 234;
            button1.Location = pos;
        }

        private void draw_controls(int i)
        {
            // add column name lable
            lables.Add(new Label() { Location = pos, Text = mnWd.column_names[i], AutoSize = true });
            this.Controls.Add(lables[i]);
            lables[i].Show();
            pos.Offset(0, 29);

            // add textbox for queries;
            textBoxes.Add(new TextBox() { Location = pos, Width = 512 });      
            this.Controls.Add(textBoxes[i]);
            textBoxes[i].Show();
            pos.Offset(0, 48);

        }

        //draw controls for table with foreign keys
        private void draw_orders_controls(int i)
        {
            ComboBox companiesBox = new ComboBox();
            ComboBox itemsBox = new ComboBox();

            lables.Add(new Label() { Location = pos, Text = mnWd.column_names[i], AutoSize = true });
            this.Controls.Add(lables[i]);
            lables[i].Show();
            pos.Offset(0, 29);

            if (mnWd.column_names[i] == "Название")
            {
                var itemsList = MainWindow.get_single_column(mnWd.currentTable.View, "`Название`");

                itemsBox.Location = pos;
                itemsBox.Width = 256;
                itemsBox.Items.AddRange(itemsList.ToArray());
                this.Controls.Add(itemsBox);
                itemsBox.Show();
                pos.Offset(0, 48);
            } 
            else if (mnWd.column_names[i] == "Организация")
            {
                var companiesList = MainWindow.get_single_column(mnWd.currentTable.View, "`Организация`");

                companiesBox.Location = pos;
                companiesBox.Width = 256;
                companiesBox.Items.AddRange(companiesList.ToArray());
                this.Controls.Add(companiesBox);
                companiesBox.Show();
                pos.Offset(0, 48);
            }
            else
            {
                textBoxes.Add(new TextBox() { Location = pos, Width = 512 });
                this.Controls.Add(textBoxes[textBoxes.Count - 1]);
                textBoxes[textBoxes.Count - 1].Show();
                pos.Offset(0, 48);
            }
        }

        private string assemble_insert_query()
        {
            List<string> values = new List<string>();
            for (int i=0; i < textBoxes.Count; i++)
            {
                if (string.IsNullOrEmpty(textBoxes[i].Text))
                {
                    values.Add("null");
                }
                else
                {
                    values.Add("'" + textBoxes[i].Text + "'");
                }
            }
            string query = "INSERT INTO " + mnWd.currentTable.Name + "(`" + string.Join("`, `", mnWd.column_names) + "`) VALUES (" + string.Join(", ", values) + ");";
            return query;
        }
       
        private string assemble_edit_query()
        {
            List<string> values = new List<string>();
            List<string> columnsToInput = new List<string>();
            for (int i = 0; i < textBoxes.Count; i++)
            {
                values.Add("'" + textBoxes[i].Text + "'");
                if (textBoxes[i].TextLength > 0)
                {
                    columnsToInput.Add($"`{mnWd.column_names[i]}` = {values[i]}");
                }
            }
            string query = $"UPDATE {mnWd.currentTable.Name} SET " + string.Join(", ", columnsToInput) + $" WHERE id = {mnWd.getRowId()};";
            return query;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (itype == InsertWindowType.Delete)
            {
                mnWd.execute_cmd(assemble_insert_query());
            }
            else if (itype == InsertWindowType.Edit)
            {
                mnWd.execute_cmd(assemble_edit_query());
            }
            mnWd.drawTable("SELECT * FROM " + mnWd.currentTable.View+";");
            System.Diagnostics.Debug.WriteLine(q);
            this.Close();
        }

    }
}
