using System;
using System.Collections.Generic;

namespace Configit.Test.DTO
{
	public class PackageDependencie
	{
		public int NumberOfPackages	{ get; set; }
		public List<Package> Packages { get; set; }
		public int NumberOfDependencies { get; set; }
	}
}
