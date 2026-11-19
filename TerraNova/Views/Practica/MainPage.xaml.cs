using CommunityToolkit.Maui.Alerts;   // Toast
using CommunityToolkit.Maui.Core;     // ToastDuration

namespace TerraNova
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        struct TDPropiedad
        {
            public string Titulo;
            public string Zona;
            public string Tipo;
            public string Operacion;
            public DateTime Fecha;
            public int Precio;
            public int Area;
        }
        TDPropiedad[] VPropiedades = new TDPropiedad[20];
        int Vis = -1;

        private async void Procesar(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Titulo.Text))
            {
                await DisplayAlertAsync("Campo obligatorio", "Debe ingresar el titulo", "OK");
                Titulo.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(Zona.Text))
            {
                await DisplayAlertAsync("Campo obligatorio", "Debe ingresar la zona", "OK");
                Zona.Focus();
                return;
            }
            if (Tipo.SelectedItem == null)
            {
                await DisplayAlertAsync("Campo obligatorio", "Debe elegir un tipo de propiedad", "OK");
                return;
            }
            // Inv: el NumericValidationBehavior nos dice si el numero es valido
            if (!ValidaPrecio.IsValid || string.IsNullOrWhiteSpace(Precio.Text))
            {
                await DisplayAlertAsync("Dato invalido", "El precio debe ser un numero entero mayor a 0", "OK");
                Precio.Focus();
                return;
            }
            if (!ValidaArea.IsValid || string.IsNullOrWhiteSpace(Area.Text))
            {
                await DisplayAlertAsync("Dato invalido", "El area debe ser un numero entero mayor a 0", "OK");
                Area.Focus();
                return;
            }
            if (Vis >= VPropiedades.Length - 1)
            {
                await DisplayAlertAsync("Vector lleno", "No se pueden registrar mas propiedades", "OK");
                return;
            }
            GuardarVector();
            MostrarVector();

            // Inv: Toast, mensaje que aparece y desaparece solo
            var Aviso = Toast.Make($"Propiedad '{Titulo.Text}' registrada ({Vis + 1} en total)", ToastDuration.Short, 16);
            await Aviso.Show();

            Limpiar();
        }
        private void GuardarVector()
        {
            Vis++;
            VPropiedades[Vis].Titulo = Titulo.Text;
            VPropiedades[Vis].Zona = Zona.Text;
            VPropiedades[Vis].Tipo = Tipo.SelectedItem!.ToString()!;
            VPropiedades[Vis].Operacion = Rb1.IsChecked ? "VENTA" : "ALQUILER";
            VPropiedades[Vis].Fecha = Fecha.Date ?? DateTime.Today;
            VPropiedades[Vis].Precio = Convert.ToInt32(Precio.Text);
            VPropiedades[Vis].Area = Convert.ToInt32(Area.Text);
        }
        private void MostrarVector()
        {
            for (int V = 0; V <= Vis; V++)
            {
                Dgv.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                string[] Datos =
                {
                    VPropiedades[V].Titulo,
                    VPropiedades[V].Zona,
                    VPropiedades[V].Tipo,
                    VPropiedades[V].Operacion,
                    VPropiedades[V].Fecha.ToShortDateString(),
                    VPropiedades[V].Precio.ToString(),
                    VPropiedades[V].Area.ToString()
                };
                for (int V2 = 0; V2 < Datos.Length; V2++)
                {
                    var Lbl = new Label
                    {
                        Text = Datos[V2],
                        TextColor = Colors.Black,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                    Dgv.Add(Lbl, V2, V + 1);
                }
            }
        }
        private void Limpiar()
        {
            Titulo.Text = "";
            Zona.Text = "";
            Tipo.SelectedIndex = -1;
            Rb1.IsChecked = true;
            Precio.Text = "";
            Area.Text = "";
            Titulo.Focus();
        }
    }
}
