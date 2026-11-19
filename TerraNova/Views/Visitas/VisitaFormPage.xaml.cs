using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class VisitaFormPage : PaginaBase
    {
        public VisitaFormPage(VisitaFormViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
