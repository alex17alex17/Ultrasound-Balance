using System;

namespace WiiBalanceBoard.Objects
{
    public class LecturaBalance
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public int NumeroPruebas { get; set; }
        public float TopLeft { get; set; }
        public float TopRight { get; set; }
        public float BottomLeft { get; set; }
        public float BottomRight { get; set; }
        public float COP_X { get; set; }
        public float COP_Y { get; set; }
        public float Total { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}