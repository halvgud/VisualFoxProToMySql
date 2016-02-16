using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sysco_dbf.Configuracion
{
    class CargarConfiguracion
    {
        public CargarConfiguracion()
        {
            InicializarVariables();
        }
        /// <summary>
        /// Inicializa las variables necesarias para realizar la conexion a la bd
        /// Se requiere un archivo config.ini en C:\System32\
        /// </summary>
        private void InicializarVariables()
        {
            try
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini")))
                {
                    var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
                    Dictionary<string, string> dictionary
                         = (from Match m in lines.SelectMany(line => Regex.Matches(line, "(.*)=(.*)").Cast<Match>())
                            select new
                            {
                                key = m.Groups[1].Value,
                                value = m.Groups[2].Value
                            }
               ).ToDictionary(p => p.key, p => p.value);
                    var config = new Configuracion();
                    var config2 = new Configuracion.General();
                    var rgx = new Regex("(.*)\\.(.*)");
                    foreach (var element in dictionary)
                    {
                        var match = rgx.Match(element.Key);
                        var prop = config.GetType().GetNestedType(match.Groups[1].Value.Trim()).GetProperty(match.Groups[2].Value.Trim());
                        if (null != prop && prop.CanWrite)
                        {
                            prop.SetValue(config2, Convert.ChangeType(element.Value, prop.PropertyType), null);
                        }

                    }

                }
                else
                {
                    Log("Error al cargar configuraciones, se cargará configuracion default");
                }
            }
            catch (Exception a)
            {
                Log("Error al leer configuraciones, se cargará configuracion default");
                Log(a.ToString());
                throw;
            }
        }
        /// <summary>
        /// Función que realiza la escritura de mensajes a un archivo
        /// </summary>
        /// <param name="mensaje">Mensaje a escribir en archivo</param>
        /// <remarks></remarks>
        public static void Log(string mensaje)
        {
            StreamWriter txtMirror = default(StreamWriter);
            try
            {
                txtMirror = new StreamWriter("Logsys_co_db.txt", true);
                txtMirror.WriteLine(DateTime.Now.ToLocalTime() + "::" + mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (txtMirror != null) txtMirror.Close();
            }
        }
    }
}
