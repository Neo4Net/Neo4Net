using System.Collections.Generic;

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
namespace Neo4Net.Cypher.@internal.tracing
{

	using CompilationPhaseTracer = Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer;

	public class TimingCompilationTracer : CompilationTracer
	{
		 public interface EventListener
		 {
			  void StartQueryCompilation( string query );
			  void QueryCompiled( QueryEvent @event );
		 }

		 public interface QueryEvent
		 {
			  string Query();

			  long NanoTime();

			  IList<PhaseEvent> Phases();
		 }

		 public interface PhaseEvent
		 {
			  Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer_CompilationPhase Phase();

			  long NanoTime();
		 }

		 internal interface Clock
		 {
			  long NanoTime();
		 }

		 public static class Clock_Fields
		 {
			 private readonly TimingCompilationTracer _outerInstance;

			 public Clock_Fields( TimingCompilationTracer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly Clock System = System.nanoTime;
		 }

		 private readonly Clock _clock;
		 private readonly EventListener _listener;

		 public TimingCompilationTracer( EventListener listener ) : this( Clock_Fields.System, listener )
		 {
		 }

		 internal TimingCompilationTracer( Clock clock, EventListener listener )
		 {
			  this._clock = clock;
			  this._listener = listener;
		 }

		 public override CompilationTracer_QueryCompilationEvent CompileQuery( string query )
		 {
			  _listener.startQueryCompilation( query );
			  return new Query( _clock, query, _listener );
		 }

		 private abstract class Event : AutoCloseable
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Clock ClockConflict;
			  internal long Time;

			  internal Event( Clock clock )
			  {
					this.ClockConflict = clock;
					this.Time = clock.NanoTime();
			  }

			  public override void Close()
			  {
					if ( ClockConflict != null )
					{
						 Time = ClockConflict.nanoTime() - Time;
						 ClockConflict = null;
						 Done();
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("UnusedDeclaration") public final long nanoTime()
			  public long NanoTime()
			  {
					return Time;
			  }

			  internal abstract void Done();

			  internal Clock Clock()
			  {
					if ( ClockConflict == null )
					{
						 throw new System.InvalidOperationException( this + " has been closed" );
					}
					return ClockConflict;
			  }
		 }

		 private class Query : Event, QueryEvent, CompilationTracer_QueryCompilationEvent
		 {
			  internal readonly string QueryString;
			  internal readonly EventListener Listener;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<Phase> PhasesConflict = new List<Phase>();

			  internal Query( Clock clock, string query, EventListener listener ) : base( clock )
			  {
					this.QueryString = query;
					this.Listener = listener;
			  }

			  public override string ToString()
			  {
					return this.GetType().Name + "[" + QueryString + "]";
			  }

			  public override Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer_CompilationPhaseEvent BeginPhase( Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer_CompilationPhase phase )
			  {
					Phase @event = new Phase( base.Clock(), phase );
					PhasesConflict.Add( @event );
					return @event;
			  }

			  internal override void Done()
			  {
					Listener.queryCompiled( this );
			  }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override string QueryConflict()
			  {
					return QueryString;
			  }

			  public override IList<PhaseEvent> Phases()
			  {
					return Collections.unmodifiableList( PhasesConflict );
			  }
		 }

		 private class Phase : Event, PhaseEvent, Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer_CompilationPhaseEvent
		 {
			  internal readonly Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer_CompilationPhase CompilationPhase;

			  internal Phase( Clock clock, Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer_CompilationPhase phase ) : base( clock )
			  {
					this.CompilationPhase = phase;
			  }

			  public override string ToString()
			  {
					return this.GetType().Name + "[" + CompilationPhase + "]";
			  }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override Neo4Net.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer_CompilationPhase PhaseConflict()
			  {
					return CompilationPhase;
			  }

			  internal override void Done()
			  {
			  }
		 }
	}

}