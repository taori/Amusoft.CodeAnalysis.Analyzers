using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.CodeGeneration.CS0407;
using Amusoft.CodeAnalysis.Analyzers.CodeGeneration.GenerateMethod;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer, Amusoft.CodeAnalysis.Analyzers.CodeGeneration.CS0407.FixByChangingReturnTypeOfMethod>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CodeGeneration
{
	[TestClass]
	public class Cs0407FixMethodReturnTests 
	{
		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await CodeFixVerifier<EmptyDiagnosticAnalyzer, FixByChangingReturnTypeOfMethod>.VerifyCodeFixAsync(string.Empty, string.Empty);
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