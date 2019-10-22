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
namespace Neo4Net.Kernel.ha.com.slave
{
	using Test = org.junit.Test;

	using IllegalProtocolVersionException = Neo4Net.com.IllegalProtocolVersionException;
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using Suppliers = Neo4Net.Functions.Suppliers;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class MasterClientResolverTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResolveMasterClientFactory()
		 public virtual void ShouldResolveMasterClientFactory()
		 {
			  // Given
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  MasterClientResolver resolver = new MasterClientResolver( NullLogProvider.Instance, Neo4Net.com.storecopy.ResponseUnpacker_Fields.NoOpResponseUnpacker, mock( typeof( InvalidEpochExceptionHandler ) ), 1, 1, 1, 1024, Suppliers.singleton( logEntryReader ) );

			  LifeSupport life = new LifeSupport();
			  try
			  {
					life.Start();
					MasterClient masterClient1 = resolver.Instantiate( "cluster://localhost", 44, null, new Monitors(), StoreId.DEFAULT, life );
					assertThat( masterClient1, instanceOf( typeof( MasterClient320 ) ) );
			  }
			  finally
			  {
					life.Shutdown();
			  }

			  IllegalProtocolVersionException illegalProtocolVersionException = new IllegalProtocolVersionException( MasterClient214.PROTOCOL_VERSION.ApplicationProtocol, MasterClient310.PROTOCOL_VERSION.ApplicationProtocol, "Protocol is too modern" );

			  // When
			  resolver.Handle( illegalProtocolVersionException );

			  // Then
			  life = new LifeSupport();
			  try
			  {
					life.Start();
					MasterClient masterClient2 = resolver.Instantiate( "cluster://localhost", 55, null, new Monitors(), StoreId.DEFAULT, life );

					assertThat( masterClient2, instanceOf( typeof( MasterClient214 ) ) );
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }
	}

}