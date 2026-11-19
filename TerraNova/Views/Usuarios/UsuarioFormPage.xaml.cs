using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class UsuarioFormPage : PaginaBase
    {
        public UsuarioFormPage(UsuarioFormViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
