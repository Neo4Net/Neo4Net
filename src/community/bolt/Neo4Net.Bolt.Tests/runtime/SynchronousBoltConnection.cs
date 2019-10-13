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
namespace Neo4Net.Bolt.runtime
{
	using Channel = io.netty.channel.Channel;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;

	using TransportThrottleGroup = Neo4Net.Bolt.transport.TransportThrottleGroup;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using Job = Neo4Net.Bolt.v1.runtime.Job;
	using ChunkedOutput = Neo4Net.Bolt.v1.transport.ChunkedOutput;

	public class SynchronousBoltConnection : BoltConnection
	{
		 private readonly EmbeddedChannel _channel;
		 private readonly PackOutput _output;
		 private readonly BoltStateMachine _machine;

		 public SynchronousBoltConnection( BoltStateMachine machine )
		 {
			  this._channel = new EmbeddedChannel();
			  this._output = new ChunkedOutput( this._channel, TransportThrottleGroup.NO_THROTTLE );
			  this._machine = machine;
		 }

		 public override string Id()
		 {
			  return _channel.id().asLongText();
		 }

		 public override SocketAddress LocalAddress()
		 {
			  return _channel.localAddress();
		 }

		 public override SocketAddress RemoteAddress()
		 {
			  return _channel.remoteAddress();
		 }

		 public override Channel Channel()
		 {
			  return _channel;
		 }

		 public override PackOutput Output()
		 {
			  return _output;
		 }

		 public override bool HasPendingJobs()
		 {
			  return false;
		 }

		 public override void Start()
		 {

		 }

		 public override void Enqueue( Job job )
		 {
			  try
			  {
					job.Perform( _machine );
			  }
			  catch ( BoltConnectionFatality connectionFatality )
			  {
					throw new Exception( connectionFatality );
			  }
		 }

		 public override bool ProcessNextBatch()
		 {
			  return true;
		 }

		 public override void HandleSchedulingError( Exception t )
		 {

		 }

		 public override void Interrupt()
		 {
			  _machine.interrupt();
		 }

		 public override void Stop()
		 {
			  _channel.finishAndReleaseAll();
			  _machine.close();
		 }
	}

}