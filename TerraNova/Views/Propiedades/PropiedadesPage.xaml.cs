using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class PropiedadesPage : PaginaBase
    {
        public PropiedadesPage(PropiedadesViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
