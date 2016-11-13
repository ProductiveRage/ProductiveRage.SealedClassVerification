using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ProductiveRage.SealedClassVerification.Analyser.Test
{
	[TestClass]
	public class DesignedForInheritanceCodeFixTests : CodeFixVerifier
	{
		[TestMethod]
		public void SealClassShouldBeMostCommonCodeFix()
		{
			var testContent = @"
				namespace TestCase
				{
					public class Example { }
				}";

			var expected = new DiagnosticResult
			{
				Id = DesignedForInheritanceAnalyser.DiagnosticId,
				Message = DesignedForInheritanceAnalyser.ClassesMustBeAbstractSealedStaticOrMarkedAsDesignedForInheritanceRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 4, 6)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);

			var fixContent = @"
				namespace TestCase
				{
					public sealed class Example { }
				}";

			VerifyCSharpFix(GetStringForCodeFixComparison(testContent), GetStringForCodeFixComparison(fixContent));
		}

		[TestMethod]
		public void IfClassHasAnyVirtualMembersThenAssumeThatInheritanceHasBeenConsideredAndOfferToAddDesignedForInheritanceAttribute()
		{
			var testContent = @"
				namespace TestCase
				{
					public class Example
					{
						public virtual string GetName()
						{
							return ""Default"";
						}
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = DesignedForInheritanceAnalyser.DiagnosticId,
				Message = DesignedForInheritanceAnalyser.ClassesMustBeAbstractSealedStaticOrMarkedAsDesignedForInheritanceRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 4, 6)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);

			var fixContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					[DesignedForInheritance]
					public class Example
					{
						public virtual string GetName()
						{
							return ""Default"";
						}
					}
				}";

			// Although a ProductiveRage.SealedClassVerification metadata reference is added to solutions in DiagnosticVerifier, VerifyCSharpFix results in compilation errors
			// about the "ProductiveRage.SealedClassVerification" namespace and "DesignedForInheritance" attribute being unavailable. I don't know why so I'm just cheating and
			// telling the test method to ignore any new compiler warnings / errors (by passed true for the allowNewCompilerDiagnostics argument).
			VerifyCSharpFix(
				GetStringForCodeFixComparison(testContent),
				GetStringForCodeFixComparison(fixContent),
				allowNewCompilerDiagnostics: true
			);
		}

		[TestMethod]
		public void IfAddingAttributeAndNamespaceIsAlreadyImportedThenDoNotAddDuplicateImport()
		{
			var testContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					public class Example
					{
						public virtual string GetName()
						{
							return ""Default"";
						}
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = DesignedForInheritanceAnalyser.DiagnosticId,
				Message = DesignedForInheritanceAnalyser.ClassesMustBeAbstractSealedStaticOrMarkedAsDesignedForInheritanceRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 6, 6)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);

			var fixContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					[DesignedForInheritance]
					public class Example
					{
						public virtual string GetName()
						{
							return ""Default"";
						}
					}
				}";

			// See comment in IfClassHasAnyVirtualMembersThenAssumeThatInheritanceHasBeenConsideredAndOfferToAddDesignedForInheritanceAttribute as to why allowNewCompilerDiagnostics
			// is being set to true
			VerifyCSharpFix(
				GetStringForCodeFixComparison(testContent),
				GetStringForCodeFixComparison(fixContent),
				allowNewCompilerDiagnostics: true
			);
		}

		/// <summary>
		/// When the code fix adds lines, it uses spaces instead of tabs (which I use in the files here) and so it's easiest to just replace tabs with runs of four spaces before
		/// making comparisons between before and after values. The strings in this file are also indented so that they appear "within" the containing method, rather than being
		/// aligned to the zero column in the editor, but this offset will not respected by lines added by the code fix - so it's just easiest to remove the offset from each
		/// line before comparing. This will also remove any whitespace from the start and end of strings so that any leading or trailiing line returns are ignored.
		/// </summary>
		private static string GetStringForCodeFixComparison(string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			const string _contentWhitespaceOffset = "				";
			var whitespaceAdjustedLines = value
				.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
				.Select(line => line.StartsWith(_contentWhitespaceOffset) ? line.Substring(_contentWhitespaceOffset.Length) : line)
				.Select(line => line.Replace("\t", "    "));
			return string.Join(Environment.NewLine, whitespaceAdjustedLines).Trim();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new DesignedForInheritanceAnalyser();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new DesignedForInheritanceCodeFixProvider();
		}
	}
}