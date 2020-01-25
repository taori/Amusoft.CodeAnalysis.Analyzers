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
		private const string LibraryFile = @"
using System;

namespace ConsoleApplication1
{
    public static class Helper
    {
        public static void Method1() {}
        public static void Method2(string value1, string value2) {}
        public static void Method3() {}
        public static void Method4() {}
        public static void Method5() {}
        public static void Method6() {}
        public static readonly string Member1;
        public static string Member2 {get;} = ""somevalue"";
    }

    public static class Helper2
    {
        public static void Method1() {}
        public static void Method2(string value1, string value2) {}
        public static void Method3() {}
        public static void Method4() {}
        public static void Method5() {}
        public static void Method6() {}
        public static readonly string Member1;
        public static string Member2 {get;} = ""somevalue"";
    }
}";

		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}

		[DataTestMethod]
		[DataRow("Member1", DisplayName = "MemberAccess Member1")]
		[DataRow("Member2", DisplayName = "MemberAccess Member2")]
		public async Task RewriteNonMethodAccess(string memberName)
		{
			var test = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleApplication1;

namespace ConsoleApplication1
{{
    class TypeName
    {{
        private string GetTypeName()
        {{
            return Helper.{memberName};
            return Helper.{memberName};
            return Helper.{memberName};
            return Helper.{memberName};
            return Helper.{memberName};
        }}
    }}
}}";


			var fixtest = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleApplication1;
using static ConsoleApplication1.Helper;

namespace ConsoleApplication1
{{
    class TypeName
    {{
        private string GetTypeName()
        {{
            return {memberName};
            return {memberName};
            return {memberName};
            return {memberName};
            return {memberName};
        }}
    }}
}}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFile},
					ExpectedDiagnostics =
					{
						// Test0.cs(16,20): info ACA0006: Import type "Helper" as static.
						Verifier.Diagnostic().WithSpan(16, 20, 16, 26).WithArguments("Helper")
					},
				},
				FixedState =
				{
					Sources = {fixtest, LibraryFile},
				},
			}.RunAsync();
		}


		[TestMethod]
		public async Task RewriteIfFiveOrMoreWithParameters()
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
            Helper.Method2(""value1"", ""value2"");
            Helper.Method2(""value1"", ""value2"");
            Helper.Method2(""value1"", ""value2"");
            Helper.Method2(""value1"", ""value2"");
            Helper.Method2(""value1"", ""value2"");
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
            Method2(""value1"", ""value2"");
            Method2(""value1"", ""value2"");
            Method2(""value1"", ""value2"");
            Method2(""value1"", ""value2"");
            Method2(""value1"", ""value2"");
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFile},
					ExpectedDiagnostics =
					{
						// Test0.cs(16,13): info ACA0006: Import type "Helper" as static.
						Verifier.Diagnostic().WithSpan(16, 13, 16, 19).WithArguments("Helper")

					},
				},
				FixedState =
				{
					Sources = {fixtest, LibraryFile},
				},
			}.RunAsync();
		}

		[TestMethod]
		public async Task RewriteOnlyDiagnosticMatches()
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
            Helper.Method3();
            Helper.Method4();
            Helper.Method5();
            Helper.Method6();
            Helper2.Method6();
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
            Method3();
            Method4();
            Method5();
            Method6();
            Helper2.Method6();
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFile},
					ExpectedDiagnostics =
					{
						// Test0.cs(16,13): info ACA0006: Import type "Helper" as static.
						Verifier.Diagnostic().WithSpan(16, 13, 16, 19).WithArguments("Helper")
					},
				},
				FixedState =
				{
					Sources = {fixtest, LibraryFile},
				},
			}.RunAsync();
		}


		[TestMethod]
		public async Task RewriteIfFiveOrMore()
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
            Helper.Method3();
            Helper.Method4();
            Helper.Method5();
            Helper.Method6();
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
            Method3();
            Method4();
            Method5();
            Method6();
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFile},
					ExpectedDiagnostics =
					{
						// Test0.cs(16,13): info ACA0006: Import type "Helper" as static.
						Verifier.Diagnostic().WithSpan(16, 13, 16, 19).WithArguments("Helper")
					},
				},
				FixedState =
				{
					Sources = {fixtest, LibraryFile},
				},
			}.RunAsync();
		}

		[TestMethod]
		public async Task DoNotDuplicateUsingstatements()
		{
			var test = @"
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
            Helper.Method1();
            Helper.Method1();
            Helper.Method1();
            Helper.Method1();
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
            Method1();
            Method1();
            Method1();
            Method1();
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFile},
					ExpectedDiagnostics =
					{
						// Test0.cs(17,13): info ACA0006: Import type "Helper" as static.
						Verifier.Diagnostic().WithSpan(17, 13, 17, 19).WithArguments("Helper")
					},
				},
				FixedState =
				{
					Sources = {fixtest, LibraryFile},
				},
			}.RunAsync();
		}

		[TestMethod]
		public async Task DoNotShowDiagnosticsForInstanceMethodInvocations()
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
            var someText = ""something"";
            var bla1 = someText.ToString();
            var bla2 = someText.ToString();
            var bla3 = someText.ToString();
            var bla4 = someText.ToString();
            var bla5 = someText.ToString();
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFile},
					ExpectedDiagnostics =
					{

					},
				},
				FixedState =
				{
					Sources = {test, LibraryFile},
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
            Helper.Method1();
            Helper.Method1();
            Helper.Method1();
        }
    }
}";

			await new CodeFixTest<StaticImportAnalyzer, FixByImportingTypeAsStatic>()
			{
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test, LibraryFile},
					ExpectedDiagnostics =
					{

					},
				},
				FixedState =
				{
					Sources = {test, LibraryFile},
				},
			}.RunAsync();
		}
	}
}