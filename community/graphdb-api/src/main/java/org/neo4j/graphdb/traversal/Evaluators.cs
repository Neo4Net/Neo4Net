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
namespace Org.Neo4j.Graphdb.traversal
{


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.INCLUDE_AND_CONTINUE;

	/// <summary>
	/// Common <seealso cref="Evaluator"/>s useful during common traversals.
	/// </summary>
	/// <seealso cref= Evaluator </seealso>
	/// <seealso cref= TraversalDescription </seealso>
	public abstract class Evaluators
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private static final PathEvaluator<?> ALL = new PathEvaluator_Adapter()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private static readonly PathEvaluator<object> ALL = new PathEvaluator_AdapterAnonymousInnerClass();

		 private class PathEvaluator_AdapterAnonymousInnerClass : PathEvaluator_Adapter
		 {
			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  return INCLUDE_AND_CONTINUE;
			 }
		 }

		 private static readonly PathEvaluator _allButStartPosition = FromDepth( 1 );

		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> an evaluator which includes everything it encounters and doesn't prune
		 ///         anything. </returns>
		 public static PathEvaluator<STATE> All<STATE>()
		 {
			  //noinspection unchecked
			  return ( PathEvaluator<STATE> ) ALL;
		 }

		 /// <returns> an evaluator which never prunes and includes everything except
		 ///         the first position, i.e. the the start node. </returns>
		 public static PathEvaluator ExcludeStartPosition()
		 {
			  return _allButStartPosition;
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluator"/> which includes positions down to {@code depth}
		 /// and prunes everything deeper than that.
		 /// </summary>
		 /// <param name="depth">   the max depth to traverse to. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> Returns an <seealso cref="Evaluator"/> which includes positions down to
		 ///         {@code depth} and prunes everything deeper than that. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> toDepth(final int depth)
		 public static PathEvaluator<STATE> ToDepth<STATE>( int depth )
		 {
			  return new PathEvaluator_AdapterAnonymousInnerClass2( depth );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass2 : PathEvaluator_Adapter<STATE>
		 {
			 private int _depth;

			 public PathEvaluator_AdapterAnonymousInnerClass2( int depth )
			 {
				 this._depth = depth;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  int pathLength = path.Length();
				  return Evaluation.of( pathLength <= _depth, pathLength < _depth );
			 }
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluator"/> which only includes positions from {@code depth}
		 /// and deeper and never prunes anything.
		 /// </summary>
		 /// <param name="depth">   the depth to start include positions from. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> Returns an <seealso cref="Evaluator"/> which only includes positions from
		 ///         {@code depth} and deeper and never prunes anything. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> fromDepth(final int depth)
		 public static PathEvaluator<STATE> FromDepth<STATE>( int depth )
		 {
			  return new PathEvaluator_AdapterAnonymousInnerClass3( depth );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass3 : PathEvaluator_Adapter<STATE>
		 {
			 private int _depth;

			 public PathEvaluator_AdapterAnonymousInnerClass3( int depth )
			 {
				 this._depth = depth;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  return Evaluation.ofIncludes( path.Length() >= _depth );
			 }
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluator"/> which only includes positions at {@code depth}
		 /// and prunes everything deeper than that.
		 /// </summary>
		 /// <param name="depth">   the depth to start include positions from. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> Returns an <seealso cref="Evaluator"/> which only includes positions at
		 ///         {@code depth} and prunes everything deeper than that. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> atDepth(final int depth)
		 public static PathEvaluator<STATE> AtDepth<STATE>( int depth )
		 {
			  return new PathEvaluator_AdapterAnonymousInnerClass4( depth );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass4 : PathEvaluator_Adapter<STATE>
		 {
			 private int _depth;

			 public PathEvaluator_AdapterAnonymousInnerClass4( int depth )
			 {
				 this._depth = depth;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  return path.Length() == _depth ? Evaluation.IncludeAndPrune : Evaluation.ExcludeAndContinue;
			 }
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluator"/> which only includes positions between
		 /// depths {@code minDepth} and {@code maxDepth}. It prunes everything deeper
		 /// than {@code maxDepth}.
		 /// </summary>
		 /// <param name="minDepth"> minimum depth a position must have to be included. </param>
		 /// <param name="maxDepth"> maximum depth a position must have to be included. </param>
		 /// @param <STATE>  the type of the state object. </param>
		 /// <returns> Returns an <seealso cref="Evaluator"/> which only includes positions between
		 ///         depths {@code minDepth} and {@code maxDepth}. It prunes everything deeper
		 ///         than {@code maxDepth}. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includingDepths(final int minDepth, final int maxDepth)
		 public static PathEvaluator<STATE> IncludingDepths<STATE>( int minDepth, int maxDepth )
		 {
			  return new PathEvaluator_AdapterAnonymousInnerClass5( minDepth, maxDepth );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass5 : PathEvaluator_Adapter<STATE>
		 {
			 private int _minDepth;
			 private int _maxDepth;

			 public PathEvaluator_AdapterAnonymousInnerClass5( int minDepth, int maxDepth )
			 {
				 this._minDepth = minDepth;
				 this._maxDepth = maxDepth;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  int length = path.Length();
				  return Evaluation.of( length >= _minDepth && length <= _maxDepth, length < _maxDepth );
			 }
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluator"/> which compares the type of the last relationship
		 /// in a <seealso cref="Path"/> to a given set of relationship types (one or more).If the type of
		 /// the last relationship in a path is of one of the given types then
		 /// {@code evaluationIfMatch} will be returned, otherwise
		 /// {@code evaluationIfNoMatch} will be returned.
		 /// </summary>
		 /// <param name="evaluationIfMatch">   the <seealso cref="Evaluation"/> to return if the type of the
		 ///                            last relationship in the path matches any of the given types. </param>
		 /// <param name="evaluationIfNoMatch"> the <seealso cref="Evaluation"/> to return if the type of the
		 ///                            last relationship in the path doesn't match any of the given types. </param>
		 /// <param name="type">                the (first) type (of possibly many) to match the last relationship
		 ///                            in paths with. </param>
		 /// <param name="orAnyOfTheseTypes">   additional types to match the last relationship in
		 ///                            paths with. </param>
		 /// @param <STATE>             the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluator"/> which compares the type of the last relationship
		 ///         in a <seealso cref="Path"/> to a given set of relationship types. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> lastRelationshipTypeIs(final Evaluation evaluationIfMatch, final Evaluation evaluationIfNoMatch, final org.neo4j.graphdb.RelationshipType type, org.neo4j.graphdb.RelationshipType... orAnyOfTheseTypes)
		 public static PathEvaluator<STATE> LastRelationshipTypeIs<STATE>( Evaluation evaluationIfMatch, Evaluation evaluationIfNoMatch, RelationshipType type, params RelationshipType[] orAnyOfTheseTypes )
		 {
			  if ( orAnyOfTheseTypes.Length == 0 )
			  {
					return new PathEvaluator_AdapterAnonymousInnerClass6( evaluationIfMatch, evaluationIfNoMatch, type );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> expectedTypes = new java.util.HashSet<>();
			  ISet<string> expectedTypes = new HashSet<string>();
			  expectedTypes.Add( type.Name() );
			  foreach ( RelationshipType otherType in orAnyOfTheseTypes )
			  {
					expectedTypes.Add( otherType.Name() );
			  }

			  return new PathEvaluator_AdapterAnonymousInnerClass7( evaluationIfMatch, evaluationIfNoMatch, expectedTypes );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass6 : PathEvaluator_Adapter<STATE>
		 {
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfMatch;
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfNoMatch;
			 private RelationshipType _type;

			 public PathEvaluator_AdapterAnonymousInnerClass6( Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfMatch, Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfNoMatch, RelationshipType type )
			 {
				 this._evaluationIfMatch = evaluationIfMatch;
				 this._evaluationIfNoMatch = evaluationIfNoMatch;
				 this._type = type;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  Relationship rel = path.LastRelationship();
				  return rel != null && rel.IsType( _type ) ? _evaluationIfMatch : _evaluationIfNoMatch;
			 }
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass7 : PathEvaluator_Adapter<STATE>
		 {
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfMatch;
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfNoMatch;
			 private ISet<string> _expectedTypes;

			 public PathEvaluator_AdapterAnonymousInnerClass7( Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfMatch, Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfNoMatch, ISet<string> expectedTypes )
			 {
				 this._evaluationIfMatch = evaluationIfMatch;
				 this._evaluationIfNoMatch = evaluationIfNoMatch;
				 this._expectedTypes = expectedTypes;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  Relationship lastRelationship = path.LastRelationship();
				  if ( lastRelationship == null )
				  {
						return _evaluationIfNoMatch;
				  }

				  return _expectedTypes.Contains( lastRelationship.Type.name() ) ? _evaluationIfMatch : _evaluationIfNoMatch;
			 }
		 }

		 /// <param name="type">              the (first) type (of possibly many) to match the last relationship
		 ///                          in paths with. </param>
		 /// <param name="orAnyOfTheseTypes"> types to match the last relationship in paths with. If any matches
		 ///                          it's considered a match. </param>
		 /// @param <STATE>           the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluator"/> which compares the type of the last relationship
		 ///         in a <seealso cref="Path"/> to a given set of relationship types. </returns>
		 /// <seealso cref= #lastRelationshipTypeIs(Evaluation, Evaluation, RelationshipType, RelationshipType...)
		 ///      Uses <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> for {@code evaluationIfMatch}
		 ///      and <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/> for {@code evaluationIfNoMatch}. </seealso>
		 public static PathEvaluator<STATE> IncludeWhereLastRelationshipTypeIs<STATE>( RelationshipType type, params RelationshipType[] orAnyOfTheseTypes )
		 {
			  return LastRelationshipTypeIs( Evaluation.IncludeAndContinue, Evaluation.ExcludeAndContinue, type, orAnyOfTheseTypes );
		 }

		 /// <param name="type">              the (first) type (of possibly many) to match the last relationship
		 ///                          in paths with. </param>
		 /// <param name="orAnyOfTheseTypes"> types to match the last relationship in paths with. If any matches
		 ///                          it's considered a match. </param>
		 /// @param <STATE>           the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluator"/> which compares the type of the last relationship
		 ///         in a <seealso cref="Path"/> to a given set of relationship types. </returns>
		 /// <seealso cref= #lastRelationshipTypeIs(Evaluation, Evaluation, RelationshipType, RelationshipType...)
		 ///      Uses <seealso cref="Evaluation.INCLUDE_AND_PRUNE"/> for {@code evaluationIfMatch}
		 ///      and <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> for {@code evaluationIfNoMatch}. </seealso>
		 public static PathEvaluator<STATE> PruneWhereLastRelationshipTypeIs<STATE>( RelationshipType type, params RelationshipType[] orAnyOfTheseTypes )
		 {
			  return LastRelationshipTypeIs( Evaluation.IncludeAndPrune, Evaluation.ExcludeAndContinue, type, orAnyOfTheseTypes );
		 }

		 /// <summary>
		 /// An <seealso cref="Evaluator"/> which will return {@code evaluationIfMatch} if <seealso cref="Path.endNode()"/>
		 /// for a given path is any of {@code nodes}, else {@code evaluationIfNoMatch}.
		 /// </summary>
		 /// <param name="evaluationIfMatch">   the <seealso cref="Evaluation"/> to return if the <seealso cref="Path.endNode()"/>
		 ///                            is any of the nodes in {@code nodes}. </param>
		 /// <param name="evaluationIfNoMatch"> the <seealso cref="Evaluation"/> to return if the <seealso cref="Path.endNode()"/>
		 ///                            doesn't match any of the nodes in {@code nodes}. </param>
		 /// <param name="possibleEndNodes">    a set of nodes to match to end nodes in paths. </param>
		 /// @param <STATE>             the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluator"/> which will return {@code evaluationIfMatch} if
		 ///         <seealso cref="Path.endNode()"/> for a given path is any of {@code nodes},
		 ///         else {@code evaluationIfNoMatch}. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> endNodeIs(final Evaluation evaluationIfMatch, final Evaluation evaluationIfNoMatch, org.neo4j.graphdb.Node... possibleEndNodes)
		 public static PathEvaluator<STATE> EndNodeIs<STATE>( Evaluation evaluationIfMatch, Evaluation evaluationIfNoMatch, params Node[] possibleEndNodes )
		 {
			  if ( possibleEndNodes.Length == 1 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node target = possibleEndNodes[0];
					Node target = possibleEndNodes[0];
					return new PathEvaluator_AdapterAnonymousInnerClass8( evaluationIfMatch, evaluationIfNoMatch, target );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<org.neo4j.graphdb.Node> endNodes = new java.util.HashSet<>(asList(possibleEndNodes));
			  ISet<Node> endNodes = new HashSet<Node>( asList( possibleEndNodes ) );
			  return new PathEvaluator_AdapterAnonymousInnerClass9( evaluationIfMatch, evaluationIfNoMatch, endNodes );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass8 : PathEvaluator_Adapter<STATE>
		 {
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfMatch;
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfNoMatch;
			 private Node _target;

			 public PathEvaluator_AdapterAnonymousInnerClass8( Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfMatch, Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfNoMatch, Node target )
			 {
				 this._evaluationIfMatch = evaluationIfMatch;
				 this._evaluationIfNoMatch = evaluationIfNoMatch;
				 this._target = target;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  return _target.Equals( path.EndNode() ) ? _evaluationIfMatch : _evaluationIfNoMatch;
			 }
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass9 : PathEvaluator_Adapter<STATE>
		 {
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfMatch;
			 private Org.Neo4j.Graphdb.traversal.Evaluation _evaluationIfNoMatch;
			 private ISet<Node> _endNodes;

			 public PathEvaluator_AdapterAnonymousInnerClass9( Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfMatch, Org.Neo4j.Graphdb.traversal.Evaluation evaluationIfNoMatch, ISet<Node> endNodes )
			 {
				 this._evaluationIfMatch = evaluationIfMatch;
				 this._evaluationIfNoMatch = evaluationIfNoMatch;
				 this._endNodes = endNodes;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  return _endNodes.Contains( path.EndNode() ) ? _evaluationIfMatch : _evaluationIfNoMatch;
			 }
		 }

		 /// <summary>
		 /// Include paths with the specified end nodes.
		 /// 
		 /// Uses Evaluators#endNodeIs(Evaluation, Evaluation, Node...) with
		 /// <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> for {@code evaluationIfMatch} and
		 /// <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/> for {@code evaluationIfNoMatch}.
		 /// </summary>
		 /// <param name="nodes">   end nodes for paths to be included in the result. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> paths where the end node is one of {@code nodes} </returns>
		 public static PathEvaluator<STATE> IncludeWhereEndNodeIs<STATE>( params Node[] nodes )
		 {
			  return EndNodeIs( Evaluation.IncludeAndContinue, Evaluation.ExcludeAndContinue, nodes );
		 }

		 public static PathEvaluator<STATE> PruneWhereEndNodeIs<STATE>( params Node[] nodes )
		 {
			  return EndNodeIs( Evaluation.IncludeAndPrune, Evaluation.ExcludeAndContinue, nodes );
		 }

		 /// <summary>
		 /// Evaluator which decides to include a <seealso cref="Path"/> if all the
		 /// {@code nodes} exist in it.
		 /// </summary>
		 /// <param name="nodes">   <seealso cref="Node"/>s that must exist in a <seealso cref="Path"/> for it
		 ///                to be included. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> <seealso cref="Evaluation.INCLUDE_AND_CONTINUE"/> if all {@code nodes}
		 ///         exist in a given <seealso cref="Path"/>, otherwise
		 ///         <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includeIfContainsAll(final org.neo4j.graphdb.Node... nodes)
		 public static PathEvaluator<STATE> IncludeIfContainsAll<STATE>( params Node[] nodes )
		 {
			  if ( nodes.Length == 1 )
			  {
					return new PathEvaluator_AdapterAnonymousInnerClass10( nodes );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<org.neo4j.graphdb.Node> fullSet = new java.util.HashSet<>(asList(nodes));
			  ISet<Node> fullSet = new HashSet<Node>( asList( nodes ) );
			  return new PathEvaluator_AdapterAnonymousInnerClass11( fullSet );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass10 : PathEvaluator_Adapter<STATE>
		 {
			 private Node[] _nodes;

			 public PathEvaluator_AdapterAnonymousInnerClass10( Node[] nodes )
			 {
				 this._nodes = nodes;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  foreach ( Node node in path.ReverseNodes() )
				  {
						if ( node.Equals( _nodes[0] ) )
						{
							 return Evaluation.IncludeAndContinue;
						}
				  }
				  return Evaluation.ExcludeAndContinue;
			 }
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass11 : PathEvaluator_Adapter<STATE>
		 {
			 private ISet<Node> _fullSet;

			 public PathEvaluator_AdapterAnonymousInnerClass11( ISet<Node> fullSet )
			 {
				 this._fullSet = fullSet;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  ISet<Node> set = new HashSet<Node>( _fullSet );
				  foreach ( Node node in path.ReverseNodes() )
				  {
						if ( set.remove( node ) && set.Count == 0 )
						{
							 return Evaluation.IncludeAndContinue;
						}
				  }
				  return Evaluation.ExcludeAndContinue;
			 }
		 }

		 /// <summary>
		 /// Whereas adding <seealso cref="Evaluator"/>s to a <seealso cref="TraversalDescription"/> puts those
		 /// evaluators in {@code AND-mode} this can group many evaluators in {@code OR-mode}.
		 /// </summary>
		 /// <param name="evaluators"> represented as one evaluators. If any of the evaluators decides
		 ///                   to include a path it will be included. </param>
		 /// @param <STATE>    the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluator"/> which decides to include a path if any of the supplied
		 ///         evaluators wants to include it. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includeIfAcceptedByAny(final PathEvaluator... evaluators)
		 public static PathEvaluator<STATE> IncludeIfAcceptedByAny<STATE>( params PathEvaluator[] evaluators )
		 {
			  return new PathEvaluator_AdapterAnonymousInnerClass12( evaluators );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass12 : PathEvaluator_Adapter<STATE>
		 {
			 private PathEvaluator[] _evaluators;

			 public PathEvaluator_AdapterAnonymousInnerClass12( PathEvaluator[] evaluators )
			 {
				 this._evaluators = evaluators;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public Evaluation evaluate(org.neo4j.graphdb.Path path, BranchState state)
			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  foreach ( PathEvaluator evaluator in _evaluators )
				  {
						if ( evaluator.evaluate( path, state ).includes() )
						{
							 return Evaluation.IncludeAndContinue;
						}
				  }
				  return Evaluation.ExcludeAndContinue;
			 }
		 }

		 /// <summary>
		 /// Whereas adding <seealso cref="Evaluator"/>s to a <seealso cref="TraversalDescription"/> puts those
		 /// evaluators in {@code AND-mode} this can group many evaluators in {@code OR-mode}.
		 /// </summary>
		 /// <param name="evaluators"> represented as one evaluators. If any of the evaluators decides
		 ///                   to include a path it will be included. </param>
		 /// @param <STATE>    the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluator"/> which decides to include a path if any of the supplied
		 ///         evaluators wants to include it. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includeIfAcceptedByAny(final Evaluator... evaluators)
		 public static PathEvaluator<STATE> IncludeIfAcceptedByAny<STATE>( params Evaluator[] evaluators )
		 {
			  return new PathEvaluator_AdapterAnonymousInnerClass13( evaluators );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass13 : PathEvaluator_Adapter<STATE>
		 {
			 private Org.Neo4j.Graphdb.traversal.Evaluator[] _evaluators;

			 public PathEvaluator_AdapterAnonymousInnerClass13( Org.Neo4j.Graphdb.traversal.Evaluator[] evaluators )
			 {
				 this._evaluators = evaluators;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  foreach ( Evaluator evaluator in _evaluators )
				  {
						if ( evaluator.Evaluate( path ).includes() )
						{
							 return Evaluation.IncludeAndContinue;
						}
				  }
				  return Evaluation.ExcludeAndContinue;
			 }
		 }

		 /// <summary>
		 /// Returns <seealso cref="Evaluator"/>s for paths with the specified depth and with an end node from the list of
		 /// possibleEndNodes. </summary>
		 /// <param name="depth"> The exact depth to filter the returned path evaluators. </param>
		 /// <param name="possibleEndNodes"> Filter for the possible nodes to end the path on. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> <seealso cref="Evaluator"/>s for paths with the specified depth and with an end node from the list of
		 /// possibleEndNodes. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> endNodeIsAtDepth(final int depth, org.neo4j.graphdb.Node... possibleEndNodes)
		 public static PathEvaluator<STATE> EndNodeIsAtDepth<STATE>( int depth, params Node[] possibleEndNodes )
		 {
			  if ( possibleEndNodes.Length == 1 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node target = possibleEndNodes[0];
					Node target = possibleEndNodes[0];
					return new PathEvaluator_AdapterAnonymousInnerClass14( depth, target );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<org.neo4j.graphdb.Node> endNodes = new java.util.HashSet<>(asList(possibleEndNodes));
			  ISet<Node> endNodes = new HashSet<Node>( asList( possibleEndNodes ) );
			  return new PathEvaluator_AdapterAnonymousInnerClass15( depth, endNodes );
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass14 : PathEvaluator_Adapter<STATE>
		 {
			 private int _depth;
			 private Node _target;

			 public PathEvaluator_AdapterAnonymousInnerClass14( int depth, Node target )
			 {
				 this._depth = depth;
				 this._target = target;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  if ( path.Length() == _depth )
				  {
						return _target.Equals( path.EndNode() ) ? Evaluation.IncludeAndPrune : Evaluation.ExcludeAndPrune;
				  }
				  return Evaluation.ExcludeAndContinue;
			 }
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass15 : PathEvaluator_Adapter<STATE>
		 {
			 private int _depth;
			 private ISet<Node> _endNodes;

			 public PathEvaluator_AdapterAnonymousInnerClass15( int depth, ISet<Node> endNodes )
			 {
				 this._depth = depth;
				 this._endNodes = endNodes;
			 }

			 public override Evaluation evaluate( Path path, BranchState state )
			 {
				  if ( path.Length() == _depth )
				  {
						return _endNodes.Contains( path.EndNode() ) ? Evaluation.IncludeAndPrune : Evaluation.ExcludeAndPrune;
				  }
				  return Evaluation.ExcludeAndContinue;
			 }
		 }
	}

}