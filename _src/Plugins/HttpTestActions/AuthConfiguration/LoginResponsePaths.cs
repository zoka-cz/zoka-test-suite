using System;
using System.Collections.Generic;
using System.Text;

namespace Zoka.TestSuite.HttpTestActions.AuthConfiguration
{
	internal class LoginResponsePaths
	{
		public string?										TokenPath { get; set; }
		public string?										TokenExpirationPath { get; set; }
		public string?										RefreshTokenPath { get; set; }
		public string?										RefreshTokenExpirationPath { get; set; }
	}
}
