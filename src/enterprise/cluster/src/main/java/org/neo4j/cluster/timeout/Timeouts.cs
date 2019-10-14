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
namespace Neo4Net.cluster.timeout
{

	using Neo4Net.cluster.com.message;
	using MessageProcessor = Neo4Net.cluster.com.message.MessageProcessor;
	using MessageSource = Neo4Net.cluster.com.message.MessageSource;
	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Timeout management for state machines. First call setTimeout to setup a timeout.
	/// Then either the timeout will trigger or cancelTimeout will have been called with
	/// the key used to create the timeout.
	/// </summary>
	public class Timeouts : MessageSource
	{
		 private long _now;

		 private MessageProcessor _receiver;
		 private TimeoutStrategy _timeoutStrategy;

		 private IDictionary<object, Timeout> _timeouts = new Dictionary<object, Timeout>();
		 private IList<KeyValuePair<object, Timeout>> _triggeredTimeouts = new List<KeyValuePair<object, Timeout>>();

		 public Timeouts( TimeoutStrategy timeoutStrategy )
		 {
			  this._timeoutStrategy = timeoutStrategy;
		 }

		 public override void AddMessageProcessor( MessageProcessor messageProcessor )
		 {
			  if ( _receiver != null )
			  {
					throw new System.NotSupportedException( "Timeouts does not yet support multiple message processors" );
			  }
			  _receiver = messageProcessor;
		 }

		 /// <summary>
		 /// Add a new timeout to the list
		 /// If this is not cancelled it will trigger a message on the message processor
		 /// </summary>
		 /// <param name="key"> </param>
		 /// <param name="timeoutMessage"> </param>
		 public virtual void SetTimeout<T1>( object key, Message<T1> timeoutMessage ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  long timeoutAt = _now + _timeoutStrategy.timeoutFor( timeoutMessage );
			  _timeouts[key] = new Timeout( this, timeoutAt, timeoutMessage );
		 }

		 public virtual long GetTimeoutFor<T1>( Message<T1> timeoutMessage ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  return _timeoutStrategy.timeoutFor( timeoutMessage );
		 }

		 /// <summary>
		 /// Cancel a timeout corresponding to a particular key. Use the same key
		 /// that was used to set it up.
		 /// </summary>
		 /// <param name="key"> </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> cancelTimeout(Object key)
		 public virtual Message<MessageType> CancelTimeout( object key )
		 {
			  Timeout timeout = _timeouts.Remove( key );
			  if ( timeout != null )
			  {
					_timeoutStrategy.timeoutCancelled( timeout.TimeoutMessageConflict );
					return timeout.TimeoutMessage;
			  }
			  return null;
		 }

		 /// <summary>
		 /// Cancel all current timeouts. This is typically used when shutting down.
		 /// </summary>
		 public virtual void CancelAllTimeouts()
		 {
			  foreach ( Timeout timeout in _timeouts.Values )
			  {
					_timeoutStrategy.timeoutCancelled( timeout.TimeoutMessage );
			  }
			  _timeouts.Clear();
		 }

		 public virtual IDictionary<object, Timeout> GetTimeouts()
		 {
			  return _timeouts;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> getTimeoutMessage(String timeoutName)
		 public virtual Message<MessageType> GetTimeoutMessage( string timeoutName )
		 {
			  Timeout timeout = _timeouts[timeoutName];
			  if ( timeout != null )
			  {
					return timeout.TimeoutMessage;
			  }
			  else
			  {
					return null;
			  }
		 }

		 public virtual void Tick( long time )
		 {
			  lock ( this )
			  {
					// Time has passed
					_now = time;

					_timeoutStrategy.tick( _now );

					// Check if any timeouts needs to be triggered
					_triggeredTimeouts.Clear();
					foreach ( KeyValuePair<object, Timeout> timeout in _timeouts.SetOfKeyValuePairs() )
					{
						 if ( timeout.Value.checkTimeout( _now ) )
						 {
							  _triggeredTimeouts.Add( timeout );
						 }
					}

					// Remove all timeouts that were triggered
					foreach ( KeyValuePair<object, Timeout> triggeredTimeout in _triggeredTimeouts )
					{
						 _timeouts.Remove( triggeredTimeout.Key );
					}
			  }

			  // Trigger timeouts
			  // This needs to be done outside of the synchronized block as it will trigger a message
			  // which will cause the statemachine to synchronize on Timeouts
			  foreach ( KeyValuePair<object, Timeout> triggeredTimeout in _triggeredTimeouts )
			  {
					triggeredTimeout.Value.trigger( _receiver );
			  }
		 }

		 public class Timeout
		 {
			 private readonly Timeouts _outerInstance;

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal long TimeoutConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> timeoutMessage;
			  internal Message<MessageType> TimeoutMessageConflict;

			  public Timeout<T1>( Timeouts outerInstance, long timeout, Message<T1> timeoutMessage ) where T1 : Neo4Net.cluster.com.message.MessageType
			  {
				  this._outerInstance = outerInstance;
					this.TimeoutConflict = timeout;
					this.TimeoutMessageConflict = timeoutMessage;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> getTimeoutMessage()
			  public virtual Message<MessageType> TimeoutMessage
			  {
				  get
				  {
						return TimeoutMessageConflict;
				  }
			  }

			  public virtual bool CheckTimeout( long now )
			  {
					if ( now >= TimeoutConflict )
					{
						 outerInstance.timeoutStrategy.TimeoutTriggered( TimeoutMessageConflict );

						 return true;
					}
					else
					{
						 return false;
					}
			  }

			  public virtual void Trigger( MessageProcessor receiver )
			  {
					receiver.Process( TimeoutMessageConflict );
			  }

			  public override string ToString()
			  {
					return TimeoutConflict + ": " + TimeoutMessageConflict;
			  }
		 }
	}

}