using System;
using System.Data.SqlClient;
using WiiBalanceBoard.Clases;

namespace WiiBalanceBoard.Services
{
    public class InterfazUsuario
    {
        public Usuario Usuario { get; set; }
        private static string ConnectionString = "Server=PP-WALL-E\\SQLEXPRESS;Database=Ultrasound;Trusted_Connection=True;";
        public string PathMainPythonScript = @"C:\Users\alexs\Desktop\Preprocesamiento\Main.py";
        public InterfazUsuario()
        {
            Usuario = new Usuario();
        }

        public void RegistrarUsuario()
        {
            ObtenerDatosUsuario();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                ObtenerNumeroDePrueba(conn);
                InsertarUsuarioEnBD(conn);
                ActualizarNumeroPruebas(conn);
            }
        }

        public void ActualizarNumeroPruebas(SqlConnection conn)
        {
            Usuario.NumeroPruebas += 1;
            string updateQuery = "UPDATE Usuarios SET NumeroPruebas = @NumeroPruebas WHERE Id = @Id";
            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@NumeroPruebas", Usuario.NumeroPruebas);
                cmd.Parameters.AddWithValue("@Id", Usuario.Id.Value);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine($"Datos de usuario. Id={Usuario.Id}, NumeroPruebas={Usuario.NumeroPruebas}");
        }

        public void InsertarUsuarioEnBD(SqlConnection conn)
        {
            // 2️⃣ Insertar nuevo usuario
            string insertQuery = "INSERT INTO Usuarios (Nombre, Apellido, Edad, Genero, Altura, Peso, NumeroPruebas, TipoEfecto, Descripcion ,TimeStamp, ConfiguracionId) " +
                                    "VALUES (@Nombre, @Apellido, @Edad, @Genero, @Altura, @Peso, @NumeroPruebas, @TipoEfecto, @Descripcion, @TimeStamp, @ConfiguracionId); " +
                                    "SELECT SCOPE_IDENTITY();"; //Esto devuelve el Id generado
            using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Nombre", Usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", Usuario.Apellido);
                cmd.Parameters.AddWithValue("@Edad", (object)Usuario.Edad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Genero", (object)Usuario.Genero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Altura", (object)Usuario.Altura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Peso", (object)Usuario.Peso ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroPruebas", Usuario.NumeroPruebas);
                cmd.Parameters.AddWithValue("@TipoEfecto", (object)Usuario.TipoEfecto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Descripcion", (object)Usuario.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TimeStamp", Usuario.TimeStamp);
                cmd.Parameters.AddWithValue("@ConfiguracionId", Usuario.ConfiguracionId);
                // Ejecutar y obtener el Id generado
                Usuario.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void ObtenerNumeroDePrueba(SqlConnection conn)
        {
            // 1️⃣ Buscar usuario por nombre y apellido
            string selectQuery = @"SELECT TOP 1 Id, NumeroPruebas FROM Usuarios WHERE Nombre = @Nombre AND Apellido = @Apellido ORDER BY TimeStamp DESC;
        "; using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Nombre", Usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", Usuario.Apellido);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Usuario existe
                        //Usuario.Id = reader.GetInt32(0);
                        Usuario.NumeroPruebas = reader.GetInt32(1);
                    }
                }
            }
        }
        public void ObtenerDatosUsuario()
        {
            Console.Write("Nombre: ");
            Usuario.Nombre = Console.ReadLine().ToUpper();

            Console.Write("Apellido: ");
            Usuario.Apellido = Console.ReadLine().ToUpper();

            Console.Write("Id de la configuración: ");
            var idConfiguracion = Console.ReadLine();
            Usuario.ConfiguracionId = int.TryParse(idConfiguracion, out int idconfig) ? (int?)idconfig : null;

            Console.Write("Descripción de la práctica (opcional): ");
            Usuario.Descripcion = Console.ReadLine();

            Usuario.TimeStamp = DateTime.Now;
        }
    }
}