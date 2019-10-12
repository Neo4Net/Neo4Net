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

	using Log = Neo4Net.Logging.Log;

	public class SimpleNettyChannel : Channel
	{
		 private readonly Log _log;
		 private readonly io.netty.channel.Channel _channel;
		 private volatile bool _disposed;

		 public SimpleNettyChannel( io.netty.channel.Channel channel, Log log )
		 {
			  this._channel = channel;
			  this._log = log;
		 }

		 public virtual bool Disposed
		 {
			 get
			 {
				  return _disposed;
			 }
		 }

		 public override void Dispose()
		 {
			 lock ( this )
			 {
				  _log.info( "Disposing channel: " + _channel );
				  _disposed = true;
				  _channel.close();
			 }
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return _channel.Open;
			 }
		 }

		 public override Future<Void> Write( object msg )
		 {
			  CheckDisposed();
			  return _channel.write( msg );
		 }

		 public override Future<Void> WriteAndFlush( object msg )
		 {
			  CheckDisposed();
			  return _channel.writeAndFlush( msg );
		 }

		 private void CheckDisposed()
		 {
			  if ( _disposed )
			  {
					throw new System.InvalidOperationException( "sending on disposed channel" );
			  }
		 }
	}

}