/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.ha
{
	using Test = org.junit.Test;

	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConversationManager = Org.Neo4j.Kernel.ha.com.master.ConversationManager;
	using HandshakeResult = Org.Neo4j.Kernel.ha.com.master.HandshakeResult;
	using InvalidEpochException = Org.Neo4j.Kernel.ha.com.master.InvalidEpochException;
	using MasterImpl = Org.Neo4j.Kernel.ha.com.master.MasterImpl;
	using SPI = Org.Neo4j.Kernel.ha.com.master.MasterImpl.SPI;
	using MasterImplTest = Org.Neo4j.Kernel.ha.com.master.MasterImplTest;
	using IdAllocation = Org.Neo4j.Kernel.ha.id.IdAllocation;
	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;

	public class MasterEpochTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailSubsequentRequestsAfterAllocateIdsAfterMasterSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailSubsequentRequestsAfterAllocateIdsAfterMasterSwitch()
		 {
			  // GIVEN
			  MasterImpl.SPI spi = MasterImplTest.mockedSpi();
			  IdAllocation servedIdAllocation = IdAllocation( 0, 999 );
			  when( spi.AllocateIds( any( typeof( IdType ) ) ) ).thenReturn( servedIdAllocation );
			  when( spi.GetTransactionChecksum( anyLong() ) ).thenReturn(10L);
			  StoreId storeId = newStoreIdForCurrentVersion();
			  MasterImpl master = new MasterImpl( spi, mock( typeof( ConversationManager ) ), mock( typeof( MasterImpl.Monitor ) ), Config.defaults( ClusterSettings.server_id, "1" ) );
			  HandshakeResult handshake = master.Handshake( 1, storeId ).response();
			  master.Start();

			  // WHEN/THEN
			  IdAllocation idAllocation = master.AllocateIds( Context( handshake.Epoch() ), IdType.NODE ).response();
			  assertEquals( servedIdAllocation.HighestIdInUse, idAllocation.HighestIdInUse );
			  try
			  {
					master.AllocateIds( Context( handshake.Epoch() + 1 ), IdType.NODE );
					fail( "Should fail with invalid epoch" );
			  }
			  catch ( InvalidEpochException )
			  { // Good
			  }
		 }

		 private IdAllocation IdAllocation( long from, int length )
		 {
			  return new IdAllocation( new IdRange( EMPTY_LONG_ARRAY, from, length ), from + length, 0 );
		 }

		 private RequestContext Context( long epoch )
		 {
			  return new RequestContext( epoch, 0, 0, 0, 0 );
		 }
	}

}