namespace Citire_rapida
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Înregistrezi pagina ca să poată fi găsită prin text
            Routing.RegisterRoute(nameof(ReadPage), typeof(ReadPage));
        }
    }
}
