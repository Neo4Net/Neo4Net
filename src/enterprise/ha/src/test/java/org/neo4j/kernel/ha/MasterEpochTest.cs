/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Test = org.junit.Test;

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using RequestContext = Neo4Net.com.RequestContext;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConversationManager = Neo4Net.Kernel.ha.com.master.ConversationManager;
	using HandshakeResult = Neo4Net.Kernel.ha.com.master.HandshakeResult;
	using InvalidEpochException = Neo4Net.Kernel.ha.com.master.InvalidEpochException;
	using MasterImpl = Neo4Net.Kernel.ha.com.master.MasterImpl;
	using SPI = Neo4Net.Kernel.ha.com.master.MasterImpl.SPI;
	using MasterImplTest = Neo4Net.Kernel.ha.com.master.MasterImplTest;
	using IdAllocation = Neo4Net.Kernel.ha.id.IdAllocation;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

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
//	import static Neo4Net.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.com.StoreIdTestFactory.newStoreIdForCurrentVersion;

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