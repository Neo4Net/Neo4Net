using System;
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
namespace Neo4Net.Internal.Diagnostics
{

	using Neo4Net.Internal.Diagnostics;
	using Neo4Net.Helpers.Collections;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using Logger = Neo4Net.Logging.Logger;
	using NullLog = Neo4Net.Logging.NullLog;

	/// <summary>
	/// Collects and manages all <seealso cref="DiagnosticsProvider"/>.
	/// </summary>
	public class DiagnosticsManager : IEnumerable<DiagnosticsProvider>, Lifecycle
	{
		 private readonly IList<DiagnosticsProvider> _providers = new CopyOnWriteArrayList<DiagnosticsProvider>();
		 private readonly Log _targetLog;
		 private volatile State _state = State.Initial;

		 public DiagnosticsManager( Log targetLog )
		 {
			  this._targetLog = targetLog;

			  _providers.Add( new DiagnosticsProviderAnonymousInnerClass( this ) );
		 }

		 private class DiagnosticsProviderAnonymousInnerClass : DiagnosticsProvider
		 {
			 private readonly DiagnosticsManager _outerInstance;

			 public DiagnosticsProviderAnonymousInnerClass( DiagnosticsManager outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public string DiagnosticsIdentifier
			 {
				 get
				 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					  return _outerInstance.GetType().FullName;
				 }
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void dump(DiagnosticsPhase phase, final org.neo4j.logging.Logger logger)
			 public void dump( DiagnosticsPhase phase, Logger logger )
			 {
				  if ( phase.Initialization || phase.ExplicitlyRequested )
				  {
						logger.Log( "Diagnostics providers:" );
						foreach ( DiagnosticsProvider provider in _outerInstance.providers )
						{
							 logger.Log( provider.DiagnosticsIdentifier );
						}
				  }
			 }

			 public void acceptDiagnosticsVisitor( object visitor )
			 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: org.neo4j.helpers.collection.Visitor<? super DiagnosticsProvider, ? extends RuntimeException> target = org.neo4j.helpers.collection.Visitor_SafeGenerics.castOrNull(DiagnosticsProvider.class, RuntimeException.class, visitor);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
				  Visitor<object, ? extends Exception> target = Neo4Net.Helpers.Collections.Visitor_SafeGenerics.CastOrNull( typeof( DiagnosticsProvider ), typeof( Exception ), visitor );
				  if ( target != null )
				  {
						foreach ( DiagnosticsProvider provider in _outerInstance.providers )
						{
							 target.Visit( provider );
						}
				  }
			 }
		 }

		 public override void Init()
		 {
			  lock ( _providers )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("hiding") State state = this.state;
					State state = this._state;
					if ( !state.startup( this ) )
					{
						 return;
					}
			  }
			  DumpAll( DiagnosticsPhase.Initialized, TargetLog );
		 }

		 public override void Start()
		 {
			  lock ( _providers )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("hiding") State state = this.state;
					State state = this._state;
					if ( !state.startup( this ) )
					{
						 return;
					}
			  }
			  DumpAll( DiagnosticsPhase.Started, TargetLog );
		 }

		 public override void Stop()
		 {
			  lock ( _providers )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("hiding") State state = this.state;
					State state = this._state;
					if ( !state.shutdown( this ) )
					{
						 return;
					}
			  }
			  DumpAll( DiagnosticsPhase.Stopping, TargetLog );
			  _providers.Clear();
		 }

		 public override void Shutdown()
		 {
			  lock ( _providers )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("hiding") State state = this.state;
					State state = this._state;
					if ( !state.shutdown( this ) )
					{
						 return;
					}
			  }
			  DumpAll( DiagnosticsPhase.Shutdown, TargetLog );
			  _providers.Clear();
		 }

		 private sealed class State
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INITIAL { boolean startup(DiagnosticsManager manager) { manager.state = STARTED; return true; } },
			  public static readonly State Started = new State( "Started", InnerEnum.Started );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           STOPPED { boolean shutdown(DiagnosticsManager manager) { return false; } };

			  private static readonly IList<State> valueList = new List<State>();

			  static State()
			  {
				  valueList.Add( INITIAL );
				  valueList.Add( Started );
				  valueList.Add( STOPPED );
			  }

			  public enum InnerEnum
			  {
				  INITIAL,
				  Started,
				  STOPPED
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private State( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal bool Startup( DiagnosticsManager manager )
			  {
					return false;
			  }

			  internal bool Shutdown( DiagnosticsManager manager )
			  {
					manager._state = STOPPED;
					return true;
			  }

			 public static IList<State> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static State valueOf( string name )
			 {
				 foreach ( State enumInstance in State.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 public virtual Log TargetLog
		 {
			 get
			 {
				  return _targetLog;
			 }
		 }

		 public virtual void DumpAll()
		 {
			  DumpAll( DiagnosticsPhase.Requested, TargetLog );
		 }

		 public virtual void Dump( string identifier )
		 {
			  Extract( identifier, TargetLog );
		 }

		 public virtual void DumpAll( Log log )
		 {
			  log.Bulk(bulkLog =>
			  {
				foreach ( DiagnosticsProvider provider in _providers )
				{
					 Dump( provider, DiagnosticsPhase.Explicit, bulkLog );
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void extract(final String identifier, org.neo4j.logging.Log log)
		 public virtual void Extract( string identifier, Log log )
		 {
			  log.Bulk(bulkLog =>
			  {
				foreach ( DiagnosticsProvider provider in _providers )
				{
					 if ( identifier.Equals( provider.DiagnosticsIdentifier ) )
					 {
						  Dump( provider, DiagnosticsPhase.Explicit, bulkLog );
						  return;
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void dumpAll(final DiagnosticsPhase phase, org.neo4j.logging.Log log)
		 private void DumpAll( DiagnosticsPhase phase, Log log )
		 {
			  log.Bulk(bulkLog =>
			  {
				phase.emitStart( bulkLog );
				foreach ( DiagnosticsProvider provider in _providers )
				{
					 Dump( provider, phase, bulkLog );
				}
				phase.emitDone( bulkLog );
			  });
		 }

		 public virtual void Register<T>( DiagnosticsExtractor<T> extractor, T source )
		 {
			  AppendProvider( ExtractedProvider( extractor, source ) );
		 }

		 public virtual void RegisterAll<T, E>( Type extractorEnum, T source ) where E : Enum<E>, DiagnosticsExtractor<T>
		 {
				 extractorEnum = typeof( E );
			  foreach ( DiagnosticsExtractor<T> extractor in extractorEnum.EnumConstants )
			  {
					Register( extractor, source );
			  }
		 }

		 public virtual void PrependProvider( DiagnosticsProvider provider )
		 {
			  State state = this._state;
			  if ( state == State.Stopped )
			  {
					return;
			  }
			  _providers.Insert( 0, provider );
			  if ( state == State.Started )
			  {
					Dump( DiagnosticsPhase.Started, provider, TargetLog );
			  }
		 }

		 public virtual void AppendProvider( DiagnosticsProvider provider )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("hiding") State state = this.state;
			  State state = this._state;
			  if ( state == State.Stopped )
			  {
					return;
			  }
			  _providers.Add( provider );
			  if ( state == State.Started )
			  {
					Dump( DiagnosticsPhase.Started, provider, TargetLog );
			  }
		 }

		 private void Dump( DiagnosticsPhase phase, DiagnosticsProvider provider, Log log )
		 {
			  phase.emitStart( log, provider );
			  Dump( provider, phase, log );
			  phase.emitDone( log, provider );
		 }

		 private static void Dump( DiagnosticsProvider provider, DiagnosticsPhase phase, Log log )
		 {
			  // Optimization to skip diagnostics dumping (which is time consuming) if there's no log anyway.
			  // This is first and foremost useful for speeding up testing.
			  if ( log == NullLog.Instance )
			  {
					return;
			  }

			  try
			  {
					provider.Dump( phase, log.InfoLogger() );
			  }
			  catch ( Exception cause )
			  {
					log.Error( "Failure while logging diagnostics for " + provider, cause );
			  }
		 }

		 public override IEnumerator<DiagnosticsProvider> Iterator()
		 {
			  return _providers.GetEnumerator();
		 }

		 internal static DiagnosticsProvider ExtractedProvider<T>( DiagnosticsExtractor<T> extractor, T source )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (extractor instanceof DiagnosticsExtractor_VisitableDiagnostics<?>)
			  if ( extractor is DiagnosticsExtractor_VisitableDiagnostics<object> )
			  {
					return new ExtractedVisitableDiagnosticsProvider<>( ( DiagnosticsExtractor_VisitableDiagnostics<T> ) extractor, source );
			  }
			  else
			  {
					return new ExtractedDiagnosticsProvider<>( extractor, source );
			  }
		 }

		 private class ExtractedDiagnosticsProvider<T> : DiagnosticsProvider
		 {
			  internal readonly DiagnosticsExtractor<T> Extractor;
			  internal readonly T Source;

			  internal ExtractedDiagnosticsProvider( DiagnosticsExtractor<T> extractor, T source )
			  {
					this.Extractor = extractor;
					this.Source = source;
			  }

			  public virtual string DiagnosticsIdentifier
			  {
				  get
				  {
						return Extractor.ToString();
				  }
			  }

			  public override void AcceptDiagnosticsVisitor( object visitor )
			  {
					// nobody visits the source of this
			  }

			  public override void Dump( DiagnosticsPhase phase, Logger logger )
			  {
					Extractor.dumpDiagnostics( Source, phase, logger );
			  }
		 }

		 private class ExtractedVisitableDiagnosticsProvider<T> : ExtractedDiagnosticsProvider<T>
		 {
			  internal ExtractedVisitableDiagnosticsProvider( DiagnosticsExtractor_VisitableDiagnostics<T> extractor, T source ) : base( extractor, source )
			  {
			  }

			  public override void AcceptDiagnosticsVisitor( object visitor )
			  {
					( ( DiagnosticsExtractor_VisitableDiagnostics<T> ) Extractor ).dispatchDiagnosticsVisitor( Source, visitor );
			  }
		 }
	}

}