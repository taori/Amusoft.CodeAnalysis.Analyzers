﻿using System.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Amusoft.CodeAnalysis.Analyzers.CS0407
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "CS0407-FixByReplacingMethodReturnType"), Shared]
	public class FixByReplacingMethodReturnType : SingleDiagnosticDocumentCodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = "CS0407";

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return "CS0407-FixByChangingReturnTypeOfMethod";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			var member = GetAnnotationValue(rootNode, MemberAnnotation);
			var typeName = GetAnnotationValue(rootNode, TypeAnnotation);
			return string.Format(Resources.MessageFormat_CS0407_FixByRewritingReturnType_0_1_2, member, typeName);
		}

		/// <inheritdoc />
		protected override async Task<Document> GetFixedDiagnosticAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken)
				.ConfigureAwait(false);
			var rootNode = await document.GetSyntaxRootAsync(cancellationToken);
			var diagnosticNode = rootNode.FindNode(span);

			if (!SymbolHelper.TryGetExpectedMethodSymbol(out var typeSymbol, out var memberSymbolInfo, diagnosticNode, semanticModel))
				return document;

			if (memberSymbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure &&
			    memberSymbolInfo.CandidateSymbols.Length > 0)
			{
				var methodDeclarationSyntax = memberSymbolInfo.CandidateSymbols[0].DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax;

				var rewritten = methodDeclarationSyntax
					.WithReturnType(SyntaxFactory.IdentifierName(typeSymbol.ReturnType.MetadataName))
					.WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation, Formatter.Annotation, Simplifier.Annotation);

				var newRoot = rootNode.ReplaceNode(methodDeclarationSyntax, rewritten)
					.WithAdditionalAnnotations(
						new SyntaxAnnotation(MemberAnnotation, methodDeclarationSyntax.Identifier.Text),
						new SyntaxAnnotation(TypeAnnotation, typeSymbol.ReturnType.Name)
					);

				return document.WithSyntaxRoot(newRoot);
			}

			return document;
		}
	}
}