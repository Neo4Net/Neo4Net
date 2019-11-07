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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Resource = Neo4Net.GraphDb.Resource;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using Procedures = Neo4Net.Kernel.Api.Internal.Procedures;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using Write = Neo4Net.Kernel.Api.Internal.Write;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using DbmsOperations = Neo4Net.Kernel.Api.dbms.DbmsOperations;
	using AnonymousContext = Neo4Net.Kernel.Api.security.AnonymousContext;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.Helpers.RelationshipSelections.allIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.Helpers.RelationshipSelections.incomingIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.Helpers.RelationshipSelections.outgoingIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.NO_VALUE;

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
//ORIGINAL LINE: protected Neo4Net.Kernel.Api.Internal.TokenWrite tokenWriteInNewTransaction() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 protected internal virtual TokenWrite TokenWriteInNewTransaction()
		 {
			  _transaction = Kernel.BeginTransaction( @implicit, AnonymousContext.WriteToken() );
			  return _transaction.tokenWrite();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.Kernel.Api.Internal.Write dataWriteInNewTransaction() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 protected internal virtual Write DataWriteInNewTransaction()
		 {
			  _transaction = Kernel.BeginTransaction( @implicit, AnonymousContext.write() );
			  return _transaction.dataWrite();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.Kernel.Api.Internal.SchemaWrite schemaWriteInNewTransaction() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 protected internal virtual SchemaWrite SchemaWriteInNewTransaction()
		 {
			  _transaction = Kernel.BeginTransaction( @implicit, AUTH_DISABLED );
			  return _transaction.schemaWrite();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.Kernel.Api.Internal.Procedures procs() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 protected internal virtual Procedures Procs()
		 {
			  _transaction = Kernel.BeginTransaction( @implicit, AnonymousContext.read() );
			  return _transaction.procedures();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.Kernel.Api.Internal.Procedures procsSchema() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 protected internal virtual Procedures ProcsSchema()
		 {
			  _transaction = Kernel.BeginTransaction( KernelTransaction.Type.@implicit, AnonymousContext.full() );
			  return _transaction.procedures();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.Kernel.Api.Internal.Transaction newTransaction() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 protected internal virtual Transaction NewTransaction()
		 {
			  _transaction = Kernel.BeginTransaction( @implicit, AnonymousContext.read() );
			  return _transaction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.Kernel.Api.Internal.Transaction newTransaction(Neo4Net.Kernel.Api.Internal.security.LoginContext loginContext) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 protected internal virtual Transaction NewTransaction( LoginContext loginContext )
		 {
			  _transaction = Kernel.BeginTransaction( @implicit, loginContext );
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
//ORIGINAL LINE: protected void commit() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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
//ORIGINAL LINE: protected void rollback() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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

		 protected internal virtual IGraphDatabaseService CreateGraphDatabase()
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
//ORIGINAL LINE: void dbWithNoCache() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 internal virtual void DbWithNoCache()
		 {
			  StopDb();
			  StartDb();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void stopDb() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private void StopDb()
		 {
			  if ( _transaction != null && _transaction.Open )
			  {
					_transaction.close();
			  }
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartDb() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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