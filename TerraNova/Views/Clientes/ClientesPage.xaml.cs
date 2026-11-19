using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class ClientesPage : PaginaBase
    {
        public ClientesPage(ClientesViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
