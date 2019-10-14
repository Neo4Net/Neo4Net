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
namespace Neo4Net.Kernel.impl.store.format.highlimit.v320
{
	using Neo4Net.Kernel.impl.store.format;
	using LabelTokenRecordFormat = Neo4Net.Kernel.impl.store.format.standard.LabelTokenRecordFormat;
	using PropertyKeyTokenRecordFormat = Neo4Net.Kernel.impl.store.format.standard.PropertyKeyTokenRecordFormat;
	using RelationshipTypeTokenRecordFormat = Neo4Net.Kernel.impl.store.format.standard.RelationshipTypeTokenRecordFormat;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

	/// <summary>
	/// Record format with very high limits, 50-bit per ID, while at the same time keeping store size small.
	/// </summary>
	/// <seealso cref= BaseHighLimitRecordFormatV3_2_0 </seealso>
	public class HighLimitV3_2_0 : BaseRecordFormats
	{
		 public static readonly string StoreVersion = StoreVersion.HIGH_LIMIT_V3_2_0.versionString();

		 public static readonly RecordFormats RecordFormats = new HighLimitV3_2_0();
		 public const string NAME = "high_limitV3_2_0";

		 public HighLimitV3_2_0() : base(StoreVersion, StoreVersion.HIGH_LIMIT_V3_2_0.introductionVersion(), 4, Capability.DENSE_NODES, Capability.RELATIONSHIP_TYPE_3BYTES, Capability.SCHEMA, Capability.LUCENE_5, Capability.SECONDARY_RECORD_UNITS)
		 {
		 }

		 public override RecordFormat<NodeRecord> Node()
		 {
			  return new NodeRecordFormatV3_2_0();
		 }

		 public override RecordFormat<RelationshipRecord> Relationship()
		 {
			  return new RelationshipRecordFormatV3_2_0();
		 }

		 public override RecordFormat<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return new RelationshipGroupRecordFormatV3_2_0();
		 }

		 public override RecordFormat<PropertyRecord> Property()
		 {
			  return new PropertyRecordFormatV3_2_0();
		 }

		 public override RecordFormat<LabelTokenRecord> LabelToken()
		 {
			  return new LabelTokenRecordFormat();
		 }

		 public override RecordFormat<PropertyKeyTokenRecord> PropertyKeyToken()
		 {
			  return new PropertyKeyTokenRecordFormat();
		 }

		 public override RecordFormat<RelationshipTypeTokenRecord> RelationshipTypeToken()
		 {
			  return new RelationshipTypeTokenRecordFormat();
		 }

		 public override RecordFormat<DynamicRecord> Dynamic()
		 {
			  return new DynamicRecordFormat();
		 }

		 public override FormatFamily FormatFamily
		 {
			 get
			 {
				  return HighLimitFormatFamily.INSTANCE;
			 }
		 }

		 public override string Name()
		 {
			  return NAME;
		 }
	}

}