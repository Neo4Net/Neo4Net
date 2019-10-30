using System;
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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using IntSet = org.eclipse.collections.api.set.primitive.IntSet;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using Iterators = Neo4Net.Collections.Helpers.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;

	/// <summary>
	/// Keeps data about how relationships are distributed between different types.
	/// </summary>
	public class DataStatistics : IEnumerable<DataStatistics.RelationshipTypeCount>
	{
		 private readonly IList<Client> _clients = new List<Client>();
		 private readonly DataImporter.Monitor _entityCounts;
		 private int _opened;
		 private RelationshipTypeCount[] _typeCounts;

		 public DataStatistics( long nodeCount, long propertyCount, RelationshipTypeCount[] sortedTypes ) : this( new DataImporter.Monitor(), sortedTypes )
		 {
			  _entityCounts.nodesImported( nodeCount );
			  _entityCounts.propertiesImported( propertyCount );
		 }

		 public DataStatistics( DataImporter.Monitor IEntityCounts, RelationshipTypeCount[] sortedTypes )
		 {
			  this._entityCounts = IEntityCounts;
			  this._typeCounts = sortedTypes;
		 }

		 public override IEnumerator<RelationshipTypeCount> Iterator()
		 {
			  return Iterators.iterator( _typeCounts );
		 }

		 public virtual int NumberOfRelationshipTypes
		 {
			 get
			 {
				  return _typeCounts.Length;
			 }
		 }

		 public virtual Client NewClient()
		 {
			 lock ( this )
			 {
				  Client client = new Client( this );
				  _clients.Add( client );
				  _opened++;
				  return client;
			 }
		 }

		 private void CloseClient()
		 {
			 lock ( this )
			 {
				  if ( --_opened == 0 )
				  {
						int highestTypeId = 0;
						foreach ( Client client in _clients )
						{
							 highestTypeId = max( highestTypeId, client.HighestTypeId );
						}
      
						long[] counts = new long[highestTypeId + 1];
						foreach ( Client client in _clients )
						{
							 client.AddTo( counts );
						}
						_typeCounts = new RelationshipTypeCount[Counts.Length];
						for ( int i = 0; i < Counts.Length; i++ )
						{
							 _typeCounts[i] = new RelationshipTypeCount( i, counts[i] );
						}
						Arrays.sort( _typeCounts );
				  }
			 }
		 }

		 public class RelationshipTypeCount : IComparable<RelationshipTypeCount>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int TypeIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long CountConflict;

			  public RelationshipTypeCount( int typeId, long count )
			  {
					this.TypeIdConflict = typeId;
					this.CountConflict = count;
			  }

			  public virtual int TypeId
			  {
				  get
				  {
						return TypeIdConflict;
				  }
			  }

			  public virtual long Count
			  {
				  get
				  {
						return CountConflict;
				  }
			  }

			  public override int CompareTo( RelationshipTypeCount o )
			  {
					return Long.compare( CountConflict, o.CountConflict );
			  }

			  public override int GetHashCode()
			  {
					const int prime = 31;
					int result = 1;
					result = prime * result + ( int )( CountConflict ^ ( ( long )( ( ulong )CountConflict >> 32 ) ) );
					result = prime * result + TypeIdConflict;
					return result;
			  }

			  public override bool Equals( object obj )
			  {
					if ( this == obj )
					{
						 return true;
					}
					if ( obj == null || this.GetType() != obj.GetType() )
					{
						 return false;
					}
					RelationshipTypeCount other = ( RelationshipTypeCount ) obj;
					return CountConflict == other.CountConflict && TypeIdConflict == other.TypeIdConflict;
			  }

			  public override string ToString()
			  {
					return format( "%s[type:%d, count:%d]", this.GetType().Name, TypeIdConflict, CountConflict );
			  }
		 }

		 public class Client : IDisposable
		 {
			 private readonly DataStatistics _outerInstance;

			 public Client( DataStatistics outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal long[] Counts = new long[8]; // index is relationship type id
			  internal int HighestTypeId;

			  public virtual void Increment( int typeId )
			  {
					if ( typeId >= Counts.Length )
					{
						 Counts = Arrays.copyOf( Counts, max( Counts.Length * 2, typeId + 1 ) );
					}
					Counts[typeId]++;
					if ( typeId > HighestTypeId )
					{
						 HighestTypeId = typeId;
					}
			  }

			  public override void Close()
			  {
					outerInstance.closeClient();
			  }

			  internal virtual void AddTo( long[] counts )
			  {
					for ( int i = 0; i <= HighestTypeId; i++ )
					{
						 counts[i] += this.Counts[i];
					}
			  }
		 }

		 public virtual RelationshipTypeCount Get( int index )
		 {
			  return _typeCounts[index];
		 }

		 public virtual IntSet Types( int startingFromType, int upToType )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableIntSet set = new org.eclipse.collections.impl.set.mutable.primitive.IntHashSet((upToType - startingFromType) * 2);
			  MutableIntSet set = new IntHashSet( ( upToType - startingFromType ) * 2 );
			  for ( int i = startingFromType; i < upToType; i++ )
			  {
					set.add( Get( i ).TypeId );
			  }
			  return set;
		 }

		 public virtual long NodeCount
		 {
			 get
			 {
				  return _entityCounts.nodesImported();
			 }
		 }

		 public virtual long PropertyCount
		 {
			 get
			 {
				  return _entityCounts.propertiesImported();
			 }
		 }

		 public virtual long RelationshipCount
		 {
			 get
			 {
				  long sum = 0;
				  foreach ( RelationshipTypeCount type in _typeCounts )
				  {
						sum += type.CountConflict;
				  }
				  return sum;
			 }
		 }

		 public override string ToString()
		 {
			  return format( "Imported:%n  %d nodes%n  %d relationships%n  %d properties", NodeCount, RelationshipCount, PropertyCount );
		 }
	}

}