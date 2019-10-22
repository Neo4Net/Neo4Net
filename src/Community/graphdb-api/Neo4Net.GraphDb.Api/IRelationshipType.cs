/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.GraphDb
{
	using TraversalDescription = Neo4Net.GraphDb.traversal.TraversalDescription;

	/// <summary>
	/// A relationship type is mandatory on all relationships and is used to navigate
	/// the graph. RelationshipType is in particular a key part of the
	/// <seealso cref="TraversalDescription traverser framework"/> but it's also used in various
	/// <seealso cref="INode.getRelationships() relationship operations"/> on Node.
	/// <para>
	/// Relationship types are declared by the client and can be handled either
	/// dynamically or statically in a Neo4Net-based application. Internally,
	/// relationship types are dynamic. This means that every time a client invokes
	/// {@link Node#createRelationshipTo(Node,RelationshipType)
	/// node.createRelationshipTo(anotherNode, newRelType)} and passes in a new
	/// relationship type then the new type will be transparently created. So
	/// instantiating a RelationshipType instance will not create it in the
	/// underlying storage, it is persisted only when the first relationship of that
	/// type is created.
	/// </para>
	/// <para>
	/// However, in case the application does not need to dynamically create
	/// relationship types (most don't), then it's nice to have the compile-time
	/// benefits of a static set of relationship types. Fortunately, RelationshipType
	/// is designed to work well with Java 5 enums. This means that it's very easy to
	/// define a set of valid relationship types by declaring an enum that implements
	/// RelationshipType and then reuse that across the application. For example,
	/// here's how you would define an enum to hold all your relationship types:
	/// 
	/// <pre>
	/// <code>
	/// enum MyRelationshipTypes implements <seealso cref="IRelationshipType"/>
	/// {
	///     CONTAINED_IN, KNOWS
	/// }
	/// </code>
	/// </pre>
	/// 
	/// Then later, it's as easy to use as:
	/// 
	/// <pre>
	/// <code>
	/// node.{@link Node#createRelationshipTo(Node, RelationshipType)
	/// createRelationshipTo}( anotherNode, <seealso cref="IRelationshipType MyRelationshipTypes.KNOWS"/> );
	/// for ( <seealso cref="IRelationship"/> rel : node.{@link Node#getRelationships(RelationshipType...)
	/// getRelationships}( MyRelationshipTypes.KNOWS ) )
	/// {
	///     // ...
	/// }
	/// </code>
	/// </pre>
	/// 
	/// </para>
	/// <para>
	/// It's very important to note that a relationship type is uniquely identified
	/// by its name, not by any particular instance that implements this interface.
	/// This means that the proper way to check if two relationship types are equal
	/// is by invoking <code>equals()</code> on their <seealso cref="name names"/>, NOT by
	/// using Java's identity operator (<code>==</code>) or <code>equals()</code> on
	/// the relationship type instances. A consequence of this is that you can NOT
	/// use relationship types in hashed collections such as
	/// <seealso cref="System.Collections.Hashtable HashMap"/> and <seealso cref="System.Collections.Generic.HashSet<object> HashSet"/>.
	/// </para>
	/// <para>
	/// However, you usually want to check whether a specific relationship
	/// <i>instance</i> is of a certain type. That is best achieved with the
	/// <seealso cref="IRelationship.isType Relationship.isType"/> method, such as:
	/// <pre>
	/// <code>
	/// if ( rel.isType( MyRelationshipTypes.CONTAINED_IN ) )
	/// {
	///     ...
	/// }
	/// </code>
	/// </pre>
	/// </para>
	/// </summary>
	public interface IRelationshipType
	{
		 /// <summary>
		 /// Returns the name of the relationship type. The name uniquely identifies a
		 /// relationship type, i.e. two different RelationshipType instances with
		 /// different object identifiers (and possibly even different classes) are
		 /// semantically equivalent if they have <seealso cref="String.equals(object) equal"/>
		 /// names.
		 /// </summary>
		 /// <returns> the name of the relationship type </returns>
		 string Name();

		 /// <summary>
		 /// Instantiates a new <seealso cref="IRelationshipType"/> with the given name.
		 /// </summary>
		 /// <param name="name"> the name of the dynamic relationship type </param>
		 /// <returns> a <seealso cref="IRelationshipType"/> with the given name </returns>
		 /// <exception cref="IllegalArgumentException"> if name is {@code null} </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static RelationshipType withName(String name)
	//	 {
	//		  if (name == null)
	//		  {
	//				throw new IllegalArgumentException("A relationship type cannot have a null name");
	//		  }
	//		  return new RelationshipType()
	//		  {
	//				@@Override public String name()
	//				{
	//					 return name;
	//				}
	//
	//				@@Override public String toString()
	//				{
	//					 return name;
	//				}
	//
	//				@@Override public boolean equals(Object that)
	//				{
	//					 if (this == that)
	//					 {
	//						  return true;
	//					 }
	//					 if (that == null || that.getClass() != getClass())
	//					 {
	//						  return false;
	//					 }
	//					 return name.equals(((RelationshipType) that).name());
	//				}
	//
	//				@@Override public int hashCode()
	//				{
	//					 return name.hashCode();
	//				}
	//		  };
	//	 }
	}

}