﻿// // Copyright 2020 Andreas Müller
// // This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// // See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System;
using System.Composition;
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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amusoft.CodeAnalysis.Analyzers.CS0123
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "CS0123-FixByRewritingParameters"), Shared]
	public class FixByRewritingParameters : SingleDiagnosticDocumentCodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = "CS0123";

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return "CS0123-FixByRewritingParameters";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			var member = GetAnnotationValue(rootNode, MemberAnnotation);
			return string.Format(Resources.MessageFormat_CS0123_FixByRewritingParameters_0, member);
		}

		/// <inheritdoc />
		protected override async Task<Document> GetFixedDiagnosticAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken)
				.ConfigureAwait(false);
			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);
			var diagnosticNode = syntaxRoot.FindNode(span);

			if (!SymbolHelper.TryGetExpectedMethodSymbol(out var methodSymbol, out var memberSymbolInfo, diagnosticNode, semanticModel))
				return document;

			if (memberSymbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure &&
			    memberSymbolInfo.CandidateSymbols.Length > 0)
			{
				var methodDeclarationSyntax = memberSymbolInfo.CandidateSymbols[0].DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax;

				var rewritten = methodDeclarationSyntax
					.WithReturnType(IdentifierName(methodSymbol.ReturnType.MetadataName))
					.WithParameterList(CreateParameterList(semanticModel, methodSymbol, methodDeclarationSyntax))
					.WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation, Formatter.Annotation, Simplifier.Annotation);

				var newRoot = syntaxRoot.ReplaceNode(methodDeclarationSyntax, new[] { rewritten })
					.WithAdditionalAnnotations(
						new SyntaxAnnotation(MemberAnnotation, methodDeclarationSyntax.Identifier.Text),
						new SyntaxAnnotation(TypeAnnotation, methodSymbol.ReturnType.Name)
					);

				return document.WithSyntaxRoot(newRoot);
			}

			return document;
		}
		
		private ParameterListSyntax CreateParameterList(SemanticModel semanticModel, IMethodSymbol methodSymbol,
			MethodDeclarationSyntax methodDeclarationSyntax)
		{
			var parameterList = methodDeclarationSyntax.ParameterList;
			var newParameters = new SeparatedSyntaxList<ParameterSyntax>();
			for (var index = 0; index < parameterList.Parameters.Count; index++)
			{
				var originalParameter = parameterList.Parameters[index];
				var newParameter = originalParameter;
				var targetTypeSymbol = methodSymbol.Parameters[index];
				var sourceTypeSymbol = semanticModel.GetSymbolInfo(originalParameter.Type).Symbol;
				if (sourceTypeSymbol == null)
					goto add;
				if (sourceTypeSymbol.Equals(targetTypeSymbol))
					goto add;

				newParameter = originalParameter.WithType(IdentifierName(targetTypeSymbol.Type.MetadataName));

				add:
				newParameters = newParameters.Add(newParameter);
			}

			return parameterList.WithParameters(newParameters);
		}
	}
}