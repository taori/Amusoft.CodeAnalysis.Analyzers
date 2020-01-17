// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Composition;
using System.Linq;
using System.Reflection;
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

namespace Amusoft.CodeAnalysis.Analyzers.CS1998
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "CS1998-FixByWrappingInTaskResult"), Shared]
	public class FixByWrappingInTaskResult : CodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = "CS1998";

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return "CS1998-FixByWrappingInTaskResult";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			return Resources.MessageFormat_CS1998_FixByWrappingInTaskResult;
		}

		/// <inheritdoc />
		protected override async Task<SyntaxNode> FixedDiagnosticAsync(SyntaxNode rootNode, SyntaxNode diagnosticNode,
			CodeFixContext context,
			CancellationToken cancellationToken)
		{
			var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken)
				.ConfigureAwait(false);
			if (diagnosticNode is MethodDeclarationSyntax methodDeclarationSyntax)
			{
				var controlFlowAnalysis = semanticModel.AnalyzeControlFlow(methodDeclarationSyntax.Body);
				if (controlFlowAnalysis.ReturnStatements.Length > 0)
				{
					var documentEditor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
						.ConfigureAwait(false);

					RemoveAsyncFromMethod(documentEditor, methodDeclarationSyntax, semanticModel);

					foreach (var returnStatement in controlFlowAnalysis.ReturnStatements)
					{
						if (returnStatement is ReturnStatementSyntax returnStatementSyntax)
						{
							if (!ShouldRewrite(returnStatementSyntax))
								continue;

							documentEditor.ReplaceNode(returnStatementSyntax, RewriteExit(returnStatementSyntax));
						}
					}

					return await documentEditor.GetChangedDocument().GetSyntaxRootAsync(cancellationToken);
				}
			}

			return rootNode;
		}

		private static void RemoveAsyncFromMethod(DocumentEditor documentEditor, MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel)
		{
			documentEditor.SetModifiers(methodDeclarationSyntax, DeclarationModifiers.From(semanticModel.GetDeclaredSymbol(methodDeclarationSyntax)).WithAsync(false));
		}

		private bool ShouldRewrite(ReturnStatementSyntax returnStatementSyntax)
		{
			if (returnStatementSyntax.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
			{
				if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
				{
					if (memberAccessExpressionSyntax.Name.Identifier.Text.Equals("FromResult")
					&& memberAccessExpressionSyntax.Expression is IdentifierNameSyntax expressionIdentifierName
					&& expressionIdentifierName.Identifier.Text.Equals("Task"))
					{
						return false;
					}
				}
			}

			return true;
		}

		private SyntaxNode RewriteExit(ReturnStatementSyntax returnStatement)
		{
			return returnStatement.WithExpression(
				InvocationExpression(
					MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						IdentifierName("Task"),
						IdentifierName("FromResult")))
				.WithArgumentList(
					ArgumentList(
						SingletonSeparatedList<ArgumentSyntax>(
							Argument(
								returnStatement.Expression))))
				);
		}
	}
}