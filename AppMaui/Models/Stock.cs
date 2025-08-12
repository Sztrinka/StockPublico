using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppMaui.Models;

public class Stock
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("producto_id")]
    public int Producto_id { get; set; }
    [JsonPropertyName("deposito_id")]
    public int Deposito_id { get; set; }
    [JsonPropertyName("cantidad_dia")]
    public int CantidadDia { get; set; }
    [JsonPropertyName("cantidad_lpz")]
    public int CantidadLpz { get; set; }
}
