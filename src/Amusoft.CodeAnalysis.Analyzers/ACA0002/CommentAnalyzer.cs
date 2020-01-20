// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Collections.Immutable;
using System.Reflection;
using Amusoft.CodeAnalysis.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0002
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CommentAnalyzer : DiagnosticAnalyzer
	{
		#region NamespaceRule

		private static readonly LocalizableString NamespaceRuleTitle = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_NamespaceRuleTitle),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString NamespaceRuleMessageFormat = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_NamespaceRuleMessageFormat),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString NamespaceRuleDescription = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_NamespaceRuleDescription),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0002.DiagnosticOnArray, NamespaceRuleTitle, NamespaceRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true, description: NamespaceRuleDescription);

		#endregion

		#region ClassRule

		private static readonly LocalizableString ClassRuleTitle = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_ClassRuleTitle),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString ClassRuleMessageFormat = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_ClassRuleMessageFormat),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString ClassRuleDescription = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_ClassRuleDescription),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly DiagnosticDescriptor ClassRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0002.DiagnosticOnArray, ClassRuleTitle, ClassRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true, description: ClassRuleDescription);

		#endregion

		#region ArrayRule

		private static readonly LocalizableString ArrayRuleTitle = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_ArrayRuleTitle),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString ArrayRuleMessageFormat = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_ArrayRuleMessageFormat),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString ArrayRuleDescription = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_ArrayRuleDescription),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly DiagnosticDescriptor ArrayRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0002.DiagnosticOnArray, ArrayRuleTitle, ArrayRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true, description: ArrayRuleDescription);

		#endregion

		/// <inheritdoc />
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSyntaxNodeAction(AnalyzeClass, syntaxKinds: SyntaxKind.ClassDeclaration);
			context.RegisterSyntaxNodeAction(AnalyzeArrayInitializer, syntaxKinds: SyntaxKind.ArrayInitializerExpression);
			context.RegisterSyntaxNodeAction(AnalyzeNamespace, syntaxKinds: SyntaxKind.NamespaceDeclaration);
		}

		private void AnalyzeNamespace(SyntaxNodeAnalysisContext context)
		{
			throw new System.NotImplementedException();
		}

		private void AnalyzeArrayInitializer(SyntaxNodeAnalysisContext context)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get { return ImmutableArray.Create(PrimaryRule); }
		}

		private void AnalyzeClass(SyntaxNodeAnalysisContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}