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
namespace Org.Neo4j.Cypher.export
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Result = Org.Neo4j.Graphdb.Result;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.loop;

	public class CypherResultSubGraph : SubGraph
	{

		 private readonly SortedDictionary<long, Node> _nodes = new SortedDictionary<long, Node>();
		 private readonly SortedDictionary<long, Relationship> _relationships = new SortedDictionary<long, Relationship>();
		 private readonly ICollection<Label> _labels = new HashSet<Label>();
		 private readonly ICollection<IndexDefinition> _indexes = new HashSet<IndexDefinition>();
		 private readonly ICollection<ConstraintDefinition> _constraints = new HashSet<ConstraintDefinition>();

		 public virtual void Add( Node node )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long id = node.getId();
			  long id = node.Id;
			  if ( !_nodes.ContainsKey( id ) )
			  {
					AddNode( id, node );
			  }
		 }

		 internal virtual void AddNode( long id, Node data )
		 {
			  _nodes[id] = data;
			  _labels.addAll( Iterables.asCollection( data.Labels ) );
		 }

		 public virtual void Add( Relationship rel )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long id = rel.getId();
			  long id = rel.Id;
			  if ( !_relationships.ContainsKey( id ) )
			  {
					AddRel( id, rel );
					Add( rel.StartNode );
					Add( rel.EndNode );
			  }
		 }

		 public static SubGraph From( Result result, GraphDatabaseService gds, bool addBetween )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CypherResultSubGraph graph = new CypherResultSubGraph();
			  CypherResultSubGraph graph = new CypherResultSubGraph();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> columns = result.columns();
			  IList<string> columns = result.Columns();
			  foreach ( IDictionary<string, object> row in loop( result ) )
			  {
					foreach ( string column in columns )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object value = row.get(column);
						 object value = row[column];
						 graph.AddToGraph( value );
					}
			  }
			  foreach ( IndexDefinition def in gds.Schema().Indexes )
			  {
					foreach ( Label label in def.Labels )
					{
						 if ( graph.Labels.Contains( label ) )
						 {
							  graph.AddIndex( def );
							  break;
						 }
					}
			  }
			  foreach ( ConstraintDefinition def in gds.Schema().Constraints )
			  {
					if ( graph.Labels.Contains( def.Label ) )
					{
						 graph.AddConstraint( def );
					}
			  }
			  if ( addBetween )
			  {
					graph.AddRelationshipsBetweenNodes();
			  }
			  return graph;
		 }

		 private void AddIndex( IndexDefinition def )
		 {
			  _indexes.Add( def );
		 }

		 private void AddConstraint( ConstraintDefinition def )
		 {
			  _constraints.Add( def );
		 }

		 private void AddRelationshipsBetweenNodes()
		 {
			  ISet<Node> newNodes = new HashSet<Node>();
			  foreach ( Node node in _nodes.Values )
			  {
					foreach ( Relationship relationship in node.Relationships )
					{
						 if ( !_relationships.ContainsKey( relationship.Id ) )
						 {
							  continue;
						 }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node other = relationship.getOtherNode(node);
						 Node other = relationship.GetOtherNode( node );
						 if ( _nodes.ContainsKey( other.Id ) || newNodes.Contains( other ) )
						 {
							  continue;
						 }
						 newNodes.Add( other );
					}
			  }
			  foreach ( Node node in newNodes )
			  {
					Add( node );
			  }
		 }

		 private void AddToGraph( object value )
		 {
			  if ( value is Node )
			  {
					Add( ( Node ) value );
			  }
			  if ( value is Relationship )
			  {
					Add( ( Relationship ) value );
			  }
			  if ( value is System.Collections.IEnumerable )
			  {
					foreach ( object inner in ( System.Collections.IEnumerable ) value )
					{
						 AddToGraph( inner );
					}
			  }
		 }

		 public virtual IEnumerable<Node> Nodes
		 {
			 get
			 {
				  return _nodes.Values;
			 }
		 }

		 public virtual IEnumerable<Relationship> Relationships
		 {
			 get
			 {
				  return _relationships.Values;
			 }
		 }

		 public virtual ICollection<Label> Labels
		 {
			 get
			 {
				  return _labels;
			 }
		 }

		 internal virtual void AddRel( long? id, Relationship rel )
		 {
			  _relationships[id] = rel;
		 }

		 public override bool Contains( Relationship relationship )
		 {
			  return _relationships.ContainsKey( relationship.Id );
		 }

		 public virtual IEnumerable<IndexDefinition> Indexes
		 {
			 get
			 {
				  return _indexes;
			 }
		 }

		 public virtual IEnumerable<ConstraintDefinition> Constraints
		 {
			 get
			 {
				  return _constraints;
			 }
		 }

	}

}