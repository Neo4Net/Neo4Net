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
namespace Org.Neo4j.ha.correctness
{

	using Org.Neo4j.cluster.com.message;
	using MessageProcessor = Org.Neo4j.cluster.com.message.MessageProcessor;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using FixedTimeoutStrategy = Org.Neo4j.cluster.timeout.FixedTimeoutStrategy;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Org.Neo4j.Helpers.Collection;

	internal class ProverTimeouts : Timeouts
	{
		 private readonly IDictionary<object, Pair<ProverTimeout, long>> _timeouts;
		 private readonly URI _to;
		 private long _time;

		 internal ProverTimeouts( URI to ) : base( new FixedTimeoutStrategy( 1 ) )
		 {
			  this._to = to;
			  _timeouts = new LinkedHashMap<object, Pair<ProverTimeout, long>>();
		 }

		 private ProverTimeouts( URI to, IDictionary<object, Pair<ProverTimeout, long>> timeouts ) : base( new FixedTimeoutStrategy( 0 ) )
		 {
			  this._to = to;
			  this._timeouts = new LinkedHashMap<object, Pair<ProverTimeout, long>>( timeouts );
		 }

		 public override void AddMessageProcessor( MessageProcessor messageProcessor )
		 {

		 }

		 public override void SetTimeout<T1>( object key, Message<T1> timeoutMessage ) where T1 : Org.Neo4j.cluster.com.message.MessageType
		 {
			  // Should we add support for using timeout strategies to order the timeouts here?
			  long timeout = _time++;

			  _timeouts[key] = Pair.of( new ProverTimeout( this, timeout, timeoutMessage.SetHeader( Message.HEADER_TO, _to.toASCIIString() ).setHeader(Message.HEADER_FROM, _to.toASCIIString()) ), timeout );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> cancelTimeout(Object key)
		 public override Message<MessageType> CancelTimeout( object key )
		 {
			  Pair<ProverTimeout, long> timeout = _timeouts.Remove( key );
			  if ( timeout != null )
			  {
					return timeout.First().TimeoutMessage;
			  }
			  return null;
		 }

		 public override void CancelAllTimeouts()
		 {
			  _timeouts.Clear();
		 }

		 public override IDictionary<object, Timeout> Timeouts
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override void Tick( long time )
		 {
			  // Don't pass this on to the parent class.
		 }

		 public virtual ProverTimeouts Snapshot()
		 {
			  return new ProverTimeouts( _to, _timeouts );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ProverTimeouts that = ( ProverTimeouts ) o;

			  IEnumerator<Pair<ProverTimeout, long>> those = that._timeouts.Values.GetEnumerator();
			  IEnumerator<Pair<ProverTimeout, long>> mine = _timeouts.Values.GetEnumerator();

			  while ( mine.MoveNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !those.hasNext() || !those.next().first().Equals(mine.Current.first()) )
					{
						 return false;
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return !those.hasNext();
		 }

		 public override int GetHashCode()
		 {
			  return _timeouts.GetHashCode();
		 }

		 public virtual bool HasTimeouts()
		 {
			  return _timeouts.Count > 0;
		 }

		 public virtual ClusterAction Pop()
		 {
			  KeyValuePair<object, Pair<ProverTimeout, long>> lowestTimeout = NextTimeout();
			  _timeouts.Remove( lowestTimeout.Key );
			  return new MessageDeliveryAction( lowestTimeout.Value.first().TimeoutMessage );
		 }

		 public virtual ClusterAction Peek()
		 {
			  KeyValuePair<object, Pair<ProverTimeout, long>> next = NextTimeout();
			  if ( next != null )
			  {
					return new MessageDeliveryAction( next.Value.first().TimeoutMessage );
			  }
			  else
			  {
					return null;
			  }
		 }

		 private KeyValuePair<object, Pair<ProverTimeout, long>> NextTimeout()
		 {
			  KeyValuePair<object, Pair<ProverTimeout, long>> lowestTimeout = null;
			  foreach ( KeyValuePair<object, Pair<ProverTimeout, long>> current in _timeouts.SetOfKeyValuePairs() )
			  {
					if ( lowestTimeout == null || lowestTimeout.Value.other() > current.Value.other() )
					{
						 lowestTimeout = current;
					}
			  }
			  return lowestTimeout;
		 }

		 internal class ProverTimeout : Timeout
		 {
			 private readonly ProverTimeouts _outerInstance;

			  internal ProverTimeout<T1>( ProverTimeouts outerInstance, long timeout, Message<T1> timeoutMessage ) where T1 : Org.Neo4j.cluster.com.message.MessageType : base( outerInstance, timeout, timeoutMessage )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override int GetHashCode()
			  {
					return TimeoutMessage.GetHashCode();
			  }

			  public override bool Equals( object obj )
			  {
					if ( obj.GetType() != this.GetType() )
					{
						 return false;
					}

					ProverTimeout that = ( ProverTimeout )obj;

					return TimeoutMessage.ToString().Equals(that.TimeoutMessage.ToString());
			  }
		 }
	}

}