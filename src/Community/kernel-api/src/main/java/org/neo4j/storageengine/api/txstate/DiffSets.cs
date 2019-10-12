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
namespace Neo4Net.Storageengine.Api.txstate
{

	/// <summary>
	/// Given a sequence of add and removal operations, instances of DiffSets track
	/// which elements need to actually be added and removed at minimum from some
	/// hypothetical target collection such that the result is equivalent to just
	/// executing the sequence of additions and removals in order
	/// </summary>
	/// @param <T> type of elements </param>
	public interface DiffSets<T>
	{

		 bool IsAdded( T elem );

		 bool IsRemoved( T elem );

		 ISet<T> Added { get; }

		 ISet<T> Removed { get; }

		 bool Empty { get; }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<T> apply(java.util.Iterator<? extends T> source);
		 IEnumerator<T> apply<T1>( IEnumerator<T1> source );

		 int Delta();

		 DiffSets<T> FilterAdded( System.Predicate<T> addedFilter );
	}

	 public sealed class DiffSets_Empty<T> : DiffSets<T>
	 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> DiffSets<T> instance()
		  public static DiffSets<T> Instance<T>()
		  {
				return InstanceConflict;
		  }

		  public static DiffSets<T> IfNull<T>( DiffSets<T> diffSets )
		  {
				return diffSets == null ? InstanceConflict : diffSets;
		  }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal static readonly DiffSets InstanceConflict = new DiffSets_Empty();

		  internal DiffSets_Empty()
		  {
				// singleton
		  }

		  public override bool IsAdded( T elem )
		  {
				return false;
		  }

		  public override bool IsRemoved( T elem )
		  {
				return false;
		  }

		  public ISet<T> Added
		  {
			  get
			  {
					return Collections.emptySet();
			  }
		  }

		  public ISet<T> Removed
		  {
			  get
			  {
					return Collections.emptySet();
			  }
		  }

		  public bool Empty
		  {
			  get
			  {
					return true;
			  }
		  }

		  public override IEnumerator<T> Apply<T1>( IEnumerator<T1> source ) where T1 : T
		  {
				return ( System.Collections.IEnumerator )source;
		  }

		  public override int Delta()
		  {
				return 0;
		  }

		  public override DiffSets<T> FilterAdded( System.Predicate<T> addedFilter )
		  {
				return this;
		  }
	 }

}