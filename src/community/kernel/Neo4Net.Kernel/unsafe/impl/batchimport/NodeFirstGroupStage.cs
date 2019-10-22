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
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using Neo4Net.Kernel.impl.store;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;
	using BatchFeedStep = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchFeedStep;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StorePrepareIdSequence = Neo4Net.@unsafe.Impl.Batchimport.store.StorePrepareIdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.RecordIdIterator.allIn;

	/// <summary>
	/// Updates dense nodes with which will be the <seealso cref="NodeRecord.setNextRel(long) first group"/> to point to,
	/// after a <seealso cref="RelationshipGroupDefragmenter"/> has been run.
	/// </summary>
	public class NodeFirstGroupStage : Stage
	{
		 public const string NAME = "Node --> Group";

		 public NodeFirstGroupStage( Configuration config, RecordStore<RelationshipGroupRecord> groupStore, NodeStore nodeStore, ByteArray cache ) : base( NAME, null, config, 0 )
		 {
			  Add( new BatchFeedStep( Control(), config, allIn(groupStore, config), groupStore.RecordSize ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new org.Neo4Net.unsafe.impl.batchimport.staging.ReadRecordsStep<>(control(), config, true, groupStore));
			  Add( new ReadRecordsStep<object>( Control(), config, true, groupStore ) );
			  Add( new NodeSetFirstGroupStep( Control(), config, nodeStore, cache ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: add(new UpdateRecordsStep<>(control(), config, nodeStore, new org.Neo4Net.unsafe.impl.batchimport.store.StorePrepareIdSequence()));
			  Add( new UpdateRecordsStep<object>( Control(), config, nodeStore, new StorePrepareIdSequence() ) );
		 }
	}

}