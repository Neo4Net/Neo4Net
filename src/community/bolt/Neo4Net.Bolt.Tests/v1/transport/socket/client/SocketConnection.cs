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

	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;

	public class SocketConnection : TransportConnection
	{
		 private Socket _socket;
		 private Stream @in;
		 private Stream @out;

		 public SocketConnection() : this(new Socket())
		 {
		 }

		 public SocketConnection( Socket socket )
		 {
			  this._socket = socket;
		 }

		 protected internal virtual Socket Socket
		 {
			 set
			 {
				  this._socket = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransportConnection connect(org.Neo4Net.helpers.HostnamePort address) throws java.io.IOException
		 public override TransportConnection Connect( HostnamePort address )
		 {
			  _socket.SoTimeout = 30000 * 1000; // TOOD

			  _socket.connect( new InetSocketAddress( address.Host, address.Port ) );
			  @in = _socket.InputStream;
			  @out = _socket.OutputStream;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransportConnection send(byte[] rawBytes) throws java.io.IOException
		 public override TransportConnection Send( sbyte[] rawBytes )
		 {
			  @out.Write( rawBytes, 0, rawBytes.Length );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] recv(int length) throws java.io.IOException
		 public override sbyte[] Recv( int length )
		 {
			  sbyte[] bytes = new sbyte[length];
			  int left = length;
			  int read;

			  try
			  {
					while ( left > 0 && ( read = @in.Read( bytes, length - left, left ) ) != -1 )
					{
						 left -= read;
					}
			  }
			  catch ( SocketTimeoutException )
			  {
					throw new SocketTimeoutException( "Reading data timed out, missing " + left + " bytes. Buffer: " + HexPrinter.hex( bytes ) );
			  }
			  //all the bytes could not be read, fail
			  if ( left != 0 )
			  {
					throw new IOException( "Failed to read " + length + " bytes, missing " + left + " bytes. Buffer: " + HexPrinter.hex( bytes ) );
			  }
			  return bytes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void disconnect() throws java.io.IOException
		 public override void Disconnect()
		 {
			  if ( _socket != null && _socket.Connected )
			  {
					_socket.close();
			  }
		 }
	}

}