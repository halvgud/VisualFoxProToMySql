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

            var matched =
                visualFoxProDataTable.AsEnumerable()
                    .Join(mySqlDataTable.AsEnumerable(), table1 => table1.Field<string>("emplnum").PadLeft(5, '0'),
                        table2 => table2.Field<string>("id_empleado").PadLeft(5, '0'),
                        (table1, table2) => new {table1, table2})
                    .Where(
                        @t =>
                            @t.table1.Field<string>("nombre") == @t.table2.Field<string>("nombre") &&
                            @t.table1.Field<string>("apellido1") == @t.table2.Field<string>("paterno") &&
                            @t.table1.Field<string>("apellido2") == @t.table2.Field<string>("materno") &&
                            @t.table1.Field<string>("imss") == @t.table2.Field<string>("nss") &&
                            @t.table1.Field<string>("estado") == @t.table2.Field<string>("estado"))
                    .Select(@t => @t.table1);
            var missing = from table1 in visualFoxProDataTable.AsEnumerable()
                          where !matched.Contains(table1)
                          select table1;
            DataTable dt = missing.CopyToDataTable();
            Empleado empleado = new Empleado();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                
                empleado.id_empleado = dt.Rows[i][0].ToString().TrimStart('0');
                empleado.Nombre = dt.Rows[i][1].ToString();
                empleado.Paterno = dt.Rows[i][2].ToString();
                empleado.Materno = dt.Rows[i][3].ToString();
                empleado.Sexo = dt.Rows[i][4].ToString();
                empleado.Id_turno = dt.Rows[i][5].ToString();
                empleado.Id_area = dt.Rows[i][6].ToString();
                empleado.Nss = dt.Rows[i][7].ToString();
                empleado.Id_estado = dt.Rows[i][8].ToString();
                if (_vfp2mysql.Existe(dt.Rows[i][0].ToString()))
                {
                    
                   _vfp2mysql.Actualizar(1,empleado);
                }
                else
                {
                    _vfp2mysql.Insertar(1,empleado);
                }
            }
            


        }
    }
}
