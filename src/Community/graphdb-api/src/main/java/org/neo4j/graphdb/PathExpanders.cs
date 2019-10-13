using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Graphdb
{

	using StandardExpander = Neo4Net.Graphdb.impl.StandardExpander;
	using Neo4Net.Graphdb.traversal;
	using Paths = Neo4Net.Graphdb.traversal.Paths;

	/// <summary>
	/// A catalog of convenient <seealso cref="PathExpander"/> factory methods.
	/// <para>
	/// Use <seealso cref="PathExpanderBuilder"/> to build specialized <seealso cref="PathExpander"/>s.
	/// </para>
	/// </summary>
	public abstract class PathExpanders
	{
		 /// <summary>
		 /// A very permissive <seealso cref="PathExpander"/> that follows any type in any direction.
		 /// </summary>
		 /// @param <STATE> the type of the object that holds the state </param>
		 /// <returns> a very permissive <seealso cref="PathExpander"/> that follows any type in any direction </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <STATE> PathExpander<STATE> allTypesAndDirections()
		 public static PathExpander<STATE> AllTypesAndDirections<STATE>()
		 {
			  return StandardExpander.DEFAULT;
		 }

		 /// <summary>
		 /// A very permissive <seealso cref="PathExpander"/> that follows {@code type} relationships in any direction.
		 /// </summary>
		 /// <param name="type"> the type of relationships to expand in any direction </param>
		 /// @param <STATE> the type of the object that holds the state </param>
		 /// <returns> a very permissive <seealso cref="PathExpander"/> that follows {@code type} relationships in any direction </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <STATE> PathExpander<STATE> forType(RelationshipType type)
		 public static PathExpander<STATE> ForType<STATE>( RelationshipType type )
		 {
			  return StandardExpander.create( type, Direction.Both );
		 }

		 /// <summary>
		 /// A very permissive <seealso cref="PathExpander"/> that follows any type in {@code direction}.
		 /// </summary>
		 /// <param name="direction"> the direction to follow relationships in </param>
		 /// @param <STATE> the type of the object that holds the state </param>
		 /// <returns> a very permissive <seealso cref="PathExpander"/> that follows any type in {@code direction} </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <STATE> PathExpander<STATE> forDirection(Direction direction)
		 public static PathExpander<STATE> ForDirection<STATE>( Direction direction )
		 {
			  return StandardExpander.create( direction );
		 }

		 /// <summary>
		 /// A very restricted <seealso cref="PathExpander"/> that follows {@code type} in {@code direction}.
		 /// </summary>
		 /// <param name="type"> the type of relationships to follow </param>
		 /// <param name="direction"> the direction to follow relationships in </param>
		 /// @param <STATE> the type of the object that holds the state </param>
		 /// <returns> a very restricted <seealso cref="PathExpander"/> that follows {@code type} in {@code direction} </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <STATE> PathExpander<STATE> forTypeAndDirection(RelationshipType type, Direction direction)
		 public static PathExpander<STATE> ForTypeAndDirection<STATE>( RelationshipType type, Direction direction )
		 {
			  return StandardExpander.create( type, direction );
		 }

		 /// <summary>
		 /// A very restricted <seealso cref="PathExpander"/> that follows only the {@code type}/{@code direction} pairs that you list.
		 /// </summary>
		 /// <param name="type1"> the type of relationships to follow in {@code direction1} </param>
		 /// <param name="direction1"> the direction to follow {@code type1} relationships in </param>
		 /// <param name="type2"> the type of relationships to follow in {@code direction2} </param>
		 /// <param name="direction2"> the direction to follow {@code type2} relationships in </param>
		 /// <param name="more"> add more {@code type}/{@code direction} pairs </param>
		 /// @param <STATE> the type of the object that holds the state </param>
		 /// <returns> a very restricted <seealso cref="PathExpander"/> that follows only the {@code type}/{@code direction} pairs that you list </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <STATE> PathExpander<STATE> forTypesAndDirections(RelationshipType type1, Direction direction1, RelationshipType type2, Direction direction2, Object... more)
		 public static PathExpander<STATE> ForTypesAndDirections<STATE>( RelationshipType type1, Direction direction1, RelationshipType type2, Direction direction2, params object[] more )
		 {
			  return StandardExpander.create( type1, direction1, type2, direction2, more );
		 }

		 /// <summary>
		 /// An expander forcing constant relationship direction
		 /// </summary>
		 /// <param name="types"> types of relationships to follow </param>
		 /// @param <STATE> the type of the object that holds the state </param>
		 /// <returns> a <seealso cref="PathExpander"/> which enforces constant relationship direction </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathExpander<STATE> forConstantDirectionWithTypes(final RelationshipType... types)
		 public static PathExpander<STATE> ForConstantDirectionWithTypes<STATE>( params RelationshipType[] types )
		 {
			  return new PathExpanderAnonymousInnerClass( types );
		 }

		 private class PathExpanderAnonymousInnerClass : PathExpander<STATE>
		 {
			 private Neo4Net.Graphdb.RelationshipType[] _types;

			 public PathExpanderAnonymousInnerClass( Neo4Net.Graphdb.RelationshipType[] types )
			 {
				 this._types = types;
			 }

			 public IEnumerable<Relationship> expand( Path path, BranchState<STATE> state )
			 {
				  if ( path.Length() == 0 )
				  {
						return path.EndNode().getRelationships(_types);
				  }
				  else
				  {
						Direction direction = getDirectionOfLastRelationship( path );
						return path.EndNode().getRelationships(direction, _types);
				  }
			 }

			 public PathExpander<STATE> reverse()
			 {
				  return this;
			 }

			 private Direction getDirectionOfLastRelationship( Path path )
			 {
				  Debug.Assert( path.Length() > 0 );
				  Direction direction = Direction.Incoming;
				  if ( path.EndNode().Equals(path.LastRelationship().EndNode) )
				  {
						direction = Direction.Outgoing;
				  }
				  return direction;
			 }
		 }

		 private PathExpanders()
		 {
			  // you should never instantiate this
		 }

		 /// <summary>
		 /// A wrapper that uses <seealso cref="org.neo4j.graphdb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths.
		 /// All expanded paths will be printed using System.out. </summary>
		 /// <param name="source">    <seealso cref="PathExpander"/> to wrap. </param>
		 /// @param <STATE>   the type of the object that holds the state </param>
		 /// <returns> A new <seealso cref="PathExpander"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathExpander<STATE> printingWrapper(final PathExpander<STATE> source)
		 public static PathExpander<STATE> PrintingWrapper<STATE>( PathExpander<STATE> source )
		 {
			  return PrintingWrapper( source, new Paths.DefaultPathDescriptor() );
		 }

		 /// <summary>
		 /// A wrapper that uses <seealso cref="org.neo4j.graphdb.traversal.Paths.DefaultPathDescriptor"/>
		 /// to print expanded paths that fulfill <seealso cref="BiFunction"/> predicate.
		 /// Will use System.out as <seealso cref="PrintStream"/>. </summary>
		 /// <param name="source">    <seealso cref="PathExpander"/> to wrap. </param>
		 /// <param name="pred">      <seealso cref="BiFunction"/> used as predicate for printing expansion. </param>
		 /// @param <STATE>   the type of the object that holds the state </param>
		 /// <returns>          A new <seealso cref="PathExpander"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathExpander<STATE> printingWrapper(final PathExpander<STATE> source, final System.Func<Path, org.neo4j.graphdb.traversal.BranchState, bool> pred)
		 public static PathExpander<STATE> PrintingWrapper<STATE>( PathExpander<STATE> source, System.Func<Path, BranchState, bool> pred )
		 {
			  return PrintingWrapper( source, pred, new Paths.DefaultPathDescriptor() );
		 }

		 /// <summary>
		 /// A wrapper that uses <seealso cref="org.neo4j.graphdb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths
		 /// using given <seealso cref="org.neo4j.graphdb.traversal.Paths.PathDescriptor"/>.
		 /// All expanded paths will be printed.
		 /// Will use System.out as <seealso cref="PrintStream"/>. </summary>
		 /// <param name="source">        <seealso cref="PathExpander"/> to wrap. </param>
		 /// <param name="descriptor">    <seealso cref="org.neo4j.graphdb.traversal.Paths.PathDescriptor"/> to use when printing paths. </param>
		 /// @param <STATE>       the type of the object that holds the state </param>
		 /// <returns>              A new <seealso cref="PathExpander"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathExpander<STATE> printingWrapper(final PathExpander<STATE> source, final org.neo4j.graphdb.traversal.Paths.PathDescriptor descriptor)
		 public static PathExpander<STATE> PrintingWrapper<STATE>( PathExpander<STATE> source, Paths.PathDescriptor descriptor )
		 {
			  return PrintingWrapper( source, ( propertyContainers, stateBranchState ) => true, descriptor );
		 }

		 /// <summary>
		 /// A wrapper that uses <seealso cref="org.neo4j.graphdb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths
		 /// that fulfill <seealso cref="BiFunction"/> predicate using given <seealso cref="org.neo4j.graphdb.traversal.Paths.PathDescriptor"/>.
		 /// Will use System.out as <seealso cref="PrintStream"/>. </summary>
		 /// <param name="source">    <seealso cref="PathExpander"/> to wrap. </param>
		 /// <param name="pred">      <seealso cref="BiFunction"/> used as predicate for printing expansion. </param>
		 /// <param name="descriptor"> <seealso cref="org.neo4j.graphdb.traversal.Paths.PathDescriptor"/> to use when printing paths. </param>
		 /// @param <STATE>   the type of the object that holds the state </param>
		 /// <returns>          A new <seealso cref="PathExpander"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathExpander<STATE> printingWrapper(final PathExpander<STATE> source, final System.Func<Path, org.neo4j.graphdb.traversal.BranchState, bool> pred, final org.neo4j.graphdb.traversal.Paths.PathDescriptor descriptor)
		 public static PathExpander<STATE> PrintingWrapper<STATE>( PathExpander<STATE> source, System.Func<Path, BranchState, bool> pred, Paths.PathDescriptor descriptor )
		 {
			  return PrintingWrapper( source, pred, descriptor, System.out );
		 }

		 /// <summary>
		 /// A wrapper that uses <seealso cref="org.neo4j.graphdb.traversal.Paths.DefaultPathDescriptor"/> to print expanded paths
		 /// that fulfill <seealso cref="BiFunction"/> predicate using given <seealso cref="org.neo4j.graphdb.traversal.Paths.PathDescriptor"/>. </summary>
		 /// <param name="source">        <seealso cref="PathExpander"/> to wrap. </param>
		 /// <param name="pred">          <seealso cref="BiFunction"/> used as predicate for printing expansion. </param>
		 /// <param name="descriptor">    <seealso cref="org.neo4j.graphdb.traversal.Paths.PathDescriptor"/> to use when printing paths. </param>
		 /// <param name="out">           <seealso cref="PrintStream"/> to use for printing expanded paths </param>
		 /// @param <STATE>       the type of the object that holds the state </param>
		 /// <returns>              A new <seealso cref="PathExpander"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathExpander<STATE> printingWrapper(final PathExpander<STATE> source, final System.Func<Path, org.neo4j.graphdb.traversal.BranchState, bool> pred, final org.neo4j.graphdb.traversal.Paths.PathDescriptor descriptor, final java.io.PrintStream out)
		 public static PathExpander<STATE> PrintingWrapper<STATE>( PathExpander<STATE> source, System.Func<Path, BranchState, bool> pred, Paths.PathDescriptor descriptor, PrintStream @out )
		 {
			  return new PathExpanderAnonymousInnerClass2( source, pred, descriptor, @out );
		 }

		 private class PathExpanderAnonymousInnerClass2 : PathExpander<STATE>
		 {
			 private Neo4Net.Graphdb.PathExpander<STATE> _source;
			 private System.Func<Path, BranchState, bool> _pred;
			 private Paths.PathDescriptor _descriptor;
			 private PrintStream @out;

			 public PathExpanderAnonymousInnerClass2( Neo4Net.Graphdb.PathExpander<STATE> source, System.Func<Path, BranchState, bool> pred, Paths.PathDescriptor descriptor, PrintStream @out )
			 {
				 this._source = source;
				 this._pred = pred;
				 this._descriptor = descriptor;
				 this.@out = @out;
			 }

			 public IEnumerable<Relationship> expand( Path path, BranchState state )
			 {
				  if ( _pred( path, state ) )
				  {
						@out.println( Paths.pathToString( path, _descriptor ) );
				  }
				  return _source.expand( path, state );
			 }

			 public PathExpander<STATE> reverse()
			 {
				  return PrintingWrapper( _source.reverse(), _pred, _descriptor, @out );
			 }
		 }
	}

}