using AppMaui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;

namespace AppMaui.Helpers;

public class BdService
{
    private readonly List<Producto> _productos = new List<Producto>
    {
        new Producto
        {
            Id = 1,
            Nombre = "Destornillador Plano",
            Observacion = "",
            Stock = new()
        },
        new Producto
        {
            Id = 2,
            Nombre = "Martillo",
            Observacion = "",
            Stock = new()
        },
        new Producto
        {
            Id = 3,
            Nombre = "Pinza Pico de Loro",
            Observacion = "",
            Stock = new()
        },
        new Producto
        {
            Id = 4,
            Nombre = "Llave Francesa",
            Observacion = "",
            Stock = new()
        },
    };
    private readonly HttpClient _httpClient;
    private readonly string _urlRest;
    private readonly string _urlFunctions;
    private readonly string _apiKey;
    public BdService()
    {
        _httpClient = new HttpClient();

    }

    public async Task<Producto> GetProductoAsync(int idProducto)
    {
        return _productos.FirstOrDefault(x => x.Id == idProducto);
    }

    public async Task<ResultadoProductos> GetProductosAsync(int paginaActual = -1, int itemsPorPagina = 15, string filtro = "", string orderBy = "", UbicacionStock ubicacionStock = 0)
    {
        

        if(ubicacionStock != 0) return await GetProductosPorDepositoPaginadosAsync(paginaActual, itemsPorPagina, filtro, orderBy, ubicacionStock);

        var productosFiltrados = _productos;
        if (!string.IsNullOrWhiteSpace(filtro))
            productosFiltrados = _productos.Where(x => x.Nombre.ToLower().Contains(filtro.ToLower())).ToList();
        return new ResultadoProductos
        {
            CantidadTotal = _productos.Count(),
            Productos = productosFiltrados
        };
    }

    public async Task<ResultadoProductos> GetProductosPorDepositoPaginadosAsync(int paginaActual = -1, int itemsPorPagina = 15, string filtro = "", string orderBy = "", UbicacionStock deposito = UbicacionStock.Taller)
    {
        var resultadoFinal = new ResultadoProductos();

        var productos = _productos.AsQueryable();
        if (!string.IsNullOrEmpty(filtro))
        {
            productos = productos.Where(x => x.Nombre.ToLower().Contains(filtro.ToLower()));
        }


        resultadoFinal.Productos = productos
            .Where(x => x.Stock.Any(s => s.Deposito_id == (int)deposito && (s.CantidadDia > 0 || s.CantidadLpz > 0)))
            .ToList();
        resultadoFinal.CantidadTotal = resultadoFinal.Productos.Count;
        return resultadoFinal;
    }

    public async Task<ResultadoProductos> GetProductosPorDepositoAsync(UbicacionStock deposito)
    {

        var resultadoFinal = new ResultadoProductos();

        resultadoFinal.Productos = _productos
            .Where(x => x.Stock.Any(s => s.Deposito_id == (int)deposito && (s.CantidadDia > 0 || s.CantidadLpz > 0)))
            .ToList();
        resultadoFinal.CantidadTotal = resultadoFinal.Productos.Count;
        return resultadoFinal;
    }

    public async Task<int> AgregarProductoAsync(Producto producto)
    {
        try
        {
            var ultimoId = _productos.Max(x => x.Id);
            producto.Id = ultimoId + 1;
            producto.Stock = new();
            _productos.Add(producto);
            return producto.Id;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    public async Task<bool> ActualizarProductoAsync(int idProducto, ProductoDTO productoModificado)
    {
        try
        {
            var productoAnterior = _productos.FirstOrDefault(x => x.Id == idProducto);
            if (productoAnterior == null) throw new Exception();

            productoAnterior.Nombre = productoModificado.Nombre;
            productoAnterior.Observacion = productoModificado.Observacion;

            return true;

        }
        catch (Exception)
        {
            return false;
        }
    }
    

    /// <summary>
    /// Elimina el producto con el id que pases
    /// </summary>
    /// <param name="idProducto">ID del producto a borrar</param>
    /// <returns></returns>
    public async Task<bool> BorrarProductoAsync(int idProducto)
    {
        try
        {
            var producto = _productos.FirstOrDefault(x => x.Id == idProducto);
            if (producto != null)
            {
                _productos.Remove(producto);
                return true;
            }
            else return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ActualizarStockAsync(int depositoId, int productoId, int cantidadDia, int cantidadLpz)
    {
        try
        {
            var producto = _productos.FirstOrDefault(x => x.Id == productoId);
            if (producto == null) return false;

            var stockDeposito = producto.Stock.FirstOrDefault(x => x.Deposito_id == depositoId);
            if (stockDeposito == null)
            {
                var nuevoDeposito = new Stock
                {
                    Deposito_id = depositoId,
                    CantidadDia = cantidadDia,
                    CantidadLpz = cantidadLpz
                };
                producto.Stock.Add(nuevoDeposito);
            } else
            {
                stockDeposito.CantidadDia = cantidadDia;
                stockDeposito.CantidadLpz = cantidadLpz;
            }

                return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<MovimientoStockError>> MoverStockAsync(List<MovimientoStock> movimientos)
    {
        foreach (var movimiento in movimientos)
        {
            var producto = _productos.FirstOrDefault(x => x.Id == movimiento.ProductoId);
            if (producto == null) continue;
            if (!producto.Stock.Any(x => x.Deposito_id == movimiento.DepositoOrigen))
            {
                var nuevoStock = new Stock
                {
                    Deposito_id = movimiento.DepositoOrigen,
                    CantidadDia = 0,
                    CantidadLpz = 0
                };
            }
            if (!producto.Stock.Any(x => x.Deposito_id == movimiento.DepositoDestino))
            {
                var nuevoStock = new Stock
                {
                    Deposito_id = movimiento.DepositoDestino,
                    CantidadDia = 0,
                    CantidadLpz = 0
                };
            }
            var stockOrigen = producto.Stock.FirstOrDefault(x => x.Deposito_id == movimiento.DepositoOrigen);
            var stockDestino = producto.Stock.FirstOrDefault(x => x.Deposito_id == movimiento.DepositoDestino);

            if (movimiento.Propietario == "dia")
            {
                stockOrigen.CantidadDia = stockOrigen.CantidadDia - movimiento.Cantidad;
                stockDestino.CantidadDia = stockDestino.CantidadDia + movimiento.Cantidad;
            }
            if (movimiento.Propietario == "lpz")
            {
                stockOrigen.CantidadLpz = stockOrigen.CantidadLpz - movimiento.Cantidad;
                stockDestino.CantidadLpz = stockDestino.CantidadLpz + movimiento.Cantidad;
            }
        }
        return new List<MovimientoStockError>();
    }

    public async Task<List<MovimientoStockError>> ConsumirProductoAsync(List<MovimientoStock> movimientos)
    {
        foreach (var movimiento in movimientos)
        {
            var producto = _productos.FirstOrDefault(x => x.Id == movimiento.ProductoId);
            if (producto == null) continue;
            if (!producto.Stock.Any(x => x.Deposito_id == movimiento.DepositoOrigen))
            {
                var nuevoStock = new Stock
                {
                    Deposito_id = movimiento.DepositoOrigen,
                    CantidadDia = 0,
                    CantidadLpz = 0
                };
            }

            var stockOrigen = producto.Stock.FirstOrDefault(x => x.Deposito_id == movimiento.DepositoOrigen);

            if (movimiento.Propietario == "dia")
            {
                stockOrigen.CantidadDia = stockOrigen.CantidadDia - movimiento.Cantidad;
            }
            if (movimiento.Propietario == "lpz")
            {
                stockOrigen.CantidadLpz = stockOrigen.CantidadLpz - movimiento.Cantidad;
            }

        }
        return new List<MovimientoStockError>();
    }



}

public class ResultadoProductos
{
    public List<Producto> Productos { get; set; } = new();
    public int CantidadTotal { get; set; }
}

public class MovimientoStockRequest
{
    [JsonPropertyName("movimientos")]
    public List<MovimientoStock> Movimientos { get; set; } = new();
}

public class MovimientoStock
{
    [JsonPropertyName("producto_id")]
    public int ProductoId { get; set; }
    [JsonPropertyName("from_deposito")]
    public int DepositoOrigen { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("to_deposito")]
    public int DepositoDestino { get; set; }
    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }
    [JsonPropertyName("propietario")]
    public string Propietario { get; set; }

    public MovimientoStock(int productoId, UbicacionStock depositoOrigen, UbicacionStock depositoDestino, int cantidad, Propietario propietario)
    {
        ProductoId = productoId;
        DepositoOrigen = (int)depositoOrigen;
        DepositoDestino = (int)depositoDestino;
        Cantidad = cantidad;
        Propietario = propietario == Models.Propietario.Dia ? "dia" : "lpz";
    }
}

public class MovimientoStockResponse
{
    [JsonPropertyName("errores")]
    public List<MovimientoStockError>? Errores { get; set; }
}

public class MovimientoStockError
{
    [JsonPropertyName("producto_id")]
    public int ProductoId { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}


public class ProductosConStock
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }
    [JsonPropertyName("observacion")]
    public string Observacion { get; set; }
    [JsonPropertyName("stock_id")]
    public int StockId { get; set; }
    [JsonPropertyName("deposito_id")]
    public int DepositoId { get; set; }
    [JsonPropertyName("cantidad_dia")]
    public int CantidadDia { get; set; }
    [JsonPropertyName("cantidad_lpz")]
    public int CantidadLpz { get; set; }
}