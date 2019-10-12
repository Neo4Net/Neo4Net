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
namespace Org.Neo4j.Kernel.impl.traversal
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb.traversal;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using Paths = Org.Neo4j.Graphdb.traversal.Paths;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;
	using TraversalContext = Org.Neo4j.Graphdb.traversal.TraversalContext;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Org.Neo4j.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asResourceIterator;

	internal class TraversalBranchImpl : TraversalBranch
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly TraversalBranch ParentConflict;
		 private readonly Relationship _howIGotHere;
		 private readonly Node _source;
		 private ResourceIterator<Relationship> _relationships;
		 // high bit here [cidd,dddd][dddd,dddd][dddd,dddd][dddd,dddd]
		 private int _depthAndEvaluationBits;
		 private int _expandedCount;

		 /*
		  * For expansion sources for all nodes except the start node
		  */
		 internal TraversalBranchImpl( TraversalBranch parent, int depth, Node source, Relationship toHere )
		 {
			  this.ParentConflict = parent;
			  this._source = source;
			  this._howIGotHere = toHere;
			  this._depthAndEvaluationBits = depth;
		 }

		 /*
		  * For the start node branches
		  */
		 internal TraversalBranchImpl( TraversalBranch parent, Node source )
		 {
			  this.ParentConflict = parent;
			  this._source = source;
			  this._howIGotHere = null;
			  this._depthAndEvaluationBits = 0;
		 }

		 protected internal virtual Evaluation Evaluation
		 {
			 set
			 {
				  this._depthAndEvaluationBits &= 0x3FFFFFFF; // First clear those value bits
				  this._depthAndEvaluationBits |= BitValue( value.includes(), 30 ) | BitValue(value.continues(), 31);
			 }
		 }

		 private int BitValue( bool value, int bit )
		 {
			  return ( value ? 1 : 0 ) << bit;
		 }

		 protected internal virtual void ExpandRelationships( PathExpander expander )
		 {
			  if ( Continues() )
			  {
					_relationships = ExpandRelationshipsWithoutChecks( expander );
			  }
			  else
			  {
					ResetRelationships();
			  }
		 }

		 protected internal virtual ResourceIterator ExpandRelationshipsWithoutChecks( PathExpander expander )
		 {
			  return asResourceIterator( expander.expand( this, BranchState.NO_STATE ).GetEnumerator() );
		 }

		 protected internal virtual bool HasExpandedRelationships()
		 {
			  return _relationships != null;
		 }

		 protected internal virtual void Evaluate( TraversalContext context )
		 {
			  Evaluation = context.Evaluate( this, null );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void initialize(final org.neo4j.graphdb.PathExpander expander, org.neo4j.graphdb.traversal.TraversalContext metadata)
		 public override void Initialize( PathExpander expander, TraversalContext metadata )
		 {
			  Evaluate( metadata );
		 }

		 public override TraversalBranch Next( PathExpander expander, TraversalContext context )
		 {
			  if ( _relationships == null )
			  {
					ExpandRelationships( expander );
			  }
			  while ( _relationships.MoveNext() )
			  {
					Relationship relationship = _relationships.Current;
					if ( relationship.Equals( _howIGotHere ) )
					{
						 context.UnnecessaryRelationshipTraversed();
						 continue;
					}
					_expandedCount++;
					Node node = relationship.GetOtherNode( _source );
					// TODO maybe an unnecessary instantiation. Instead pass in this+node+relationship to uniqueness check
					TraversalBranch next = NewNextBranch( node, relationship );
					if ( context.IsUnique( next ) )
					{
						 context.RelationshipTraversed();
						 next.Initialize( expander, context );
						 return next;
					}
					else
					{
						 context.UnnecessaryRelationshipTraversed();
					}
			  }
			  ResetRelationships();
			  return null;
		 }

		 protected internal virtual TraversalBranch NewNextBranch( Node node, Relationship relationship )
		 {
			  return new TraversalBranchImpl( this, Length() + 1, node, relationship );
		 }

		 public override void Prune()
		 {
			  ResetRelationships();
		 }

		 private void ResetRelationships()
		 {
			  if ( _relationships != null )
			  {
					_relationships.close();
			  }
			  _relationships = Iterators.emptyResourceIterator();
		 }

		 public override int Length()
		 {
			  return _depthAndEvaluationBits & 0x3FFFFFFF;
		 }

		 public override TraversalBranch Parent()
		 {
			  return this.ParentConflict;
		 }

		 public override int Expanded()
		 {
			  return _expandedCount;
		 }

		 public override bool Includes()
		 {
			  return ( _depthAndEvaluationBits & 0x40000000 ) != 0;
		 }

		 public override bool Continues()
		 {
			  return ( _depthAndEvaluationBits & 0x80000000 ) != 0;
		 }

		 public override void Evaluation( Evaluation eval )
		 {
			  Evaluation = Evaluation.of( Includes() & eval.includes(), Continues() & eval.continues() );
		 }

		 public override Node StartNode()
		 {
			  return FindStartBranch().endNode();
		 }

		 private TraversalBranch FindStartBranch()
		 {
			  TraversalBranch branch = this;
			  while ( branch.Length() > 0 )
			  {
					branch = branch.Parent();
			  }
			  return branch;
		 }

		 public override Node EndNode()
		 {
			  return _source;
		 }

		 public override Relationship LastRelationship()
		 {
			  return _howIGotHere;
		 }

		 public override IEnumerable<Relationship> Relationships()
		 {
			  LinkedList<Relationship> relationships = new LinkedList<Relationship>();
			  TraversalBranch branch = this;
			  while ( branch.Length() > 0 )
			  {
					relationships.AddFirst( branch.LastRelationship() );
					branch = branch.Parent();
			  }
			  return relationships;
		 }

		 public override IEnumerable<Relationship> ReverseRelationships()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<Relationship>
		 {
			 private readonly TraversalBranchImpl _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( TraversalBranchImpl outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 branch = outerInstance;
			 }

			 private TraversalBranch branch;

			 protected internal override Relationship fetchNextOrNull()
			 {
				  try
				  {
						return branch != null ? branch.lastRelationship() : null;
				  }
				  finally
				  {
						branch = branch != null ? branch.parent() : null;
				  }
			 }
		 }

		 public override IEnumerable<Node> Nodes()
		 {
			  LinkedList<Node> nodes = new LinkedList<Node>();
			  TraversalBranch branch = this;
			  while ( branch.Length() > 0 )
			  {
					nodes.AddFirst( branch.EndNode() );
					branch = branch.Parent();
			  }
			  nodes.AddFirst( branch.EndNode() );
			  return nodes;
		 }

		 public override IEnumerable<Node> ReverseNodes()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass2(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass2 : PrefetchingIterator<Node>
		 {
			 private readonly TraversalBranchImpl _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass2( TraversalBranchImpl outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 branch = outerInstance;
			 }

			 private TraversalBranch branch;

			 protected internal override Node fetchNextOrNull()
			 {
				  try
				  {
						return branch.length() >= 0 ? branch.endNode() : null;
				  }
				  finally
				  {
						branch = branch.parent();
				  }
			 }
		 }

		 public override IEnumerator<PropertyContainer> Iterator()
		 {
			  LinkedList<PropertyContainer> entities = new LinkedList<PropertyContainer>();
			  TraversalBranch branch = this;
			  while ( branch.Length() > 0 )
			  {
					entities.AddFirst( branch.EndNode() );
					entities.AddFirst( branch.LastRelationship() );
					branch = branch.Parent();
			  }
			  entities.AddFirst( branch.EndNode() );
			  return entities.GetEnumerator();
		 }

		 public override int GetHashCode()
		 {
			  TraversalBranch branch = this;
			  int hashCode = 1;
			  while ( branch.Length() > 0 )
			  {
					Relationship relationship = branch.LastRelationship();
					hashCode = 31 * hashCode + relationship.GetHashCode();
					branch = branch.Parent();
			  }
			  if ( hashCode == 1 )
			  {
					hashCode = EndNode().GetHashCode();
			  }
			  return hashCode;
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj == this )
			  {
					return true;
			  }
			  if ( !( obj is TraversalBranch ) )
			  {
					return false;
			  }

			  TraversalBranch branch = this;
			  TraversalBranch other = ( TraversalBranch ) obj;
			  if ( branch.Length() != other.Length() )
			  {
					return false;
			  }

			  while ( branch.Length() > 0 )
			  {
					if ( !branch.LastRelationship().Equals(other.LastRelationship()) )
					{
						 return false;
					}
					branch = branch.Parent();
					other = other.Parent();
			  }
			  return true;
		 }

		 public override string ToString()
		 {
			  return Paths.defaultPathToString( this );
		 }

		 public override object State()
		 {
			  return null;
		 }
	}

}