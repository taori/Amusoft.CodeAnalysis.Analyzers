// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.ACA0002;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0002.CommentAnalyzer,
		Amusoft.CodeAnalysis.Analyzers.ACA0002.FixByRemovingNamespaceComments>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.ACA0002
{
	[TestClass]
	public class FixCommentsOnNamespaceTests
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
    // some comment
    public enum Bla{}

    class TypeName
    {
        TypeName()
        {
        }

        private void TestMethod(int arg)
        {
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

namespace ConsoleApplication1
{
    public enum Bla{}

    class TypeName
    {
        TypeName()
        {
        }

        private void TestMethod(int arg)
        {
        }
    }
}";
			var diagnostics = new[]
			{
				// Test0.cs(9,11): info ACA0005: Comments can be removed from this namespace.
				Verifier.Diagnostic(CommentAnalyzer.NamespaceRule).WithSpan(9, 11, 9, 30)
			};

			await Verifier.VerifyCodeFixAsync(test, diagnostics, fixtest);
		}
	}
}