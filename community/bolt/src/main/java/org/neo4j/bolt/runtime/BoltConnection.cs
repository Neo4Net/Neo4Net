using System;

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


	using PackOutput = Org.Neo4j.Bolt.v1.packstream.PackOutput;
	using Job = Org.Neo4j.Bolt.v1.runtime.Job;

	public interface BoltConnection
	{

		 /// <summary>
		 /// Returns a unique, not changing over time string that can safely be used to identify this connection.
		 /// </summary>
		 /// <returns> identifier </returns>
		 string Id();

		 /// <summary>
		 /// Returns the local (server) socket address that this client is bound to
		 /// </summary>
		 /// <returns> local endpoint </returns>
		 SocketAddress LocalAddress();

		 /// <summary>
		 /// Returns the remote (client) socket address that this client established the connection from.
		 /// </summary>
		 /// <returns> remote endpoint </returns>
		 SocketAddress RemoteAddress();

		 /// <summary>
		 /// Returns the underlying raw netty channel
		 /// </summary>
		 /// <returns> netty channel </returns>
		 Channel Channel();

		 /// <summary>
		 /// Returns the packer that's used to generate response streams
		 /// </summary>
		 PackOutput Output();

		 /// <summary>
		 /// Returns whether there's any pending Job waiting to be processed
		 /// </summary>
		 /// <returns> true when there's at least one job in the queue </returns>
		 bool HasPendingJobs();

		 /// <summary>
		 /// Executes extra initialisation routines before taking this connection into use
		 /// </summary>
		 void Start();

		 /// <summary>
		 /// Adds submitted job to the job queue for execution (at the earliest time possible)
		 /// </summary>
		 /// <param name="job"> the job to be added </param>
		 void Enqueue( Job job );

		 /// <summary>
		 /// Executes a batch of queued jobs, which is executed in an another thread (which is part of a thread pool)
		 /// </summary>
		 /// <returns> false when no more processing should take place (i.e. connection is closed) </returns>
		 bool ProcessNextBatch();

		 /// <summary>
		 /// Invoked when an exception is caught during the scheduling of the pending jobs. The caught exception would mostly
		 /// be <seealso cref="RejectedExecutionException"/> which is thrown by the thread pool executor when it fails to accept
		 /// submitted jobs
		 /// </summary>
		 /// <param name="t"> the exception occurred during scheduling </param>
		 void HandleSchedulingError( Exception t );

		 /// <summary>
		 /// Interrupt and (possibly) stop the current running job, but continue processing next jobs
		 /// </summary>
		 void Interrupt();

		 /// <summary>
		 /// Stops this connection
		 /// </summary>
		 void Stop();

	}

}