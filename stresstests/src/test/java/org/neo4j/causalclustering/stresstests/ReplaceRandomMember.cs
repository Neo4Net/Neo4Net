using System;
using System.Threading;

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
namespace Org.Neo4j.causalclustering.stresstests
{
	using Matchers = org.hamcrest.Matchers;


	using Org.Neo4j.causalclustering.discovery;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.BackupUtil.restoreFromBackup;

	internal class ReplaceRandomMember : RepeatOnRandomMember
	{
		 /* Basic pass criteria for the stress test. We must have replaced at least two members. */
		 private const int MIN_SUCCESSFUL_REPLACEMENTS = 2;

		 /* Backups retry a few times with a pause in between. */
		 private const long MAX_BACKUP_FAILURES = 20;
		 private const long RETRY_TIMEOUT_MILLIS = 5000;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private readonly Cluster<object> _cluster;
		 private readonly FileSystemAbstraction _fs;
		 private readonly Log _log;
		 private readonly BackupHelper _backupHelper;

		 private int _successfulReplacements;

		 internal ReplaceRandomMember( Control control, Resources resources ) : base( control, resources )
		 {
			  this._cluster = resources.Cluster();
			  this._backupHelper = new BackupHelper( resources );
			  this._fs = resources.FileSystem();
			  this._log = resources.LogProvider().getLog(this.GetType());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doWorkOnMember(org.neo4j.causalclustering.discovery.ClusterMember oldMember) throws Exception
		 public override void DoWorkOnMember( ClusterMember oldMember )
		 {
			  bool replaceFromBackup = ThreadLocalRandom.current().nextBoolean();

			  File backup = null;
			  if ( replaceFromBackup )
			  {
					backup = CreateBackupWithRetries( oldMember );
			  }

			  _log.info( "Stopping: " + oldMember );
			  oldMember.shutdown();

			  ClusterMember newMember = ( oldMember is CoreClusterMember ) ? _cluster.newCoreMember() : _cluster.newReadReplica();

			  if ( replaceFromBackup )
			  {
					_log.info( "Restoring backup: " + backup.Name + " to: " + newMember );
					restoreFromBackup( backup, _fs, newMember );
					_fs.deleteRecursively( backup );
			  }

			  _log.info( "Starting: " + newMember );
			  newMember.start();

			  _successfulReplacements++;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createBackupWithRetries(org.neo4j.causalclustering.discovery.ClusterMember member) throws Exception
		 private File CreateBackupWithRetries( ClusterMember member )
		 {
			  int failureCount = 0;

			  while ( true )
			  {
					Optional<File> backupOpt = _backupHelper.backup( member );
					if ( backupOpt.Present )
					{
						 return backupOpt.get();
					}
					else
					{
						 failureCount++;

						 if ( failureCount >= MAX_BACKUP_FAILURES )
						 {
							  throw new Exception( format( "Backup failed %s times in a row.", failureCount ) );
						 }

						 _log.info( "Retrying backup in %s ms.", RETRY_TIMEOUT_MILLIS );
						 Thread.Sleep( RETRY_TIMEOUT_MILLIS );
					}
			  }
		 }

		 public override void Validate()
		 {
			  assertThat( _successfulReplacements, Matchers.greaterThanOrEqualTo( MIN_SUCCESSFUL_REPLACEMENTS ) );
		 }
	}

}