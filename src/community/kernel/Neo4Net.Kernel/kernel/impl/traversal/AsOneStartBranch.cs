using System.Collections.Generic;

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

	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using BranchSelector = Neo4Net.GraphDb.traversal.BranchSelector;
	using Evaluation = Neo4Net.GraphDb.traversal.Evaluation;
	using Neo4Net.GraphDb.traversal;
	using TraversalBranch = Neo4Net.GraphDb.traversal.TraversalBranch;
	using TraversalContext = Neo4Net.GraphDb.traversal.TraversalContext;
	using TraversalDescription = Neo4Net.GraphDb.traversal.TraversalDescription;
	using UniquenessFactory = Neo4Net.GraphDb.traversal.UniquenessFactory;

	/// <summary>
	/// A <seealso cref="TraversalBranch"/> that abstracts the fact that it is actually
	/// potentially several branches, i.e. several start nodes. A
	/// <seealso cref="TraversalDescription.traverse(Node...)"/> call can supply more than
	/// one starting <seealso cref="Node"/> and for implementation simplicity a
	/// <seealso cref="BranchSelector"/> starts from one <seealso cref="TraversalBranch"/>.
	/// This class bridges that gap.
	/// 
	/// @author Mattias Persson
	/// </summary>
	internal class AsOneStartBranch : TraversalBranch
	{
		 private IEnumerator<TraversalBranch> _branches;
		 private int _expanded;
		 private readonly TraversalContext _context;
		 private readonly InitialBranchState _initialState;
		 private readonly UniquenessFactory _uniqueness;

		 internal AsOneStartBranch( TraversalContext context, IEnumerable<Node> nodes, InitialBranchState initialState, UniquenessFactory uniqueness )
		 {
			  this._context = context;
			  this._initialState = initialState;
			  this._uniqueness = uniqueness;
			  this._branches = ToBranches( nodes );
		 }

		 private IEnumerator<TraversalBranch> ToBranches( IEnumerable<Node> nodes )
		 {
			  if ( _uniqueness.eagerStartBranches() )
			  {
					IList<TraversalBranch> result = new List<TraversalBranch>();
					foreach ( Node node in nodes )
					{
						 result.Add( new StartNodeTraversalBranch( _context, this, node, _initialState ) );
					}
					return result.GetEnumerator();
			  }
			  else
			  {
					return new TraversalBranchIterator( this, nodes.GetEnumerator() );
			  }
		 }

		 public override TraversalBranch Parent()
		 {
			  return null;
		 }

		 public override int Length()
		 {
			  return -1;
		 }

		 public override Node EndNode()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override Relationship LastRelationship()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override TraversalBranch Next( PathExpander expander, TraversalContext metadata )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _branches.hasNext() )
			  {
					_expanded++;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return _branches.next().next(expander, metadata);
			  }
			  return null;
		 }

		 public override int Expanded()
		 {
			  return _expanded;
		 }

		 public override bool Continues()
		 {
			  return true;
		 }

		 public override bool Includes()
		 {
			  return false;
		 }

		 public override void Evaluation( Evaluation eval )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Initialize( PathExpander expander, TraversalContext metadata )
		 {
		 }

		 public override Node StartNode()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override IEnumerable<Relationship> Relationships()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override IEnumerable<Relationship> ReverseRelationships()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override IEnumerable<Node> Nodes()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override IEnumerable<Node> ReverseNodes()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override IEnumerator<PropertyContainer> Iterator()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Prune()
		 {
			  _branches = System.Linq.Enumerable.Empty<TraversalBranch>().GetEnumerator();
		 }

		 public override object State()
		 {
			  throw new System.NotSupportedException();
		 }

		 private class TraversalBranchIterator : IEnumerator<TraversalBranch>
		 {
			 private readonly AsOneStartBranch _outerInstance;

			  internal readonly IEnumerator<Node> NodeIterator;

			  internal TraversalBranchIterator( AsOneStartBranch outerInstance, IEnumerator<Node> nodeIterator )
			  {
				  this._outerInstance = outerInstance;
					this.NodeIterator = nodeIterator;
			  }

			  public override bool HasNext()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return NodeIterator.hasNext();
			  }

			  public override TraversalBranch Next()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return new StartNodeTraversalBranch( _outerInstance.context, _outerInstance, NodeIterator.next(), _outerInstance.initialState );
			  }

			  public override void Remove()
			  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					NodeIterator.remove();
			  }
		 }
	}

}