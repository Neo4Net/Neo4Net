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
namespace Org.Neo4j.Kernel.impl.traversal
{
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.traversal;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;
	using TraversalContext = Org.Neo4j.Graphdb.traversal.TraversalContext;

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