using System;
using System.Data.SqlClient;
using WiiBalanceBoard.Objects;

namespace WiiBalanceBoard.Services
{
    public class UserInterface
    {
        public User Usuario { get; set; }
        private static string ConnectionString = "Server=PP-WALL-E\\SQLEXPRESS;Database=Ultrasound;Trusted_Connection=True;";
        public string PathMainPythonScript = @"C:\Users\alexs\Desktop\Preprocesamiento\Main.py";
        public UserInterface()
        {
            Usuario = new User();
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
            string insertQuery = "INSERT INTO Usuarios (Nombre, Apellido, Edad, Genero, Altura, Peso, NumeroPruebas, TipoEfecto, Descripcion ,TimeStamp) " +
                                    "VALUES (@Nombre, @Apellido, @Edad, @Genero, @Altura, @Peso, @NumeroPruebas, @TipoEfecto, @Descripcion, @TimeStamp); " +
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
                // Ejecutar y obtener el Id generado
                Usuario.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void ObtenerNumeroDePrueba(SqlConnection conn)
        {
            // 1️⃣ Buscar usuario por nombre y apellido
            string selectQuery = "SELECT Id, NumeroPruebas FROM Usuarios WHERE Nombre = @Nombre AND Apellido = @Apellido";
            using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
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

            Console.Write("Edad (opcional): ");
            var edadStr = Console.ReadLine();
            Usuario.Edad = int.TryParse(edadStr, out int edad) ? (int?)edad : null;

            Console.Write("Genero (opcional): ");
            Usuario.Genero = Console.ReadLine();

            Console.Write("Altura en metros (opcional): ");
            var alturaStr = Console.ReadLine();
            Usuario.Altura = float.TryParse(alturaStr, out float altura) ? (float?)altura : null;

            Console.Write("Peso en kg (opcional): ");
            var pesoStr = Console.ReadLine();
            Usuario.Peso = float.TryParse(pesoStr, out float peso) ? (float?)peso : null;

            Console.WriteLine("Tipo de efecto. Elige una opción:");
            Console.WriteLine("1- Con toque ligero");
            Console.WriteLine("2- Sin toque ligero");
            Console.WriteLine("3- Otro");
            Console.Write("Elige una de las opciones anteriores (1/2/3): ");
            var tipoEfecto = Console.ReadLine();
            var tipo = "";

            if(tipoEfecto == "1")
                tipo = "Con toque ligero";
            else if(tipoEfecto == "2")
                tipo = "Sin toque ligero";
            else
                tipo = "otro";

            Usuario.TipoEfecto = tipo;

            Console.Write("Descripción de la práctica (opcional): ");
            Usuario.Descripcion = Console.ReadLine();

            Usuario.TimeStamp = DateTime.Now;
        }
    }
}