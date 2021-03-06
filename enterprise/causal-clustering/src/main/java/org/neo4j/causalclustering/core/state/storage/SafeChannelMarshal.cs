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
namespace Org.Neo4j.causalclustering.core.state.storage
{

	using Org.Neo4j.causalclustering.messaging.marshalling;
	using EndOfStreamException = Org.Neo4j.causalclustering.messaging.EndOfStreamException;
	using ReadPastEndException = Org.Neo4j.Storageengine.Api.ReadPastEndException;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;

	/// <summary>
	/// Wrapper class to handle ReadPastEndExceptions in a safe manner transforming it
	/// to the checked EndOfStreamException which does not inherit from an IOException.
	/// </summary>
	/// @param <STATE> The type of state marshalled. </param>
	public abstract class SafeChannelMarshal<STATE> : ChannelMarshal<STATE>
	{
		public abstract void Marshal( STATE state, Org.Neo4j.Storageengine.Api.WritableChannel channel );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final STATE unmarshal(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 public override STATE Unmarshal( ReadableChannel channel )
		 {
			  try
			  {
					return Unmarshal0( channel );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw new EndOfStreamException( e );
			  }
		 }

		 /// <summary>
		 /// The specific implementation of unmarshal which does not have to deal
		 /// with the IOException <seealso cref="ReadPastEndException"/> and can safely throw
		 /// the checked EndOfStreamException.
		 /// </summary>
		 /// <param name="channel"> The channel to read from. </param>
		 /// <returns> An unmarshalled object. </returns>
		 /// <exception cref="IOException"> </exception>
		 /// <exception cref="EndOfStreamException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract STATE unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException;
		 protected internal abstract STATE Unmarshal0( ReadableChannel channel );
	}

}