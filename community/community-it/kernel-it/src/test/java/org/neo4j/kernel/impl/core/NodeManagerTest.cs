﻿using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.core
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using PlaceboTransaction = Org.Neo4j.Kernel.impl.coreapi.PlaceboTransaction;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.addToCollection;

	public class NodeManagerTest
	{
		 private GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void init()
		 public virtual void Init()
		 {
			  _db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stop()
		 public virtual void Stop()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getAllNodesIteratorShouldPickUpHigherIdsThanHighIdWhenStarted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getAllNodesIteratorShouldPickUpHigherIdsThanHighIdWhenStarted()
		 {
			  {
			  // GIVEN
					Transaction tx = _db.beginTx();
					_db.createNode();
					_db.createNode();
					tx.Success();
					tx.Close();
			  }

			  // WHEN iterator is started
			  Transaction transaction = _db.beginTx();
			  IEnumerator<Node> allNodes = _db.AllNodes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  allNodes.next();

			  // and WHEN another node is then added
			  Thread thread = new Thread(() =>
			  {
				Transaction newTx = _db.beginTx();
				assertThat( newTx, not( instanceOf( typeof( PlaceboTransaction ) ) ) );
				_db.createNode();
				newTx.success();
				newTx.close();
			  });
			  thread.Start();
			  thread.Join();

			  // THEN the new node is picked up by the iterator
			  assertThat( addToCollection( allNodes, new List<>() ).size(), @is(2) );
			  transaction.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getAllRelationshipsIteratorShouldPickUpHigherIdsThanHighIdWhenStarted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getAllRelationshipsIteratorShouldPickUpHigherIdsThanHighIdWhenStarted()
		 {
			  // GIVEN
			  Transaction tx = _db.beginTx();
			  CreateRelationshipAssumingTxWith( "key", 1 );
			  CreateRelationshipAssumingTxWith( "key", 2 );
			  tx.Success();
			  tx.Close();

			  // WHEN
			  tx = _db.beginTx();
			  IEnumerator<Relationship> allRelationships = _db.AllRelationships.GetEnumerator();

			  Thread thread = new Thread(() =>
			  {
				Transaction newTx = _db.beginTx();
				assertThat( newTx, not( instanceOf( typeof( PlaceboTransaction ) ) ) );
				CreateRelationshipAssumingTxWith( "key", 3 );
				newTx.success();
				newTx.close();
			  });
			  thread.Start();
			  thread.Join();

			  // THEN
			  assertThat( addToCollection( allRelationships, new List<>() ).size(), @is(3) );
			  tx.Success();
			  tx.Close();
		 }

		 private Relationship CreateRelationshipAssumingTxWith( string key, object value )
		 {
			  Node a = _db.createNode();
			  Node b = _db.createNode();
			  Relationship relationship = a.CreateRelationshipTo( b, RelationshipType.withName( "FOO" ) );
			  relationship.SetProperty( key, value );
			  return relationship;
		 }
	}

}