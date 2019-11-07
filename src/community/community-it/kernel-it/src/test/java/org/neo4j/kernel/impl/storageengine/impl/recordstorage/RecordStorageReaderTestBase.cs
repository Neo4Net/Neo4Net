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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using After = org.junit.After;
	using Before = org.junit.Before;

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using ClockContext = Neo4Net.Kernel.Impl.Api.ClockContext;
	using KernelStatement = Neo4Net.Kernel.Impl.Api.KernelStatement;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;

	/// <summary>
	/// Base class for disk layer tests, which test read-access to committed data.
	/// </summary>
	public abstract class RecordStorageReaderTestBase
	{
		 protected internal GraphDatabaseAPI Db;
		 protected internal readonly Label Label1 = label( "FirstLabel" );
		 protected internal readonly Label Label2 = label( "SecondLabel" );
		 protected internal readonly RelationshipType RelType1 = RelationshipType.withName( "type1" );
		 protected internal readonly RelationshipType RelType2 = RelationshipType.withName( "type2" );
		 protected internal readonly string PropertyKey = "name";
		 protected internal readonly string OtherPropertyKey = "age";
		 protected internal KernelStatement State;
		 protected internal StorageReader StorageReader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") @Before public void before()
		 public virtual void Before()
		 {
			  Db = ( GraphDatabaseAPI ) CreateGraphDatabase();
			  DependencyResolver resolver = Db.DependencyResolver;
			  this.StorageReader = resolver.ResolveDependency( typeof( StorageEngine ) ).newReader();
			  this.State = new KernelStatement( null, null, StorageReader, LockTracer.NONE, null, new ClockContext(), EmptyVersionContextSupplier.EMPTY );
		 }

		 protected internal virtual IGraphDatabaseService CreateGraphDatabase()
		 {
			  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  Db.shutdown();
		 }

		 protected internal static Node CreateLabeledNode( IGraphDatabaseService db, IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( labels );
					foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( property.Key, property.Value );
					}
					tx.Success();
					return node;
			  }
		 }

		 protected internal virtual int LabelId( Label label )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					int id = Ktx().tokenRead().nodeLabel(label.Name());
					tx.Success();
					return id;
			  }
		 }

		 protected internal virtual int RelationshipTypeId( RelationshipType type )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					int id = Ktx().tokenRead().relationshipType(type.Name());
					tx.Success();
					return id;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected String relationshipType(int id) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 protected internal virtual string RelationshipType( int id )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					string name = Ktx().tokenRead().relationshipTypeName(id);
					tx.Success();
					return name;
			  }
		 }

		 protected internal virtual int PropertyKeyId( string propertyKey )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					int id = Ktx().tokenRead().propertyKey(propertyKey);
					tx.Success();
					return id;
			  }
		 }

		 protected internal virtual KernelTransaction Ktx()
		 {
			  DependencyResolver dependencyResolver = Db.DependencyResolver;
			  return dependencyResolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
		 }
	}

}