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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "Amusoft.CodeAnalysis.Analyzers.ACA0002-FixByRemovingNamespaceComments"), Shared]
	public class FixByRemovingNamespaceComments : SingleDiagnosticDocumentCodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = DiagnosticIds.ACA0002.DiagnosticOnNamespace;

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return $"{DiagnosticId}-FixByRemovingNamespaceComments";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			return Resources.CommentAnalyzer_NamespaceRuleMessageFormat;
		}

		/// <inheritdoc />
		protected override async Task<Document> GetFixedDiagnosticAsync(Document document, TextSpan span,
			CancellationToken cancellationToken)
		{
			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);
			var diagnosticNode = syntaxRoot.FindNode(span);

			if (diagnosticNode?.Parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
				return CommentRemovalUtility.RewriteDocument(document, syntaxRoot, namespaceDeclarationSyntax);

			return document;
		}
	}
}