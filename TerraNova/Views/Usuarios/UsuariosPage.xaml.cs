using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class UsuariosPage : PaginaBase
    {
        public UsuariosPage(UsuariosViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
