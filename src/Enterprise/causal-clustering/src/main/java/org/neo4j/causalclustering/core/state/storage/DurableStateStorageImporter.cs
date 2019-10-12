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
namespace Neo4Net.causalclustering.core.state.storage
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class DurableStateStorageImporter<STATE> : DurableStateStorage<STATE>
	{
		 public DurableStateStorageImporter( FileSystemAbstraction fileSystemAbstraction, File stateDir, string name, StateMarshal<STATE> marshal, int numberOfEntriesBeforeRotation, System.Func<DatabaseHealth> databaseHealthSupplier, LogProvider logProvider ) : base( fileSystemAbstraction, stateDir, name, marshal, numberOfEntriesBeforeRotation, logProvider )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void persist(STATE state) throws java.io.IOException
		 public virtual void Persist( STATE state )
		 {
			  base.PersistStoreData( state );
			  base.SwitchStoreFile();
			  base.PersistStoreData( state );
		 }
	}

}