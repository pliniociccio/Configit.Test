using System.Collections.Generic;

namespace Configit.Test.DTO
{
	public class Package
	{
		public string Name { get; set; }
		public string Version { get; set; }

		public List<Dependencie> Dependencies { get; set; }
	}
}
