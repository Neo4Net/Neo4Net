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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Read_Fields.ANY_RELATIONSHIP_TYPE;

	public class RecordRelationshipScanCursorTest
	{
		 private const long RELATIONSHIP_ID = 1L;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 private NeoStores _neoStores;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _neoStores != null )
			  {
					_neoStores.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  StoreFactory storeFactory = StoreFactory;
			  _neoStores = storeFactory.OpenAllNeoStores( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveUsedRelationship()
		 public virtual void RetrieveUsedRelationship()
		 {
			  RelationshipStore relationshipStore = _neoStores.RelationshipStore;
			  CreateRelationshipRecord( RELATIONSHIP_ID, 1, relationshipStore, true );

			  using ( RecordRelationshipScanCursor cursor = CreateRelationshipCursor() )
			  {
					cursor.Single( RELATIONSHIP_ID );
					assertTrue( cursor.Next() );
					assertEquals( RELATIONSHIP_ID, cursor.EntityReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveUnusedRelationship()
		 public virtual void RetrieveUnusedRelationship()
		 {
			  RelationshipStore relationshipStore = _neoStores.RelationshipStore;
			  relationshipStore.HighId = 10;
			  CreateRelationshipRecord( RELATIONSHIP_ID, 1, relationshipStore, false );

			  using ( RecordRelationshipScanCursor cursor = CreateRelationshipCursor() )
			  {
					cursor.Single( RELATIONSHIP_ID );
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanAllInUseRelationships()
		 public virtual void ShouldScanAllInUseRelationships()
		 {
			  // given
			  RelationshipStore relationshipStore = _neoStores.RelationshipStore;
			  int count = 100;
			  relationshipStore.HighId = count;
			  ISet<long> expected = new HashSet<long>();
			  for ( long id = 0; id < count; id++ )
			  {
					bool inUse = Random.nextBoolean();
					CreateRelationshipRecord( id, 1, relationshipStore, inUse );
					if ( inUse )
					{
						 expected.Add( id );
					}
			  }

			  // when
			  AssertSeesRelationships( expected, ANY_RELATIONSHIP_TYPE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanAllInUseRelationshipsOfCertainType()
		 public virtual void ShouldScanAllInUseRelationshipsOfCertainType()
		 {
			  // given
			  RelationshipStore relationshipStore = _neoStores.RelationshipStore;
			  int count = 100;
			  relationshipStore.HighId = count;
			  ISet<long> expected = new HashSet<long>();
			  int theType = 1;
			  for ( long id = 0; id < count; id++ )
			  {
					bool inUse = Random.nextBoolean();
					int type = Random.Next( 3 );
					CreateRelationshipRecord( id, type, relationshipStore, inUse );
					if ( inUse && type == theType )
					{
						 expected.Add( id );
					}
			  }

			  // when
			  AssertSeesRelationships( expected, theType );
		 }

		 private void AssertSeesRelationships( ISet<long> expected, int type )
		 {
			  using ( RecordRelationshipScanCursor cursor = CreateRelationshipCursor() )
			  {
					cursor.Scan( type );
					while ( cursor.Next() )
					{
						 // then
						 assertTrue( cursor.ToString(), expected.remove(cursor.EntityReference()) );
					}
			  }
			  assertTrue( expected.Count == 0 );
		 }

		 private void CreateRelationshipRecord( long id, int type, RelationshipStore relationshipStore, bool used )
		 {
			 relationshipStore.UpdateRecord( ( new RelationshipRecord( id ) ).initialize( used, -1, 1, 2, type, -1, -1, -1, -1, true, true ) );
		 }

		 private StoreFactory StoreFactory
		 {
			 get
			 {
				  return new StoreFactory( Storage.directory().databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(Storage.fileSystem()), Storage.pageCache(), Storage.fileSystem(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			 }
		 }

		 private RecordRelationshipScanCursor CreateRelationshipCursor()
		 {
			  return new RecordRelationshipScanCursor( _neoStores.RelationshipStore );
		 }
	}

}