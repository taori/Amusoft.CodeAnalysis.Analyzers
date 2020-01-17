// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.ACA0001;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using AnalyzerVerifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.AnalyzerVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0001.Analyzer>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Amusoft.CodeAnalysis.Analyzers.ACA0001.Analyzer,
		Amusoft.CodeAnalysis.Analyzers.ACA0001.FixByFowardingToCollectionChildren>;

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

            var diagnostic = AnalyzerVerifier.Diagnostic(Analyzer.DiagnosticId)
	            .WithLocation(5,6);

            AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
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

            var diagnostic = AnalyzerVerifier.Diagnostic(Analyzer.DiagnosticId)
	            .WithLocation(5, 6);

            AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
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

            var diagnostic = AnalyzerVerifier.Diagnostic(Analyzer.DiagnosticId)
	            .WithLocation(5, 6);

            AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
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

            var diagnostic = AnalyzerVerifier.Diagnostic(Analyzer.DiagnosticId)
	            .WithLocation(5, 6);

            AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
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

            var diagnostic = AnalyzerVerifier.Diagnostic(Analyzer.DiagnosticId)
	            .WithLocation(5, 6);

            AnalyzerVerifier.VerifyAnalyzerAsync(test, diagnostic);
        }

        [TestMethod]
        public void VerifyReturnTaskNonBoolSupportedType()
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
	        AnalyzerVerifier.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public void VerifyReturnNoFixUnsupportedType()
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

	        AnalyzerVerifier.VerifyAnalyzerAsync(test);
        }
    }
}