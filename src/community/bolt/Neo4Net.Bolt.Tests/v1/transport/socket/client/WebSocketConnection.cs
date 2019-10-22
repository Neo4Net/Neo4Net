using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Bolt.v1.transport.socket.client
{
	using RemoteEndpoint = org.eclipse.jetty.websocket.api.RemoteEndpoint;
	using Session = org.eclipse.jetty.websocket.api.Session;
	using WebSocketListener = org.eclipse.jetty.websocket.api.WebSocketListener;
	using WebSocketClient = org.eclipse.jetty.websocket.client.WebSocketClient;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;


	public class WebSocketConnection : TransportConnection, WebSocketListener
	{
		 private readonly System.Func<WebSocketClient> _clientSupplier;
		 private readonly System.Func<HostnamePort, URI> _uriGenerator;

		 private readonly sbyte[] _poisonPill = "poison".Bytes;

		 private WebSocketClient _client;
		 private RemoteEndpoint _server;

		 // Incoming data goes on this queue
		 private readonly LinkedBlockingQueue<sbyte[]> _received = new LinkedBlockingQueue<sbyte[]>();

		 // Current input data being handled, popped off of 'received' queue
		 private sbyte[] _currentReceiveBuffer;

		 // Index into the current receive buffer
		 private int _currentReceiveIndex;

		 public WebSocketConnection() : this(WebSocketClient::new, address -> URI.create("ws://" + address.Host + ":" + address.Port))
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 public WebSocketConnection( System.Func<WebSocketClient> clientSupplier, System.Func<HostnamePort, URI> uriGenerator )
		 {
			  this._clientSupplier = clientSupplier;
			  this._uriGenerator = uriGenerator;
		 }

		 internal WebSocketConnection( WebSocketClient client ) : this( null, null )
		 {
			  this._client = client;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransportConnection connect(org.Neo4Net.helpers.HostnamePort address) throws Exception
		 public override TransportConnection Connect( HostnamePort address )
		 {
			  URI target = _uriGenerator.apply( address );

			  _client = _clientSupplier.get();
			  _client.start();

			  Session session;
			  try
			  {
					session = _client.connect( this, target ).get( 10, SECONDS );
			  }
			  catch ( Exception e )
			  {
					throw new IOException( "Failed to connect to the server within 10 seconds", e );
			  }

			  _server = session.Remote;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransportConnection send(byte[] rawBytes) throws java.io.IOException
		 public override TransportConnection Send( sbyte[] rawBytes )
		 {
			  // The WS client *mutates* the buffer we give it, so we need to copy it here to allow the caller to retain
			  // ownership
			  ByteBuffer wrap = ByteBuffer.wrap( Arrays.copyOf( rawBytes, rawBytes.Length ) );
			  _server.sendBytes( wrap );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] recv(int length) throws java.io.IOException, InterruptedException
		 public override sbyte[] Recv( int length )
		 {
			  int remaining = length;
			  sbyte[] target = new sbyte[remaining];
			  while ( remaining > 0 )
			  {
					WaitForReceivedData( length, remaining, target );
					for ( int i = 0; i < Math.Min( remaining, _currentReceiveBuffer.Length - _currentReceiveIndex ); i++ )
					{
						 target[length - remaining] = _currentReceiveBuffer[_currentReceiveIndex++];
						 remaining--;
					}
			  }
			  return target;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitForReceivedData(int length, int remaining, byte[] target) throws InterruptedException, java.io.IOException
		 private void WaitForReceivedData( int length, int remaining, sbyte[] target )
		 {
			  long start = DateTimeHelper.CurrentUnixTimeMillis();
			  while ( _currentReceiveBuffer == null || _currentReceiveIndex >= _currentReceiveBuffer.Length )
			  {
					_currentReceiveIndex = 0;
					_currentReceiveBuffer = _received.poll( 10, MILLISECONDS );

					if ( ( _currentReceiveBuffer == null && ( _client.Stopped || _client.Stopping ) ) || _currentReceiveBuffer == _poisonPill )
					{
						 // no data received
						 throw new IOException( "Connection closed while waiting for data from the server." );
					}
					if ( DateTimeHelper.CurrentUnixTimeMillis() - start > 30_000 )
					{
						 throw new IOException( "Waited 30 seconds for " + remaining + " bytes, " + "" + ( length - remaining ) + " was received: " + HexPrinter.hex( ByteBuffer.wrap( target ), 0, length - remaining ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void disconnect() throws java.io.IOException
		 public override void Disconnect()
		 {
			  if ( _client != null )
			  {
					try
					{
						 _client.stop();
					}
					catch ( Exception e )
					{
						 throw new IOException( e );
					}
			  }
		 }

		 public override void OnWebSocketBinary( sbyte[] bytes, int i, int i2 )
		 {
			  _received.add( bytes );
		 }

		 public override void OnWebSocketClose( int i, string s )
		 {
			  _received.add( _poisonPill );
		 }

		 public override void OnWebSocketConnect( Session session )
		 {
		 }

		 public override void OnWebSocketError( Exception throwable )
		 {
		 }

		 public override void OnWebSocketText( string s )
		 {
		 }
	}

}