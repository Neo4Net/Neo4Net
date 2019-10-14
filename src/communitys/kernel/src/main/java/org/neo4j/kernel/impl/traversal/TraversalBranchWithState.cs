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
namespace Neo4Net.Kernel.impl.traversal
{
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb.traversal;
	using Neo4Net.Graphdb.traversal;
	using TraversalBranch = Neo4Net.Graphdb.traversal.TraversalBranch;
	using TraversalContext = Neo4Net.Graphdb.traversal.TraversalContext;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;

	public class TraversalBranchWithState : TraversalBranchImpl, BranchState
	{
		 protected internal readonly object StateForMe;
		 protected internal object StateForChildren;

		 public TraversalBranchWithState( TraversalBranch parent, int depth, Node source, Relationship toHere, object inheritedState ) : base( parent, depth, source, toHere )
		 {
			  this.StateForMe = this.StateForChildren = inheritedState;
		 }

		 public TraversalBranchWithState( TraversalBranch parent, Node source, InitialBranchState initialState ) : base( parent, source )
		 {
			  this.StateForMe = this.StateForChildren = initialState.initialState( this );
		 }

		 public virtual object State
		 {
			 set
			 {
				  this.StateForChildren = value;
			 }
			 get
			 {
				  return this.StateForMe;
			 }
		 }


		 protected internal override TraversalBranch NewNextBranch( Node node, Relationship relationship )
		 {
			  return new TraversalBranchWithState( this, Length() + 1, node, relationship, StateForChildren );
		 }

		 protected internal override ResourceIterator<Relationship> ExpandRelationshipsWithoutChecks( PathExpander expander )
		 {
			  System.Collections.IEnumerable expandIterable = expander.expand( this, this );
			  return Iterators.asResourceIterator( expandIterable.GetEnumerator() );
		 }

		 public override object State()
		 {
			  return this.StateForMe;
		 }

		 protected internal override void Evaluate( TraversalContext context )
		 {
			  Evaluation = context.Evaluate( this, this );
		 }
	}

}