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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using DuplicateInputIdException = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.DuplicateInputIdException;
	using AsyncEvent = Neo4Net.Utils.Concurrent.AsyncEvent;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.withMessage;

	public class BadCollector : Collector
	{
		 /// <summary>
		 /// Introduced to avoid creating an exception for every reported bad thing, since it can be
		 /// quite the performance hogger for scenarios where there are many many bad things to collect.
		 /// </summary>
		 internal abstract class ProblemReporter : AsyncEvent
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int TypeConflict;

			  internal ProblemReporter( int type )
			  {
					this.TypeConflict = type;
			  }

			  internal virtual int Type()
			  {
					return TypeConflict;
			  }

			  internal abstract string Message();

			  internal abstract InputException Exception();
		 }

		 internal interface Monitor
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void beforeProcessEvent()
	//		  {
	//		  }
		 }

		 internal static readonly Monitor NO_MONITOR = new MonitorAnonymousInnerClass();

		 private class MonitorAnonymousInnerClass : Monitor
		 {
		 }

		 internal const int BAD_RELATIONSHIPS = 0x1;
		 internal const int DUPLICATE_NODES = 0x2;
		 internal const int EXTRA_COLUMNS = 0x4;

		 internal const int COLLECT_ALL = BAD_RELATIONSHIPS | DUPLICATE_NODES | EXTRA_COLUMNS;
		 public const long UNLIMITED_TOLERANCE = -1;
		 internal const int DEFAULT_BACK_PRESSURE_THRESHOLD = 10_000;

		 private readonly PrintStream @out;
		 private readonly long _tolerance;
		 private readonly int _collect;
		 private readonly int _backPressureThreshold;
		 private readonly bool _logBadEntries;
		 private readonly Monitor _monitor;

		 // volatile since one importer thread calls collect(), where this value is incremented and later the "main"
		 // thread calls badEntries() to get a count.
		 private readonly AtomicLong _badEntries = new AtomicLong();
		 private readonly AsyncEvents<ProblemReporter> _logger;
		 private readonly Thread _eventProcessor;
		 private readonly LongAdder _queueSize = new LongAdder();

		 public BadCollector( Stream @out, long tolerance, int collect ) : this( @out, tolerance, collect, DEFAULT_BACK_PRESSURE_THRESHOLD, false, NO_MONITOR )
		 {
		 }

		 internal BadCollector( Stream @out, long tolerance, int collect, int backPressureThreshold, bool skipBadEntriesLogging, Monitor monitor )
		 {
			  this.@out = new PrintStream( @out );
			  this._tolerance = tolerance;
			  this._collect = collect;
			  this._backPressureThreshold = backPressureThreshold;
			  this._logBadEntries = !skipBadEntriesLogging;
			  this._monitor = monitor;
			  this._logger = new AsyncEvents<ProblemReporter>( this.processEvent, AsyncEvents.Monitor_Fields.NONE );
			  this._eventProcessor = new Thread( _logger );
			  this._eventProcessor.Start();
		 }

		 private void ProcessEvent( ProblemReporter report )
		 {
			  _monitor.beforeProcessEvent();
			  @out.println( report.Message() );
			  _queueSize.add( -1 );
		 }

		 public override void CollectBadRelationship( object startId, string startIdGroup, string type, object endId, string endIdGroup, object specificValue )
		 {
			  Collect( new RelationshipsProblemReporter( startId, startIdGroup, type, endId, endIdGroup, specificValue ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void collectExtraColumns(final String source, final long row, final String value)
		 public override void CollectExtraColumns( string source, long row, string value )
		 {
			  Collect( new ExtraColumnsProblemReporter( row, source, value ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void collectDuplicateNode(final Object id, long actualId, final String group)
		 public override void CollectDuplicateNode( object id, long actualId, string group )
		 {
			  Collect( new NodesProblemReporter( id, group ) );
		 }

		 public virtual bool CollectingBadRelationships
		 {
			 get
			 {
				  return Collects( BAD_RELATIONSHIPS );
			 }
		 }

		 private void Collect( ProblemReporter report )
		 {
			  bool collect = Collects( report.Type() );
			  if ( collect )
			  {
					// This type of problem is collected and we're within the max threshold, so it's OK
					long count = _badEntries.incrementAndGet();
					if ( _tolerance == UNLIMITED_TOLERANCE || count <= _tolerance )
					{
						 // We're within the threshold
						 if ( _logBadEntries )
						 {
							  // Send this to the logger... but first apply some back pressure if queue is growing big
							  while ( _queueSize.sum() >= _backPressureThreshold )
							  {
									LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 ) );
							  }
							  _logger.send( report );
							  _queueSize.add( 1 );
						 }
						 return; // i.e. don't treat this as an exception
					}
			  }

			  InputException exception = report.Exception();
			  throw collect ? withMessage( exception, format( "Too many bad entries %d, where last one was: %s", _badEntries.longValue(), exception.Message ) ) : exception;
		 }

		 public override void Close()
		 {
			  _logger.shutdown();
			  try
			  {
					_logger.awaitTermination();
					_eventProcessor.Join();
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
			  }
			  finally
			  {
					@out.flush();
					@out.close();
			  }
		 }

		 public override long BadEntries()
		 {
			  return _badEntries.get();
		 }

		 private bool Collects( int bit )
		 {
			  return ( _collect & bit ) != 0;
		 }

		 private class RelationshipsProblemReporter : ProblemReporter
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string MessageConflict;
			  internal readonly object SpecificValue;
			  internal readonly object StartId;
			  internal readonly string StartIdGroup;
			  internal readonly string Type;
			  internal readonly object EndId;
			  internal readonly string EndIdGroup;

			  internal RelationshipsProblemReporter( object startId, string startIdGroup, string type, object endId, string endIdGroup, object specificValue ) : base( BAD_RELATIONSHIPS )
			  {
					this.StartId = startId;
					this.StartIdGroup = startIdGroup;
					this.Type = type;
					this.EndId = endId;
					this.EndIdGroup = endIdGroup;
					this.SpecificValue = specificValue;
			  }

			  public override string Message()
			  {
					return ReportMessage;
			  }

			  public override InputException Exception()
			  {
					return new InputException( ReportMessage );
			  }

			  internal virtual string ReportMessage
			  {
				  get
				  {
						if ( string.ReferenceEquals( MessageConflict, null ) )
						{
							 MessageConflict = !MissingData ? format( "%s (%s)-[%s]->%s (%s) referring to missing node %s", StartId, StartIdGroup, Type, EndId, EndIdGroup, SpecificValue ) : format( "%s (%s)-[%s]->%s (%s) is missing data", StartId, StartIdGroup, Type, EndId, EndIdGroup );
						}
						return MessageConflict;
				  }
			  }

			  internal virtual bool MissingData
			  {
				  get
				  {
						return StartId == null || EndId == null || string.ReferenceEquals( Type, null );
				  }
			  }
		 }

		 private class NodesProblemReporter : ProblemReporter
		 {
			  internal readonly object Id;
			  internal readonly string Group;

			  internal NodesProblemReporter( object id, string group ) : base( DUPLICATE_NODES )
			  {
					this.Id = id;
					this.Group = group;
			  }

			  public override string Message()
			  {
					return DuplicateInputIdException.message( Id, Group );
			  }

			  public override InputException Exception()
			  {
					return new DuplicateInputIdException( Id, Group );
			  }
		 }

		 private class ExtraColumnsProblemReporter : ProblemReporter
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string MessageConflict;
			  internal readonly long Row;
			  internal readonly string Source;
			  internal readonly string Value;

			  internal ExtraColumnsProblemReporter( long row, string source, string value ) : base( EXTRA_COLUMNS )
			  {
					this.Row = row;
					this.Source = source;
					this.Value = value;
			  }

			  public override string Message()
			  {
					return ReportMessage;
			  }

			  public override InputException Exception()
			  {
					return new InputException( ReportMessage );
			  }

			  internal virtual string ReportMessage
			  {
				  get
				  {
						if ( string.ReferenceEquals( MessageConflict, null ) )
						{
							 MessageConflict = format( "Extra column not present in header on line %d in %s with value %s", Row, Source, Value );
						}
						return MessageConflict;
				  }
			  }
		 }
	}

}