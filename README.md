# ProductiveRage.SealedClassVerification

In "[Writing React apps using Bridge.NET - The Dan Way (Part Three)](http://www.productiverage.com/writing-react-apps-using-bridgenet-the-dan-way-part-three)" I said that I believe that 99% of classes should be abstract, sealed or static and that leaving them open for inheritance requires special consideration and thought to ensure that they are correctly designed to be derived from. Unfortunately, this is the default case in C# - if a class is not sealed then it may be inherited from, whether the author planned for it or not (which is unlike methods which are *not* overloadable by default, they require the "virtual" keyword to be explicitly added to them).

In an answer to the question "[What are five things you hate about your favorite language?](http://stackoverflow.com/a/282342/3813189)", the legendary Jon Skeet said that

> "Classes should be sealed by default"

.. to which [Rasmus Faber](http://stackoverflow.com/users/5542/rasmus-faber) replied that 

>  I think a better solution.. would be a DesignedForInheritanceAttribute and a warning from the compiler when a class derives from it anyway

Since it's likely impossible to determine through static analysis whether a class that is not abstract, sealed or static was intentionally written in that state or whether the author didn't think about it, I think that this would be an excellent compromise!

This library declares a **[DesignedForInheritance]** attribute and includes an analyser to verify that classes are abstract, sealed, static *or* they have this attribute on them.

## NuGet package availability

This project has been built into two packages -

* for [Bridge.NET](http://bridge.net/): ([ProductiveRage.SealedClassVerification.Bridge](https://www.nuget.org/packages/ProductiveRage.SealedClassVerification.Bridge))

* for .NET 4.5: ([ProductiveRage.SealedClassVerification.Net](https://www.nuget.org/packages/ProductiveRage.SealedClassVerification.Net))

## Examples

The following class will be highlighted with a warning -

	// This is not allowed - is the class really not abstract, sealed or static by design or is it just because
	// the author didn't give any thought to it? If it's the former then use the [DesignedForInheritance] attribute
	// to tell the analyser (and future code maintainers) that it was a conscious decision. If it's the latter then
	// then the warning should encourage the expenditure of a little thought on the matter! (Probably it should be
	// sealed until the sorts of facilities that should be available to derived classes are better understood).
	
	public class Example { }
	
> Any class that is abstract, sealed or static may not be decorated with a [DesignedForInheritance] attribute since it is not applicable (sealed and static classes may not be inherited from and abstract classes may be presumed to have been explicitly designed for inheritance)
	
All of the following are considered fine -

	[DesignedForInheritance]
	public class Example { }

	public abstract class Example { }

	public sealed class Example { }

	public static class Example { }

Note that you may not use the **DesignedForInheritance** on classes that are abstract, sealed or static as a statement has already been made about their extensibility (or lack of). As such, a **DesignedForInheritance** attribute on them is most likely a mistake and the class will be highlighted with a warning -

> Any class that is not abstract, sealed or static should be decorated with a [DesignedForInheritance] attribute to indicate that this was a conscious decision and it has not been left in this state unintentionally