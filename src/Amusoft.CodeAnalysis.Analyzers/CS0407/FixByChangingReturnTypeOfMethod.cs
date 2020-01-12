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

namespace Amusoft.CodeAnalysis.Analyzers.CodeGeneration.CS0407
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "CS0407-FixByChangingReturnTypeOfMethod"), Shared]
	public class FixByChangingReturnTypeOfMethod : CodeFixProviderBase
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
			return string.Format(Resources.MessageFormat_CS0407_FixMethodReturnType_0_1, member, typeName);
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

		/// <inheritdoc />
		protected override async Task<SyntaxNode> FixedDiagnosticAsync(SyntaxNode rootNode, SyntaxNode diagnosticNode, CodeFixContext context,
			CancellationToken cancellationToken)
		{
			var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (!(diagnosticNode is ArgumentSyntax attributeSyntax))
				return rootNode;

			if (!TryGetExpectedTypeSymbol(out ITypeSymbol typeSymbol, attributeSyntax, semanticModel))
				return rootNode;

			var speculativeSymbol = semanticModel.GetSpeculativeSymbolInfo(attributeSyntax.Expression.SpanStart, attributeSyntax.Expression, SpeculativeBindingOption.BindAsExpression);

			if (speculativeSymbol.CandidateReason == CandidateReason.OverloadResolutionFailure &&
			    speculativeSymbol.CandidateSymbols.Length > 0)
			{
				var methodDeclarationSyntax = speculativeSymbol.CandidateSymbols[0].DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax;

				var rewritten = methodDeclarationSyntax
					.WithReturnType(SyntaxFactory.IdentifierName(typeSymbol.MetadataName))
					.WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation, Formatter.Annotation, Simplifier.Annotation);

				var newRoot = rootNode.ReplaceNode(methodDeclarationSyntax, rewritten)
					.WithAdditionalAnnotations(
						new SyntaxAnnotation(MemberAnnotation, methodDeclarationSyntax.Identifier.Text),
						new SyntaxAnnotation(TypeAnnotation, typeSymbol.Name)
					);

				return newRoot;
			}

			return rootNode;
		}
	}
}