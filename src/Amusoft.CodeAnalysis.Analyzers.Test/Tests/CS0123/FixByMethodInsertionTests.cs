// // Copyright 2020 Andreas Müller
// // This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// // See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
		Amusoft.CodeAnalysis.Analyzers.CS0123.FixByIntroducingMethod>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CS0123
{
	[TestClass]
	public class FixByMethodInsertionTests
	{
		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}

		[TestMethod]
		public async Task DiagnosticAtObjectCreationExpression()
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
        TypeName()
        {
            var action = new Func<int, string>(TestMethod);
        }

        private int TestMethod(int arg)
        {
            throw new NotImplementedException();
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
    class TypeName
    {
        TypeName()
        {
            var action = new Func<int, string>(TestMethod);
        }

        private string TestMethod(int arg)
        {
            throw new NotImplementedException();
        }
    }
}";
			var diagnostics = new[]
			{
				CompilerError("CS0407").WithLocation(15, 48),
			};

			await Verifier.VerifyCodeFixAsync(test, diagnostics, fixtest);
		}
	}
}