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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.Preconditions.checkArgument;

	internal sealed class SortedMergeJoin
	{
		 private long _nextFromA = -1;
		 private long _nextFromB = -1;
		 private Value[] _valuesFromA;
		 private Value[] _valuesFromB;
		 private int _indexOrder;

		 internal void Initialize( IndexOrder indexOrder )
		 {
			  this._indexOrder = indexOrder == IndexOrder.DESCENDING ? 1 : -1;
			  this._nextFromA = -1;
			  this._nextFromB = -1;
			  this._valuesFromA = null;
			  this._valuesFromB = null;
		 }

		 internal bool NeedsA()
		 {
			  return _nextFromA == -1;
		 }

		 internal bool NeedsB()
		 {
			  return _nextFromB == -1;
		 }

		 internal void SetA( long nodeId, Value[] values )
		 {
			  _nextFromA = nodeId;
			  _valuesFromA = values;
		 }

		 internal void SetB( long nodeId, Value[] values )
		 {
			  _nextFromB = nodeId;
			  _valuesFromB = values;
		 }

		 internal void Next( Sink sink )
		 {
			  int c = 0;
			  if ( _valuesFromA != null && _valuesFromB != null )
			  {
					checkArgument( _valuesFromA.Length == _valuesFromB.Length, "Expected index and txState values to have same dimensions, but got %d values from index and %d from txState", _valuesFromB.Length, _valuesFromA.Length );

					for ( int i = 0; c == 0 && i < _valuesFromA.Length; i++ )
					{
						 c = Values.COMPARATOR.Compare( _valuesFromA[i], _valuesFromB[i] );
					}
			  }

			  if ( _nextFromB == -1 || Integer.signum( c ) == _indexOrder )
			  {
					sink.AcceptSortedMergeJoin( _nextFromA, _valuesFromA );
					_nextFromA = -1;
					_valuesFromA = null;
			  }
			  else
			  {
					sink.AcceptSortedMergeJoin( _nextFromB, _valuesFromB );
					_nextFromB = -1;
					_valuesFromB = null;
			  }
		 }

		 internal interface Sink
		 {
			  void AcceptSortedMergeJoin( long nodeId, Value[] values );
		 }
	}

}