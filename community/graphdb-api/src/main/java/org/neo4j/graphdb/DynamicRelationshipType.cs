using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Graphdb
{
	/// <summary>
	/// A dynamically instantiated and named <seealso cref="RelationshipType"/>. This class is
	/// a convenience implementation of <code>RelationshipType</code> that is
	/// typically used when relationship types are created and named after a
	/// condition that can only be detected at runtime.
	/// <para>
	/// If all relationship types are known at compile time, it's better to use the
	/// relationship type enum idiom as outlined in <seealso cref="RelationshipType"/>.
	/// </para>
	/// <para>
	/// It's very important to note that a relationship type is uniquely identified
	/// by its name, not by any particular instance that implements this interface.
	/// This means that the proper way to check if two relationship types are equal
	/// is by invoking <code>equals()</code> on their <seealso cref="name() names"/>, NOT by
	/// using Java's identity operator (<code>==</code>) or <code>equals()</code> on
	/// the relationship type instances. A consequence of this is that you can NOT
	/// use relationship types in hashed collections such as
	/// <seealso cref="System.Collections.Hashtable HashMap"/> and <seealso cref="System.Collections.Generic.HashSet<object> HashSet"/>.
	/// </para>
	/// <para>
	/// However, you usually want to check whether a specific relationship
	/// <i>instance</i> is of a certain type. That is best achieved with the
	/// <seealso cref="Relationship.isType Relationship.isType"/> method, such as:
	/// 
	/// <pre>
	/// <code>
	/// <seealso cref="RelationshipType"/> type = DynamicRelationshipType.<seealso cref="withName(string) withName"/>( "myname" );
	/// if ( rel.<seealso cref="Relationship.isType(RelationshipType) isType"/>( type ) )
	/// {
	///     ...
	/// }
	/// </code>
	/// </pre>
	/// </para>
	/// </summary>
	/// @deprecated use <seealso cref="RelationshipType.withName(string)"/> instead 
	[Obsolete("use <seealso cref=\"RelationshipType.withName(string)\"/> instead")]
	public sealed class DynamicRelationshipType : RelationshipType
	{
		 private readonly string _name;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private DynamicRelationshipType(final String name)
		 private DynamicRelationshipType( string name )
		 {
			  if ( string.ReferenceEquals( name, null ) )
			  {
					throw new System.ArgumentException( "A relationship type cannot " + "have a null name" );
			  }
			  this._name = name;
		 }

		 /// <summary>
		 /// Instantiates a new DynamicRelationshipType with the given name.
		 /// There's more information regarding relationship types over at
		 /// <seealso cref="RelationshipType"/>.
		 /// </summary>
		 /// <param name="name"> the name of the dynamic relationship type </param>
		 /// <returns> a DynamicRelationshipType with the given name </returns>
		 /// <exception cref="IllegalArgumentException"> if name is <code>null</code> </exception>
		 /// @deprecated use <seealso cref="RelationshipType.withName(string)"/> instead 
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static DynamicRelationshipType withName(final String name)
		 [Obsolete("use <seealso cref=\"RelationshipType.withName(string)\"/> instead")]
		 public static DynamicRelationshipType WithName( string name )
		 {
			  return new DynamicRelationshipType( name );
		 }

		 /// <summary>
		 /// Returns the name of this relationship type. The name uniquely identifies
		 /// a relationship type, i.e. two different RelationshipType instances with
		 /// different object identifiers (and possibly even different classes) are
		 /// semantically equivalent if they have <seealso cref="String.equals(object) equal"/>
		 /// names.
		 /// </summary>
		 /// <returns> the name of the relationship type </returns>
		 public override string Name()
		 {
			  return this._name;
		 }

		 /// <summary>
		 /// Returns a string representation of this dynamic relationship type.
		 /// </summary>
		 /// <returns> a string representation of this dynamic relationship type </returns>
		 /// <seealso cref= java.lang.Object#toString() </seealso>
		 public override string ToString()
		 {
			  return this._name;
		 }

		 /// <summary>
		 /// Implements the identity-based equals defined by {@link Object
		 /// java.lang.Object}. This means that this dynamic relationship type
		 /// instance will NOT be equal to other relationship types with the same
		 /// name. As outlined in the documentation for {@link RelationshipType
		 /// RelationshipType}, the proper way to check for equivalence between two
		 /// relationship types is to compare their {@link RelationshipType#name()
		 /// names}.
		 /// </summary>
		 /// <returns> <code>true</code> if <code>other</code> is the same instance as
		 ///         this dynamic relationship type, <code>false</code> otherwise </returns>
		 /// <seealso cref= java.lang.Object#equals(java.lang.Object) </seealso>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean equals(final Object other)
		 public override bool Equals( object other )
		 {
			  return base.Equals( other );
		 }

		 /// <summary>
		 /// Implements the default hash function as defined by {@link Object
		 /// java.lang.Object}. This means that if you put a dynamic relationship
		 /// instance into a hash-based collection, it most likely will NOT behave as
		 /// you expect. Please see the documentation of {@link #equals(Object)
		 /// equals} and the <seealso cref="DynamicRelationshipType class documentation"/> for
		 /// more details.
		 /// </summary>
		 /// <returns> a hash code value for this dynamic relationship type instance </returns>
		 /// <seealso cref= java.lang.Object#hashCode() </seealso>
		 public override int GetHashCode()
		 {
			  return base.GetHashCode();
		 }
	}

}