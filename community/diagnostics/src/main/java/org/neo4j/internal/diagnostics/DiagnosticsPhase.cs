using System.Collections.Generic;

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
namespace Org.Neo4j.@internal.Diagnostics
{
	using Log = Org.Neo4j.Logging.Log;

	public sealed class DiagnosticsPhase
	{
		 public static readonly DiagnosticsPhase Requested = new DiagnosticsPhase( "Requested", InnerEnum.Requested, true, false );
		 public static readonly DiagnosticsPhase Explicit = new DiagnosticsPhase( "Explicit", InnerEnum.Explicit, true, false );
		 public static readonly DiagnosticsPhase Initialized = new DiagnosticsPhase( "Initialized", InnerEnum.Initialized, false, true );
		 public static readonly DiagnosticsPhase Started = new DiagnosticsPhase( "Started", InnerEnum.Started, false, true );
		 public static readonly DiagnosticsPhase Stopping = new DiagnosticsPhase( "Stopping", InnerEnum.Stopping, false, false );
		 public static readonly DiagnosticsPhase Shutdown = new DiagnosticsPhase( "Shutdown", InnerEnum.Shutdown, false, false );
		 public static readonly DiagnosticsPhase  = new DiagnosticsPhase( "", InnerEnum. );

		 private static readonly IList<DiagnosticsPhase> valueList = new List<DiagnosticsPhase>();

		 static DiagnosticsPhase()
		 {
			 valueList.Add( Requested );
			 valueList.Add( Explicit );
			 valueList.Add( Initialized );
			 valueList.Add( Started );
			 valueList.Add( Stopping );
			 valueList.Add( Shutdown );
			 valueList.Add();
		 }

		 public enum InnerEnum
		 {
			 Requested,
			 Explicit,
			 Initialized,
			 Started,
			 Stopping,
			 Shutdown,
          
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly bool requested;
		 private readonly bool initial;

		 internal DiagnosticsPhase( string name, InnerEnum innerEnum, bool requested, bool initial )
		 {
			  this._requested = requested;
			  this._initial = initial;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal void EmitStart( Org.Neo4j.Logging.Log log )
		 {
			  log.Info( "--- " + this + " START ---" );
		 }

		 internal void EmitDone( Org.Neo4j.Logging.Log log )
		 {
			  log.Info( "--- " + this + " END ---" );
		 }

		 internal void EmitStart( Org.Neo4j.Logging.Log log, DiagnosticsProvider provider )
		 {
			  log.Info( "--- " + this + " for " + provider.DiagnosticsIdentifier + " START ---" );
		 }

		 internal void EmitDone( Org.Neo4j.Logging.Log log, DiagnosticsProvider provider )
		 {
			  log.Info( "--- " + this + " for " + provider.DiagnosticsIdentifier + " END ---" );
		 }

		 public bool Initialization
		 {
			 get
			 {
				  return _initial;
			 }
		 }

		 public bool ExplicitlyRequested
		 {
			 get
			 {
				  return _requested;
			 }
		 }

		 public override string ToString()
		 {
			  return name() + " diagnostics";
		 }

		public static IList<DiagnosticsPhase> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static DiagnosticsPhase valueOf( string name )
		{
			foreach ( DiagnosticsPhase enumInstance in DiagnosticsPhase.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}