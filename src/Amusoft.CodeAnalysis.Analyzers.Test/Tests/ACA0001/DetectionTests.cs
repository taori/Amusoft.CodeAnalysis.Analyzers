// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.ACA0001;
using Amusoft.CodeAnalysis.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using AnalyzerVerifier =
	Microsoft.CodeAnalysis.CSharp.Testing.MSTest.AnalyzerVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0001.Analyzer>;
using Verifier =
	Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0001.Analyzer,
		Amusoft.CodeAnalysis.Analyzers.ACA0001.FixByForwardingToCollectionChildren>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.ACA0001
{
	[TestClass]
	public class DetectionTests
	{
		[TestMethod]
		public async Task EmptySourceNoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}


		[TestMethod]
		public async Task FieldButNoDelegation()
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

			await AnalyzerVerifier.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task FieldWithDelegation()
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

			var diagnostic = CompilerError("CS0535").WithSpan(11, 26, 11, 37)
					.WithArguments("ConsoleApplication1.TypeName", "System.IDisposable.Dispose()")
				;

			await AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
		}

		[TestMethod]
		public async Task FieldAsListWithDelegation()
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
             private List<IDisposable> _disposables;
        }
    }";

			var diagnostic = CompilerError("CS0535").WithSpan(11, 26, 11, 37)
				.WithArguments("ConsoleApplication1.TypeName", "System.IDisposable.Dispose()");

			await AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
		}

		[TestMethod]
		public async Task FieldWithInvalidDelegation()
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
        class TypeName : ISomethingElse
        {
             private List<IDisposable> _disposables;
        }

        public interface ISomethingElse {}
    }";

			await AnalyzerVerifier.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task PropertyWithDelegation()
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

			var diagnostic = CompilerError("CS0535").WithSpan(11, 26, 11, 37)
				.WithArguments("ConsoleApplication1.TypeName", "System.IDisposable.Dispose()");

			await AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
		}

		[TestMethod]
		public async Task VerifyReturnTaskNonBoolSupportedType()
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
    public interface ICustomInterface
    {
        Task<string> Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public Task<string> Method1(object p1)
        {
            throw new NotImplementedException();
        }
    }
}";
			await AnalyzerVerifier.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task VerifyReturnNoFixUnsupportedType()
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
    public interface ICustomInterface
    {
        string Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public string Method1(object p1)
        {
            throw new NotImplementedException();
        }
    }
}";

			await AnalyzerVerifier.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task VerifyDiagnosticMethodFiltering()
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
    public interface ICustomInterface
    {
        void Method1(object p1);
        void Method1(string p1);
        void Method2(string p1, int p2);
        void Method2(string p1);
        void Method3();
    }

    class TypeName : ICustomInterface, IDisposable
    {
        private ICollection<ICustomInterface> _disposables;

        public void Method1(object p1)
        {
        }

        public void Method1(string p1)
        {
            var a = 1;
        }

        public void Method2(string p1, int p2)
        {
            throw new NotImplementedException();
        }

        public void Method2(string p1)
        {
            throw new NotImplementedException();
        }

        public void Method3()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}";

            await new Helpers.AnalyzerTest<Analyzer>()
			{
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
				CompilerDiagnostics = CompilerDiagnostics.Warnings | CompilerDiagnostics.Suggestions,
				TestState =
				{
					Sources = {test},
					ExpectedDiagnostics =
					{
						CompilerWarning("CS1591").WithSpan(11, 22, 11, 38)
							.WithArguments("ConsoleApplication1.ICustomInterface"),
						CompilerWarning("CS1591").WithSpan(13, 14, 13, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(object)"),
						CompilerWarning("CS1591").WithSpan(14, 14, 14, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(string)"),
						CompilerWarning("CS1591").WithSpan(15, 14, 15, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string, int)"),
						CompilerWarning("CS1591").WithSpan(16, 14, 16, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string)"),
						CompilerWarning("CS1591").WithSpan(17, 14, 17, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method3()"),
						CompilerWarning("CS0169").WithSpan(22, 47, 22, 59)
							.WithArguments("ConsoleApplication1.TypeName._disposables"),
						AnalyzerVerifier.Diagnostic(Analyzer.DiagnosticId).WithSpan(24, 21, 24, 28).WithArguments("Method1", "_disposables"),
						CompilerWarning("CS0219").WithSpan(30, 17, 30, 18).WithArguments("a"),
						Verifier.Diagnostic().WithSpan(33, 21, 33, 28).WithArguments("Method2", "_disposables"),
						Verifier.Diagnostic().WithSpan(38, 21, 38, 28).WithArguments("Method2", "_disposables"),
						Verifier.Diagnostic().WithSpan(43, 21, 43, 28).WithArguments("Method3", "_disposables")
					},
				}
			}.RunAsync();

		}
	}
}