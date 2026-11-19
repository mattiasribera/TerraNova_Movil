using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class VentasPage : PaginaBase
    {
        public VentasPage(VentasViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
