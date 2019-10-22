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
namespace Neo4Net.causalclustering.messaging.marshalling
{

	using Neo4Net.causalclustering.core.state.storage;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	/// <summary>
	/// Implementations of this class perform marshalling (encoding/decoding) of <seealso cref="STATE"/>
	/// into/from a <seealso cref="WritableChannel"/> and a <seealso cref="ReadableChannel"/> respectively.
	/// 
	/// N.B.: Implementations should prefer to extend <seealso cref="SafeChannelMarshal"/> to handle
	/// <seealso cref="org.Neo4Net.storageengine.api.ReadPastEndException"/> correctly.
	/// </summary>
	/// @param <STATE> The class of objects supported by this marshal </param>
	public interface ChannelMarshal<STATE>
	{
		 /// <summary>
		 /// Marshals the state into the channel.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void marshal(STATE state, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException;
		 void Marshal( STATE state, WritableChannel channel );

		 /// <summary>
		 /// Unmarshals an instance of <seealso cref="STATE"/> from channel. If the channel does not have enough bytes
		 /// to fully read an instance then an <seealso cref="EndOfStreamException"/> must be thrown.
		 /// 
		 /// N.B: The ReadableChannel is sort of broken in its implementation and throws
		 /// <seealso cref="org.Neo4Net.storageengine.api.ReadPastEndException"/> which is a subclass of IOException
		 /// and that is problematic since usually the case of reaching the end of a stream actually
		 /// requires handling distinct from that of arbitrary IOExceptions. Although it was possible
		 /// to catch that particular exception explicitly, you would not get compiler/IDE support
		 /// for making that apparent.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: STATE unmarshal(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException;
		 STATE Unmarshal( ReadableChannel channel );
	}

}