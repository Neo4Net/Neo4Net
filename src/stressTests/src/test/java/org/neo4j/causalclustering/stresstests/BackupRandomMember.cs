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

	using Neo4Net.causalclustering.discovery;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;

	internal class BackupRandomMember : RepeatOnRandomMember
	{
		 private readonly Log _log;
		 private readonly BackupHelper _backupHelper;
		 private readonly FileSystemAbstraction _fs;

		 internal BackupRandomMember( Control control, Resources resources ) : base( control, resources )
		 {
			  this._log = resources.LogProvider().getLog(this.GetType());
			  this._fs = resources.FileSystem();
			  this._backupHelper = new BackupHelper( resources );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doWorkOnMember(Neo4Net.causalclustering.discovery.ClusterMember member) throws Exception
		 public override void DoWorkOnMember( ClusterMember member )
		 {
			  Optional<File> backupDir = _backupHelper.backup( member );
			  if ( backupDir.Present )
			  {
					_fs.deleteRecursively( backupDir.get() );
			  }
		 }

		 public override void Validate()
		 {
			  if ( _backupHelper.successfulBackups.get() == 0 )
			  {
					throw new System.InvalidOperationException( "Failed to perform any backups" );
			  }

			  _log.info( string.Format( "Performed {0:D}/{1:D} successful backups.", _backupHelper.successfulBackups.get(), _backupHelper.backupNumber.get() ) );
		 }
	}

}