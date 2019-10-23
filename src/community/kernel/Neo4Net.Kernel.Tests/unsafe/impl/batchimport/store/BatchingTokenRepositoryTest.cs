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
namespace Neo4Net.@unsafe.Impl.Batchimport.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using NamedToken = Neo4Net.Kernel.Api.Internal.NamedToken;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using Neo4Net.Kernel.impl.store;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using BatchingLabelTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingLabelTokenRepository;
	using BatchingPropertyKeyTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingPropertyKeyTokenRepository;
	using BatchingRelationshipTypeTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingRelationshipTypeTokenRepository;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BatchingTokenRepositoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.PageCacheAndDependenciesRule storage = new org.Neo4Net.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDedupLabelIds()
		 public virtual void ShouldDedupLabelIds()
		 {
			  // GIVEN
			  BatchingLabelTokenRepository repo = new BatchingLabelTokenRepository( mock( typeof( TokenStore ) ) );

			  // WHEN
			  long[] ids = repo.GetOrCreateIds( new string[] { "One", "Two", "One" } );

			  // THEN
			  assertTrue( NodeLabelsField.isSane( ids ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSortLabelIds()
		 public virtual void ShouldSortLabelIds()
		 {
			  // GIVEN
			  BatchingLabelTokenRepository repo = new BatchingLabelTokenRepository( mock( typeof( TokenStore ) ) );
			  long[] expected = new long[] { repo.GetOrCreateId( "One" ), repo.GetOrCreateId( "Two" ), repo.GetOrCreateId( "Three" ) };

			  // WHEN
			  long[] ids = repo.GetOrCreateIds( new string[] { "Two", "One", "Three" } );

			  // THEN
			  assertArrayEquals( expected, ids );
			  assertTrue( NodeLabelsField.isSane( ids ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectExistingTokens()
		 public virtual void ShouldRespectExistingTokens()
		 {
			  // given
			  TokenStore<RelationshipTypeTokenRecord> tokenStore = mock( typeof( TokenStore ) );
			  int previousHighId = 5;
			  when( tokenStore.HighId ).thenReturn( ( long ) previousHighId );
			  BatchingRelationshipTypeTokenRepository repo = new BatchingRelationshipTypeTokenRepository( tokenStore );
			  verify( tokenStore ).HighId;

			  // when
			  int tokenId = repo.GetOrCreateId( "NEW_ONE" );

			  // then
			  assertEquals( previousHighId, tokenId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlushNewTokens()
		 public virtual void ShouldFlushNewTokens()
		 {
			  // given

			  try (PageCache pageCache = Storage.pageCache(); NeoStores stores = new StoreFactory(Storage.directory().databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(Storage.fileSystem()), pageCache, Storage.fileSystem(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY)
						 .openNeoStores( true, StoreType.PROPERTY_KEY_TOKEN, StoreType.PROPERTY_KEY_TOKEN_NAME ))
						 {
					TokenStore<PropertyKeyTokenRecord> tokenStore = stores.PropertyKeyTokenStore;
					int rounds = 3;
					int tokensPerRound = 4;
					using ( BatchingPropertyKeyTokenRepository repo = new BatchingPropertyKeyTokenRepository( tokenStore ) )
					{
						 // when first creating some tokens
						 int expectedId = 0;
						 int tokenNameAsInt = 0;
						 for ( int round = 0; round < rounds; round++ )
						 {
							  for ( int i = 0; i < tokensPerRound; i++ )
							  {
									int tokenId = repo.GetOrCreateId( ( tokenNameAsInt++ ).ToString() );
									assertEquals( expectedId + i, tokenId );
							  }
							  assertEquals( expectedId, tokenStore.HighId );
							  repo.Flush();
							  assertEquals( expectedId + tokensPerRound, tokenStore.HighId );
							  expectedId += tokensPerRound;
						 }
					}

					IList<NamedToken> tokens = tokenStore.Tokens;
					assertEquals( tokensPerRound * rounds, tokens.Count );
					foreach ( NamedToken token in tokens )
					{
						 assertEquals( token.Id(), parseInt(token.Name()) );
					}
						 }
		 }
	}

}