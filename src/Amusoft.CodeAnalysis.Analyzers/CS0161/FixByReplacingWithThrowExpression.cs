// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Amusoft.CodeAnalysis.Analyzers.CS0161
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "CS0161-FixByReplacingWithThrowExpression"), Shared]
	public class FixByReplacingWithThrowExpression : CodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = "CS0161";

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return "CS0161-FixByReplacingWithThrowExpression";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			var member = GetAnnotationValue(rootNode, MemberAnnotation);
			var typeName = GetAnnotationValue(rootNode, TypeAnnotation);
			return string.Format(Resources.MessageFormat_CS0161_FixByReplacingWithThrowExpression, member, typeName);
		}

		/// <inheritdoc />
		protected override async Task<SyntaxNode> FixedDiagnosticAsync(SyntaxNode rootNode, SyntaxNode diagnosticNode,
			CodeFixContext context,
			CancellationToken cancellationToken)
		{
			return rootNode;
		}
	}
}