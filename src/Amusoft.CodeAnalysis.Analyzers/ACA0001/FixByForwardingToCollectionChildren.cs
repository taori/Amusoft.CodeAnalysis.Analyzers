﻿// Copyright 2019 Andreas Müller
// This file is a part of Amusoft.Roslyn.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.Roslyn.Analyzers/blob/master/LICENSE for details

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
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0001
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "ACA0001-FixByForwardingToCollectionChildren"), Shared]
	public class FixByForwardingToCollectionChildren : CodeFixProvider
	{
		private const string CodeFixUniqueKey = "ACA0001-FixByForwardingToCollectionChildren";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Analyzer.DiagnosticId); }
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
				var title = diagnostic.Descriptor.MessageFormat.ToString();
				if (diagnostic.Properties.TryGetValue(Analyzer.Properties.MemberName, out var memberName))
					context.RegisterCodeFix(
						CodeAction.Create(title, c => CreateChangedDocument(context, c, diagnostic),
							CodeFixUniqueKey + memberName), diagnostic);
			}

			return Task.CompletedTask;
		}

		private async Task<Document> CreateChangedDocument(CodeFixContext context, CancellationToken cancellationToken,
			Diagnostic diagnostic)
		{
			var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root.FindNode(diagnostic.Location.SourceSpan);
			var methodNode = node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
			if (methodNode != null)
			{
				if (!diagnostic.Properties.TryGetValue(Analyzer.Properties.MemberName, out var memberName))
					return context.Document;

				var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken);
				var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);
				if (methodSymbol == null)
					return context.Document;

				var newMethodBody = CreateMethodBody(semanticModel, methodNode, memberName, methodSymbol);
				var rewrittenMethod = methodNode
					.WithBody(newMethodBody)
					.WithExpressionBody(null)
					.WithSemicolonToken(Token(SyntaxKind.None))
					.WithAdditionalAnnotations(Formatter.Annotation);

				var replacedRoot = root.ReplaceNode(methodNode, rewrittenMethod);

				return context.Document.WithSyntaxRoot(replacedRoot);
			}

			return context.Document;
		}

		private static BlockSyntax CreateMethodBody(SemanticModel semanticModel,
			MethodDeclarationSyntax methodNode, string memberName, IMethodSymbol methodSymbol)
		{
			Analyzer.DiagnosticHelper.TryAnalyzeMethod(semanticModel, methodSymbol, out var returnTask,
				out var returnBool);

			if (returnBool)
			{
				return RewriteAsBoolMethod(methodNode, memberName);
			}

			if (returnTask)
			{
				return RewriteAsTaskMethod(methodNode, memberName);
			}

			return RewriteAsVoidMethod(methodNode, memberName);
		}

		private static BlockSyntax RewriteAsBoolMethod(MethodDeclarationSyntax methodNode,
			string memberName)
		{
			return Block(
				SingletonList<StatementSyntax>(
					ReturnStatement(
						InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
								IdentifierName(memberName), IdentifierName("All")))
							.WithArgumentList(
								ArgumentList(
									SingletonSeparatedList(Argument(
										SimpleLambdaExpression(Parameter(Identifier("item")),
											CreateIterationCall(methodNode))))))
					)
				)
			);
		}

		private static BlockSyntax RewriteAsTaskMethod(MethodDeclarationSyntax methodNode,
			string memberName)
		{
			if (methodNode.Modifiers.Any(SyntaxKind.AsyncKeyword))
			{
				return Block(SingletonList<StatementSyntax>(
						ExpressionStatement(
							AwaitExpression(
								InvocationExpression(
									MemberAccessExpression(
										SyntaxKind.SimpleMemberAccessExpression,
										IdentifierName("Task"),
										IdentifierName("WhenAll")
									),
									ArgumentList(
										SingletonSeparatedList(
											Argument(
												InvocationExpression(
													MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
														IdentifierName(memberName),
														IdentifierName("Select")
													),
													ArgumentList(
														SingletonSeparatedList(
															Argument(
																SimpleLambdaExpression(
																	Parameter(
																		Identifier("item")
																	),
																	CreateIterationCall(methodNode)
																)
															)
														)
													)
												)
											)
										)
									)
								)
							)
						)
					)
				);
			}
			else
			{
				return Block(SingletonList<StatementSyntax>(
						ReturnStatement(
							InvocationExpression(
								MemberAccessExpression(
									SyntaxKind.SimpleMemberAccessExpression,
									IdentifierName("Task"),
									IdentifierName("WhenAll")
								),
								ArgumentList(
									SingletonSeparatedList(
										Argument(
											InvocationExpression(
												MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
													IdentifierName(memberName),
													IdentifierName("Select")
												),
												ArgumentList(
													SingletonSeparatedList(
														Argument(
															SimpleLambdaExpression(
																Parameter(
																	Identifier("item")
																),
																CreateIterationCall(methodNode)
															)
														)
													)
												)
											)
										)
									)
								)
							)
						)
					)
				);
			}
		}

		private static BlockSyntax RewriteAsVoidMethod(MethodDeclarationSyntax methodNode,
			string memberName)
		{
			return Block(
				SingletonList<StatementSyntax>(
					ForEachStatement(
						IdentifierName("var"),
						Identifier("item"),
						IdentifierName(memberName),
						Block(
							SingletonList<StatementSyntax>(
								ExpressionStatement(
									CreateIterationCall(methodNode))))
					)
				)
			);
		}

		private static InvocationExpressionSyntax CreateIterationCall(MethodDeclarationSyntax methodNode)
		{
			if (methodNode.ParameterList.Parameters.Any())
			{
				return InvocationExpression(
					MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						IdentifierName("item"),
						IdentifierName(methodNode.Identifier.Text)
					)).WithArgumentList(
					ArgumentList(
						SeparatedList<ArgumentSyntax>(CreateMethodArguments(methodNode)
						)));
			}
			else
			{
				return InvocationExpression(
					MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						IdentifierName("item"),
						IdentifierName(methodNode.Identifier.Text)
					));
			}
		}

		private static IEnumerable<ArgumentSyntax> CreateMethodArguments(MethodDeclarationSyntax methodNode)
		{
			return methodNode.ParameterList.Parameters.Select(d => Argument(IdentifierName(d.Identifier.Text)));
		}
	}
}