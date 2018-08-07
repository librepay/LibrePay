using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace BitcoinPOS_App.Droid
{
    [Activity(Label = "BitcoinPOS_App", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);
            LoadApplication(new App(ConfigDI));
        }

        private void ConfigDI(ContainerBuilder cb)
        {
            cb.RegisterAssemblyTypes(typeof(MainActivity).Assembly)
                .InNamespace(nameof(BitcoinPOS_App) + "." + nameof(Droid) + "." + nameof(Services))
                .AsImplementedInterfaces()
                //TODO: Check this lifecycle
                .SingleInstance();
        }
    }
}