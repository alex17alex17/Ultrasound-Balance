using System;
using WiimoteLib;

namespace WiiBalanceBoard
{
    internal class Program
    {
        private static void Main()
        {
            var wm = new Wiimote();

            wm.WiimoteChanged += OnWiimoteChanged;
            wm.Connect();

            // Esperamos unos milisegundos para que la conexión inicialice correctamente
            System.Threading.Thread.Sleep(500);

            if (wm.WiimoteState.ExtensionType == ExtensionType.BalanceBoard)
            {
                Console.WriteLine("✅ Balance Board detectada correctamente");
            }
            else
            {
                Console.WriteLine($"⚠️ No se detectó una Balance Board (tipo detectado: {wm.WiimoteState.ExtensionType})");
                return;
            }

            Console.WriteLine("Presiona ENTER para salir...");
            Console.ReadLine();

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
                return; // ignora lecturas vacías

            Console.WriteLine($"TL:{TL:F2}  TR:{TR:F2}  BL:{BL:F2}  BR:{BR:F2}  Total:{total:F2}");
        }
    }
}