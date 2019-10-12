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
namespace Org.Neo4j.cluster
{

	using MessageSender = Org.Neo4j.cluster.com.message.MessageSender;
	using MessageSource = Org.Neo4j.cluster.com.message.MessageSource;
	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using TimeoutStrategy = Org.Neo4j.cluster.timeout.TimeoutStrategy;
	using Config = Org.Neo4j.Kernel.configuration.Config;

	/// <summary>
	/// Factory for instantiating ProtocolServers.
	/// </summary>
	/// <seealso cref= ProtocolServer </seealso>
	public interface ProtocolServerFactory
	{
		 ProtocolServer NewProtocolServer( InstanceId me, TimeoutStrategy timeouts, MessageSource input, MessageSender output, AcceptorInstanceStore acceptorInstanceStore, ElectionCredentialsProvider electionCredentialsProvider, Executor stateMachineExecutor, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, Config config );
	}

}