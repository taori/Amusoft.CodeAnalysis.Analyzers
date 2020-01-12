﻿using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.CS0123;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CS0123
{
	[TestClass]
	public class FixByMethodIntroductionTests : TestHelper.CodeFixVerifier
	{
		[TestMethod]
		public async Task NoAction()
		{
			await CodeFixVerifier<EmptyDiagnosticAnalyzer, FixByIntroducingMethod>.VerifyCodeFixAsync(string.Empty, string.Empty);
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
            var action = new Func<string, int>(TestMethod);
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
            var action = new Func<string, int>(TestMethod);
        }

        private int TestMethod(int arg)
        {
            throw new NotImplementedException();
        }

        private int TestMethod(string arg)
        {
            throw new NotImplementedException();
        }
    }
}";
			var diagnostic1 = DiagnosticResult.CompilerError("CS0123").WithLocation(15, 26);

			await CodeFixVerifier<EmptyDiagnosticAnalyzer, FixByIntroducingMethod>.VerifyCodeFixAsync(test, diagnostic1, fixtest);
		}
	}
}