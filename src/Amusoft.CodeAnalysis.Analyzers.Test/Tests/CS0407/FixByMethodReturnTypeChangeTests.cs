﻿using System;
using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.CS0407;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer, Amusoft.CodeAnalysis.Analyzers.CS0407.FixByReplacingMethodReturnType>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CS0407
{
	[TestClass]
	public class FixByMethodReturnTypeChangeTests
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

        private int TestMethod(int param1)
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

        private string TestMethod(int param1)
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

		[TestMethod]
		public async Task DiagnosticAtObjectCreationExpressionWithSimilarOverload()
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

        private int TestMethod(int param1)
        {
            throw new NotImplementedException();
        }

        private int TestMethod(bool param1)
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

        private string TestMethod(int param1)
        {
            throw new NotImplementedException();
        }

        private int TestMethod(bool param1)
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