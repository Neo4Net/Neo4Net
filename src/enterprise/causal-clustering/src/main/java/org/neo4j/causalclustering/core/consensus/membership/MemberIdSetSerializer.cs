using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.membership
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	/// <summary>
	/// Format:
	/// ┌────────────────────────────────────────────┐
	/// │ memberCount                        4 bytes │
	/// │ member 0   ┌──────────────────────────────┐│
	/// │            │mostSignificantBits    8 bytes││
	/// │            │leastSignificantBits   8 bytes││
	/// │            └──────────────────────────────┘│
	/// │ ...                                        │
	/// │ member n   ┌──────────────────────────────┐│
	/// │            │mostSignificantBits    8 bytes││
	/// │            │leastSignificantBits   8 bytes││
	/// │            └──────────────────────────────┘│
	/// └────────────────────────────────────────────┘
	/// </summary>
	public class MemberIdSetSerializer
	{
		 private MemberIdSetSerializer()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void marshal(MemberIdSet memberSet, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException
		 public static void Marshal( MemberIdSet memberSet, WritableChannel channel )
		 {
			  ISet<MemberId> members = memberSet.Members;
			  channel.PutInt( members.Count );

			  MemberId.Marshal memberIdMarshal = new MemberId.Marshal();

			  foreach ( MemberId member in members )
			  {
					memberIdMarshal.MarshalConflict( member, channel );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static MemberIdSet unmarshal(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
		 public static MemberIdSet Unmarshal( ReadableChannel channel )
		 {
			  HashSet<MemberId> members = new HashSet<MemberId>();
			  int memberCount = channel.Int;

			  MemberId.Marshal memberIdMarshal = new MemberId.Marshal();

			  for ( int i = 0; i < memberCount; i++ )
			  {
					members.Add( memberIdMarshal.Unmarshal( channel ) );
			  }

			  return new MemberIdSet( members );
		 }
	}

}