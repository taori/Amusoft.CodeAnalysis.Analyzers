using System;
using Amusoft.CodeAnalysis.Analyzers.CodeGeneration.GenerateMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CodeGeneration
{
	[TestClass]
	public class GenerateMethodTests : CodeFixVerifier
	{
		[TestMethod]
		public void NoAction()
		{
			var test = @"";
			
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void Cs0123FixGenerateFixedArgument()
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
			var expected = new DiagnosticResult
			{
				Id = "CS0123",
				Message = string.Format(Resources.GenerateMethodFixerMessageFormat),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 11, 15)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

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
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void Cs0123FixGenerateFixedReturn()
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
			var expected = new DiagnosticResult
			{
				Id = "CS0123",
				Message = string.Format(Resources.GenerateMethodFixerMessageFormat),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 11, 15)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

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

        private int TestMethod(int arg)
        {
            throw new NotImplementedException();
        }

        private string TestMethod(int arg)
        {
            throw new NotImplementedException();
        }
    }
}";
			VerifyCSharpFix(test, fixtest);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new Fixer();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return null;
		}
	}
}