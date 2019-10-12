/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Bolt
{
	using Channel = io.netty.channel.Channel;

	using TrackedNetworkConnection = Neo4Net.Kernel.api.net.TrackedNetworkConnection;
	using BoltConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.BoltConnectionInfo;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;

	/// <summary>
	/// A channel through which Bolt messaging can occur.
	/// </summary>
	public class BoltChannel : TrackedNetworkConnection
	{
		 private readonly string _id;
		 private readonly long _connectTime;
		 private readonly string _connector;
		 private readonly Channel _rawChannel;

		 private volatile string _username;
		 private volatile string _userAgent;
		 private volatile ClientConnectionInfo _info;

		 public BoltChannel( string id, string connector, Channel rawChannel )
		 {
			  this._id = id;
			  this._connectTime = DateTimeHelper.CurrentUnixTimeMillis();
			  this._connector = connector;
			  this._rawChannel = rawChannel;
			  this._info = CreateConnectionInfo();
		 }

		 public virtual Channel RawChannel()
		 {
			  return _rawChannel;
		 }

		 public virtual ClientConnectionInfo Info()
		 {
			  return _info;
		 }

		 public override string Id()
		 {
			  return _id;
		 }

		 public override long ConnectTime()
		 {
			  return _connectTime;
		 }

		 public override string Connector()
		 {
			  return _connector;
		 }

		 public override SocketAddress ServerAddress()
		 {
			  return _rawChannel.localAddress();
		 }

		 public override SocketAddress ClientAddress()
		 {
			  return _rawChannel.remoteAddress();
		 }

		 public override string Username()
		 {
			  return _username;
		 }

		 public override string UserAgent()
		 {
			  return _userAgent;
		 }

		 public override void UpdateUser( string username, string userAgent )
		 {
			  this._username = username;
			  this._userAgent = userAgent;
			  this._info = CreateConnectionInfo();
		 }

		 public override void Close()
		 {
			  Channel rawChannel = rawChannel();
			  if ( rawChannel.Open )
			  {
					rawChannel.close().syncUninterruptibly();
			  }
		 }

		 public override string ToString()
		 {
			  return "BoltChannel{" +
						"id='" + _id + '\'' +
						", connectTime=" + _connectTime +
						", connector='" + _connector + '\'' +
						", rawChannel=" + _rawChannel +
						", username='" + _username + '\'' +
						", userAgent='" + _userAgent + '\'' +
						'}';
		 }

		 private ClientConnectionInfo CreateConnectionInfo()
		 {
			  return new BoltConnectionInfo( _id, _username, _userAgent, ClientAddress(), ServerAddress() );
		 }
	}

}