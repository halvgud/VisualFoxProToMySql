using System.Data;
using System.Data.OleDb;
using MySql.Data.MySqlClient;
using Sysco_dbf.Configuracion;

namespace Sysco_dbf.Datos
{
    class VFP2MySql
    {
        private MySqlConnection _conexionMySql;
        public static string StrSelectQuery;
        private static OleDbConnection _conexionVFP;
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
                    StrSelectQuery = @"select
                    emplnum,
                    nombre,
                    apellido1,
                    apellido2,
                    sexo,
                    empltur,

                    empldep,
                    (case when emplbaj!='' then 'I' else 'A' end) as estado,
                    imss
                    from 
                    EMPLMST.dbf
                ";
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
                    var myQuery = new OleDbCommand(Query(2), _conexionVFP);
                    var da = new OleDbDataAdapter(myQuery);
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

        internal bool AbrirConexionVfp()
        {
            try
            {
                _conexionVFP = new OleDbConnection(@"Driver={Microsoft Visual FoxPro Driver};SourceType=DBF;SourceDB=" +
                              Configuracion.Configuracion.General.VisualFoxProDireccion + @"\;"); 
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

