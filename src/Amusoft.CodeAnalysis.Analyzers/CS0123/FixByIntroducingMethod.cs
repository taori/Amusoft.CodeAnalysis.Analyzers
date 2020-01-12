// // Copyright 2020 Andreas Müller
// // This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// // See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.CodeAnalysis.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Amusoft.CodeAnalysis.Analyzers.CS0123
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "CS0123-FixByIntroducingMethod"), Shared]
	public class FixByIntroducingMethod : CodeFixProviderBase
	{
		/// <inheritdoc />
		protected override string DiagnosticId { get; } = "CS0123";

		/// <inheritdoc />
		protected override string GetEquivalenceKey(SyntaxNode rootNode)
		{
			return "CS0123-FixByIntroducingMethod";
		}

		/// <inheritdoc />
		protected override string GetTitle(SyntaxNode rootNode)
		{
			var member = GetAnnotationValue(rootNode, MemberAnnotation);
			var typeName = GetAnnotationValue(rootNode, TypeAnnotation);
			return string.Format(Resources.MessageFormat_CS0407_FixMethodReturnType_0_1, member, typeName);
		}

		/// <inheritdoc />
		protected override async Task<SyntaxNode> FixedDiagnosticAsync(SyntaxNode rootNode, SyntaxNode diagnosticNode,
			CodeFixContext context,
			CancellationToken cancellationToken)
		{
			return rootNode;
		}
	}
}