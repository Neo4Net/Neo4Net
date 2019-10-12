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
namespace Org.Neo4j.Test.matchers
{
	using Description = org.hamcrest.Description;
	using TypeSafeDiagnosingMatcher = org.hamcrest.TypeSafeDiagnosingMatcher;

	public class ByteArrayMatcher : TypeSafeDiagnosingMatcher<sbyte[]>
	{
		 private static readonly char[] _hexadecimals = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		 public static ByteArrayMatcher ByteArray( params sbyte[] expected )
		 {
			  return new ByteArrayMatcher( expected );
		 }

		 public static ByteArrayMatcher ByteArray( params int[] expected )
		 {
			  sbyte[] bytes = new sbyte[expected.Length];
			  for ( int i = 0; i < expected.Length; i++ )
			  {
					bytes[i] = ( sbyte ) expected[i];
			  }
			  return ByteArray( bytes );
		 }

		 private readonly sbyte[] _expected;

		 public ByteArrayMatcher( sbyte[] expected )
		 {
			  this._expected = expected;
		 }

		 protected internal override bool MatchesSafely( sbyte[] actual, Description description )
		 {
			  if ( actual.Length != _expected.Length )
			  {
					Describe( actual, description );
					return false;
			  }
			  for ( int i = 0; i < _expected.Length; i++ )
			  {
					if ( actual[i] != _expected[i] )
					{
						 Describe( actual, description );
						 return false;
					}
			  }
			  return true;
		 }

		 public override void DescribeTo( Description description )
		 {
			  Describe( _expected, description );
		 }

		 private void Describe( sbyte[] bytes, Description description )
		 {
			  string prefix = "byte[] { ";
			  string suffix = "}";
			  StringBuilder sb = new StringBuilder( bytes.Length * 3 + prefix.Length + suffix.Length );

			  sb.Append( prefix );
			  //noinspection ForLoopReplaceableByForEach
			  for ( int i = 0; i < bytes.Length; i++ )
			  {
					int b = bytes[i] & 0xFF;
					char hi = _hexadecimals[b >> 4];
					char lo = _hexadecimals[b & 0x0F];
					sb.Append( hi ).Append( lo ).Append( ' ' );
			  }
			  sb.Append( suffix );
			  description.appendText( sb.ToString() );
		 }
	}

}