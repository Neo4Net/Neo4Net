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
namespace Org.Neo4j.Kernel.Api.Impl.Index.collector
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;

	/// <summary>
	/// Document values iterators that are primitive long iterators that can access value by field from document
	/// and provides information about how many items remains in the underlying source.
	/// </summary>
	public interface ValuesIterator : DocValuesAccess, LongIterator
	{
		 int Remaining();

		 float CurrentScore();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 ValuesIterator EMPTY = new ValuesIterator.Adapter(0)
	//	 {
	//		  @@Override protected boolean fetchNext()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public long current()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public float currentScore()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long getValue(String field)
	//		  {
	//				return 0;
	//		  }
	//	 };
	}

	 public abstract class ValuesIterator_Adapter : PrimitiveLongCollections.PrimitiveLongBaseIterator, ValuesIterator
	 {
		 public abstract long GetValue( string field );
		 public abstract long Current();
		  protected internal readonly int Size;
		  protected internal int Index;

		  /// <summary>
		  /// Gets the score for the current iterator position.
		  /// </summary>
		  /// <returns> The score of the value, or 0 if scoring is not kept or applicable. </returns>
		  public override abstract float CurrentScore();

		  internal ValuesIterator_Adapter( int size )
		  {
				this.Size = size;
		  }

		  /// <returns> the number of docs left in this iterator. </returns>
		  public override int Remaining()
		  {
				return Size - Index;
		  }
	 }

}