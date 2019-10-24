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
namespace Neo4Net.Kernel.impl.factory
{

	using Suppliers = Neo4Net.Functions.Suppliers;
	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using MultipleFoundException = Neo4Net.GraphDb.MultipleFoundException;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Result = Neo4Net.GraphDb.Result;
	using StringSearchMode = Neo4Net.GraphDb.StringSearchMode;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using KernelEventHandler = Neo4Net.GraphDb.Events.KernelEventHandler;
	using Neo4Net.GraphDb.Events;
	using IndexManager = Neo4Net.GraphDb.Index.IndexManager;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using URLAccessValidationError = Neo4Net.GraphDb.security.URLAccessValidationError;
	using BidirectionalTraversalDescription = Neo4Net.GraphDb.Traversal.BidirectionalTraversalDescription;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using Neo4Net.Collections.Helpers;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using NodeIndexCursor = Neo4Net.Kernel.Api.Internal.NodeIndexCursor;
	using NodeLabelIndexCursor = Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor;
	using NodeValueIndexCursor = Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using Write = Neo4Net.Kernel.Api.Internal.Write;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using InvalidTransactionTypeKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
	using SchemaKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using IOUtils = Neo4Net.Io.IOUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.Impl.Api;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using GraphPropertiesProxy = Neo4Net.Kernel.impl.core.GraphPropertiesProxy;
	using NodeProxy = Neo4Net.Kernel.impl.core.NodeProxy;
	using RelationshipProxy = Neo4Net.Kernel.impl.core.RelationshipProxy;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using TokenNotFoundException = Neo4Net.Kernel.impl.core.TokenNotFoundException;
	using Neo4Net.Kernel.impl.coreapi;
	using IndexManagerImpl = Neo4Net.Kernel.impl.coreapi.IndexManagerImpl;
	using IndexProviderImpl = Neo4Net.Kernel.impl.coreapi.IndexProviderImpl;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using PlaceboTransaction = Neo4Net.Kernel.impl.coreapi.PlaceboTransaction;
	using IPropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using Neo4Net.Kernel.impl.coreapi;
	using ReadOnlyRelationshipIndexFacade = Neo4Net.Kernel.impl.coreapi.ReadOnlyRelationshipIndexFacade;
	using RelationshipAutoIndexerFacade = Neo4Net.Kernel.impl.coreapi.RelationshipAutoIndexerFacade;
	using TopLevelTransaction = Neo4Net.Kernel.impl.coreapi.TopLevelTransaction;
	using SchemaImpl = Neo4Net.Kernel.impl.coreapi.schema.SchemaImpl;
	using Neo4NetTransactionalContextFactory = Neo4Net.Kernel.impl.query.Neo4NetTransactionalContextFactory;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Neo4Net.Kernel.impl.query.TransactionalContextFactory;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using BidirectionalTraversalDescriptionImpl = Neo4Net.Kernel.impl.traversal.BidirectionalTraversalDescriptionImpl;
	using MonoDirectionalTraversalDescription = Neo4Net.Kernel.impl.traversal.MonoDirectionalTraversalDescription;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.transaction_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.emptyResourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.explicitindex.InternalAutoIndexing.NODE_AUTO_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.explicitindex.InternalAutoIndexing.RELATIONSHIP_AUTO_INDEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.utf8Value;

	/// <summary>
	/// Implementation of the IGraphDatabaseService/GraphDatabaseService interfaces - the "Core API". Given an <seealso cref="SPI"/>
	/// implementation, this provides users with
	/// a clean facade to interact with the database.
	/// </summary>
	public class GraphDatabaseFacade : GraphDatabaseAPI, EmbeddedProxySPI
	{
		 private static readonly IPropertyContainerLocker _locker = new IPropertyContainerLocker();

		 private Schema _schema;
		 private System.Func<IndexManager> _indexManager;
		 private ThreadToStatementContextBridge _statementContext;
		 private SPI _spi;
		 private TransactionalContextFactory _contextFactory;
		 private Config _config;
		 private TokenHolders _tokenHolders;

		 /// <summary>
		 /// This is what you need to implement to get your very own <seealso cref="GraphDatabaseFacade"/>. This SPI exists as a thin
		 /// layer to make it easy to provide
		 /// alternate <seealso cref="org.Neo4Net.graphdb.GraphDatabaseService"/> instances without having to re-implement this whole API
		 /// implementation.
		 /// </summary>
		 public interface ISPI
		 {
			  /// <summary>
			  /// Check if database is available, waiting up to {@code timeout} if it isn't. If the timeout expires before
			  /// database available, this returns false
			  /// </summary>
			  bool DatabaseIsAvailable( long timeout );

			  DependencyResolver Resolver();

			  StoreId StoreId();

			  DatabaseLayout DatabaseLayout();

			  /// <summary>
			  /// Eg. Neo4Net Enterprise HA, Neo4Net Community Standalone.. </summary>
			  string Name();

			  void Shutdown();

			  /// <summary>
			  /// Begin a new kernel transaction with specified timeout in milliseconds.
			  /// </summary>
			  /// <exception cref="org.Neo4Net.graphdb.TransactionFailureException"> if unable to begin, or a transaction already exists. </exception>
			  /// <seealso cref= GraphDatabaseAPI#BeginTransaction(KernelTransaction.Type, LoginContext) </seealso>
			  KernelTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout );

			  /// <summary>
			  /// Execute a cypher statement </summary>
			  Result ExecuteQuery( string query, MapValue parameters, TransactionalContext context );

			  AutoIndexing AutoIndexing();

			  void RegisterKernelEventHandler( KernelEventHandler handler );

			  void UnregisterKernelEventHandler( KernelEventHandler handler );

			  void registerTransactionEventHandler<T>( TransactionEventHandler<T> handler );

			  void unregisterTransactionEventHandler<T>( TransactionEventHandler<T> handler );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.net.URL validateURLAccess(java.net.URL url) throws org.Neo4Net.graphdb.security.URLAccessValidationError;
			  URL ValidateURLAccess( URL url );

			  GraphDatabaseQueryService QueryService();

			  Kernel Kernel();
		 }

		 public GraphDatabaseFacade()
		 {
		 }

		 /// <summary>
		 /// Create a new Core API facade, backed by the given SPI and using pre-resolved dependencies
		 /// </summary>
		 public virtual void Init( SPI spi, ThreadToStatementContextBridge txBridge, Config config, TokenHolders tokenHolders )
		 {
			  this._spi = spi;
			  this._config = config;
			  this._schema = new SchemaImpl( () => txBridge.GetKernelTransactionBoundToThisThread(true) );
			  this._statementContext = txBridge;
			  this._tokenHolders = tokenHolders;
			  this._indexManager = Suppliers.lazySingleton(() =>
			  {
				IndexProviderImpl idxProvider = new IndexProviderImpl( this, () => txBridge.GetKernelTransactionBoundToThisThread(true) );
				AutoIndexerFacade<Node> nodeAutoIndexer = new AutoIndexerFacade<Node>( () => new ReadOnlyIndexFacade<Neo4Net.GraphDb.Index.ReadableIndex<T>>(idxProvider.getOrCreateNodeIndex(NODE_AUTO_INDEX, null)), spi.AutoIndexing().nodes() );
				RelationshipAutoIndexerFacade relAutoIndexer = new RelationshipAutoIndexerFacade( () => new ReadOnlyRelationshipIndexFacade(idxProvider.getOrCreateRelationshipIndex(RELATIONSHIP_AUTO_INDEX, null)), spi.AutoIndexing().relationships() );

				return new IndexManagerImpl( () => txBridge.GetKernelTransactionBoundToThisThread(true), idxProvider, nodeAutoIndexer, relAutoIndexer );
			  });

			  this._contextFactory = Neo4NetTransactionalContextFactory.create( spi, txBridge, _locker );
		 }

		 public override Node CreateNode()
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						return NewNodeProxy( transaction.DataWrite().nodeCreate() );
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override long? CreateNodeId()
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						return transaction.DataWrite().nodeCreate();
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override Node CreateNode( params Label[] labels )
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						TokenWrite tokenWrite = transaction.TokenWrite();
						int[] labelIds = new int[labels.Length];
						string[] labelNames = new string[labels.Length];
						for ( int i = 0; i < labelNames.Length; i++ )
						{
							 labelNames[i] = labels[i].Name();
						}
						tokenWrite.LabelGetOrCreateForNames( labelNames, labelIds );
      
						Write write = transaction.DataWrite();
						long nodeId = write.NodeCreateWithLabels( labelIds );
						return NewNodeProxy( nodeId );
					  }
			  }
			  catch ( ConstraintValidationException e )
			  {
					throw new ConstraintViolationException( "Unable to add label.", e );
			  }
			  catch ( SchemaKernelException e )
			  {
					throw new System.ArgumentException( e );
			  }
			  catch ( KernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override Node GetNodeById( long id )
		 {
			  if ( id < 0 )
			  {
					throw new NotFoundException( format( "Node %d not found", id ), new IEntityNotFoundException( EntityType.NODE, id ) );
			  }

			  KernelTransaction ktx = _statementContext.getKernelTransactionBoundToThisThread( true );
			  AssertTransactionOpen( ktx );
			  using ( Statement ignore = ktx.AcquireStatement() )
			  {
					if ( !ktx.DataRead().nodeExists(id) )
					{
						 throw new NotFoundException( format( "Node %d not found", id ), new IEntityNotFoundException( EntityType.NODE, id ) );
					}
					return NewNodeProxy( id );
			  }
		 }

		 public override Relationship GetRelationshipById( long id )
		 {
			  if ( id < 0 )
			  {
					throw new NotFoundException( format( "Relationship %d not found", id ), new IEntityNotFoundException( EntityType.RELATIONSHIP, id ) );
			  }

			  KernelTransaction ktx = _statementContext.getKernelTransactionBoundToThisThread( true );
			  AssertTransactionOpen( ktx );
			  using ( Statement ignore = _statementContext.get() )
			  {
					if ( !ktx.DataRead().relationshipExists(id) )
					{
						 throw new NotFoundException( format( "Relationship %d not found", id ), new IEntityNotFoundException( EntityType.RELATIONSHIP, id ) );
					}
					return NewRelationshipProxy( id );
			  }
		 }

		 [Obsolete]
		 public override IndexManager Index()
		 {
			  return _indexManager.get();
		 }

		 public override Schema Schema()
		 {
			  AssertTransactionOpen();
			  return _schema;
		 }

		 public override bool IsAvailable( long timeoutMillis )
		 {
			  return _spi.databaseIsAvailable( timeoutMillis );
		 }

		 public override void Shutdown()
		 {
			  _spi.shutdown();
		 }

		 public override Transaction BeginTx()
		 {
			  return BeginTransaction( KernelTransaction.Type.@explicit, AUTH_DISABLED );
		 }

		 public override Transaction BeginTx( long timeout, TimeUnit unit )
		 {
			  return BeginTransaction( KernelTransaction.Type.@explicit, AUTH_DISABLED, timeout, unit );
		 }

		 public override InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext )
		 {
			  return BeginTransactionInternal( type, loginContext, _config.get( transaction_timeout ).toMillis() );
		 }

		 public override InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout, TimeUnit unit )
		 {
			  return BeginTransactionInternal( type, loginContext, unit.toMillis( timeout ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query )
		 {
			  return Execute( query, Collections.emptyMap() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query, long timeout, java.util.concurrent.TimeUnit unit) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query, long timeout, TimeUnit unit )
		 {
			  return Execute( query, Collections.emptyMap(), timeout, unit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query, java.util.Map<String,Object> parameters) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query, IDictionary<string, object> parameters )
		 {
			  // ensure we have a tx and create a context (the tx is gonna get closed by the Cypher result)
			  InternalTransaction transaction = BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED );

			  return Execute( transaction, query, ValueUtils.asParameterMapValue( parameters ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query, java.util.Map<String,Object> parameters, long timeout, java.util.concurrent.TimeUnit unit) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query, IDictionary<string, object> parameters, long timeout, TimeUnit unit )
		 {
			  InternalTransaction transaction = BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED, timeout, unit );
			  return Execute( transaction, query, ValueUtils.asParameterMapValue( parameters ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(org.Neo4Net.kernel.impl.coreapi.InternalTransaction transaction, String query, org.Neo4Net.values.virtual.MapValue parameters) throws org.Neo4Net.graphdb.QueryExecutionException
		 public virtual Result Execute( InternalTransaction transaction, string query, MapValue parameters )
		 {
			  TransactionalContext context = _contextFactory.newContext( ClientConnectionInfo.EMBEDDED_CONNECTION, transaction, query, parameters );
			  return _spi.executeQuery( query, parameters, context );
		 }

		 public virtual ResourceIterable<Node> AllNodes
		 {
			 get
			 {
				  KernelTransaction ktx = _statementContext.getKernelTransactionBoundToThisThread( true );
				  AssertTransactionOpen( ktx );
				  return () =>
				  {
					Statement statement = ktx.AcquireStatement();
					NodeCursor cursor = ktx.Cursors().allocateNodeCursor();
					ktx.DataRead().allNodesScan(cursor);
					return new PrefetchingResourceIteratorAnonymousInnerClass( this, statement, cursor );
				  };
			 }
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass : PrefetchingResourceIterator<Node>
		 {
			 private readonly GraphDatabaseFacade _outerInstance;

			 private Statement _statement;
			 private NodeCursor _cursor;

			 public PrefetchingResourceIteratorAnonymousInnerClass( GraphDatabaseFacade outerInstance, Statement statement, NodeCursor cursor )
			 {
				 this.outerInstance = outerInstance;
				 this._statement = statement;
				 this._cursor = cursor;
			 }

			 protected internal override Node fetchNextOrNull()
			 {
				  if ( _cursor.next() )
				  {
						return outerInstance.NewNodeProxy( _cursor.nodeReference() );
				  }
				  else
				  {
						close();
						return null;
				  }
			 }

			 public override void close()
			 {
				  _cursor.close();
				  _statement.close();
			 }
		 }

		 public virtual ResourceIterable<Relationship> AllRelationships
		 {
			 get
			 {
				  KernelTransaction ktx = _statementContext.getKernelTransactionBoundToThisThread( true );
				  AssertTransactionOpen( ktx );
				  return () =>
				  {
					Statement statement = ktx.AcquireStatement();
					RelationshipScanCursor cursor = ktx.Cursors().allocateRelationshipScanCursor();
					ktx.DataRead().allRelationshipsScan(cursor);
					return new PrefetchingResourceIteratorAnonymousInnerClass2( this, statement, cursor );
				  };
			 }
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass2 : PrefetchingResourceIterator<Relationship>
		 {
			 private readonly GraphDatabaseFacade _outerInstance;

			 private Statement _statement;
			 private RelationshipScanCursor _cursor;

			 public PrefetchingResourceIteratorAnonymousInnerClass2( GraphDatabaseFacade outerInstance, Statement statement, RelationshipScanCursor cursor )
			 {
				 this.outerInstance = outerInstance;
				 this._statement = statement;
				 this._cursor = cursor;
			 }

			 protected internal override Relationship fetchNextOrNull()
			 {
				  if ( _cursor.next() )
				  {
						return outerInstance.NewRelationshipProxy( _cursor.relationshipReference(), _cursor.sourceNodeReference(), _cursor.type(), _cursor.targetNodeReference() );
				  }
				  else
				  {
						close();
						return null;
				  }
			 }

			 public override void close()
			 {
				  _cursor.close();
				  _statement.close();
			 }
		 }

		 public virtual ResourceIterable<Label> AllLabelsInUse
		 {
			 get
			 {
				  return AllInUse( TokenAccess.LABELS );
			 }
		 }

		 public virtual ResourceIterable<RelationshipType> AllRelationshipTypesInUse
		 {
			 get
			 {
				  return AllInUse( TokenAccess.RELATIONSHIP_TYPES );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private <T> org.Neo4Net.graphdb.ResourceIterable<T> allInUse(final org.Neo4Net.kernel.impl.api.TokenAccess<T> tokens)
		 private ResourceIterable<T> AllInUse<T>( TokenAccess<T> tokens )
		 {
			  AssertTransactionOpen();
			  return () => tokens.InUse(_statementContext.getKernelTransactionBoundToThisThread(true));
		 }

		 public virtual ResourceIterable<Label> AllLabels
		 {
			 get
			 {
				  return All( TokenAccess.LABELS );
			 }
		 }

		 public virtual ResourceIterable<RelationshipType> AllRelationshipTypes
		 {
			 get
			 {
				  return All( TokenAccess.RELATIONSHIP_TYPES );
			 }
		 }

		 public virtual ResourceIterable<string> AllPropertyKeys
		 {
			 get
			 {
				  return All( TokenAccess.PROPERTY_KEYS );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private <T> org.Neo4Net.graphdb.ResourceIterable<T> all(final org.Neo4Net.kernel.impl.api.TokenAccess<T> tokens)
		 private ResourceIterable<T> All<T>( TokenAccess<T> tokens )
		 {
			  AssertTransactionOpen();
			  return () =>
			  {
				KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
				return tokens.All( transaction );
			  };
		 }

		 public override KernelEventHandler RegisterKernelEventHandler( KernelEventHandler handler )
		 {
			  _spi.registerKernelEventHandler( handler );
			  return handler;
		 }

		 public override TransactionEventHandler<T> RegisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  _spi.registerTransactionEventHandler( handler );
			  return handler;
		 }

		 public override KernelEventHandler UnregisterKernelEventHandler( KernelEventHandler handler )
		 {
			  _spi.unregisterKernelEventHandler( handler );
			  return handler;
		 }

		 public override TransactionEventHandler<T> UnregisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  _spi.unregisterTransactionEventHandler( handler );
			  return handler;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.graphdb.ResourceIterator<org.Neo4Net.graphdb.Node> findNodes(final org.Neo4Net.graphdb.Label myLabel, final String key, final Object value)
		 public override ResourceIterator<Node> FindNodes( Label myLabel, string key, object value )
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  TokenRead tokenRead = transaction.TokenRead();
			  int labelId = tokenRead.NodeLabel( myLabel.Name() );
			  int propertyId = tokenRead.PropertyKey( key );
			  return NodesByLabelAndProperty( transaction, labelId, IndexQuery.exact( propertyId, Values.of( value ) ) );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, string key1, object value1, string key2, object value2 )
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  TokenRead tokenRead = transaction.TokenRead();
			  int labelId = tokenRead.NodeLabel( label.Name() );
			  return NodesByLabelAndProperties( transaction, labelId, IndexQuery.exact( tokenRead.PropertyKey( key1 ), Values.of( value1 ) ), IndexQuery.exact( tokenRead.PropertyKey( key2 ), Values.of( value2 ) ) );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, string key1, object value1, string key2, object value2, string key3, object value3 )
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  TokenRead tokenRead = transaction.TokenRead();
			  int labelId = tokenRead.NodeLabel( label.Name() );
			  return NodesByLabelAndProperties( transaction, labelId, IndexQuery.exact( tokenRead.PropertyKey( key1 ), Values.of( value1 ) ), IndexQuery.exact( tokenRead.PropertyKey( key2 ), Values.of( value2 ) ), IndexQuery.exact( tokenRead.PropertyKey( key3 ), Values.of( value3 ) ) );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, IDictionary<string, object> propertyValues )
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  TokenRead tokenRead = transaction.TokenRead();
			  int labelId = tokenRead.NodeLabel( label.Name() );
			  IndexQuery.ExactPredicate[] queries = new IndexQuery.ExactPredicate[propertyValues.Count];
			  int i = 0;
			  foreach ( KeyValuePair<string, object> entry in propertyValues.SetOfKeyValuePairs() )
			  {
					queries[i++] = IndexQuery.exact( tokenRead.PropertyKey( entry.Key ), Values.of( entry.Value ) );
			  }
			  return NodesByLabelAndProperties( transaction, labelId, queries );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.graphdb.ResourceIterator<org.Neo4Net.graphdb.Node> findNodes(final org.Neo4Net.graphdb.Label myLabel, final String key, final String value, final org.Neo4Net.graphdb.StringSearchMode searchMode)
		 public override ResourceIterator<Node> FindNodes( Label myLabel, string key, string value, StringSearchMode searchMode )
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );
			  TokenRead tokenRead = transaction.TokenRead();
			  int labelId = tokenRead.NodeLabel( myLabel.Name() );
			  int propertyId = tokenRead.PropertyKey( key );
			  IndexQuery query;
			  switch ( searchMode )
			  {
			  case StringSearchMode.EXACT:
					query = IndexQuery.exact( propertyId, utf8Value( value.GetBytes( UTF_8 ) ) );
					break;
			  case StringSearchMode.PREFIX:
					query = IndexQuery.stringPrefix( propertyId, utf8Value( value.GetBytes( UTF_8 ) ) );
					break;
			  case StringSearchMode.SUFFIX:
					query = IndexQuery.stringSuffix( propertyId, utf8Value( value.GetBytes( UTF_8 ) ) );
					break;
			  case StringSearchMode.CONTAINS:
					query = IndexQuery.stringContains( propertyId, utf8Value( value.GetBytes( UTF_8 ) ) );
					break;
			  default:
					throw new System.InvalidOperationException( "Unknown string search mode: " + searchMode );
			  }
			  return NodesByLabelAndProperty( transaction, labelId, query );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Node findNode(final org.Neo4Net.graphdb.Label myLabel, final String key, final Object value)
		 public override Node FindNode( Label myLabel, string key, object value )
		 {
			  using ( ResourceIterator<Node> iterator = FindNodes( myLabel, key, value ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !iterator.hasNext() )
					{
						 return null;
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node node = iterator.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iterator.hasNext() )
					{
						 throw new MultipleFoundException( format( "Found multiple nodes with label: '%s', property name: '%s' and property " + "value: '%s' while only one was expected.", myLabel, key, value ) );
					}
					return node;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.graphdb.ResourceIterator<org.Neo4Net.graphdb.Node> findNodes(final org.Neo4Net.graphdb.Label myLabel)
		 public override ResourceIterator<Node> FindNodes( Label myLabel )
		 {
			  return AllNodesWithLabel( myLabel );
		 }

		 private InternalTransaction BeginTransactionInternal( KernelTransaction.Type type, LoginContext loginContext, long timeoutMillis )
		 {
			  if ( _statementContext.hasTransaction() )
			  {
					// FIXME: perhaps we should check that the new type and access mode are compatible with the current tx
					return new PlaceboTransaction( _statementContext.getKernelTransactionBoundToThisThread( true ) );
			  }
			  return new TopLevelTransaction( _spi.BeginTransaction( type, loginContext, timeoutMillis ) );
		 }

		 private ResourceIterator<Node> NodesByLabelAndProperty( KernelTransaction transaction, int labelId, IndexQuery query )
		 {
			  Statement statement = transaction.AcquireStatement();
			  Read read = transaction.DataRead();

			  if ( query.PropertyKeyId() == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN || labelId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
			  {
					statement.Close();
					return emptyResourceIterator();
			  }
			  IndexReference index = transaction.SchemaRead().index(labelId, query.PropertyKeyId());
			  if ( index != IndexReference.NO_INDEX )
			  {
					// Ha! We found an index - let's use it to find matching nodes
					try
					{
						 NodeValueIndexCursor cursor = transaction.Cursors().allocateNodeValueIndexCursor();
						 read.NodeIndexSeek( index, cursor, IndexOrder.NONE, false, query );

						 return new NodeCursorResourceIterator<Node>( cursor, statement, this.newNodeProxy );
					}
					catch ( KernelException )
					{
						 // weird at this point but ignore and fallback to a label scan
					}
			  }

			  return GetNodesByLabelAndPropertyWithoutIndex( statement, labelId, query );
		 }

		 private ResourceIterator<Node> NodesByLabelAndProperties( KernelTransaction transaction, int labelId, params IndexQuery.ExactPredicate[] queries )
		 {
			  Statement statement = transaction.AcquireStatement();
			  Read read = transaction.DataRead();

			  if ( IsInvalidQuery( labelId, queries ) )
			  {
					statement.Close();
					return emptyResourceIterator();
			  }

			  int[] propertyIds = GetPropertyIds( queries );
			  IndexReference index = FindMatchingIndex( transaction, labelId, propertyIds );

			  if ( index != IndexReference.NO_INDEX )
			  {
					try
					{
						 NodeValueIndexCursor cursor = transaction.Cursors().allocateNodeValueIndexCursor();
						 read.NodeIndexSeek( index, cursor, IndexOrder.NONE, false, GetReorderedIndexQueries( index.Properties(), queries ) );
						 return new NodeCursorResourceIterator<Node>( cursor, statement, this.newNodeProxy );
					}
					catch ( KernelException )
					{
						 // weird at this point but ignore and fallback to a label scan
					}
			  }
			  return GetNodesByLabelAndPropertyWithoutIndex( statement, labelId, queries );
		 }

		 private static IndexReference FindMatchingIndex( KernelTransaction transaction, int labelId, int[] propertyIds )
		 {
			  IndexReference index = transaction.SchemaRead().index(labelId, propertyIds);
			  if ( index != IndexReference.NO_INDEX )
			  {
					// index found with property order matching the query
					return index;
			  }
			  else
			  {
					// attempt to find matching index with different property order
					Arrays.sort( propertyIds );
					AssertNoDuplicates( propertyIds, transaction.TokenRead() );

					int[] workingCopy = new int[propertyIds.Length];

					IEnumerator<IndexReference> indexes = transaction.SchemaRead().indexesGetForLabel(labelId);
					while ( indexes.MoveNext() )
					{
						 index = indexes.Current;
						 int[] original = index.Properties();
						 if ( HasSamePropertyIds( original, workingCopy, propertyIds ) )
						 {
							  // Ha! We found an index with the same properties in another order
							  return index;
						 }
					}
					return IndexReference.NO_INDEX;
			  }
		 }

		 private static IndexQuery[] GetReorderedIndexQueries( int[] indexPropertyIds, IndexQuery[] queries )
		 {
			  IndexQuery[] orderedQueries = new IndexQuery[queries.Length];
			  for ( int i = 0; i < indexPropertyIds.Length; i++ )
			  {
					int propertyKeyId = indexPropertyIds[i];
					foreach ( IndexQuery query in queries )
					{
						 if ( query.PropertyKeyId() == propertyKeyId )
						 {
							  orderedQueries[i] = query;
							  break;
						 }
					}
			  }
			  return orderedQueries;
		 }

		 private static bool HasSamePropertyIds( int[] original, int[] workingCopy, int[] propertyIds )
		 {
			  if ( original.Length == propertyIds.Length )
			  {
					Array.Copy( original, 0, workingCopy, 0, original.Length );
					Arrays.sort( workingCopy );
					return Arrays.Equals( propertyIds, workingCopy );
			  }
			  return false;
		 }

		 private static int[] GetPropertyIds( IndexQuery[] queries )
		 {
			  int[] propertyIds = new int[queries.Length];
			  for ( int i = 0; i < queries.Length; i++ )
			  {
					propertyIds[i] = queries[i].PropertyKeyId();
			  }
			  return propertyIds;
		 }

		 private static bool IsInvalidQuery( int labelId, IndexQuery[] queries )
		 {
			  bool invalidQuery = labelId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN;
			  foreach ( IndexQuery query in queries )
			  {
					int propertyKeyId = query.PropertyKeyId();
					invalidQuery = invalidQuery || propertyKeyId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN;
			  }
			  return invalidQuery;
		 }

		 private static void AssertNoDuplicates( int[] propertyIds, TokenRead tokenRead )
		 {
			  int prev = propertyIds[0];
			  for ( int i = 1; i < propertyIds.Length; i++ )
			  {
					int curr = propertyIds[i];
					if ( curr == prev )
					{
						 SilentTokenNameLookup tokenLookup = new SilentTokenNameLookup( tokenRead );
						 throw new System.ArgumentException( format( "Provided two queries for property %s. Only one query per property key can be performed", tokenLookup.PropertyKeyGetName( curr ) ) );
					}
					prev = curr;
			  }
		 }

		 private ResourceIterator<Node> GetNodesByLabelAndPropertyWithoutIndex( Statement statement, int labelId, params IndexQuery[] queries )
		 {
			  KernelTransaction transaction = _statementContext.getKernelTransactionBoundToThisThread( true );

			  NodeLabelIndexCursor nodeLabelCursor = transaction.Cursors().allocateNodeLabelIndexCursor();
			  NodeCursor nodeCursor = transaction.Cursors().allocateNodeCursor();
			  PropertyCursor propertyCursor = transaction.Cursors().allocatePropertyCursor();

			  transaction.DataRead().nodeLabelScan(labelId, nodeLabelCursor);

			  return new NodeLabelPropertyIterator( transaction.DataRead(), nodeLabelCursor, nodeCursor, propertyCursor, statement, this.newNodeProxy, queries );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.graphdb.ResourceIterator<org.Neo4Net.graphdb.Node> allNodesWithLabel(final org.Neo4Net.graphdb.Label myLabel)
		 private ResourceIterator<Node> AllNodesWithLabel( Label myLabel )
		 {
			  KernelTransaction ktx = _statementContext.getKernelTransactionBoundToThisThread( true );
			  Statement statement = ktx.AcquireStatement();

			  int labelId = ktx.TokenRead().nodeLabel(myLabel.Name());
			  if ( labelId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
			  {
					statement.Close();
					return Iterators.emptyResourceIterator();
			  }

			  NodeLabelIndexCursor cursor = ktx.Cursors().allocateNodeLabelIndexCursor();
			  ktx.DataRead().nodeLabelScan(labelId, cursor);
			  return new NodeCursorResourceIterator<Node>( cursor, statement, this.newNodeProxy );
		 }

		 public override TraversalDescription TraversalDescription()
		 {
			  return new MonoDirectionalTraversalDescription( _statementContext );
		 }

		 public override BidirectionalTraversalDescription BidirectionalTraversalDescription()
		 {
			  return new BidirectionalTraversalDescriptionImpl( _statementContext );
		 }

		 // GraphDatabaseAPI
		 public virtual DependencyResolver DependencyResolver
		 {
			 get
			 {
				  return _spi.resolver();
			 }
		 }

		 public override StoreId StoreId()
		 {
			  return _spi.storeId();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL validateURLAccess(java.net.URL url) throws org.Neo4Net.graphdb.security.URLAccessValidationError
		 public override URL ValidateURLAccess( URL url )
		 {
			  return _spi.validateURLAccess( url );
		 }

		 public override DatabaseLayout DatabaseLayout()
		 {
			  return _spi.databaseLayout();
		 }

		 public override string ToString()
		 {
			  return _spi.name() + " [" + DatabaseLayout() + "]";
		 }

		 public override Statement Statement()
		 {
			  return _statementContext.get();
		 }

		 public override KernelTransaction KernelTransaction()
		 {
			  return _statementContext.getKernelTransactionBoundToThisThread( true );
		 }

		 public virtual IGraphDatabaseService GraphDatabase
		 {
			 get
			 {
				  return this;
			 }
		 }

		 public override void AssertInUnterminatedTransaction()
		 {
			  _statementContext.assertInUnterminatedTransaction();
		 }

		 public override void FailTransaction()
		 {
			  _statementContext.getKernelTransactionBoundToThisThread( true ).failure();
		 }

		 public override RelationshipProxy NewRelationshipProxy( long id )
		 {
			  return new RelationshipProxy( this, id );
		 }

		 public override RelationshipProxy NewRelationshipProxy( long id, long startNodeId, int typeId, long endNodeId )
		 {
			  return new RelationshipProxy( this, id, startNodeId, typeId, endNodeId );
		 }

		 public override NodeProxy NewNodeProxy( long nodeId )
		 {
			  return new NodeProxy( this, nodeId );
		 }

		 public override RelationshipType GetRelationshipTypeById( int type )
		 {
			  try
			  {
					string name = _tokenHolders.relationshipTypeTokens().getTokenById(type).name();
					return RelationshipType.withName( name );
			  }
			  catch ( TokenNotFoundException )
			  {
					throw new System.InvalidOperationException( "Kernel API returned non-existent relationship type: " + type );
			  }
		 }

		 public override GraphPropertiesProxy NewGraphPropertiesProxy()
		 {
			  return new GraphPropertiesProxy( this );
		 }

		 private class NodeLabelPropertyIterator : PrefetchingNodeResourceIterator
		 {
			  internal readonly Read Read;
			  internal readonly NodeLabelIndexCursor NodeLabelCursor;
			  internal readonly NodeCursor NodeCursor;
			  internal readonly PropertyCursor PropertyCursor;
			  internal readonly IndexQuery[] Queries;

			  internal NodeLabelPropertyIterator( Read read, NodeLabelIndexCursor nodeLabelCursor, NodeCursor nodeCursor, PropertyCursor propertyCursor, Statement statement, NodeFactory nodeFactory, params IndexQuery[] queries ) : base( statement, nodeFactory )
			  {
					this.Read = read;
					this.NodeLabelCursor = nodeLabelCursor;
					this.NodeCursor = nodeCursor;
					this.PropertyCursor = propertyCursor;
					this.Queries = queries;
			  }

			  protected internal override long FetchNext()
			  {
					bool hasNext;
					do
					{
						 hasNext = NodeLabelCursor.next();

					} while ( hasNext && !HasPropertiesWithValues() );

					if ( hasNext )
					{
						 return NodeLabelCursor.nodeReference();
					}
					else
					{
						 Close();
						 return NO_ID;
					}
			  }

			  internal override void CloseResources( Statement statement )
			  {
					IOUtils.closeAllSilently( statement, NodeLabelCursor, NodeCursor, PropertyCursor );
			  }

			  internal virtual bool HasPropertiesWithValues()
			  {
					int targetCount = Queries.Length;
					Read.singleNode( NodeLabelCursor.nodeReference(), NodeCursor );
					if ( NodeCursor.next() )
					{
						 NodeCursor.properties( PropertyCursor );
						 while ( PropertyCursor.next() )
						 {
							  foreach ( IndexQuery query in Queries )
							  {
									if ( PropertyCursor.propertyKey() == query.PropertyKeyId() )
									{
										 if ( query.AcceptsValueAt( PropertyCursor ) )
										 {
											  targetCount--;
											  if ( targetCount == 0 )
											  {
													return true;
											  }
										 }
										 else
										 {
											  return false;
										 }
									}
							  }
						 }
					}
					return false;
			  }
		 }

		 private void AssertTransactionOpen()
		 {
			  AssertTransactionOpen( _statementContext.getKernelTransactionBoundToThisThread( true ) );
		 }

		 private static void AssertTransactionOpen( KernelTransaction transaction )
		 {
			  if ( transaction.Terminated )
			  {
					Status terminationReason = transaction.ReasonIfTerminated.orElse( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
					throw new TransactionTerminatedException( terminationReason );
			  }
		 }

		 private sealed class NodeCursorResourceIterator<CURSOR> : PrefetchingNodeResourceIterator where CURSOR : Neo4Net.Kernel.Api.Internal.NodeIndexCursor
		 {
			  internal readonly CURSOR Cursor;

			  internal NodeCursorResourceIterator( CURSOR cursor, Statement statement, NodeFactory nodeFactory ) : base( statement, nodeFactory )
			  {
					this.Cursor = cursor;
			  }

			  internal override long FetchNext()
			  {
					if ( Cursor.next() )
					{
						 return Cursor.nodeReference();
					}
					else
					{
						 close();
						 return NO_ID;
					}
			  }

			  internal override void CloseResources( Statement statement )
			  {
					IOUtils.closeAllSilently( statement, Cursor );
			  }
		 }

		 private abstract class PrefetchingNodeResourceIterator : ResourceIterator<Node>
		 {
			 public abstract ResourceIterator<R> Map( System.Func<T, R> map );
			 public abstract java.util.stream.Stream<T> Stream();
			  internal readonly Statement Statement;
			  internal readonly NodeFactory NodeFactory;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long NextConflict;
			  internal bool Closed;

			  internal const long NOT_INITIALIZED = -2L;
			  protected internal const long NO_ID = -1L;

			  internal PrefetchingNodeResourceIterator( Statement statement, NodeFactory nodeFactory )
			  {
					this.Statement = statement;
					this.NodeFactory = nodeFactory;
					this.NextConflict = NOT_INITIALIZED;
			  }

			  public override bool HasNext()
			  {
					if ( NextConflict == NOT_INITIALIZED )
					{
						 NextConflict = FetchNext();
					}
					return NextConflict != NO_ID;
			  }

			  public override Node Next()
			  {
					if ( !HasNext() )
					{
						 Close();
						 throw new NoSuchElementException();
					}
					Node nodeProxy = NodeFactory.make( NextConflict );
					NextConflict = FetchNext();
					return nodeProxy;
			  }

			  public override void Close()
			  {
					if ( !Closed )
					{
						 NextConflict = NO_ID;
						 CloseResources( Statement );
						 Closed = true;
					}
			  }

			  internal abstract long FetchNext();

			  internal abstract void CloseResources( Statement statement );
		 }

		 private interface NodeFactory
		 {
			  NodeProxy Make( long id );
		 }
	}

}