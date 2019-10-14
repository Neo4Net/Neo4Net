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
	using Neo4Net.Graphdb.traversal;
	using TraversalBranch = Neo4Net.Graphdb.traversal.TraversalBranch;
	using TraversalContext = Neo4Net.Graphdb.traversal.TraversalContext;

	internal class StartNodeTraversalBranch : TraversalBranchWithState
	{
		 private readonly InitialBranchState _initialState;

		 internal StartNodeTraversalBranch( TraversalContext context, TraversalBranch parent, Node source, InitialBranchState initialState ) : base( parent, source, initialState )
		 {
			  this._initialState = initialState;
			  Evaluate( context );
			  context.IsUniqueFirst( this );
		 }

		 public override TraversalBranch Next( PathExpander expander, TraversalContext metadata )
		 {
			  if ( !HasExpandedRelationships() )
			  {
					ExpandRelationships( expander );
					return this;
			  }
			  return base.Next( expander, metadata );
		 }

		 protected internal override TraversalBranch NewNextBranch( Node node, Relationship relationship )
		 {
			  return _initialState != InitialBranchState.NO_STATE ? new TraversalBranchWithState( this, 1, node, relationship, StateForChildren ) : new TraversalBranchImpl( this, 1, node, relationship );
		 }
	}

}