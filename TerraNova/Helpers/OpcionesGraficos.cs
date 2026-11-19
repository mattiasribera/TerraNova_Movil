using System.Globalization;
using System.Text.Json;
using TerraNova.Shared;

namespace TerraNova.Helpers
{
    // Arma las "opciones" de ECharts (JSON) a partir de los datos de la API.
    // Documentacion de opciones: https://echarts.apache.org/en/option.html
    public static class OpcionesGraficos
    {
        private static readonly CultureInfo Cultura = new("es-BO");

        private static readonly Dictionary<string, string> ColorEstado = new()
        {
            ["Disponible"] = "#198754", ["Reservada"] = "#FFC107", ["Vendida"] = "#DC3545", ["Alquilada"] = "#0D6EFD",
            ["Programada"] = "#0D6EFD", ["Realizada"] = "#198754", ["Cancelada"] = "#DC3545"
        };

        private static string Json(object Opciones) => JsonSerializer.Serialize(Opciones);

        // Barras (monto) + linea (cantidad) de los ultimos 12 meses
        public static string VentasPorMes(IEnumerable<VentaDTO> Ventas)
        {
            var Inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);
            var Meses = Enumerable.Range(0, 12).Select(I => Inicio.AddMonths(I)).ToList();
            var PorMes = Ventas.Where(V => V.Fecha >= Inicio)
                               .GroupBy(V => new DateTime(V.Fecha.Year, V.Fecha.Month, 1))
                               .ToDictionary(G => G.Key, G => (Monto: G.Sum(V => V.Monto), Cantidad: G.Count()));

            return Json(new
            {
                tooltip = new { trigger = "axis" },
                legend = new { data = new[] { "Monto ($)", "Operaciones" }, bottom = 0 },
                grid = new { left = 60, right = 40, top = 20, bottom = 50 },
                xAxis = new { type = "category", data = Meses.Select(M => M.ToString("MMM yy", Cultura)).ToArray() },
                yAxis = new object[]
                {
                    new { type = "value", name = "", axisLabel = new { formatter = "${value}" } },
                    new { type = "value", name = "", minInterval = 1, splitLine = new { show = false } }
                },
                series = new object[]
                {
                    new { name = "Monto ($)", type = "bar", barMaxWidth = 28, itemStyle = new { borderRadius = new[] { 6, 6, 0, 0 } },
                          data = Meses.Select(M => PorMes.TryGetValue(M, out var D) ? D.Monto : 0).ToArray() },
                    new { name = "Operaciones", type = "line", yAxisIndex = 1, smooth = true, symbolSize = 7,
                          itemStyle = new { color = "#B8860B" },
                          data = Meses.Select(M => PorMes.TryGetValue(M, out var D) ? D.Cantidad : 0).ToArray() }
                }
            });
        }

        // Dona con la cantidad por estado (propiedades o visitas)
        public static string PorEstado(IEnumerable<string> Estados)
        {
            var Datos = Estados.GroupBy(E => E).OrderByDescending(G => G.Count())
                .Select(G => new
                {
                    name = G.Key,
                    value = G.Count(),
                    itemStyle = new { color = ColorEstado.TryGetValue(G.Key, out var C) ? C : "#6C757D" }
                }).ToArray();

            return Json(new
            {
                tooltip = new { trigger = "item", formatter = "{b}: {c} ({d}%)" },
                legend = new { bottom = 0 },
                series = new object[]
                {
                    new { type = "pie", radius = new[] { "45%", "72%" }, center = new[] { "50%", "45%" },
                          itemStyle = new { borderRadius = 6, borderColor = "#fff", borderWidth = 2 },
                          label = new { formatter = "{c}" },
                          data = Datos }
                }
            });
        }

        // Barras horizontales: comisiones generadas por agente (solo Admin)
        public static string ComisionesPorAgente(IEnumerable<VentaDTO> Ventas)
        {
            var Top = Ventas.Where(V => V.IdTrabajadorNavigation != null)
                .GroupBy(V => V.IdTrabajadorNavigation!.NombreCompleto)
                .Select(G => new { Agente = G.Key, Total = G.Sum(V => V.ComisionGenerada) })
                .OrderByDescending(X => X.Total).Take(6).Reverse().ToList();

            return Json(new
            {
                tooltip = new { trigger = "axis", axisPointer = new { type = "shadow" } },
                grid = new { left = 120, right = 40, top = 10, bottom = 30 },
                xAxis = new { type = "value", axisLabel = new { formatter = "${value}" } },
                yAxis = new { type = "category", data = Top.Select(X => X.Agente).ToArray() },
                series = new object[]
                {
                    new { name = "Comisión", type = "bar", barMaxWidth = 22, itemStyle = new { color = "#B8860B", borderRadius = new[] { 0, 6, 6, 0 } },
                          label = new { show = true, position = "right", formatter = "${c}" },
                          data = Top.Select(X => Math.Round(X.Total, 0)).ToArray() }
                }
            });
        }
    }
}
