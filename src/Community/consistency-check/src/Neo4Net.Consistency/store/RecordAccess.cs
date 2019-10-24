using System.Collections.Generic;

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
namespace Neo4Net.Consistency.Store
{

	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using MultiPassStore = Neo4Net.Consistency.checking.full.MultiPassStore;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

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