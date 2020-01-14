// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
		Amusoft.CodeAnalysis.Analyzers.CS1998.FixByWrappingInTaskResult>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CS1998
{
	[TestClass]
	public class FixByWrappingInTaskResultTests
	{
		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}

		[TestMethod]
		public async Task MixedMethodDeclaration()
		{
			var test = @"namespace ConsoleApplication1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Diagnostics;
	using System.IO;

	public class TestClass
	{
		private string _workspaceFile;

		public async Task<Configuration[]> LoadStorageContentAsync()
		{
			var fileInfo = new FileInfo(_workspaceFile);
			if (!fileInfo.Exists)
			{
				Log.Error($""No configuration storage located at { fileInfo.FullName}."");
				return Array.Empty<Configuration>();
			}

			using (var stream = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
			{
				try
				{
					var storage = new Storage();
					return storage.Configurations.ToArray();
				}
				catch (Exception e)
				{
					Log.Error(e);
					return Array.Empty<Configuration>();
				}
			}
		}

		public class Log
		{
			public static void Error(Exception p0)
			{
				throw new NotImplementedException();
			}

			public static void Error(string p0)
			{
				throw new NotImplementedException();
			}
		}

		public class Storage
		{
			public Configuration[] Configurations { get; set; }
		}
		public class Configuration
		{
			public Guid Id { get; set; }
			public string ConfigurationName { get; set; }
		}
	}
}";


			var fixtest = @"namespace ConsoleApplication1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Diagnostics;
	using System.IO;

	public class TestClass
	{
		private string _workspaceFile;

		public Task<Configuration[]> LoadStorageContentAsync()
		{
			var fileInfo = new FileInfo(_workspaceFile);
			if (!fileInfo.Exists)
			{
				Log.Error($""No configuration storage located at { fileInfo.FullName}."");
				return Task.FromResult(Array.Empty<Configuration>());
			}

			using (var stream = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
			{
				try
				{
					var storage = new Storage();
					return Task.FromResult(storage.Configurations.ToArray());
				}
				catch (Exception e)
				{
					Log.Error(e);
					return Task.FromResult(Array.Empty<Configuration>());
				}
			}
		}

		public class Log
		{
			public static void Error(Exception p0)
			{
				throw new NotImplementedException();
			}

			public static void Error(string p0)
			{
				throw new NotImplementedException();
			}
		}

		public class Storage
		{
			public Configuration[] Configurations { get; set; }
		}
		public class Configuration
		{
			public Guid Id { get; set; }
			public string ConfigurationName { get; set; }
		}
	}
}";
			var diagnostics = new[]
			{
				CompilerWarning("CS1998"),
			};

			await Verifier.VerifyCodeFixAsync(test, diagnostics, fixtest);
		}
	}
}