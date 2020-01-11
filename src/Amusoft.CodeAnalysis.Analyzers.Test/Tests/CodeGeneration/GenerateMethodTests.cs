﻿using System;
using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.CodeGeneration.GenerateMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer, Amusoft.CodeAnalysis.Analyzers.CodeGeneration.GenerateMethod.Fixer>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CodeGeneration
{
	[TestClass]
	public class GenerateMethodTests : TestHelper.CodeFixVerifier
	{
		[TestMethod]
		public async Task NoAction()
		{
			await CodeFixVerifier<EmptyDiagnosticAnalyzer, Fixer>.VerifyCodeFixAsync(string.Empty, string.Empty);
		}

		[TestMethod]
		public async Task Cs0123FixGenerateFixedArgument()
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
			var diagnostic1 = CompilerError("CS0123").WithLocation(15, 26);

			await Verify.VerifyCodeFixAsync(test, diagnostic1, fixtest);
		}

		[TestMethod]
		public async Task Cs0407FixGenerateFixedReturn()
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

			await Verify.VerifyCodeFixAsync(test, diagnostics, fixtest);
        }
	}
}