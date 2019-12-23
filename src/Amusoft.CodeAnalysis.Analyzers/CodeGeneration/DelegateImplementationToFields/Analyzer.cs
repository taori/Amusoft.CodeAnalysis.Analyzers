// Copyright 2018 Andreas Müller
// This file is a part of Amusoft.Roslyn.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.Roslyn.Analyzers/blob/master/LICENSE for details

using System.Collections.Immutable;
using System.Linq;
using Amusoft.CodeAnalysis.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Amusoft.CodeAnalysis.Analyzers.CodeGeneration.DelegateImplementationToFields
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class Analyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "ACACG0003";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.GeneratePocoAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.GeneratePocoAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.GeneratePocoAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "CodeGeneration";

		private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get { return ImmutableArray.Create(Rule); }
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntax, 
				SyntaxKind.PropertyDeclaration, SyntaxKind.FieldDeclaration);
		}

		private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			if (context.Node.IsKind(SyntaxKind.PropertyDeclaration))
				AnalyzePropertySyntax(context);
			if (context.Node.IsKind(SyntaxKind.FieldDeclaration))
				AnalyzeFieldDeclarationSyntax(context);
		}


		private static bool MemberOfClassIsCandidate(SyntaxNodeAnalysisContext context, INamedTypeSymbol memberTypeSymbol, SyntaxNode memberSyntax)
		{
			if (memberTypeSymbol.TryGetEnumerableType(context.SemanticModel, out var implementationType))
			{
				var classDeclarationSyntax = memberSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
				if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is var classSymbol)
				{
					if (classSymbol.AllInterfaces.Contains(implementationType))
					{
						return true;
					}
				}
			}

			return false;
		}
		private static void AnalyzeFieldDeclarationSyntax(SyntaxNodeAnalysisContext context)
		{
			if (context.Node is FieldDeclarationSyntax fieldDeclarationSyntax)
			{
				if (context.ContainingSymbol is IFieldSymbol fieldSymbol)
				{
					if (MemberOfClassIsCandidate(context, fieldSymbol.Type as INamedTypeSymbol, fieldDeclarationSyntax))
					{
						context.ReportDiagnostic(Diagnostic.Create(Rule,
							fieldDeclarationSyntax.Declaration.Variables.First().GetLocation(),
							fieldDeclarationSyntax.Declaration.Variables.First().Identifier.Text));
					}
				}
			}
		}

		private static void AnalyzePropertySyntax(SyntaxNodeAnalysisContext context)
		{
			if (context.Node is PropertyDeclarationSyntax propertyDeclarationSyntax)
			{
				if (context.ContainingSymbol is IPropertySymbol propertySymbol)
				{
					if (MemberOfClassIsCandidate(context, propertySymbol.GetMethod.ReturnType as INamedTypeSymbol, propertyDeclarationSyntax))
					{
						context.ReportDiagnostic(Diagnostic.Create(Rule,
							propertyDeclarationSyntax.Identifier.GetLocation(),
							propertyDeclarationSyntax.Identifier.Text));
					}
				}
			}
		}
	}
}
