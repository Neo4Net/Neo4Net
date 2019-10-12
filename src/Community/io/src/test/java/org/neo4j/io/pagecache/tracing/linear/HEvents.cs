using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Io.pagecache.tracing.linear
{


	/// <summary>
	/// Container of events for page cache tracers that are used to build linear historical representation of page cache
	/// events.
	/// In case if event can generate any other event it will properly add it to corresponding tracer and it will be also
	/// tracked. </summary>
	/// <seealso cref= LinearHistoryTracer </seealso>
	internal class HEvents
	{
		 private HEvents()
		 {
		 }

		 internal sealed class EndHEvent : HEvent
		 {
			  internal static readonly IDictionary<Type, string> ClassSimpleNameCache = new IdentityHashMap<Type, string>();
			  internal IntervalHEvent Event;

			  internal EndHEvent( IntervalHEvent @event )
			  {
					this.Event = @event;
			  }

			  public override void Print( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( '-' );
					base.Print( @out, exceptionLinePrefix );
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( ", elapsedMicros:" );
					@out.print( ( Time - Event.time ) / 1000 );
					@out.print( ", endOf:" );
					Type eventClass = Event.GetType();
					string className = ClassSimpleNameCache.computeIfAbsent( eventClass, k => eventClass.Name );
					@out.print( className );
					@out.print( '#' );
					@out.print( System.identityHashCode( Event ) );
			  }
		 }

		 internal class MappedFileHEvent : HEvent
		 {
			  internal File File;

			  internal MappedFileHEvent( File file )
			  {
					this.File = file;
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					Print( @out, File );
			  }
		 }

		 internal class UnmappedFileHEvent : HEvent
		 {
			  internal File File;

			  internal UnmappedFileHEvent( File file )
			  {
					this.File = file;
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					Print( @out, File );
			  }
		 }

		 public class EvictionRunHEvent : IntervalHEvent, EvictionRunEvent
		 {
			  internal int PagesToEvict;

			  internal EvictionRunHEvent( LinearHistoryTracer tracer, int pagesToEvict ) : base( tracer )
			  {
					this.PagesToEvict = pagesToEvict;
			  }

			  public override EvictionEvent BeginEviction()
			  {
					return Tracer.add( new EvictionHEvent( Tracer ) );
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( ", pagesToEvict:" );
					@out.print( PagesToEvict );
			  }
		 }

		 public class FlushHEvent : IntervalHEvent, FlushEvent
		 {
			  internal long FilePageId;
			  internal long CachePageId;
			  internal int PageCount;
			  internal File File;
			  internal int BytesWritten;
			  internal IOException Exception;

			  internal FlushHEvent( LinearHistoryTracer tracer, long filePageId, long cachePageId, PageSwapper swapper ) : base( tracer )
			  {
					this.FilePageId = filePageId;
					this.CachePageId = cachePageId;
					this.PageCount = 1;
					this.File = swapper.File();
			  }

			  public override void AddBytesWritten( long bytes )
			  {
					BytesWritten += ( int )bytes;
			  }

			  public override void Done()
			  {
					Close();
			  }

			  public override void Done( IOException exception )
			  {
					this.Exception = exception;
					Done();
			  }

			  public override void AddPagesFlushed( int pageCount )
			  {
					this.PageCount = pageCount;
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( ", filePageId:" );
					@out.print( FilePageId );
					@out.print( ", cachePageId:" );
					@out.print( CachePageId );
					@out.print( ", pageCount:" );
					@out.print( PageCount );
					Print( @out, File );
					@out.print( ", bytesWritten:" );
					@out.print( BytesWritten );
					Print( @out, Exception, exceptionLinePrefix );
			  }
		 }

		 public class MajorFlushHEvent : IntervalHEvent, MajorFlushEvent, FlushEventOpportunity
		 {
			  internal File File;

			  internal MajorFlushHEvent( LinearHistoryTracer tracer, File file ) : base( tracer )
			  {
					this.File = file;
			  }

			  public override FlushEventOpportunity FlushEventOpportunity()
			  {
					return this;
			  }

			  public override FlushEvent BeginFlush( long filePageId, long cachePageId, PageSwapper swapper )
			  {
					return Tracer.add( new FlushHEvent( Tracer, filePageId, cachePageId, swapper ) );
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					Print( @out, File );
			  }
		 }

		 public class PinHEvent : IntervalHEvent, PinEvent
		 {
			  internal bool ExclusiveLock;
			  internal long FilePageId;
			  internal File File;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long CachePageIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool HitConflict;

			  internal PinHEvent( LinearHistoryTracer tracer, bool exclusiveLock, long filePageId, PageSwapper swapper ) : base( tracer )
			  {
					this.ExclusiveLock = exclusiveLock;
					this.FilePageId = filePageId;
					this.HitConflict = true;
					this.File = swapper.File();
			  }

			  public virtual long CachePageId
			  {
				  set
				  {
						this.CachePageIdConflict = value;
				  }
			  }

			  public override PageFaultEvent BeginPageFault()
			  {
					HitConflict = false;
					return Tracer.add( new PageFaultHEvent( Tracer ) );
			  }

			  public override void Hit()
			  {
			  }

			  public override void Done()
			  {
					Close();
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( ", filePageId:" );
					@out.print( FilePageId );
					@out.print( ", cachePageId:" );
					@out.print( CachePageIdConflict );
					@out.print( ", hit:" );
					@out.print( HitConflict );
					Print( @out, File );
					@out.append( ", exclusiveLock:" );
					@out.print( ExclusiveLock );
			  }
		 }

		 public class PageFaultHEvent : IntervalHEvent, PageFaultEvent
		 {
			  internal int BytesRead;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long CachePageIdConflict;
			  internal bool PageEvictedByFaulter;
			  internal Exception Exception;

			  internal PageFaultHEvent( LinearHistoryTracer linearHistoryTracer ) : base( linearHistoryTracer )
			  {
			  }

			  public override void AddBytesRead( long bytes )
			  {
					BytesRead += ( int )bytes;
			  }

			  public virtual long CachePageId
			  {
				  set
				  {
						this.CachePageIdConflict = value;
				  }
			  }

			  public override void Done()
			  {
					Close();
			  }

			  public override void Done( Exception throwable )
			  {
					this.Exception = throwable;
					Done();
			  }

			  public override EvictionEvent BeginEviction()
			  {
					PageEvictedByFaulter = true;
					return Tracer.add( new EvictionHEvent( Tracer ) );
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( ", cachePageId:" );
					@out.print( CachePageIdConflict );
					@out.print( ", bytesRead:" );
					@out.print( BytesRead );
					@out.print( ", pageEvictedByFaulter:" );
					@out.print( PageEvictedByFaulter );
					Print( @out, Exception, exceptionLinePrefix );
			  }
		 }

		 public class EvictionHEvent : IntervalHEvent, EvictionEvent, FlushEventOpportunity
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FilePageIdConflict;
			  internal File File;
			  internal IOException Exception;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long CachePageIdConflict;

			  internal EvictionHEvent( LinearHistoryTracer linearHistoryTracer ) : base( linearHistoryTracer )
			  {
			  }

			  public virtual long FilePageId
			  {
				  set
				  {
						this.FilePageIdConflict = value;
				  }
			  }

			  public virtual PageSwapper Swapper
			  {
				  set
				  {
						File = value == null ? null : value.File();
				  }
			  }

			  public override FlushEventOpportunity FlushEventOpportunity()
			  {
					return this;
			  }

			  public override void ThrewException( IOException exception )
			  {
					this.Exception = exception;
			  }

			  public virtual long CachePageId
			  {
				  set
				  {
						this.CachePageIdConflict = value;
				  }
			  }

			  public override FlushEvent BeginFlush( long filePageId, long cachePageId, PageSwapper swapper )
			  {
					return Tracer.add( new FlushHEvent( Tracer, filePageId, cachePageId, swapper ) );
			  }

			  internal override void PrintBody( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( ", filePageId:" );
					@out.print( FilePageIdConflict );
					@out.print( ", cachePageId:" );
					@out.print( CachePageIdConflict );
					Print( @out, File );
					Print( @out, Exception, exceptionLinePrefix );
			  }
		 }

		 public abstract class HEvent
		 {
			  internal static readonly HEvent end = new HEventAnonymousInnerClass();

			  private class HEventAnonymousInnerClass : HEvent
			  {
				  internal override void printBody( PrintStream @out, string exceptionLinePrefix )
				  {
						@out.print( " EOF " );
				  }
			  }

			  internal readonly long Time;
			  internal readonly long ThreadId;
			  internal readonly string ThreadName;
			  internal volatile HEvent Prev;

			  internal HEvent()
			  {
					Time = System.nanoTime();
					Thread thread = Thread.CurrentThread;
					ThreadId = thread.Id;
					ThreadName = thread.Name;
					System.identityHashCode( this );
			  }

			  public static HEvent Reverse( HEvent events )
			  {
					HEvent current = end;
					while ( events != end )
					{
						 HEvent prev;
						 do
						 {
							  prev = events.Prev;
						 } while ( prev == null );
						 events.Prev = current;
						 current = events;
						 events = prev;
					}
					return current;
			  }

			  public virtual void Print( PrintStream @out, string exceptionLinePrefix )
			  {
					@out.print( this.GetType().Name );
					@out.print( '#' );
					@out.print( System.identityHashCode( this ) );
					@out.print( '[' );
					@out.print( "time:" );
					@out.print( ( Time - end.time ) / 1000 );
					@out.print( ", threadId:" );
					@out.print( ThreadId );
					PrintBody( @out, exceptionLinePrefix );
					@out.print( ']' );
			  }

			  internal abstract void PrintBody( PrintStream @out, string exceptionLinePrefix );

			  protected internal void Print( PrintStream @out, File file )
			  {
					@out.print( ", file:" );
					@out.print( file == null ? "<null>" : file.Path );
			  }

			  protected internal void Print( PrintStream @out, Exception exception, string linePrefix )
			  {
					if ( exception != null )
					{
						 @out.println( ", exception:" );
						 MemoryStream buf = new MemoryStream();
						 PrintStream sbuf = new PrintStream( buf );
						 exception.printStackTrace( sbuf );
						 sbuf.flush();
						 StreamReader reader = new StreamReader( new StringReader( buf.ToString() ) );
						 try
						 {
							  string line = reader.ReadLine();
							  while ( !string.ReferenceEquals( line, null ) )
							  {
									@out.print( linePrefix );
									@out.print( '\t' );
									@out.println( line );
									line = reader.ReadLine();
							  }
							  @out.print( linePrefix );
						 }
						 catch ( IOException e )
						 {
							  throw new Exception( e );
						 }
					}
			  }
		 }

		 public abstract class IntervalHEvent : HEvent
		 {
			  protected internal LinearHistoryTracer Tracer;

			  internal IntervalHEvent( LinearHistoryTracer tracer )
			  {
					this.Tracer = tracer;
			  }

			  public virtual void Close()
			  {
					Tracer.add( new EndHEvent( this ) );
			  }
		 }
	}

}