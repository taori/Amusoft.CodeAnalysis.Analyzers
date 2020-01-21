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
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0006
{
	[ExportCodeFixProvider(LanguageNames.CSharp,
		 Name = "Amusoft.CodeAnalysis.Analyzers.ACA0006-FixByImportingTypeAsStatic"), Shared]
	public class FixByImportingTypeAsStatic : SingleDiagnosticDocumentCodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = StaticImportAnalyzer.PrimaryRule.Id;

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return $"{DiagnosticId}-FixByImportingTypeAsStatic";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			var typeName = GetAnnotationValue(rootNode, TypeAnnotation);
			return string.Format(Resources.StaticImportAnalyzer_PrimaryRuleMessageFormat_0, typeName);
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

			var accessExpressionSyntax = diagnosticNode
				.AncestorsAndSelf()
				.OfType<MemberAccessExpressionSyntax>()
				.FirstOrDefault();

			if (accessExpressionSyntax == null)
				return document;

			var typeSymbol = semanticModel.GetSymbolInfo(accessExpressionSyntax.Name).Symbol?.ContainingType;
			if (typeSymbol == null)
				return document;

			var usingDirective = UsingDirective(
					ParseName(typeSymbol.ToString()))
				.WithStaticKeyword(Token(SyntaxKind.StaticKeyword))
				.WithAdditionalAnnotations(Formatter.Annotation);

			IdentifierNameSyntax s = IdentifierName("bla");
			var lastDirective = syntaxRoot.DescendantNodes().OfType<UsingDirectiveSyntax>().LastOrDefault();
			var replacedEnd = InvocationExpression(
				ParseExpression(accessExpressionSyntax.Name.Identifier.Text), ArgumentList()
			);

			var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
				.ConfigureAwait(false);
			editor.InsertAfter(lastDirective, usingDirective);
			editor.ReplaceNode(accessExpressionSyntax,
				replacedEnd);

			var updated = syntaxRoot
				.InsertNodesAfter(lastDirective, new[] {usingDirective})
				.ReplaceNode(accessExpressionSyntax, replacedEnd);

			return document.WithSyntaxRoot(updated);
			return editor.GetChangedDocument();
		}
	}
}