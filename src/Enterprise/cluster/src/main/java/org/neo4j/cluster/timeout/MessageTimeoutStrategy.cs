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
namespace Neo4Net.cluster.timeout
{

	using Neo4Net.cluster.com.message;
	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Timeout strategy that allows you to specify per message type what timeout to use
	/// </summary>
	public class MessageTimeoutStrategy : TimeoutStrategy
	{
		 private IDictionary<MessageType, long> _timeouts = new Dictionary<MessageType, long>();

		 private TimeoutStrategy @delegate;

		 public MessageTimeoutStrategy( TimeoutStrategy @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public virtual MessageTimeoutStrategy Timeout( MessageType messageType, long timeout )
		 {
			  _timeouts[messageType] = timeout;
			  return this;
		 }

		 public virtual MessageTimeoutStrategy RelativeTimeout( MessageType messageType, MessageType relativeTo, long timeout )
		 {
			  _timeouts[messageType] = _timeouts[relativeTo] + timeout;
			  return this;
		 }

		 public override long TimeoutFor( Message message )
		 {
			  long? timeout = _timeouts[message.MessageType];
			  if ( timeout == null )
			  {
					return @delegate.TimeoutFor( message );
			  }
			  else
			  {
					return timeout.Value;
			  }
		 }

		 public override void TimeoutTriggered( Message timeoutMessage )
		 {
			  @delegate.TimeoutTriggered( timeoutMessage );
		 }

		 public override void TimeoutCancelled( Message timeoutMessage )
		 {
			  @delegate.TimeoutCancelled( timeoutMessage );
		 }

		 public override void Tick( long now )
		 {
			  @delegate.Tick( now );
		 }
	}

}