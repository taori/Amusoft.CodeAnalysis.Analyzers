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
using Microsoft.CodeAnalysis.Text;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0002
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "Amusoft.CodeAnalysis.Analyzers.ACA0002-FixByRemovingClassComments"), Shared]
	public class FixByRemovingClassComments : SingleDiagnosticDocumentCodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = DiagnosticIds.ACA0002.DiagnosticOnClass;

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return $"{DiagnosticId}-FixByRemovingClassComments";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			var member = GetAnnotationValue(rootNode, MemberAnnotation);
			var typeName = GetAnnotationValue(rootNode, TypeAnnotation);
			return string.Format(Resources.CommentAnalyzer_ClassRuleMessageFormat, member, typeName);
		}

		/// <inheritdoc />
		protected override async Task<Document> GetFixedDiagnosticAsync(Document document, TextSpan span,
			CancellationToken cancellationToken)
		{
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken)
				.ConfigureAwait(false);
			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);
			var diagnosticNode = syntaxRoot.FindNode(span);

			return document;
		}
	}
}