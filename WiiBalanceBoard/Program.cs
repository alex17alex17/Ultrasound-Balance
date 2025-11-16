using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using WiiBalanceBoard.Objects;
using WiiBalanceBoard.Services;
using WiimoteLib;

namespace WiiBalanceBoard
{
    internal class Program
    {
        // ⚙️ Configuración
        private static string connectionString = "Server=PP-WALL-E\\SQLEXPRESS;Database=Ultrasound;Trusted_Connection=True;";

        private static List<LecturaBalance> buffer = new List<LecturaBalance>();
        private static object lockObj = new object();
        private static Timer batchTimer;
        private static UserInterface userInterface = new UserInterface();


        private static void Main()
        {
            userInterface.RegistrarUsuario();

            var wm = new Wiimote();
            wm.WiimoteChanged += OnWiimoteChanged;
            wm.Connect();


            System.Threading.Thread.Sleep(500);

            if (wm.WiimoteState.ExtensionType == ExtensionType.BalanceBoard)
                Console.WriteLine("✅ Balance Board detectada. Guardando lecturas por lotes cada 1s...");
            else
            {
                Console.WriteLine("⚠️ No se detectó una Balance Board.");
                return;
            }

            // 🔄 Timer que guarda los datos cada 1 segundo
            batchTimer = new Timer(1000);
            batchTimer.Elapsed += (s, e) => FlushBufferToDatabase();
            batchTimer.Start();

            Console.WriteLine("Presiona ENTER para salir...");
            Console.ReadLine();

            batchTimer.Stop();
            FlushBufferToDatabase(); // guarda lo que quede pendiente
            wm.Disconnect();
        }

        private static void OnWiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            var state = e.WiimoteState.BalanceBoardState;
            var s = state.SensorValuesKg;

            float TL = s.TopLeft; // En Kilogramos
            float TR = s.TopRight;
            float BL = s.BottomLeft;
            float BR = s.BottomRight;
            float total = state.WeightKg;

            if (total < 0.01f)
                return;

            // Dimensiones aproximadas de la Wii Balance Board (en metros)
            const float L = 0.43f; // largo (X) Se mide la distancia entre los sensores izquierdo y derecho
            const float W = 0.24f; // ancho (Y)

            float copX = ((TR + BR) - (TL + BL)) / (TL + TR + BL + BR) * (L / 2);
            float copY = ((TL + TR) - (BL + BR)) / (TL + TR + BL + BR) * (W / 2);

            var sample = new LecturaBalance
            {
                UsuarioId = userInterface.Usuario.Id,
                NumeroPruebas = userInterface.Usuario.NumeroPruebas,
                TopLeft = TL,
                TopRight = TR,
                BottomLeft = BL,
                BottomRight = BR,
                COP_X = copX,
                COP_Y = copY,
                Total = total,
                TimeStamp = DateTime.Now,
            };

            lock (lockObj)
            {
                buffer.Add(sample);
            }
        }

        private static void FlushBufferToDatabase()
        {
            List<LecturaBalance> copy;

            lock (lockObj)
            {
                if (buffer.Count == 0)
                    return;

                copy = new List<LecturaBalance>(buffer);
                buffer.Clear();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Crea un DataTable temporal para usar SqlBulkCopy
                    DataTable table = new DataTable();
                    table.Columns.Add("UsuarioId", typeof(int));
                    table.Columns.Add("NumeroPruebas", typeof(int));
                    table.Columns.Add("TopLeft", typeof(float));
                    table.Columns.Add("TopRight", typeof(float));
                    table.Columns.Add("BottomLeft", typeof(float));
                    table.Columns.Add("BottomRight", typeof(float));
                    table.Columns.Add("COP_X", typeof(float));
                    table.Columns.Add("COP_Y", typeof(float));
                    table.Columns.Add("Total", typeof(float));
                    table.Columns.Add("TimeStamp", typeof(DateTime));

                    foreach (var s in copy)
                    {
                        table.Rows.Add(s.UsuarioId, s.NumeroPruebas, s.TopLeft, s.TopRight, s.BottomLeft, s.BottomRight, s.COP_X, s.COP_Y, s.Total, s.TimeStamp);
                    }

                    using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
                    {
                        bulk.DestinationTableName = "LecturasBalanceBoard";

                        // 🔗 Mapeos explícitos
                        bulk.ColumnMappings.Add("UsuarioId", "UsuarioId");
                        bulk.ColumnMappings.Add("NumeroPruebas", "NumeroPruebas");
                        bulk.ColumnMappings.Add("TopLeft", "TopLeft");
                        bulk.ColumnMappings.Add("TopRight", "TopRight");
                        bulk.ColumnMappings.Add("BottomLeft", "BottomLeft");
                        bulk.ColumnMappings.Add("BottomRight", "BottomRight");
                        bulk.ColumnMappings.Add("COP_X", "COP_X");
                        bulk.ColumnMappings.Add("COP_Y", "COP_Y");
                        bulk.ColumnMappings.Add("Total", "Total");
                        bulk.ColumnMappings.Add("TimeStamp", "TimeStamp");

                        bulk.WriteToServer(table);
                    }

                    Console.WriteLine($"💾 Insertadas {copy.Count} lecturas en la base de datos ({DateTime.Now:HH:mm:ss})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en inserción por lotes: {ex.Message}");
            }
        }
    }
}