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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	public abstract class EntryUpdater<Key> : IDisposable
	{
		 private readonly Lock @lock;
		 private Thread _thread;

		 internal EntryUpdater( Lock @lock )
		 {
			  this.@lock = @lock;
			  if ( @lock != null )
			  {
					this._thread = Thread.CurrentThread;
					@lock.@lock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void apply(Key key, ValueUpdate update) throws java.io.IOException;
		 public abstract void Apply( Key key, ValueUpdate update );

		 public override void Close()
		 {
			  if ( _thread != null )
			  {
					if ( _thread != Thread.CurrentThread )
					{
						 throw new System.InvalidOperationException( "Closing on different thread." );
					}
					@lock.unlock();
					_thread = null;
			  }
		 }

		 protected internal virtual void EnsureOpenOnSameThread()
		 {
			  if ( _thread != Thread.CurrentThread )
			  {
					throw new System.InvalidOperationException( "The updater is not available." );
			  }
		 }

		 protected internal virtual void EnsureOpen()
		 {
			  if ( _thread == null )
			  {
					throw new System.InvalidOperationException( "The updater is not available." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static <Key> EntryUpdater<Key> noUpdates()
		 internal static EntryUpdater<Key> NoUpdates<Key>()
		 {
			  return NO_UPDATES;
		 }

		 private static readonly EntryUpdater NO_UPDATES = new EntryUpdaterAnonymousInnerClass();

		 private class EntryUpdaterAnonymousInnerClass : EntryUpdater
		 {
			 public EntryUpdaterAnonymousInnerClass() : base(null)
			 {
			 }

			 public override void apply( object o, ValueUpdate update )
			 {
			 }

			 public override void close()
			 {
			 }
		 }
	}

}