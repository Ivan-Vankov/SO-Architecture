using System;
using TypeReferences;

namespace Vaflov {
    /// <summary>
    /// A <see cref="TypeReference"/> that holds names of base type names. Base types are needed to constrain generic
    /// argument selection only to the types that can be chosen as a generic argument.
    /// </summary>
    [Serializable]
    public class TypeReferenceWithBaseTypes : TypeReference {
        public string[] BaseTypeNames;
        public bool TriggerDropdownImmediately;
    }
}