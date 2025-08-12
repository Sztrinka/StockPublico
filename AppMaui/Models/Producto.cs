using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppMaui.Models;
//public class Producto               PONE ESTO MISIO DALE GRACIAS
//{
//    public string Nombre { get; set; }
//    public int Cantidad { get; set; }     // Día
//    public int Cantidad2 { get; set; }    // LPZ
//    public int Movil1 { get; set; }
//    public int U2 { get; set; }
//    public int U3 { get; set; }
//    public string Observaciones { get; set; }

//    public int Total => Cantidad + Cantidad2 + U1 + U2 + U3;
//}
public class Producto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }
    [JsonPropertyName("observacion")]
    public string Observacion { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stock")]
    public List<Stock> Stock { get; set; }

}

public static class ExtensionesProducto
{
    public static int GetStock(this Producto producto, UbicacionStock deposito, Propietario propietario)
    {
        var depositoId = (int)deposito;
        if (producto.Stock is not null && producto.Stock.Any(x => x.Deposito_id == depositoId))
        {
            if(propietario == Propietario.Dia)
                return producto.Stock.FirstOrDefault(x => x.Deposito_id == depositoId)?.CantidadDia ?? 0;
            if (propietario == Propietario.Lpz)
                return producto.Stock.FirstOrDefault(x => x.Deposito_id == depositoId)?.CantidadLpz ?? 0;
        }
        return 0;
    }
}

public enum Propietario
{
    Dia,
    Lpz
}

public enum UbicacionStock
{
    Taller = 1,
    Movil1 = 2,
    Movil2 = 3,
    Movil3 = 4
}