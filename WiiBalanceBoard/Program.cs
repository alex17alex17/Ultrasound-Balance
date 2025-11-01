using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using WiimoteLib;

namespace WiiBalanceBoard
{
    class Program
    {
        // ⚙️ Configuración
        static string connectionString = "Server=PP-WALL-E\\SQLEXPRESS;Database=Ultrasound;Trusted_Connection=True;";
        static List<BalanceSample> buffer = new List<BalanceSample>();
        static object lockObj = new object();
        static Timer batchTimer;

        static void Main()
        {
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

            float TL = s.TopLeft;
            float TR = s.TopRight;
            float BL = s.BottomLeft;
            float BR = s.BottomRight;
            float total = state.WeightKg;

            if (total < 0.01f)
                return;

            var sample = new BalanceSample
            {
                TimeStamp = DateTime.Now,
                TopLeft = TL,
                TopRight = TR,
                BottomLeft = BL,
                BottomRight = BR,
                Total = total
            };

            lock (lockObj)
            {
                buffer.Add(sample);
            }
        }

        private static void FlushBufferToDatabase()
        {
            List<BalanceSample> copy;

            lock (lockObj)
            {
                if (buffer.Count == 0)
                    return;

                copy = new List<BalanceSample>(buffer);
                buffer.Clear();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Crea un DataTable temporal para usar SqlBulkCopy
                    DataTable table = new DataTable();
                    table.Columns.Add("TimeStamp", typeof(DateTime));
                    table.Columns.Add("TopLeft", typeof(float));
                    table.Columns.Add("TopRight", typeof(float));
                    table.Columns.Add("BottomLeft", typeof(float));
                    table.Columns.Add("BottomRight", typeof(float));
                    table.Columns.Add("Total", typeof(float));

                    foreach (var s in copy)
                    {
                        table.Rows.Add(s.TimeStamp, s.TopLeft, s.TopRight, s.BottomLeft, s.BottomRight, s.Total);
                    }

                    using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
                    {
                        bulk.DestinationTableName = "BalanceBoardReadings";

                        // 🔗 Mapeos explícitos
                        bulk.ColumnMappings.Add("TimeStamp", "TimeStamp");
                        bulk.ColumnMappings.Add("TopLeft", "TopLeft");
                        bulk.ColumnMappings.Add("TopRight", "TopRight");
                        bulk.ColumnMappings.Add("BottomLeft", "BottomLeft");
                        bulk.ColumnMappings.Add("BottomRight", "BottomRight");
                        bulk.ColumnMappings.Add("Total", "Total");

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

    class BalanceSample
    {
        public DateTime TimeStamp { get; set; }
        public float TopLeft { get; set; }
        public float TopRight { get; set; }
        public float BottomLeft { get; set; }
        public float BottomRight { get; set; }
        public float Total { get; set; }
    }
}