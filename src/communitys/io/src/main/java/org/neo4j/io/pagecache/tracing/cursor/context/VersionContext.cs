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
namespace Neo4Net.Io.pagecache.tracing.cursor.context
{
	/// <summary>
	/// Context that contains state of ongoing versioned data read or write.
	/// 
	/// <br/>
	/// Context can be in one of two states:
	/// <ul>
	///     <li>Read context: reading is performed for a version that context was initialised with.
	///     As soon as reader that associated with a context will observe data with version that it higher,
	///     context will be marked as dirty.
	///     <br/>
	///     For example, when context is initialised with last closed transaction id and if
	///     at the end of reading operation context is not marked as dirty its guarantee that context did not
	///     encounter any data from more recent transaction.</li>
	///     <li>Write context: context that performs data modifications. Any modifications will be tagged with
	///     some version that write context was initialised with.
	///     <br/>
	///     For example, commit will start write context that with a version that is equal to current
	///     committing transaction id.
	///     </li>
	/// </ul>
	/// By default non context will be initialised with last closed transaction id which is equal to <seealso cref="Long.MAX_VALUE"/>
	/// and transaction id that is equal to minimal possible transaction id: 1.
	/// </summary>
	public interface VersionContext
	{
		 /// <summary>
		 /// Initialize read context with latest closed transaction id as it current version.
		 /// </summary>
		 void InitRead();

		 /// <summary>
		 /// Initialize write context with committingTxId as modification version. </summary>
		 /// <param name="committingTxId"> currently committing transaction id </param>
		 void InitWrite( long committingTxId );

		 /// <summary>
		 /// Context currently committing transaction id </summary>
		 /// <returns> committing transaction id </returns>
		 long CommittingTransactionId();

		 /// <summary>
		 /// Last closed transaction id that read context was initialised with </summary>
		 /// <returns> last closed transaction id </returns>
		 long LastClosedTransactionId();

		 /// <summary>
		 /// Mark current context as dirty
		 /// </summary>
		 void MarkAsDirty();

		 /// <summary>
		 /// Check whenever current context is dirty </summary>
		 /// <returns> true if context is dirty, false otherwise </returns>
		 bool Dirty { get; }

	}

}