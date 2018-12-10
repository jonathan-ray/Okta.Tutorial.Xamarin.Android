namespace OktaTutorial.Xamdroid.Model
{
	public class AuthorizationResult
	{
		public string AccessToken { get; set; }

		public string IdentityToken { get; set; }

		public bool IsAuthorized { get; set; }

		public string RefreshToken { get; set; }

		public string Scope { get; set; }
	}
}