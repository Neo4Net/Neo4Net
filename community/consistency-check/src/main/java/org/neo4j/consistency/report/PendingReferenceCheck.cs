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
namespace Org.Neo4j.Consistency.report
{
	using Org.Neo4j.Consistency.checking;
	using Org.Neo4j.Consistency.checking;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	public class PendingReferenceCheck<REFERENCED> where REFERENCED : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private CheckerEngine _engine;
		 private readonly ComparativeRecordChecker _checker;

		 internal PendingReferenceCheck( CheckerEngine engine, ComparativeRecordChecker checker )
		 {
			  this._engine = engine;
			  this._checker = checker;
		 }

		 public override string ToString()
		 {
			 lock ( this )
			 {
				  if ( _engine == null )
				  {
						return string.Format( "CompletedReferenceCheck{{{0}}}", _checker );
				  }
				  else
				  {
						return ConsistencyReporter.PendingCheckToString( _engine, _checker );
				  }
			 }
		 }

		 public virtual void CheckReference( REFERENCED referenced, RecordAccess records )
		 {
			  ConsistencyReporter.DispatchReference( Engine(), _checker, referenced, records );
		 }

		 public virtual void CheckDiffReference( REFERENCED oldReferenced, REFERENCED newReferenced, RecordAccess records )
		 {
			  ConsistencyReporter.DispatchChangeReference( Engine(), _checker, oldReferenced, newReferenced, records );
		 }

		 public virtual void Skip()
		 {
			 lock ( this )
			 {
				  if ( _engine != null )
				  {
						ConsistencyReporter.DispatchSkip( _engine );
						_engine = null;
				  }
			 }
		 }

		 private CheckerEngine Engine()
		 {
			 lock ( this )
			 {
				  if ( _engine == null )
				  {
						throw new System.InvalidOperationException( "Reference has already been checked." );
				  }
				  try
				  {
						return _engine;
				  }
				  finally
				  {
						_engine = null;
				  }
			 }
		 }
	}

}