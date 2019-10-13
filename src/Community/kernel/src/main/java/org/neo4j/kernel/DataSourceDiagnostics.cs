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
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using Logger = Neo4Net.Logging.Logger;

	internal abstract class DataSourceDiagnostics : DiagnosticsExtractor<NeoStoreDataSource>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TRANSACTION_RANGE("Transaction log:") { void dump(NeoStoreDataSource source, org.neo4j.logging.Logger log) { org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles = source.getDependencyResolver().resolveDependency(org.neo4j.kernel.impl.transaction.log.files.LogFiles.class); try { for(long logVersion = logFiles.getLowestLogVersion(); logFiles.versionExists(logVersion); logVersion++) { if(logFiles.hasAnyEntries(logVersion)) { org.neo4j.kernel.impl.transaction.log.entry.LogHeader header = logFiles.extractHeader(logVersion); long firstTransactionIdInThisLog = header.lastCommittedTxId + 1; log.log("Oldest transaction " + firstTransactionIdInThisLog + " found in log with version " + logVersion); return; } } log.log("No transactions found in any log"); } catch(java.io.IOException e) { log.log("Error trying to figure out oldest transaction in log"); } } };

		 private static readonly IList<DataSourceDiagnostics> valueList = new List<DataSourceDiagnostics>();

		 static DataSourceDiagnostics()
		 {
			 valueList.Add( TRANSACTION_RANGE );
		 }

		 public enum InnerEnum
		 {
			 TRANSACTION_RANGE
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private DataSourceDiagnostics( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private readonly string message;

		 internal DataSourceDiagnostics( string name, InnerEnum innerEnum, string message )
		 {
			  this._message = message;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void dumpDiagnostics(final NeoStoreDataSource source, org.neo4j.internal.diagnostics.DiagnosticsPhase phase, org.neo4j.logging.Logger logger)
		 public void DumpDiagnostics( NeoStoreDataSource source, Neo4Net.@internal.Diagnostics.DiagnosticsPhase phase, Neo4Net.Logging.Logger logger )
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

		 internal abstract void dump( NeoStoreDataSource source, Neo4Net.Logging.Logger logger );

		public static IList<DataSourceDiagnostics> values()
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

		public static DataSourceDiagnostics valueOf( string name )
		{
			foreach ( DataSourceDiagnostics enumInstance in DataSourceDiagnostics.valueList )
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