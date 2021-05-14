using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A set of roles.
    /// </summary>
    [Serializable]
    public class RoleSet : IDeserializationCallback, IReadOnlyCollection<string>, ISerializable, ISet<string>
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<string>.Add(string item) => Add(item);

        /// <inheritdoc/>
        public RoleSet() => Roles = new Set();

        /// <inheritdoc/>
        public RoleSet(IEnumerable<string> roles) => Roles = new Set(roles.Select(role => role?.ToUpper() ?? throw new ArgumentNullException(nameof(role))));

        /// <inheritdoc/>
        public RoleSet(SerializationInfo info, StreamingContext context) => Roles = new Set(info, context);

        /// <summary>
        /// The roles.
        /// </summary>
        protected Set Roles { get; }

        /// <summary>
        /// A set of strings.
        /// </summary>
        protected class Set : HashSet<string>
        {
            /// <inheritdoc/>
            public Set()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            /// <inheritdoc/>
            public Set(IEnumerable<string> roles)
                : base(roles, StringComparer.OrdinalIgnoreCase)
            {
            }

            /// <inheritdoc/>
            public Set(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        /// <inheritdoc/>
        public IEnumerator<string> GetEnumerator() => Roles.GetEnumerator();

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public bool Add(string role) => Roles.Add(role?.ToUpper() ?? throw new ArgumentNullException(nameof(role)));

        /// <inheritdoc/>
        public bool Contains(string role) => Roles.Contains(role);

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<string> other) => Roles.IsProperSubsetOf(other);

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<string> other) => Roles.IsProperSupersetOf(other);

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<string> other) => Roles.IsSubsetOf(other);

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<string> other) => Roles.IsSupersetOf(other);

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<string> other) => Roles.Overlaps(other);

        /// <inheritdoc/>
        public bool Remove(string role) => Roles.Remove(role);

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<string> other) => Roles.SetEquals(other);

        /// <inheritdoc/>
        public int Count => Roles.Count;

        /// <inheritdoc/>
        public void Clear() => Roles.Clear();

        /// <inheritdoc/>
        public void CopyTo(string[] array, int arrayIndex) => Roles.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public void ExceptWith(IEnumerable<string> other) => Roles.ExceptWith(other);

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context) => Roles.GetObjectData(info, context);

        /// <inheritdoc/>
        public void IntersectWith(IEnumerable<string> other) => Roles.IntersectWith(other);

        /// <inheritdoc/>
        public void OnDeserialization(object sender) => Roles.OnDeserialization(sender);

        /// <inheritdoc/>
        public void SymmetricExceptWith(IEnumerable<string> other) => Roles.SymmetricExceptWith(other.Select(role => role?.ToUpper() ?? throw new ArgumentNullException(nameof(role))));

        /// <inheritdoc/>
        public void UnionWith(IEnumerable<string> other) => Roles.SymmetricExceptWith(other.Select(role => role?.ToUpper() ?? throw new ArgumentNullException(nameof(role))));
    }
}
