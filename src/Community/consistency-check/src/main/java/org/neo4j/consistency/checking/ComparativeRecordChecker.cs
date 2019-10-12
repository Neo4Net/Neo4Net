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
namespace Neo4Net.Consistency.checking
{
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;

	public interface ComparativeRecordChecker<RECORD, REFERRED, REPORT> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REFERRED : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
	{
		 void CheckReference( RECORD record, REFERRED referred, CheckerEngine<RECORD, REPORT> engine, RecordAccess records );
	}

}