using System;
using Amusoft.CodeAnalysis.Analyzers.CodeGeneration.DelegateImplementationToFields;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CodeGeneration
{
	[TestClass]
	public class DelegateImplementationToFieldsTests : TestHelper.CodeFixVerifier
	{
		[TestMethod]
		public void EmptyNoAction()
		{
			var test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void FieldButNoDelegation()
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
             private ICollection<IDisposable> _disposables;
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void FieldWithDelegation()
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
        class TypeName : IDisposable
        {
             private ICollection<IDisposable> _disposables;
        }
    }";
			var expected = new DiagnosticResult
			{
				Id = Analyzer.DiagnosticId,
				Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 13, 47)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

//		[TestMethod]
		public void NoFieldNoChange()
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
        }
    }";
			var expected = new DiagnosticResult
			{
				Id = Analyzer.DiagnosticId,
				Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat),
				Severity = DiagnosticSeverity.Warning,
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
			return new Analyzer();
		}
	}
}