using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Amusoft.CodeAnalysis.Analyzers.Extensions
{
	public static class ContextExtensions
	{
		public static async Task<SyntaxNode> GetDiagnosticNodeAsync(this CodeFixContext context, Diagnostic diagnostic)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			return root.FindNode(diagnostic.Location.SourceSpan);
		}
	}
}