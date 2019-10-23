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
namespace Neo4Net.Test.rule
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Result = Neo4Net.GraphDb.Result;
	using StringSearchMode = Neo4Net.GraphDb.StringSearchMode;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.config;
	using KernelEventHandler = Neo4Net.GraphDb.Events.KernelEventHandler;
	using Neo4Net.GraphDb.Events;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.GraphDb.factory.GraphDatabaseFactory;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using URLAccessValidationError = Neo4Net.GraphDb.security.URLAccessValidationError;
	using BidirectionalTraversalDescription = Neo4Net.GraphDb.Traversal.BidirectionalTraversalDescription;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	public abstract class DatabaseRule : ExternalResource, GraphDatabaseAPI
	{
		public abstract Node CreateNode( params Label[] labels );
		 private GraphDatabaseBuilder _databaseBuilder;
		 private GraphDatabaseAPI _database;
		 private DatabaseLayout _databaseLayout;
		 private System.Func<Statement> _statementSupplier;
		 private bool _startEagerly = true;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<org.Neo4Net.graphdb.config.Setting<?>, String> globalConfig = new java.util.HashMap<>();
		 private readonly IDictionary<Setting<object>, string> _globalConfig = new Dictionary<Setting<object>, string>();
		 private readonly Monitors _monitors = new Monitors();

		 /// <summary>
		 /// Means the database will be started on first <seealso cref="getGraphDatabaseAPI()"/>}
		 /// or <seealso cref="ensureStarted()"/> call.
		 /// </summary>
		 public virtual DatabaseRule StartLazily()
		 {
			  _startEagerly = false;
			  return this;
		 }

		 public virtual T When<T>( System.Func<GraphDatabaseService, T> function )
		 {
			  return function( GraphDatabaseAPI );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void executeAndCommit(System.Action<? super org.Neo4Net.graphdb.GraphDatabaseService> consumer)
		 public virtual void ExecuteAndCommit<T1>( System.Action<T1> consumer )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: transaction((System.Func<? super org.Neo4Net.graphdb.GraphDatabaseService,Void>) t ->
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  Transaction((System.Func<object, Void>) t =>
			  {
				consumer( t );
				return null;
			  }, true);
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <T> T executeAndCommit(System.Func<? super org.Neo4Net.graphdb.GraphDatabaseService, T> function)
		 public virtual T ExecuteAndCommit<T, T1>( System.Func<T1> function )
		 {
			  return Transaction( function, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <T> T executeAndRollback(System.Func<? super org.Neo4Net.graphdb.GraphDatabaseService, T> function)
		 public virtual T ExecuteAndRollback<T, T1>( System.Func<T1> function )
		 {
			  return Transaction( function, false );
		 }

		 public virtual System.Func<FROM, TO> Tx<FROM, TO>( System.Func<FROM, TO> function )
		 {
			  return from =>
			  {
				Function<GraphDatabaseService, TO> inner = graphDb => function( from );
				return ExecuteAndCommit( inner );
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private <T> T transaction(System.Func<? super org.Neo4Net.graphdb.GraphDatabaseService, T> function, boolean commit)
		 private T Transaction<T, T1>( System.Func<T1> function, bool commit )
		 {
			  return Tx( GraphDatabaseAPI, commit, RetryHandler_Fields.NoRetry, function );
		 }

		 /// <summary>
		 /// Perform a transaction, with the option to automatically retry on failure.
		 /// </summary>
		 /// <param name="db"> <seealso cref="GraphDatabaseService"/> to apply the transaction on. </param>
		 /// <param name="retry"> <seealso cref="RetryHandler"/> deciding what type of failures to retry on. </param>
		 /// <param name="transaction"> <seealso cref="Consumer"/> containing the transaction logic. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static void tx(org.Neo4Net.graphdb.GraphDatabaseService db, RetryHandler retry, System.Action<? super org.Neo4Net.graphdb.GraphDatabaseService> transaction)
		 public static void Tx<T1>( IGraphDatabaseService db, RetryHandler retry, System.Action<T1> transaction )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: System.Func<? super org.Neo4Net.graphdb.GraphDatabaseService,Void> voidFunction = _db ->
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  System.Func<object, Void> voidFunction = _db =>
			  {
				transaction( _db );
				return null;
			  };
			  Tx( db, true, retry, voidFunction );
		 }

		 /// <summary>
		 /// Perform a transaction, with the option to automatically retry on failure.
		 /// Also returning a result from the supplied transaction function.
		 /// </summary>
		 /// <param name="db"> <seealso cref="GraphDatabaseService"/> to apply the transaction on. </param>
		 /// <param name="commit"> whether or not to call <seealso cref="Transaction.success()"/> in the end. </param>
		 /// <param name="retry"> <seealso cref="RetryHandler"/> deciding what type of failures to retry on. </param>
		 /// <param name="transaction"> <seealso cref="Function"/> containing the transaction logic and returning a result. </param>
		 /// <returns> result from transaction <seealso cref="Function"/>. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <T> T tx(org.Neo4Net.graphdb.GraphDatabaseService db, boolean commit, RetryHandler retry, System.Func<? super org.Neo4Net.graphdb.GraphDatabaseService, T> transaction)
		 public static T Tx<T, T1>( IGraphDatabaseService db, bool commit, RetryHandler retry, System.Func<T1> transaction )
		 {
			  while ( true )
			  {
					try
					{
							using ( Transaction tx = Db.beginTx() )
							{
							 T result = transaction( db );
							 if ( commit )
							 {
								  tx.Success();
							 }
							 return result;
							}
					}
					catch ( Exception t )
					{
						 if ( !retry( t ) )
						 {
							  throw t;
						 }
						 // else continue one more time
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query )
		 {
			  return GraphDatabaseAPI.execute( query );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query, long timeout, java.util.concurrent.TimeUnit unit) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query, long timeout, TimeUnit unit )
		 {
			  return GraphDatabaseAPI.execute( query, timeout, unit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query, java.util.Map<String, Object> parameters) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query, IDictionary<string, object> parameters )
		 {
			  return GraphDatabaseAPI.execute( query, parameters );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result execute(String query, java.util.Map<String,Object> parameters, long timeout, java.util.concurrent.TimeUnit unit) throws org.Neo4Net.graphdb.QueryExecutionException
		 public override Result Execute( string query, IDictionary<string, object> parameters, long timeout, TimeUnit unit )
		 {
			  return GraphDatabaseAPI.execute( query, parameters, timeout, unit );
		 }

		 public override InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext )
		 {
			  return GraphDatabaseAPI.BeginTransaction( type, loginContext );
		 }

		 public override InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout, TimeUnit unit )
		 {
			  return GraphDatabaseAPI.BeginTransaction( type, loginContext, timeout, unit );
		 }

		 public override Transaction BeginTx()
		 {
			  return GraphDatabaseAPI.beginTx();
		 }

		 public override Transaction BeginTx( long timeout, TimeUnit timeUnit )
		 {
			  return GraphDatabaseAPI.beginTx( timeout, timeUnit );
		 }

		 public override Node CreateNode( params Label[] labels )
		 {
			  return GraphDatabaseAPI.createNode( labels );
		 }

		 public override Node GetNodeById( long id )
		 {
			  return GraphDatabaseAPI.getNodeById( id );
		 }

		 [Obsolete]
		 public override IndexManager Index()
		 {
			  return GraphDatabaseAPI.index();
		 }

		 public override Schema Schema()
		 {
			  return GraphDatabaseAPI.schema();
		 }

		 protected internal override void Before()
		 {
			  Create();
			  if ( _startEagerly )
			  {
					EnsureStarted();
			  }
		 }

		 protected internal override void After( bool success )
		 {
			  Shutdown( success );
		 }

		 private void Create()
		 {
			  CreateResources();
			  try
			  {
					GraphDatabaseFactory factory = NewFactory();
					factory.Monitors = _monitors;
					Configure( factory );
					_databaseBuilder = NewBuilder( factory );
					_globalConfig.forEach( _databaseBuilder.setConfig );
			  }
			  catch ( Exception e )
			  {
					DeleteResources();
					throw e;
			  }
		 }

		 /// <returns> the high level monitor in the database. </returns>
		 public virtual Monitors Monitors
		 {
			 get
			 {
				  return _monitors;
			 }
		 }

		 protected internal virtual void DeleteResources()
		 {
		 }

		 protected internal virtual void CreateResources()
		 {
		 }

		 protected internal abstract GraphDatabaseFactory NewFactory();

		 protected internal abstract GraphDatabaseBuilder NewBuilder( GraphDatabaseFactory factory );

		 protected internal virtual void Configure( GraphDatabaseFactory databaseFactory )
		 {
			  // Override to configure the database factory
		 }

		 /// <summary>
		 /// <seealso cref="DatabaseRule"/> now implements <seealso cref="GraphDatabaseAPI"/> directly, so no need. Also for ensuring
		 /// a lazily started database is created, use <seealso cref="ensureStarted()"/> instead.
		 /// </summary>
		 public virtual GraphDatabaseAPI GraphDatabaseAPI
		 {
			 get
			 {
				  EnsureStarted();
				  return _database;
			 }
		 }

		 public virtual void EnsureStarted()
		 {
			 lock ( this )
			 {
				  if ( _database == null )
				  {
						_database = ( GraphDatabaseAPI ) _databaseBuilder.newGraphDatabase();
						_databaseLayout = _database.databaseLayout();
						_statementSupplier = ResolveDependency( typeof( ThreadToStatementContextBridge ) );
				  }
			 }
		 }

		 /// <summary>
		 /// Adds or replaces a setting for the database managed by this database rule.
		 /// <para>
		 /// If this method is called when constructing the rule, the setting is considered a global setting applied to all tests.
		 /// </para>
		 /// <para>
		 /// If this method is called inside a specific test, i.e. after <seealso cref="before()"/>, but before started (a call to <seealso cref="startLazily()"/> have been made),
		 /// then this setting will be considered a test-specific setting, adding to or overriding the global settings for this test only.
		 /// Test-specific settings will be remembered throughout a test, even between restarts.
		 /// </para>
		 /// <para>
		 /// If this method is called when a database is already started an <seealso cref="System.InvalidOperationException"/> will be thrown since the setting
		 /// will have no effect, instead letting the developer notice that and change the test code.
		 /// </para>
		 /// </summary>
		 public virtual DatabaseRule WithSetting<T1>( Setting<T1> key, string value )
		 {
			  if ( _database != null )
			  {
					// Database already started
					throw new System.InvalidOperationException( "Wanted to set " + key + "=" + value + ", but database has already been started" );
			  }
			  if ( _databaseBuilder != null )
			  {
					// Test already started, but db not yet started
					_databaseBuilder.setConfig( key, value );
			  }
			  else
			  {
					// Test haven't started, we're still in phase of constructing this rule
					_globalConfig[key] = value;
			  }
			  return this;
		 }

		 /// <summary>
		 /// Applies all settings in the settings map.
		 /// </summary>
		 /// <seealso cref= #withSetting(Setting, String) </seealso>
		 public virtual DatabaseRule WithSettings<T1>( IDictionary<T1> configuration )
		 {
			  configuration.forEach( this.withSetting );
			  return this;
		 }

		 public interface RestartAction
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run(org.Neo4Net.io.fs.FileSystemAbstraction fs, org.Neo4Net.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException;
			  void Run( FileSystemAbstraction fs, DatabaseLayout databaseLayout );
		 }

		 public static class RestartAction_Fields
		 {
			 private readonly DatabaseRule _outerInstance;

			 public RestartAction_Fields( DatabaseRule outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly RestartAction Empty = ( fs, storeDirectory ) =>
			  {
				// duh
			  };

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.internal.GraphDatabaseAPI restartDatabase(String... configChanges) throws java.io.IOException
		 public virtual GraphDatabaseAPI RestartDatabase( params string[] configChanges )
		 {
			  return RestartDatabase( RestartAction_Fields.Empty, configChanges );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.internal.GraphDatabaseAPI restartDatabase(RestartAction action, String... configChanges) throws java.io.IOException
		 public virtual GraphDatabaseAPI RestartDatabase( RestartAction action, params string[] configChanges )
		 {
			  FileSystemAbstraction fs = ResolveDependency( typeof( FileSystemAbstraction ) );
			  _database.shutdown();
			  action.Run( fs, _databaseLayout );
			  _database = null;
			  // This DatabaseBuilder has already been configured with the global settings as well as any test-specific settings,
			  // so just apply these additional settings.
			  _databaseBuilder.Config = stringMap( configChanges );
			  return GraphDatabaseAPI;
		 }

		 public override void Shutdown()
		 {
			  Shutdown( true );
		 }

		 private void Shutdown( bool deleteResources )
		 {
			  _statementSupplier = null;
			  try
			  {
					if ( _database != null )
					{
						 _database.shutdown();
					}
			  }
			  finally
			  {
					if ( deleteResources )
					{
						 deleteResources();
					}
					_database = null;
			  }
		 }

		 public virtual void ShutdownAndKeepStore()
		 {
			  Shutdown( false );
		 }

		 public virtual T ResolveDependency<T>( Type type )
		 {
				 type = typeof( T );
			  return GraphDatabaseAPI.DependencyResolver.resolveDependency( type );
		 }

		 public virtual Statement Statement()
		 {
			  EnsureStarted();
			  return _statementSupplier.get();
		 }

		 public virtual KernelTransaction Transaction()
		 {
			  EnsureStarted();
			  return _database.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
		 }

		 public virtual DependencyResolver DependencyResolver
		 {
			 get
			 {
				  return _database.DependencyResolver;
			 }
		 }

		 public override StoreId StoreId()
		 {
			  return _database.storeId();
		 }

		 public override DatabaseLayout DatabaseLayout()
		 {
			  return _database.databaseLayout();
		 }

		 public virtual string DatabaseDirAbsolutePath
		 {
			 get
			 {
				  return DatabaseLayout().databaseDirectory().AbsolutePath;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL validateURLAccess(java.net.URL url) throws org.Neo4Net.graphdb.security.URLAccessValidationError
		 public override URL ValidateURLAccess( URL url )
		 {
			  return _database.validateURLAccess( url );
		 }

		 public override Node CreateNode()
		 {
			  return _database.createNode();
		 }

		 public override long? CreateNodeId()
		 {
			  return _database.createNodeId();
		 }

		 public override Relationship GetRelationshipById( long id )
		 {
			  return _database.getRelationshipById( id );
		 }

		 public virtual ResourceIterable<Node> AllNodes
		 {
			 get
			 {
				  return _database.AllNodes;
			 }
		 }

		 public virtual ResourceIterable<Relationship> AllRelationships
		 {
			 get
			 {
				  return _database.AllRelationships;
			 }
		 }

		 public virtual ResourceIterable<Label> AllLabelsInUse
		 {
			 get
			 {
				  return _database.AllLabelsInUse;
			 }
		 }

		 public virtual ResourceIterable<RelationshipType> AllRelationshipTypesInUse
		 {
			 get
			 {
				  return _database.AllRelationshipTypesInUse;
			 }
		 }

		 public virtual ResourceIterable<Label> AllLabels
		 {
			 get
			 {
				  return _database.AllLabels;
			 }
		 }

		 public virtual ResourceIterable<RelationshipType> AllRelationshipTypes
		 {
			 get
			 {
				  return _database.AllRelationshipTypes;
			 }
		 }

		 public virtual ResourceIterable<string> AllPropertyKeys
		 {
			 get
			 {
				  return _database.AllPropertyKeys;
			 }
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, string key, object value )
		 {
			  return _database.findNodes( label, key, value );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, string key1, object value1, string key2, object value2 )
		 {
			  return _database.findNodes( label, key1, value1, key2, value2 );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, string key1, object value1, string key2, object value2, string key3, object value3 )
		 {
			  return _database.findNodes( label, key1, value1, key2, value2, key3, value3 );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, IDictionary<string, object> propertyValues )
		 {
			  return _database.findNodes( label, propertyValues );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label, string key, string template, StringSearchMode searchMode )
		 {
			  return _database.findNodes( label, key, template, searchMode );
		 }

		 public override Node FindNode( Label label, string key, object value )
		 {
			  return _database.findNode( label, key, value );
		 }

		 public override ResourceIterator<Node> FindNodes( Label label )
		 {
			  return _database.findNodes( label );
		 }

		 public override bool IsAvailable( long timeout )
		 {
			  return _database.isAvailable( timeout );
		 }

		 public override TransactionEventHandler<T> RegisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  return _database.registerTransactionEventHandler( handler );
		 }

		 public override TransactionEventHandler<T> UnregisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  return _database.unregisterTransactionEventHandler( handler );
		 }

		 public override KernelEventHandler RegisterKernelEventHandler( KernelEventHandler handler )
		 {
			  return _database.registerKernelEventHandler( handler );
		 }

		 public override KernelEventHandler UnregisterKernelEventHandler( KernelEventHandler handler )
		 {
			  return _database.unregisterKernelEventHandler( handler );
		 }

		 public override TraversalDescription TraversalDescription()
		 {
			  return _database.traversalDescription();
		 }

		 public override BidirectionalTraversalDescription BidirectionalTraversalDescription()
		 {
			  return _database.bidirectionalTraversalDescription();
		 }
	}

}