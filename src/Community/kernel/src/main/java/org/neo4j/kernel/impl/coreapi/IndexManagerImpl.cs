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
namespace Neo4Net.Kernel.impl.coreapi
{

	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using RelationshipAutoIndexer = Neo4Net.Graphdb.index.RelationshipAutoIndexer;
	using RelationshipIndex = Neo4Net.Graphdb.index.RelationshipIndex;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using InvalidTransactionTypeKernelException = Neo4Net.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using ExplicitIndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using InternalAutoIndexing = Neo4Net.Kernel.Impl.Api.explicitindex.InternalAutoIndexing;

	public class IndexManagerImpl : IndexManager
	{
		 private readonly System.Func<Transaction> _transactionBridge;
		 private readonly IndexProvider _provider;
		 private readonly AutoIndexer<Node> _nodeAutoIndexer;
		 private readonly RelationshipAutoIndexer _relAutoIndexer;

		 public IndexManagerImpl( System.Func<Transaction> bridge, IndexProvider provider, AutoIndexer<Node> nodeAutoIndexer, RelationshipAutoIndexer relAutoIndexer )
		 {
			  this._transactionBridge = bridge;
			  this._provider = provider;
			  this._nodeAutoIndexer = nodeAutoIndexer;
			  this._relAutoIndexer = relAutoIndexer;
		 }

		 public override bool ExistsForNodes( string indexName )
		 {
			  try
			  {
					_transactionBridge.get().indexRead().nodeExplicitIndexGetConfiguration(indexName);
					return true;
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					return false;
			  }
		 }

		 public override Index<Node> ForNodes( string indexName )
		 {
			  return ForNodes( indexName, null );
		 }

		 public override Index<Node> ForNodes( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  Index<Node> toReturn = _provider.getOrCreateNodeIndex( indexName, customConfiguration );

			  // TODO move this into kernel layer
			  if ( InternalAutoIndexing.NODE_AUTO_INDEX.Equals( indexName ) )
			  {
					return new ReadOnlyIndexFacade<Node>( toReturn );
			  }
			  return toReturn;
		 }

		 public override string[] NodeIndexNames()
		 {

			  return _transactionBridge.get().indexRead().nodeExplicitIndexesGetAll();
		 }

		 public override bool ExistsForRelationships( string indexName )
		 {
			  try
			  {
					_transactionBridge.get().indexRead().relationshipExplicitIndexGetConfiguration(indexName);
					return true;
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					return false;
			  }
		 }

		 public override RelationshipIndex ForRelationships( string indexName )
		 {
			  return ForRelationships( indexName, null );
		 }

		 public override RelationshipIndex ForRelationships( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  RelationshipIndex toReturn = _provider.getOrCreateRelationshipIndex( indexName, customConfiguration );

			  // TODO move this into kernel layer
			  if ( InternalAutoIndexing.RELATIONSHIP_AUTO_INDEX.Equals( indexName ) )
			  {
					return new RelationshipReadOnlyIndexFacade( toReturn );
			  }
			  return toReturn;
		 }

		 public override string[] RelationshipIndexNames()
		 {

			  return _transactionBridge.get().indexRead().relationshipExplicitIndexesGetAll();
		 }

		 public override IDictionary<string, string> GetConfiguration<T1>( Index<T1> index ) where T1 : Neo4Net.Graphdb.PropertyContainer
		 {
			  try
			  {
					Transaction transaction = _transactionBridge.get();
					if ( index.EntityType.Equals( typeof( Node ) ) )
					{
						 return transaction.IndexRead().nodeExplicitIndexGetConfiguration(index.Name);
					}
					if ( index.EntityType.Equals( typeof( Relationship ) ) )
					{
						 return transaction.IndexRead().relationshipExplicitIndexGetConfiguration(index.Name);
					}
					throw new System.ArgumentException( "Unknown entity type " + index.EntityType.SimpleName );
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( "No node index '" + index.Name + "' found" );
			  }
		 }

		 public override string SetConfiguration<T1>( Index<T1> index, string key, string value ) where T1 : Neo4Net.Graphdb.PropertyContainer
		 {
			  // Configuration changes should be done transactionally. However this
			  // has always been done non-transactionally, so it's not a regression.
			  try
			  {
					Transaction transaction = _transactionBridge.get();
					if ( index.EntityType.Equals( typeof( Node ) ) )
					{
						 return transaction.IndexWrite().nodeExplicitIndexSetConfiguration(index.Name, key, value);
					}
					if ( index.EntityType.Equals( typeof( Relationship ) ) )
					{
						 return transaction.IndexWrite().relationshipExplicitIndexSetConfiguration(index.Name, key, value);
					}
					throw new System.ArgumentException( "Unknown entity type " + index.EntityType.SimpleName );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException e )
			  {
					throw new NotFoundException( e );
			  }
		 }

		 public override string RemoveConfiguration<T1>( Index<T1> index, string key ) where T1 : Neo4Net.Graphdb.PropertyContainer
		 {
			  // Configuration changes should be done transactionally. However this
			  // has always been done non-transactionally, so it's not a regression.

			  try
			  {
					Transaction transaction = _transactionBridge.get();
					if ( index.EntityType.Equals( typeof( Node ) ) )
					{
						 return transaction.IndexWrite().nodeExplicitIndexRemoveConfiguration(index.Name, key);
					}
					if ( index.EntityType.Equals( typeof( Relationship ) ) )
					{
						 return transaction.IndexWrite().relationshipExplicitIndexRemoveConfiguration(index.Name, key);
					}
					throw new System.ArgumentException( "Unknown entity type " + index.EntityType.SimpleName );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException e )
			  {
					throw new NotFoundException( e );
			  }
		 }

		 public virtual AutoIndexer<Node> NodeAutoIndexer
		 {
			 get
			 {
				  return _nodeAutoIndexer;
			 }
		 }

		 public virtual RelationshipAutoIndexer RelationshipAutoIndexer
		 {
			 get
			 {
				  return _relAutoIndexer;
			 }
		 }
	}

}