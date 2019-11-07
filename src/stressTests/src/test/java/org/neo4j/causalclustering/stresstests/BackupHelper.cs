using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.stresstests
{

	using OnlineBackupCommandBuilder = Neo4Net.backup.impl.OnlineBackupCommandBuilder;
	using Neo4Net.causalclustering.discovery;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.backup.impl.SelectedBackupProtocol.CATCHUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.transaction_advertised_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.findCauseOrSuppressed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.NullOutputStream.NULL_OUTPUT_STREAM;

	internal class BackupHelper
	{
		 private static readonly ISet<Type> _benignExceptions = asSet( typeof( ConnectException ), typeof( ClosedChannelException ) );

		 internal AtomicLong BackupNumber = new AtomicLong();
		 internal AtomicLong SuccessfulBackups = new AtomicLong();

		 private readonly File _baseBackupDir;
		 private readonly Log _log;

		 internal BackupHelper( Resources resources )
		 {
			  this._baseBackupDir = resources.BackupDir();
			  this._log = resources.LogProvider().getLog(this.GetType());
		 }

		 /// <summary>
		 /// Performs a backup and returns the path to it. Benign failures are swallowed and an empty optional gets returned.
		 /// </summary>
		 /// <param name="member"> The member to perform the backup against. </param>
		 /// <returns> The optional backup. </returns>
		 /// <exception cref="Exception"> If any unexpected exceptions happen. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Optional<java.io.File> backup(Neo4Net.causalclustering.discovery.ClusterMember member) throws Exception
		 internal virtual Optional<File> Backup( ClusterMember member )
		 {
			  AdvertisedSocketAddress address = member.config().Get(transaction_advertised_address);
			  string backupName = "backup-" + BackupNumber.AndIncrement;

			  OnlineBackupCommandBuilder backupCommand = ( new OnlineBackupCommandBuilder() ).withOutput(NULL_OUTPUT_STREAM).withSelectedBackupStrategy(CATCHUP).withConsistencyCheck(true).withHost(address.Hostname).withPort(address.Port);

			  try
			  {
					backupCommand.Backup( _baseBackupDir, backupName );
					_log.info( string.Format( "Created backup {0} from {1}", backupName, member ) );

					SuccessfulBackups.incrementAndGet();

					return ( new File( _baseBackupDir, backupName ) );
			  }
			  catch ( CommandFailed e )
			  {
					Optional<Exception> benignException = findCauseOrSuppressed( e, t => _benignExceptions.Contains( t.GetType() ) );
					if ( benignException.Present )
					{
						 _log.info( "Benign failure: " + benignException.get().Message );
					}
					else
					{
						 throw e;
					}
			  }
			  return null;
		 }
	}

}