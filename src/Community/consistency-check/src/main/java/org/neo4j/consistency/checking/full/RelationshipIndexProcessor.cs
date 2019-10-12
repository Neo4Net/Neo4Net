using System.Collections.Generic;

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
namespace Neo4Net.Consistency.checking.full
{

	using IndexAccessors = Neo4Net.Consistency.checking.index.IndexAccessors;
	using ConsistencyReporter = Neo4Net.Consistency.report.ConsistencyReporter;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	public class RelationshipIndexProcessor : RecordProcessor_Adapter<RelationshipRecord>
	{
		 private readonly ConsistencyReporter _reporter;
		 private readonly RelationshipToIndexCheck _checker;

		 internal RelationshipIndexProcessor( ConsistencyReporter reporter, IndexAccessors indexes, PropertyReader propertyReader, IList<StoreIndexDescriptor> relationshipIndexes )
		 {
			  this._reporter = reporter;
			  _checker = new RelationshipToIndexCheck( relationshipIndexes, indexes, propertyReader );
		 }

		 public override void Process( RelationshipRecord relationshipRecord )
		 {
			  _reporter.forRelationship( relationshipRecord, _checker );
		 }
	}

}