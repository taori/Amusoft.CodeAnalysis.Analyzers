﻿// // Copyright 2020 Andreas Müller
// // This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// // See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.CS0123;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
		Amusoft.CodeAnalysis.Analyzers.CS0123.FixByIntroducingMethod>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CS0123
{
	[TestClass]
	public class FixByMethodInsertionTests
	{
		[TestMethod]
		public async Task NoAction()
		{
			await Verifier.VerifyCodeFixAsync(string.Empty, string.Empty);
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
            var action = new Func<string, int>(TestMethod);
        }

        private int TestMethod(int param1)
        {
            var something = 1;
            return something;
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

        private int TestMethod(int param1)
        {
            var something = 1;
            return something;
        }

        private int TestMethod(string param1)
        {
            throw new NotImplementedException();
        }
    }
}";
			var diagnostic1 = CompilerError("CS0123").WithLocation(15, 26);

			await Verifier.VerifyCodeFixAsync(test, diagnostic1, fixtest);
		}

		[TestMethod]
		public async Task DiagnosticAtObjectCreationExpressionPosition1ParameterTask()
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
            var action = new Func<string, bool, int>(TestMethod);
        }

        private int TestMethod(int param1, bool param2)
        {
            var something = 1;
            return something;
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
            var action = new Func<string, bool, int>(TestMethod);
        }

        private int TestMethod(int param1, bool param2)
        {
            var something = 1;
            return something;
        }

        private int TestMethod(string param1, bool param2)
        {
            throw new NotImplementedException();
        }
    }
}";
			var diagnostic1 = CompilerError("CS0123").WithLocation(15, 26);

			await Verifier.VerifyCodeFixAsync(test, diagnostic1, fixtest);
		}

		[TestMethod]
		public async Task DiagnosticAtObjectCreationExpressionPosition2ParameterTask()
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
            var action = new Func<string, int, int>(TestMethod);
        }

        private int TestMethod(string param1, bool param2)
        {
            var something = 1;
            return something;
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
            var action = new Func<string, int, int>(TestMethod);
        }

        private int TestMethod(string param1, bool param2)
        {
            var something = 1;
            return something;
        }

        private int TestMethod(string param1, int param2)
        {
            throw new NotImplementedException();
        }
    }
}";
			var diagnostic1 = CompilerError("CS0123").WithLocation(15, 26);

			await Verifier.VerifyCodeFixAsync(test, diagnostic1, fixtest);
		}

		[TestMethod]
		public async Task DiagnosticAtObjectCreationExpressionPosition1And2ParameterTask()
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
            var action = new Func<string, string, int>(TestMethod);
        }

        private int TestMethod(bool param1, bool param2)
        {
            var something = 1;
            return something;
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
            var action = new Func<string, string, int>(TestMethod);
        }

        private int TestMethod(bool param1, bool param2)
        {
            var something = 1;
            return something;
        }

        private int TestMethod(string param1, string param2)
        {
            throw new NotImplementedException();
        }
    }
}";
			var diagnostic1 = CompilerError("CS0123").WithLocation(15, 26);

			await Verifier.VerifyCodeFixAsync(test, diagnostic1, fixtest);
		}
	}
}