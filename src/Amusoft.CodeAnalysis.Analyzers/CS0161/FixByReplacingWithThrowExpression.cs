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
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Amusoft.CodeAnalysis.Analyzers.CS0161
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "CS0161-FixByReplacingWithThrowExpression"), Shared]
	public class FixByReplacingWithThrowExpression : SingleDiagnosticDocumentCodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = "CS0161";

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return "CS0161-FixByReplacingWithThrowExpression";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			var member = GetAnnotationValue(rootNode, MemberAnnotation);
			var typeName = GetAnnotationValue(rootNode, TypeAnnotation);
			return string.Format(Resources.MessageFormat_CS0161_FixByReplacingWithThrowExpression, member, typeName);
		}

		/// <inheritdoc />
		protected override async Task<Document> GetFixedDiagnosticAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken)
				.ConfigureAwait(false);
			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);
			var diagnosticNode = syntaxRoot.FindNode(span);

			if (diagnosticNode is MethodDeclarationSyntax methodDeclarationSyntax)
			{
				return await FixMethodAsync(document, semanticModel, methodDeclarationSyntax);
			}

			return document;
		}
		
		private async Task<Document> FixMethodAsync(Document document, SemanticModel semanticModel,
			MethodDeclarationSyntax methodDeclarationSyntax)
		{
			var editor = await DocumentEditor.CreateAsync(document);
			foreach (var conditionStatement in methodDeclarationSyntax.DescendantNodes().OfType<IfStatementSyntax>())
			{
				if (RequiresFix(semanticModel, conditionStatement.Statement))
				{
					editor.ReplaceNode(conditionStatement.Statement,
						GetFixedStatement(conditionStatement.Statement));
				}

				if (RequiresFix(semanticModel, conditionStatement.Else.Statement))
				{
					var fixedStatement = GetFixedStatement(conditionStatement.Else.Statement);
					editor.ReplaceNode(conditionStatement.Else.Statement,
						fixedStatement);
				}
			}

			return editor.GetChangedDocument();
		}

		private SyntaxNode GetFixedStatement(StatementSyntax statementSyntax)
		{
			if (statementSyntax is BlockSyntax blockSyntax)
			{
				var fixedStatement = ThrowStatement(
					ObjectCreationExpression(
						IdentifierName(
							nameof(NotImplementedException)
						)
					).WithArgumentList(ArgumentList())
				);

				var fixedBlock = blockSyntax.Statements.Insert(0, fixedStatement);

				return blockSyntax.WithStatements(fixedBlock);
			}

			// statementSyntax.InsertNodesBefore()
			return statementSyntax.InsertNodesBefore(
				statementSyntax,
				new[]
				{
					ThrowExpression(
						ObjectCreationExpression(
							IdentifierName(nameof(NotImplementedException))))
				}
			);
		}

		private bool RequiresFix(SemanticModel semanticModel, StatementSyntax statementSyntax)
		{
			var controlFlow = semanticModel.AnalyzeControlFlow(statementSyntax);
			return controlFlow.ExitPoints.Length == 0;
		}
	}
}