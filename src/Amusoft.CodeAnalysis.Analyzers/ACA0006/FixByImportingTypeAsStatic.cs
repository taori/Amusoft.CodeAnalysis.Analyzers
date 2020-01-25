// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System;
using System.Collections.Generic;
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

			var diagnosticExpression = diagnosticNode
				.AncestorsAndSelf()
				.OfType<MemberAccessExpressionSyntax>()
				.FirstOrDefault();
			if (diagnosticExpression == null)
				return document;

			var diagnosticTypeSymbol = semanticModel.GetSymbolInfo(diagnosticExpression.Name).Symbol?.ContainingSymbol;

			var matchingExpressions = syntaxRoot
				.DescendantNodes()
				.OfType<MemberAccessExpressionSyntax>()
				.Select(expression => (expression, methodSymbol: semanticModel.GetSymbolInfo(expression.Name).Symbol))
				.Where(tuple => tuple.methodSymbol.IsStatic && tuple.methodSymbol.ContainingSymbol.Equals(diagnosticTypeSymbol));

			var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
				.ConfigureAwait(false);

			var usingDirective = UsingDirective(
					ParseName(diagnosticTypeSymbol.ToString()))
				.WithStaticKeyword(Token(SyntaxKind.StaticKeyword))
				.WithAdditionalAnnotations(Formatter.Annotation);
			var lastDirective = syntaxRoot.DescendantNodes().OfType<UsingDirectiveSyntax>().LastOrDefault();
			
			if (!UsingDirectiveAlreadyExists(syntaxRoot, usingDirective))
			{
				editor.InsertAfter(lastDirective, usingDirective);
			}

			foreach (var accessExpressionTuple in matchingExpressions)
			{
				var accessExpressionSyntax = accessExpressionTuple.expression;
				var accessExpressionMethodSymbol = accessExpressionTuple.methodSymbol;
				var typeSymbol = accessExpressionMethodSymbol.ContainingType;
				if (typeSymbol == null)
					return document;

				var replacement = GetReplacementNode(semanticModel, accessExpressionSyntax);
				if (replacement == null)
					return document;

				var replacementTarget = GetInvocationTarget(semanticModel, accessExpressionSyntax);

				editor.ReplaceNode(replacementTarget, replacement);
			}

			return editor.GetChangedDocument();
		}

		private bool UsingDirectiveAlreadyExists(SyntaxNode syntaxRoot,
			UsingDirectiveSyntax newDirective)
		{
			return syntaxRoot
				.DescendantNodes()
				.OfType<UsingDirectiveSyntax>()
				.Any(directive => !directive.StaticKeyword.IsMissing
				                  && directive.Name.IsEquivalentTo(newDirective.Name));
		}

		private SyntaxNode GetInvocationTarget(SemanticModel semanticModel,
			MemberAccessExpressionSyntax accessExpressionSyntax)
		{
			if (accessExpressionSyntax.Parent is InvocationExpressionSyntax parentInvocationExpressionSyntax)
				return parentInvocationExpressionSyntax;

			return accessExpressionSyntax;
		}

		private SyntaxNode GetReplacementNode(SemanticModel semanticModel,
			MemberAccessExpressionSyntax accessExpressionSyntax)
		{
			var memberSymbol = semanticModel.GetSymbolInfo(accessExpressionSyntax.Name).Symbol;
			if (memberSymbol != null)
			{
				if (memberSymbol is IMethodSymbol methodSymbol)
				{
					if (accessExpressionSyntax.Parent is InvocationExpressionSyntax invocationExpression)
						return InvocationExpression(
								IdentifierName(accessExpressionSyntax.Name.Identifier.Text),
								invocationExpression.ArgumentList
							)
							.WithAdditionalAnnotations(Formatter.Annotation);
				}

				if (memberSymbol is IFieldSymbol || memberSymbol is IPropertySymbol)
					return IdentifierName(accessExpressionSyntax.Name.Identifier)
						.WithAdditionalAnnotations(Formatter.Annotation);
			}

			return null;
		}
	}
}