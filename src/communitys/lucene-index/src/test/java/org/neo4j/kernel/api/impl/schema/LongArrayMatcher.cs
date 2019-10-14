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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using Description = org.hamcrest.Description;
	using TypeSafeDiagnosingMatcher = org.hamcrest.TypeSafeDiagnosingMatcher;

	// TODO: move to common test module
	internal class LongArrayMatcher : TypeSafeDiagnosingMatcher<long[]>
	{

		 public static LongArrayMatcher EmptyArrayMatcher()
		 {
			  return new LongArrayMatcher( new long[]{} );
		 }

		 public static LongArrayMatcher Of( params long[] values )
		 {
			  return new LongArrayMatcher( values );
		 }

		 private long[] _expectedArray;

		 internal LongArrayMatcher( long[] expectedArray )
		 {
			  this._expectedArray = expectedArray;
		 }

		 protected internal override bool MatchesSafely( long[] items, Description mismatchDescription )
		 {
			  DescribeArray( items, mismatchDescription );
			  if ( items.Length != _expectedArray.Length )
			  {
					return false;
			  }
			  for ( int i = 0; i < items.Length; i++ )
			  {
					if ( items[i] != _expectedArray[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public override void DescribeTo( Description description )
		 {
			  DescribeArray( _expectedArray, description );
		 }

		 private void DescribeArray( long[] value, Description description )
		 {
			  description.appendText( "long[]" ).appendText( Arrays.ToString( value ) );
		 }
	}

}