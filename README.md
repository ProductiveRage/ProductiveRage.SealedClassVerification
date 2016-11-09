# ProductiveRage.SealedClassVerification

In "[Writing React apps using Bridge.NET - The Dan Way (Part Three)](http://www.productiverage.com/writing-react-apps-using-bridgenet-the-dan-way-part-three)" I said that I believe that 99% of classes should be abstract, sealed or static and that leaving them open for inheritance requires special consideration and thought to ensure that they are correctly designed to be derived from. Unfortunately, this is the default case in C# - if a class is not sealed then it may be inherited from, whether the author planned for it or not (which is unlike methods which are *not* overloadable by default, they require the "virtual" keyword to be explicitly added to them).

In an answer to the question "[What are five things you hate about your favorite language?](http://stackoverflow.com/a/282342/3813189)", the legendary Jon Skeet said that he wished that

> "Classes should be sealed by default"

.. to which [Rasmus Faber](http://stackoverflow.com/users/5542/rasmus-faber replied that 

>  I think a better solution.. would be a DesignedForInheritanceAttribute and a warning from the compiler when a class derives from it anyway

Since it's likely impossible to determine through static analysis whether a class that is not abstract, sealed or static was intentionally written in that state or whether the author didn't think about it, I think that this would be an excellent compromise!

This library declares a [DesignedForInheritance] attribute and includes an analyser to verify that classes are abstract, sealed, static *or* they have this attribute on them.

Currently (as of November 2016), this is only available for Bridge.NET projects but I intend to create a version of the library that will work with projects that use the .NET framework as well.