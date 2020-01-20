// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Amusoft.CodeAnalysis.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

		public static readonly DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0002.DiagnosticOnNamespace, NamespaceRuleTitle, NamespaceRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true,
			description: NamespaceRuleDescription);

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

		public static readonly DiagnosticDescriptor ClassRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0002.DiagnosticOnClass, ClassRuleTitle, ClassRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true, description: ClassRuleDescription);

		#endregion

		#region MethodRule

		private static readonly LocalizableString MethodRuleTitle = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_MethodRuleTitle),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString MethodRuleMessageFormat = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_MethodRuleMessageFormat),
			Resources.ResourceManager,
			typeof(Resources));

		private static readonly LocalizableString MethodRuleDescription = new LocalizableResourceString(
			nameof(Resources.CommentAnalyzer_MethodRuleDescription),
			Resources.ResourceManager,
			typeof(Resources));

		public static readonly DiagnosticDescriptor MethodRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0002.DiagnosticOnMethod, MethodRuleTitle, MethodRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true, description: MethodRuleDescription);

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

		public static readonly DiagnosticDescriptor ArrayRule = new DiagnosticDescriptor(
			DiagnosticIds.ACA0002.DiagnosticOnArray, ArrayRuleTitle, ArrayRuleMessageFormat,
			"ACA Diagnostics", DiagnosticSeverity.Info, isEnabledByDefault: true, description: ArrayRuleDescription);

		#endregion

		/// <inheritdoc />
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(NamespaceRule, ClassRule, MethodRule, ArrayRule);

		/// <inheritdoc />
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSyntaxNodeAction(AnalyzeNamespace, syntaxKinds: SyntaxKind.NamespaceDeclaration);
			context.RegisterSyntaxNodeAction(AnalyzeClass, syntaxKinds: SyntaxKind.ClassDeclaration);
			context.RegisterSyntaxNodeAction(AnalyzeMethod, syntaxKinds: SyntaxKind.MethodDeclaration);
			context.RegisterSyntaxNodeAction(AnalyzeArrayInitializer,
				syntaxKinds: SyntaxKind.ArrayInitializerExpression);
		}

		private bool HasTrivia(SyntaxNode node)
		{
			return node.DescendantTrivia().Any(d =>
				d.IsKind(SyntaxKind.MultiLineCommentTrivia)
				|| d.IsKind(SyntaxKind.SingleLineCommentTrivia)
			);
		}

		private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
		{
			if (HasTrivia(context.Node) && context.Node is MethodDeclarationSyntax syntax)
			{
				context.ReportDiagnostic(Diagnostic.Create(MethodRule, syntax.Identifier.GetLocation()));
			}
		}

		private void AnalyzeNamespace(SyntaxNodeAnalysisContext context)
		{
			if (HasTrivia(context.Node) && context.Node is NamespaceDeclarationSyntax syntax)
			{
				context.ReportDiagnostic(Diagnostic.Create(NamespaceRule, syntax.Name.GetLocation()));
			}
		}

		private void AnalyzeArrayInitializer(SyntaxNodeAnalysisContext context)
		{
			if (HasTrivia(context.Node) && context.Node is InitializerExpressionSyntax syntax)
			{
				if (syntax.Parent is ImplicitArrayCreationExpressionSyntax implicitArray)
					context.ReportDiagnostic(Diagnostic.Create(ArrayRule, implicitArray.NewKeyword.GetLocation()));
				if (syntax.Parent is ArrayCreationExpressionSyntax explicitArray)
					context.ReportDiagnostic(Diagnostic.Create(ArrayRule, explicitArray.NewKeyword.GetLocation()));
			}
		}

		private void AnalyzeClass(SyntaxNodeAnalysisContext context)
		{
			if (HasTrivia(context.Node) && context.Node is ClassDeclarationSyntax syntax)
			{
				context.ReportDiagnostic(Diagnostic.Create(ClassRule, syntax.Identifier.GetLocation()));
			}
		}
	}
}