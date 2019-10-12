using System;
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
namespace Neo4Net.Kernel.impl.traversal
{

	using Direction = Neo4Net.Graphdb.Direction;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Neo4Net.Graphdb;
	using PathExpanders = Neo4Net.Graphdb.PathExpanders;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Resource = Neo4Net.Graphdb.Resource;
	using StandardExpander = Neo4Net.Graphdb.impl.StandardExpander;
	using BranchOrderingPolicies = Neo4Net.Graphdb.traversal.BranchOrderingPolicies;
	using BranchOrderingPolicy = Neo4Net.Graphdb.traversal.BranchOrderingPolicy;
	using Evaluator = Neo4Net.Graphdb.traversal.Evaluator;
	using Evaluators = Neo4Net.Graphdb.traversal.Evaluators;
	using Neo4Net.Graphdb.traversal;
	using Neo4Net.Graphdb.traversal;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;
	using Traverser = Neo4Net.Graphdb.traversal.Traverser;
	using Uniqueness = Neo4Net.Graphdb.traversal.Uniqueness;
	using UniquenessFactory = Neo4Net.Graphdb.traversal.UniquenessFactory;

	public sealed class MonoDirectionalTraversalDescription : TraversalDescription
	{
		 internal static readonly System.Func<Resource> NoStatement = () => Neo4Net.Graphdb.Resource_Fields.Empty;

		 internal readonly PathExpander Expander;
		 internal readonly InitialBranchState InitialState;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: final System.Func<? extends org.neo4j.graphdb.Resource> statementSupplier;
		 internal readonly System.Func<Resource> StatementSupplier;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly UniquenessFactory UniquenessConflict;
		 internal readonly object UniquenessParameter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly PathEvaluator EvaluatorConflict;
		 internal readonly BranchOrderingPolicy BranchOrdering;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: final java.util.Comparator<? super org.neo4j.graphdb.Path> sorting;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 internal readonly IComparer<object> Sorting;
		 internal readonly ICollection<Node> EndNodes;

		 public MonoDirectionalTraversalDescription() : this(NoStatement)
		 {
			  /*
			   * Use one statement per operation performed, rather than a global statement for the whole traversal. This is
			   * significantly less performant, and only used when accessing the traversal framework via the legacy access
			   * methods (eg. Traversal.description()).
			   */
		 }

		 public MonoDirectionalTraversalDescription<T1>( System.Func<T1> statementProvider ) where T1 : Neo4Net.Graphdb.Resource : this( PathExpanders.allTypesAndDirections(), Uniqueness.NODE_GLOBAL, null, Evaluators.all(), InitialBranchState.NO_STATE, BranchOrderingPolicies.PREORDER_DEPTH_FIRST, null, null, statementProvider )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private MonoDirectionalTraversalDescription(org.neo4j.graphdb.PathExpander expander, org.neo4j.graphdb.traversal.UniquenessFactory uniqueness, Object uniquenessParameter, org.neo4j.graphdb.traversal.PathEvaluator evaluator, org.neo4j.graphdb.traversal.InitialBranchState initialState, org.neo4j.graphdb.traversal.BranchOrderingPolicy branchOrdering, java.util.Comparator<? super org.neo4j.graphdb.Path> sorting, java.util.Collection<org.neo4j.graphdb.Node> endNodes, System.Func<? extends org.neo4j.graphdb.Resource> statementSupplier)
		 private MonoDirectionalTraversalDescription<T1, T2>( PathExpander expander, UniquenessFactory uniqueness, object uniquenessParameter, PathEvaluator evaluator, InitialBranchState initialState, BranchOrderingPolicy branchOrdering, IComparer<T1> sorting, ICollection<Node> endNodes, System.Func<T2> statementSupplier ) where T2 : Neo4Net.Graphdb.Resource
		 {
			  this.Expander = expander;
			  this.UniquenessConflict = uniqueness;
			  this.UniquenessParameter = uniquenessParameter;
			  this.EvaluatorConflict = evaluator;
			  this.BranchOrdering = branchOrdering;
			  this.Sorting = sorting;
			  this.EndNodes = endNodes;
			  this.InitialState = initialState;
			  this.StatementSupplier = statementSupplier;
		 }

		 public override Traverser Traverse( Node startNode )
		 {
			  return Traverse( Collections.singletonList( startNode ) );
		 }

		 public override Traverser Traverse( params Node[] startNodes )
		 {
			  return Traverse( Arrays.asList( startNodes ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.graphdb.traversal.Traverser traverse(final Iterable<org.neo4j.graphdb.Node> iterableStartNodes)
		 public override Traverser Traverse( IEnumerable<Node> iterableStartNodes )
		 {
			  return new DefaultTraverser(() =>
			  {
				Resource statement = StatementSupplier.get();
				MonoDirectionalTraverserIterator iterator = new MonoDirectionalTraverserIterator( statement, UniquenessConflict.create( UniquenessParameter ), Expander, BranchOrdering, EvaluatorConflict, iterableStartNodes, InitialState, UniquenessConflict );
				return Sorting != null ? new SortingTraverserIterator( statement, Sorting, iterator ) : iterator;
			  });
		 }

		 /* (non-Javadoc)
		  * @see org.neo4j.graphdb.traversal.TraversalDescription#uniqueness(org.neo4j.graphdb.traversal.Uniqueness)
		  */
		 public override TraversalDescription Uniqueness( UniquenessFactory uniqueness )
		 {
			  return new MonoDirectionalTraversalDescription( Expander, uniqueness, null, EvaluatorConflict, InitialState, BranchOrdering, Sorting, EndNodes, StatementSupplier );
		 }

		 /* (non-Javadoc)
		  * @see org.neo4j.graphdb.traversal.TraversalDescription#uniqueness(org.neo4j.graphdb.traversal.Uniqueness, java.lang.Object)
		  */
		 public override TraversalDescription Uniqueness( UniquenessFactory uniqueness, object parameter )
		 {
			  if ( this.UniquenessConflict == uniqueness && ( UniquenessParameter == null ? parameter == null : UniquenessParameter.Equals( parameter ) ) )
			  {
					return this;
			  }

			  return new MonoDirectionalTraversalDescription( Expander, uniqueness, parameter, EvaluatorConflict, InitialState, BranchOrdering, Sorting, EndNodes, StatementSupplier );
		 }

		 public override TraversalDescription Evaluator( Evaluator evaluator )
		 {
			  return evaluator( new Neo4Net.Graphdb.traversal.Evaluator_AsPathEvaluator( evaluator ) );
		 }

		 public override TraversalDescription Evaluator( PathEvaluator evaluator )
		 {
			  if ( this.EvaluatorConflict == evaluator )
			  {
					return this;
			  }
			  NullCheck( evaluator, typeof( Evaluator ), "RETURN_ALL" );
			  return new MonoDirectionalTraversalDescription( Expander, UniquenessConflict, UniquenessParameter, AddEvaluator( this.EvaluatorConflict, evaluator ), InitialState, BranchOrdering, Sorting, EndNodes, StatementSupplier );
		 }

		 protected internal static PathEvaluator AddEvaluator( PathEvaluator existing, PathEvaluator toAdd )
		 {
			  if ( existing is MultiEvaluator )
			  {
					return ( ( MultiEvaluator ) existing ).add( toAdd );
			  }
			  else
			  {
					return existing == Evaluators.all() ? toAdd : new MultiEvaluator(existing, toAdd);
			  }
		 }

		 protected internal static void NullCheck<T>( T parameter, Type parameterType, string defaultName )
		 {
				 parameterType = typeof( T );
			  if ( parameter == default( T ) )
			  {
					string typeName = parameterType.Name;
					throw new System.ArgumentException( typeName + " may not be null, use " + typeName + "." + defaultName + " instead." );
			  }
		 }

		 /* (non-Javadoc)
		  * @see org.neo4j.graphdb.traversal.TraversalDescription#order(org.neo4j.graphdb.traversal.Order)
		  */
		 public override TraversalDescription Order( BranchOrderingPolicy order )
		 {
			  if ( this.BranchOrdering == order )
			  {
					return this;
			  }
			  return new MonoDirectionalTraversalDescription( Expander, UniquenessConflict, UniquenessParameter, EvaluatorConflict, InitialState, order, Sorting, EndNodes, StatementSupplier );
		 }

		 public override TraversalDescription DepthFirst()
		 {
			  return Order( BranchOrderingPolicies.PREORDER_DEPTH_FIRST );
		 }

		 public override TraversalDescription BreadthFirst()
		 {
			  return Order( BranchOrderingPolicies.PREORDER_BREADTH_FIRST );
		 }

		 /* (non-Javadoc)
		  * @see org.neo4j.graphdb.traversal.TraversalDescription#relationships(org.neo4j.graphdb.RelationshipType)
		  */
		 public override TraversalDescription Relationships( RelationshipType type )
		 {
			  return Relationships( type, Direction.BOTH );
		 }

		 /* (non-Javadoc)
		  * @see org.neo4j.graphdb.traversal.TraversalDescription#relationships(org.neo4j.graphdb.RelationshipType, org.neo4j.graphdb.Direction)
		  */
		 public override TraversalDescription Relationships( RelationshipType type, Direction direction )
		 {
			  if ( Expander is StandardExpander )
			  {
					return Expand( ( ( StandardExpander ) Expander ).add( type, direction ) );
			  }
			  throw new System.InvalidOperationException( "The current expander cannot be added to" );
		 }

		 public override TraversalDescription Expand<T1>( PathExpander<T1> expander )
		 {
			  if ( expander.Equals( this.Expander ) )
			  {
					return this;
			  }
			  return new MonoDirectionalTraversalDescription( expander, UniquenessConflict, UniquenessParameter, EvaluatorConflict, InitialState, BranchOrdering, Sorting, EndNodes, StatementSupplier );
		 }

		 public override TraversalDescription Expand<STATE>( PathExpander<STATE> expander, InitialBranchState<STATE> initialState )
		 {
			  return new MonoDirectionalTraversalDescription( expander, UniquenessConflict, UniquenessParameter, EvaluatorConflict, initialState, BranchOrdering, Sorting, EndNodes, StatementSupplier );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.neo4j.graphdb.traversal.TraversalDescription sort(java.util.Comparator<? super org.neo4j.graphdb.Path> sorting)
		 public override TraversalDescription Sort<T1>( IComparer<T1> sorting )
		 {
			  return new MonoDirectionalTraversalDescription( Expander, UniquenessConflict, UniquenessParameter, EvaluatorConflict, InitialState, BranchOrdering, sorting, EndNodes, StatementSupplier );
		 }

		 public override TraversalDescription Reverse()
		 {
			  return new MonoDirectionalTraversalDescription( Expander.reverse(), UniquenessConflict, UniquenessParameter, EvaluatorConflict, InitialState.reverse(), BranchOrdering, Sorting, EndNodes, StatementSupplier );
		 }
	}

}