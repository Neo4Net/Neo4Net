using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.cluster.com.message
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class NetworkSenderReceiverIT
	{
		 public enum TestMessage
		 {
			  HelloWorld
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAMessageFromAClientWhichIsReceivedByAServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAMessageFromAClientWhichIsReceivedByAServer()
		 {

			  // given
			  int port1 = PortAuthority.allocatePort();
			  int port2 = PortAuthority.allocatePort();

			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );

			  LifeSupport life = new LifeSupport();

			  Server server1 = new Server( latch, MapUtil.stringMap( ClusterSettings.cluster_server.name(), "localhost:" + port1, ClusterSettings.server_id.name(), "1", ClusterSettings.initial_hosts.name(), "localhost:" + port1 + ",localhost:" + port2 ) );

			  life.Add( server1 );

			  Server server2 = new Server( latch, MapUtil.stringMap( ClusterSettings.cluster_server.name(), "localhost:" + port2, ClusterSettings.server_id.name(), "2", ClusterSettings.initial_hosts.name(), "localhost:" + port1 + ",localhost:" + port2 ) );

			  life.Add( server2 );

			  life.Start();

			  // when

			  server1.Process( Message.To( TestMessage.HelloWorld, URI.create( "cluster://127.0.0.1:" + port2 ),"Hello World" ) );

			  // then

			  assertTrue( latch.await( 5, TimeUnit.SECONDS ) );

			  assertTrue( "server1 should have processed the message", server1.ProcessedMessage() );
			  assertTrue( "server2 should have processed the message", server2.ProcessedMessage() );

			  life.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void senderThatStartsAfterReceiverShouldEventuallyConnectSuccessfully() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SenderThatStartsAfterReceiverShouldEventuallyConnectSuccessfully()
		 {
			  /*
			   * This test verifies that a closed channel from a sender to a receiver is removed from the connections
			   * mapping in the sender. It starts a sender, connects it to a receiver and sends a message.
			   *
			   * We should be testing this without resorting to using a NetworkReceiver. But, as prophets Mick Jagger and
			   * Keith Richards mention in their scriptures, you can't always get what you want. In this case,
			   * NetworkSender creates on its own the things required to communicate with the outside world, and this
			   * means it creates actual sockets. To interact with it then, we need to setup listeners for those sockets
			   * and respond properly. Hence, NetworkReceiver. Yes, this means that this test requires to open actual
			   * network sockets.
			   *
			   * Read on for further hacks in place.
			   */
			  NetworkSender sender = null;
			  NetworkReceiver receiver = null;
			  try
			  {
					LogProvider logProviderMock = mock( typeof( LogProvider ) );
					Log logMock = mock( typeof( Log ) );
					when( logProviderMock.getLog( ArgumentMatchers.any<Type>() ) ).thenReturn(logMock);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Semaphore sem = new java.util.concurrent.Semaphore(0);
					Semaphore sem = new Semaphore( 0 );

					/*
					 * A semaphore AND a boolean? Weird, you may think, as the purpose is clearly to step through the
					 * connection setup/teardown process. So, let's discuss what happens here more clearly.
					 *
					 * The sender and receiver are started. Trapped by the semaphore release on listeningAt()
					 * The sender sends through the first message, it is received by the receiver. Trapped by the semaphore
					 *      release on listeningAt() which is triggered on the first message receive on the receiver
					 * The receiver is stopped, trapped by the overridden stop() method of the logging service.
					 * The sender sends a message through, which will trigger the ChannelClosedException. This is where it
					 *      gets tricky. See, normally, since we waited for the semaphore on NetworkReceiver.stop() and an
					 *      happensBefore edge exists and all these good things, it should be certain that the Receiver is
					 *      actually stopped and the message would fail to be sent. That would be too easy though. In reality,
					 *      netty will not wait for all listening threads to stop before returning, so the receiver is not
					 *      guaranteed to not be listening for incoming connections when stop() returns. This happens rarely,
					 *      but the result is that the message "HelloWorld2" should fail with an exception (triggering the warn
					 *      method on the logger) but it doesn't. So we can't block, but we must retry until we know the
					 *      message failed to be sent and the exception happened, which is what this test is all about. We do
					 *      that with a boolean that is tested upon continuously with sent messages until the error happens.
					 *      Then we proceed with...
					 * The receiver is started. Trapped by the listeningAt() callback.
					 * The sender sends a message.
					 * The receiver receives it, trapped by the dummy processor added to the receiver.
					 */
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean senderChannelClosed = new java.util.concurrent.atomic.AtomicBoolean(false);
					AtomicBoolean senderChannelClosed = new AtomicBoolean( false );

					doAnswer(invocation =>
					{
					 senderChannelClosed.set( true );
					 return null;
					}).when( logMock ).warn( anyString() );

					int port = PortAuthority.allocatePort();

					receiver = new NetworkReceiver(mock(typeof(NetworkReceiver.Monitor)), new ConfigurationAnonymousInnerClass(this, port)
				  , NullLogProvider.Instance)
				  {
						 public void stop() throws Exception
						 {
							  base.stop();
							  sem.release();
						 }
				  };

					sender = new NetworkSender(mock(typeof(NetworkSender.Monitor)), new ConfigurationAnonymousInnerClass(this, port)
				  , receiver, logProviderMock);

					sender.Init();
					sender.Start();

					receiver.AddNetworkChannelsListener( new NetworkChannelsListenerAnonymousInnerClass( this, sem ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean received = new java.util.concurrent.atomic.AtomicBoolean(false);
					AtomicBoolean received = new AtomicBoolean( false );

					receiver.AddMessageProcessor(message =>
					{
					 received.set( true );
					 sem.release();
					 return true;
					});

					receiver.Init();
					receiver.Start();

					sem.acquire(); // wait for start from listeningAt() in the NetworkChannelsListener

					sender.Process( Message.To( TestMessage.HelloWorld, URI.create( "cluster://127.0.0.1:" + port ), "Hello World" ) );

					sem.acquire(); // wait for process from the MessageProcessor

					receiver.Stop();

					sem.acquire(); // wait for overridden stop method in receiver

					/*
					 * This is the infernal loop of doom. We keep sending messages until one fails with a ClosedChannelException
					 * which we have no better way to grab other than through the logger.warn() call which will occur.
					 *
					 * This code will hang if the warn we rely on is removed or if the receiver never stops - in general, if
					 * the closed channel exception is not thrown. This is not an ideal failure mode but it's the best we can
					 * do, given that NetworkSender is provided with very few things from its environment.
					 */
					while ( !senderChannelClosed.get() )
					{
						 sender.Process( Message.To( TestMessage.HelloWorld, URI.create( "cluster://127.0.0.1:" + port ), "Hello World2" ) );
						 /*
						  * This sleep is not necessary, it's just nice. If it's omitted, everything will work, but we'll
						  * spam messages over the network as fast as possible. Even when the race between send and
						  * receiver.stop() does not occur, we will still send 3-4 messages through at full speed. If it
						  * does occur, then we are looking at hundreds. So we just back off a bit and let things work out.
						  */
						 Thread.Sleep( 5 );
					}

					receiver.Start();

					sem.acquire(); // wait for receiver.listeningAt()

					received.set( false );

					sender.Process( Message.To( TestMessage.HelloWorld, URI.create( "cluster://127.0.0.1:" + port ), "Hello World3" ) );

					sem.acquire(); // wait for receiver.process();

					assertTrue( received.get() );
			  }
			  finally
			  {
					if ( sender != null )
					{
						 sender.Stop();
						 sender.Shutdown();
					}
					if ( receiver != null )
					{
						 receiver.Stop();
						 receiver.Shutdown();
					}
			  }
		 }

		 private class ConfigurationAnonymousInnerClass : NetworkReceiver.Configuration
		 {
			 private readonly NetworkSenderReceiverIT _outerInstance;

			 private int _port;

			 public ConfigurationAnonymousInnerClass( NetworkSenderReceiverIT outerInstance, int port )
			 {
				 this.outerInstance = outerInstance;
				 this._port = port;
			 }

			 public HostnamePort clusterServer()
			 {
				  return new HostnamePort( "127.0.0.1:" + _port );
			 }

			 public int defaultPort()
			 {
				  return -1; // never used
			 }

			 public string name()
			 {
				  return null;
			 }
		 }

		 private class ConfigurationAnonymousInnerClass : NetworkSender.Configuration
		 {
			 private readonly NetworkSenderReceiverIT _outerInstance;

			 private int _port;

			 public ConfigurationAnonymousInnerClass( NetworkSenderReceiverIT outerInstance, int port )
			 {
				 this.outerInstance = outerInstance;
				 this._port = port;
			 }

			 public int port()
			 {
				  return _port;
			 }

			 public int defaultPort()
			 {
				  return -1; // never used
			 }
		 }

		 private class NetworkChannelsListenerAnonymousInnerClass : NetworkReceiver.NetworkChannelsListener
		 {
			 private readonly NetworkSenderReceiverIT _outerInstance;

			 private Semaphore _sem;

			 public NetworkChannelsListenerAnonymousInnerClass( NetworkSenderReceiverIT outerInstance, Semaphore sem )
			 {
				 this.outerInstance = outerInstance;
				 this._sem = sem;
			 }

			 public void listeningAt( URI me )
			 {
				  _sem.release();
			 }

			 public void channelOpened( URI to )
			 {
			 }

			 public void channelClosed( URI to )
			 {
			 }
		 }

		 private class Server : Lifecycle, MessageProcessor
		 {
			  internal readonly NetworkReceiver NetworkReceiver;
			  internal readonly NetworkSender NetworkSender;

			  internal readonly LifeSupport Life = new LifeSupport();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal AtomicBoolean ProcessedMessageConflict = new AtomicBoolean();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Server(final java.util.concurrent.CountDownLatch latch, final java.util.Map<String, String> config)
			  internal Server( System.Threading.CountdownEvent latch, IDictionary<string, string> config )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.configuration.Config conf = org.Neo4Net.kernel.configuration.Config.defaults(config);
					Config conf = Config.defaults( config );
					NetworkReceiver = Life.add(new NetworkReceiver(mock(typeof(NetworkReceiver.Monitor)), new ConfigurationAnonymousInnerClass2(this, conf)
				  , NullLogProvider.Instance));

					NetworkSender = Life.add(new NetworkSender(mock(typeof(NetworkSender.Monitor)), new ConfigurationAnonymousInnerClass2(this, conf)
				  , NetworkReceiver, NullLogProvider.Instance));

					Life.add( new LifecycleAdapterAnonymousInnerClass( this, latch ) );
			  }

			  private class ConfigurationAnonymousInnerClass2 : NetworkReceiver.Configuration
			  {
				  private readonly Server _outerInstance;

				  private Config _conf;

				  public ConfigurationAnonymousInnerClass2( Server outerInstance, Config conf )
				  {
					  this.outerInstance = outerInstance;
					  this._conf = conf;
				  }

				  public HostnamePort clusterServer()
				  {
						return _conf.get( ClusterSettings.cluster_server );
				  }

				  public int defaultPort()
				  {
						return -1; // never used
				  }

				  public string name()
				  {
						return null;
				  }
			  }

			  private class ConfigurationAnonymousInnerClass2 : NetworkSender.Configuration
			  {
				  private readonly Server _outerInstance;

				  private Config _conf;

				  public ConfigurationAnonymousInnerClass2( Server outerInstance, Config conf )
				  {
					  this.outerInstance = outerInstance;
					  this._conf = conf;
				  }

				  public int defaultPort()
				  {
						return -1; // never used
				  }

				  public int port()
				  {
						return _conf.get( ClusterSettings.cluster_server ).Port;
				  }
			  }

			  private class LifecycleAdapterAnonymousInnerClass : LifecycleAdapter
			  {
				  private readonly Server _outerInstance;

				  private System.Threading.CountdownEvent _latch;

				  public LifecycleAdapterAnonymousInnerClass( Server outerInstance, System.Threading.CountdownEvent latch )
				  {
					  this.outerInstance = outerInstance;
					  this._latch = latch;
				  }

				  public override void start()
				  {
						_outerInstance.networkReceiver.addMessageProcessor(message =>
						{
						 // server receives a message
						 _outerInstance.processedMessage.set( true );
						 _latch.Signal();
						 return true;
						});
				  }
			  }

			  public override void Init()
			  {
			  }

			  public override void Start()
			  {

					Life.start();
			  }

			  public override void Stop()
			  {
					Life.stop();
			  }

			  public override void Shutdown()
			  {
			  }

			  public override bool Process<T1>( Message<T1> message ) where T1 : MessageType
			  {
					// server sends a message
					this.ProcessedMessageConflict.set( true );
					return NetworkSender.process( message );
			  }

			  public virtual bool ProcessedMessage()
			  {
					return this.ProcessedMessageConflict.get();
			  }
		 }
	}

}