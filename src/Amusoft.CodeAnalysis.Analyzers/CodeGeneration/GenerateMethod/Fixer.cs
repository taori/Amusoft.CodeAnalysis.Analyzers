// Copyright 2019 Andreas Müller
// This file is a part of Amusoft.Roslyn.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.Roslyn.Analyzers/blob/master/LICENSE for details

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amusoft.CodeAnalysis.Analyzers.CodeGeneration.GenerateMethod
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "GenerateMethodFixer"), Shared]
	public class Fixer : CodeFixProvider
	{
		private const string AnnotationMethod = "META:method";
		private const string AnnotationType = "META:type";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CS0407", "CS0123"); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			foreach (var diagnostic in context.Diagnostics)
			{
				if (diagnostic.Id.Equals("CS0407", StringComparison.OrdinalIgnoreCase))
				{
					var canFixResult = await CanFixCs0407Async(context, diagnostic);
					if(!canFixResult.canFix)
						continue;

					context.RegisterCodeFix(
						CodeAction.Create(string.Format(Resources.GenerateMethodFixerCS407MessageFormat, canFixResult.methodName, canFixResult.typeName),
							c => FixCs0407Async(context, c, diagnostic),
							equivalenceKey: "CS0407"), diagnostic);
				}

				if (diagnostic.Id.Equals("CS0123", StringComparison.OrdinalIgnoreCase))
				{
					context.RegisterCodeFix(
						CodeAction.Create(Resources.GenerateMethodFixerMessageFormat,
							c => FixCs0123Async(context, c, diagnostic),
							equivalenceKey: "CS0123"), diagnostic);
				}
			}
		}

		private async Task<(bool canFix, string methodName, string typeName)> CanFixCs0407Async(CodeFixContext context, Diagnostic diagnostic)
		{
			var fixedDocument = await FixCs0407Async(context, context.CancellationToken, diagnostic).ConfigureAwait(false);
			var oldRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var newRoot = await fixedDocument.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var method = newRoot.GetAnnotations(AnnotationMethod).Select(d => d.Data).FirstOrDefault();
			var newType = newRoot.GetAnnotations(AnnotationType).Select(d => d.Data).FirstOrDefault();

			return (!oldRoot.IsEquivalentTo(newRoot), method, newType);
		}

		private async Task<Document> FixCs0123Async(CodeFixContext context, CancellationToken cancellationToken, Diagnostic diagnostic)
		{
			return context.Document;
		}

		private async Task<Document> FixCs0407Async(CodeFixContext context, CancellationToken cancellationToken, Diagnostic diagnostic)
		{
			var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var tree = await context.Document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
			var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root.FindNode(diagnostic.Location.SourceSpan);

			if (!(node is ArgumentSyntax attributeSyntax))
				return context.Document;

			if (!TryGetExpectedTypeSymbol(out ITypeSymbol typeSymbol, node, semanticModel))
				return context.Document;

			var speculativeSymbol = semanticModel.GetSpeculativeSymbolInfo(attributeSyntax.Expression.SpanStart, attributeSyntax.Expression, SpeculativeBindingOption.BindAsExpression);

			if (speculativeSymbol.CandidateReason == CandidateReason.OverloadResolutionFailure &&
				speculativeSymbol.CandidateSymbols.Length > 0)
			{
				var methodDeclarationSyntax = speculativeSymbol.CandidateSymbols[0].DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax;
				var rewritten = methodDeclarationSyntax
					.WithReturnType(SyntaxFactory.IdentifierName(typeSymbol.MetadataName))
					.WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation, Formatter.Annotation, Simplifier.Annotation);

				var newRoot = root.ReplaceNode(methodDeclarationSyntax, rewritten)
					.WithAdditionalAnnotations(
						new SyntaxAnnotation(AnnotationMethod, methodDeclarationSyntax.Identifier.Text),
						new SyntaxAnnotation(AnnotationType, typeSymbol.Name)
						);
				return context.Document
					.WithSyntaxRoot(newRoot);
			}

			return context.Document;
		}

		private bool TryGetExpectedTypeSymbol(out ITypeSymbol symbol, SyntaxNode node, SemanticModel semanticModel)
		{
			symbol = null;
			if (node is ArgumentSyntax argumentSyntax)
			{
				var creationSyntax = argumentSyntax.AncestorsAndSelf().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
				if (creationSyntax == null)
					return false;

				var symbolInfo = semanticModel.GetSymbolInfo(creationSyntax.Type).Symbol;
				if (symbolInfo is INamedTypeSymbol namedType)
				{
					symbol = namedType?.DelegateInvokeMethod?.ReturnType;
					return symbol != null;
				}
			}

			symbol = null;
			return false;
		}


		// class TypeName
		// {
		// 	TypeName()
		// 	{
		// 		var action = new Func<int, string>(TestMethod);
		// 	}
		//
		// 	private int TestMethod(int arg)
		// 	{
		// 		throw new NotImplementedException();
		// 	}
		// }
	}
}
