﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Consistency.store
{

	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using MultiPassStore = Org.Neo4j.Consistency.checking.full.MultiPassStore;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	public interface RecordAccess
	{
		 RecordReference<DynamicRecord> Schema( long id );

		 RecordReference<NodeRecord> Node( long id );

		 RecordReference<RelationshipRecord> Relationship( long id );

		 RecordReference<PropertyRecord> Property( long id );

		 RecordReference<RelationshipTypeTokenRecord> RelationshipType( int id );

		 RecordReference<PropertyKeyTokenRecord> PropertyKey( int id );

		 RecordReference<DynamicRecord> String( long id );

		 RecordReference<DynamicRecord> Array( long id );

		 RecordReference<DynamicRecord> RelationshipTypeName( int id );

		 RecordReference<DynamicRecord> NodeLabels( long id );

		 RecordReference<LabelTokenRecord> Label( int id );

		 RecordReference<DynamicRecord> LabelName( int id );

		 RecordReference<DynamicRecord> PropertyKeyName( int id );

		 RecordReference<NeoStoreRecord> Graph();

		 RecordReference<RelationshipGroupRecord> RelationshipGroup( long id );

		 // The following methods doesn't belong here, but makes code in the rest of the CC immensely easier

		 IEnumerator<PropertyRecord> RawPropertyChain( long firstId );

		 bool ShouldCheck( long id, MultiPassStore store );

		 CacheAccess CacheAccess();
	}

}