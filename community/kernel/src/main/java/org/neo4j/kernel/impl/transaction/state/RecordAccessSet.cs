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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRecord = Org.Neo4j.Kernel.impl.store.record.SchemaRecord;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;

	public interface RecordAccessSet
	{
		 RecordAccess<NodeRecord, Void> NodeRecords { get; }

		 RecordAccess<PropertyRecord, PrimitiveRecord> PropertyRecords { get; }

		 RecordAccess<RelationshipRecord, Void> RelRecords { get; }

		 RecordAccess<RelationshipGroupRecord, int> RelGroupRecords { get; }

		 RecordAccess<SchemaRecord, SchemaRule> SchemaRuleChanges { get; }

		 RecordAccess<PropertyKeyTokenRecord, Void> PropertyKeyTokenChanges { get; }

		 RecordAccess<LabelTokenRecord, Void> LabelTokenChanges { get; }

		 RecordAccess<RelationshipTypeTokenRecord, Void> RelationshipTypeTokenChanges { get; }

		 bool HasChanges();

		 int ChangeSize();

		 void Close();
	}

}