using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.catchup
{

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

	internal class CatchUpChannelPool<CHANNEL> where CHANNEL : CatchUpChannelPool.Channel
	{
		 private readonly IDictionary<AdvertisedSocketAddress, LinkedList<CHANNEL>> _idleChannels = new Dictionary<AdvertisedSocketAddress, LinkedList<CHANNEL>>();
		 private readonly ISet<CHANNEL> _activeChannels = new HashSet<CHANNEL>();
		 private readonly System.Func<AdvertisedSocketAddress, CHANNEL> _factory;

		 internal CatchUpChannelPool( System.Func<AdvertisedSocketAddress, CHANNEL> factory )
		 {
			  this._factory = factory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CHANNEL acquire(org.neo4j.helpers.AdvertisedSocketAddress catchUpAddress) throws Exception
		 internal virtual CHANNEL Acquire( AdvertisedSocketAddress catchUpAddress )
		 {
			  CHANNEL channel = GetIdleChannel( catchUpAddress );

			  if ( channel == null )
			  {
					channel = _factory.apply( catchUpAddress );
					try
					{
						 channel.Connect();
						 AssertActive( channel, catchUpAddress );
					}
					catch ( Exception e )
					{
						 channel.Close();
						 throw e;
					}
			  }

			  AddActiveChannel( channel );

			  return channel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertActive(CHANNEL channel, org.neo4j.helpers.AdvertisedSocketAddress address) throws java.net.ConnectException
		 private void AssertActive( CHANNEL channel, AdvertisedSocketAddress address )
		 {
			  if ( !channel.Active )
			  {
					throw new ConnectException( "Unable to connect to " + address );
			  }
		 }

		 private CHANNEL GetIdleChannel( AdvertisedSocketAddress catchUpAddress )
		 {
			 lock ( this )
			 {
				  CHANNEL channel = null;
				  LinkedList<CHANNEL> channels = _idleChannels[catchUpAddress];
				  if ( channels != null )
				  {
						while ( ( channel = channels.RemoveFirst() ) != null )
						{
							 if ( channel.Active )
							 {
								  break;
							 }
						}
						if ( channels.Count == 0 )
						{
							 _idleChannels.Remove( catchUpAddress );
						}
				  }
				  return channel;
			 }
		 }

		 private void AddActiveChannel( CHANNEL channel )
		 {
			 lock ( this )
			 {
				  _activeChannels.Add( channel );
			 }
		 }

		 private void RemoveActiveChannel( CHANNEL channel )
		 {
			 lock ( this )
			 {
				  _activeChannels.remove( channel );
			 }
		 }

		 internal virtual void Dispose( CHANNEL channel )
		 {
			  RemoveActiveChannel( channel );
			  channel.Close();
		 }

		 internal virtual void Release( CHANNEL channel )
		 {
			 lock ( this )
			 {
				  RemoveActiveChannel( channel );
				  _idleChannels.computeIfAbsent( channel.Destination(), address => new LinkedList<>() ).add(channel);
			 }
		 }

		 internal virtual void Close()
		 {
			  CollectDisposed().forEach(Channel.close);
		 }

		 private ISet<CHANNEL> CollectDisposed()
		 {
			 lock ( this )
			 {
				  ISet<CHANNEL> disposed;
				  disposed = concat( _idleChannels.Values.stream().flatMap(System.Collections.ICollection.stream), _activeChannels.stream() ).collect(Collectors.toSet());
      
				  _idleChannels.Clear();
				  _activeChannels.Clear();
				  return disposed;
			 }
		 }

		 internal interface Channel
		 {
			  AdvertisedSocketAddress Destination();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void connect() throws Exception;
			  void Connect();

			  bool Active { get; }

			  void Close();
		 }
	}

}