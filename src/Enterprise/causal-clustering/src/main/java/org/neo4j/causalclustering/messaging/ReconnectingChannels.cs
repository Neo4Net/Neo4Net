using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.messaging
{

	using ProtocolStack = Neo4Net.causalclustering.protocol.handshake.ProtocolStack;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Neo4Net.Helpers.Collections;
	using Streams = Neo4Net.Stream.Streams;

	public class ReconnectingChannels
	{
		 private readonly ConcurrentDictionary<AdvertisedSocketAddress, ReconnectingChannel> _lazyChannelMap = new ConcurrentDictionary<AdvertisedSocketAddress, ReconnectingChannel>();

		 public virtual int Size()
		 {
			  return _lazyChannelMap.Count;
		 }

		 public virtual ReconnectingChannel Get( AdvertisedSocketAddress to )
		 {
			  return _lazyChannelMap[to];
		 }

		 public virtual ReconnectingChannel PutIfAbsent( AdvertisedSocketAddress to, ReconnectingChannel timestampedLazyChannel )
		 {
			  return _lazyChannelMap.GetOrAdd( to, timestampedLazyChannel );
		 }

		 public virtual ICollection<ReconnectingChannel> Values()
		 {
			  return _lazyChannelMap.Values;
		 }

		 public virtual void Remove( AdvertisedSocketAddress address )
		 {
			  _lazyChannelMap.Remove( address );
		 }

		 public virtual Stream<Pair<AdvertisedSocketAddress, ProtocolStack>> InstalledProtocols()
		 {
			  return _lazyChannelMap.SetOfKeyValuePairs().Select(this.installedProtocolOpt).flatMap(Streams.ofOptional);
		 }

		 private Optional<Pair<AdvertisedSocketAddress, ProtocolStack>> InstalledProtocolOpt( KeyValuePair<AdvertisedSocketAddress, ReconnectingChannel> entry )
		 {
			  return entry.Value.installedProtocolStack().map(protocols => Pair.of(entry.Key, protocols));
		 }
	}

}