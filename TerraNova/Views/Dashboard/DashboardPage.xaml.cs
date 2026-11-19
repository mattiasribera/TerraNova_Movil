using TerraNova.ViewModels;

namespace TerraNova.Views
{
    public partial class DashboardPage : PaginaBase
    {
        public DashboardPage(DashboardViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
