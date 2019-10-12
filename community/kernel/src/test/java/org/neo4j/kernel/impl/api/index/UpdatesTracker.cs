using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	public class UpdatesTracker
	{
		 private int _created;
		 private int _updated;
		 private int _deleted;
		 private int _createdDuringPopulation;
		 private int _updatedDuringPopulation;
		 private int _deletedDuringPopulation;
		 private bool _populationCompleted;

		 public virtual void IncreaseCreated( int num )
		 {
			  _created += num;
		 }

		 public virtual void IncreaseDeleted( int num )
		 {
			  _deleted += num;
		 }

		 public virtual void IncreaseUpdated( int num )
		 {
			  _updated += num;
		 }

		 internal virtual void NotifyPopulationCompleted()
		 {
			  if ( _populationCompleted )
			  {
					return;
			  }

			  _populationCompleted = true;
			  _createdDuringPopulation = _created;
			  _updatedDuringPopulation = _updated;
			  _deletedDuringPopulation = _deleted;
		 }

		 public virtual bool PopulationCompleted
		 {
			 get
			 {
				  return _populationCompleted;
			 }
		 }

		 public virtual int Created()
		 {
			  return _created;
		 }

		 public virtual int Deleted()
		 {
			  return _deleted;
		 }

		 public virtual int Updated
		 {
			 get
			 {
				  return _updated;
			 }
		 }

		 public virtual int CreatedDuringPopulation()
		 {
			  return _createdDuringPopulation;
		 }

		 public virtual int DeletedDuringPopulation()
		 {
			  return _deletedDuringPopulation;
		 }

		 public virtual int UpdatedDuringPopulation
		 {
			 get
			 {
				  return _updatedDuringPopulation;
			 }
		 }

		 public virtual int CreatedAfterPopulation()
		 {
			  return _created - _createdDuringPopulation;
		 }

		 public virtual int DeletedAfterPopulation()
		 {
			  return _deleted - _deletedDuringPopulation;
		 }

		 public virtual int UpdatedAfterPopulation()
		 {
			  return _updated - _updatedDuringPopulation;
		 }

		 public virtual void Add( UpdatesTracker updatesTracker )
		 {
			  Debug.Assert( PopulationCompleted );
			  Debug.Assert( updatesTracker.PopulationCompleted );
			  this._created += updatesTracker._created;
			  this._deleted += updatesTracker._deleted;
			  this._updated += updatesTracker._updated;
			  this._createdDuringPopulation += updatesTracker._createdDuringPopulation;
			  this._updatedDuringPopulation += updatesTracker._updatedDuringPopulation;
			  this._deletedDuringPopulation += updatesTracker._deletedDuringPopulation;
		 }

		 public override string ToString()
		 {
			  return "UpdatesTracker{" +
						"created=" + _created +
						", deleted=" + _deleted +
						", createdDuringPopulation=" + _createdDuringPopulation +
						", updatedDuringPopulation=" + _updatedDuringPopulation +
						", deletedDuringPopulation=" + _deletedDuringPopulation +
						", createdAfterPopulation=" + CreatedAfterPopulation() +
						", updatedAfterPopulation=" + UpdatedAfterPopulation() +
						", deletedAfterPopulation=" + DeletedAfterPopulation() +
						", populationCompleted=" + _populationCompleted +
						'}';
		 }
	}

}