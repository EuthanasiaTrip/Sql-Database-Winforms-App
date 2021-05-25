using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WindowsFormsApp1
{
    internal class ToolStripMenuItemWithSQL : ToolStripMenuItem
    {
        public string SQLquery { get; set; }
    }
    public partial class MainWindow : Form
    {
        private string[] queriesList = new string[]
        {
            //2
            "select Цена, Название from products_view",
            //3
            "select Цена, Название from products_view order by Название",
            //4
            "select Цена, Название as Товар from products_view where Цена > 3000",
            //5
            "select Цена, Название as Товар from products_view where Цена between 3000 and 10000",
            //6
            "select * from products_view where Название like 'М%'",
            //7
            "select avg(`Цена` * product_amount) as `Средняя стоимость товаров` from products_view",
            //8
            "select* from products where `Название` = {}",
            //9
            "select costumers.Организация, products.Название " +
            "from applications inner join products on applications.`Товар` = products.id " +
            "inner join costumers on applications.`Покупатель` = costumers.id where costumers.имя = 'Андрей'",
            //10
            "select(Цена* product_amount) as `Стоимость товара` from products_view",
            //11
            "select costumers.Организация, sum(applications.`Требуемое количество` * products.Цена) as `Сумма` " +
            "from applications inner join products on applications.`Товар` = products.id" +
            " inner join costumers on applications.`Покупатель` = costumers.id "+
            "group by costumers.Организация",
            //12

            "select costumers.Организация, sum(applications.`Требуемое количество` * products.Цена) as `Сумма` " +
            "from applications inner join products on applications.`Товар` = products.id" +
            " inner join costumers on applications.`Покупатель` = costumers.id "+
            "group by costumers.Организация order by Сумма desc limit 1",
            //13
            "select costumers.Фамилия, costumers.Имя, costumers.Отчество, max(applications.`Требуемое количество`)" +
            " from applications inner join products on applications.`Товар` = products.id " +
            "inner join costumers on applications.`Покупатель` = costumers.id where products.Название = 'Молоко'",
            //14
            "select costumers.Фамилия, costumers.Имя, costumers.Отчество, products.Название, " +
            "applications.`Требуемое количество`, products.product_amount from applications " +
            "inner join products on applications.`Товар` = products.id " +
            "inner join costumers on applications.`Покупатель` = costumers.id where applications.`Требуемое количество` > products.product_amount",
            //15
            "select* from costumers where Адрес is null",
            //16
            "select products.`Название`, applications.`Требуемое количество`, (applications.`Требуемое количество` * products.Цена) as `Стоимость` " +
            "from applications inner join products on applications.`Товар` = products.id " +
            "inner join costumers on applications.`Покупатель` = costumers.id",
            //17
            "select* from applications where day(`Дата размещения заявки`) = 1",
            //18
            "select costumers.id, applications.`Номер заявки`, (applications.`Требуемое количество` * products.Цена) " +
            "from applications inner join products on applications.`Товар` = products.id " +
            "inner join costumers on applications.`Покупатель` = costumers.id",
            //19
            "select applications.`Номер заказа`",
            //20
            "select costumers.`Организация`, (applications.`Требуемое количество` * products.Цена) as `Сумма`, " +
            "applications.`Дата размещения заявки` from applications " +
            "inner join products on applications.`Товар` = products.id " +
            "inner join costumers on applications.`Покупатель` = costumers.id",
            //21
            "select * from app_view where `Дата оплаты` is null"

    };
        private ToolStripMenuItemWithSQL[] premadeQueries = new ToolStripMenuItemWithSQL[20];

        private void drawPremadeQuery(object sender, EventArgs e)
        {
            isPrmdQuerySelected = true;
            currentTable.Name = null;
            currentTable.View = null;

            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;

            set_table_name(null);

            ToolStripMenuItemWithSQL n = (ToolStripMenuItemWithSQL)sender;
            drawTable(n.SQLquery);
        }

    }
}
