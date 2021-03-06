﻿/*
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
namespace Org.Neo4j.Consistency.checking.full
{

	using Org.Neo4j.Consistency.checking;
	using Org.Neo4j.Consistency.checking;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.NodeInUseWithCorrectLabelsCheck.sortAndDeduplicate;

	public class RelationshipInUseWithCorrectRelationshipTypeCheck <RECORD, REPORT> : ComparativeRecordChecker<RECORD, RelationshipRecord, REPORT> where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipInUseWithCorrectRelationshipTypeReport
	{
		 private readonly long[] _indexRelationshipTypes;

		 public RelationshipInUseWithCorrectRelationshipTypeCheck( long[] expectedEntityTokenIds )
		 {
			  this._indexRelationshipTypes = sortAndDeduplicate( expectedEntityTokenIds );
		 }

		 public override void CheckReference( RECORD record, RelationshipRecord relationshipRecord, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  if ( relationshipRecord.InUse() )
			  {
					// Relationship indexes are always semantically multi-token, which means that the relationship record just need to have one of the possible
					// relationship types mentioned by the index. Relationships can't have more than one type anyway.
					long type = relationshipRecord.Type;
					if ( Arrays.binarySearch( _indexRelationshipTypes, type ) < 0 )
					{
						 // The relationship did not have any of the relationship types mentioned by the index.
						 foreach ( long indexRelationshipType in _indexRelationshipTypes )
						 {
							  engine.Report().relationshipDoesNotHaveExpectedRelationshipType(relationshipRecord, indexRelationshipType);
						 }
					}
			  }
			  else
			  {
					engine.Report().relationshipNotInUse(relationshipRecord);
			  }
		 }
	}

}