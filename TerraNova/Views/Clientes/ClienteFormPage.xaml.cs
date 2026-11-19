using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class ClienteFormPage : PaginaBase
    {
        public ClienteFormPage(ClienteFormViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
