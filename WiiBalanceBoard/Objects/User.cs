using System;

namespace WiiBalanceBoard.Objects
{
    public class User
    {
        public int? Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int? Edad { get; set; }
        public string Genero { get; set; }
        public float? Altura { get; set; }
        public float? Peso { get; set; }
        public int NumeroPruebas { get; set; } = 0;
        public string TipoEfecto { get; set; }
        public string Descripcion { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}