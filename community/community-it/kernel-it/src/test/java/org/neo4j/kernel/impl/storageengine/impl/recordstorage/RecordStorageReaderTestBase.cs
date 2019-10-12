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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage
{
	using After = org.junit.After;
	using Before = org.junit.Before;

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using ClockContext = Org.Neo4j.Kernel.Impl.Api.ClockContext;
	using KernelStatement = Org.Neo4j.Kernel.Impl.Api.KernelStatement;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

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

		 protected internal virtual GraphDatabaseService CreateGraphDatabase()
		 {
			  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  Db.shutdown();
		 }

		 protected internal static Node CreateLabeledNode( GraphDatabaseService db, IDictionary<string, object> properties, params Label[] labels )
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
//ORIGINAL LINE: protected String relationshipType(int id) throws org.neo4j.internal.kernel.api.exceptions.KernelException
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