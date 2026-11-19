using TerraNova.Constantes;

namespace TerraNova.Controls
{
    // Campo de contraseña con boton "ojo" para mostrar u ocultar lo que se escribe.
    // Uso en XAML:  <controls:CampoContrasena Texto="{Binding Contrasena}" Placeholder="Contraseña"/>
    public class CampoContrasena : Grid
    {
        public static readonly BindableProperty TextoProperty = BindableProperty.Create(
            nameof(Texto), typeof(string), typeof(CampoContrasena), string.Empty, BindingMode.TwoWay);

        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
            nameof(Placeholder), typeof(string), typeof(CampoContrasena), string.Empty);

        public static readonly BindableProperty ComandoEnterProperty = BindableProperty.Create(
            nameof(ComandoEnter), typeof(System.Windows.Input.ICommand), typeof(CampoContrasena));

        public string Texto { get => (string)GetValue(TextoProperty); set => SetValue(TextoProperty, value); }
        public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
        public System.Windows.Input.ICommand? ComandoEnter { get => (System.Windows.Input.ICommand?)GetValue(ComandoEnterProperty); set => SetValue(ComandoEnterProperty, value); }

        public CampoContrasena()
        {
            ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto));
            ColumnSpacing = 4;

            var Campo = new Entry { IsPassword = true };
            Campo.SetBinding(Entry.TextProperty, new Binding(nameof(Texto), BindingMode.TwoWay, source: this));
            Campo.SetBinding(Entry.PlaceholderProperty, new Binding(nameof(Placeholder), source: this));
            Campo.SetBinding(Entry.ReturnCommandProperty, new Binding(nameof(ComandoEnter), source: this));

            var Ojo = new Button { Text = Iconos.Ver, VerticalOptions = LayoutOptions.Center };
            Ojo.SetDynamicResource(StyleProperty, "BotonIcono");
            ToolTipProperties.SetText(Ojo, "Mostrar contraseña");
            Ojo.Clicked += (_, _) =>
            {
                Campo.IsPassword = !Campo.IsPassword;
                Ojo.Text = Campo.IsPassword ? Iconos.Ver : Iconos.Ocultar;
                ToolTipProperties.SetText(Ojo, Campo.IsPassword ? "Mostrar contraseña" : "Ocultar contraseña");
            };

            Grid.SetColumn(Ojo, 1);
            Children.Add(Campo);
            Children.Add(Ojo);
        }
    }
}
