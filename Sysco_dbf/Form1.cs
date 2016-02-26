using System.Data;
using System.Linq;
using System.Windows.Forms;
using Sysco_dbf.Configuracion;
using Sysco_dbf.Datos;
using System.IO;
using System;

namespace Sysco_dbf
{
    public partial class Form1 : Form
    {
        private VFP2MySql _vfp2mysql;
        private CargarConfiguracion _cargarConfiguracion;
        public Form1()
        {
            // InitializeComponent();
            _cargarConfiguracion = new CargarConfiguracion();
            _vfp2mysql = new VFP2MySql();
            GetFile();
            this.Close();
        }
        private void GetFile()
        {
            try
            {
                var dirInfo = new DirectoryInfo(Configuracion.Configuracion.General.VisualFoxProDireccion);
                var fecha = DateTime.Parse(dirInfo.LastWriteTime.TimeOfDay.ToString());
                DateTime fecha2 = new DateTime();
                if (Configuracion.Configuracion.General.UltimaModificacion ==null)
                {
                    actualizarUltimaModificacion(fecha);
                }
                fecha2 = Configuracion.Configuracion.General.UltimaModificacion;
                var diferencia = DateTime.Compare(fecha2, fecha);
                if (diferencia < 0)
                {
                    CompararHilos();
                    actualizarUltimaModificacion(fecha);
                }
                System.Threading.Thread.Sleep(1000*60*Configuracion.Configuracion.General.Tiempo);
                GetFile();
            }
            catch (Exception e)
            {
                CargarConfiguracion.Log(e.Message);
            }
        }
        private void actualizarUltimaModificacion(DateTime fecha)
        {
            try
            {
                Configuracion.Configuracion.General.UltimaModificacion = fecha;
                var contenido = string.Empty;
                using (StreamReader lector = new StreamReader(Application.StartupPath + "\\config.ini"))
                {
                    contenido = lector.ReadToEnd();
                    lector.Close();
                }
                contenido = System.Text.RegularExpressions.Regex.Replace(contenido, @"UltimaModificacion=.+$", @"UltimaModificacion=" + fecha);
                using (StreamWriter escritor = new StreamWriter(Application.StartupPath + "\\config.ini"))
                {
                    escritor.Write(contenido);
                    escritor.Close();
                }
            }
            catch (Exception e)
            {
                CargarConfiguracion.Log(e.Message);
            }
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
                        (table1, table2) => new { table1, table2 })
                    .Where(
                        @t =>
                            @t.table1.Field<string>("nombre").Trim() == @t.table2.Field<string>("nombre") &&
                            @t.table1.Field<string>("apellido1").Trim() == @t.table2.Field<string>("paterno") &&
                            @t.table1.Field<string>("apellido2").Trim() == @t.table2.Field<string>("materno") &&
                            @t.table1.Field<string>("imss").Trim() == @t.table2.Field<string>("nss") &&
                            @t.table1.Field<string>("estado").Trim() == @t.table2.Field<string>("estado"))
                    .Select(@t => @t.table1);
            var missing = from table1 in visualFoxProDataTable.AsEnumerable()
                          where !matched.Contains(table1)
                          select table1;
            if (missing.ToList().Count > 0)
            {
                DataTable dt = missing.CopyToDataTable();
                Empleado empleado = new Empleado();
                for (var i = 0; i < dt.Rows.Count; i++)
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
                    _vfp2mysql.Actualizar(_vfp2mysql.Existe(dt.Rows[i][0].ToString()) ? 1 : 2, empleado);
                }
            }
        }

        private void CompararHilos()
        {
            var mySqlDataTable = new DataTable();
            var visualFoxProDataTable = new DataTable();
            var dt = new DataTable();
            
            unsafe
            {

                mySqlDataTable = _vfp2mysql.Select(2);
                visualFoxProDataTable = _vfp2mysql.SelectVfp(1);
                dt = visualFoxProDataTable.Clone();
                System.Threading.Tasks.Parallel.For(0, visualFoxProDataTable.Rows.Count, y =>
                {
                    for (var x = 0; x < mySqlDataTable.Rows.Count; x++)
                    {
                        if (visualFoxProDataTable.Rows[y]["emplnum"].ToString().PadLeft(5, '0') == mySqlDataTable.Rows[x]["id_empleado"].ToString().PadLeft(5, '0'))
                        {
                            if (visualFoxProDataTable.Rows[y]["nombre"].ToString().Trim() != mySqlDataTable.Rows[x]["nombre"].ToString() ||
                                     visualFoxProDataTable.Rows[y]["apellido1"].ToString().Trim() != mySqlDataTable.Rows[x]["paterno"].ToString() ||
                                     visualFoxProDataTable.Rows[y]["apellido2"].ToString().Trim() != mySqlDataTable.Rows[x]["materno"].ToString() ||
                                     visualFoxProDataTable.Rows[y]["imss"].ToString().Trim() != mySqlDataTable.Rows[x]["nss"].ToString() ||
                                     visualFoxProDataTable.Rows[y]["estado"].ToString().Trim() != mySqlDataTable.Rows[x]["estado"].ToString())
                            {
                                DataRow row;
                                row = dt.NewRow();
                                row.ItemArray = visualFoxProDataTable.Rows[y].ItemArray;
                                dt.Rows.Add(row);
                            }
                        }
                    }
                });
            }
            Empleado empleado = new Empleado();
            for (var i = 0; i < dt.Rows.Count; i++)
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
                _vfp2mysql.Actualizar(_vfp2mysql.Existe(dt.Rows[i][0].ToString()) ? 1 : 2, empleado);
            }
        }
    }
}
