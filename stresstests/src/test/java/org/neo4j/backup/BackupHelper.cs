using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.backup
{

	using BackupClient = Org.Neo4j.backup.impl.BackupClient;
	using BackupOutcome = Org.Neo4j.backup.impl.BackupOutcome;
	using BackupProtocolService = Org.Neo4j.backup.impl.BackupProtocolService;
	using ConsistencyCheck = Org.Neo4j.backup.impl.ConsistencyCheck;
	using Predicates = Org.Neo4j.Function.Predicates;
	using IsChannelClosedException = Org.Neo4j.helper.IsChannelClosedException;
	using IsConnectionException = Org.Neo4j.helper.IsConnectionException;
	using IsConnectionResetByPeer = Org.Neo4j.helper.IsConnectionResetByPeer;
	using IsStoreClosed = Org.Neo4j.helper.IsStoreClosed;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupProtocolServiceFactory.backupProtocolService;

	public class BackupHelper
	{

		 private static readonly System.Predicate<Exception> _isTransientError = Predicates.any( new IsConnectionException(), new IsConnectionResetByPeer(), new IsChannelClosedException(), new IsStoreClosed() );

		 private BackupHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static BackupResult backup(String host, int port, java.nio.file.Path targetDirectory) throws Exception
		 public static BackupResult Backup( string host, int port, Path targetDirectory )
		 {
			  MemoryStream outputStream = new MemoryStream();
			  bool consistent = true;
			  bool transientFailure = false;
			  bool failure = false;
			  try
			  {
					using ( BackupProtocolService backupProtocolService = backupProtocolService( outputStream ) )
					{
						 BackupOutcome backupOutcome = backupProtocolService.DoIncrementalBackupOrFallbackToFull( host, port, DatabaseLayout.of( targetDirectory.toFile() ), ConsistencyCheck.FULL, Config.defaults(), BackupClient.BIG_READ_TIMEOUT, false );
						 consistent = backupOutcome.Consistent;
					}
			  }
			  catch ( Exception t )
			  {
					if ( _isTransientError.test( t ) )
					{
						 transientFailure = true;
					}
					else
					{
						 failure = true;
						 throw t;
					}
			  }
			  finally
			  {
					if ( !consistent || failure )
					{
						 FlushToStandardOutput( outputStream );
					}
					IOUtils.closeAllSilently( outputStream );
			  }
			  return new BackupResult( consistent, transientFailure );
		 }

		 private static void FlushToStandardOutput( MemoryStream outputStream )
		 {
			  try
			  {
					outputStream.writeTo( System.out );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}