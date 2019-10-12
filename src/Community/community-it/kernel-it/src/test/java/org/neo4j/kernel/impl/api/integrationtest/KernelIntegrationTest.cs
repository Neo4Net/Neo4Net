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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Resource = Neo4Net.Graphdb.Resource;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using Procedures = Neo4Net.@internal.Kernel.Api.Procedures;
	using PropertyCursor = Neo4Net.@internal.Kernel.Api.PropertyCursor;
	using RelationshipScanCursor = Neo4Net.@internal.Kernel.Api.RelationshipScanCursor;
	using SchemaWrite = Neo4Net.@internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Neo4Net.@internal.Kernel.Api.TokenWrite;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using DbmsOperations = Neo4Net.Kernel.api.dbms.DbmsOperations;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.helpers.RelationshipSelections.allIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.helpers.RelationshipSelections.incomingIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.helpers.RelationshipSelections.outgoingIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	public abstract class KernelIntegrationTest
	{
		private bool InstanceFieldsInitialized = false;

		public KernelIntegrationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( TestDir ).around( FileSystemRule );
		}

		 protected internal readonly TestDirectory TestDir = TestDirectory.testDirectory();
		 protected internal readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDir).around(fileSystemRule);
		 public RuleChain RuleChain;
		 protected internal GraphDatabaseAPI Db;
		 internal ThreadToStatementContextBridge StatementContextSupplier;
		 protected internal Kernel Kernel;
		 protected internal IndexingService IndexingService;

		 private Transaction _transaction;
		 private DbmsOperations _dbmsOperations;
		 protected internal DependencyResolver DependencyResolver;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.internal.kernel.api.TokenWrite tokenWriteInNewTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 protected internal virtual TokenWrite TokenWriteInNewTransaction()
		 {
			  _transaction = Kernel.beginTransaction( @implicit, AnonymousContext.writeToken() );
			  return _transaction.tokenWrite();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.internal.kernel.api.Write dataWriteInNewTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 protected internal virtual Write DataWriteInNewTransaction()
		 {
			  _transaction = Kernel.beginTransaction( @implicit, AnonymousContext.write() );
			  return _transaction.dataWrite();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.internal.kernel.api.SchemaWrite schemaWriteInNewTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 protected internal virtual SchemaWrite SchemaWriteInNewTransaction()
		 {
			  _transaction = Kernel.beginTransaction( @implicit, AUTH_DISABLED );
			  return _transaction.schemaWrite();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.internal.kernel.api.Procedures procs() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual Procedures Procs()
		 {
			  _transaction = Kernel.beginTransaction( @implicit, AnonymousContext.read() );
			  return _transaction.procedures();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.internal.kernel.api.Procedures procsSchema() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual Procedures ProcsSchema()
		 {
			  _transaction = Kernel.beginTransaction( KernelTransaction.Type.@implicit, AnonymousContext.full() );
			  return _transaction.procedures();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.internal.kernel.api.Transaction newTransaction() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual Transaction NewTransaction()
		 {
			  _transaction = Kernel.beginTransaction( @implicit, AnonymousContext.read() );
			  return _transaction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.internal.kernel.api.Transaction newTransaction(org.neo4j.internal.kernel.api.security.LoginContext loginContext) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual Transaction NewTransaction( LoginContext loginContext )
		 {
			  _transaction = Kernel.beginTransaction( @implicit, loginContext );
			  return _transaction;
		 }

		 /// <summary>
		 /// Create a temporary section wherein other transactions can be started an committed, and after which the <em>current</em> transaction will be restored as
		 /// current.
		 /// </summary>
		 protected internal virtual Resource CaptureTransaction()
		 {
			  Transaction tx = _transaction;
			  return () =>
			  {
				_transaction = tx;
			  };
		 }

		 protected internal virtual DbmsOperations DbmsOperations()
		 {
			  return _dbmsOperations;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void commit() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual void Commit()
		 {
			  _transaction.success();
			  try
			  {
					_transaction.close();
			  }
			  finally
			  {
					_transaction = null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void rollback() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual void Rollback()
		 {
			  _transaction.failure();
			  try
			  {
					_transaction.close();
			  }
			  finally
			  {
					_transaction = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  StartDb();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  StopDb();
		 }

		 protected internal virtual void StartDb()
		 {
			  Db = ( GraphDatabaseAPI ) CreateGraphDatabase();
			  DependencyResolver = Db.DependencyResolver;
			  Kernel = DependencyResolver.resolveDependency( typeof( Kernel ) );
			  IndexingService = DependencyResolver.resolveDependency( typeof( IndexingService ) );
			  StatementContextSupplier = DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
			  _dbmsOperations = DependencyResolver.resolveDependency( typeof( DbmsOperations ) );
		 }

		 protected internal virtual GraphDatabaseService CreateGraphDatabase()
		 {
			  GraphDatabaseBuilder graphDatabaseBuilder = Configure( CreateGraphDatabaseFactory() ).setFileSystem(FileSystemRule.get()).newEmbeddedDatabaseBuilder(TestDir.storeDir());
			  return Configure( graphDatabaseBuilder ).newGraphDatabase();
		 }

		 protected internal virtual TestGraphDatabaseFactory CreateGraphDatabaseFactory()
		 {
			  return new TestGraphDatabaseFactory();
		 }

		 protected internal virtual GraphDatabaseBuilder Configure( GraphDatabaseBuilder graphDatabaseBuilder )
		 {
			  return graphDatabaseBuilder;
		 }

		 protected internal virtual TestGraphDatabaseFactory Configure( TestGraphDatabaseFactory factory )
		 {
			  return factory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dbWithNoCache() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 internal virtual void DbWithNoCache()
		 {
			  StopDb();
			  StartDb();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void stopDb() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void StopDb()
		 {
			  if ( _transaction != null && _transaction.Open )
			  {
					_transaction.close();
			  }
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartDb() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 internal virtual void RestartDb()
		 {
			  StopDb();
			  StartDb();
		 }

		 internal virtual Value RelationshipGetProperty( Transaction transaction, long relationship, int property )
		 {
			  using ( RelationshipScanCursor cursor = transaction.Cursors().allocateRelationshipScanCursor(), PropertyCursor properties = transaction.Cursors().allocatePropertyCursor() )
			  {
					transaction.DataRead().singleRelationship(relationship, cursor);
					if ( !cursor.Next() )
					{
						 return NO_VALUE;
					}
					else
					{
						 cursor.Properties( properties );
						 while ( properties.Next() )
						 {
							  if ( properties.PropertyKey() == property )
							  {
									return properties.PropertyValue();
							  }
						 }
						 return NO_VALUE;
					}
			  }
		 }

		 internal virtual IEnumerator<long> NodeGetRelationships( Transaction transaction, long node, Direction direction )
		 {
			  return NodeGetRelationships( transaction, node, direction, null );
		 }

		 internal virtual IEnumerator<long> NodeGetRelationships( Transaction transaction, long node, Direction direction, int[] types )
		 {
			  NodeCursor cursor = transaction.Cursors().allocateNodeCursor();
			  transaction.DataRead().singleNode(node, cursor);
			  if ( !cursor.Next() )
			  {
					return emptyIterator();
			  }

			  switch ( direction.innerEnumValue )
			  {
			  case Direction.InnerEnum.OUTGOING:
					return outgoingIterator( transaction.Cursors(), cursor, types, (id, startNodeId, typeId, endNodeId) => id );
			  case Direction.InnerEnum.INCOMING:
					return incomingIterator( transaction.Cursors(), cursor, types, (id, startNodeId, typeId, endNodeId) => id );
			  case Direction.InnerEnum.BOTH:
					return allIterator( transaction.Cursors(), cursor, types, (id, startNodeId, typeId, endNodeId) => id );
			  default:
					throw new System.InvalidOperationException( direction + " is not a valid direction" );
			  }
		 }

		 protected internal virtual int CountNodes( Transaction transaction )
		 {
			  int result = 0;
			  using ( NodeCursor cursor = transaction.Cursors().allocateNodeCursor() )
			  {
					transaction.DataRead().allNodesScan(cursor);
					while ( cursor.Next() )
					{
						 result++;
					}
			  }
			  return result;
		 }

		 internal static int CountRelationships( Transaction transaction )
		 {
			  int result = 0;
			  using ( RelationshipScanCursor cursor = transaction.Cursors().allocateRelationshipScanCursor() )
			  {
					transaction.DataRead().allRelationshipsScan(cursor);
					while ( cursor.Next() )
					{
						 result++;
					}
			  }
			  return result;
		 }

		 internal virtual KernelImpl InternalKernel()
		 {
			  return ( KernelImpl )Kernel;
		 }
	}

}