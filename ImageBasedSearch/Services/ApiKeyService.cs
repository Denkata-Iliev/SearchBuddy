using ImageBasedSearch.Services.Contracts;
using System.Security.Cryptography;

namespace ImageBasedSearch.Services
{
	public class ApiKeyService : IApiKeyService
	{
		private const string _prefix = "CT-";
		private const int _numberOfSecureBytesToGenerate = 64;
		private const int _lengthOfKey = 32;

		public string GenerateApiKey()
		{
			var bytes = RandomNumberGenerator.GetBytes(_numberOfSecureBytesToGenerate);

			string base64String = Convert.ToBase64String(bytes)
				.Replace("+", "-")
				.Replace("/", "_");

			var keyLength = _lengthOfKey - _prefix.Length;

			return _prefix + base64String[..keyLength];
		}
	}
}
