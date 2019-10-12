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

	using Node = Neo4Net.Graphdb.Node;
	using Resource = Neo4Net.Graphdb.Resource;
	using BidirectionalTraversalDescription = Neo4Net.Graphdb.traversal.BidirectionalTraversalDescription;
	using BranchCollisionPolicy = Neo4Net.Graphdb.traversal.BranchCollisionPolicy;
	using Evaluator = Neo4Net.Graphdb.traversal.Evaluator;
	using Evaluators = Neo4Net.Graphdb.traversal.Evaluators;
	using Neo4Net.Graphdb.traversal;
	using SideSelectorPolicy = Neo4Net.Graphdb.traversal.SideSelectorPolicy;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;
	using Traverser = Neo4Net.Graphdb.traversal.Traverser;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.BranchCollisionPolicies.STANDARD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.SideSelectorPolicies.ALTERNATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.traversal.MonoDirectionalTraversalDescription.NO_STATEMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.traversal.MonoDirectionalTraversalDescription.addEvaluator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.traversal.MonoDirectionalTraversalDescription.nullCheck;

	public class BidirectionalTraversalDescriptionImpl : BidirectionalTraversalDescription
	{
		 internal readonly MonoDirectionalTraversalDescription Start;
		 internal readonly MonoDirectionalTraversalDescription End;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly PathEvaluator CollisionEvaluatorConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly SideSelectorPolicy SideSelectorConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly BranchCollisionPolicy CollisionPolicyConflict;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: final System.Func<? extends org.neo4j.graphdb.Resource> statementFactory;
		 internal readonly System.Func<Resource> StatementFactory;
		 internal readonly int MaxDepth;

		 private BidirectionalTraversalDescriptionImpl<T1>( MonoDirectionalTraversalDescription start, MonoDirectionalTraversalDescription end, BranchCollisionPolicy collisionPolicy, PathEvaluator collisionEvaluator, SideSelectorPolicy sideSelector, System.Func<T1> statementFactory, int maxDepth ) where T1 : Neo4Net.Graphdb.Resource
		 {
			  this.Start = start;
			  this.End = end;
			  this.CollisionPolicyConflict = collisionPolicy;
			  this.CollisionEvaluatorConflict = collisionEvaluator;
			  this.SideSelectorConflict = sideSelector;
			  this.StatementFactory = statementFactory;
			  this.MaxDepth = maxDepth;
		 }

		 public BidirectionalTraversalDescriptionImpl<T1>( System.Func<T1> statementFactory ) where T1 : Neo4Net.Graphdb.Resource : this( new MonoDirectionalTraversalDescription(), new MonoDirectionalTraversalDescription(), STANDARD, Evaluators.all(), ALTERNATING, statementFactory, int.MaxValue )
		 {
		 }

		 public BidirectionalTraversalDescriptionImpl() : this(NO_STATEMENT)
		 {
		 }

		 public override BidirectionalTraversalDescription StartSide( TraversalDescription startSideDescription )
		 {
			  AssertIsMonoDirectional( startSideDescription );
			  return new BidirectionalTraversalDescriptionImpl( ( MonoDirectionalTraversalDescription )startSideDescription, this.End, this.CollisionPolicyConflict, this.CollisionEvaluatorConflict, this.SideSelectorConflict, StatementFactory, this.MaxDepth );
		 }

		 public override BidirectionalTraversalDescription EndSide( TraversalDescription endSideDescription )
		 {
			  AssertIsMonoDirectional( endSideDescription );
			  return new BidirectionalTraversalDescriptionImpl( this.Start, ( MonoDirectionalTraversalDescription )endSideDescription, this.CollisionPolicyConflict, this.CollisionEvaluatorConflict, this.SideSelectorConflict, StatementFactory, this.MaxDepth );
		 }

		 public override BidirectionalTraversalDescription MirroredSides( TraversalDescription sideDescription )
		 {
			  AssertIsMonoDirectional( sideDescription );
			  return new BidirectionalTraversalDescriptionImpl( ( MonoDirectionalTraversalDescription )sideDescription, ( MonoDirectionalTraversalDescription )sideDescription.Reverse(), CollisionPolicyConflict, CollisionEvaluatorConflict, SideSelectorConflict, StatementFactory, MaxDepth );
		 }

		 public override BidirectionalTraversalDescription CollisionPolicy( BranchCollisionPolicy collisionPolicy )
		 {
			  return new BidirectionalTraversalDescriptionImpl( this.Start, this.End, collisionPolicy, this.CollisionEvaluatorConflict, this.SideSelectorConflict, StatementFactory, this.MaxDepth );
		 }

		 public override BidirectionalTraversalDescription CollisionEvaluator( PathEvaluator collisionEvaluator )
		 {
			  nullCheck( collisionEvaluator, typeof( Evaluator ), "RETURN_ALL" );
			  return new BidirectionalTraversalDescriptionImpl( this.Start, this.End, this.CollisionPolicyConflict, addEvaluator( this.CollisionEvaluatorConflict, collisionEvaluator ), this.SideSelectorConflict, StatementFactory, MaxDepth );
		 }

		 public override BidirectionalTraversalDescription CollisionEvaluator( Evaluator collisionEvaluator )
		 {
			  return collisionEvaluator( new Neo4Net.Graphdb.traversal.Evaluator_AsPathEvaluator( collisionEvaluator ) );
		 }

		 public override BidirectionalTraversalDescription SideSelector( SideSelectorPolicy sideSelector, int maxDepth )
		 {
			  return new BidirectionalTraversalDescriptionImpl( this.Start, this.End, this.CollisionPolicyConflict, this.CollisionEvaluatorConflict, sideSelector, StatementFactory, maxDepth );
		 }

		 public override Traverser Traverse( Node start, Node end )
		 {
			  return Traverse( Arrays.asList( start ), Arrays.asList( end ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.graphdb.traversal.Traverser traverse(final Iterable<org.neo4j.graphdb.Node> startNodes, final Iterable<org.neo4j.graphdb.Node> endNodes)
		 public override Traverser Traverse( IEnumerable<Node> startNodes, IEnumerable<Node> endNodes )
		 {
			  return new DefaultTraverser(() =>
			  {
				Resource resource = StatementFactory.get();
				bool success = false;
				try
				{
					 BidirectionalTraverserIterator iterator = new BidirectionalTraverserIterator( resource, Start, End, SideSelectorConflict, CollisionPolicyConflict, CollisionEvaluatorConflict, MaxDepth, startNodes, endNodes );
					 success = true;
					 return iterator;
				}
				finally
				{
					 if ( !success )
					 {
						  resource.close();
					 }
				}
			  });
		 }

		 /// <summary>
		 /// We currently only support mono-directional traversers as "inner" traversers, so we need to check specifically
		 /// for this when the user specifies traversers to work with.
		 /// </summary>
		 private void AssertIsMonoDirectional( TraversalDescription traversal )
		 {
			  if ( !( traversal is MonoDirectionalTraversalDescription ) )
			  {
					throw new System.ArgumentException( "The bi-directional traversals currently do not support using " + "anything but mono-directional traversers as start and stop points. Please provide a regular " + "mono-directional traverser instead." );
			  }
		 }
	}

}