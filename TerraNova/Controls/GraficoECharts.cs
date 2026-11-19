namespace TerraNova.Controls
{
    // Control reutilizable que muestra un grafico de ECharts.
    // Uso en XAML:  <controls:GraficoECharts Opciones="{Binding Datos.GraficoVentas}" HeightRequest="300"/>
    // "Opciones" es el JSON de opciones de ECharts (ver Helpers/OpcionesGraficos.cs).
    //
    // Por dentro usa HybridWebView (MAUI 9+): carga Resources/Raw/graficos/index.html,
    // espera el aviso "listo" de la pagina y le envia los datos con JavaScript.
    public class GraficoECharts : ContentView
    {
        public static readonly BindableProperty OpcionesProperty = BindableProperty.Create(
            nameof(Opciones), typeof(string), typeof(GraficoECharts), null,
            propertyChanged: (Control, _, _) => ((GraficoECharts)Control).Dibujar());

        public string? Opciones
        {
            get => (string?)GetValue(OpcionesProperty);
            set => SetValue(OpcionesProperty, value);
        }

        private readonly HybridWebView Web;
        private bool PaginaLista;

        public GraficoECharts()
        {
            Web = new HybridWebView
            {
                HybridRoot = "graficos",
                DefaultFile = "index.html",
                BackgroundColor = Colors.Transparent
            };
            Web.RawMessageReceived += (_, Evento) =>
            {
                if (Evento.Message == "listo")
                {
                    PaginaLista = true;
                    Dibujar();
                }
            };
            Content = Web;
        }

        private void Dibujar()
        {
            if (!PaginaLista || string.IsNullOrEmpty(Opciones)) return;
            var Script = $"dibujar({Opciones})";
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try { await Web.EvaluateJavaScriptAsync(Script); }
                catch { /* si la pagina se cerro mientras se dibujaba, no pasa nada */ }
            });
        }
    }
}
