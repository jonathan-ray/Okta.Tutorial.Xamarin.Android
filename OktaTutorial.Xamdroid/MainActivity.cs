using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using OktaTutorial.Xamdroid.Authentication;
using OktaTutorial.Xamdroid.Extensions;
using OktaTutorial.Xamdroid.Model;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace OktaTutorial.Xamdroid
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, LaunchMode = LaunchMode.SingleTask)]
	public class MainActivity : AppCompatActivity
	{
		private static AuthorizationProvider authorizationProvider;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			authorizationProvider = new AuthorizationProvider(this);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu_main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			int id = item.ItemId;
			if (id == Resource.Id.action_settings)
			{
				return true;
			}
			if (id == Resource.Id.action_signin)
			{
				OnSignInAttempted();
				return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);

			if (intent != null && intent.Data.Path.Equals("/callback", StringComparison.OrdinalIgnoreCase))
			{
				authorizationProvider.NotifyCallback(intent);
			}
		}

		private async void OnSignInAttempted()
		{
			ProgressBar signInProgress = FindViewById<ProgressBar>(Resource.Id.signin_progress);
			signInProgress.Visibility = ViewStates.Visible;

			AuthorizationResult authorizationResult = await authorizationProvider.SignInAsync();

			if (!string.IsNullOrWhiteSpace(authorizationResult.AccessToken) && authorizationResult.IsAuthorized)
			{
				// Retrieve the user's name from the JSON Web Token
				JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
				JwtSecurityToken jsonToken = tokenHandler.ReadJwtToken(authorizationResult.IdentityToken);
				IEnumerable<Claim> claims = jsonToken?.Payload?.Claims;
				string name = claims?.FirstOrDefault(x => x.Type == "name")?.Value;

				// Navigate to Dashboard activity
				Intent dashboardIntent = new Intent(this, typeof(DashboardActivity));
				dashboardIntent.PutNameExtra(name);
				StartActivity(dashboardIntent);
				Finish();
			}
			else
			{
				// Display an error to the user
				AlertDialog authorizationErrorDialog = new AlertDialog.Builder(this)
					.SetTitle("Error!")
					.SetMessage("We were unable to authorize you.")
					.Create();

				authorizationErrorDialog.Show();
				signInProgress.Visibility = ViewStates.Gone;
			}
		}
	}
}