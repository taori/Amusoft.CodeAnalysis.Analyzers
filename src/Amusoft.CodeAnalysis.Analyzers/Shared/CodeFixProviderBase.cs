using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Amusoft.CodeAnalysis.Analyzers.Shared
{
	/// <summary>
	/// <inheritdoc />
	/// </summary>
	public abstract class CodeFixProviderBase : CodeFixProvider
	{
		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		/// <inheritdoc />
		public override ImmutableArray<string> FixableDiagnosticIds => new[] { DiagnosticId }.ToImmutableArray();

		protected abstract string DiagnosticId { get; }

		protected const string TitleAnnotation = "META:CodeFixTitleAnnotation";
		protected const string MemberAnnotation = "META:MemberAnnotation";
		protected const string TypeAnnotation = "META:TypeAnnotation";

		protected abstract string GetEquivalenceKey(SyntaxNode rootNode);
		protected abstract string GetTitle(SyntaxNode rootNode);

		protected virtual async Task<Document> GetFixedDocumentAsync(CodeFixContext context,
			CancellationToken cancellationToken, Diagnostic diagnostic)
		{
			var rootNode = await context.Document.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);
			var fixedRoot = await FixedDiagnosticAsync(rootNode, rootNode.FindNode(diagnostic.Location.SourceSpan), context, cancellationToken)
				.ConfigureAwait(false);

			return context.Document
				.WithSyntaxRoot(fixedRoot);
		}

		protected abstract Task<SyntaxNode> FixedDiagnosticAsync(SyntaxNode rootNode, SyntaxNode diagnosticNode,
			CodeFixContext context, CancellationToken cancellationToken);

		/// <inheritdoc />
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var originalRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			foreach (var diagnostic in context.Diagnostics)
			{
				if (!diagnostic.Id.Equals(DiagnosticId, StringComparison.OrdinalIgnoreCase))
					continue;

				var fixedDocument = await GetFixedDocumentAsync(context, context.CancellationToken, diagnostic)
					.ConfigureAwait(false);
				var fixedRoot = await fixedDocument.GetSyntaxRootAsync(context.CancellationToken)
					.ConfigureAwait(false);

				if(fixedRoot.IsEquivalentTo(originalRoot))
					continue;

				context.RegisterCodeFix(CodeAction.Create(GetTitle(fixedRoot), c => Task.FromResult(fixedDocument), GetEquivalenceKey(fixedRoot)), diagnostic);
			}
		}

		protected string GetAnnotationValue(SyntaxNode rootNode, string annotation, string defaultValue = "unknown")
		{
			return rootNode.GetAnnotations(annotation).FirstOrDefault(d => d.Kind == annotation)?.Data ?? defaultValue;
		}
	}
}