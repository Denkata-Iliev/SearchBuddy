﻿using Microsoft.AspNetCore.Identity;

namespace ImageBasedSearch.Database
{
	public class User : IdentityUser
	{
		public string IndexName { get => $"user_{Id}_album"; }

		public string ApiKey { get; set; } = string.Empty;
	}
}
