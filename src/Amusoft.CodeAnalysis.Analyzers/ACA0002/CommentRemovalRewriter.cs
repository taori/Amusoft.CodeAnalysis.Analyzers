// Copyright  Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0002
{
	internal class CommentRemovalRewriter : CSharpSyntaxRewriter
	{
		/// <inheritdoc />
		public CommentRemovalRewriter(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
		{
		}

		/// <inheritdoc />
		public CommentRemovalRewriter(SyntaxTrivia replacementTrivia, bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
		{
			ReplacementTrivia = replacementTrivia;
		}

		public SyntaxTrivia ReplacementTrivia { get; set; } = SyntaxFactory.ElasticMarker; 

		/// <inheritdoc />
		public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
		{
			if(trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
				return ReplacementTrivia;

			return base.VisitTrivia(trivia);
		}
	}
}