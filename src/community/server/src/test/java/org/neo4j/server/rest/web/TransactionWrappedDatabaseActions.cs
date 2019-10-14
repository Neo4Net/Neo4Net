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
namespace Neo4Net.Server.rest.web
{

	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using EndNodeNotFoundException = Neo4Net.Server.rest.domain.EndNodeNotFoundException;
	using StartNodeNotFoundException = Neo4Net.Server.rest.domain.StartNodeNotFoundException;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using ConstraintDefinitionRepresentation = Neo4Net.Server.rest.repr.ConstraintDefinitionRepresentation;
	using IndexDefinitionRepresentation = Neo4Net.Server.rest.repr.IndexDefinitionRepresentation;
	using IndexRepresentation = Neo4Net.Server.rest.repr.IndexRepresentation;
	using IndexedEntityRepresentation = Neo4Net.Server.rest.repr.IndexedEntityRepresentation;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using NodeRepresentation = Neo4Net.Server.rest.repr.NodeRepresentation;
	using Neo4Net.Server.rest.repr;
	using RelationshipRepresentation = Neo4Net.Server.rest.repr.RelationshipRepresentation;

	/// <summary>
	/// A class that is helpful when testing DatabaseActions. The alternative would be to writ tx-scaffolding in each test.
	/// <para>
	/// Some methods are _not_ wrapped: those are the ones that return a representation which is later serialised,
	/// as that requires a transaction. For those, the test have scaffolding added.
	/// </para>
	/// </summary>
	public class TransactionWrappedDatabaseActions : DatabaseActions
	{
		 private readonly GraphDatabaseAPI _graph;

		 public TransactionWrappedDatabaseActions( GraphDatabaseAPI graph ) : base( graph )
		 {
			  this._graph = graph;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.NodeRepresentation createNode(java.util.Map<String, Object> properties, org.neo4j.graphdb.Label... labels) throws PropertyValueException
		 public override NodeRepresentation CreateNode( IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					NodeRepresentation nodeRepresentation = base.CreateNode( properties, labels );
					transaction.Success();
					return nodeRepresentation;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.NodeRepresentation getNode(long nodeId) throws NodeNotFoundException
		 public override NodeRepresentation GetNode( long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					NodeRepresentation node = base.GetNode( nodeId );
					transaction.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteNode(long nodeId) throws NodeNotFoundException, org.neo4j.graphdb.ConstraintViolationException
		 public override void DeleteNode( long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.DeleteNode( nodeId );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setNodeProperty(long nodeId, String key, Object value) throws PropertyValueException, NodeNotFoundException
		 public override void SetNodeProperty( long nodeId, string key, object value )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.SetNodeProperty( nodeId, key, value );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeNodeProperty(long nodeId, String key) throws NodeNotFoundException, NoSuchPropertyException
		 public override void RemoveNodeProperty( long nodeId, string key )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveNodeProperty( nodeId, key );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setAllNodeProperties(long nodeId, java.util.Map<String, Object> properties) throws PropertyValueException, NodeNotFoundException
		 public override void SetAllNodeProperties( long nodeId, IDictionary<string, object> properties )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.SetAllNodeProperties( nodeId, properties );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeAllNodeProperties(long nodeId) throws NodeNotFoundException, PropertyValueException
		 public override void RemoveAllNodeProperties( long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveAllNodeProperties( nodeId );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addLabelToNode(long nodeId, java.util.Collection<String> labelNames) throws NodeNotFoundException, org.neo4j.server.rest.repr.BadInputException
		 public override void AddLabelToNode( long nodeId, ICollection<string> labelNames )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.AddLabelToNode( nodeId, labelNames );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeLabelFromNode(long nodeId, String labelName) throws NodeNotFoundException
		 public override void RemoveLabelFromNode( long nodeId, string labelName )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveLabelFromNode( nodeId, labelName );
					transaction.Success();
			  }
		 }

		 public override IndexRepresentation CreateNodeIndex( IDictionary<string, object> indexSpecification )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					IndexRepresentation indexRepresentation = base.CreateNodeIndex( indexSpecification );
					transaction.Success();
					return indexRepresentation;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.RelationshipRepresentation createRelationship(long startNodeId, long endNodeId, String type, java.util.Map<String, Object> properties) throws org.neo4j.server.rest.domain.StartNodeNotFoundException, org.neo4j.server.rest.domain.EndNodeNotFoundException, PropertyValueException
		 public override RelationshipRepresentation CreateRelationship( long startNodeId, long endNodeId, string type, IDictionary<string, object> properties )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					RelationshipRepresentation relationshipRepresentation = base.CreateRelationship( startNodeId, endNodeId, type, properties );
					transaction.Success();
					return relationshipRepresentation;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.RelationshipRepresentation getRelationship(long relationshipId) throws RelationshipNotFoundException
		 public override RelationshipRepresentation GetRelationship( long relationshipId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					RelationshipRepresentation relationship = base.GetRelationship( relationshipId );
					transaction.Success();
					return relationship;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteRelationship(long relationshipId) throws RelationshipNotFoundException
		 public override void DeleteRelationship( long relationshipId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.DeleteRelationship( relationshipId );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.ListRepresentation getNodeRelationships(long nodeId, RelationshipDirection direction, java.util.Collection<String> types) throws NodeNotFoundException
		 public override ListRepresentation GetNodeRelationships( long nodeId, RelationshipDirection direction, ICollection<string> types )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					ListRepresentation nodeRelationships = base.GetNodeRelationships( nodeId, direction, types );
					transaction.Success();
					return nodeRelationships;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setAllRelationshipProperties(long relationshipId, java.util.Map<String, Object> properties) throws PropertyValueException, RelationshipNotFoundException
		 public override void SetAllRelationshipProperties( long relationshipId, IDictionary<string, object> properties )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.SetAllRelationshipProperties( relationshipId, properties );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setRelationshipProperty(long relationshipId, String key, Object value) throws PropertyValueException, RelationshipNotFoundException
		 public override void SetRelationshipProperty( long relationshipId, string key, object value )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.SetRelationshipProperty( relationshipId, key, value );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeAllRelationshipProperties(long relationshipId) throws RelationshipNotFoundException, PropertyValueException
		 public override void RemoveAllRelationshipProperties( long relationshipId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveAllRelationshipProperties( relationshipId );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeRelationshipProperty(long relationshipId, String key) throws RelationshipNotFoundException, NoSuchPropertyException
		 public override void RemoveRelationshipProperty( long relationshipId, string key )
		 {

			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveRelationshipProperty( relationshipId, key );
					transaction.Success();
			  }
		 }

		 public override IndexedEntityRepresentation AddToNodeIndex( string indexName, string key, string value, long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					IndexedEntityRepresentation indexedEntityRepresentation = base.AddToNodeIndex( indexName, key, value, nodeId );
					transaction.Success();
					return indexedEntityRepresentation;
			  }
		 }

		 public override void RemoveFromNodeIndex( string indexName, string key, string value, long id )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveFromNodeIndex( indexName, key, value, id );
					transaction.Success();
			  }
		 }

		 public override void RemoveFromNodeIndexNoValue( string indexName, string key, long id )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveFromNodeIndexNoValue( indexName, key, id );
					transaction.Success();
			  }
		 }

		 public override void RemoveFromNodeIndexNoKeyValue( string indexName, long id )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					base.RemoveFromNodeIndexNoKeyValue( indexName, id );
					transaction.Success();
			  }
		 }

		 public override PathRepresentation FindSinglePath( long startId, long endId, IDictionary<string, object> map )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					PathRepresentation singlePath = base.FindSinglePath( startId, endId, map );
					transaction.Success();
					return singlePath;
			  }
		 }

		 public override ListRepresentation GetNodesWithLabel( string labelName, IDictionary<string, object> properties )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					ListRepresentation nodesWithLabel = base.GetNodesWithLabel( labelName, properties );
					transaction.Success();
					return nodesWithLabel;
			  }
		 }

		 public override IndexDefinitionRepresentation CreateSchemaIndex( string labelName, IEnumerable<string> propertyKey )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					IndexDefinitionRepresentation indexDefinitionRepresentation = base.CreateSchemaIndex( labelName, propertyKey );
					transaction.Success();
					return indexDefinitionRepresentation;
			  }
		 }

		 public override bool DropSchemaIndex( string labelName, string propertyKey )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					bool result = base.DropSchemaIndex( labelName, propertyKey );
					transaction.Success();
					return result;
			  }
		 }

		 public override ConstraintDefinitionRepresentation CreatePropertyUniquenessConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					ConstraintDefinitionRepresentation constraintDefinitionRepresentation = base.CreatePropertyUniquenessConstraint( labelName, propertyKeys );
					transaction.Success();
					return constraintDefinitionRepresentation;
			  }
		 }

		 public override bool DropPropertyUniquenessConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					bool result = base.DropPropertyUniquenessConstraint( labelName, propertyKeys );
					transaction.Success();
					return result;
			  }
		 }

		 public override bool DropNodePropertyExistenceConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					bool result = base.DropNodePropertyExistenceConstraint( labelName, propertyKeys );
					transaction.Success();
					return result;
			  }
		 }

		 public override bool DropRelationshipPropertyExistenceConstraint( string typeName, IEnumerable<string> propertyKeys )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					bool result = base.DropRelationshipPropertyExistenceConstraint( typeName, propertyKeys );
					transaction.Success();
					return result;
			  }
		 }
	}

}