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
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 13, 47)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
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
			var expected = new DiagnosticResult
			{
				Id = Analyzer.DiagnosticId,
				Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 13, 40)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
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
			var expected = new DiagnosticResult
			{
				Id = Analyzer.DiagnosticId,
				Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 13, 47)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
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

        public interface ICustomInterface
        {
            void Method1(object p1);
            void Method1(string p1);
            void Method2(string p1, int p2);
            void Method2(string p1);
            void Method3();
        }
    }";
			var expected = new DiagnosticResult
			{
				Id = Analyzer.DiagnosticId,
				Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat),
				Severity = DiagnosticSeverity.Info,
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

        public interface ICustomInterface
        {
            void Method1(object p1);
            void Method1(string p1);
            void Method2(string p1, int p2);
            void Method2(string p1);
            void Method3();
        }
    }";
			VerifyCSharpFix(test, fixtest);
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

        public interface ICustomInterface
        {
            void Method1(object p1);
            void Method1(string p1);
            void Method2(string p1, int p2);
            void Method2(string p1);
            void Method3();
        }
    }";
			var expected = new DiagnosticResult
			{
				Id = Analyzer.DiagnosticId,
				Message = string.Format(Resources.DelegateImplementationToFieldAnalyzerMessageFormat),
				Severity = DiagnosticSeverity.Info,
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

        public interface ICustomInterface
        {
            void Method1(object p1);
            void Method1(string p1);
            void Method2(string p1, int p2);
            void Method2(string p1);
            void Method3();
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new Fixer();
		}

		public void Bla()
		{
			throw new NotImplementedException();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new Analyzer();
		}
	}
}