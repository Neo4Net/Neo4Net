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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Neo4Net.Kernel.impl.store;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StorePrepareIdSequence = Neo4Net.@unsafe.Impl.Batchimport.store.StorePrepareIdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.RelationshipGroupCache.GROUP_ENTRY_SIZE;

	/// <summary>
	/// Writes cached <seealso cref="RelationshipGroupRecord"/> from <seealso cref="ScanAndCacheGroupsStage"/> to store. This is done
	/// as a separate step because here the cache is supposed to contain complete chains of relationship group records
	/// for a section of the node store. Steps:
	/// 
	/// <ol>
	/// <li><seealso cref="ReadGroupsFromCacheStep"/> reads complete relationship group chains from <seealso cref="RelationshipGroupCache"/>.
	/// </li>
	/// <li><seealso cref="EncodeGroupsStep"/> sets correct <seealso cref="RelationshipGroupRecord.setNext(long)"/> pointers for records.</li>
	/// <li><seealso cref="UpdateRecordsStep"/> writes the relationship group records to store.</li>
	/// </ol>
	/// </summary>
	public class WriteGroupsStage : Stage
	{
		 public const string NAME = "Write";

		 public WriteGroupsStage( Configuration config, RelationshipGroupCache cache, RecordStore<RelationshipGroupRecord> store ) : base( NAME, null, config, 0 )
		 {
			  Add( new ReadGroupsFromCacheStep( Control(), config, cache.GetEnumerator(), GROUP_ENTRY_SIZE ) );
			  Add( new EncodeGroupsStep( Control(), config, store ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new UpdateRecordsStep<>(control(), config, store, new org.neo4j.unsafe.impl.batchimport.store.StorePrepareIdSequence()));
			  Add( new UpdateRecordsStep<object>( Control(), config, store, new StorePrepareIdSequence() ) );
		 }
	}

}