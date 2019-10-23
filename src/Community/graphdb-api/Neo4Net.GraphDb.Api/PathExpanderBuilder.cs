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

	using OrderedByTypeExpander = Neo4Net.GraphDb.impl.OrderedByTypeExpander;
	using StandardExpander = Neo4Net.GraphDb.impl.StandardExpander;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.BOTH;

	/// <summary>
	/// A fluent builder for creating specialized <seealso cref="PathExpander path expanders"/>.
	/// <para>
	/// See <seealso cref="PathExpanders"/> for a catalog of common expanders.
	/// </para>
	/// </summary>
	public class PathExpanderBuilder
	{
		 /// <summary>
		 /// A <seealso cref="PathExpanderBuilder"/> that follows no relationships. You start with this and use
		 /// <seealso cref="add(IRelationshipType, Direction)"/> to form a restrictive PathExpander with just a few expansion rules
		 /// in it.
		 /// </summary>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> that follows no relationships </returns>
		 public static PathExpanderBuilder Empty()
		 {
			  return new PathExpanderBuilder( StandardExpander.EMPTY );
		 }

		 /// <summary>
		 /// A <seealso cref="PathExpanderBuilder"/> that follows no relationships. You start with this and use
		 /// <seealso cref="add(IRelationshipType, Direction)"/> to form a restrictive PathExpander with just a few expansion rules
		 /// in it.
		 /// </summary>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> that follows no relationships </returns>
		 public static PathExpanderBuilder EmptyOrderedByType()
		 {
			  return new PathExpanderBuilder( new OrderedByTypeExpander() );
		 }

		 /// <summary>
		 /// A <seealso cref="PathExpanderBuilder"/> that is seeded with all possible relationship types in {@link Direction#BOTH both
		 /// directions}. You start with this and <seealso cref="remove(IRelationshipType) remove types"/> to form a permissive
		 /// <seealso cref="PathExpander"/> with just a few exceptions in it.
		 /// </summary>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> that is seeded with all possible relationship types in {@link Direction#BOTH both
		 /// directions} </returns>
		 public static PathExpanderBuilder AllTypesAndDirections()
		 {
			  return new PathExpanderBuilder( StandardExpander.DEFAULT );
		 }

		 /// <summary>
		 /// A <seealso cref="PathExpanderBuilder"/> seeded with all possible types but restricted to {@code direction}. You start
		 /// with this and <seealso cref="remove(IRelationshipType) remove types"/> to form a permissive <seealso cref="PathExpander"/> with
		 /// just a few exceptions in it.
		 /// </summary>
		 /// <param name="direction"> The direction you want to restrict expansions to </param>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> seeded with all possible types but restricted to {@code direction}. </returns>
		 public static PathExpanderBuilder AllTypes( Direction direction )
		 {
			  return new PathExpanderBuilder( StandardExpander.create( direction ) );
		 }

		 /// <summary>
		 /// Add a pair of {@code type} and <seealso cref="Direction.BOTH"/> to the PathExpander configuration.
		 /// </summary>
		 /// <param name="type"> the type to add for expansion in both directions </param>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> with the added expansion of {@code type} relationships in both directions </returns>
		 public virtual PathExpanderBuilder Add( IRelationshipType type )
		 {
			  return Add( type, BOTH );
		 }

		 /// <summary>
		 /// Add a pair of {@code type} and {@code direction} to the PathExpander configuration.
		 /// </summary>
		 /// <param name="type"> the type to add for expansion </param>
		 /// <param name="direction"> the direction to restrict the expansion to </param>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> with the added expansion of {@code type} relationships in the given direction </returns>
		 public virtual PathExpanderBuilder Add( IRelationshipType type, Direction direction )
		 {
			  return new PathExpanderBuilder( _expander.add( type, direction ) );
		 }

		 /// <summary>
		 /// Remove expansion of {@code type} in any direction from the PathExpander configuration.
		 /// <para>
		 /// Example: {@code PathExpanderBuilder.allTypesAndDirections().remove(type).add(type, Direction.INCOMING)}
		 /// would restrict the <seealso cref="PathExpander"/> to only follow {@code Direction.INCOMING} relationships for {@code
		 /// type} while following any other relationship type in either direction.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="type"> the type to remove from expansion </param>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> with expansion of {@code type} relationships removed </returns>
		 public virtual PathExpanderBuilder Remove( IRelationshipType type )
		 {
			  return new PathExpanderBuilder( _expander.Remove( type ) );
		 }

		 /// <summary>
		 /// Adds a <seealso cref="INode"/> filter.
		 /// </summary>
		 /// <param name="filter"> a Predicate for filtering nodes. </param>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> with the added node filter. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public PathExpanderBuilder addNodeFilter(System.Predicate<? super Node> filter)
		 public virtual PathExpanderBuilder AddNodeFilter<T1>( System.Predicate<T1> filter )
		 {
			  return new PathExpanderBuilder( _expander.AddNodeFilter( filter ) );
		 }

		 /// <summary>
		 /// Adds a <seealso cref="IRelationship"/> filter.
		 /// </summary>
		 /// <param name="filter"> a Predicate for filtering relationships. </param>
		 /// <returns> a <seealso cref="PathExpanderBuilder"/> with the added relationship filter. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public PathExpanderBuilder addRelationshipFilter(System.Predicate<? super Relationship> filter)
		 public virtual PathExpanderBuilder AddRelationshipFilter<T1>( System.Predicate<T1> filter )
		 {
			  return new PathExpanderBuilder( _expander.addRelationshipFilter( filter ) );
		 }

		 /// <summary>
		 /// Produce a <seealso cref="PathExpander"/> from the configuration you have built up.
		 /// </summary>
		 /// @param <STATE> the type of the object holding the state </param>
		 /// <returns> a PathExpander produced from the configuration you have built up </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <STATE> PathExpander<STATE> build()
		 public virtual IPathExpander<STATE> Build<STATE>()
		 {
			  return _expander;
		 }

		 private readonly StandardExpander _expander;

		 private PathExpanderBuilder( StandardExpander expander )
		 {
			  _expander = expander;
		 }
	}

}