using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.backup.stresstests
{

	using Control = Neo4Net.causalclustering.stresstests.Control;
	using Workload = Neo4Net.helper.Workload;

	internal class BackupLoad : Workload
	{

		 private readonly string _backupHostname;
		 private readonly int _backupPort;
		 private readonly Path _backupDir;

		 internal BackupLoad( Control control, string backupHostname, int backupPort, Path backupDir ) : base( control )
		 {
			  this._backupHostname = backupHostname;
			  this._backupPort = backupPort;
			  this._backupDir = backupDir;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void doWork() throws Exception
		 protected internal override void DoWork()
		 {
			  BackupResult backupResult = BackupHelper.backup( _backupHostname, _backupPort, _backupDir );
			  if ( !backupResult.Consistent )
			  {
					throw new Exception( "Inconsistent backup" );
			  }
			  if ( backupResult.TransientErrorOnBackup )
			  {
					LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 ) );
			  }
		 }
	}

}