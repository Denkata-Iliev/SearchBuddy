namespace ImageBasedSearch.Models
{
	public class User
	{
		public Guid Id { get; set; }

		public string Email { get; set; } = string.Empty;

		public string IndexName { get => $"user_{Id}_album"; }

		public string ApiKey { get; set; } = string.Empty;
	}
}
