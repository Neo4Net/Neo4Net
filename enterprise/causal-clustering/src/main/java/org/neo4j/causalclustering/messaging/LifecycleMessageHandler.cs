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
namespace Org.Neo4j.causalclustering.messaging
{
	using CoreLife = Org.Neo4j.causalclustering.core.state.CoreLife;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;

	/// <summary>
	/// A <seealso cref="Inbound.MessageHandler"/> that can be started and stopped in <seealso cref="CoreLife"/>.
	/// It is required that if this MessageHandler delegates to another MessageHandler to handle messages
	/// then the delegate will also have lifecycle methods called
	/// </summary>
	public interface LifecycleMessageHandler<M> : Inbound_MessageHandler<M> where M : Message
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void start(org.neo4j.causalclustering.identity.ClusterId clusterId) throws Throwable;
		 void Start( ClusterId clusterId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stop() throws Throwable;
		 void Stop();
	}

}