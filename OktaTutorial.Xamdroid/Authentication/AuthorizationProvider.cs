using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using OktaTutorial.Xamdroid.Extensions;
using OktaTutorial.Xamdroid.Model;
using OpenId.AppAuth;

namespace OktaTutorial.Xamdroid.Authentication
{
	public class AuthorizationProvider
	{
		private static readonly Uri ConfigurationEndpoint =
			Uri.Parse($"{Configuration.OrgUrl}/oauth2/default/.well-known/openid-configuration");

		private static readonly string[] Scopes = new[] { "openid", "profile", "email", "offline_access" };

		private AuthorizationRequest authorizationRequest;

		private readonly AuthorizationService authorizationService;

		private AuthState authorizationState;

		private readonly Context context;

		private PendingIntent completedIntent;

		private readonly TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

		public AuthorizationProvider(Context context)
		{
			authorizationService = new AuthorizationService(context);
			this.context = context;
		}

		public async Task<AuthorizationResult> SignInAsync()
		{
			AuthorizationServiceConfiguration serviceConfiguration = await AuthorizationServiceConfiguration.FetchFromUrlAsync(ConfigurationEndpoint);
			BuildAuthorizationRequest(serviceConfiguration);
			BuildCompletedIntent(serviceConfiguration);
			return await RequestAuthorization();
		}

		public void NotifyCallback(Intent intent)
		{
			if (!intent.TryGetAuthStateFromExtra(out authorizationState))
			{
				taskCompletionSource.SetResult(false);
				return;
			}

			AuthorizationResponse authorizationResponse = AuthorizationResponse.FromIntent(intent);
			AuthorizationException authorizationException = AuthorizationException.FromIntent(intent);

			authorizationState.Update(authorizationResponse, authorizationException);

			if (authorizationException != null)
			{
				taskCompletionSource.SetResult(false);
				return;
			}

			authorizationService.PerformTokenRequest(authorizationResponse.CreateTokenExchangeRequest(), ReceivedTokenResponse);
		}

		private void BuildAuthorizationRequest(AuthorizationServiceConfiguration serviceConfiguration)
		{
			AuthorizationRequest.Builder builder = new AuthorizationRequest.Builder(
				serviceConfiguration,
				Configuration.ClientId,
				ResponseTypeValues.Code,
				Uri.Parse(Configuration.LoginRedirectUri));

			builder.SetScope(string.Join(" ", Scopes));

			authorizationRequest = builder.Build();
		}

		private void BuildCompletedIntent(AuthorizationServiceConfiguration serviceConfiguration)
		{
			Intent intent = new Intent(context, typeof(MainActivity));
			intent.PutAuthStateExtra(new AuthState());

			completedIntent = PendingIntent.GetActivity(context, authorizationRequest.GetHashCode(), intent, 0);
		}

		private async Task<AuthorizationResult> RequestAuthorization()
		{
			authorizationService.PerformAuthorizationRequest(authorizationRequest, completedIntent);

			await taskCompletionSource.Task;

			return new AuthorizationResult()
			{
				AccessToken = authorizationState?.AccessToken,
				IdentityToken = authorizationState?.IdToken,
				IsAuthorized = authorizationState?.IsAuthorized ?? false,
				RefreshToken = authorizationState?.RefreshToken,
				Scope = authorizationState?.Scope
			};
		}

		private void ReceivedTokenResponse(TokenResponse tokenResponse, AuthorizationException authorizationException)
		{
			authorizationState.Update(tokenResponse, authorizationException);
			taskCompletionSource.SetResult(true);
		}
	}
}