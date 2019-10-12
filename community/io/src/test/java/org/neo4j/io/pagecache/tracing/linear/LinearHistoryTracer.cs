using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.Io.pagecache.tracing.linear
{

	/// <summary>
	/// Records a linearized history of all (global and cursors) internal page cache events.
	/// Only use this for debugging internal data race bugs and the like, in the page cache.
	/// </summary>
	internal class LinearHistoryTracer
	{
		private bool InstanceFieldsInitialized = false;

		public LinearHistoryTracer()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			@out = new PrintStream( _bufferOut );
		}

		 private readonly AtomicReference<HEvents.HEvent> _history = new AtomicReference<HEvents.HEvent>();

		 // The output buffering mechanics are pre-allocated in case we have to deal with low-memory situations.
		 // The output switching is guarded by the monitor lock on the LinearHistoryPageCacheTracer instance.
		 // The class name cache is similarly guarded the monitor lock. In short, only a single thread can print history
		 // at a time.
		 private readonly SwitchableBufferedOutputStream _bufferOut = new SwitchableBufferedOutputStream();
		 private PrintStream @out;

		 internal virtual bool ProcessHistory( System.Action<HEvents.HEvent> processor )
		 {
			 lock ( this )
			 {
				  HEvents.HEvent events = _history.getAndSet( null );
				  if ( events == null )
				  {
						return false;
				  }
				  events = HEvents.HEvent.Reverse( events );
				  while ( events != null )
				  {
						processor( events );
						events = events.Prev;
				  }
				  return true;
			 }
		 }

		 internal virtual E Add<E>( E @event ) where E : HEvents.HEvent
		 {
			  HEvents.HEvent prev = _history.getAndSet( @event );
			  @event.Prev = prev == null ? HEvents.HEvent.end : prev;
			  return @event;
		 }

		 internal virtual void PrintHistory( PrintStream outputStream )
		 {
			 lock ( this )
			 {
				  _bufferOut.Out = outputStream;
				  if ( !ProcessHistory( new HistoryPrinter( this ) ) )
				  {
						@out.println( "No events recorded." );
				  }
				  @out.flush();
			 }
		 }

		 private class SwitchableBufferedOutputStream : BufferedOutputStream
		 {

			  internal SwitchableBufferedOutputStream() : base(null) // No output target by default. This is changed in printHistory.
			  {
					//noinspection ConstantConditions
			  }

			  public virtual Stream Out
			  {
				  set
				  {
						base.@out = value;
				  }
			  }
		 }

		 private class HistoryPrinter : System.Action<HEvents.HEvent>
		 {
			 private readonly LinearHistoryTracer _outerInstance;

			  internal readonly IList<HEvents.HEvent> ConcurrentIntervals;

			  internal HistoryPrinter( LinearHistoryTracer outerInstance )
			  {
				  this._outerInstance = outerInstance;
					this.ConcurrentIntervals = new LinkedList<HEvents.HEvent>();
			  }

			  public override void Accept( HEvents.HEvent @event )
			  {
					string exceptionLinePrefix = exceptionLinePrefix( ConcurrentIntervals.Count );
					if ( @event.GetType() == typeof(HEvents.EndHEvent) )
					{
						 HEvents.EndHEvent endHEvent = ( HEvents.EndHEvent ) @event;
						 int idx = ConcurrentIntervals.IndexOf( endHEvent.Event );
						 Putcs( outerInstance.@out, '|', idx );
						 outerInstance.@out.print( '-' );
						 int left = ConcurrentIntervals.Count - idx - 1;
						 Putcs( outerInstance.@out, '|', left );
						 outerInstance.@out.print( "   " );
						 endHEvent.Print( outerInstance.@out, exceptionLinePrefix );
						 ConcurrentIntervals.RemoveAt( idx );
						 if ( left > 0 )
						 {
							  outerInstance.@out.println();
							  Putcs( outerInstance.@out, '|', idx );
							  Putcs( outerInstance.@out, '/', left );
						 }
					}
					else if ( @event is HEvents.IntervalHEvent )
					{
						 Putcs( outerInstance.@out, '|', ConcurrentIntervals.Count );
						 outerInstance.@out.print( "+   " );
						 @event.Print( outerInstance.@out, exceptionLinePrefix );
						 ConcurrentIntervals.Add( @event );
					}
					else
					{
						 Putcs( outerInstance.@out, '|', ConcurrentIntervals.Count );
						 outerInstance.@out.print( ">   " );
						 @event.Print( outerInstance.@out, exceptionLinePrefix );
					}
					outerInstance.@out.println();
			  }

			  internal virtual string ExceptionLinePrefix( int size )
			  {
					StringBuilder sb = new StringBuilder();
					for ( int i = 0; i < size; i++ )
					{
						 sb.Append( '|' );
					}
					sb.Append( ":  " );
					return sb.ToString();
			  }

			  internal virtual void Putcs( PrintStream @out, char c, int count )
			  {
					for ( int i = 0; i < count; i++ )
					{
						 @out.print( c );
					}
			  }
		 }
	}

}