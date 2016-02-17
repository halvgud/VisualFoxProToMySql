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
                    StrSelectQuery = @"select  emplnum,  nombre, apellido1, apellido2, sexo, empltur, empldep, imss,'A' as estado from C:\Git\Sysco\scproject\emplmst.dbf ";
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
                        id_departamento,
                        id_estado
                        from empleado";  
                    break;
            case3:
                    StrSelectQuery = @"";
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
            var query = "select 1 from empleado where id_empleado='@empleado'";

            try
            {
                AbrirConexion();
                MySqlDataAdapter MyDA = new MySqlDataAdapter();
                MyDA.SelectCommand= new MySqlCommand(query, _conexionMySql);
                MyDA.SelectCommand.Parameters.AddWithValue("@empleado", seleccion);
                MySqlDataReader reader = MyDA.SelectCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException e)
            {
                throw e;
            }
            finally
            {
                CerrarConexion();
            }
        }

        public void Actualizar(int seleccion)
        {
            try
            {

                var cmd = new MySqlCommand { CommandText = Query(seleccion) };
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

