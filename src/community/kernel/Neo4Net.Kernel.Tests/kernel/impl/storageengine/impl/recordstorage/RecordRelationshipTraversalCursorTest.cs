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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using RelationshipDirection = Neo4Net.Storageengine.Api.RelationshipDirection;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.LOOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.OUTGOING;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RecordRelationshipTraversalCursorTest
	public class RecordRelationshipTraversalCursorTest
	{
		 private const long FIRST_OWNING_NODE = 1;
		 private const long SECOND_OWNING_NODE = 2;
		 private const int TYPE = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());

		 private NeoStores _neoStores;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.neo4j.storageengine.api.RelationshipDirection direction;
		 public RelationshipDirection Direction;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(value = 1) public boolean dense;
		 public bool Dense;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static Iterable<Object[]> parameters()
		 public static IEnumerable<object[]> Parameters()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { LOOP, false },
				  new object[] { LOOP, true },
				  new object[] { OUTGOING, false },
				  new object[] { OUTGOING, true },
				  new object[] { INCOMING, false },
				  new object[] { INCOMING, true }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupStores()
		 public virtual void SetupStores()
		 {
			  DatabaseLayout storeLayout = Storage.directory().databaseLayout();
			  Config config = Config.defaults( pagecache_memory, "8m" );
			  PageCache pageCache = Storage.pageCache();
			  FileSystemAbstraction fs = Storage.fileSystem();
			  DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fs );
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  StoreFactory storeFactory = new StoreFactory( storeLayout, config, idGeneratorFactory, pageCache, fs, logProvider, EMPTY );
			  _neoStores = storeFactory.OpenAllNeoStores( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutDownStores()
		 public virtual void ShutDownStores()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveNodeRelationships()
		 public virtual void RetrieveNodeRelationships()
		 {
			  CreateNodeRelationships();

			  using ( RecordRelationshipTraversalCursor cursor = NodeRelationshipCursor )
			  {
					cursor.Init( FIRST_OWNING_NODE, 1L );
					assertTrue( cursor.Next() );

					cursor.Init( FIRST_OWNING_NODE, 2 );
					assertTrue( cursor.Next() );

					cursor.Init( FIRST_OWNING_NODE, 3 );
					assertTrue( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveUsedRelationshipChain()
		 public virtual void RetrieveUsedRelationshipChain()
		 {
			  CreateRelationshipChain( 4 );
			  long expectedNodeId = 1;
			  using ( RecordRelationshipTraversalCursor cursor = NodeRelationshipCursor )
			  {
					cursor.Init( FIRST_OWNING_NODE, 1 );
					while ( cursor.Next() )
					{
						 assertEquals( "Should load next relationship in a sequence", expectedNodeId++, cursor.EntityReference() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveRelationshipChainWithUnusedLink()
		 public virtual void RetrieveRelationshipChainWithUnusedLink()
		 {
			  _neoStores.RelationshipStore.HighId = 10;
			  CreateRelationshipChain( 4 );
			  UnUseRecord( 3 );
			  int[] expectedRelationshipIds = new int[]{ 1, 2, 4 };
			  int relationshipIndex = 0;
			  using ( RecordRelationshipTraversalCursor cursor = NodeRelationshipCursor )
			  {
					cursor.Init( FIRST_OWNING_NODE, 1 );
					while ( cursor.Next() )
					{
						 assertEquals( "Should load next relationship in a sequence", expectedRelationshipIds[relationshipIndex++], cursor.EntityReference() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDenseNodeWithNoRelationships()
		 public virtual void ShouldHandleDenseNodeWithNoRelationships()
		 {
			  // This can actually happen, since we upgrade sparse node --> dense node when creating relationships,
			  // but we don't downgrade dense --> sparse when we delete relationships. So if we have a dense node
			  // which no longer has relationships, there was this assumption that we could just call getRecord
			  // on the NodeRecord#getNextRel() value. Although that value could actually be -1
			  using ( RecordRelationshipTraversalCursor cursor = NodeRelationshipCursor )
			  {
					// WHEN
					cursor.Init( FIRST_OWNING_NODE, NO_NEXT_RELATIONSHIP.intValue() );

					// THEN
					assertFalse( cursor.Next() );
			  }
		 }

		 private void CreateNodeRelationships()
		 {
			  RelationshipStore relationshipStore = _neoStores.RelationshipStore;
			  if ( Dense )
			  {
					RecordStore<RelationshipGroupRecord> relationshipGroupStore = _neoStores.RelationshipGroupStore;
					relationshipGroupStore.UpdateRecord( CreateRelationshipGroup( 1, 1 ) );
					relationshipGroupStore.UpdateRecord( CreateRelationshipGroup( 2, 2 ) );
					relationshipGroupStore.UpdateRecord( CreateRelationshipGroup( 3, 3 ) );
			  }

			  relationshipStore.UpdateRecord( CreateRelationship( 1, NO_NEXT_RELATIONSHIP.intValue() ) );
			  relationshipStore.UpdateRecord( CreateRelationship( 2, NO_NEXT_RELATIONSHIP.intValue() ) );
			  relationshipStore.UpdateRecord( CreateRelationship( 3, NO_NEXT_RELATIONSHIP.intValue() ) );
		 }

		 private void UnUseRecord( long recordId )
		 {
			  RelationshipStore relationshipStore = _neoStores.RelationshipStore;
			  RelationshipRecord relationshipRecord = relationshipStore.GetRecord( recordId, new RelationshipRecord( -1 ), RecordLoad.FORCE );
			  relationshipRecord.InUse = false;
			  relationshipStore.UpdateRecord( relationshipRecord );
		 }

		 private RelationshipGroupRecord CreateRelationshipGroup( long id, long relationshipId )
		 {
			  return new RelationshipGroupRecord( id, TYPE, GetFirstOut( relationshipId ), GetFirstIn( relationshipId ), GetFirstLoop( relationshipId ), FIRST_OWNING_NODE, true );
		 }

		 private long GetFirstLoop( long firstLoop )
		 {
			  return Direction == LOOP ? firstLoop : Record.NO_NEXT_RELATIONSHIP.intValue();
		 }

		 private long GetFirstIn( long firstIn )
		 {
			  return Direction == INCOMING ? firstIn : Record.NO_NEXT_RELATIONSHIP.intValue();
		 }

		 private long GetFirstOut( long firstOut )
		 {
			  return Direction == OUTGOING ? firstOut : Record.NO_NEXT_RELATIONSHIP.intValue();
		 }

		 private void CreateRelationshipChain( int recordsInChain )
		 {
			  RelationshipStore relationshipStore = _neoStores.RelationshipStore;
			  for ( int i = 1; i < recordsInChain; i++ )
			  {
					relationshipStore.UpdateRecord( CreateRelationship( i, i + 1 ) );
			  }
			  relationshipStore.UpdateRecord( CreateRelationship( recordsInChain, NO_NEXT_RELATIONSHIP.intValue() ) );
			  if ( Dense )
			  {
					RecordStore<RelationshipGroupRecord> relationshipGroupStore = _neoStores.RelationshipGroupStore;
					for ( int i = 1; i < recordsInChain; i++ )
					{
						 relationshipGroupStore.UpdateRecord( CreateRelationshipGroup( i, i ) );
					}
					relationshipGroupStore.UpdateRecord( CreateRelationshipGroup( recordsInChain, NO_NEXT_RELATIONSHIP.intValue() ) );
			  }
		 }

		 private RelationshipRecord CreateRelationship( long id, long nextRelationship )
		 {
			  return new RelationshipRecord( id, true, FirstNode, SecondNode, TYPE, NO_NEXT_RELATIONSHIP.intValue(), nextRelationship, NO_NEXT_RELATIONSHIP.intValue(), nextRelationship, false, false );
		 }

		 private long SecondNode
		 {
			 get
			 {
				  return FirstNode == FIRST_OWNING_NODE ? SECOND_OWNING_NODE : FIRST_OWNING_NODE;
			 }
		 }

		 private long FirstNode
		 {
			 get
			 {
				  return Direction == OUTGOING ? FIRST_OWNING_NODE : SECOND_OWNING_NODE;
			 }
		 }

		 private RecordRelationshipTraversalCursor NodeRelationshipCursor
		 {
			 get
			 {
				  return new RecordRelationshipTraversalCursor( _neoStores.RelationshipStore, _neoStores.RelationshipGroupStore );
			 }
		 }
	}

}