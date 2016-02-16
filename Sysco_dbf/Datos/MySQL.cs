using System.Data;
using MySql.Data.MySqlClient;
using Sysco_dbf.Configuracion;

namespace Sysco_dbf.Datos
{
    class MySQL
    {
        private MySqlConnection _conexion;
        public static string StrSelectQuery;
        public MySQL()
        {
            var connectionString = "SERVER=" + Configuracion.Configuracion.General.MysqlHost + ";" + "DATABASE=" +
                          Configuracion.Configuracion.General.MysqlDatabase + ";" + "UID="
                          + Configuracion.Configuracion.General.MysqlUsuario + ";" + "PASSWORD="
                          + Configuracion.Configuracion.General.MysqlPassword + ";";

            _conexion = new MySqlConnection(connectionString);
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
                        SelectCommand = new MySqlCommand(Query(seleccion), _conexion)
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

        internal bool AbrirConexion()
        {
            try
            {
                _conexion.Open();
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
                _conexion.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }
    }
}

