﻿/*
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
namespace Org.Neo4j.Kernel.ha.com.master
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using TxChecksumVerifier = Org.Neo4j.com.TxChecksumVerifier;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class MasterServerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanExistentLockSessionOnFinishOffChannel()
		 public virtual void ShouldCleanExistentLockSessionOnFinishOffChannel()
		 {
			  Master master = mock( typeof( Master ) );
			  ConversationManager conversationManager = mock( typeof( ConversationManager ) );
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  MasterServer masterServer = new MasterServer( master, mock( typeof( LogProvider ) ), mock( typeof( Server.Configuration ) ), mock( typeof( TxChecksumVerifier ) ), mock( typeof( ByteCounterMonitor ) ), mock( typeof( RequestMonitor ) ), conversationManager, logEntryReader );
			  RequestContext requestContext = new RequestContext( 1L, 1, 1, 0, 0L );

			  masterServer.StopConversation( requestContext );

			  Mockito.verify( conversationManager ).stop( requestContext );
		 }
	}

}