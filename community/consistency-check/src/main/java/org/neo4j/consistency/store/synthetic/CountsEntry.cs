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
namespace Org.Neo4j.Consistency.store.synthetic
{
	using Org.Neo4j.Consistency.checking;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using CountsKey = Org.Neo4j.Kernel.impl.store.counts.keys.CountsKey;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	/// <summary>
	/// Synthetic record type that stands in for a real record to fit in conveniently
	/// with consistency checking
	/// </summary>
	public class CountsEntry : AbstractBaseRecord
	{
		 private CountsKey _key;
		 private long _count;

		 public CountsEntry( CountsKey key, long count ) : base( -1 )
		 {
			  this._key = key;
			  this._count = count;
			  InUse = true;
		 }

		 public override void Clear()
		 {
			  base.Clear();
			  _key = null;
			  _count = 0;
		 }

		 public override string ToString()
		 {
			  return "CountsEntry[" + _key + ": " + _count + "]";
		 }

		 public override long Id
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public virtual CountsKey CountsKey
		 {
			 get
			 {
				  return _key;
			 }
		 }

		 public virtual long Count
		 {
			 get
			 {
				  return _count;
			 }
		 }

		 public abstract class CheckAdapter : RecordCheck<CountsEntry, Org.Neo4j.Consistency.report.ConsistencyReport_CountsConsistencyReport>
		 {
			 public abstract void Check( RECORD record, Org.Neo4j.Consistency.checking.CheckerEngine<RECORD, REPORT> engine, Org.Neo4j.Consistency.store.RecordAccess records );
		 }
	}

}