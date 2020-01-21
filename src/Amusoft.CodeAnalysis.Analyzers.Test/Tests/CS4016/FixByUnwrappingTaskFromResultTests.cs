// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier =
	Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
		Amusoft.CodeAnalysis.Analyzers.CS4016.FixByUnwrappingTaskFromResult>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CS4016
{
	[TestClass]
	public class FixByUnwrappingTaskFromResultTests
	{
		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}

		[TestMethod]
		public async Task FixMethod()
		{
			var test = @"
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class TestClass
{
	private string _workspaceFile;

	public async Task<Configuration[]> LoadStorageContentAsync()
	{
		var fileInfo = new FileInfo(_workspaceFile);
		if (!fileInfo.Exists)
		{
			Log.Error($""No configuration storage located at {fileInfo.FullName}."");
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
}";

			var fixtest = @"
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class TestClass
{
	private string _workspaceFile;

	public async Task<Configuration[]> LoadStorageContentAsync()
	{
		var fileInfo = new FileInfo(_workspaceFile);
		if (!fileInfo.Exists)
		{
			Log.Error($""No configuration storage located at {fileInfo.FullName}."");
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
}";
			var diagnostics = new[]
			{
				// Test0.cs(17,11): error CS4016: Since this is an async method, the return expression must be of type 'TestClass.Configuration[]' rather than 'Task<TestClass.Configuration[]>'
				DiagnosticResult.CompilerError("CS4016").WithSpan(17, 11, 17, 56)
					.WithArguments("TestClass.Configuration[]"),
// Test0.cs(25,12): error CS4016: Since this is an async method, the return expression must be of type 'TestClass.Configuration[]' rather than 'Task<TestClass.Configuration[]>'
				DiagnosticResult.CompilerError("CS4016").WithSpan(25, 12, 25, 61)
					.WithArguments("TestClass.Configuration[]"),
// Test0.cs(30,12): error CS4016: Since this is an async method, the return expression must be of type 'TestClass.Configuration[]' rather than 'Task<TestClass.Configuration[]>'
				DiagnosticResult.CompilerError("CS4016").WithSpan(30, 12, 30, 57)
					.WithArguments("TestClass.Configuration[]")
			};

			await Verifier.VerifyCodeFixAsync(test, diagnostics, fixtest);
		}
	}
}