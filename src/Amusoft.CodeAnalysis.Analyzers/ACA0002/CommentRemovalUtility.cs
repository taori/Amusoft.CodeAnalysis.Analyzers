// Copyright  Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0002
{
	internal static class CommentRemovalUtility
	{
		public static Document RewriteDocument(Document document, SyntaxNode root,
			SyntaxNode targetNode, SyntaxTrivia? replacementTrivia = null)
		{
			replacementTrivia = replacementTrivia ?? SyntaxFactory.ElasticMarker;

			var rewritten = new CommentRemovalRewriter(replacementTrivia.Value).Visit(targetNode);

			return document.WithSyntaxRoot(root.ReplaceNode(targetNode, rewritten));
		}
	}
}