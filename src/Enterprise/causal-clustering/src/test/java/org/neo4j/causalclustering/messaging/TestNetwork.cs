using System;
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
namespace Neo4Net.causalclustering.messaging
{

	public class TestNetwork<T>
	{
		 private readonly IDictionary<T, Inbound> _inboundChannels = new Dictionary<T, Inbound>();
		 private readonly IDictionary<T, Outbound> _outboundChannels = new Dictionary<T, Outbound>();

		 private readonly AtomicLong _seqGen = new AtomicLong();
		 private readonly System.Func<T, T, long> _latencySpecMillis;

		 public TestNetwork( System.Func<T, T, long> latencySpecMillis )
		 {
			  this._latencySpecMillis = latencySpecMillis;
		 }

		 public virtual void Disconnect( T endpoint )
		 {
			  DisconnectOutbound( endpoint );
			  DisconnectInbound( endpoint );
		 }

		 public virtual void Reconnect( T endpoint )
		 {
			  ReconnectInbound( endpoint );
			  ReconnectOutbound( endpoint );
		 }

		 public virtual void Reset()
		 {
			  _inboundChannels.Values.forEach( Inbound.reconnect );
			  _outboundChannels.Values.forEach( Outbound.reconnect );
		 }

		 public virtual void DisconnectInbound( T endpoint )
		 {
			  _inboundChannels[endpoint].disconnect();
		 }

		 public virtual void ReconnectInbound( T endpoint )
		 {
			  _inboundChannels[endpoint].reconnect();
		 }

		 public virtual void DisconnectOutbound( T endpoint )
		 {
			  _outboundChannels[endpoint].disconnect();
		 }

		 public virtual void ReconnectOutbound( T endpoint )
		 {
			  _outboundChannels[endpoint].reconnect();
		 }

		 public virtual void Start()
		 {
			  foreach ( Inbound inbound in _inboundChannels.Values )
			  {
					inbound.Start();
			  }

			  foreach ( Outbound outbound in _outboundChannels.Values )
			  {
					outbound.Start();
			  }
		 }

		 public virtual void Stop()
		 {
			  foreach ( Outbound outbound in _outboundChannels.Values )
			  {
					try
					{
						 outbound.Stop();
					}
					catch ( InterruptedException )
					{
						 Thread.CurrentThread.Interrupt();
					}
			  }

			  foreach ( Inbound inbound in _inboundChannels.Values )
			  {
					try
					{
						 inbound.Stop();
					}
					catch ( InterruptedException )
					{
						 Thread.CurrentThread.Interrupt();
					}
			  }
		 }

		 public class Outbound : Neo4Net.causalclustering.messaging.Outbound<T, Message>
		 {
			 private readonly TestNetwork<T> _outerInstance;

			  internal NetworkThread NetworkThread;
			  internal volatile bool Disconnected;
			  internal T Me;

			  public Outbound( TestNetwork<T> outerInstance, T me )
			  {
				  this._outerInstance = outerInstance;
					this.Me = me;
					outerInstance.outboundChannels[me] = this;
			  }

			  public virtual void Start()
			  {
					NetworkThread = new NetworkThread( this );
					NetworkThread.Start();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws InterruptedException
			  public virtual void Stop()
			  {
					NetworkThread.kill();
			  }

			  public override void Send( T destination, Message message, bool block )
			  {
					if ( block )
					{
						 throw new System.NotSupportedException( "Not implemented" );
					}
					DoSend( destination, message, DateTimeHelper.CurrentUnixTimeMillis() );
			  }

			  internal virtual void DoSend( T destination, Message message, long now )
			  {
					long atMillis = now + outerInstance.latencySpecMillis.apply( Me, destination );
					NetworkThread.scheduleDelivery( destination, message, atMillis );
			  }

			  public virtual void Disconnect()
			  {
					Disconnected = true;
			  }

			  public virtual void Reconnect()
			  {
					Disconnected = false;
			  }

			  internal class NetworkThread : Thread
			  {
				  private readonly TestNetwork.Outbound _outerInstance;

				  public NetworkThread( TestNetwork.Outbound outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

					internal volatile bool Done;

					internal readonly SortedSet<MessageContext> MsgQueue = new SortedSet<MessageContext>((IComparer<MessageContext>)(o1, o2) =>
					{
					 int res = Long.compare( o1.atMillis, o2.atMillis );

					 if ( res == 0 && o1 != o2 )
					 {
						  res = o1.seqNum < o2.seqNum ? -1 : 1;
					 }
					 return res;
					});

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void kill() throws InterruptedException
					public virtual void Kill()
					{
						 Done = true;
						 this.Interrupt();
						 this.Join();
					}

					private class MessageContext
					{
						private readonly TestNetwork.Outbound.NetworkThread _outerInstance;

						 internal readonly T Destination;
						 internal readonly Message Message;
						 internal long AtMillis;
						 internal long SeqNum;

						 internal MessageContext( TestNetwork.Outbound.NetworkThread outerInstance, T destination, Message message, long atMillis )
						 {
							 this._outerInstance = outerInstance;
							  this.Destination = destination;
							  this.Message = message;
							  this.AtMillis = atMillis;
							  this.SeqNum = outerInstance.outerInstance.outerInstance.seqGen.AndIncrement;
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
							  MessageContext that = ( MessageContext ) o;
							  return SeqNum == that.SeqNum;
						 }

						 public override int GetHashCode()
						 {
							  return Objects.hash( SeqNum );
						 }
					}

					public virtual void ScheduleDelivery( T destination, Message message, long atMillis )
					{
						lock ( this )
						{
							 if ( !outerInstance.Disconnected )
							 {
								  MsgQueue.Add( new MessageContext( this, destination, message, atMillis ) );
								  Monitor.PulseAll( this );
							 }
						}
					}

					public override void Run()
					{
						lock ( this )
						{
							 while ( !Done )
							 {
								  long now = DateTimeHelper.CurrentUnixTimeMillis();
      
								  /* Process message ready for delivery */
								  IEnumerator<MessageContext> itr = MsgQueue.GetEnumerator();
								  MessageContext context;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
								  while ( itr.MoveNext() && (context = itr.next()).atMillis <= now )
								  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
										itr.remove();
										Inbound inbound = outerInstance.outerInstance.inboundChannels[context.Destination];
										if ( inbound != null )
										{
											 inbound.Deliver( context.Message );
										}
								  }
      
								  /* Waiting logic */
								  try
								  {
										try
										{
											 MessageContext first = MsgQueue.Min;
											 long waitTime = first.AtMillis - DateTimeHelper.CurrentUnixTimeMillis();
											 if ( waitTime > 0 )
											 {
												  Monitor.Wait( this, TimeSpan.FromMilliseconds( waitTime ) );
											 }
										}
										catch ( NoSuchElementException )
										{
											 Monitor.Wait( this, TimeSpan.FromMilliseconds( 1000 ) );
										}
								  }
								  catch ( InterruptedException )
								  {
										Done = true;
								  }
							 }
						}
					}
			  }
		 }

		 public class Inbound : Neo4Net.causalclustering.messaging.Inbound<Message>
		 {
			 private readonly TestNetwork<T> _outerInstance;

			  internal Inbound_MessageHandler<Message> Handler;
			  internal readonly BlockingQueue<Message> Q = new ArrayBlockingQueue<Message>( 64, true );
			  internal NetworkThread NetworkThread;

			  public Inbound( TestNetwork<T> outerInstance, T endpoint )
			  {
				  this._outerInstance = outerInstance;
					outerInstance.inboundChannels[endpoint] = this;
			  }

			  public virtual void Start()
			  {
					NetworkThread = new NetworkThread( this );
					NetworkThread.Start();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws InterruptedException
			  public virtual void Stop()
			  {
					NetworkThread.kill();
			  }

			  internal volatile bool Disconnected;

			  public virtual void Deliver( Message message )
			  {
				  lock ( this )
				  {
						if ( !Disconnected )
						{
							 // do not throw is the queue is full, emulate the drop of messages instead
							 Q.offer( message );
						}
				  }
			  }

			  public override void RegisterHandler( Inbound_MessageHandler<Message> handler )
			  {
					this.Handler = handler;
			  }

			  public virtual void Disconnect()
			  {
					Disconnected = true;
			  }

			  public virtual void Reconnect()
			  {
					Disconnected = false;
			  }

			  internal class NetworkThread : Thread
			  {
				  private readonly TestNetwork.Inbound _outerInstance;

				  public NetworkThread( TestNetwork.Inbound outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

					internal volatile bool Done;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void kill() throws InterruptedException
					public virtual void Kill()
					{
						 Done = true;
						 this.Interrupt();
						 this.Join();
					}

					public override void Run()
					{
						 while ( !Done )
						 {
							  try
							  {
									Message message = outerInstance.Q.poll( 1, TimeUnit.SECONDS );
									if ( message != null && outerInstance.Handler != null )
									{
										 outerInstance.Handler.handle( message );
									}
							  }
							  catch ( InterruptedException )
							  {
									Done = true;
							  }
						 }
					}
			  }
		 }
	}

}