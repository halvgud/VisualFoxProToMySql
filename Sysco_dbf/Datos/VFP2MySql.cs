using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using MySql.Data.MySqlClient;
using Sysco_dbf.Configuracion;
using System.Windows.Forms;

namespace Sysco_dbf.Datos
{
    class VFP2MySql
    {
        private MySqlConnection _conexionMySql;
        public static string StrSelectQuery;
        private static OdbcConnection _conexionVFP;
        public VFP2MySql()
        {
            var connectionString = "SERVER=" + Configuracion.Configuracion.General.MysqlHost + ";" + "DATABASE=" +
                          Configuracion.Configuracion.General.MysqlDatabase + ";" + "UID="
                          + Configuracion.Configuracion.General.MysqlUsuario + ";" + "PASSWORD="
                          + Configuracion.Configuracion.General.MysqlPassword + ";";

            _conexionMySql = new MySqlConnection(connectionString);
            

        }
        public string Query(int seleccion)
        {
            switch (seleccion)
            {
                case 1:
                    StrSelectQuery ="select  emplnum,  nombre, apellido1, apellido2, "
                        +" sexo, empltur, empldep, imss,'A' as estado from '"
                        + Configuracion.Configuracion.General.VisualFoxProDireccion + "' where "
                        +" empty(emplbaj) "
                        +"union"
                        +" select  emplnum,  nombre, apellido1, apellido2, sexo, "
                                                +"empltur, empldep, imss,'I' as estado from '"+ Configuracion.Configuracion.General.VisualFoxProDireccion+  "' where "
                        +" !empty(emplbaj)";
                    break;
                case 2:
                    StrSelectQuery = @"select
                        id_empleado,
                        nombre,
                        materno,
                        paterno,
                        sexo, 
                        id_turno,
                        id_area,
                        nss,
                        estado as estado
                        from empleado";  
                    break;
            case 3:
                    StrSelectQuery = @""; break;
            }

            return StrSelectQuery;
        }

        /// <summary>
        /// Funcion meramente interna de la clase
        /// </summary>
        /// <param name="seleccion">id del query</param>
        /// <param name="empleado">Clase con los valores del empleado</param>
        /// <returns></returns>
        internal string Query(int seleccion, Empleado empleado)
        {
            switch (seleccion)
            {
                case 1:
                    StrSelectQuery = "" +
                                     "update empleado " +
                                     "set nombre='"+empleado.Nombre.Trim() +"'"+
                                     ",materno='" + empleado.Materno.Trim() + "'" +
                                     ",paterno='" + empleado.Paterno.Trim() + "'" +
                                    ",sexo='" + empleado.Sexo.Trim() + "'" +
                                    ",id_turno='" + empleado.Id_turno.Trim() + "'" +
                                    ",id_area='" + empleado.Id_area.Trim() + "'" +
                                    ",nss='" + empleado.Nss.Trim() + "'" +
                                    ",estado='"+empleado.Id_estado+"'"
                                    +" where id_empleado='"+empleado.id_empleado+"'";
                    break;
                case 2:
                    StrSelectQuery = "insert into empleado(id_empleado,nombre,paterno,materno,sexo,id_turno" +
                                     ",id_area,nss,estado) values(" +
                                     "'" + empleado.id_empleado.Trim() + "'," +
                                     "'" + empleado.Nombre.Trim() + "'," +
                                     "'" + empleado.Paterno.Trim() + "'," +
                                     "'" + empleado.Materno.Trim() + "'," +
                                     "'" + empleado.Sexo.Trim() + "'," +
                                     "'" + empleado.Id_turno.Trim() + "'," +
                                     "'" + empleado.Id_area.Trim() + "'," +
                                     "'" + empleado.Nss.Trim() + "'," +
                                     "'" + empleado.Id_estado.Trim() + "')";
                    break;
            }
            return StrSelectQuery;
        }
        public DataTable Select(int seleccion)
        {
            try
            {
                if (AbrirConexion())
                {
                    var adaptador = new MySqlDataAdapter
                    {
                        SelectCommand = new MySqlCommand(Query(seleccion), _conexionMySql)
                    };
                    var tabla = new DataTable();
                    adaptador.Fill(tabla);
                    return tabla;
                }
                else return null;
            }
            finally
            {
                CerrarConexion();
            }
        }

        public DataTable SelectVfp(int seleccion)
        {
            try
            {
                if (AbrirConexionVfp())
                {
                    var resultado = new DataTable();
                    var myQuery = new OdbcCommand(Query(seleccion), _conexionVFP);
                    var da = new OdbcDataAdapter(myQuery);
                    da.Fill(resultado);
                    CerrarConexionVfp();
                    return resultado;
                }
                else return null;
            }
            finally
            {
                CerrarConexionVfp();
            }
        }
        public bool Existe(string seleccion)
        {
            var query = "select 1 from empleado where id_empleado='"+seleccion.TrimStart('0')+"'";

            try
            {
             using (MySqlCommand cmd = new MySqlCommand())
             {
                 AbrirConexion();
            cmd.Connection = _conexionMySql;
                 cmd.CommandText = query;
                 return cmd.ExecuteReader().HasRows;
             }
                
            }
            catch (MySqlException e)
            {
                CargarConfiguracion.Log(e.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return false;
        }

        public void Actualizar(int seleccion,Empleado emp)
        {
            try
            {

                var cmd = new MySqlCommand { CommandText = Query(seleccion,emp) };
                if (!AbrirConexion()) return;
                cmd.Connection = _conexionMySql;
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                CargarConfiguracion.Log(e.Message);
                throw;
            }
            finally
            {
                CerrarConexion();
            }
        }
        internal bool AbrirConexionVfp()
        {
            try     
            {
                _conexionVFP = new OdbcConnection(@"Driver={Microsoft Visual FoxPro Driver};SourceType=DBF;SourceDB=" + @"C:\Git\Sysco\scproject\emplmst.dbf" + ";Exclusive=No; Collate=Machine;NULL=NO;DELETED=YES;BACKGROUNDFETCH=NO;"); 
                _conexionVFP.Open();
                return true;
            }
            catch (OleDbException oe)
            {
                CargarConfiguracion.Log(oe.Message);
                throw;
            }
        }

        internal bool CerrarConexionVfp()
        {
            try
            {
                _conexionVFP.Close();
                return true;
            }
            catch (OleDbException ex)
            {
                CargarConfiguracion.Log(ex.Message);
                throw ex;
            }
        }
        internal bool AbrirConexion()
        {
            try
            {
                _conexionMySql.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                CargarConfiguracion.Log(ex.ToString());
                return false;
            }
        }
        internal bool CerrarConexion()
        {
            try
            {
                _conexionMySql.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }
    }
}

