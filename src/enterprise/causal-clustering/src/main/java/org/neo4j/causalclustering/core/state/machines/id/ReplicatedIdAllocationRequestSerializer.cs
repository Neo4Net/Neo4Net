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
namespace Neo4Net.causalclustering.core.state.machines.id
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	public class ReplicatedIdAllocationRequestSerializer
	{

		 private ReplicatedIdAllocationRequestSerializer()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void marshal(ReplicatedIdAllocationRequest idRangeRequest, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
		 public static void Marshal( ReplicatedIdAllocationRequest idRangeRequest, WritableChannel channel )
		 {
			  ( new MemberId.Marshal() ).marshal(idRangeRequest.Owner(), channel);
			  channel.PutInt( idRangeRequest.IdType().ordinal() );
			  channel.PutLong( idRangeRequest.IdRangeStart() );
			  channel.PutInt( idRangeRequest.IdRangeLength() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static ReplicatedIdAllocationRequest unmarshal(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
		 public static ReplicatedIdAllocationRequest Unmarshal( ReadableChannel channel )
		 {
			  MemberId owner = ( new MemberId.Marshal() ).unmarshal(channel);
			  IdType idType = Enum.GetValues( typeof( IdType ) )[channel.Int];
			  long idRangeStart = channel.Long;
			  int idRangeLength = channel.Int;

			  return new ReplicatedIdAllocationRequest( owner, idType, idRangeStart, idRangeLength );
		 }
	}

}