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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using Neo4Net.Kernel.impl.transaction.state;
	using Tracker = Neo4Net.Kernel.impl.transaction.state.RelationshipCreatorTest.Tracker;

	public class TrackingRecordProxy<RECORD, ADDITIONAL> : RecordAccess_RecordProxy<RECORD, ADDITIONAL>
	{
		 private readonly RecordAccess_RecordProxy<RECORD, ADDITIONAL> @delegate;
		 private readonly Tracker _tracker;
		 private readonly bool _created;
		 private bool _changed;

		 public TrackingRecordProxy( RecordAccess_RecordProxy<RECORD, ADDITIONAL> @delegate, bool created, Tracker tracker )
		 {
			  this.@delegate = @delegate;
			  this._created = created;
			  this._tracker = tracker;
			  this._changed = created;
		 }

		 public virtual long Key
		 {
			 get
			 {
				  return @delegate.Key;
			 }
		 }

		 public override RECORD ForChangingLinkage()
		 {
			  TrackChange();
			  return @delegate.ForChangingLinkage();
		 }

		 private void TrackChange()
		 {
			  if ( !_created && !_changed )
			  {
					_tracker.changingRelationship( Key );
					_changed = true;
			  }
		 }

		 public override RECORD ForChangingData()
		 {
			  TrackChange();
			  return @delegate.ForChangingData();
		 }

		 public override RECORD ForReadingLinkage()
		 {
			  return @delegate.ForReadingLinkage();
		 }

		 public override RECORD ForReadingData()
		 {
			  return @delegate.ForReadingData();
		 }

		 public virtual ADDITIONAL AdditionalData
		 {
			 get
			 {
				  return @delegate.AdditionalData;
			 }
		 }

		 public virtual RECORD Before
		 {
			 get
			 {
				  return @delegate.Before;
			 }
		 }

		 public virtual bool Changed
		 {
			 get
			 {
				  return _changed;
			 }
		 }

		 public virtual bool Created
		 {
			 get
			 {
				  return _created;
			 }
		 }
	}

}