using System;
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
namespace Neo4Net.Server.rest.domain
{

	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.index;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using RelationshipIndex = Neo4Net.GraphDb.index.RelationshipIndex;
	using ConstraintCreator = Neo4Net.GraphDb.Schema.ConstraintCreator;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Neo4Net.Helpers.Collections;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using Database = Neo4Net.Server.database.Database;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;

	public class GraphDbHelper
	{
		 private readonly Database _database;

		 public GraphDbHelper( Database database )
		 {
			  this._database = database;
		 }

		 public virtual int NumberOfNodes
		 {
			 get
			 {
				  Kernel kernel = _database.Graph.DependencyResolver.resolveDependency( typeof( Kernel ) );
				  try
				  {
						  using ( Neo4Net.Kernel.Api.Internal.Transaction tx = kernel.BeginTransaction( @implicit, AnonymousContext.read() ) )
						  {
							return Math.toIntExact( tx.DataRead().nodesGetCount() );
						  }
				  }
				  catch ( TransactionFailureException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 public virtual int NumberOfRelationships
		 {
			 get
			 {
				  Kernel kernel = _database.Graph.DependencyResolver.resolveDependency( typeof( Kernel ) );
				  try
				  {
						  using ( Neo4Net.Kernel.Api.Internal.Transaction tx = kernel.BeginTransaction( @implicit, AnonymousContext.read() ) )
						  {
							return Math.toIntExact( tx.DataRead().relationshipsGetCount() );
						  }
				  }
				  catch ( TransactionFailureException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 public virtual IDictionary<string, object> GetNodeProperties( long nodeId )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.read() ) )
			  {
					Node node = _database.Graph.getNodeById( nodeId );
					IDictionary<string, object> allProperties = node.AllProperties;
					tx.Success();
					return allProperties;
			  }
		 }

		 public virtual void SetNodeProperties( long nodeId, IDictionary<string, object> properties )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.writeToken() ) )
			  {
					Node node = _database.Graph.getNodeById( nodeId );
					foreach ( KeyValuePair<string, object> propertyEntry in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( propertyEntry.Key, propertyEntry.Value );
					}
					tx.Success();
			  }
		 }

		 public virtual long CreateNode( params Label[] labels )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.writeToken() ) )
			  {
					Node node = _database.Graph.createNode( labels );
					tx.Success();
					return node.Id;
			  }
		 }

		 public virtual long CreateNode( IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.writeToken() ) )
			  {
					Node node = _database.Graph.createNode( labels );
					foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( entry.Key, entry.Value );
					}
					tx.Success();
					return node.Id;
			  }
		 }

		 public virtual void DeleteNode( long id )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.write() ) )
			  {
					Node node = _database.Graph.getNodeById( id );
					node.Delete();
					tx.Success();
			  }
		 }

		 public virtual long CreateRelationship( string type, long startNodeId, long endNodeId )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.writeToken() ) )
			  {
					Node startNode = _database.Graph.getNodeById( startNodeId );
					Node endNode = _database.Graph.getNodeById( endNodeId );
					Relationship relationship = startNode.CreateRelationshipTo( endNode, RelationshipType.withName( type ) );
					tx.Success();
					return relationship.Id;
			  }
		 }

		 public virtual long CreateRelationship( string type )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.writeToken() ) )
			  {
					Node startNode = _database.Graph.createNode();
					Node endNode = _database.Graph.createNode();
					Relationship relationship = startNode.CreateRelationshipTo( endNode, RelationshipType.withName( type ) );
					tx.Success();
					return relationship.Id;
			  }
		 }

		 public virtual void SetRelationshipProperties( long relationshipId, IDictionary<string, object> properties )

		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.writeToken() ) )
			  {
					Relationship relationship = _database.Graph.getRelationshipById( relationshipId );
					foreach ( KeyValuePair<string, object> propertyEntry in properties.SetOfKeyValuePairs() )
					{
						 relationship.SetProperty( propertyEntry.Key, propertyEntry.Value );
					}
					tx.Success();
			  }
		 }

		 public virtual IDictionary<string, object> GetRelationshipProperties( long relationshipId )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.read() ) )
			  {
					Relationship relationship = _database.Graph.getRelationshipById( relationshipId );
					IDictionary<string, object> allProperties = relationship.AllProperties;
					tx.Success();
					return allProperties;
			  }
		 }

		 public virtual Relationship GetRelationship( long relationshipId )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.read() ) )
			  {
					Relationship relationship = _database.Graph.getRelationshipById( relationshipId );
					tx.Success();
					return relationship;
			  }
		 }

		 public virtual void AddNodeToIndex( string indexName, string key, object value, long id )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					_database.Graph.index().forNodes(indexName).add(_database.Graph.getNodeById(id), key, value);
					tx.Success();
			  }
		 }

		 public virtual ICollection<long> QueryIndexedNodes( string indexName, string key, object value )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.write() ) )
			  {
					ICollection<long> result = new List<long>();
					foreach ( Node node in _database.Graph.index().forNodes(indexName).query(key, value) )
					{
						 result.Add( node.Id );
					}
					tx.Success();
					return result;
			  }
		 }

		 public virtual ICollection<long> GetIndexedNodes( string indexName, string key, object value )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.write() ) )
			  {
					ICollection<long> result = new List<long>();
					foreach ( Node node in _database.Graph.index().forNodes(indexName).get(key, value) )
					{
						 result.Add( node.Id );
					}
					tx.Success();
					return result;
			  }
		 }

		 public virtual ICollection<long> GetIndexedRelationships( string indexName, string key, object value )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.write() ) )
			  {
					ICollection<long> result = new List<long>();
					foreach ( Relationship relationship in _database.Graph.index().forRelationships(indexName).get(key, value) )
					{
						 result.Add( relationship.Id );
					}
					tx.Success();
					return result;
			  }
		 }

		 public virtual void AddRelationshipToIndex( string indexName, string key, string value, long relationshipId )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					Index<Relationship> index = _database.Graph.index().forRelationships(indexName);
					index.Add( _database.Graph.getRelationshipById( relationshipId ), key, value );
					tx.Success();
			  }

		 }

		 public virtual string[] NodeIndexes
		 {
			 get
			 {
				  using ( Transaction transaction = _database.Graph.BeginTransaction( @implicit, AnonymousContext.read() ) )
				  {
						return _database.Graph.index().nodeIndexNames();
				  }
			 }
		 }

		 public virtual Index<Node> CreateNodeFullTextIndex( string named )
		 {
			  using ( Transaction transaction = _database.Graph.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					Index<Node> index = _database.Graph.index().forNodes(named, MapUtil.stringMap(Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "fulltext"));
					transaction.Success();
					return index;
			  }
		 }

		 public virtual Index<Node> CreateNodeIndex( string named )
		 {
			  using ( Transaction transaction = _database.Graph.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					Index<Node> nodeIndex = _database.Graph.index().forNodes(named);
					transaction.Success();
					return nodeIndex;
			  }
		 }

		 public virtual string[] RelationshipIndexes
		 {
			 get
			 {
				  using ( Transaction transaction = _database.Graph.BeginTransaction( @implicit, AnonymousContext.read() ) )
				  {
						return _database.Graph.index().relationshipIndexNames();
				  }
			 }
		 }

		 public virtual long FirstNode
		 {
			 get
			 {
				  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.write() ) )
				  {
						try
						{
							 Node referenceNode = _database.Graph.getNodeById( 0L );
   
							 tx.Success();
							 return referenceNode.Id;
						}
						catch ( NotFoundException )
						{
							 Node newNode = _database.Graph.createNode();
							 tx.Success();
							 return newNode.Id;
						}
				  }
			 }
		 }

		 public virtual Index<Relationship> CreateRelationshipIndex( string named )
		 {
			  using ( Transaction transaction = _database.Graph.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					RelationshipIndex relationshipIndex = _database.Graph.index().forRelationships(named);
					transaction.Success();
					return relationshipIndex;
			  }
		 }

		 public virtual IEnumerable<string> GetNodeLabels( long node )
		 {
			  return new IterableWrapperAnonymousInnerClass( this );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<string, Label>
		 {
			 private readonly GraphDbHelper _outerInstance;

			 public IterableWrapperAnonymousInnerClass( GraphDbHelper outerInstance ) : base( outerInstance.database.Graph.getNodeById( node ).Labels )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override string underlyingObjectToObject( Label @object )
			 {
				  return @object.Name();
			 }
		 }

		 public virtual void AddLabelToNode( long node, string labelName )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.writeToken() ) )
			  {
					_database.Graph.getNodeById( node ).addLabel( label( labelName ) );
					tx.Success();
			  }
		 }

		 public virtual IEnumerable<IndexDefinition> GetSchemaIndexes( string labelName )
		 {
			  return _database.Graph.schema().getIndexes(label(labelName));
		 }

		 public virtual IndexDefinition CreateSchemaIndex( string labelName, string propertyKey )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					IndexDefinition index = _database.Graph.schema().indexFor(label(labelName)).on(propertyKey).create();
					tx.Success();
					return index;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.Neo4Net.GraphDb.Schema.ConstraintDefinition> getPropertyUniquenessConstraints(String labelName, final String propertyKey)
		 public virtual IEnumerable<ConstraintDefinition> GetPropertyUniquenessConstraints( string labelName, string propertyKey )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AnonymousContext.read() ) )
			  {
					IEnumerable<ConstraintDefinition> definitions = Iterables.filter(item =>
					{
					 if ( item.isConstraintType( ConstraintType.UNIQUENESS ) )
					 {
						  IEnumerable<string> keys = item.PropertyKeys;
						  return single( keys ).Equals( propertyKey );
					 }
					 else
					 {
						  return false;
					 }

					}, _database.Graph.schema().getConstraints(label(labelName)));
					tx.Success();
					return definitions;
			  }
		 }

		 public virtual ConstraintDefinition CreatePropertyUniquenessConstraint( string labelName, IList<string> propertyKeys )
		 {
			  using ( Transaction tx = _database.Graph.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					ConstraintCreator creator = _database.Graph.schema().constraintFor(label(labelName));
					foreach ( string propertyKey in propertyKeys )
					{
						 creator = creator.AssertPropertyIsUnique( propertyKey );
					}
					ConstraintDefinition result = creator.Create();
					tx.Success();
					return result;
			  }
		 }

		 public virtual long GetLabelCount( long nodeId )
		 {
			  using ( Transaction transaction = _database.Graph.BeginTransaction( @implicit, AnonymousContext.read() ) )
			  {
					return count( _database.Graph.getNodeById( nodeId ).Labels );
			  }
		 }
	}

}