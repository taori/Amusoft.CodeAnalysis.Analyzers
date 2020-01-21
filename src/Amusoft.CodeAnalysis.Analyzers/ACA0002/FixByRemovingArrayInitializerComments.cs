// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System;
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
using SyntaxTrivia = Microsoft.CodeAnalysis.SyntaxTrivia;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0002
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "Amusoft.CodeAnalysis.Analyzers.ACA0002-FixByRemovingArrayInitializerComments"), Shared]
	public class FixByRemovingArrayInitializerComments : SingleDiagnosticDocumentCodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = DiagnosticIds.ACA0002.DiagnosticOnArray;

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return $"{DiagnosticId}-FixByRemovingArrayInitializerComments";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			return Resources.CommentAnalyzer_ArrayRuleCodeFixMessage;
		}

		/// <inheritdoc />
		protected override async Task<Document> GetFixedDiagnosticAsync(Document document, TextSpan span,
			CancellationToken cancellationToken)
		{
			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);

			// SyntaxTrivia replacementTrivia = ElasticCarriageReturnLineFeed;
			var comments = syntaxRoot.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));

			var replaced = syntaxRoot
				.ReplaceTrivia(comments, (b, a) => ElasticMarker);
			
			return document.WithSyntaxRoot(replaced);

			// if (syntaxRoot.FindNode(span) is ImplicitArrayCreationExpressionSyntax implicitSyntaxNode)
			// 	return CommentRemovalUtility.RewriteDocument(document, syntaxRoot, implicitSyntaxNode, replacementTrivia);
			//
			// if (syntaxRoot.FindNode(span) is ArrayCreationExpressionSyntax arrayCreationSyntax)
			// 	return CommentRemovalUtility.RewriteDocument(document, syntaxRoot, arrayCreationSyntax, replacementTrivia);
			//
			// if (syntaxRoot.FindNode(span) is StackAllocArrayCreationExpressionSyntax stackAllocExpressionSyntax)
			// 	return CommentRemovalUtility.RewriteDocument(document, syntaxRoot, stackAllocExpressionSyntax, replacementTrivia);
			//
			// return document;
		}
	}
}