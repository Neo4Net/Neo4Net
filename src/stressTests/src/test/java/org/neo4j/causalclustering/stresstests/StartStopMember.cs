using System.Threading;

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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.stresstests.ConsistencyHelper.assertStoreConsistent;

	public class StartStopMember : WorkOnMember
	{
		 private readonly Log _log;
		 private readonly FileSystemAbstraction _fileSystem;

		 internal StartStopMember( Resources resources )
		 {
			  this._log = resources.LogProvider().getLog(this.GetType());
			  this._fileSystem = resources.FileSystem();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doWorkOnMember(Neo4Net.causalclustering.discovery.ClusterMember member) throws Exception
		 public override void DoWorkOnMember( ClusterMember member )
		 {
			  File databaseDirectory = member.database().DatabaseLayout().databaseDirectory();
			  _log.info( "Stopping: " + member );
			  member.shutdown();

			  assertStoreConsistent( _fileSystem, databaseDirectory );

			  Thread.Sleep( 5000 );
			  _log.info( "Starting: " + member );
			  member.start();
		 }
	}

}