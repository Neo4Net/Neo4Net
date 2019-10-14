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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	/// <summary>
	/// Keys in <seealso cref="LabelScanLayout"/>, each key consists of {@code labelId} and {@code nodeIdRange}, i.e.
	/// {@code nodeId/rangeSize}, where each range is a small bit set of size <seealso cref="LabelScanValue.RANGE_SIZE"/>.
	/// </summary>
	internal class LabelScanKey
	{
		 internal int LabelId;
		 internal long IdRange;

		 internal LabelScanKey()
		 {
			  Clear();
		 }

		 internal LabelScanKey( int labelId, long idRange )
		 {
			  Set( labelId, idRange );
		 }

		 /// <summary>
		 /// Sets this key.
		 /// </summary>
		 /// <param name="labelId"> labelId for this key. </param>
		 /// <param name="idRange"> node idRange for this key. </param>
		 /// <returns> this key instance, for convenience. </returns>
		 internal LabelScanKey Set( int labelId, long idRange )
		 {
			  this.LabelId = labelId;
			  this.IdRange = idRange;
			  return this;
		 }

		 internal void Clear()
		 {
			  Set( -1, -1 );
		 }

		 public override string ToString()
		 {
			  return "[label:" + LabelId + ",range:" + IdRange + "]";
		 }
	}

}