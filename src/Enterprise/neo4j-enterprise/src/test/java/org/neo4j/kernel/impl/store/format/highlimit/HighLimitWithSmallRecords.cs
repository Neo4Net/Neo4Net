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
namespace Neo4Net.Kernel.impl.store.format.highlimit
{
	using Neo4Net.Kernel.impl.store.format;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;

	/// <summary>
	/// A <seealso cref="HighLimit"/> record format that forces records to be split in two units more often than the original format.
	/// </summary>
	public class HighLimitWithSmallRecords : HighLimit
	{
		 public new const string NAME = "high_limit_with_small_records";
		 public new const string StoreVersion = "vT.H.0";
		 public new static readonly RecordFormats RecordFormats = new HighLimitWithSmallRecords();

		 private static readonly int _nodeRecordSize = NodeRecordFormat.RECORD_SIZE / 2;
		 private static readonly int _relationshipRecordSize = RelationshipRecordFormat.RECORD_SIZE / 2;
		 private static readonly int _relationshipGroupRecordSize = RelationshipGroupRecordFormat.RECORD_SIZE / 2;

		 private HighLimitWithSmallRecords()
		 {
		 }

		 public override string StoreVersion()
		 {
			  return StoreVersion;
		 }

		 public override RecordFormat<NodeRecord> Node()
		 {
			  return new NodeRecordFormat( _nodeRecordSize );
		 }

		 public override RecordFormat<RelationshipRecord> Relationship()
		 {
			  return new RelationshipRecordFormat( _relationshipRecordSize );
		 }

		 public override RecordFormat<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return new RelationshipGroupRecordFormat( _relationshipGroupRecordSize );
		 }
	}

}