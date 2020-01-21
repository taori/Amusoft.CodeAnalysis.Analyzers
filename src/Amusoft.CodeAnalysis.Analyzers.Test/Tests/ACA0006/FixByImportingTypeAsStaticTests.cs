// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.ACA0006;
using Amusoft.CodeAnalysis.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0006.StaticImportAnalyzer, Amusoft.CodeAnalysis.Analyzers.ACA0006.FixByImportingTypeAsStatic>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.ACA0006
{
	[TestClass]
	public class FixByImportingTypeAsStaticTests
	{
		private const string LibraryFileNoCandidate = @"
using System;

namespace ConsoleApplication1
{
    public static class Helper
    {
        public static void Method1() {}
        public static void Method2() {}
        public static void Method3() {}
        public static void Method4() {}
    }
}";

		private const string LibraryFileCandidate = @"
using System;

namespace ConsoleApplication1
{
    public static class Helper
    {
        public static void Method1() {}
        public static void Method2() {}
        public static void Method3() {}
        public static void Method4() {}
        public static void Method5() {}
    }
}";

		private const string LibraryFileCandidate2 = @"
using System;

namespace ConsoleApplication1
{
    public static class Helper
    {
        public static void Method1() {}
        public static void Method2() {}
        public static void Method3() {}
        public static readonly string Member1;
        public static string Member2 {get;} = ""somevalue"";
    }
}";

		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}

		// [DataTestMethod]
		// [DataRow(LibraryFileCandidate, DisplayName = "MethodsOnly")]
		// [DataRow(LibraryFileCandidate2, DisplayName = "MixedMembers")]
		[TestMethod]
		public async Task RewriteIfFiveOrMore()
		// public async Task RewriteIfFiveOrMore(string libraryFile)
		{
			var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleApplication1;

namespace ConsoleApplication1
{
    class TypeName
    {
        TypeName()
        {
            Helper.Method1();
        }
    }
}";


			var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleApplication1;
using static ConsoleApplication1.Helper;

namespace ConsoleApplication1
{
    class TypeName
    {
        TypeName()
        {
            Method1();
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFileCandidate},
					ExpectedDiagnostics =
					{
						// Test0.cs(16,13): info ACA0006: Import type "Helper" as static.
						Verifier.Diagnostic().WithSpan(16, 13, 16, 19).WithArguments("Helper")
					},
				},
				FixedState =
				{
					Sources = {fixtest, LibraryFileCandidate},
				},
			}.RunAsync();
		}

		[TestMethod]
		public async Task DoNotRewriteIfLessThan5Methods()
		{
			var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleApplication1;

namespace ConsoleApplication1
{
    class TypeName
    {
        TypeName()
        {
            Helper.Method1();
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFileNoCandidate},
					ExpectedDiagnostics =
					{

					},
				},
				FixedState =
				{
					Sources = {test, LibraryFileNoCandidate},
				},
			}.RunAsync();
		}
	}
}