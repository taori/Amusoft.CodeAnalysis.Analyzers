using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.ACA0001;
using Amusoft.CodeAnalysis.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0001.Analyzer, Amusoft.CodeAnalysis.Analyzers.ACA0001.FixByForwardingToCollectionChildren>;

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
		public async Task VerifyReturnBoolSupportedType()
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
		public async Task VerifyReturnTaskBoolSupportedType()
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
        Task<bool> Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public Task<bool> Method1(object p1)
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
        Task<bool> Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public Task<bool> Method1(object p1)
        {
            return Task.FromResult(_disposables.All(item => item.Method1(p1)));
        }
    }
}";
			var expectedDiagnostics = new[]
			{
				Verifier.Diagnostic(Analyzer.DiagnosticId)
					.WithLocation(20, 27)
					.WithArguments("Method1", "_disposables")
					.WithSeverity(DiagnosticSeverity.Info),
			};

			await Verifier.VerifyCodeFixAsync(test, expectedDiagnostics, fixtest);
		}


		[TestMethod]
		public async Task VerifyReturnTaskBoolAsyncSupportedType()
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
        Task<bool> Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public async Task<bool> Method1(object p1)
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
        Task<bool> Method1(object p1);
    }

    class TypeName : ICustomInterface
    {
        private ICollection<ICustomInterface> _disposables;

        public async Task<bool> Method1(object p1)
        {
            return _disposables.All(item => item.Method1(p1));
        }
    }
}";
			var expectedDiagnostics = new[]
			{
				Verifier.Diagnostic(Analyzer.DiagnosticId)
					.WithLocation(20, 33)
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

            var expectedDiagnostics = new[]
			{
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(24, 21).WithArguments("Method1", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(29, 21).WithArguments("Method1", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(34, 21).WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(39, 21).WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(44, 21).WithArguments("Method3", "_disposables").WithSeverity(DiagnosticSeverity.Info),
			};

			await Verifier.VerifyCodeFixAsync(test, expectedDiagnostics, fixtest);
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

            var expectedDiagnostics = new[]
			{
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(24, 21).WithArguments("Method1", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(33, 21).WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(38, 21).WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(43, 21).WithArguments("Method3", "_disposables").WithSeverity(DiagnosticSeverity.Info),
			};

			await Verifier.VerifyCodeFixAsync(test, expectedDiagnostics, fixtest);
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

            var expectedDiagnostics = new[]
			{
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(24, 21).WithArguments("Method1", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(33, 21).WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(38, 21).WithArguments("Method2", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(43, 21).WithArguments("Method3", "_disposables").WithSeverity(DiagnosticSeverity.Info),
				Verifier.Diagnostic(Analyzer.DiagnosticId).WithLocation(48, 21).WithArguments("Dispose", "_disposables").WithSeverity(DiagnosticSeverity.Info),
			};

			await Verifier.VerifyCodeFixAsync(test, expectedDiagnostics, fixtest);
		}


//         [TestMethod]
//         public void VerifyPartialRewriteWithProperty()
//         {
//             var test = @"
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Diagnostics;
//
// namespace ConsoleApplication1
// {
//     public interface ICustomInterface
//     {
//         void Method1(object p1);
//         void Method1(string p1);
//         void Method2(string p1, int p2);
//         void Method2(string p1);
//         void Method3();
//     }
//
//     class TypeName : ICustomInterface
//     {
//         private ICollection<ICustomInterface> Disposables { get; set; }
//
//         public void Method1(object p1)
//         {
//         }
//
//         public void Method1(string p1)
//         {
//             var a = 1;
//         }
//
//         public void Method2(string p1, int p2)
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Method2(string p1)
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Method3()
//         {
//             throw new NotImplementedException();
//         }
//     }
// }";
//             var expected = new[]
//             {
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 24, 21) }
//                 },
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 33, 21) }
//                 },
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 38, 21) }
//                 },
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 43, 21) }
//                 },
//             };
//
//             // var b = this.Locations()
//             VerifyCSharpDiagnostic(test, expected);
//
//             var fixtest = @"
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Diagnostics;
//
// namespace ConsoleApplication1
// {
//     public interface ICustomInterface
//     {
//         void Method1(object p1);
//         void Method1(string p1);
//         void Method2(string p1, int p2);
//         void Method2(string p1);
//         void Method3();
//     }
//
//     class TypeName : ICustomInterface
//     {
//         private ICollection<ICustomInterface> Disposables { get; set; }
//
//         public void Method1(object p1)
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method1(p1);
//             }
//         }
//
//         public void Method1(string p1)
//         {
//             var a = 1;
//         }
//
//         public void Method2(string p1, int p2)
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method2(p1, p2);
//             }
//         }
//
//         public void Method2(string p1)
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method2(p1);
//             }
//         }
//
//         public void Method3()
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method3();
//             }
//         }
//     }
// }";
//             VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
//         }
//
//         // [TestMethod]
//         public void MultipleMemberVerification()
//         {
//             var test = @"
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Diagnostics;
//
// namespace ConsoleApplication1
// {
//     public interface ICustomInterface
//     {
//         void Method1(object p1);
//         void Method1(string p1);
//         void Method2(string p1, int p2);
//         void Method2(string p1);
//         void Method3();
//     }
//
//     class TypeName : ICustomInterface
//     {
//         private ICollection<ICustomInterface> Disposables { get; set; }
//         private ICollection<ICustomInterface> Disposables2 { get; set; }
//
//         public void Method1(object p1)
//         {
//         }
//
//         public void Method1(string p1)
//         {
//             var a = 1;
//         }
//
//         public void Method2(string p1, int p2)
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Method2(string p1)
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Method3()
//         {
//             throw new NotImplementedException();
//         }
//     }
// }";
//             var expected = new[]
//             {
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 25, 21) }
//                 },
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "Disposables2"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 25, 21) }
//                 },
//                 
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 34, 21) }
//                 },
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables2"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 34, 21) }
//                 },
//                 
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 39, 21) }
//                 },
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables2"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 39, 21) }
//                 },
//                 
//                 new DiagnosticResult
//                 {
//                     Id = Analyzer.DiagnosticId,
//                     Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "Disposables"),
//                     Severity = DiagnosticSeverity.Info,
//                     Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 44, 21) }
//                 },
//                 new DiagnosticResult
//                 {
// 	                Id = Analyzer.DiagnosticId,
// 	                Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "Disposables2"),
// 	                Severity = DiagnosticSeverity.Info,
// 	                Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 44, 21) }
//                 },
//             };
//
//             // var b = this.Locations()
//             VerifyCSharpDiagnostic(test, expected);
//
//             var fixtest1 = @"
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Diagnostics;
//
// namespace ConsoleApplication1
// {
//     public interface ICustomInterface
//     {
//         void Method1(object p1);
//         void Method1(string p1);
//         void Method2(string p1, int p2);
//         void Method2(string p1);
//         void Method3();
//     }
//
//     class TypeName : ICustomInterface
//     {
//         private ICollection<ICustomInterface> Disposables { get; set; }
//         private ICollection<ICustomInterface> Disposables2 { get; set; }
//
//         public void Method1(object p1)
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method1(p1);
//             }
//         }
//
//         public void Method1(string p1)
//         {
//             var a = 1;
//         }
//
//         public void Method2(string p1, int p2)
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method2(p1, p2);
//             }
//         }
//
//         public void Method2(string p1)
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method2(p1);
//             }
//         }
//
//         public void Method3()
//         {
//             foreach (var item in Disposables)
//             {
//                 item.Method3();
//             }
//         }
//     }
// }";
//
//             var fixtest2 = @"
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Diagnostics;
//
// namespace ConsoleApplication1
// {
//     public interface ICustomInterface
//     {
//         void Method1(object p1);
//         void Method1(string p1);
//         void Method2(string p1, int p2);
//         void Method2(string p1);
//         void Method3();
//     }
//
//     class TypeName : ICustomInterface
//     {
//         private ICollection<ICustomInterface> Disposables { get; set; }
//         private ICollection<ICustomInterface> Disposables2 { get; set; }
//
//         public void Method1(object p1)
//         {
//             foreach (var item in Disposables2)
//             {
//                 item.Method1(p1);
//             }
//         }
//
//         public void Method1(string p1)
//         {
//             var a = 1;
//         }
//
//         public void Method2(string p1, int p2)
//         {
//             foreach (var item in Disposables2)
//             {
//                 item.Method2(p1, p2);
//             }
//         }
//
//         public void Method2(string p1)
//         {
//             foreach (var item in Disposables2)
//             {
//                 item.Method2(p1);
//             }
//         }
//
//         public void Method3()
//         {
//             foreach (var item in Disposables2)
//             {
//                 item.Method3();
//             }
//         }
//     }
// }";
//
//             VerifyCSharpFix(test, fixtest1, allowNewCompilerDiagnostics: true, skippedFixIndices:new []{4,5,6,7});
//             VerifyCSharpFix(test, fixtest2, allowNewCompilerDiagnostics: true, skippedFixIndices:new []{0,1,2,3});
//         }

	}
}