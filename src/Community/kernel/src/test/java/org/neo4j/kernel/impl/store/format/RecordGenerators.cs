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
namespace Neo4Net.Kernel.impl.store.format
{
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	public interface RecordGenerators
	{

		 RecordGenerators_Generator<NodeRecord> Node();

		 RecordGenerators_Generator<RelationshipRecord> Relationship();

		 RecordGenerators_Generator<PropertyRecord> Property();

		 RecordGenerators_Generator<RelationshipGroupRecord> RelationshipGroup();

		 RecordGenerators_Generator<RelationshipTypeTokenRecord> RelationshipTypeToken();

		 RecordGenerators_Generator<PropertyKeyTokenRecord> PropertyKeyToken();

		 RecordGenerators_Generator<LabelTokenRecord> LabelToken();

		 RecordGenerators_Generator<DynamicRecord> Dynamic();
	}

	 public interface RecordGenerators_Generator<RECORD> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
	 {
		  RECORD Get( int recordSize, RecordFormat<RECORD> format, long id );
	 }

}