using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class TrabajadoresPage : PaginaBase
    {
        public TrabajadoresPage(TrabajadoresViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
