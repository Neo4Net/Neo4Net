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
namespace Neo4Net.Kernel.Api.StorageEngine.TxState
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


		 IEnumerator<T> Apply<T1>( IEnumerator<T1> source );

		 int Delta();

		 DiffSets<T> FilterAdded( System.Predicate<T> addedFilter );
	}

	 public sealed class DiffSets_Empty<T> : DiffSets<T>
	 {

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

		  public bool IsAdded( T elem )
		  {
				return false;
		  }

		  public bool IsRemoved( T elem )
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

		  public  IEnumerator<T> Apply<T1>( IEnumerator<T1> source ) where T1 : T
		  {
				return ( System.Collections.IEnumerator )source;
		  }

		  public  int Delta()
		  {
				return 0;
		  }

		  public  DiffSets<T> FilterAdded( System.Predicate<T> addedFilter )
		  {
				return this;
		  }
	 }

}