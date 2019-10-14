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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Index.@internal.gbptree;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// Writes a failure message to a header in a <seealso cref="GBPTree"/>.
	/// </summary>
	internal class FailureHeaderWriter : System.Action<PageCursor>
	{
		 /// <summary>
		 /// The {@code short} length field containing the length (number of bytes) of the failure message.
		 /// </summary>
		 private const int HEADER_LENGTH_FIELD_LENGTH = 2;

		 private readonly sbyte[] _failureBytes;

		 internal FailureHeaderWriter( sbyte[] failureBytes )
		 {
			  this._failureBytes = failureBytes;
		 }

		 public override void Accept( PageCursor cursor )
		 {
			  sbyte[] bytesToWrite = _failureBytes;
			  cursor.PutByte( NativeIndexPopulator.BYTE_FAILED );
			  int availableSpace = cursor.CurrentPageSize - cursor.Offset;
			  if ( bytesToWrite.Length + HEADER_LENGTH_FIELD_LENGTH > availableSpace )
			  {
					bytesToWrite = Arrays.copyOf( bytesToWrite, availableSpace - HEADER_LENGTH_FIELD_LENGTH );
			  }
			  cursor.PutShort( ( short ) bytesToWrite.Length );
			  cursor.PutBytes( bytesToWrite );
		 }
	}

}