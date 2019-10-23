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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRecord = Neo4Net.Kernel.Impl.Store.Records.SchemaRecord;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;

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