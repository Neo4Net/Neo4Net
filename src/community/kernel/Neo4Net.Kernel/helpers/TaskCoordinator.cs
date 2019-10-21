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
namespace Neo4Net.Helpers
{

	using Neo4Net.Functions;

	/// <summary>
	/// Represents a collection point for various <seealso cref="TaskControl"/> instances that need to be waited on and potentially
	/// cancelled en mass. Instances of <seealso cref="TaskControl"/> acquired through the <seealso cref="newInstance()"/> method can be
	/// notified of cancellation with the semantics of <seealso cref="CancellationRequest"/>.
	/// </summary>
	[Obsolete]
	public class TaskCoordinator : Cancelable, IFactory<TaskControl>
	{
		 private static readonly AtomicIntegerFieldUpdater<TaskCoordinator> _tasksUpdater = AtomicIntegerFieldUpdater.newUpdater( typeof( TaskCoordinator ), "tasks" );
		 private volatile bool _cancelled;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("UnusedDeclaration") private volatile int tasks;
		 private volatile int _tasks;
		 private readonly long _sleepTime;
		 private readonly TimeUnit _sleepUnit;

		 [Obsolete]
		 public TaskCoordinator( long sleepTime, TimeUnit sleepUnit )
		 {
			  this._sleepTime = sleepTime;
			  this._sleepUnit = sleepUnit;
		 }

		 [Obsolete]
		 public override void Cancel()
		 {
			  _cancelled = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitCompletion() throws InterruptedException
		 [Obsolete]
		 public virtual void AwaitCompletion()
		 {
			  while ( _tasks != 0 )
			  {
					_sleepUnit.sleep( _sleepTime );
			  }
		 }

		 [Obsolete]
		 public override TaskControl NewInstance()
		 {
			  if ( _cancelled )
			  {
					throw new System.InvalidOperationException( "This manager has already been cancelled." );
			  }
			  _tasksUpdater.incrementAndGet( this );
			  return new TaskControlAnonymousInnerClass( this );
		 }

		 private class TaskControlAnonymousInnerClass : TaskControl
		 {
			 private readonly TaskCoordinator _outerInstance;

			 public TaskControlAnonymousInnerClass( TaskCoordinator outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 private volatile bool closed;

			 public void close()
			 {
				  if ( !closed )
				  {
						closed = true;
						_tasksUpdater.decrementAndGet( _outerInstance );
				  }
			 }

			 public bool cancellationRequested()
			 {
				  return _outerInstance.cancelled;
			 }
		 }
	}

}