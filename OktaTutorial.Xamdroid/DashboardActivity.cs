using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using OktaTutorial.Xamdroid.Extensions;

namespace OktaTutorial.Xamdroid
{
	[Activity(Label = "DashboardActivity")]
	public class DashboardActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_dashboard);

			string name = Intent.GetNameExtra();
			TextView welcomeMessage = FindViewById<TextView>(Resource.Id.welcome_message);
			welcomeMessage.Text = Resources.GetString(Resource.String.welcome_message_format, name);
		}
	}
}