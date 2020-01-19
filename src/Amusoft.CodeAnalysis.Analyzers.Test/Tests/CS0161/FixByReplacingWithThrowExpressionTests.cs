// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.CodeAnalysis.Testing.DiagnosticResult;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
		Amusoft.CodeAnalysis.Analyzers.CS0161.FixByReplacingWithThrowExpression>;

namespace Amusoft.CodeAnalysis.Analyzers.Test.Tests.CS0161
{
	[TestClass]
	public class FixByReplacingWithThrowExpressionTests
	{
		[TestMethod]
		public async Task EmptySourceNoAction()
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
	public class Test
	{
	    public static int Main() // CS0161
	    {
	        int i = 5;
	        if (i < 10)
	        {
	            return i;
	        }
	        else
	        {
	            // Uncomment the following line to resolve.
	            // return 1;  
	        }
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
	public class Test
	{
	    public static int Main() // CS0161
	    {
	        int i = 5;
	        if (i < 10)
	        {
	            return i;
	        }
	        else
	        {
	            throw new NotImplementedException();
	            // Uncomment the following line to resolve.
	            // return 1;  
	        }
	    }
	}
}";
			var diagnostics = new DiagnosticResult[]
			{
			};

			await Verifier.VerifyCodeFixAsync(test, diagnostics, fixtest);
		}
	}
}