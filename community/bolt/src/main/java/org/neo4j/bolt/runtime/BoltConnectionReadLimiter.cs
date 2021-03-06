﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.runtime
{
	using Channel = io.netty.channel.Channel;

	using Job = Org.Neo4j.Bolt.v1.runtime.Job;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	/// <summary>
	/// Queue monitor that changes <seealso cref="Channel"/> auto-read setting based on the job queue size.
	/// Methods <seealso cref="enqueued(BoltConnection, Job)"/> and <seealso cref="drained(BoltConnection, System.Collections.ICollection)"/> are synchronized to make sure
	/// queue size and channel auto-read are modified together as an atomic operation.
	/// </summary>
	public class BoltConnectionReadLimiter : BoltConnectionQueueMonitor
	{
		 private readonly Log _log;
		 private readonly int _lowWatermark;
		 private readonly int _highWatermark;

		 private int _queueSize;

		 public BoltConnectionReadLimiter( LogService logService, int lowWatermark, int highWatermark )
		 {
			  if ( highWatermark <= 0 )
			  {
					throw new System.ArgumentException( "invalid highWatermark value" );
			  }

			  if ( lowWatermark < 0 || lowWatermark >= highWatermark )
			  {
					throw new System.ArgumentException( "invalid lowWatermark value" );
			  }

			  this._log = logService.GetInternalLog( this.GetType() );
			  this._lowWatermark = lowWatermark;
			  this._highWatermark = highWatermark;
		 }

		 protected internal virtual int LowWatermark
		 {
			 get
			 {
				  return _lowWatermark;
			 }
		 }

		 protected internal virtual int HighWatermark
		 {
			 get
			 {
				  return _highWatermark;
			 }
		 }

		 public override void Enqueued( BoltConnection to, Job job )
		 {
			 lock ( this )
			 {
				  _queueSize += 1;
				  CheckLimitsOnEnqueue( to );
			 }
		 }

		 public override void Drained( BoltConnection from, ICollection<Job> batch )
		 {
			 lock ( this )
			 {
				  _queueSize -= batch.Count;
				  CheckLimitsOnDequeue( from );
			 }
		 }

		 private void CheckLimitsOnEnqueue( BoltConnection connection )
		 {
			  Channel channel = connection.Channel();

			  if ( _queueSize > _highWatermark && channel.config().AutoRead )
			  {
					if ( _log != null )
					{
						 _log.warn( "Channel [%s]: client produced %d messages on the worker queue, auto-read is being disabled.", channel.remoteAddress(), _queueSize );
					}

					channel.config().AutoRead = false;
			  }
		 }

		 private void CheckLimitsOnDequeue( BoltConnection connection )
		 {
			  Channel channel = connection.Channel();

			  if ( _queueSize <= _lowWatermark && !channel.config().AutoRead )
			  {
					if ( _log != null )
					{
						 _log.warn( "Channel [%s]: consumed messages on the worker queue below %d, auto-read is being enabled.", channel.remoteAddress(), _lowWatermark );
					}

					channel.config().AutoRead = true;
			  }
		 }

	}

}