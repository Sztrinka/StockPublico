using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppMaui.Models;

public class ProductoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
    public int Cantidad1 { get; set; }
    public int Cantidad2 { get; set; }
    public int M1 { get; set; }
    public int M2 { get; set; }
    public int M3 { get; set; }

    public int Total => Cantidad1 + Cantidad2 + M1 + M2 + M3;
    public int CantidadAMostrar { get; set; }
}
