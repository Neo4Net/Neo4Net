/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.state.machines.locks
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class ReplicatedLockTokenSerializer
	{

		 private ReplicatedLockTokenSerializer()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void marshal(ReplicatedLockTokenRequest tokenRequest, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 public static void Marshal( ReplicatedLockTokenRequest tokenRequest, WritableChannel channel )
		 {
			  channel.PutInt( tokenRequest.Id() );
			  ( new MemberId.Marshal() ).marshal(tokenRequest.Owner(), channel);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static ReplicatedLockTokenRequest unmarshal(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 public static ReplicatedLockTokenRequest Unmarshal( ReadableChannel channel )
		 {
			  int candidateId = channel.Int;
			  MemberId owner = ( new MemberId.Marshal() ).unmarshal(channel);

			  return new ReplicatedLockTokenRequest( owner, candidateId );
		 }
	}

}