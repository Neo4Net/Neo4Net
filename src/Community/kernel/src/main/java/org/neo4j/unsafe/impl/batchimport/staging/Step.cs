﻿/*
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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using StepStats = Neo4Net.@unsafe.Impl.Batchimport.stats.StepStats;

	/// <summary>
	/// One step in <seealso cref="Stage"/>, where a <seealso cref="Stage"/> is a sequence of steps. Each step works on batches.
	/// Batches are typically received from an upstream step, or produced in the step itself. If there are more steps
	/// <seealso cref="setDownstream(Step) downstream"/> then processed batches are passed down. Each step has maximum
	/// "work-ahead" size where it awaits the downstream step to catch up if the queue size goes beyond that number.
	/// 
	/// Batches are associated with a ticket, which is simply a long value incremented for each batch.
	/// It's the first step that is responsible for generating these tickets, which will stay unchanged with
	/// each batch all the way through the stage. Steps that have multiple threads processing batches can process
	/// received batches in any order, but must make sure to send batches to its downstream
	/// (i.e. calling <seealso cref="receive(long, object)"/> on its downstream step) ordered by ticket.
	/// </summary>
	/// @param <T> the type of batch objects received from upstream. </param>
	public interface Step<T> : Parallelizable, AutoCloseable, Panicable
	{
		 /// <summary>
		 /// Whether or not tickets arrive in <seealso cref="receive(long, object)"/> ordered by ticket number.
		 /// </summary>

		 /// <summary>
		 /// Starts the processing in this step, such that calls to <seealso cref="receive(long, object)"/> can be accepted.
		 /// </summary>
		 /// <param name="orderingGuarantees"> which ordering guarantees that will be upheld. </param>
		 void Start( int orderingGuarantees );

		 /// <returns> name of this step. </returns>
		 string Name();

		 /// <summary>
		 /// Receives a batch from upstream, queues it for processing.
		 /// </summary>
		 /// <param name="ticket"> ticket associates with the batch. Tickets are generated by producing steps and must follow
		 /// each batch all the way through a stage. </param>
		 /// <param name="batch"> the batch object to queue for processing. </param>
		 /// <returns> how long it time (millis) was spent waiting for a spot in the queue. </returns>
		 long Receive( long ticket, T batch );

		 /// <returns> statistics about this step at this point in time. </returns>
		 StepStats Stats();

		 /// <summary>
		 /// Called by upstream to let this step know that it will not send any more batches.
		 /// </summary>
		 void EndOfUpstream();

		 /// <returns> {@code true} if this step has received AND processed all batches from upstream, or in
		 /// the case of a producer, that this step has produced all batches. </returns>
		 bool Completed { get; }

		 /// <summary>
		 /// Waits for this step to become completed, i.e. until <seealso cref="isCompleted()"/> returns {@code true}. If this step is already completed
		 /// then this method will return immediately.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void awaitCompleted() throws InterruptedException;
		 void AwaitCompleted();

		 /// <summary>
		 /// Called by the <seealso cref="Stage"/> when setting up the stage. This will form a pipeline of steps,
		 /// making up the stage. </summary>
		 /// <param name="downstreamStep"> <seealso cref="Step"/> to send batches to downstream. </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void setDownstream(Step<?> downstreamStep);
		 Step<T1> Downstream<T1> { set; }

		 /// <summary>
		 /// Closes any resources kept open by this step. Called after a <seealso cref="Stage"/> is executed, whether successful or not.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws Exception;
		 void Close();
	}

	public static class Step_Fields
	{
		 public const int ORDER_SEND_DOWNSTREAM = 0x1;
		 public const int RECYCLE_BATCHES = 0x2;
	}

}