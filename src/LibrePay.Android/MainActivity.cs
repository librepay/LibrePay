using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using Plugin.Iconize;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace LibrePay.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);
            Iconize.Init(Resource.Id.toolbar, Resource.Id.sliding_tabs);
            LoadApplication(new App(ConfigDI));
        }

        private void ConfigDI(ContainerBuilder cb)
        {
            cb.RegisterAssemblyTypes(typeof(MainActivity).Assembly)
                .InNamespace(nameof(LibrePay) + "." + nameof(Droid) + "." + nameof(Services))
                .AsImplementedInterfaces()
                //TODO: Check this lifecycle
                .SingleInstance();
        }
    }
}
