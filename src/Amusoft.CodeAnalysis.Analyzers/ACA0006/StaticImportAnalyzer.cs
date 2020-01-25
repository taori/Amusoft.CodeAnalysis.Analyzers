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
		private static readonly LocalizableString PrimaryRuleTitle = new LocalizableResourceString(
			nameof(Resources.StaticImportAnalyzer_PrimaryRuleTitle),
			Resources.ResourceManager,
			typeof(Resources));

		#region PrimaryRule

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
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get { return ImmutableArray.Create(PrimaryRule); }
		}

		/// <inheritdoc />
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSemanticModelAction(AnalyzeSemanticModel);
		}

		private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
		{
			var model = context.SemanticModel;
			var root = model.SyntaxTree.GetRoot(context.CancellationToken);
			var memberAccessExpressions = root
				.DescendantNodes()
				.OfType<MemberAccessExpressionSyntax>();

			var filteredSymbols = memberAccessExpressions
				.Select(expression =>
					(
						typeSymbol: model.GetSymbolInfo(expression.Expression).Symbol as INamedTypeSymbol,
						memberSymbol: model.GetSymbolInfo(expression.Name).Symbol,
						expression: expression
					)
				)
				.Where(tuple => tuple.typeSymbol != null
				                && tuple.typeSymbol.IsStatic
				                && tuple.memberSymbol != null
				                && tuple.memberSymbol.IsStatic);

			var grouped = filteredSymbols.ToLookup(tuple => tuple.typeSymbol);
			foreach (var group in grouped)
			{
				if (grouped[group.Key].Count() > 4)
				{
					if (grouped[group.Key].FirstOrDefault() is var firstType)
					{
						if (firstType.expression.Expression is IdentifierNameSyntax classNameSyntax)
						{
							context.ReportDiagnostic(
								Diagnostic.Create(
									PrimaryRule,
									firstType.expression.Expression.GetLocation(),
									classNameSyntax.Identifier.Text
								)
							);
						}
					}
				}
			}
		}
	}
}