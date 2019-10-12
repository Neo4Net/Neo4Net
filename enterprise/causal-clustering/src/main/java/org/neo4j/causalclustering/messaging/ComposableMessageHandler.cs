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
namespace Org.Neo4j.causalclustering.messaging
{

	using Org.Neo4j.causalclustering.core.consensus;

	public interface ComposableMessageHandler : System.Func<LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>, LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: LifecycleMessageHandler<org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> apply(LifecycleMessageHandler<org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> messageHandler);
		 LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> apply<T1>( LifecycleMessageHandler<T1> messageHandler );
	}

}