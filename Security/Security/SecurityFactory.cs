using System;
using System.Collections.Generic;
using System.Text;

namespace Security
{
	public static class SecurityFactory
	{
		public static ISecurityManager GetSecurity = new SecurityAdapter();
	}
}
