using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class VentaFormPage : PaginaBase
    {
        public VentaFormPage(VentaFormViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
