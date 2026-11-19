using TerraNova.Shared;

namespace TerraNova.Models
{
    // Todo lo que muestra la pantalla de Inicio. Cambia segun el rol:
    // Admin ve los totales de la empresa; un Agente ve solo SUS datos.
    public class ResumenDashboard
    {
        public bool EsAgente { get; set; }
        public string Subtitulo { get; set; } = string.Empty;
        public List<Indicador> Indicadores { get; set; } = new();

        // Graficos (JSON de opciones de ECharts)
        public string TituloGraficoEstados { get; set; } = string.Empty;
        public string GraficoVentas { get; set; } = string.Empty;
        public string GraficoEstados { get; set; } = string.Empty;
        public string GraficoAgentes { get; set; } = string.Empty;   // solo Admin

        public List<PropiedadDTO> UltimasPropiedades { get; set; } = new();
        public List<ActividadReciente> ActividadReciente { get; set; } = new();
        public List<VisitaDTO> ProximasVisitas { get; set; } = new();
    }
}
