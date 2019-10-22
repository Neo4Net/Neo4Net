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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Numbers.safeCastLongToInt;

	/// <summary>
	/// Base class for common functionality for any <seealso cref="NumberArray"/> where the data lives inside heap.
	/// </summary>
	internal abstract class HeapNumberArray<N> : BaseNumberArray<N> where N : NumberArray<N>
	{
		 protected internal HeapNumberArray( int itemSize, long @base ) : base( itemSize, @base )
		 {
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  visitor.HeapUsage( length() * itemSize ); // roughly
		 }

		 public override void Close()
		 { // Nothing to close
		 }

		 protected internal virtual int Index( long index )
		 {
			  return safeCastLongToInt( rebase( index ) );
		 }
	}

}