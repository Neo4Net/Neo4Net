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
namespace Neo4Net.Index.@internal.gbptree
{

	/// <summary>
	/// Able to <seealso cref="merge(object, object, ValueMerger)"/> and <seealso cref="remove(object)"/> key/value pairs
	/// into a <seealso cref="GBPTree"/>. After all modifications have taken place the writer must be <seealso cref="close() closed"/>,
	/// typically using try-with-resource clause.
	/// </summary>
	/// @param <KEY> type of keys </param>
	/// @param <VALUE> type of values </param>
	public interface Writer<KEY, VALUE> : System.IDisposable
	{
		 /// <summary>
		 /// Associate given {@code key} with given {@code value}.
		 /// Any existing {@code value} associated with {@code key} will be overwritten.
		 /// </summary>
		 /// <param name="key"> key to associate with value </param>
		 /// <param name="value"> value to associate with key </param>
		 /// <exception cref="UncheckedIOException"> on index access error. </exception>
		 void Put( KEY key, VALUE value );

		 /// <summary>
		 /// If the {@code key} doesn't already exist in the index the {@code key} will be added and the {@code value}
		 /// associated with it. If the {@code key} already exists then its existing {@code value} will be merged with
		 /// the given {@code value}, using the <seealso cref="ValueMerger"/>. If the <seealso cref="ValueMerger"/> returns a non-null
		 /// value that value will be associated with the {@code key}, otherwise (if it returns {@code null}) nothing will
		 /// be written.
		 /// </summary>
		 /// <param name="key"> key for which to merge values. </param>
		 /// <param name="value"> value to merge with currently associated value for the {@code key}. </param>
		 /// <param name="valueMerger"> <seealso cref="ValueMerger"/> to consult if key already exists. </param>
		 /// <exception cref="UncheckedIOException"> on index access error. </exception>
		 void Merge( KEY key, VALUE value, ValueMerger<KEY, VALUE> valueMerger );

		 /// <summary>
		 /// Removes a key, returning it's associated value, if found.
		 /// </summary>
		 /// <param name="key"> key to remove. </param>
		 /// <returns> value which was associated with the remove key, if found, otherwise {@code null}. </returns>
		 /// <exception cref="UncheckedIOException"> on index access error. </exception>
		 VALUE Remove( KEY key );
	}

}