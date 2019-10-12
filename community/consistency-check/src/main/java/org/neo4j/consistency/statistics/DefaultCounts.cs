using System.Text;

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
namespace Org.Neo4j.Consistency.statistics
{

	public class DefaultCounts : Counts
	{
		 private readonly long[][] _counts;

		 public DefaultCounts( int threads )
		 {
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: _counts = new long[Enum.GetValues(typeof(Counts_Type)).length][threads];
			  _counts = RectangularArrays.RectangularLongArray( Enum.GetValues( typeof( Counts_Type ) ).length, threads );
		 }

		 private long[] Counts( Counts_Type type )
		 {
			  return _counts[( int )type];
		 }

		 public override long Sum( Counts_Type type )
		 {
			  long[] all = _counts[( int )type];
			  long total = 0;
			  foreach ( long one in all )
			  {
					total += one;
			  }
			  return total;
		 }

		 public override void IncAndGet( Counts_Type type, int threadIndex )
		 {
			  Counts( type )[threadIndex]++;
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder( "Counts:" );
			  foreach ( Counts_Type type in Enum.GetValues( typeof( Counts_Type ) ) )
			  {
					long sum = sum( type );
					if ( sum > 0 )
					{
						 builder.Append( format( "%n  %d %s", sum, type.name() ) );
					}
			  }
			  return builder.ToString();
		 }

		 public override void Reset()
		 {
			  foreach ( long[] c in _counts )
			  {
					Arrays.fill( c, 0 );
			  }
		 }
	}

}