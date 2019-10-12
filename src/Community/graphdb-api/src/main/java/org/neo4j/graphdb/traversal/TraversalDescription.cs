using System.Collections.Generic;

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
namespace Neo4Net.Graphdb.traversal
{

	using Neo4Net.Graphdb;

	/// <summary>
	/// Represents a description of a traversal. This interface describes the rules
	/// and behavior of a traversal. A traversal description is immutable and each
	/// method which adds or modifies the behavior returns a new instances that
	/// includes the new modification, leaving the instance which returns the new
	/// instance intact. For instance,
	/// 
	/// <pre>
	/// TraversalDescription td = new TraversalDescriptionImpl();
	/// td.depthFirst();
	/// </pre>
	/// 
	/// is not going to modify td. you will need to reassign td, like
	/// 
	/// <pre>
	/// td = td.depthFirst();
	/// </pre>
	/// <para>
	/// When all the rules and behaviors have been described the traversal is started
	/// by using <seealso cref="traverse(Node)"/> where a starting node is supplied. The
	/// <seealso cref="Traverser"/> that is returned is then used to step through the graph,
	/// and return the positions that matches the rules.
	/// </para>
	/// </summary>
	public interface TraversalDescription
	{
		 /// <summary>
		 /// Sets the <seealso cref="UniquenessFactory"/> for creating the
		 /// <seealso cref="UniquenessFilter"/> to use.
		 /// </summary>
		 /// <param name="uniqueness"> the <seealso cref="UniquenessFactory"/> the creator
		 /// of the desired <seealso cref="UniquenessFilter"/> to use. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Uniqueness( UniquenessFactory uniqueness );

		 /// <summary>
		 /// Sets the <seealso cref="UniquenessFactory"/> for creating the
		 /// <seealso cref="UniquenessFilter"/> to use. It also accepts an extra parameter
		 /// which is mandatory for certain uniquenesses.
		 /// </summary>
		 /// <param name="uniqueness"> the <seealso cref="UniquenessFactory"/> the creator
		 /// of the desired <seealso cref="UniquenessFilter"/> to use. </param>
		 /// <param name="parameter"> an extra parameter
		 /// which is mandatory for certain uniquenesses. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Uniqueness( UniquenessFactory uniqueness, object parameter );

		 /// <summary>
		 /// Adds {@code evaluator} to the list of evaluators which will control the
		 /// behavior of the traversal. Each <seealso cref="Evaluator"/> can decide whether or
		 /// not to include a position in the traverser result, i.e. return it from
		 /// the <seealso cref="Traverser"/> iterator and also whether to continue down that
		 /// path or to prune, so that the traverser won't continue further down that
		 /// path.
		 /// 
		 /// Multiple <seealso cref="Evaluator"/>s can be added. For a path to be included in
		 /// the result, all evaluators must agree to include it, i.e. returning
		 /// either <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> or
		 /// <seealso cref="Evaluation.INCLUDE_AND_PRUNE"/>. For making the traversal continue
		 /// down that path all evaluators must agree to continue from that path, i.e.
		 /// returning either <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> or
		 /// <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/>.
		 /// </summary>
		 /// <param name="evaluator"> the <seealso cref="Evaluator"/> to add to the traversal </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Evaluator( Evaluator evaluator );

		 /// <summary>
		 /// Adds {@code evaluator} to the list of evaluators which will control the
		 /// behavior of the traversal. Each <seealso cref="PathEvaluator"/> can decide whether or
		 /// not to include a position in the traverser result, i.e. return it from
		 /// the <seealso cref="Traverser"/> iterator and also whether to continue down that
		 /// path or to prune, so that the traverser won't continue further down that
		 /// path.
		 /// 
		 /// Multiple <seealso cref="PathEvaluator"/>s can be added. For a path to be included in
		 /// the result, all evaluators must agree to include it, i.e. returning
		 /// either <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> or
		 /// <seealso cref="Evaluation.INCLUDE_AND_PRUNE"/>. For making the traversal continue
		 /// down that path all evaluators must agree to continue from that path, i.e.
		 /// returning either <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> or
		 /// <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/>.
		 /// </summary>
		 /// <param name="evaluator"> the <seealso cref="PathEvaluator"/> to add to the traversal </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Evaluator( PathEvaluator evaluator );

		 /// <summary>
		 /// Sets the <seealso cref="BranchOrderingPolicy"/> to use. A <seealso cref="BranchSelector"/>
		 /// is the basic decisions in the traversal of "where to go next".
		 /// Examples of default implementations are "breadth first" and
		 /// "depth first", which can be set with convenience methods
		 /// <seealso cref="breadthFirst()"/> and <seealso cref="depthFirst()"/>.
		 /// </summary>
		 /// <param name="selector"> the factory which creates the <seealso cref="BranchSelector"/>
		 /// to use with the traversal. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Order( BranchOrderingPolicy selector );

		 /// <summary>
		 /// A convenience method for <seealso cref="order(BranchOrderingPolicy)"/>
		 /// where a "preorder depth first" selector is used. Positions which are
		 /// deeper than the current position will be returned before positions on
		 /// the same depth. See http://en.wikipedia.org/wiki/Depth-first_search </summary>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription DepthFirst();

		 /// <summary>
		 /// A convenience method for <seealso cref="order(BranchOrderingPolicy)"/>
		 /// where a "preorder breadth first" selector is used. All positions with
		 /// the same depth will be returned before advancing to the next depth.
		 /// See http://en.wikipedia.org/wiki/Breadth-first_search </summary>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription BreadthFirst();

		 /// <summary>
		 /// Adds {@code type} to the list of relationship types to traverse.
		 /// There's no priority or order in which types to traverse.
		 /// </summary>
		 /// <param name="type"> the <seealso cref="RelationshipType"/> to add to the list of types
		 /// to traverse. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Relationships( RelationshipType type );

		 /// <summary>
		 /// Adds {@code type} to the list of relationship types to traverse in
		 /// the given {@code direction}. There's no priority or order in which
		 /// types to traverse.
		 /// </summary>
		 /// <param name="type"> the <seealso cref="RelationshipType"/> to add to the list of types
		 /// to traverse. </param>
		 /// <param name="direction"> the <seealso cref="Direction"/> to traverse this type of
		 /// relationship in. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Relationships( RelationshipType type, Direction direction );

		 /// <summary>
		 /// Sets the <seealso cref="PathExpander"/> as the expander of relationships,
		 /// discarding all previous calls to
		 /// <seealso cref="relationships(RelationshipType)"/> and
		 /// <seealso cref="relationships(RelationshipType, Direction)"/> or any other expand method.
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: TraversalDescription expand(org.neo4j.graphdb.PathExpander<?> expander);
		 TraversalDescription expand<T1>( PathExpander<T1> expander );

		 /// <summary>
		 /// Sets the <seealso cref="PathExpander"/> as the expander of relationships,
		 /// discarding all previous calls to
		 /// <seealso cref="relationships(RelationshipType)"/> and
		 /// <seealso cref="relationships(RelationshipType, Direction)"/> or any other expand method.
		 /// The supplied <seealso cref="InitialBranchState"/> will provide the initial traversal branches
		 /// with state values which flows down throughout the traversal and can be changed
		 /// for child branches by the <seealso cref="PathExpander"/> at any level.
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use. </param>
		 /// <param name="initialState"> factory for supplying the initial traversal branches with
		 /// state values potentially used by the <seealso cref="PathExpander"/>. </param>
		 /// @param <STATE> the type of the state object </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription expand<STATE>( PathExpander<STATE> expander, InitialBranchState<STATE> initialState );

		 /// <param name="comparator"> the <seealso cref="System.Collections.IComparer"/> to use for sorting the paths. </param>
		 /// <returns> the paths from this traversal sorted according to {@code comparator}. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: TraversalDescription sort(java.util.Comparator<? super org.neo4j.graphdb.Path> comparator);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 TraversalDescription sort<T1>( IComparer<T1> comparator );

		 /// <summary>
		 /// Creates an identical <seealso cref="TraversalDescription"/>, although reversed in
		 /// how it traverses the graph.
		 /// </summary>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 TraversalDescription Reverse();

		 /// <summary>
		 /// Traverse from a single start node based on all the rules and behavior
		 /// in this description. A <seealso cref="Traverser"/> is returned which is
		 /// used to step through the graph and getting results back. The traversal
		 /// is not guaranteed to start before the Traverser is used.
		 /// </summary>
		 /// <param name="startNode"> <seealso cref="Node"/> to start traversing from. </param>
		 /// <returns> a <seealso cref="Traverser"/> used to step through the graph and to get
		 /// results from. </returns>
		 Traverser Traverse( Node startNode );

		 /// <summary>
		 /// Traverse from a set of start nodes based on all the rules and behavior
		 /// in this description. A <seealso cref="Traverser"/> is returned which is
		 /// used to step through the graph and getting results back. The traversal
		 /// is not guaranteed to start before the Traverser is used.
		 /// </summary>
		 /// <param name="startNodes"> <seealso cref="Node"/>s to start traversing from. </param>
		 /// <returns> a <seealso cref="Traverser"/> used to step through the graph and to get
		 /// results from. </returns>
		 Traverser Traverse( params Node[] startNodes );

		 /// <summary>
		 /// Traverse from a iterable of start nodes based on all the rules and behavior
		 /// in this description. A <seealso cref="Traverser"/> is returned which is
		 /// used to step through the graph and getting results back. The traversal
		 /// is not guaranteed to start before the Traverser is used.
		 /// </summary>
		 /// <param name="iterableStartNodes"> <seealso cref="Node"/>s to start traversing from. </param>
		 /// <returns> a <seealso cref="Traverser"/> used to step through the graph and to get
		 /// results from. </returns>
		 Traverser Traverse( IEnumerable<Node> iterableStartNodes );
	}

}