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
namespace Neo4Net.Kernel.impl.store.record
{
	using CloneableInPublic = Neo4Net.Helpers.CloneableInPublic;

	/// <summary>
	/// <seealso cref="AbstractBaseRecord records"/> are intended to be reusable. Created with a zero-arg constructor
	/// and initialized with the public {@code initialize} method exposed by the specific record implementations,
	/// or <seealso cref="clear() cleared"/> if reading a record that isn't in use.
	/// </summary>
	public abstract class AbstractBaseRecord : CloneableInPublic
	{
		 public const int NO_ID = -1;
		 private long _id;
		 // Used for the "record unit" feature where one logical record may span two physical records,
		 // as to still keep low and fixed record size, but support occasionally bigger records.
		 private long _secondaryUnitId;
		 // This flag is for when a record required a secondary unit, was changed, as a result of that change
		 // no longer requires that secondary unit and gets updated. In that scenario we still want to know
		 // about the secondary unit id so that we can free it when the time comes to apply the record to store.
		 private bool _requiresSecondaryUnit;
		 private bool _inUse;
		 private bool _created;
		 // Flag that indicates usage of fixed references format.
		 // Fixed references format allows to avoid encoding/decoding of references in variable length format and as result
		 // speed up records read/write operations.
		 private bool _useFixedReferences;

		 protected internal AbstractBaseRecord( long id )
		 {
			  this._id = id;
			  Clear();
		 }

		 protected internal virtual AbstractBaseRecord Initialize( bool inUse )
		 {
			  this._inUse = inUse;
			  this._created = false;
			  this._secondaryUnitId = NO_ID;
			  this._requiresSecondaryUnit = false;
			  this._useFixedReferences = false;
			  return this;
		 }

		 /// <summary>
		 /// Clears this record to its initial state. Initializing this record with an {@code initialize-method}
		 /// doesn't require clear the record first, either initialize or clear suffices.
		 /// Subclasses, most specific subclasses only, implements this method by calling initialize with
		 /// zero-like arguments.
		 /// </summary>
		 public virtual void Clear()
		 {
			  _inUse = false;
			  _created = false;
			  _secondaryUnitId = NO_ID;
			  _requiresSecondaryUnit = false;
			  this._useFixedReferences = false;
		 }

		 public virtual long Id
		 {
			 get
			 {
				  return _id;
			 }
			 set
			 {
				  this._id = value;
			 }
		 }

		 public virtual int IntId
		 {
			 get
			 {
				  return Math.toIntExact( _id );
			 }
		 }


		 /// <summary>
		 /// Sets a secondary record unit ID for this record. If this is set to something other than <seealso cref="NO_ID"/>
		 /// then <seealso cref="requiresSecondaryUnit()"/> will return {@code true}.
		 /// Setting this id is separate from setting <seealso cref="requiresSecondaryUnit()"/> since this secondary unit id
		 /// may be used to just free that id at the time of updating in the store if a record goes from two to one unit.
		 /// </summary>
		 public virtual long SecondaryUnitId
		 {
			 set
			 {
				  this._secondaryUnitId = value;
			 }
			 get
			 {
				  return this._secondaryUnitId;
			 }
		 }

		 public virtual bool HasSecondaryUnitId()
		 {
			  return _secondaryUnitId != NO_ID;
		 }


		 public virtual bool RequiresSecondaryUnit
		 {
			 set
			 {
				  this._requiresSecondaryUnit = value;
			 }
		 }

		 /// <returns> whether or not a secondary record unit ID has been assigned. </returns>
		 public virtual bool RequiresSecondaryUnit()
		 {
			  return _requiresSecondaryUnit;
		 }

		 public bool InUse()
		 {
			  return _inUse;
		 }

		 public virtual bool InUse
		 {
			 set
			 {
				  this._inUse = value;
			 }
		 }

		 public void SetCreated()
		 {
			  this._created = true;
		 }

		 public bool Created
		 {
			 get
			 {
				  return _created;
			 }
		 }

		 public virtual bool UseFixedReferences
		 {
			 get
			 {
				  return _useFixedReferences;
			 }
			 set
			 {
				  this._useFixedReferences = value;
			 }
		 }


		 public override int GetHashCode()
		 {
			  return ( int )( ( ( long )( ( ulong )_id >> 32 ) ) ^ _id );
		 }

		 public override bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  if ( obj == null )
			  {
					return false;
			  }
			  if ( this.GetType() != obj.GetType() )
			  {
					return false;
			  }
			  AbstractBaseRecord other = ( AbstractBaseRecord ) obj;
			  return _id == other._id;
		 }

		 public override AbstractBaseRecord Clone()
		 {
			  throw new System.NotSupportedException();
		 }
	}

}