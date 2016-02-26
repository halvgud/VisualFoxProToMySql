using System;

namespace Sysco_dbf.Configuracion
{
    public class Configuracion
    {
        public class General
        {
            public static string MysqlHost { get; set; }
            public static string MysqlPassword { get; set; }
            public static string MysqlUsuario { get; set; }
            public static string MysqlDatabase { get; set; }
            public static string VisualFoxProDireccion { get; set; }
            public static DateTime UltimaModificacion { get; set; }
            public static int Tiempo { get; set; };
        }
    }
}
