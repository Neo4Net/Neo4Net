using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.scheduler
{

	using DeferredExecutor = Neo4Net.Scheduler.DeferredExecutor;

	/// <summary>
	/// Buffers all tasks sent to it, and is able to replay those messages into
	/// another Executor.
	/// <para>
	/// This will replay tasks in the order they are received.
	/// </para>
	/// <para>
	/// You should also not use this executor, when there is a risk that it can be
	/// subjected to an unbounded quantity of tasks, since the buffer keeps
	/// all messages until it gets a chance to replay them.
	/// </para>
	/// </summary>
	public class BufferingExecutor : DeferredExecutor
	{
		 private readonly LinkedList<ThreadStart> _buffer = new LinkedList<ThreadStart>();

		 private volatile Executor _realExecutor;

		 public virtual void SatisfyWith( Executor executor )
		 {
			  lock ( this )
			  {
					if ( _realExecutor != null )
					{
						 throw new Exception( "real executor is already set. Cannot override" );
					}
					_realExecutor = executor;
					ReplayBuffer();
			  }
		 }

		 private void ReplayBuffer()
		 {
			  ThreadStart command = PollRunnable();
			  while ( command != null )
			  {
					_realExecutor.execute( command );
					command = PollRunnable();
			  }
		 }

		 private ThreadStart PollRunnable()
		 {
			  lock ( _buffer )
			  {
					return _buffer.RemoveFirst();
			  }
		 }

		 private void QueueRunnable( ThreadStart command )
		 {
			  lock ( _buffer )
			  {
					_buffer.AddLast( command );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void execute(@Nonnull Runnable command)
		 public override void Execute( ThreadStart command )
		 {
			  // First do an unsynchronized check to see if a realExecutor is present
			  if ( _realExecutor != null )
			  {
					_realExecutor.execute( command );
					return;
			  }

			  // Now do a synchronized check to avoid race conditions
			  lock ( this )
			  {
					if ( _realExecutor != null )
					{
						 _realExecutor.execute( command );
						 return;
					}
					else
					{
						 QueueRunnable( command );
					}
			  }
		 }
	}

}