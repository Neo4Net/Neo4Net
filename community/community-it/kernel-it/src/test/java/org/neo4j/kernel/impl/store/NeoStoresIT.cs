﻿/*
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
namespace Org.Neo4j.Kernel.impl.store
{
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using Race = Org.Neo4j.Test.Race;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	public class NeoStoresIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, "1");
		 public static readonly DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.dense_node_threshold, "1");

		 private static readonly RelationshipType _friend = RelationshipType.withName( "FRIEND" );

		 private const string LONG_STRING_VALUE = "ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALA"
					+
					"ALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALALONG!!";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteOutTheDynamicChainBeforeUpdatingThePropertyRecord() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteOutTheDynamicChainBeforeUpdatingThePropertyRecord()
		 {
			  Race race = new Race();
			  long[] latestNodeId = new long[1];
			  AtomicLong writes = new AtomicLong();
			  AtomicLong reads = new AtomicLong();
			  long endTime = currentTimeMillis() + SECONDS.toMillis(2);
			  race.WithEndCondition( () => (writes.get() > 100 && reads.get() > 10_000) || currentTimeMillis() > endTime );
			  race.AddContestant(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Node node = Db.createNode();
					 latestNodeId[0] = node.Id;
					 node.setProperty( "largeProperty", LONG_STRING_VALUE );
					 tx.success();
				}
				writes.incrementAndGet();
			  });
			  race.AddContestant(() =>
			  {
				try
				{
					using ( Transaction tx = Db.GraphDatabaseAPI.beginTx() )
					{
						 Node node = Db.GraphDatabaseAPI.getNodeById( latestNodeId[0] );
						 foreach ( string propertyKey in node.PropertyKeys )
						 {
							  node.getProperty( propertyKey );
						 }
						 tx.success();
					}
				}
				catch ( NotFoundException )
				{
					 // This will catch nodes not found (expected) and also PropertyRecords not found (shouldn't happen
					 // but handled in shouldWriteOutThePropertyRecordBeforeReferencingItFromANodeRecord)
				}
				reads.incrementAndGet();
			  });
			  race.Go();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteOutThePropertyRecordBeforeReferencingItFromANodeRecord() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteOutThePropertyRecordBeforeReferencingItFromANodeRecord()
		 {
			  Race race = new Race();
			  long[] latestNodeId = new long[1];
			  AtomicLong writes = new AtomicLong();
			  AtomicLong reads = new AtomicLong();
			  long endTime = currentTimeMillis() + SECONDS.toMillis(2);
			  race.WithEndCondition( () => (writes.get() > 100 && reads.get() > 10_000) || currentTimeMillis() > endTime );
			  race.AddContestant(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Node node = Db.createNode();
					 latestNodeId[0] = node.Id;
					 node.setProperty( "largeProperty", LONG_STRING_VALUE );
					 tx.success();
				}
				writes.incrementAndGet();
			  });
			  race.AddContestant(() =>
			  {
				try
				{
					using ( Transaction tx = Db.GraphDatabaseAPI.beginTx() )
					{
						 Node node = Db.GraphDatabaseAPI.getNodeById( latestNodeId[0] );
   
						 foreach ( string propertyKey in node.PropertyKeys )
						 {
							  node.getProperty( propertyKey );
						 }
						 tx.success();
					}
				}
				catch ( NotFoundException e )
				{
					 if ( Exceptions.contains( e, typeof( InvalidRecordException ) ) )
					 {
						  throw e;
					 }
				}
				reads.incrementAndGet();
			  });
			  race.Go();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteOutThePropertyRecordBeforeReferencingItFromARelationshipRecord() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteOutThePropertyRecordBeforeReferencingItFromARelationshipRecord()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long node1Id;
			  long node1Id;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long node2Id;
			  long node2Id;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node1 = Db.createNode();
					node1Id = node1.Id;

					Node node2 = Db.createNode();
					node2Id = node2.Id;

					tx.Success();
			  }

			  Race race = new Race();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] latestRelationshipId = new long[1];
			  long[] latestRelationshipId = new long[1];
			  AtomicLong writes = new AtomicLong();
			  AtomicLong reads = new AtomicLong();
			  long endTime = currentTimeMillis() + SECONDS.toMillis(2);
			  race.WithEndCondition( () => (writes.get() > 100 && reads.get() > 10_000) || currentTimeMillis() > endTime );
			  race.AddContestant(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Node node1 = Db.GraphDatabaseAPI.getNodeById( node1Id );
					 Node node2 = Db.GraphDatabaseAPI.getNodeById( node2Id );

					 Relationship rel = node1.createRelationshipTo( node2, _friend );
					 latestRelationshipId[0] = rel.Id;
					 rel.setProperty( "largeProperty", LONG_STRING_VALUE );

					 tx.Success();
				}
				writes.incrementAndGet();
			  });
			  race.AddContestant(() =>
			  {
				try
				{
					using ( Transaction tx = Db.GraphDatabaseAPI.beginTx() )
					{
						 Relationship rel = Db.GraphDatabaseAPI.getRelationshipById( latestRelationshipId[0] );
   
						 foreach ( string propertyKey in rel.PropertyKeys )
						 {
							  rel.getProperty( propertyKey );
						 }
						 tx.Success();
					}
				}
				catch ( NotFoundException e )
				{
					 if ( Exceptions.contains( e, typeof( InvalidRecordException ) ) )
					 {
						  throw e;
					 }
				}
				reads.incrementAndGet();
			  });
			  race.Go();
		 }
	}

}