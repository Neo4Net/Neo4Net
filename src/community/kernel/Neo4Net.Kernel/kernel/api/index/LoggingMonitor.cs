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
namespace Neo4Net.Kernel.Api.Index
{
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;


	using Log = Neo4Net.Logging.Log;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.duration;

	public class LoggingMonitor : IndexProvider.Monitor
	{
		 private readonly Log _log;

		 public LoggingMonitor( Log log )
		 {
			  this._log = log;
		 }

		 public override void FailedToOpenIndex( StoreIndexDescriptor descriptor, string action, Exception cause )
		 {
			  _log.error( "Failed to open index:" + descriptor.Id + ". " + action, cause );
		 }

		 public override void RecoveryCleanupRegistered( File indexFile, IndexDescriptor indexDescriptor )
		 {
			  _log.info( "Schema index cleanup job registered: " + IndexDescription( indexFile, indexDescriptor ) );
		 }

		 public override void RecoveryCleanupStarted( File indexFile, IndexDescriptor indexDescriptor )
		 {
			  _log.info( "Schema index cleanup job started: " + IndexDescription( indexFile, indexDescriptor ) );
		 }

		 public override void RecoveryCleanupFinished( File indexFile, IndexDescriptor indexDescriptor, long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
		 {
			  StringJoiner joiner = new StringJoiner( ", ", "Schema index cleanup job finished: " + IndexDescription( indexFile, indexDescriptor ) + " ", "" );
			  joiner.add( "Number of pages visited: " + numberOfPagesVisited );
			  joiner.add( "Number of cleaned crashed pointers: " + numberOfCleanedCrashPointers );
			  joiner.add( "Time spent: " + duration( durationMillis ) );
			  _log.info( joiner.ToString() );
		 }

		 public override void RecoveryCleanupClosed( File indexFile, IndexDescriptor indexDescriptor )
		 {
			  _log.info( "Schema index cleanup job closed: " + IndexDescription( indexFile, indexDescriptor ) );
		 }

		 public override void RecoveryCleanupFailed( File indexFile, IndexDescriptor indexDescriptor, Exception throwable )
		 {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("Schema index cleanup job failed: %s.%nCaused by: %s", indexDescription(indexFile, indexDescriptor), org.apache.commons.lang3.exception.ExceptionUtils.getStackTrace(throwable)));
			  _log.info( string.Format( "Schema index cleanup job failed: %s.%nCaused by: %s", IndexDescription( indexFile, indexDescriptor ), ExceptionUtils.getStackTrace( throwable ) ) );
		 }

		 private string IndexDescription( File indexFile, IndexDescriptor indexDescriptor )
		 {
			  return "descriptor=" + indexDescriptor.ToString() + ", indexFile=" + indexFile.AbsolutePath;
		 }
	}

}