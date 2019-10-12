/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.store.format.standard
{
	using Org.Neo4j.Kernel.impl.store.format;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	public class StandardV3_4 : BaseRecordFormats
	{
		 public static readonly string StoreVersion = StoreVersion.STANDARD_V3_4.versionString();
		 public static readonly RecordFormats RecordFormats = new StandardV3_4();
		 public const string NAME = "standard";

		 public StandardV3_4() : base(StoreVersion, StoreVersion.STANDARD_V3_4.introductionVersion(), 8, Capability.SCHEMA, Capability.DENSE_NODES, Capability.LUCENE_5, Capability.POINT_PROPERTIES, Capability.TEMPORAL_PROPERTIES)
		 {
		 }

		 public override RecordFormat<NodeRecord> Node()
		 {
			  return new NodeRecordFormat();
		 }

		 public override RecordFormat<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return new RelationshipGroupRecordFormat();
		 }

		 public override RecordFormat<RelationshipRecord> Relationship()
		 {
			  return new RelationshipRecordFormat();
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
				  return StandardFormatFamily.Instance;
			 }
		 }

		 public override string Name()
		 {
			  return NAME;
		 }
	}

}