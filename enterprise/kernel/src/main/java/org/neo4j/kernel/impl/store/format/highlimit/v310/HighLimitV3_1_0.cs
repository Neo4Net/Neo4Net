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
namespace Org.Neo4j.Kernel.impl.store.format.highlimit.v310
{
	using Org.Neo4j.Kernel.impl.store.format;
	using LabelTokenRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.LabelTokenRecordFormat;
	using PropertyKeyTokenRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.PropertyKeyTokenRecordFormat;
	using RelationshipTypeTokenRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.RelationshipTypeTokenRecordFormat;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	/// <summary>
	/// Record format with very high limits, 50-bit per ID, while at the same time keeping store size small.
	/// </summary>
	/// <seealso cref= BaseHighLimitRecordFormatV3_1_0 </seealso>
	public class HighLimitV3_1_0 : BaseRecordFormats
	{
		 /// <summary>
		 /// Default maximum number of bits that can be used to represent id
		 /// </summary>
		 internal const int DEFAULT_MAXIMUM_BITS_PER_ID = 50;

		 public static readonly string StoreVersion = StoreVersion.HIGH_LIMIT_V3_1_0.versionString();

		 public static readonly RecordFormats RecordFormats = new HighLimitV3_1_0();
		 public const string NAME = "high_limitV3_1_0";

		 public HighLimitV3_1_0() : base(StoreVersion, StoreVersion.HIGH_LIMIT_V3_1_0.introductionVersion(), 3, Capability.DENSE_NODES, Capability.RELATIONSHIP_TYPE_3BYTES, Capability.SCHEMA, Capability.LUCENE_5, Capability.SECONDARY_RECORD_UNITS)
		 {
		 }

		 public override RecordFormat<NodeRecord> Node()
		 {
			  return new NodeRecordFormatV3_1_0();
		 }

		 public override RecordFormat<RelationshipRecord> Relationship()
		 {
			  return new RelationshipRecordFormatV3_1_0();
		 }

		 public override RecordFormat<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return new RelationshipGroupRecordFormatV3_1_0();
		 }

		 public override RecordFormat<PropertyRecord> Property()
		 {
			  return new PropertyRecordFormatV3_1_0();
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