// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Amusoft.CodeAnalysis.Analyzers.Shared
{
	public static class SymbolHelper
	{
		public static bool TryGetExpectedMethodSymbol(out IMethodSymbol symbol, out SymbolInfo memberSymbolInfo, SyntaxNode node, SemanticModel semanticModel)
		{
			symbol = null;
			memberSymbolInfo = default;

			if (node is ArgumentSyntax argumentSyntax)
			{
				var creationSyntax = argumentSyntax.AncestorsAndSelf().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
				if (creationSyntax == null)
					return false;

				var symbolInfo = semanticModel.GetSymbolInfo(creationSyntax.Type);
				var localSymbol = symbolInfo.Symbol;
				if (localSymbol is INamedTypeSymbol namedType)
				{
					memberSymbolInfo = semanticModel.GetSpeculativeSymbolInfo(argumentSyntax.Expression.SpanStart, argumentSyntax.Expression, SpeculativeBindingOption.BindAsExpression);

					symbol = namedType.DelegateInvokeMethod;
					return symbol != null;
				}
			}

			if (node is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
			{
				// return TryGetExpectedTypeMemberSymbol(out symbol, out memberSymbolInfo, objectCreationExpressionSyntax.ArgumentList.Arguments[0], semanticModel);

				var typeSymbolInfo = semanticModel.GetSymbolInfo(objectCreationExpressionSyntax.Type);
				if (typeSymbolInfo.Symbol is INamedTypeSymbol namedTypeSymbol)
				{
					if (objectCreationExpressionSyntax.ArgumentList.Arguments.Count <= 0)
						return false;
					var localArgumentSyntax = objectCreationExpressionSyntax.ArgumentList.Arguments[0];
					symbol = namedTypeSymbol.DelegateInvokeMethod;
					var symbolInfo = semanticModel.GetSpeculativeSymbolInfo(
						localArgumentSyntax.Expression.SpanStart,
						localArgumentSyntax.Expression, SpeculativeBindingOption.BindAsExpression);
					memberSymbolInfo = symbolInfo;
					return symbol != null && objectCreationExpressionSyntax.ArgumentList.Arguments.Count > 0;
				}
			}

			symbol = null;
			return false;
		}
	}
}