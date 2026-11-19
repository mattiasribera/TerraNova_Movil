using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class TrabajadorFormPage : PaginaBase
    {
        public TrabajadorFormPage(TrabajadorFormViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
