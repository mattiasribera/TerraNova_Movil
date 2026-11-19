using TerraNova.ViewModels;

namespace TerraNova.Views
{
    // Todas las paginas heredan de esta: al mostrarse, le piden al ViewModel que cargue sus datos
    // (equivalente a OnInitializedAsync de Blazor)
    public class PaginaBase : ContentPage
    {
        public PaginaBase()
        {
            BackgroundColor = Color.FromArgb("#F4F1EA");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ICargable Vm)
                await Vm.Cargar();
        }
    }
}
