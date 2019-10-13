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
namespace Neo4Net.Kernel.impl.transaction.state
{
	/// <summary>
	/// Provides access to records, both for reading and for writing.
	/// </summary>
	public interface RecordAccess<RECORD, ADDITIONAL>
	{
		 /// <summary>
		 /// Gets an already loaded record, or loads it as part of this call if it wasn't. The <seealso cref="RecordProxy"/>
		 /// returned has means of communicating when to get access to the actual record for reading or writing.
		 /// With that information any additional loading or storing can be inferred for the specific
		 /// use case (implementation).
		 /// </summary>
		 /// <param name="key"> the record key. </param>
		 /// <param name="additionalData"> additional data to put in the record after loaded. </param>
		 /// <returns> a <seealso cref="RecordProxy"/> for the record for {@code key}. </returns>
		 RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetOrLoad( long key, ADDITIONAL additionalData );

		 RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetIfLoaded( long key );

		 [Obsolete]
		 void SetTo( long key, RECORD newRecord, ADDITIONAL additionalData );

		 RecordAccess_RecordProxy<RECORD, ADDITIONAL> SetRecord( long key, RECORD record, ADDITIONAL additionalData );

		 /// <summary>
		 /// Creates a new record with the given {@code key}. Any {@code additionalData} is set in the
		 /// record before returning.
		 /// </summary>
		 /// <param name="key"> the record key. </param>
		 /// <param name="additionalData"> additional data to put in the record after loaded. </param>
		 /// <returns> a <seealso cref="RecordProxy"/> for the record for {@code key}. </returns>
		 RecordAccess_RecordProxy<RECORD, ADDITIONAL> Create( long key, ADDITIONAL additionalData );

		 /// <summary>
		 /// Closes the record access.
		 /// </summary>
		 void Close();

		 int ChangeSize();

		 IEnumerable<RecordAccess_RecordProxy<RECORD, ADDITIONAL>> Changes();

		 /// <summary>
		 /// A proxy for a record that encapsulates load/store actions to take, knowing when the underlying record is
		 /// requested for reading or for writing.
		 /// </summary>

		 /// <summary>
		 /// Hook for loading and creating records.
		 /// </summary>
	}

	 public interface RecordAccess_RecordProxy<RECORD, ADDITIONAL>
	 {
		  long Key { get; }

		  RECORD ForChangingLinkage();

		  RECORD ForChangingData();

		  RECORD ForReadingLinkage();

		  RECORD ForReadingData();

		  ADDITIONAL AdditionalData { get; }

		  RECORD Before { get; }

		  bool Changed { get; }

		  bool Created { get; }
	 }

	 public interface RecordAccess_Loader<RECORD, ADDITIONAL>
	 {
		  RECORD NewUnused( long key, ADDITIONAL additionalData );

		  RECORD Load( long key, ADDITIONAL additionalData );

		  void EnsureHeavy( RECORD record );

		  RECORD Clone( RECORD record );
	 }

}