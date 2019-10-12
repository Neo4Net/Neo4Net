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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using InOrder = org.mockito.InOrder;

	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Loaders = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.Loaders;
	using RelationshipGroupGetter = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RelationshipGroupGetter;
	using RelationshipGroupPosition = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RelationshipGroupGetter.RelationshipGroupPosition;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using Org.Neo4j.Kernel.impl.store;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using StoreType = Org.Neo4j.Kernel.impl.store.StoreType;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;
	using Org.Neo4j.@unsafe.Batchinsert.@internal;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;

	public class RelationshipGroupGetterTest
	{
		private bool InstanceFieldsInitialized = false;

		public RelationshipGroupGetterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fs );
			RuleChain = RuleChain.outerRule( _fs ).around( _testDirectory ).around( _pageCache );
		}

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCache = new PageCacheRule();
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory).around(pageCache);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbortLoadingGroupChainIfComeTooFar()
		 public virtual void ShouldAbortLoadingGroupChainIfComeTooFar()
		 {
			  // GIVEN a node with relationship group chain 2-->4-->10-->23
			  LogProvider logProvider = NullLogProvider.Instance;
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(_fs.get()), _pageCache.getPageCache(_fs.get()), _fs.get(), logProvider, EmptyVersionContextSupplier.EMPTY );
			  using ( NeoStores stores = storeFactory.OpenNeoStores( true, StoreType.RELATIONSHIP_GROUP ) )
			  {
					RecordStore<RelationshipGroupRecord> store = spy( stores.RelationshipGroupStore );

					RelationshipGroupRecord group2 = Group( 0, 2 );
					RelationshipGroupRecord group4 = Group( 1, 4 );
					RelationshipGroupRecord group10 = Group( 2, 10 );
					RelationshipGroupRecord group23 = Group( 3, 23 );
					Link( group2, group4, group10, group23 );
					store.UpdateRecord( group2 );
					store.UpdateRecord( group4 );
					store.UpdateRecord( group10 );
					store.UpdateRecord( group23 );
					RelationshipGroupGetter groupGetter = new RelationshipGroupGetter( store );
					NodeRecord node = new NodeRecord( 0, true, group2.Id, -1 );

					// WHEN trying to find relationship group 7
					RecordAccess<RelationshipGroupRecord, int> access = new DirectRecordAccess<RelationshipGroupRecord, int>( store, Loaders.relationshipGroupLoader( store ) );
					RelationshipGroupGetter.RelationshipGroupPosition result = groupGetter.GetRelationshipGroup( node, 7, access );

					// THEN only groups 2, 4 and 10 should have been loaded
					InOrder verification = inOrder( store );
					verification.verify( store ).getRecord( eq( group2.Id ), any( typeof( RelationshipGroupRecord ) ), any( typeof( RecordLoad ) ) );
					verification.verify( store ).getRecord( eq( group4.Id ), any( typeof( RelationshipGroupRecord ) ), any( typeof( RecordLoad ) ) );
					verification.verify( store ).getRecord( eq( group10.Id ), any( typeof( RelationshipGroupRecord ) ), any( typeof( RecordLoad ) ) );
					verification.verify( store, never() ).getRecord(eq(group23.Id), any(typeof(RelationshipGroupRecord)), any(typeof(RecordLoad)));

					// it should also be reported as not found
					assertNull( result.Group() );
					// with group 4 as closes previous one
					assertEquals( group4, result.ClosestPrevious().forReadingData() );
			  }
		 }

		 private static void Link( params RelationshipGroupRecord[] groups )
		 {
			  for ( int i = 0; i < groups.Length; i++ )
			  {
					if ( i > 0 )
					{
						 groups[i].Prev = groups[i - 1].Id;
					}
					if ( i < groups.Length - 1 )
					{
						 groups[i].Next = groups[i + 1].Id;
					}
			  }
		 }

		 private RelationshipGroupRecord Group( long id, int type )
		 {
			  RelationshipGroupRecord group = new RelationshipGroupRecord( id, type );
			  group.InUse = true;
			  return group;
		 }
	}

}