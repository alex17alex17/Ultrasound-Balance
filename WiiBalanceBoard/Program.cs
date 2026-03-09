using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
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
        private const int CuentaRegresivaInicialSegundos = 10; // segundos
        private const int DuracionCapturaSegundos = 32; // segundos
        private static bool Capturar;
        private static Wiimote wm;


        private static void Main()
        {
            userInterface.RegistrarUsuario();

            MostrarCuentaRegresiva();;

            wm = new Wiimote();
            wm.WiimoteChanged += OnWiimoteChanged;
            wm.Connect();

            System.Threading.Thread.Sleep(500);

            if (wm.WiimoteState.ExtensionType == ExtensionType.BalanceBoard)
                Console.WriteLine("✅ Balance Board detectada. Guardando lecturas por lotes cada 1s...");
            else
            {
                Console.WriteLine("⚠️ No se detectó una Balance Board.");
                wm.WiimoteChanged -= OnWiimoteChanged;
                wm.Disconnect();
                return;
            }

            Capturar = true;

            // 🔄 Timer que guarda los datos cada 1 segundo
            batchTimer = new Timer(1000);
            batchTimer.AutoReset = true;
            batchTimer.Elapsed += OnBatchTimerElapsed;
            batchTimer.Start();

            ControlDeTiempoParaFinalizarPrograma();

            //Console.WriteLine("Presiona ENTER para salir...");
            //Console.ReadLine();

            FinalizarCapturaLimpia();
            //batchTimer.Stop();
            //FlushBufferToDatabase(); // guarda lo que quede pendiente
            //wm.Disconnect();
            
            EjecutarPython(userInterface.Usuario.Id.Value);
        }

        private static void OnBatchTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Capturar) return;
            FlushBufferToDatabase();
        }

        private static void FinalizarCapturaLimpia()
        {
            Console.WriteLine("🛑 Tiempo finalizado. Cerrando captura...");

            Capturar = false;

            if (batchTimer != null)
            {
                batchTimer.Stop();
                batchTimer.Elapsed -= OnBatchTimerElapsed;
                batchTimer.Dispose();
                batchTimer = null;
            }

            if (wm != null)
            {
                wm.WiimoteChanged -= OnWiimoteChanged;
            }

            FlushBufferToDatabase(); // último lote

            if (wm != null)
            {
                try { wm.Disconnect(); }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error al desconectar Wiimote: {ex.Message}");
                }
            }
        }

        private static void OnWiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            if (!Capturar)
                return;

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
            if (!Capturar)
                return;

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

        public static void MostrarCuentaRegresiva()
        {
            // ⏳ Cuenta regresiva de 10 segundos antes de iniciar
            Console.WriteLine("⏳ Preparándose para iniciar en 10 segundos...");

            for (int i = CuentaRegresivaInicialSegundos; i > 0; i--)
            {
                Console.WriteLine($"{i}...");
                System.Threading.Thread.Sleep(1000);
            }

            Console.WriteLine("🚀 ¡Inicio de la captura de datos!");
        }

        public static void ControlDeTiempoParaFinalizarPrograma()
        {
            // ⏱️ Registrar tiempo inicial
            DateTime inicio = DateTime.Now;

            Console.WriteLine("⌛ Capturando datos durante 30 segundos...");

            // 🛑 Mantener programa activo solo durante 30 segundos
            while ((DateTime.Now - inicio).TotalSeconds < DuracionCapturaSegundos)
            {
                System.Threading.Thread.Sleep(1000); // para no consumir CPU
            }

            Console.WriteLine("🛑 Tiempo finalizado. Cerrando captura...");
        }

        public static int EjecutarPython(double numero)
        {
            var baseDir = AppContext.BaseDirectory;
            var candidateScript = Path.GetFullPath(
                Path.Combine(baseDir, "..", "..", "ScriptsPython", "Main.py")
            );

            string scriptPath = candidateScript;

            if (!File.Exists(scriptPath))
            {
                if (userInterface != null && !string.IsNullOrWhiteSpace(userInterface.PathMainPythonScript) && File.Exists(userInterface.PathMainPythonScript))
                    scriptPath = userInterface.PathMainPythonScript;
                else
                {
                    Console.WriteLine($"[EjecutarPython] Script no encontrado. Buscado: '{candidateScript}' y en userInterface.PathMainPythonScript.");
                    throw new FileNotFoundException($"No se encontró el script de Python. Buscado en: '{candidateScript}'");
                }
            }

            var workingDir = Path.GetDirectoryName(scriptPath) ?? baseDir;
            if (!Directory.Exists(workingDir))
            {
                Console.WriteLine($"[EjecutarPython] WorkingDirectory inexistente: '{workingDir}'. Usando baseDir '{baseDir}'.");
                workingDir = baseDir;
            }

            // Localizar python.exe en PATH (opcional)
            string TryFindInPath(string exeName)
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (string.IsNullOrWhiteSpace(pathEnv)) return null;
                foreach (var part in pathEnv.Split(';'))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(part)) continue;
                        var candidate = Path.Combine(part.Trim(), exeName);
                        if (File.Exists(candidate)) return candidate;
                    }
                    catch { }
                }
                return null;
            }

            var pythonExe = TryFindInPath("python.exe") ?? TryFindInPath("python") ?? "python";

            Console.WriteLine($"[EjecutarPython] Ejecutando: '{pythonExe}' \"{scriptPath}\" {numero}");
            Console.WriteLine($"[EjecutarPython] WorkingDirectory: '{workingDir}'");

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\" {numero}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            };

            // Forzar UTF-8 para la comunicación con el proceso Python
            try
            {
                psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            }
            catch { /* no crítico */ }

            // Asegurar que .NET decodifique como UTF-8
            try
            {
                psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
                psi.StandardErrorEncoding = System.Text.Encoding.UTF8;
            }
            catch { /* disponible en .NET Framework 4.8; si no, el env var ayuda */ }

            var sbOut = new System.Text.StringBuilder();
            var sbErr = new System.Text.StringBuilder();

            using (var p = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                p.OutputDataReceived += (s, e) => { if (e.Data != null) { sbOut.AppendLine(e.Data); Console.WriteLine(e.Data); } };
                p.ErrorDataReceived += (s, e) => { if (e.Data != null) { sbErr.AppendLine(e.Data); Console.Error.WriteLine(e.Data); } };

                if (!p.Start())
                    throw new InvalidOperationException("No se pudo iniciar el proceso Python.");

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();

                var exit = p.ExitCode;
                Console.WriteLine($"[EjecutarPython] ExitCode: {exit}");
                if (!string.IsNullOrWhiteSpace(sbErr.ToString()))
                    Console.WriteLine($"[EjecutarPython] STDERR: {sbErr}");
                return exit;
            }
        }
    }
}