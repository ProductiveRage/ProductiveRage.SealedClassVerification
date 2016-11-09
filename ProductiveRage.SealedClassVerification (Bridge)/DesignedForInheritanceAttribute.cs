using System;

namespace ProductiveRage.SealedClassVerification
{
	/// <summary>
	/// To ensure that classes have not been identified as being abstract, sealed or static by accident (it is the default state to have none of these modifiers), each such
	/// class should be decorated with this attribute to indicate that they have, in fact, been designed to be derived from
	/// </summary>
	public sealed class DesignedForInheritanceAttribute : Attribute { }
}
