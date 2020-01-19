using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.ACA0001;
using Amusoft.CodeAnalysis.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier =
	Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0001.Analyzer,
		Amusoft.CodeAnalysis.Analyzers.ACA0001.FixByForwardingToCollectionChildren>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.ACA0001
{
	[TestClass]
	public class FixByForwardingToCollectionChildrenTests
	{
		[TestMethod]
		public void EmptyNoAction()
		{
			Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
		}
		[TestMethod]
		public async Task CanFixTask()
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
        Task Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public Task Method1(object p1)
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
    public interface ICustomInterface
    {
        Task Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public Task Method1(object p1)
        {
            return Task.WhenAll(_disposables.Select(item => item.Method1(p1)));
        }
    }
}";

			var expected = new DiagnosticResult[]
			{
				// Test0.cs(20,21): info ACA0001: Forward execution of "Method1" to member "_disposables"
				Verifier.Diagnostic().WithSpan(20, 21, 20, 28).WithArguments("Method1", "_disposables")
            };

			await Verifier.VerifyCodeFixAsync(test, expected, fixtest);
		}

		[TestMethod]
		public async Task CanFixAsyncTask()
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
        Task Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public async Task Method1(object p1)
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
    public interface ICustomInterface
    {
        Task Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public async Task Method1(object p1)
        {
            await Task.WhenAll(_disposables.Select(item => item.Method1(p1)));
        }
    }
}";

			var expected = new DiagnosticResult[]
			{
				// Test0.cs(20,27): info ACA0001: Forward execution of "Method1" to member "_disposables"
				Verifier.Diagnostic().WithSpan(20, 27, 20, 34).WithArguments("Method1", "_disposables")
            };

			await Verifier.VerifyCodeFixAsync(test, expected, fixtest);
		}

        [TestMethod]
		public async Task CanFixBool()
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
        bool Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public bool Method1(object p1)
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
    public interface ICustomInterface
    {
        bool Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public bool Method1(object p1)
        {
            return _disposables.All(item => item.Method1(p1));
        }
    }
}";
			var expectedDiagnostics = new[]
			{
				Verifier.Diagnostic(Analyzer.DiagnosticId)
					.WithSpan(20, 21, 20, 28)
					.WithArguments("Method1", "_disposables")
					.WithSeverity(DiagnosticSeverity.Info),
			};

			await Verifier.VerifyCodeFixAsync(test, expectedDiagnostics, fixtest);
		}
        
        [TestMethod]
		public async Task VerifyFullRewrite()
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

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public void Method1(object p1)
        {
            throw new NotImplementedException();
        }

        public void Method1(string p1)
        {
            throw new NotImplementedException();
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
    public interface ICustomInterface
    {
        void Method1(object p1);
        void Method1(string p1);
        void Method2(string p1, int p2);
        void Method2(string p1);
        void Method3();
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public void Method1(object p1)
        {
            foreach (var item in _disposables)
            {
                item.Method1(p1);
            }
        }

        public void Method1(string p1)
        {
            foreach (var item in _disposables)
            {
                item.Method1(p1);
            }
        }

        public void Method2(string p1, int p2)
        {
            foreach (var item in _disposables)
            {
                item.Method2(p1, p2);
            }
        }

        public void Method2(string p1)
        {
            foreach (var item in _disposables)
            {
                item.Method2(p1);
            }
        }

        public void Method3()
        {
            foreach (var item in _disposables)
            {
                item.Method3();
            }
        }
    }
}";

			await new CodeFixTest<Analyzer, FixByForwardingToCollectionChildren>()
			{
				TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
				CompilerDiagnostics = CompilerDiagnostics.Suggestions,
				TestState =
				{
					Sources = {test},
					ExpectedDiagnostics =
					{
						Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(24, 21)
							.WithArguments("Method1", "_disposables").WithSeverity(DiagnosticSeverity.Info),
						Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(29, 21)
							.WithArguments("Method1", "_disposables").WithSeverity(DiagnosticSeverity.Info),
						Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(34, 21)
							.WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
						Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(39, 21)
							.WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
						Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(44, 21)
							.WithArguments("Method3", "_disposables").WithSeverity(DiagnosticSeverity.Info),

						// Test0.cs(11,22): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(11, 22, 11, 38)
							.WithArguments("ConsoleApplication1.ICustomInterface"),
// Test0.cs(13,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(object)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(13, 14, 13, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(object)"),
// Test0.cs(14,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(14, 14, 14, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(string)"),
// Test0.cs(15,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string, int)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(15, 14, 15, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string, int)"),
// Test0.cs(16,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(16, 14, 16, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string)"),
// Test0.cs(17,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method3()'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(17, 14, 17, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method3()"),
// Test0.cs(22,47): warning CS0169: The field 'TypeName._disposables' is never used
						DiagnosticResult.CompilerWarning("CS0169").WithSpan(22, 47, 22, 59)
							.WithArguments("ConsoleApplication1.TypeName._disposables")
					},
				},
				FixedState =
				{
					ExpectedDiagnostics =
					{
						// Test0.cs(11,22): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(11, 22, 11, 38)
							.WithArguments("ConsoleApplication1.ICustomInterface"),
// Test0.cs(13,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(object)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(13, 14, 13, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(object)"),
// Test0.cs(14,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(14, 14, 14, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(string)"),
// Test0.cs(15,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string, int)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(15, 14, 15, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string, int)"),
// Test0.cs(16,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(16, 14, 16, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string)"),
// Test0.cs(17,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method3()'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(17, 14, 17, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method3()"),
// Test0.cs(22,47): warning CS0649: Field 'TypeName._disposables' is never assigned to, and will always have its default value null
						DiagnosticResult.CompilerWarning("CS0649").WithSpan(22, 47, 22, 59)
							.WithArguments("ConsoleApplication1.TypeName._disposables", "null")
					},
					Sources = {fixtest}
				},
			}.RunAsync();
		}

		[TestMethod]
		public async Task VerifyPartialRewrite()
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

    class TypeName : ICustomInterface
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
    public interface ICustomInterface
    {
        void Method1(object p1);
        void Method1(string p1);
        void Method2(string p1, int p2);
        void Method2(string p1);
        void Method3();
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public void Method1(object p1)
        {
            foreach (var item in _disposables)
            {
                item.Method1(p1);
            }
        }

        public void Method1(string p1)
        {
            var a = 1;
        }

        public void Method2(string p1, int p2)
        {
            foreach (var item in _disposables)
            {
                item.Method2(p1, p2);
            }
        }

        public void Method2(string p1)
        {
            foreach (var item in _disposables)
            {
                item.Method2(p1);
            }
        }

        public void Method3()
        {
            foreach (var item in _disposables)
            {
                item.Method3();
            }
        }
    }
}";
			var expected = new[]
			{
				// Test0.cs(24,21): info ACA0001: Forward execution of "Method1" to member "_disposables"
				Verifier.Diagnostic().WithSpan(24, 21, 24, 28).WithArguments("Method1", "_disposables"),
// Test0.cs(33,21): info ACA0001: Forward execution of "Method2" to member "_disposables"
				Verifier.Diagnostic().WithSpan(33, 21, 33, 28).WithArguments("Method2", "_disposables"),
// Test0.cs(38,21): info ACA0001: Forward execution of "Method2" to member "_disposables"
				Verifier.Diagnostic().WithSpan(38, 21, 38, 28).WithArguments("Method2", "_disposables"),
// Test0.cs(43,21): info ACA0001: Forward execution of "Method3" to member "_disposables"
				Verifier.Diagnostic().WithSpan(43, 21, 43, 28).WithArguments("Method3", "_disposables")
			};
            await Verifier.VerifyCodeFixAsync(test, expected, fixtest);

			await new CodeFixTest<Analyzer, FixByForwardingToCollectionChildren>()
			{
				TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
				CompilerDiagnostics = CompilerDiagnostics.Errors,
				TestState =
				{
					Sources = {test},
					ExpectedDiagnostics =
                    {
	                    // Test0.cs(24,21): info ACA0001: Forward execution of "Method1" to member "_disposables"
	                    Verifier.Diagnostic().WithSpan(24, 21, 24, 28).WithArguments("Method1", "_disposables"),
// Test0.cs(33,21): info ACA0001: Forward execution of "Method2" to member "_disposables"
	                    Verifier.Diagnostic().WithSpan(33, 21, 33, 28).WithArguments("Method2", "_disposables"),
// Test0.cs(38,21): info ACA0001: Forward execution of "Method2" to member "_disposables"
	                    Verifier.Diagnostic().WithSpan(38, 21, 38, 28).WithArguments("Method2", "_disposables"),
// Test0.cs(43,21): info ACA0001: Forward execution of "Method3" to member "_disposables"
	                    Verifier.Diagnostic().WithSpan(43, 21, 43, 28).WithArguments("Method3", "_disposables")
                    },
				},
				FixedState =
				{
					ExpectedDiagnostics =
					{
                    },
					Sources = {fixtest}
				},
			}.RunAsync();
		}

		[TestMethod]
		public async Task VerifyInheritedInterfaceAnalyzers()
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
    public interface ICustomInterface : IDisposable
    {
        void Method1(object p1);
        void Method1(string p1);
        void Method2(string p1, int p2);
        void Method2(string p1);
        void Method3();
    }

    class TypeName : ICustomInterface
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

			var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    public interface ICustomInterface : IDisposable
    {
        void Method1(object p1);
        void Method1(string p1);
        void Method2(string p1, int p2);
        void Method2(string p1);
        void Method3();
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public void Method1(object p1)
        {
            foreach (var item in _disposables)
            {
                item.Method1(p1);
            }
        }

        public void Method1(string p1)
        {
            var a = 1;
        }

        public void Method2(string p1, int p2)
        {
            foreach (var item in _disposables)
            {
                item.Method2(p1, p2);
            }
        }

        public void Method2(string p1)
        {
            foreach (var item in _disposables)
            {
                item.Method2(p1);
            }
        }

        public void Method3()
        {
            foreach (var item in _disposables)
            {
                item.Method3();
            }
        }

        public void Dispose()
        {
            foreach (var item in _disposables)
            {
                item.Dispose();
            }
        }
    }
}";

			await new CodeFixTest<Analyzer, FixByForwardingToCollectionChildren>()
			{
				TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
				CompilerDiagnostics = CompilerDiagnostics.Suggestions,
				TestState =
				{
					Sources = {test},
					ExpectedDiagnostics =
					{
						// Test0.cs(11,22): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(11, 22, 11, 38)
							.WithArguments("ConsoleApplication1.ICustomInterface"),
// Test0.cs(13,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(object)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(13, 14, 13, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(object)"),
// Test0.cs(14,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(14, 14, 14, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(string)"),
// Test0.cs(15,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string, int)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(15, 14, 15, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string, int)"),
// Test0.cs(16,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(16, 14, 16, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string)"),
// Test0.cs(17,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method3()'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(17, 14, 17, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method3()"),
// Test0.cs(22,47): warning CS0169: The field 'TypeName._disposables' is never used
						DiagnosticResult.CompilerWarning("CS0169").WithSpan(22, 47, 22, 59)
							.WithArguments("ConsoleApplication1.TypeName._disposables"),
// Test0.cs(24,21): info ACA0001: Forward execution of "Method1" to member "_disposables"
						Verifier.Diagnostic().WithSpan(24, 21, 24, 28).WithArguments("Method1", "_disposables"),
// Test0.cs(30,17): warning CS0219: The variable 'a' is assigned but its value is never used
						DiagnosticResult.CompilerWarning("CS0219").WithSpan(30, 17, 30, 18).WithArguments("a"),
// Test0.cs(33,21): info ACA0001: Forward execution of "Method2" to member "_disposables"
						Verifier.Diagnostic().WithSpan(33, 21, 33, 28).WithArguments("Method2", "_disposables"),
// Test0.cs(38,21): info ACA0001: Forward execution of "Method2" to member "_disposables"
						Verifier.Diagnostic().WithSpan(38, 21, 38, 28).WithArguments("Method2", "_disposables"),
// Test0.cs(43,21): info ACA0001: Forward execution of "Method3" to member "_disposables"
						Verifier.Diagnostic().WithSpan(43, 21, 43, 28).WithArguments("Method3", "_disposables"),
// Test0.cs(48,21): info ACA0001: Forward execution of "Dispose" to member "_disposables"
						Verifier.Diagnostic().WithSpan(48, 21, 48, 28).WithArguments("Dispose", "_disposables")
					},
				},
				FixedState =
				{
					ExpectedDiagnostics =
					{
						// Test0.cs(11,22): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(11, 22, 11, 38)
							.WithArguments("ConsoleApplication1.ICustomInterface"),
// Test0.cs(13,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(object)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(13, 14, 13, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(object)"),
// Test0.cs(14,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method1(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(14, 14, 14, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method1(string)"),
// Test0.cs(15,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string, int)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(15, 14, 15, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string, int)"),
// Test0.cs(16,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method2(string)'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(16, 14, 16, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method2(string)"),
// Test0.cs(17,14): warning CS1591: Missing XML comment for publicly visible type or member 'ICustomInterface.Method3()'
						DiagnosticResult.CompilerWarning("CS1591").WithSpan(17, 14, 17, 21)
							.WithArguments("ConsoleApplication1.ICustomInterface.Method3()"),
// Test0.cs(22,47): warning CS0649: Field 'TypeName._disposables' is never assigned to, and will always have its default value null
						DiagnosticResult.CompilerWarning("CS0649").WithSpan(22, 47, 22, 59)
							.WithArguments("ConsoleApplication1.TypeName._disposables", "null"),
// Test0.cs(34,17): warning CS0219: The variable 'a' is assigned but its value is never used
						DiagnosticResult.CompilerWarning("CS0219").WithSpan(34, 17, 34, 18).WithArguments("a")
					},
					Sources = {fixtest}
				},
			}.RunAsync();
		}


		[TestMethod]
		public async Task VerifyPartialRewriteWithProperty()
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

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> Disposables { get; set; }

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
    public interface ICustomInterface
    {
        void Method1(object p1);
        void Method1(string p1);
        void Method2(string p1, int p2);
        void Method2(string p1);
        void Method3();
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> Disposables { get; set; }

        public void Method1(object p1)
        {
            foreach (var item in Disposables)
            {
                item.Method1(p1);
            }
        }

        public void Method1(string p1)
        {
            var a = 1;
        }

        public void Method2(string p1, int p2)
        {
            foreach (var item in Disposables)
            {
                item.Method2(p1, p2);
            }
        }

        public void Method2(string p1)
        {
            foreach (var item in Disposables)
            {
                item.Method2(p1);
            }
        }

        public void Method3()
        {
            foreach (var item in Disposables)
            {
                item.Method3();
            }
        }
    }
}";
			var expected = new DiagnosticResult[]
			{
				// Test0.cs(24,21): info ACA0001: Forward execution of "Method1" to member "Disposables"
				Verifier.Diagnostic().WithSpan(24, 21, 24, 28).WithArguments("Method1", "Disposables"),
// Test0.cs(33,21): info ACA0001: Forward execution of "Method2" to member "Disposables"
				Verifier.Diagnostic().WithSpan(33, 21, 33, 28).WithArguments("Method2", "Disposables"),
// Test0.cs(38,21): info ACA0001: Forward execution of "Method2" to member "Disposables"
				Verifier.Diagnostic().WithSpan(38, 21, 38, 28).WithArguments("Method2", "Disposables"),
// Test0.cs(43,21): info ACA0001: Forward execution of "Method3" to member "Disposables"
				Verifier.Diagnostic().WithSpan(43, 21, 43, 28).WithArguments("Method3", "Disposables")
            };

			await Verifier.VerifyCodeFixAsync(test, expected, fixtest);
		}

		// [TestMethod]
		public void MultipleMemberVerification()
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

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> Disposables { get; set; }
        private ICollection<ICustomInterface> Disposables2 { get; set; }

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
    }
}";

			var fixtest1 = @"
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

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> Disposables { get; set; }
        private ICollection<ICustomInterface> Disposables2 { get; set; }

        public void Method1(object p1)
        {
            foreach (var item in Disposables)
            {
                item.Method1(p1);
            }
        }

        public void Method1(string p1)
        {
            var a = 1;
        }

        public void Method2(string p1, int p2)
        {
            foreach (var item in Disposables)
            {
                item.Method2(p1, p2);
            }
        }

        public void Method2(string p1)
        {
            foreach (var item in Disposables)
            {
                item.Method2(p1);
            }
        }

        public void Method3()
        {
            foreach (var item in Disposables)
            {
                item.Method3();
            }
        }
    }
}";

			var fixtest2 = @"
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

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> Disposables { get; set; }
        private ICollection<ICustomInterface> Disposables2 { get; set; }

        public void Method1(object p1)
        {
            foreach (var item in Disposables2)
            {
                item.Method1(p1);
            }
        }

        public void Method1(string p1)
        {
            var a = 1;
        }

        public void Method2(string p1, int p2)
        {
            foreach (var item in Disposables2)
            {
                item.Method2(p1, p2);
            }
        }

        public void Method2(string p1)
        {
            foreach (var item in Disposables2)
            {
                item.Method2(p1);
            }
        }

        public void Method3()
        {
            foreach (var item in Disposables2)
            {
                item.Method3();
            }
        }
    }
}";

		}
	}
}