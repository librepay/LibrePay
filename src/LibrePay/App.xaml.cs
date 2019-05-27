using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Autofac;
using LibrePay.Interfaces.Providers;
using LibrePay.Interfaces.Services.Navigation;
using LibrePay.ViewModels;
using NBitcoin;
using Plugin.Iconize;
using Plugin.Iconize.Fonts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace LibrePay
{
    public partial class App
    {
        private readonly Action<ContainerBuilder> _configDI;
        public static ILifetimeScope Container { get; private set; }

        public const string Name = "LibrePay";

        public App(Action<ContainerBuilder> configDI)
        {
            _configDI = configDI ?? throw new ArgumentNullException(nameof(configDI));
            InitializeComponent();


            // Adds entropy to the random utility used by NBitcoin
            RandomUtils.AddEntropy($"{DateTime.Now.Ticks}-{Device.RuntimePlatform}");

            Iconize.With(new MaterialModule());

            Init();
        }

        internal void Init()
        {
            CreateDIContainer();

            var cultureInfo = Container.Resolve<CultureInfo>();
            CultureInfo.DefaultThreadCurrentCulture =
                CultureInfo.DefaultThreadCurrentUICulture =
                    CultureInfo.CurrentCulture =
                        CultureInfo.CurrentUICulture =
                            cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Container.Resolve<INavigationService>()
                .InitializeAsync<MainPageViewModel>();
        }

        private CultureInfo RegisterCulture(IComponentContext ctx)
        {
            var settingsProvider = ctx.Resolve<ISettingsProvider>();
            var cultureTag = settingsProvider.GetValueAsync<string>(SettingsKeys.CultureTag)
                .Result;

            if (string.IsNullOrWhiteSpace(cultureTag))
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                settingsProvider.SetValueAsync(SettingsKeys.CultureTag, currentCulture.IetfLanguageTag)
                    .Wait();
                return currentCulture;
            }

            return CultureInfo.GetCultureInfoByIetfLanguageTag(cultureTag);
        }

        private void CreateDIContainer()
        {
            Container?.Dispose();

            var cb = new ContainerBuilder();

            // register views
            cb.RegisterAssemblyTypes(typeof(App).Assembly)
                .InNamespace(nameof(LibrePay) + "." + nameof(Views))
                .AsSelf()
                .InstancePerLifetimeScope();
            // register view models
            cb.RegisterAssemblyTypes(typeof(App).Assembly)
                .InNamespace(nameof(LibrePay) + "." + nameof(ViewModels))
                .AsSelf()
                .InstancePerLifetimeScope();

            // register services
            cb.RegisterAssemblyTypes(typeof(App).Assembly)
                .InNamespace(nameof(LibrePay) + "." + nameof(Services))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            // register providers
            cb.RegisterAssemblyTypes(typeof(App).Assembly)
                .InNamespace(nameof(LibrePay) + "." + nameof(Providers))
                .AsImplementedInterfaces()
                .SingleInstance();

            // register culture
            cb.Register(RegisterCulture)
                .SingleInstance();

            // register platform specific
            _configDI(cb);

            Container = cb.Build().BeginLifetimeScope();
        }

        protected override void OnStart()
        {
            Debug.WriteLine("Starting...", "APP");
        }

        protected override void OnSleep()
        {
            Debug.WriteLine("Sleeping...", "APP");
        }

        protected override void OnResume()
        {
            Debug.WriteLine("Resuming...", "APP");
        }
    }
}
