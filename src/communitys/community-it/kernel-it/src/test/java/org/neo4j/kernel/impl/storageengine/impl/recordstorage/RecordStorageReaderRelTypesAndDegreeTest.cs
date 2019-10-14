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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipDirection = Neo4Net.Storageengine.Api.RelationshipDirection;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using StorageRelationshipGroupCursor = Neo4Net.Storageengine.Api.StorageRelationshipGroupCursor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Read_Fields.ANY_RELATIONSHIP_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.TokenRead_Fields.NO_TOKEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.TestRelType.IN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.TestRelType.LOOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.TestRelType.OUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.OUTGOING;

	public class RecordStorageReaderRelTypesAndDegreeTest : RecordStorageReaderTestBase
	{
		 private const int RELATIONSHIPS_COUNT = 20;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 protected internal override GraphDatabaseService CreateGraphDatabase()
		 {
			  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.dense_node_threshold, RELATIONSHIPS_COUNT.ToString()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void degreesForDenseNodeWithPartiallyDeletedRelGroupChain()
		 public virtual void DegreesForDenseNodeWithPartiallyDeletedRelGroupChain()
		 {
			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain();

			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( IN );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( OUT );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( LOOP );

			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( IN, OUT );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( OUT, LOOP );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( IN, LOOP );

			  TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( IN, OUT, LOOP );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void degreesForDenseNodeWithPartiallyDeletedRelChains()
		 public virtual void DegreesForDenseNodeWithPartiallyDeletedRelChains()
		 {
			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( false, false, false );

			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( true, false, false );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( false, true, false );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( false, false, true );

			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( true, true, false );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( true, true, true );
			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( true, false, true );

			  TestDegreesForDenseNodeWithPartiallyDeletedRelChains( true, true, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void degreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain()
		 public virtual void DegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain()
		 {
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain();

			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( IN );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( OUT );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( LOOP );

			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( IN, OUT );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( IN, LOOP );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( OUT, LOOP );

			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( IN, OUT, LOOP );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void degreeByDirectionForDenseNodeWithPartiallyDeletedRelChains()
		 public virtual void DegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains()
		 {
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( false, false, false );

			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( true, false, false );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( false, true, false );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( false, false, true );

			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( true, true, false );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( true, true, true );
			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( true, false, true );

			  TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( true, true, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void degreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain()
		 public virtual void DegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain()
		 {
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain();

			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( IN );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( OUT );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( LOOP );

			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( IN, OUT );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( OUT, LOOP );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( IN, LOOP );

			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( IN, OUT, LOOP );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void degreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains()
		 public virtual void DegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains()
		 {
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( false, false, false );

			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( true, false, false );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( false, true, false );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( false, false, true );

			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( true, true, false );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( true, true, true );
			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( true, false, true );

			  TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( true, true, true );
		 }

		 private void TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelGroupChain( params TestRelType[] typesToDelete )
		 {
			  int inRelCount = RandomRelCount();
			  int outRelCount = RandomRelCount();
			  int loopRelCount = RandomRelCount();

			  long nodeId = CreateNode( inRelCount, outRelCount, loopRelCount );
			  StorageNodeCursor cursor = NewCursor( nodeId );

			  foreach ( TestRelType type in typesToDelete )
			  {
					MarkRelGroupNotInUse( nodeId, type );
					switch ( type )
					{
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.In:
						 inRelCount = 0;
						 break;
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.Out:
						 outRelCount = 0;
						 break;
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.Loop:
						 loopRelCount = 0;
						 break;
					default:
						 throw new System.ArgumentException( "Unknown type: " + type );
					}
			  }

			  assertEquals( outRelCount + loopRelCount, DegreeForDirection( cursor, OUTGOING ) );
			  assertEquals( inRelCount + loopRelCount, DegreeForDirection( cursor, INCOMING ) );
			  assertEquals( inRelCount + outRelCount + loopRelCount, DegreeForDirection( cursor, RelationshipDirection.LOOP ) );
		 }

		 private void TestDegreeByDirectionForDenseNodeWithPartiallyDeletedRelChains( bool modifyInChain, bool modifyOutChain, bool modifyLoopChain )
		 {
			  int inRelCount = RandomRelCount();
			  int outRelCount = RandomRelCount();
			  int loopRelCount = RandomRelCount();

			  long nodeId = CreateNode( inRelCount, outRelCount, loopRelCount );
			  StorageNodeCursor cursor = NewCursor( nodeId );

			  if ( modifyInChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, IN );
			  }
			  if ( modifyOutChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, OUT );
			  }
			  if ( modifyLoopChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, LOOP );
			  }

			  assertEquals( outRelCount + loopRelCount, DegreeForDirection( cursor, OUTGOING ) );
			  assertEquals( inRelCount + loopRelCount, DegreeForDirection( cursor, INCOMING ) );
			  assertEquals( inRelCount + outRelCount + loopRelCount, DegreeForDirection( cursor, RelationshipDirection.LOOP ) );
		 }

		 private int DegreeForDirection( StorageNodeCursor cursor, RelationshipDirection direction )
		 {
			  return DegreeForDirectionAndType( cursor, direction, ANY_RELATIONSHIP_TYPE );
		 }

		 private int DegreeForDirectionAndType( StorageNodeCursor cursor, RelationshipDirection direction, int relType )
		 {
			  int degree = 0;
			  using ( StorageRelationshipGroupCursor groups = StorageReader.allocateRelationshipGroupCursor() )
			  {
					groups.Init( cursor.EntityReference(), cursor.RelationshipGroupReference() );
					while ( groups.Next() )
					{
						 if ( relType == ANY_RELATIONSHIP_TYPE || relType == groups.Type() )
						 {
							  switch ( direction )
							  {
							  case RelationshipDirection.OUTGOING:
									degree += groups.OutgoingCount() + groups.LoopCount();
									break;
							  case RelationshipDirection.INCOMING:
									degree += groups.IncomingCount() + groups.LoopCount();
									break;
							  case RelationshipDirection.LOOP:
									degree += groups.OutgoingCount() + groups.IncomingCount() + groups.LoopCount();
									break;
							  default:
									throw new System.ArgumentException( direction.name() );
							  }
						 }
					}
			  }
			  return degree;
		 }

		 private void TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelGroupChain( params TestRelType[] typesToDelete )
		 {
			  int inRelCount = RandomRelCount();
			  int outRelCount = RandomRelCount();
			  int loopRelCount = RandomRelCount();

			  long nodeId = CreateNode( inRelCount, outRelCount, loopRelCount );
			  StorageNodeCursor cursor = NewCursor( nodeId );

			  foreach ( TestRelType type in typesToDelete )
			  {
					MarkRelGroupNotInUse( nodeId, type );
					switch ( type )
					{
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.In:
						 inRelCount = 0;
						 break;
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.Out:
						 outRelCount = 0;
						 break;
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.Loop:
						 loopRelCount = 0;
						 break;
					default:
						 throw new System.ArgumentException( "Unknown type: " + type );
					}
			  }

			  assertEquals( 0, DegreeForDirectionAndType( cursor, OUTGOING, RelTypeId( IN ) ) );
			  assertEquals( outRelCount, DegreeForDirectionAndType( cursor, OUTGOING, RelTypeId( OUT ) ) );
			  assertEquals( loopRelCount, DegreeForDirectionAndType( cursor, OUTGOING, RelTypeId( LOOP ) ) );

			  assertEquals( 0, DegreeForDirectionAndType( cursor, INCOMING, RelTypeId( OUT ) ) );
			  assertEquals( inRelCount, DegreeForDirectionAndType( cursor, INCOMING, RelTypeId( IN ) ) );
			  assertEquals( loopRelCount, DegreeForDirectionAndType( cursor, INCOMING, RelTypeId( LOOP ) ) );

			  assertEquals( inRelCount, DegreeForDirectionAndType( cursor, RelationshipDirection.LOOP, RelTypeId( IN ) ) );
			  assertEquals( outRelCount, DegreeForDirectionAndType( cursor, RelationshipDirection.LOOP, RelTypeId( OUT ) ) );
			  assertEquals( loopRelCount, DegreeForDirectionAndType( cursor, RelationshipDirection.LOOP, RelTypeId( LOOP ) ) );
		 }

		 private void TestDegreeByDirectionAndTypeForDenseNodeWithPartiallyDeletedRelChains( bool modifyInChain, bool modifyOutChain, bool modifyLoopChain )
		 {
			  int inRelCount = RandomRelCount();
			  int outRelCount = RandomRelCount();
			  int loopRelCount = RandomRelCount();

			  long nodeId = CreateNode( inRelCount, outRelCount, loopRelCount );
			  StorageNodeCursor cursor = NewCursor( nodeId );

			  if ( modifyInChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, IN );
			  }
			  if ( modifyOutChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, OUT );
			  }
			  if ( modifyLoopChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, LOOP );
			  }

			  assertEquals( 0, DegreeForDirectionAndType( cursor, OUTGOING, RelTypeId( IN ) ) );
			  assertEquals( outRelCount, DegreeForDirectionAndType( cursor, OUTGOING, RelTypeId( OUT ) ) );
			  assertEquals( loopRelCount, DegreeForDirectionAndType( cursor, OUTGOING, RelTypeId( LOOP ) ) );

			  assertEquals( 0, DegreeForDirectionAndType( cursor, INCOMING, RelTypeId( OUT ) ) );
			  assertEquals( inRelCount, DegreeForDirectionAndType( cursor, INCOMING, RelTypeId( IN ) ) );
			  assertEquals( loopRelCount, DegreeForDirectionAndType( cursor, INCOMING, RelTypeId( LOOP ) ) );

			  assertEquals( inRelCount, DegreeForDirectionAndType( cursor, RelationshipDirection.LOOP, RelTypeId( IN ) ) );
			  assertEquals( outRelCount, DegreeForDirectionAndType( cursor, RelationshipDirection.LOOP, RelTypeId( OUT ) ) );
			  assertEquals( loopRelCount, DegreeForDirectionAndType( cursor, RelationshipDirection.LOOP, RelTypeId( LOOP ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipTypesForDenseNodeWithPartiallyDeletedRelGroupChain()
		 public virtual void RelationshipTypesForDenseNodeWithPartiallyDeletedRelGroupChain()
		 {
			  TestRelationshipTypesForDenseNode( this.noNodeChange, asSet( IN, OUT, LOOP ) );

			  TestRelationshipTypesForDenseNode( nodeId => markRelGroupNotInUse( nodeId, IN ), asSet( OUT, LOOP ) );
			  TestRelationshipTypesForDenseNode( nodeId => markRelGroupNotInUse( nodeId, OUT ), asSet( IN, LOOP ) );
			  TestRelationshipTypesForDenseNode( nodeId => markRelGroupNotInUse( nodeId, LOOP ), asSet( IN, OUT ) );

			  TestRelationshipTypesForDenseNode( nodeId => markRelGroupNotInUse( nodeId, IN, OUT ), asSet( LOOP ) );
			  TestRelationshipTypesForDenseNode( nodeId => markRelGroupNotInUse( nodeId, IN, LOOP ), asSet( OUT ) );
			  TestRelationshipTypesForDenseNode( nodeId => markRelGroupNotInUse( nodeId, OUT, LOOP ), asSet( IN ) );

			  TestRelationshipTypesForDenseNode( nodeId => markRelGroupNotInUse( nodeId, IN, OUT, LOOP ), emptySet() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipTypesForDenseNodeWithPartiallyDeletedRelChains()
		 public virtual void RelationshipTypesForDenseNodeWithPartiallyDeletedRelChains()
		 {
			  TestRelationshipTypesForDenseNode( this.markRandomRelsNotInUse, asSet( IN, OUT, LOOP ) );
		 }

		 private void TestRelationshipTypesForDenseNode( System.Action<long> nodeChanger, ISet<TestRelType> expectedTypes )
		 {
			  int inRelCount = RandomRelCount();
			  int outRelCount = RandomRelCount();
			  int loopRelCount = RandomRelCount();

			  long nodeId = CreateNode( inRelCount, outRelCount, loopRelCount );
			  nodeChanger( nodeId );

			  StorageNodeCursor cursor = NewCursor( nodeId );

			  assertEquals( expectedTypes, RelTypes( cursor ) );
		 }

		 private ISet<TestRelType> RelTypes( StorageNodeCursor cursor )
		 {
			  ISet<TestRelType> types = new HashSet<TestRelType>();
			  using ( StorageRelationshipGroupCursor groups = StorageReader.allocateRelationshipGroupCursor() )
			  {
					groups.Init( cursor.EntityReference(), cursor.RelationshipGroupReference() );
					while ( groups.Next() )
					{
						 types.Add( RelTypeForId( groups.Type() ) );
					}
			  }
			  return types;
		 }

		 private void TestDegreesForDenseNodeWithPartiallyDeletedRelGroupChain( params TestRelType[] typesToDelete )
		 {
			  int inRelCount = RandomRelCount();
			  int outRelCount = RandomRelCount();
			  int loopRelCount = RandomRelCount();

			  long nodeId = CreateNode( inRelCount, outRelCount, loopRelCount );
			  StorageNodeCursor cursor = NewCursor( nodeId );

			  foreach ( TestRelType type in typesToDelete )
			  {
					MarkRelGroupNotInUse( nodeId, type );
					switch ( type )
					{
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.In:
						 inRelCount = 0;
						 break;
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.Out:
						 outRelCount = 0;
						 break;
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TestRelType.Loop:
						 loopRelCount = 0;
						 break;
					default:
						 throw new System.ArgumentException( "Unknown type: " + type );
					}
			  }

			  ISet<TestDegreeItem> expectedDegrees = new HashSet<TestDegreeItem>();
			  if ( outRelCount != 0 )
			  {
					expectedDegrees.Add( new TestDegreeItem( RelTypeId( OUT ), outRelCount, 0 ) );
			  }
			  if ( inRelCount != 0 )
			  {
					expectedDegrees.Add( new TestDegreeItem( RelTypeId( IN ), 0, inRelCount ) );
			  }
			  if ( loopRelCount != 0 )
			  {
					expectedDegrees.Add( new TestDegreeItem( RelTypeId( LOOP ), loopRelCount, loopRelCount ) );
			  }

			  ISet<TestDegreeItem> actualDegrees = Degrees( cursor );

			  assertEquals( expectedDegrees, actualDegrees );
		 }

		 private void TestDegreesForDenseNodeWithPartiallyDeletedRelChains( bool modifyInChain, bool modifyOutChain, bool modifyLoopChain )
		 {
			  int inRelCount = RandomRelCount();
			  int outRelCount = RandomRelCount();
			  int loopRelCount = RandomRelCount();

			  long nodeId = CreateNode( inRelCount, outRelCount, loopRelCount );
			  StorageNodeCursor cursor = NewCursor( nodeId );

			  if ( modifyInChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, IN );
			  }
			  if ( modifyOutChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, OUT );
			  }
			  if ( modifyLoopChain )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, LOOP );
			  }

			  ISet<TestDegreeItem> expectedDegrees = new HashSet<TestDegreeItem>( asList( new TestDegreeItem( RelTypeId( OUT ), outRelCount, 0 ), new TestDegreeItem( RelTypeId( IN ), 0, inRelCount ), new TestDegreeItem( RelTypeId( LOOP ), loopRelCount, loopRelCount ) ) );

			  ISet<TestDegreeItem> actualDegrees = Degrees( cursor );

			  assertEquals( expectedDegrees, actualDegrees );
		 }

		 private ISet<TestDegreeItem> Degrees( StorageNodeCursor nodeCursor )
		 {
			  ISet<TestDegreeItem> degrees = new HashSet<TestDegreeItem>();
			  using ( StorageRelationshipGroupCursor groups = StorageReader.allocateRelationshipGroupCursor() )
			  {
					groups.Init( nodeCursor.EntityReference(), nodeCursor.RelationshipGroupReference() );
					while ( groups.Next() )
					{
						 degrees.Add( new TestDegreeItem( groups.Type(), groups.OutgoingCount() + groups.LoopCount(), groups.IncomingCount() + groups.LoopCount() ) );
					}
			  }
			  return degrees;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.neo4j.storageengine.api.StorageNodeCursor newCursor(long nodeId)
		 private StorageNodeCursor NewCursor( long nodeId )
		 {
			  StorageNodeCursor nodeCursor = StorageReader.allocateNodeCursor();
			  nodeCursor.Single( nodeId );
			  assertTrue( nodeCursor.Next() );
			  return nodeCursor;
		 }

		 private void NoNodeChange( long nodeId )
		 {
		 }

		 private void MarkRandomRelsNotInUse( long nodeId )
		 {
			  foreach ( TestRelType type in Enum.GetValues( typeof( TestRelType ) ) )
			  {
					MarkRandomRelsInGroupNotInUse( nodeId, type );
			  }
		 }

		 private void MarkRandomRelsInGroupNotInUse( long nodeId, TestRelType type )
		 {
			  NodeRecord node = GetNodeRecord( nodeId );
			  assertTrue( node.Dense );

			  long relGroupId = node.NextRel;
			  while ( relGroupId != NO_NEXT_RELATIONSHIP.intValue() )
			  {
					RelationshipGroupRecord relGroup = GetRelGroupRecord( relGroupId );

					if ( type == RelTypeForId( relGroup.Type ) )
					{
						 MarkRandomRelsInChainNotInUse( relGroup.FirstOut );
						 MarkRandomRelsInChainNotInUse( relGroup.FirstIn );
						 MarkRandomRelsInChainNotInUse( relGroup.FirstLoop );
						 return;
					}

					relGroupId = relGroup.Next;
			  }

			  throw new System.InvalidOperationException( "No relationship group with type: " + type + " found" );
		 }

		 private void MarkRandomRelsInChainNotInUse( long relId )
		 {
			  if ( relId != NO_NEXT_RELATIONSHIP.intValue() )
			  {
					RelationshipRecord record = GetRelRecord( relId );

					bool shouldBeMarked = Random.nextBoolean();
					if ( shouldBeMarked )
					{
						 record.InUse = false;
						 Update( record );
					}

					MarkRandomRelsInChainNotInUse( record.FirstNextRel );
					bool isLoopRelationship = record.FirstNextRel == record.SecondNextRel;
					if ( !isLoopRelationship )
					{
						 MarkRandomRelsInChainNotInUse( record.SecondNextRel );
					}
			  }
		 }

		 private void MarkRelGroupNotInUse( long nodeId, params TestRelType[] types )
		 {
			  NodeRecord node = GetNodeRecord( nodeId );
			  assertTrue( node.Dense );

			  ISet<TestRelType> typesToRemove = asSet( types );

			  long relGroupId = node.NextRel;
			  while ( relGroupId != NO_NEXT_RELATIONSHIP.intValue() )
			  {
					RelationshipGroupRecord relGroup = GetRelGroupRecord( relGroupId );
					TestRelType type = RelTypeForId( relGroup.Type );

					if ( typesToRemove.Contains( type ) )
					{
						 relGroup.InUse = false;
						 Update( relGroup );
					}

					relGroupId = relGroup.Next;
			  }
		 }

		 private int RelTypeId( TestRelType type )
		 {
			  int id = RelationshipTypeId( type );
			  assertNotEquals( NO_TOKEN, id );
			  return id;
		 }

		 private long CreateNode( int inRelCount, int outRelCount, int loopRelCount )
		 {
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < inRelCount; i++ )
					{
						 Node start = Db.createNode();
						 start.CreateRelationshipTo( node, IN );
					}
					for ( int i = 0; i < outRelCount; i++ )
					{
						 Node end = Db.createNode();
						 node.CreateRelationshipTo( end, OUT );
					}
					for ( int i = 0; i < loopRelCount; i++ )
					{
						 node.CreateRelationshipTo( node, LOOP );
					}
					tx.Success();
			  }
			  return node.Id;
		 }

		 private TestRelType RelTypeForId( int id )
		 {
			  try
			  {
					return Enum.Parse( typeof( TestRelType ), RelationshipType( id ) );
			  }
			  catch ( KernelException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private static R GetRecord<R>( RecordStore<R> store, long id ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return RecordStore.getRecord( store, id, RecordLoad.FORCE );
		 }

		 private NodeRecord GetNodeRecord( long id )
		 {
			  return GetRecord( ResolveNeoStores().NodeStore, id );
		 }

		 private RelationshipRecord GetRelRecord( long id )
		 {
			  return GetRecord( ResolveNeoStores().RelationshipStore, id );
		 }

		 private RelationshipGroupRecord GetRelGroupRecord( long id )
		 {
			  return GetRecord( ResolveNeoStores().RelationshipGroupStore, id );
		 }

		 private void Update( RelationshipGroupRecord record )
		 {
			  ResolveNeoStores().RelationshipGroupStore.updateRecord(record);
		 }

		 private void Update( RelationshipRecord record )
		 {
			  ResolveNeoStores().RelationshipStore.updateRecord(record);
		 }

		 private NeoStores ResolveNeoStores()
		 {
			  DependencyResolver resolver = Db.DependencyResolver;
			  RecordStorageEngine storageEngine = resolver.ResolveDependency( typeof( RecordStorageEngine ) );
			  return storageEngine.TestAccessNeoStores();
		 }

		 private int RandomRelCount()
		 {
			  return RELATIONSHIPS_COUNT + Random.Next( 20 );
		 }

	}

}