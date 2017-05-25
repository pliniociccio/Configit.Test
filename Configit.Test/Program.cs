using Configit.Test.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Configit.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			StartsConfigitTest();

			Console.ReadLine();
		}

		/// <summary>
		/// Encapsulate the start app logic
		/// </summary>
		public static void StartsConfigitTest()
		{
			ResolveDependencies(ReadFiles());
		}

		/// <summary>
		/// Reads the files from the path and resolves dependencies
		/// </summary>
		/// <returns></returns>
		public static async void ResolveDependencies(List<FileInfo> filesInfo)
		{
			foreach (var file in filesInfo)
			{
				var fileContent = await ReadTextFileAsync(file.FullName);
				var packagesAndDependencies = fileContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				var packageDependencie = TransformFileIntoObject(packagesAndDependencies);

				var pass = CheckPackages(packageDependencie);

				Console.WriteLine("File {0} Pass: {1}", file.Name, pass);
			}


			Console.WriteLine("\r\nPress any key to close the program");
			Console.ReadLine();
		}
		 
		/// <summary>
		/// Check packages according to documentation
		/// </summary>
		/// <param name="packageDependencie">Package with dependencies</param>
		/// <returns>Bool if package is valid</returns>
		public static bool CheckPackages(PackageDependencie packageDependencie)
		{
			//Checks if more than one version of a package is required the installation is invalid. 
			var allDependencies = packageDependencie.Packages.Where(p => p.Dependencies != null).SelectMany(p => p.Dependencies).GroupBy(d => d.Name);
			foreach (var dependencie in allDependencies)
			{
				if (dependencie.Count() > 1)
					return false;
			}
			
			foreach (var package in packageDependencie.Packages)
			{
				if (package.Dependencies != null)
				{
					foreach (var dependencie in package.Dependencies)
					{
						if (!dependencie.IsValid)
							return false;

						var dependencieFromThisPackage = packageDependencie.Packages.FirstOrDefault(p => p.Name == dependencie.Name);
						if (dependencieFromThisPackage != null)
						{
							if (dependencieFromThisPackage.Version == dependencie.Version)
								return true;
							return false;
						}
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Transform file into object PackageDependencie
		/// </summary>
		/// <param name="packagesAndDependencies">List with packages and dependencies from file</param>
		/// <returns>PackageDependencie file</returns>
		private static PackageDependencie TransformFileIntoObject(List<string> packagesAndDependencies)
		{
			var packageDependencie = new PackageDependencie();

			var numberOfPackagesIndex = packagesAndDependencies.FindIndex(l => !l.Contains(','));
			packageDependencie.NumberOfPackages = Convert.ToInt32(packagesAndDependencies[numberOfPackagesIndex]);

			var numberOfDependenciesIndex = packagesAndDependencies.FindLastIndex(l => !l.Contains(','));
			packageDependencie.NumberOfDependencies = Convert.ToInt32(packagesAndDependencies[numberOfDependenciesIndex]);

			var packagesFromFile = packagesAndDependencies.GetRange(numberOfPackagesIndex + 1, packageDependencie.NumberOfPackages);
			packageDependencie.Packages = TransformToPackageObject(packagesFromFile);

			var dependenciesFromFile = packagesAndDependencies.GetRange(numberOfDependenciesIndex + 1, packageDependencie.NumberOfDependencies);
			TransformToDependencieObject(dependenciesFromFile, packageDependencie.Packages);

			return packageDependencie;
		}

		/// <summary>
		/// Transform the packages and dependencies from file into package objects
		/// </summary>
		/// <param name="packagesAndDependencies">Packages and dependencies from file</param>
		/// <returns>List with object package</returns>
		private static List<Package> TransformToPackageObject(List<string> packagesAndDependencies)
		{
			var packages = new List<Package>();
			foreach (var packageFromFile in packagesAndDependencies)
			{
				var packagesAndVersions = packageFromFile.Split(',');
				int i = 0;
				var package = packagesAndVersions.GroupBy(x => Math.Floor(i++ / 2.0))
							.Select(g => new Package { Name = g.ElementAt(0), Version = g.ElementAt(1) });

				packages.AddRange(package);
			}
			return packages;
		}

		/// <summary>
		/// Transform dependencie object and set it to package
		/// </summary>
		/// <param name="packagesAndDependencies">Main object</param>
		/// <param name="packages">Packages to set dependencies</param>
		private static void TransformToDependencieObject(List<string> packagesAndDependencies, List<Package> packages)
		{
			foreach (var packageFromFile in packagesAndDependencies)
			{
				var packagesAndVersions = packageFromFile.Split(',');

				var package = packages.FirstOrDefault(p => p.Name == packagesAndVersions[0] && p.Version == packagesAndVersions[1]);
				if (package != null)
				{
					if (package.Dependencies == null)
						package.Dependencies = new List<Dependencie>();

					var dependencie = new Dependencie();
					if (packagesAndVersions.Count() == 2)
					{
						dependencie.IsValid = true;
					}
					else if (packagesAndVersions.Count() > 4)
					{
						dependencie.IsValid = false;
					}
					else if (packagesAndVersions.Count() > 2)
					{
						dependencie.Name = packagesAndVersions[2];
						dependencie.Version = packagesAndVersions[3];
						dependencie.IsValid = true;
					}
					package.Dependencies.Add(dependencie);
				}
			}
		}

		/// <summary>
		/// Read the text file Async
		/// </summary>
		/// <returns>Content as string</returns>
		public static async Task<string> ReadTextFileAsync(string path)
		{
			var sr = new StreamReader(path);
			return await sr.ReadToEndAsync();
		}

		/// <summary>
		/// Read files in the path inputted by User
		/// </summary>
		/// <returns>Files from this path</returns>
		private static List<FileInfo> ReadFiles()
		{
			var exists = false;
			var files = new List<FileInfo>();
			do
			{

				Console.WriteLine("Please inform the path with package dependencies to be analyzed: ");

				var path = Console.ReadLine();
				if (Directory.Exists(path))
				{
					var di = new DirectoryInfo(path);
					files = di.GetFiles("*").ToList();
					exists = files.Any();
				}

				if (!exists)
					Console.WriteLine("Path is invalid!\r\n");

			}
			while (!exists);
			return files;
		}
	}
}