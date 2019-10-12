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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.store
{

	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;

	/// <summary>
	/// Assumes that records have been allocated such that there will be a free record, right after a given record,
	/// to place the secondary unit of that record.
	/// </summary>
	public class SecondaryUnitPrepareIdSequence : PrepareIdSequence
	{
		 public override System.Func<long, IdSequence> Apply( IdSequence idSequence )
		 {
			  return new NeighbourIdSequence();
		 }

		 private class NeighbourIdSequence : System.Func<long, IdSequence>, IdSequence
		 {
			  internal long Id;
			  internal bool Returned;

			  public override IdSequence Apply( long firstUnitId )
			  {
					this.Id = firstUnitId;
					Returned = false;
					return this;
			  }

			  public override long NextId()
			  {
					try
					{
						 if ( Returned )
						 {
							  throw new System.InvalidOperationException( "Already returned" );
						 }
						 return Id + 1;
					}
					finally
					{
						 Returned = true;
					}
			  }

			  public override IdRange NextIdBatch( int size )
			  {
					throw new System.NotSupportedException();
			  }
		 }
	}

}