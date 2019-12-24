using System;
using System.Collections.Generic;
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

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void FieldAsListWithDelegation()
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

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void FieldWithInvalidDelegation()
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

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void PropertyWithDelegation()
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

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void VerifyFullRewrite()
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
			var expected = new[]
			{
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 24, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 29, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 34, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 39, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 44, 21) }
				},
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
			VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
		}

        [TestMethod]
        public void VerifyDiagnosticMethodFiltering()
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
            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "_disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 24, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 33, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 38, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "_disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 43, 21) }
                },
            };

            // var b = this.Locations()
            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
		public void VerifyPartialRewrite()
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
			var expected = new[]
			{
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 24, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 33, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 38, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 43, 21) }
				},
			};

			// var b = this.Locations()
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
			VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
		}
        
        [TestMethod]
		public void VerifyInheritedInterfaceAnalyzers()
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
			var expected = new[]
			{
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 24, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 33, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 38, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 43, 21) }
				},
				new DiagnosticResult
				{
					Id = Analyzer.DiagnosticId,
					Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Dispose", "_disposables"),
					Severity = DiagnosticSeverity.Info,
					Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 48, 21) }
				},
			};

			// var b = this.Locations()
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
			VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
		}


        [TestMethod]
        public void VerifyPartialRewriteWithProperty()
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
            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 24, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 33, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 38, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 43, 21) }
                },
            };

            // var b = this.Locations()
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
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
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
            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 25, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method1", "Disposables2"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 25, 21) }
                },
                
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 34, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables2"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 34, 21) }
                },
                
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 39, 21) }
                },
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method2", "Disposables2"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 39, 21) }
                },
                
                new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "Disposables"),
                    Severity = DiagnosticSeverity.Info,
                    Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 44, 21) }
                },
                new DiagnosticResult
                {
	                Id = Analyzer.DiagnosticId,
	                Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat, "Method3", "Disposables2"),
	                Severity = DiagnosticSeverity.Info,
	                Locations = new DiagnosticResultLocation[]{ ("Test0.cs", 44, 21) }
                },
            };

            // var b = this.Locations()
            VerifyCSharpDiagnostic(test, expected);

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

            VerifyCSharpFix(test, fixtest1, allowNewCompilerDiagnostics: true, skippedFixIndices:new []{4,5,6,7});
            VerifyCSharpFix(test, fixtest2, allowNewCompilerDiagnostics: true, skippedFixIndices:new []{0,1,2,3});
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