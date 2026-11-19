using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class VisitasPage : PaginaBase
    {
        public VisitasPage(VisitasViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
