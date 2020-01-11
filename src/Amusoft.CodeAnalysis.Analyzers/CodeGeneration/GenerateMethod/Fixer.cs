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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amusoft.CodeAnalysis.Analyzers.CodeGeneration.GenerateMethod
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "GenerateMethodFixer"), Shared]
	public class Fixer : CodeFixProvider 
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CS0407", "CS0123"); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			foreach (var diagnostic in context.Diagnostics)
			{
				if (diagnostic.Id.Equals("CS0407", StringComparison.OrdinalIgnoreCase))
					context.RegisterCodeFix(
						CodeAction.Create(Resources.GenerateMethodFixerMessageFormat,
							c => FixCs0407Async(context, c, diagnostic), 
							equivalenceKey: "CS1503"), diagnostic);

				if (diagnostic.Id.Equals("CS0123", StringComparison.OrdinalIgnoreCase))
					context.RegisterCodeFix(
						CodeAction.Create(Resources.GenerateMethodFixerMessageFormat,
							c => FixCs0123Async(context, c, diagnostic), 
							equivalenceKey: "CS0123"), diagnostic);
			}

			return Task.CompletedTask;
		}

		private async Task<Document> FixCs0123Async(CodeFixContext context, CancellationToken cancellationToken, Diagnostic diagnostic)
		{
			return context.Document;
		}

		private async Task<Document> FixCs0407Async(CodeFixContext context, CancellationToken cancellationToken, Diagnostic diagnostic)
		{
			if (!context.Document.TryGetSemanticModel(out var semanticModel))
				return context.Document;

			var tree = await context.Document.GetSyntaxTreeAsync(cancellationToken);
			var root = await tree.GetRootAsync(cancellationToken);
			var node = root.FindNode(diagnostic.Location.SourceSpan);

			if (!(node is ArgumentSyntax attributeSyntax))
				return context.Document;

			
			var speculativeSymbol = semanticModel.GetSpeculativeSymbolInfo(attributeSyntax.Expression.SpanStart, attributeSyntax.Expression, SpeculativeBindingOption.BindAsExpression);

			if (speculativeSymbol.CandidateReason == CandidateReason.OverloadResolutionFailure &&
			    speculativeSymbol.CandidateSymbols.Length > 0)
			{
				var syntax = speculativeSymbol.CandidateSymbols[0].DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax;
				var spanStart = syntax.Span.Start;
				var bindingOption = SpeculativeBindingOption.BindAsTypeOrNamespace;
				var bcd = semanticModel.GetSpeculativeTypeInfo(attributeSyntax.Span.Start, attributeSyntax, bindingOption);
				var speculativeSymbolInfo = semanticModel.GetSpeculativeSymbolInfo(attributeSyntax.Span.Start, attributeSyntax, bindingOption);
			}
			if (node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault() is var
				methodDeclarationSyntax)
			{

			}

			return context.Document;
		}
	}
}
