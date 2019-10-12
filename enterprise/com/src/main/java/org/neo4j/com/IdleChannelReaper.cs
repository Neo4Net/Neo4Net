using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.com
{
	using Channel = org.jboss.netty.channel.Channel;


	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Keeps track of a set of channels and when they were last active.
	/// Run this object periodically to close any channels that have been inactive for longer than the threshold.
	/// This functionality is required because sometimes Netty doesn't tell us when channels are
	/// closed or disconnected. Most of the time it does, but this acts as a safety
	/// net for those we don't get notifications for. When the bug is fixed remove this class.
	/// </summary>
	public class IdleChannelReaper : ThreadStart
	{
		 private readonly IDictionary<Channel, Request> _connectedChannels = new Dictionary<Channel, Request>();
		 private Clock _clock;
		 private readonly Log _msgLog;
		 private ChannelCloser _channelCloser;
		 private long _thresholdMillis;

		 public IdleChannelReaper( ChannelCloser channelCloser, LogProvider logProvider, Clock clock, long thresholdMillis )
		 {
			  this._channelCloser = channelCloser;
			  this._clock = clock;
			  this._thresholdMillis = thresholdMillis;
			  _msgLog = logProvider.getLog( this.GetType() );
		 }

		 public virtual void Add( Channel channel, RequestContext requestContext )
		 {
			 lock ( this )
			 {
				  Request previous = _connectedChannels[channel];
				  if ( previous != null )
				  {
						previous.LastTimeHeardOf = _clock.millis();
				  }
				  else
				  {
						_connectedChannels[channel] = new Request( requestContext, _clock.millis() );
				  }
			 }
		 }

		 public virtual Request Remove( Channel channel )
		 {
			 lock ( this )
			 {
				  return _connectedChannels.Remove( channel );
			 }
		 }

		 public virtual bool Update( Channel channel )
		 {
			 lock ( this )
			 {
				  Request request = _connectedChannels[channel];
				  if ( request == null )
				  {
						return false;
				  }
      
				  request.LastTimeHeardOf = _clock.millis();
				  return true;
			 }
		 }

		 public override void Run()
		 {
			 lock ( this )
			 {
				  foreach ( KeyValuePair<Channel, Request> entry in _connectedChannels.SetOfKeyValuePairs() )
				  {
						Channel channel = entry.Key;
						long age = _clock.millis() - entry.Value.lastTimeHeardOf;
						if ( age > _thresholdMillis )
						{
							 _msgLog.info( "Found a silent channel " + entry + ", " + age );
							 _channelCloser.tryToCloseChannel( channel );
						}
						else if ( age > _thresholdMillis / 2 )
						{
							 if ( !( channel.Open && channel.Connected && channel.Bound ) )
							 {
								  _channelCloser.tryToCloseChannel( channel );
							 }
						}
				  }
			 }
		 }

		 public class Request
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RequestContext RequestContextConflict;

			  internal long LastTimeHeardOf;

			  public Request( RequestContext requestContext, long lastTimeHeardOf )
			  {
					this.RequestContextConflict = requestContext;
					this.LastTimeHeardOf = lastTimeHeardOf;
			  }

			  public virtual RequestContext RequestContext
			  {
				  get
				  {
						return RequestContextConflict;
				  }
			  }
		 }
	}

}