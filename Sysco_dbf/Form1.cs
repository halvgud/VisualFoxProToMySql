using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sysco_dbf.Configuracion;
using Sysco_dbf.Datos;

namespace Sysco_dbf
{
    public partial class Form1 : Form
    {
        private VFP2MySql _vfp2mysql;
        private CargarConfiguracion _cargarConfiguracion;
        public Form1()
        {
            InitializeComponent();
            _cargarConfiguracion = new CargarConfiguracion();
             _vfp2mysql= new VFP2MySql();
            Comparar();
        }

        private void Comparar()
        {
            DataTable mySqlDataTable = new DataTable();
            DataTable visualFoxProDataTable = new DataTable();
            mySqlDataTable = _vfp2mysql.Select(1);
            visualFoxProDataTable = _vfp2mysql.SelectVfp(2);
                    var query = from r1 in mySqlDataTable.AsEnumerable()
                        join r2 in visualFoxProDataTable.AsEnumerable()
                        on r1.Field<string>("emplnum") equals r2.Field<string>("id_empleado")
                        where !r1.Field<int>("score").Equals(r2.Field<int>("score")) || 
                                !r1.Field<int>("score").Equals(r2.Field<int>("score"))
                        select new
                        {
                            name = r1.Field<string>("name"),
                            score1 = r1.Field<int>("score"),
                            score2 = r2.Field<int>("score")
                        };

            foreach (var item in query)
            {                //if(item.score1 != item.score2)

                Console.WriteLine("{0} {1} {2} ", item.name, item.score1, item.score2);

            }
        }
    }
}
