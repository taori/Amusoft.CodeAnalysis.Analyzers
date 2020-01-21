// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.ACA0002;
using Amusoft.CodeAnalysis.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0002.CommentAnalyzer,
		Amusoft.CodeAnalysis.Analyzers.ACA0002.FixByRemovingClassComments>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.ACA0002
{
	[TestClass]
	public class FixCommentsOnClassTests
	{
		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}

		[TestMethod]
		public async Task SimpleRemoval()
		{

			var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {
        // some comment
        public enum Bla{}
    }
}";


			var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {
        public enum Bla{}
    }
}";
			await new CodeFixTest<CommentAnalyzer, FixByRemovingClassComments>()
			{
				TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
				CompilerDiagnostics = CompilerDiagnostics.Suggestions,
				TestState =
				{
					Sources = {test},
					ExpectedDiagnostics =
					{
						// Test0.cs(9,11): info ACA0005: Comments can be removed from this namespace.
						Verifier.Diagnostic(CommentAnalyzer.NamespaceRule).WithSpan(9, 11, 9, 30),
// Test0.cs(11,11): info ACA0002: Comments can be removed from this class.
						Verifier.Diagnostic(CommentAnalyzer.ClassRule).WithSpan(11, 11, 11, 19)
					},
				},
				FixedState =
				{
					InheritanceMode = StateInheritanceMode.Explicit,
					Sources = {fixtest}
				},
			}.RunAsync();
		}
	}
}