﻿/*
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
namespace Neo4Net.Consistency.checking
{
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using Neo4Net.Consistency.Store;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;

	public interface ICheckerEngine<RECORD, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
	{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: <REFERRED extends Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void comparativeCheck(Neo4Net.consistency.store.RecordReference<REFERRED> other, ComparativeRecordChecker<RECORD, ? super REFERRED, REPORT> checker);
		 void comparativeCheck<REFERRED, T1>( RecordReference<REFERRED> other, ComparativeRecordChecker<T1> checker );

		 REPORT Report();
	}

}