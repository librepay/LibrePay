using System.Reflection;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Xunit.Sdk;
using Xunit.Runners.UI;

namespace LibrePay.UnitTests.Runner
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            AddTestAssembly(Assembly.GetExecutingAssembly());

            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            // or in any reference assemblies

            AddTestAssembly(typeof(FakeData).Assembly);
            // or in any assembly that you load (since JIT is available)

            base.OnCreate(bundle);
        }
    }
}
