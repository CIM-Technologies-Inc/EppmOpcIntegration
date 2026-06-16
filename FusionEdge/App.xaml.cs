using FusionEdge.Components.Services;

namespace FusionEdge
{
    public partial class App : Application
    {
        //public App()
        //{
        //    InitializeComponent();
        //}

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new MainPage()) { Title = "FusionEdge" };
        //}
        public App(ScheduleRunnerService scheduler)
        {
            InitializeComponent();
       
            scheduler.Start();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            //return new Window(new MainPage())
            //{
            //    Title = "FusionEdge"
            //};
            var window = new Window(new MainPage())
            {
                Title = "FusionEdge"
            };

            window.Width = 1000;
            window.Height = 800;

            return window;
        }
    }
}
