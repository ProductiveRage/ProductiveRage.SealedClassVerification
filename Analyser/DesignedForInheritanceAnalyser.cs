using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProductiveRage.SealedClassVerification.Analyser
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DesignedForInheritanceAnalyser : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "DesignedForInheritance";
		private const string Category = "Design";
		public static DiagnosticDescriptor ClassesMustBeAbstractSealedStaticOrMarkedAsDesignedForInheritanceRule = new DiagnosticDescriptor(
			DiagnosticId,
			title: GetLocalizableString(nameof(Resources.MustUseDesignedForInheritanceDescription)),
			messageFormat: GetLocalizableString(nameof(Resources.MustUseDesignedForInheritanceDescription)),
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);
		public static DiagnosticDescriptor AttributeMustNotBeUsedOnAbstractSealedOrStaticClassRule = new DiagnosticDescriptor(
			DiagnosticId,
			title: GetLocalizableString(nameof(Resources.MustNotUseDesignedForInheritanceDescription)),
			messageFormat: GetLocalizableString(nameof(Resources.MustNotUseDesignedForInheritanceDescription)),
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(
					ClassesMustBeAbstractSealedStaticOrMarkedAsDesignedForInheritanceRule,
					AttributeMustNotBeUsedOnAbstractSealedOrStaticClassRule
				);
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(LookForNonIdentifyingClassDefinitions, SyntaxKind.ClassDeclaration);
		}

		private void LookForNonIdentifyingClassDefinitions(SyntaxNodeAnalysisContext context)
		{
			var classDeclaration = context.Node as ClassDeclarationSyntax;
			if (classDeclaration == null)
				return;

			// If the class is abstract, sealed or static then that's fine - no more work to do
			var isLimitedInScopeByClassModifiers = classDeclaration.Modifiers.Any(modifier =>
			{
				var modifierKind = modifier.Kind();
				return
					(modifierKind == SyntaxKind.AbstractKeyword) ||
					(modifierKind == SyntaxKind.SealedKeyword) ||
					(modifierKind == SyntaxKind.StaticKeyword);
			});

			if (isLimitedInScopeByClassModifiers)
			{
				if (HasDesignedForInheritanceAttribute(classDeclaration, context))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						AttributeMustNotBeUsedOnAbstractSealedOrStaticClassRule,
						classDeclaration.GetLocation(),
						classDeclaration.Identifier.Text
					));
				}
			}
			else
			{
				if (!HasDesignedForInheritanceAttribute(classDeclaration, context))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						ClassesMustBeAbstractSealedStaticOrMarkedAsDesignedForInheritanceRule,
						classDeclaration.GetLocation(),
						classDeclaration.Identifier.Text
					));
				}
			}
		}

		private static bool HasDesignedForInheritanceAttribute(ClassDeclarationSyntax classDeclaration, SyntaxNodeAnalysisContext context)
		{
			if (classDeclaration == null)
				throw new ArgumentNullException(nameof(classDeclaration));

			// If the class has none of those modifiers then look for a [DesignedForInheritance] attribute. If it doesn't have any attributes or doesn't have any whose
			// name is "DesignedForInheritance" or "DesignedForInheritanceAttribute" then we can move on without having done much work. If we find at least one attribute
			// with that name then we need to dig deeper and use the semantic model to determine what attribute class is being referenced (it's possible that someone has
			// created their DesignedForInheritance attribute class somewhere and only the ProductiveRage.SealedClassVerification.DesignedForInheritance should count).
			// Accessing the semantic model is more expensive than working with the syntax data that we have now so we'll only perform that lookup if really necessary.
			// Note: If the DesignedForInheritance wasn't in a Bridge library then the following code would be simpler because we could reference the library from this
			//       analyser and access the type name and the containing namespace more easily (but the ProductiveRage.SealedClassVerification project references the
			//       Bridge library and this analyser project uses the .NET framework and it's not possible for a single project to access both). Instead, we need to
			//       work with fixed strings for the class name and the namespace.
			const string ATTRIBUTE_NAME = "DesignedForInheritance";
			var attributesThatMayBeDesignedForInheritance = classDeclaration.AttributeLists
				.SelectMany(attributeList => attributeList.Attributes)
				.Select(attribute =>
				{
					var name = attribute.Name.ToString().Split('.').Last();
					if ((name == ATTRIBUTE_NAME) || (name == ATTRIBUTE_NAME + "Attribute"))
						return attribute;
					return null;
				})
				.Where(attribute => attribute != null);
			if (!attributesThatMayBeDesignedForInheritance.Any())
				return false;

			// If we've identified any attributes that are named "DesignedForInheritance" (or "DesignedForInheritanceAttribute") then we need to look up where the
			// attribute classes are declared and ensure that they are the genuine "ProductiveRage.SealedClassVerification.DesignedForInheritance" article. Again, if
			// we could reference the DesignedForInheritance then we could compare the full namespace of the referenced class with the namespaces of the attributes
			// here (but we can't because the DesignedForInheritance class is declared in a Bridge project).
			const string ATTRIBUTE_NAMESPACE = "ProductiveRage.SealedClassVerification";
			var attributesThatAreConfirmedToBeDesignedForInheritance = attributesThatMayBeDesignedForInheritance
				.Select(attribute =>
				{
					var attributeType = context.SemanticModel.GetTypeInfo(attribute);
					if (attributeType.Type is IErrorTypeSymbol)
						return null;

					var containingNamespace = attributeType.Type.ContainingNamespace;
					var namespaceSegments = new List<string>();
					while ((containingNamespace != null) && !string.IsNullOrWhiteSpace(containingNamespace.Name))
					{
						namespaceSegments.Insert(0, containingNamespace.Name);
						containingNamespace = containingNamespace.ContainingNamespace;
					}
					if (string.Join(".", namespaceSegments) != ATTRIBUTE_NAMESPACE)
						return null;
					return attribute;
				})
				.Where(attribute => attribute != null);
			return attributesThatAreConfirmedToBeDesignedForInheritance.Any();
		}

		private static LocalizableString GetLocalizableString(string nameOfLocalizableResource)
		{
			return new LocalizableResourceString(nameOfLocalizableResource, Resources.ResourceManager, typeof(Resources));
		}
	}
}
