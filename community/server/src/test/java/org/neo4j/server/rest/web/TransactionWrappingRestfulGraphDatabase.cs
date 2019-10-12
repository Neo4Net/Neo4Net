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
namespace Org.Neo4j.Server.rest.web
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;

	/// <summary>
	/// A convenient class for testing RestfulGraphDatabase: just wraps every "web call" in a transaction like the
	/// TransactionFilter would have if deployed in a jax-rs container
	/// </summary>
	public class TransactionWrappingRestfulGraphDatabase : RestfulGraphDatabase
	{
		 private readonly GraphDatabaseService _graph;
		 private readonly RestfulGraphDatabase _restfulGraphDatabase;

		 public TransactionWrappingRestfulGraphDatabase( GraphDatabaseService graph, RestfulGraphDatabase restfulGraphDatabase ) : base( null, null, null, null )
		 {

			  this._graph = graph;
			  this._restfulGraphDatabase = restfulGraphDatabase;
		 }

		 public override Response AddToNodeIndex( string indexName, string unique, string uniqueness, string postBody )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.addToNodeIndex( indexName, unique, uniqueness, postBody );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response CreateRelationship( long startNodeId, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.createRelationship( startNodeId, body );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response DeleteNodeIndex( string indexName )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteNodeIndex( indexName );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response GetNodeRelationships( long nodeId, DatabaseActions.RelationshipDirection direction, AmpersandSeparatedCollection types )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.getNodeRelationships( nodeId, direction, types );
					return response;
			  }
		 }

		 public override Response DeleteAllNodeProperties( long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteAllNodeProperties( nodeId );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response GetAllNodeProperties( long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.getAllNodeProperties( nodeId );
					return response;
			  }
		 }

		 public override Response CreateNode( string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.createNode( body );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response DeleteAllRelationshipProperties( long relationshipId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteAllRelationshipProperties( relationshipId );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response AddToRelationshipIndex( string indexName, string unique, string uniqueness, string postBody )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.addToRelationshipIndex( indexName, unique, uniqueness, postBody );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response GetIndexedNodes( string indexName, string key, string value )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.getIndexedNodes( indexName, key, value );
					return response;
			  }
		 }

		 public override Response RelationshipIndexRoot
		 {
			 get
			 {
				  using ( Transaction transaction = _graph.beginTx() )
				  {
						Response response = _restfulGraphDatabase.RelationshipIndexRoot;
						return response;
				  }
			 }
		 }

		 public override Response SetRelationshipProperty( long relationshipId, string key, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.setRelationshipProperty( relationshipId, key, body );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response GetSchemaConstraintsForLabelAndPropertyUniqueness( string labelName, AmpersandSeparatedCollection propertyKeys )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response GetSchemaConstraintsForLabelAndUniqueness( string labelName )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response GetSchemaConstraintsForLabel( string labelName )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response SchemaConstraints
		 {
			 get
			 {
				  throw new System.NotSupportedException( "TODO" );
			 }
		 }

		 public override Response DropPropertyUniquenessConstraint( string labelName, AmpersandSeparatedCollection properties )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response CreatePropertyUniquenessConstraint( string labelName, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.createPropertyUniquenessConstraint( labelName, body );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response GetSchemaIndexesForLabel( string labelName )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response DropSchemaIndex( string labelName, AmpersandSeparatedCollection properties )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response CreateSchemaIndex( string labelName, string body )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response AllPaths( long startNode, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.allPaths( startNode, body );
			  }
		 }

		 public override Response SinglePath( long startNode, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.singlePath( startNode, body );
			  }
		 }

		 public override Response DeleteFromRelationshipIndex( string indexName, long id )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response DeleteFromRelationshipIndexNoValue( string indexName, string key, long id )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response DeleteFromRelationshipIndex( string indexName, string key, string value, long id )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteFromRelationshipIndex( indexName, key, value, id );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response DeleteFromNodeIndexNoKeyValue( string indexName, long id )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response DeleteFromNodeIndexNoValue( string indexName, string key, long id )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response DeleteFromNodeIndex( string indexName, string key, string value, long id )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteFromNodeIndex( indexName, key, value, id );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response GetIndexedRelationshipsByQuery( string indexName, string key, string query, string order )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getIndexedRelationshipsByQuery( indexName, key, query, order );
			  }
		 }

		 public override Response GetIndexedRelationshipsByQuery( string indexName, string query, string order )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getIndexedRelationshipsByQuery( indexName, query, order );
			  }
		 }

		 public override Response StopAutoIndexingProperty( string type, string property )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.stopAutoIndexingProperty( type, property );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response StartAutoIndexingProperty( string type, string property )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.startAutoIndexingProperty( type, property );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response GetAutoIndexedProperties( string type )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getAutoIndexedProperties( type );
			  }
		 }

		 public override Response SetAutoIndexerEnabled( string type, string enable )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.setAutoIndexerEnabled( type, enable );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response IsAutoIndexerEnabled( string type )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.isAutoIndexerEnabled( type );
			  }
		 }

		 public override Response GetIndexedRelationships( string indexName, string key, string value )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getIndexedRelationships( indexName, key, value );
			  }
		 }

		 public override Response GetIndexedNodesByQuery( string indexName, string key, string query, string order )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getIndexedNodesByQuery( indexName, key, query, order );
			  }
		 }

		 public override Response GetAutoIndexedNodes( string type, string key, string value )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response GetRelationshipFromIndexUri( string indexName, string key, string value, long id )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getRelationshipFromIndexUri( indexName, key, value, id );
			  }
		 }

		 public override Response GetNodeFromIndexUri( string indexName, string key, string value, long id )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getNodeFromIndexUri( indexName, key, value, id );
			  }
		 }

		 public override Response DeleteRelationshipIndex( string indexName )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteRelationshipIndex( indexName );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response GetAutoIndexedNodesByQuery( string type, string query )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response GetIndexedNodesByQuery( string indexName, string query, string order )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getIndexedNodesByQuery( indexName, query, order );
			  }
		 }

		 public override Response JsonCreateRelationshipIndex( string json )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.jsonCreateRelationshipIndex( json );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response JsonCreateNodeIndex( string json )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.jsonCreateNodeIndex( json );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response NodeIndexRoot
		 {
			 get
			 {
				  using ( Transaction transaction = _graph.beginTx() )
				  {
						return _restfulGraphDatabase.NodeIndexRoot;
				  }
			 }
		 }

		 public override Response DeleteRelationshipProperty( long relationshipId, string key )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteRelationshipProperty( relationshipId, key );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response SetAllRelationshipProperties( long relationshipId, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.setAllRelationshipProperties( relationshipId, body );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response GetRelationshipProperty( long relationshipId, string key )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getRelationshipProperty( relationshipId, key );
			  }
		 }

		 public override Response GetAllRelationshipProperties( long relationshipId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getAllRelationshipProperties( relationshipId );
			  }
		 }

		 public override Response GetNodeRelationships( long nodeId, DatabaseActions.RelationshipDirection direction )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response DeleteRelationship( long relationshipId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteRelationship( relationshipId );
					transaction.Success();
					return response;
			  }
		 }

		 public override Response GetRelationship( long relationshipId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getRelationship( relationshipId );
			  }
		 }

		 public override Response GetAllLabels( bool inUse )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getAllLabels( inUse );
			  }
		 }

		 public override Response GetNodesWithLabelAndProperty( string labelName, UriInfo uriInfo )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response GetNodeLabels( long nodeId )
		 {
			  using ( Transaction ignored = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getNodeLabels( nodeId );
			  }
		 }

		 public override Response RemoveNodeLabel( long nodeId, string labelName )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response SetNodeLabels( long nodeId, string body )
		 {
			  throw new System.NotSupportedException( "TODO" );
		 }

		 public override Response AddNodeLabel( long nodeId, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.addNodeLabel( nodeId, body );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response DeleteNodeProperty( long nodeId, string key )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteNodeProperty( nodeId, key );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response GetNodeProperty( long nodeId, string key )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getNodeProperty( nodeId, key );
			  }
		 }

		 public override Response SetNodeProperty( long nodeId, string key, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.setNodeProperty( nodeId, key, body );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response SetAllNodeProperties( long nodeId, string body )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.setAllNodeProperties( nodeId, body );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response DeleteNode( long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					Response response = _restfulGraphDatabase.deleteNode( nodeId );
					if ( response.Status < 300 )
					{
						 transaction.Success();
					}
					return response;
			  }
		 }

		 public override Response GetNode( long nodeId )
		 {
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					return _restfulGraphDatabase.getNode( nodeId );
			  }
		 }

		 public override Response Root
		 {
			 get
			 {
				  using ( Transaction transaction = _graph.beginTx() )
				  {
						return _restfulGraphDatabase.Root;
				  }
			 }
		 }
	}

}