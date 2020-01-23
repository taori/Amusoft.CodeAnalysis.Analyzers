// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System;
using System.Collections.Immutable;
using System.Linq;
using Amusoft.CodeAnalysis.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0006
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class StaticImportAnalyzer : DiagnosticAnalyzer
	{
		#region PrimaryRule

		private static readonly LocalizableString PrimaryRuleTitle = new LocalizableResourceString(
			nameof(Resources.StaticImportAnalyzer_PrimaryRuleTitle),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString PrimaryRuleMessageFormat = new LocalizableResourceString(
			nameof(Resources.StaticImportAnalyzer_PrimaryRuleMessageFormat_0),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString PrimaryRuleDescription = new LocalizableResourceString(
			nameof(Resources.StaticImportAnalyzer_PrimaryRuleDescription),
			Resources.ResourceManager,
			typeof(Resources));

		public static readonly DiagnosticDescriptor PrimaryRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0006.DiagnosticOnStaticClass, PrimaryRuleTitle, PrimaryRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true, description: PrimaryRuleDescription);

		#endregion

		/// <inheritdoc />
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSyntaxNodeAction(AnalyzeMemberAccessExpression,
				syntaxKinds: SyntaxKind.SimpleMemberAccessExpression);
		}

		/// <inheritdoc />
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get { return ImmutableArray.Create(PrimaryRule); }
		}

		private void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
		{
			if (context.Node is MemberAccessExpressionSyntax memberAccessExpression)
			{
				if (!(context.SemanticModel.GetSymbolInfo(memberAccessExpression.Expression).Symbol is INamedTypeSymbol))
					return;

				var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpression.Name).Symbol;
				if (methodSymbol?.ContainingType != null
				    && methodSymbol.ContainingType.IsStatic
				    && methodSymbol.ContainingType.GetMembers()
					    .Count(IsMethodSymbolCandidate) >= 5)
				{
					if (memberAccessExpression.Expression is IdentifierNameSyntax methodClassIdentifier)
					{
						context.ReportDiagnostic(
							Diagnostic.Create(
								PrimaryRule,
								memberAccessExpression.Expression.GetLocation(),
								methodClassIdentifier.Identifier.Text
							)
						);
					}
				}
			}
		}

		private bool IsMethodSymbolCandidate(ISymbol methodSymbolCandidate)
		{
			return methodSymbolCandidate.IsStatic
			       && methodSymbolCandidate.Kind == SymbolKind.Method;
		}
	}
}