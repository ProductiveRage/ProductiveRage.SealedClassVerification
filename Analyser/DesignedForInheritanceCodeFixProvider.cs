using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProductiveRage.SealedClassVerification.Analyser
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DesignedForInheritanceCodeFixProvider)), Shared]
	public sealed class DesignedForInheritanceCodeFixProvider : CodeFixProvider
	{
		private static readonly LocalizableResourceString sealClassTitle = new LocalizableResourceString(nameof(Resources.SealClass), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableResourceString addAttributeTitle = new LocalizableResourceString(nameof(Resources.AddAttribute), Resources.ResourceManager, typeof(Resources));

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(DesignedForInheritanceAnalyser.DiagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			// Find the class declaration identified by the diagnostic..
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent as ClassDeclarationSyntax;
			if (classDeclaration == null)
				return;

			// If a class has been marked as abstract or sealed then the author has considered the inheritance story and if they've marked it as static then they have
			// avoided the need to (since static classes can't be derived from). If a class doesn't have any of these modifiers then I believe that 99% of the time it
			// should be marked as sealed (and the author has forgotten or not thought about it) - as such, most of time I think that the only code fix that should be
			// available is one that seals the class (if the default was to add the [DesignedForInheritance] then it would be too easy to utilise it as a quick way to
			// get the analyser to be quiet, even when the class HASN'T really been designed to be derived from). However, there are some ways to guess that some
			// thought has gone into how the class may be inherited from - if it has any virtual members then it wouldn't make any sense for it to be sealed (in fact,
			// it would result in an error; CS0549 "'{0}' is a new virtual member in sealed class '{1}') and so the presence of a virtual member should indicate that
			// the appropriate code fix would be to add the [DesignedForInheritance] attribute.
			var virtualMembers = classDeclaration.ChildNodes()
				.Select(node =>
				{
					var method = node as MethodDeclarationSyntax;
					if (method != null)
						return new { Member = node, Modifiers = method.Modifiers };
					var property = node as PropertyDeclarationSyntax;
					if (property != null)
						return new { Member = node, Modifiers = property.Modifiers };
					var field = node as MethodDeclarationSyntax;
					if (field != null)
						return new { Member = node, Modifiers = field.Modifiers };
					return null;
				})
				.Where(nodeWithModifiers => (nodeWithModifiers != null) && nodeWithModifiers.Modifiers.Any(SyntaxKind.VirtualKeyword));
			if (virtualMembers.Any())
			{
				context.RegisterCodeFix(
					CodeAction.Create(
						title: addAttributeTitle.ToString(),
						createChangedDocument: cancellationToken => AddDesignedForInheritanceAttribute(context.Document, classDeclaration, cancellationToken),
						equivalenceKey: "seal"
					),
					diagnostic
				);
			}
			else
			{
				context.RegisterCodeFix(
					CodeAction.Create(
						title: sealClassTitle.ToString(),
						createChangedDocument: cancellationToken => Seal(context.Document, classDeclaration, cancellationToken),
						equivalenceKey: "add-attribute"
					),
					diagnostic
				);
			}
		}

		private async Task<Document> AddDesignedForInheritanceAttribute(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
		{
			var root = await document
				.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);

			var attributeToAdd = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(DesignedForInheritanceAnalyser.AttributeName));
			root = root.ReplaceNode(
				classDeclaration,
				classDeclaration.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attributeToAdd))) // See http://stackoverflow.com/a/37598072/3813189
			);

			var usingDirectives = ((CompilationUnitSyntax)root).Usings;
			if (!usingDirectives.Any(usingDirective => (usingDirective.Alias == null) && (usingDirective.Name.ToFullString() == DesignedForInheritanceAnalyser.AttributeNamespace)))
			{
				root = ((CompilationUnitSyntax)root).WithUsings( // Courtesy of http://stackoverflow.com/a/17677024
					usingDirectives.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(DesignedForInheritanceAnalyser.AttributeNamespace)))
				);
			}

			return document.WithSyntaxRoot(root);
		}

		private async Task<Document> Seal(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
		{
			var root = await document
				.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);
			return document.WithSyntaxRoot(
				root.ReplaceNode(
					classDeclaration,
					classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword))
				)
			);
		}
	}
}
