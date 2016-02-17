using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
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
            var mySqlDataTable = new DataTable();
            var visualFoxProDataTable = new DataTable();
            mySqlDataTable = _vfp2mysql.Select(2);
            visualFoxProDataTable = _vfp2mysql.SelectVfp(1);

            var matched = from table1 in visualFoxProDataTable.AsEnumerable()
                          join table2 in mySqlDataTable.AsEnumerable() on table1.Field<string>("emplnum") equals table2.Field<string>("id_empleado").PadLeft(5, '0')
                          where table1.Field<int>("nombre") == table2.Field<int>("nombre") || table1.Field<string>("apellido1") == table2.Field<string>("paterno") || table1.Field<object>("apellido2") == table2.Field<object>("materno")  || table1.Field<String>("imss") == table2.Field<String>("nss") || table1.Field<String>("estado") == table2.Field<String>("estado")
                          select table1;
            var missing = from table1 in visualFoxProDataTable.AsEnumerable()
                          where !matched.Contains(table1)
                          select table1;
            DataTable dt = missing.CopyToDataTable();
            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (_vfp2mysql.Existe(dt.Rows[i][0].ToString()))
                {
                   // _vfp2mysql.Actualizar();
                }
                else
                {
                   // _vfp2mysql.Insertar();
                }
            }
            


        }
    }
}
