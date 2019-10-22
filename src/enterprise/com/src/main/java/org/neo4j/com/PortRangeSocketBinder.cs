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
namespace Neo4Net.com
{
	using ServerBootstrap = org.jboss.netty.bootstrap.ServerBootstrap;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelException = org.jboss.netty.channel.ChannelException;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;

	public class PortRangeSocketBinder
	{
		 private ServerBootstrap _bootstrap;
		 private const string ALL_INTERFACES_ADDRESS = "0.0.0.0";

		 public PortRangeSocketBinder( ServerBootstrap bootstrap )
		 {
			  this._bootstrap = bootstrap;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Connection bindToFirstAvailablePortInRange(org.Neo4Net.helpers.HostnamePort serverAddress) throws org.jboss.netty.channel.ChannelException
		 public virtual Connection BindToFirstAvailablePortInRange( HostnamePort serverAddress )
		 {
			  int[] ports = serverAddress.Ports;
			  string host = serverAddress.Host;

			  Channel channel;
			  InetSocketAddress socketAddress;
			  ChannelException lastException = null;

			  PortIterator portIterator = new PortIterator( ports );
			  while ( portIterator.MoveNext() )
			  {
					int? port = portIterator.Current;
					if ( string.ReferenceEquals( host, null ) || host.Equals( ALL_INTERFACES_ADDRESS ) )
					{
						 socketAddress = new InetSocketAddress( port );
					}
					else
					{
						 socketAddress = new InetSocketAddress( host, port );
					}
					try
					{
						 channel = _bootstrap.bind( socketAddress );
						 return new Connection( socketAddress, channel );
					}
					catch ( ChannelException e )
					{
						 if ( lastException != null )
						 {
							  e.addSuppressed( lastException );
						 }
						 lastException = e;
					}
			  }

			  throw Objects.requireNonNull( lastException );
		 }
	}

}