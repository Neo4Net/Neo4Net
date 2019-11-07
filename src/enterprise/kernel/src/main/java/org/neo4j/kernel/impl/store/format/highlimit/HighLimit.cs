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
namespace Neo4Net.Kernel.impl.store.format.highlimit
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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.format.highlimit.HighLimitFormatSettings.RELATIONSHIP_TYPE_TOKEN_MAXIMUM_ID_BITS;

	/// <summary>
	/// Record format with very high limits, 50-bit per ID, while at the same time keeping store size small.
	/// </summary>
	/// <seealso cref= BaseHighLimitRecordFormat </seealso>
	public class HighLimit : BaseRecordFormats
	{
		 public static readonly string StoreVersion = StoreVersion.HIGH_LIMIT_V3_4_0.versionString();

		 public static readonly RecordFormats RecordFormats = new HighLimit();
		 public const string NAME = "high_limit";

		 protected internal HighLimit() : base(StoreVersion, StoreVersion.HIGH_LIMIT_V3_4_0.introductionVersion(), 5, Capability.DENSE_NODES, Capability.RELATIONSHIP_TYPE_3BYTES, Capability.SCHEMA, Capability.LUCENE_5, Capability.POINT_PROPERTIES, Capability.TEMPORAL_PROPERTIES, Capability.SECONDARY_RECORD_UNITS)
		 {
		 }

		 public override RecordFormat<NodeRecord> Node()
		 {
			  return new NodeRecordFormat();
		 }

		 public override RecordFormat<RelationshipRecord> Relationship()
		 {
			  return new RelationshipRecordFormat();
		 }

		 public override RecordFormat<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return new RelationshipGroupRecordFormat();
		 }

		 public override RecordFormat<PropertyRecord> Property()
		 {
			  return new PropertyRecordFormat();
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
			  return new RelationshipTypeTokenRecordFormat( RELATIONSHIP_TYPE_TOKEN_MAXIMUM_ID_BITS );
		 }

		 public override RecordFormat<DynamicRecord> Dynamic()
		 {
			  return new DynamicRecordFormat();
		 }

		 public override FormatFamily FormatFamily
		 {
			 get
			 {
				  return HighLimitFormatFamily.Instance;
			 }
		 }

		 public override string Name()
		 {
			  return NAME;
		 }
	}

}