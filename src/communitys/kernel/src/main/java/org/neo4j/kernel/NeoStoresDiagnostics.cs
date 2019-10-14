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
namespace Neo4Net.Kernel
{
	using Neo4Net.@internal.Diagnostics;
	using DiagnosticsPhase = Neo4Net.@internal.Diagnostics.DiagnosticsPhase;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Logger = Neo4Net.Logging.Logger;

	public abstract class NeoStoresDiagnostics : DiagnosticsExtractor<NeoStores>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NEO_STORE_VERSIONS("Store versions:") { void dump(org.neo4j.kernel.impl.store.NeoStores source, org.neo4j.logging.Logger logger) { source.logVersions(logger); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NEO_STORE_ID_USAGE("Id usage:") { void dump(org.neo4j.kernel.impl.store.NeoStores source, org.neo4j.logging.Logger logger) { source.logIdUsage(logger); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NEO_STORE_RECORDS("Neostore records:") { void dump(org.neo4j.kernel.impl.store.NeoStores source, org.neo4j.logging.Logger logger) { source.getMetaDataStore().logRecords(logger); } };

		 private static readonly IList<NeoStoresDiagnostics> valueList = new List<NeoStoresDiagnostics>();

		 static NeoStoresDiagnostics()
		 {
			 valueList.Add( NEO_STORE_VERSIONS );
			 valueList.Add( NEO_STORE_ID_USAGE );
			 valueList.Add( NEO_STORE_RECORDS );
		 }

		 public enum InnerEnum
		 {
			 NEO_STORE_VERSIONS,
			 NEO_STORE_ID_USAGE,
			 NEO_STORE_RECORDS
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private NeoStoresDiagnostics( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private readonly string message;

		 internal NeoStoresDiagnostics( string name, InnerEnum innerEnum, string message )
		 {
			  this._message = message;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void dumpDiagnostics(final org.neo4j.kernel.impl.store.NeoStores source, org.neo4j.internal.diagnostics.DiagnosticsPhase phase, org.neo4j.logging.Logger logger)
		 public void DumpDiagnostics( Neo4Net.Kernel.impl.store.NeoStores source, Neo4Net.@internal.Diagnostics.DiagnosticsPhase phase, Neo4Net.Logging.Logger logger )
		 {
			  if ( Applicable( phase ) )
			  {
					logger.Log( _message );
					Dump( source, logger );
			  }
		 }

		 internal bool Applicable( Neo4Net.@internal.Diagnostics.DiagnosticsPhase phase )
		 {
			  return phase.Initialization || phase.ExplicitlyRequested;
		 }

		 internal abstract void dump( Neo4Net.Kernel.impl.store.NeoStores source, Neo4Net.Logging.Logger logger );

		public static IList<NeoStoresDiagnostics> values()
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

		public static NeoStoresDiagnostics valueOf( string name )
		{
			foreach ( NeoStoresDiagnostics enumInstance in NeoStoresDiagnostics.valueList )
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