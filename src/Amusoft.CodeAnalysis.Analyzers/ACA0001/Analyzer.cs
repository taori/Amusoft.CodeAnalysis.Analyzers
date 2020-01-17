// Copyright 2018 Andreas Müller
// This file is a part of Amusoft.Roslyn.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.Roslyn.Analyzers/blob/master/LICENSE for details

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Amusoft.CodeAnalysis.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Amusoft.CodeAnalysis.Analyzers.ACA0001
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class Analyzer : DiagnosticAnalyzer
	{
		internal static class Properties
		{
			public const string MemberName = "MemberName";
		}
		
		public const string DiagnosticId = "ACA0001";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DelegateImplementationToFieldAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DelegateImplementationToFieldAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.DelegateImplementationToFieldAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "CodeGeneration";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get { return ImmutableArray.Create(Rule); }
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntax,SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			if (context.Node.IsKind(SyntaxKind.ClassDeclaration))
				AnalyzeClass(context);
		}

		private static Dictionary<ITypeSymbol, HashSet<ISymbol>> GetMemberCandidates(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclarationSyntax)
		{
			var fieldSymbols = classDeclarationSyntax
				.ChildNodes()
				.OfType<FieldDeclarationSyntax>()
				.Select(syntax => context.SemanticModel.GetDeclaredSymbol(syntax.Declaration.Variables.First()) as IFieldSymbol)
				.ToImmutableArray();

			var propertySymbols = classDeclarationSyntax
				.ChildNodes()
				.OfType<PropertyDeclarationSyntax>()
				.Select(syntax => context.SemanticModel.GetDeclaredSymbol(syntax))
				.ToImmutableArray();

			var memberCandidates = new Dictionary<ITypeSymbol, HashSet<ISymbol>>();
			foreach (var fieldSymbol in fieldSymbols)
			{
				if (fieldSymbol.Type.TryGetEnumerableType(context.SemanticModel, out var implementationSymbol))
				{
					var members = memberCandidates.GetOrInitialize(implementationSymbol, d => new HashSet<ISymbol>());
					members.Add(fieldSymbol);
				}
			}

			foreach (var propertySymbol in propertySymbols)
			{
				if (propertySymbol.GetMethod.ReturnType.TryGetEnumerableType(context.SemanticModel, out var implementationSymbol))
				{
					var members = memberCandidates.GetOrInitialize(implementationSymbol, d => new HashSet<ISymbol>());
					members.Add(propertySymbol);
				}
			}

			return memberCandidates;
		}

		private static bool IsMethodCandidate(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodSyntax, Dictionary<ITypeSymbol, HashSet<ISymbol>> memberCandidates)
		{
			bool IsMethodCandidate(ITypeSymbol interfaceSymbol, IMethodSymbol currentMethodSymbol)
			{
				foreach (var memberSymbol in interfaceSymbol.GetMembers())
				{
					if (currentMethodSymbol.ContainingType.FindImplementationForInterfaceMember(memberSymbol).Equals(currentMethodSymbol))
						return true;
				}

				if (interfaceSymbol.AllInterfaces.Any(d => IsMethodCandidate(d, currentMethodSymbol)))
					return true;

				return false;
			}

			var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);
			if (methodSymbol == null)
				return false;

			return memberCandidates.Keys.Any(interfaceSymbol => IsMethodCandidate(interfaceSymbol, methodSymbol));
		}


		private static bool MethodIsEmptyOrNotImplemented(MethodDeclarationSyntax methodDeclarationSyntax)
		{
			if (methodDeclarationSyntax.Body.Statements.Count == 0)
				return true;

			if (methodDeclarationSyntax.Body.Statements.Count == 1 && methodDeclarationSyntax.Body.Statements[0] is ThrowStatementSyntax)
			{
				return true;
			}

			return false;
		}

		private static bool MethodCanBeFixed(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
		{
			if (semanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is IMethodSymbol methodSymbol)
			{
				if (!DiagnosticHelper.TryAnalyzeMethod(semanticModel, methodSymbol, out _, out _))
					return false;

				return true;
			}

			return false;
		}

		public static class DiagnosticHelper
		{
			public static bool TryAnalyzeMethod(SemanticModel semanticModel, IMethodSymbol methodSymbol, out bool returnTask, out bool returnBool)
			{
				returnTask = false;
				returnBool = false;

				if (methodSymbol == null)
					return false;

				if (methodSymbol.ReturnsVoid)
					return true;

				var boolType = semanticModel.Compilation.GetTypeByMetadataName("System.Boolean");
				if (methodSymbol.ReturnType.Equals(boolType))
				{
					returnBool = true;
					return true;
				}

				var taskGenericType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
				if (methodSymbol.ReturnType is INamedTypeSymbol ntMethodSymbol && ntMethodSymbol.ConstructedFrom.Equals(taskGenericType))
				{
					returnTask = true;
					returnBool = ntMethodSymbol.TypeArguments[0].Equals(boolType);
					return returnBool;
				}

				return false;
			}
		}

		private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
		{
			if (context.Node is ClassDeclarationSyntax classDeclarationSyntax && context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, context.CancellationToken) is var classSymbol)
			{
				if (classSymbol.AllInterfaces.Length == 0)
					return;

				var memberCandidates = GetMemberCandidates(context, classDeclarationSyntax);

				var methods = classDeclarationSyntax
					.ChildNodes()
					.OfType<MethodDeclarationSyntax>()
					.Where(MethodIsEmptyOrNotImplemented)
					.Where(d => MethodCanBeFixed(context.SemanticModel, d))
					.Where(d => IsMethodCandidate(context, d, memberCandidates))
					.ToImmutableArray();
				
				foreach (var interfaceSymbol in classSymbol.AllInterfaces)
				{
					if (memberCandidates.TryGetValue(interfaceSymbol, out var matchingMembers))
					{
						foreach (var memberSymbol in matchingMembers)
						{
							AttachDiagnostics(context, memberSymbol, methods);
						}
					}
				}
			}
		}

		private static void AttachDiagnostics(SyntaxNodeAnalysisContext context, ISymbol memberSymbol, ImmutableArray<MethodDeclarationSyntax> methods)
		{
			foreach (var methodDeclarationSyntax in methods)
			{
				foreach (var syntaxReference in memberSymbol.DeclaringSyntaxReferences)
				{
					var syntax = syntaxReference.GetSyntax();

					if (syntax is VariableDeclaratorSyntax variableDeclarator)
					{
						context.ReportDiagnostic(Diagnostic.Create(
							Rule, 
							methodDeclarationSyntax.Identifier.GetLocation(),
							new Dictionary<string, string>(){{Properties.MemberName, variableDeclarator.Identifier.Text}}.ToImmutableDictionary(),
							methodDeclarationSyntax.Identifier.Text, variableDeclarator.Identifier.Text));
					}

					if (syntax is PropertyDeclarationSyntax propertyDeclarationSyntax)
					{
						context.ReportDiagnostic(Diagnostic.Create(
							Rule,
							methodDeclarationSyntax.Identifier.GetLocation(),
							new Dictionary<string, string>() { { Properties.MemberName, propertyDeclarationSyntax.Identifier.Text } }.ToImmutableDictionary(),
							methodDeclarationSyntax.Identifier.Text, propertyDeclarationSyntax.Identifier.Text));
					}
				}
			}
		}
	}
}
